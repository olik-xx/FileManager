namespace FileManager.Scheduler
{
    using System;
    using System.Threading.Tasks;
    using FileManager.Resources;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Запускает на выполнение задание "Автоудаление логов".
    /// </summary>
    [DisallowConcurrentExecution]
    public class ClearLogJob : IJob
    {
        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            ILogger<ClearLogService> logger = null;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var service = dataMap[nameof(ClearLogService)] as ClearLogService;
                if (service == null)
                    throw new ArgumentNullException(nameof(ClearLogService), "Service is not initialize");

                logger = dataMap["logger"] as ILogger<ClearLogService>; 
                logger.LogInformation("Запуск задания \"{jobName}\".", SchedulerResource.ClearLog);

                await Task.Run(service.Execute);
            }
            catch (TaskCanceledException)
            {
                // ignore: здесь скорее всего перезапуск задания
            }
            catch (NullReferenceException)
            {
                // ignore: здесь скорее всего перезапуск задания
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError("Произошла ошибка во время выполнения задания \"{jobName}\": {ex}", SchedulerResource.ClearLog, ex);
                throw new JobExecutionException(ex);
            }
        }
    }
}
