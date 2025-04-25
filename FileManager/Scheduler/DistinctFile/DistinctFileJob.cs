namespace FileManager.Scheduler
{
    using FileManager.Options;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Запускает на выполнение задание "Поиск дубликатов в файле".
    /// </summary>
    [DisallowConcurrentExecution]
    internal class DistinctFileJob : IJob
    {
        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            ILogger<DistinctFileService> logger = null;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                logger = dataMap["logger"] as ILogger<DistinctFileService>;

                if (!(dataMap["options"] is FileManageOptions options))
                    throw new ArgumentNullException(nameof(options));

                if (!(dataMap["file"] is FileInfo fileInfo))
                    throw new ArgumentNullException(nameof(fileInfo));

                logger?.LogInformation("Запуск задания \"{jobName}\".", context.JobDetail.Key.Name);

                var service = new DistinctFileService(options, logger);
                await service.Execute(fileInfo, context.JobDetail.Key.Name);
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
                    logger.LogError("Произошла ошибка во время выполнения задания \"{jobName}\": {ex}", context.JobDetail.Key.Name, ex);
            }
        }
    }
}
