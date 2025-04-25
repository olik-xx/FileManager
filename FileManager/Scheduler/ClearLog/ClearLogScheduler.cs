namespace FileManager.Scheduler
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Представляет шедуллер задания "Автоудаление логов".
    /// </summary>
    internal class ClearLogScheduler : ISchedulerDetail
    {
        /// <inheritdoc/>
        public IJobDetail JobDetail { get; set; }

        /// <inheritdoc/>
        public ITrigger Trigger { get; set; }

        /// <summary>
        /// Представляет ключ задания "Автоудаление логов".
        /// </summary>
        public static readonly JobKey JobKey = new JobKey(SchedulerResource.ClearLog, SchedulerResource.GroupFiles);

        /// <inheritdoc/>
        public ISchedulerDetail Init()
        {
            ILogger<ClearLogService> logger = null;
            try
            {
                logger = ParamHelper.WebApplication.Services.GetRequiredService<ILogger<ClearLogService>>();

                string[] dirs = new[] 
                {
                    Path.GetDirectoryName(Helper.GetLogFilePath()) ?? Path.Combine(AppContext.BaseDirectory, "Log"),
                    Path.Combine(ParamHelper.FileManageOptions.Outdir, "log"),
                    Path.Combine(ParamHelper.FileManageOptions.Outdir, "log2")
                };

                JobDetail = JobBuilder.Create<ClearLogJob>()
                      .WithIdentity(JobKey)
                      .WithDescription(SchedulerResource.ClearLog)
                      .RequestRecovery()
                      .Build();
                JobDetail.JobDataMap[nameof(ClearLogService)] = new ClearLogService(ParamHelper.KeepLogDay, dirs, logger);
                JobDetail.JobDataMap["logger"] = logger;

                TimeSpan time = new TimeSpan(21, 00, 0); // не имеет смысла выводить на параметры

                Trigger = TriggerBuilder.Create()
                   .WithIdentity(new TriggerKey(SchedulerResource.ClearLog, SchedulerResource.GroupStart))
                   .WithDescription(SchedulerResource.ClearLog)
                   .WithDailyTimeIntervalSchedule
                     (x =>
                        x.WithIntervalInHours(24)
                       .OnEveryDay()
                       .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(time.Hours, time.Minutes))
                       .WithMisfireHandlingInstructionFireAndProceed()
                     )
                    .Build();

                return this;
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError("Произошла ошибка построения шедуллера {name}: {ex}", nameof(ClearLogScheduler), ex);
                return null;
            }
        }
    }
}
