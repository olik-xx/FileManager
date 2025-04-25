namespace FileManager
{
    using FileManager.Helpers;
    using FileManager.Scheduler;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Web;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;

    internal class Program
    {
        /// <summary>
        /// Представляет планировщик задач.
        /// </summary>
        private static SchedulerControl _scheduler;

        static void Main(string[] args)
        {
            Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            logger.Info($"---- Старт службы. Версия v.{ParamHelper.Version} ----");

            bool onStop = false;
            try
            {
                var webApplicationOptions = new WebApplicationOptions()
                {
                    ContentRootPath = AppContext.BaseDirectory, 
                    Args = args,
                    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
                };
                var builder = WebApplication.CreateBuilder(webApplicationOptions);


                builder.Logging.ClearProviders();
                builder.Host.UseNLog();
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(60);
                });

                builder.Host.UseWindowsService();
                ParamHelper.WebApplication = builder.Build();
                //
                ParamHelper.Configuration = builder.Configuration;
                builder.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);

                // планировщик
                _scheduler = new SchedulerControl(new TaskStorage());
                _scheduler.Start();
                logger.Info("Шедуллер успешно запущен");

                // автоудаление файловых логов при запуске
                if (ParamHelper.KeepLogDay > 0)
                    _scheduler.RunImmediately(ClearLogScheduler.JobKey).ConfigureAwait(false).GetAwaiter();

                //
                ParamHelper.WebApplication.Run();

                onStop = true;
                logger.Info("Остановка службы...");

                if (_scheduler != null)
                {
                    _scheduler.Shutdown(false);
                    logger.Info("Шедуллер успешно остановлен");
                }

                logger.Info($"---- Служба успешно остановлена. Версия v.{ParamHelper.Version} ---- ");
            }
            catch (Exception ex)
            {
                logger?.Error($"Возникла непредвиденная ошибка при {(onStop ? "остановке" : "старте")} системы: {ex}");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
    }
}
