namespace FileManager.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FileManager.Resources;
    using Quartz;
    using Quartz.Impl;
    using Quartz.Impl.Matchers;

    /// <summary>
    /// Класс по управлению планировщиком заданий.
    /// </summary>
    public class SchedulerControl
    {
        private static IScheduler _sched;
        private readonly ITaskStorage _storage;

        static SchedulerControl()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _sched = schedulerFactory.GetScheduler().GetAwaiter().GetResult(); 
        }

        /// <summary>
        /// Инициализирует экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="storage">Хранилище заданий.</param>
        public SchedulerControl(ITaskStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Запускает потоки заданий.
        /// </summary>
        public void Start()
        {
            // Все задания должны добавляться без внутренних exception
            foreach (var task in _storage)
                _sched.ScheduleJob(task.JobDetail, task.Trigger);

            // прослушиватели
            //this._sched.AddJob();
            foreach (KeyValuePair<IJobListener, JobKey> item in this._storage.JobListeners)
                _sched.ListenerManager.AddJobListener(item.Key, KeyMatcher<JobKey>.KeyEquals(item.Value));

            _sched.Start();
        }

        /// <summary>
        /// Останавливает потоки заданий.
        /// </summary>
        /// <param name="wait">Признак, стоит ли ожидать окончания заданий.</param>
        public void Shutdown(bool wait)
        {
            _sched.Shutdown(wait);
        }

        /// <summary>
        /// Перезапускает задание по указанному ключу.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        public async void Restart(JobKey jobKey)
        {
            if (await _sched.CheckExists(jobKey))
            {
                IReadOnlyCollection<ITrigger> triggers = await _sched.GetTriggersOfJob(jobKey);
                foreach (ITrigger trigger in triggers)
                    await RescheduleJob(trigger.Key, trigger);
            }
        }

        /// <summary>
        /// Запускает на немедленное выполнение задание по указанным параметрам.
        /// </summary>
        /// <param name="jobName">Наименование задания.</param>
        /// <param name="groupName">Наименование группы заданий.</param>
        public Task RunJobImmediately(string jobName, string groupName)
        {
            return RunImmediately(new JobKey(jobName, groupName));
        }

        /// <summary>
        /// Запускает на немедленное выполнение задание по указанному ключу.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        public async Task RunImmediately(JobKey jobKey)
        {
            if (!await _sched.CheckExists(jobKey))
                throw new ArgumentException(SchedulerResource.InvalidJobKey);

            await _sched.TriggerJob(jobKey);
        }

        /// <summary>
        /// Запускает на немедленное выполнение всех заданий для указанной группы.
        /// </summary>
        /// <param name="groupName">Наименование группы заданий.</param>
        public async Task RunImmediately(string groupName)
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupEquals(groupName);
            var jobKeys = await _sched.GetJobKeys(groupMatcher);
            foreach (var key in jobKeys)
                await RunImmediately(key);
        }

        /// <summary>
        /// Проверяет, содержится ли задание по указанному ключу.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        public Task<bool> ContainsJob(JobKey jobKey)
        {
            return _sched.CheckExists(jobKey);
        }

        /// <summary>
        /// Выполняет проверку валидации времени следующего исполнения для указанного задания.
        /// </summary>
        /// <param name="jobKey">Ключ значения.</param>
        /// <param name="diffhours">Допустимый интервал расхождения, в часах, так как задания конкурируют между собой и могут быть пропущены.</param>
        /// <returns>Значение <see langword="true"/>, если превышено время расхождения, иначе - значение <see langword="false"/>.</returns>
        public async Task<bool> ValidateByHours(JobKey jobKey, double diffhours)
        {
            if (!await _sched.CheckExists(jobKey))
               throw new ArgumentException(SchedulerResource.InvalidJobKey);

            IReadOnlyCollection<ITrigger> triggers = await _sched.GetTriggersOfJob(jobKey);
            foreach (ITrigger trigger in triggers)
            {
                DateTime? nextdate = trigger.GetNextFireTimeUtc()?.ToLocalTime().DateTime;
                if (nextdate.HasValue &&  Math.Ceiling(DateTime.Now.Subtract(nextdate.Value).TotalHours) > diffhours) // была смена системного времени хотя бы для одного из триггеров
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Возвращает следующее время исполнения указанного задания.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        /// <returns>Локальное время исполнения.</returns>
        public async Task<DateTime?> GetNextTime(JobKey jobKey)
        {
            if (!await _sched.CheckExists(jobKey))
                throw new ArgumentException(SchedulerResource.InvalidJobKey);

            IReadOnlyCollection<ITrigger> triggers = await _sched.GetTriggersOfJob(jobKey);
            foreach (ITrigger trigger in triggers)
                return trigger.GetNextFireTimeUtc()?.ToLocalTime().DateTime;
            return null;
        }

        /// <summary>
        /// Выполняет проверку триггеров заданий, и если они в прошлом, то перезапускает их.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        /// <param name="newTrigger">Новый триггер, если не может быть воспроизведен точь-в-точь.</param>
        /// <param name="diffmin">Допустимый интервал расхождения, в минутах.</param>
        public async void CheckNextTime(JobKey jobKey, ITrigger newTrigger, int diffmin = 5)
        {
            if (await _sched.CheckExists(jobKey))
            {
                IReadOnlyCollection<ITrigger> triggers = await _sched.GetTriggersOfJob(jobKey);
                foreach (ITrigger trigger in triggers)
                {
                    DateTime? nextdate = trigger.GetNextFireTimeUtc()?.ToLocalTime().DateTime;
                    if (nextdate.HasValue && Math.Abs(DateTime.Now.Subtract(nextdate.Value).TotalMinutes) > diffmin) // была смена системного времени
                        await RescheduleJob(trigger.Key, newTrigger ?? trigger);
                }
            }
        }

        /// <summary>
        /// Выполняет удаление задания по ключу. Возвращает результат операции.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        /// <returns>Значение <see langword="true"/>, если задание было найдено и удалено, иначе - значение <see langword="false"/>.</returns>
        public Task<bool> DeleteJob(JobKey jobKey)
        {
            return _sched.DeleteJob(jobKey);
        }

        /// <summary>
        /// Стартует задание по указанным параметрам. Возвращает время следующего запуска.
        /// </summary>
        /// <param name="job">Задание.</param>
        /// <param name="trigger">Триггер выполнения.</param>
        /// <returns></returns>
        public Task<DateTimeOffset> ScheduleJob(IJobDetail job, ITrigger trigger)
        {
            return _sched.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// Временно останавливает задание, останавливая все его триггеры.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        public async Task Pause(JobKey jobKey)
        {
            if (!await _sched.CheckExists(jobKey))
                throw new ArgumentException(SchedulerResource.InvalidJobKey);

            await _sched.PauseJob(jobKey);
        }

        /// <summary>
        /// Запускает задание на паузе.
        /// </summary>
        /// <param name="jobKey">Ключ задания.</param>
        public async Task Resume(JobKey jobKey)
        {
            if (!await _sched.CheckExists(jobKey))
                throw new ArgumentException(SchedulerResource.InvalidJobKey);

            await _sched.ResumeJob(jobKey);
        }

        ///// <summary>
        ///// Перезапуск задания с новым триггером.
        ///// </summary>
        ///// <param name="trigger">Триггер выполнения.</param>
        //private Task<DateTimeOffset?> RestartJob(ITrigger trigger)
        //{
        //    if (trigger != null)
        //        return _sched.RescheduleJob(trigger.Key, trigger.ReInit());
        //    return null;
        //}

        /// <summary>
        /// Выполняет замену триггера для задания по ключу <paramref name="triggerKey"/>. Возвращает время следующего запуска.
        /// </summary>
        /// <param name="triggerKey">Ключ триггера на удаление.</param>
        /// <param name="newTrigger">Новый триггер выполнения с ключом <paramref name="triggerKey"/>.</param>
        /// <returns></returns>
        public Task<DateTimeOffset?> RescheduleJob(TriggerKey triggerKey, ITrigger newTrigger)
        {
            return _sched.RescheduleJob(triggerKey, newTrigger);
        }
    }
}
