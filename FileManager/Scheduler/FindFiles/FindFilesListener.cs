namespace FileManager.Scheduler
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Quartz.Listener;

    /// <summary>
    /// Представляет прослушиватель задания "Поиск файлов".
    /// </summary>
    internal class FindFilesListener : JobChainingJobListener
    {
        /// <summary>
        /// Представляет логер.
        /// </summary>
        private readonly ILogger<FindFilesListener> _logger;

        /// <summary>
        /// Инициализирует экземпляр класса с указанным именем.
        /// </summary>
        /// <param name="name">Наименование прослушивателя.</param>
        /// <param name="logger">Логер.</param>
        public FindFilesListener(ILogger<FindFilesListener> logger) : base(nameof(FindFilesListener))
        {
            _logger = logger;
        }

        public override string Name
        {
            get
            {
                return nameof(FindFilesListener);
            }
        }

        /// <summary>
        /// Обработка прослушивателя.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            //base.JobWasExecuted(context, jobException);
            // only run second job if first job was executed successfully
            if (jobException != null) return Task.CompletedTask;

            FileInfo[] files = context.Get("files") as FileInfo[];
            if (files == null || files.Length == 0) return Task.CompletedTask;

            // формируем задание на обработку файлов
            for (int i = 0; i < files.Length; i++)
            {
                string jobName = $"{SchedulerResource.DistinctFile} {files[i].Name}";
                _logger.LogInformation("Формирование задания \"{jobName}\"", jobName);
                try
                {
                    IJobDetail job = JobBuilder.Create<DistinctFileJob>()
                        .WithIdentity(new JobKey(jobName, SchedulerResource.GroupFiles))
                        .WithDescription(jobName)
                        .StoreDurably(true)
                        .Build();
                    job.JobDataMap["file"] = files[i];
                    job.JobDataMap["options"] = ParamHelper.FileManageOptions;
                    job.JobDataMap["logger"] = ParamHelper.WebApplication.Services.GetRequiredService<ILogger<DistinctFileService>>();

                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(new TriggerKey(jobName, SchedulerResource.GroupDataFile))
                        .WithDescription(jobName)
                        .StartAt(DateTime.UtcNow.AddSeconds(i))
                        .Build();

                    // ..и запускаем его 
                    context.Scheduler.ScheduleJob(job, trigger);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Произошла ошибка при создании шедуллера задания \"{jobName}\": {ex}", jobName, ex);
                    throw;
                }
            }

            //
            return Task.CompletedTask;
        }

    }
}
