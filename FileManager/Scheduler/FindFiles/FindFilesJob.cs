namespace FileManager.Scheduler
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using FileManager.Scheduler.FindFiles;
    using Microsoft.Extensions.Logging;
    using Quartz;

    /// <summary>
    /// Запускает на выполнение задание "Поиск файлов".
    /// </summary>
    [DisallowConcurrentExecution]
    internal class FindFilesJob : IJob
    {
        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            ILogger<FindFilesService> logger = null;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var service = dataMap[nameof(FindFilesService)] as FindFilesService;
                if (service == null)
                    throw new ArgumentNullException(nameof(FindFilesService), "Service is not initialize");

                logger = dataMap["logger"] as ILogger<FindFilesService>;
                logger.LogInformation("Запуск задания \"{jobName}\".", SchedulerResource.FindFiles);

                // перечитали конфиг
                Helper.Rebind();

                FileInfo[] files = await Task.Run(service.Execute);
                context.Put("files", files); // записываем всегда, а обрабатываем уже в listener
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
                    logger.LogError("Произошла ошибка во время выполнения задания \"{jobName}\": {ex}", SchedulerResource.FindFiles, ex);

                throw new JobExecutionException(ex);

                //// для повтора задания
                //JobExecutionException jex = new JobExecutionException(ex);

                //await Task.Delay(TimeSpan.FromMinutes(1));
                ////fire it again
                //jex.RefireImmediately = true;
                //throw jex;
            }
        }
    }
}
