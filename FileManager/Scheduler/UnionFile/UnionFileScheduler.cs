namespace FileManager.Scheduler
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Представляет шедуллер задания "Поиск пересечений между файлами".
    /// </summary>
    internal class UnionFileScheduler : ISchedulerDetail
    {
        /// <inheritdoc/>
        public IJobDetail JobDetail { get; set; }

        /// <inheritdoc/>
        public ITrigger Trigger { get; set; }

        /// <inheritdoc/>
        public ISchedulerDetail Init()
        {
            ILogger<UnionFileService> logger = null;
            try
            {
                logger = ParamHelper.WebApplication.Services.GetRequiredService<ILogger<UnionFileService>>();

                JobDetail = JobBuilder.Create<UnionFileJob>()
                      .WithIdentity(new JobKey(SchedulerResource.UnionFile, SchedulerResource.GroupFiles))
                      .WithDescription(SchedulerResource.UnionFile)
                      .RequestRecovery()
                      .Build();
                JobDetail.JobDataMap[nameof(UnionFileService)] = new UnionFileService(ParamHelper.FileManageOptions, logger);
                JobDetail.JobDataMap["logger"] = logger;

                Trigger = TriggerBuilder.Create()
                   .WithIdentity(new TriggerKey(SchedulerResource.UnionFile, SchedulerResource.GroupDataFile))
                   .WithDescription(SchedulerResource.UnionFile)
                   .WithSimpleSchedule
                    (x =>
                        x.WithIntervalInMinutes(ParamHelper.FileManageOptions.InterFileTimeoutIdle)
                        .RepeatForever()
                        .WithMisfireHandlingInstructionIgnoreMisfires()// ignore
                    )
                    .StartNow()
                    //.StartAt(DateTime.UtcNow.AddMinutes(1))
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
