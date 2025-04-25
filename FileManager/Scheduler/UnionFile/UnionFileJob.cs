namespace FileManager.Scheduler
{
    using FileManager.Resources;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Запускает на выполнение задание "Поиск пересечений между файлами".
    /// </summary>
    [DisallowConcurrentExecution]
    internal class UnionFileJob : IJob
    {
        /// <inheritdoc/>
        public Task Execute(IJobExecutionContext context)
        {
            ILogger<UnionFileService> logger = null;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var service = dataMap[nameof(UnionFileService)] as UnionFileService;
                if (service == null)
                    throw new ArgumentNullException(nameof(UnionFileService), "Service is not initialize");

                logger = dataMap["logger"] as ILogger<UnionFileService>;
                logger.LogInformation("Запуск задания \"{jobName}\".", SchedulerResource.UnionFile);

                return Task.Run(service.Execute);
            }
            catch (TaskCanceledException)
            {
                // ignore: здесь скорее всего перезапуск задания
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError("Произошла ошибка во время выполнения задания \"{jobName}\": {ex}", SchedulerResource.UnionFile, ex);

                throw new JobExecutionException(ex);
            }
            return Task.CompletedTask;
        }
    }
}
