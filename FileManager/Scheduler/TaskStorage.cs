namespace FileManager.Scheduler
{
    using System.Collections;
    using System.Collections.Generic;
    using FileManager.Helpers;
    using FileManager.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Представляет класс для хранения заданий планировщика службы.
    /// </summary>
    internal class TaskStorage : ITaskStorage
    {
        /// <summary>
        /// Представляет список заданий.
        /// </summary>
        private readonly List<ISchedulerDetail> _tasks;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public TaskStorage()
        {
            _tasks = new List<ISchedulerDetail>();

            var findFiles = new FindFilesScheduler().Init();
            if (findFiles != null)
                _tasks.Add(findFiles);

            var union = new UnionFileScheduler().Init();
            if (union != null)
                _tasks.Add(union);

            if (ParamHelper.KeepLogDay > 0)
            {
                var logs = new ClearLogScheduler().Init();
                if (logs != null)
                    _tasks.Add(logs);
            }

            // прослушиватели 
            var listener = new FindFilesListener (ParamHelper.WebApplication.Services.GetRequiredService<ILogger<FindFilesListener>>());
            listener.AddJobChainLink(findFiles.JobDetail.Key, new JobKey(SchedulerResource.DistinctFile, SchedulerResource.GroupFiles));
            _jobListeners.Add(listener, findFiles.JobDetail.Key);
        }

        /// <summary>
        /// Возвращает количество заданий в хранилище.
        /// </summary>
        public int Count
        {
            get
            {
                return _tasks.Count;
            }
        }

        /// <summary>
        /// Представляет коллекцию прослушивателей заданий.
        /// </summary>
        private Dictionary<IJobListener, JobKey> _jobListeners = new Dictionary<IJobListener, JobKey>();
        /// <summary>
        /// Возвращает коллекцию прослушивателей.
        /// </summary>
        public Dictionary<IJobListener, JobKey> JobListeners
        {
            get
            {
                return _jobListeners;
            }
        }

        /// <summary>
        /// Возвращает итератор.
        /// </summary>
        /// <returns>Итератор коллекции.</returns>
        public IEnumerator<ISchedulerDetail> GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }

        /// <summary>
        /// Возвращает итератор.
        /// </summary>
        /// <returns>Итератор коллекции.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
