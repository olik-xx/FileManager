namespace FileManager.Scheduler
{
    using System.IO;
    using FileManager.Resources;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис по исполнению задания "Автоудаление логов".
    /// </summary>
    public class ClearLogService
    {
        /// <summary>
        /// Представляет логер.
        /// </summary>
        private readonly ILogger<ClearLogService> _logger;

        /// <summary>
        /// Возвращает или задаёт количество дней хранения файловых логов.
        /// </summary>
        private int _keepLogDay;

        /// <summary>
        /// Возвращает или задаёт каталоги файловых логов.
        /// </summary>
        private string[] _dirs;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанными параметрами.
        /// </summary>
        /// <param name="keepLogDay">Количество дней хранения файловых логов.</param>
        /// <param name="dirs">Каталоги обработки.</param>
        /// <param name="logger">Логер сервиса.</param>
        public ClearLogService(int keepLogDay, string[] dirs, ILogger<ClearLogService> logger) 
        {
            _keepLogDay = keepLogDay;
            _dirs = dirs;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет задание по удалению файловых логов.
        /// </summary>
        public void Execute()
        {
            for (int i = 0; i < _dirs.Length; i++)
            {
                if (!Directory.Exists(_dirs[i])) continue;
                foreach (FileInfo file in new DirectoryInfo(_dirs[i]).GetFiles())
                {
                    if (file.LastWriteTime.AddDays(_keepLogDay) < DateTime.Now)
                        file.Delete();
                }
            }
           

            _logger.LogInformation("Задание \"{jobName}\" успешно выполнено. Каталог(и) {dir} успешно очищен(ы).", SchedulerResource.ClearLog,string.Join(",", _dirs)); 
        }
    }
}
