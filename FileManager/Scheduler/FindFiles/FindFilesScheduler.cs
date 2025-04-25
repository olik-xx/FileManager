namespace FileManager.Scheduler
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using FileManager.Scheduler.FindFiles;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Представляет шедуллер задания "Поиск файлов".
    /// </summary>
    internal class FindFilesScheduler : ISchedulerDetail
    {
        /// <inheritdoc/>
        public IJobDetail JobDetail { get; set; }

        /// <inheritdoc/>
        public ITrigger Trigger { get; set; }

        /// <inheritdoc/>
        public ISchedulerDetail Init()
        {
            ILogger<FindFilesService> logger = null;
            try
            {
                logger = ParamHelper.WebApplication.Services.GetRequiredService<ILogger<FindFilesService>>();

                JobDetail = JobBuilder.Create<FindFilesJob>()
                      .WithIdentity(new JobKey(SchedulerResource.FindFiles, SchedulerResource.GroupFiles))
                      .WithDescription(SchedulerResource.FindFiles)
                      .RequestRecovery() //Job should be re-executed if a 'recovery' or 'fail-over' situation is encountered.
                      .Build();
                JobDetail.JobDataMap[nameof(FindFilesService)] = new FindFilesService(logger);
                JobDetail.JobDataMap["logger"] = logger;

                Trigger = TriggerBuilder.Create()
                   .WithIdentity(new TriggerKey(SchedulerResource.FindFiles, SchedulerResource.GroupStart))
                   .WithDescription(SchedulerResource.FindFiles)
                   .WithSimpleSchedule
                    (x =>
                        x.WithIntervalInMinutes(ParamHelper.FileManageOptions.TimeoutIdle)
                        .RepeatForever()
                        .WithMisfireHandlingInstructionIgnoreMisfires()// ignore
                    )
                    .StartAt(DateTime.UtcNow.AddSeconds(2))
                    .Build();

                return this;
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError("Произошла ошибка построения шедуллера {name}: {ex}", nameof(FindFilesScheduler), ex);
                return null;
            }
        }
    }
}
