using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("FileManagerTest")]
namespace FileManager.Scheduler
{
    using FileManager.Cache;
    using FileManager.Helpers;
    using FileManager.Options;
    using FileManager.Resources;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис по исполнению задания "Поиск пересечений между файлами".
    /// </summary>
    internal class UnionFileService
    {
        /// <summary>
        /// Представляет логер сервиса.
        /// </summary>
        private readonly ILogger<UnionFileService> _logger;

        /// <summary>
        /// Представляет параметры обработки файлов.
        /// </summary>
        private readonly FileManageOptions _options;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным логером и параметрами обработки файлов.
        /// </summary>
        /// <param name="options">Параметры обработки файлов.</param>
        /// <param name="logger">Файловый логер задания.</param>
        public UnionFileService(FileManageOptions options, ILogger<UnionFileService> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет само задание по поиску и обработке дубликатов между файлами.
        /// </summary>
        /// <returns>Возвращает набор пересечений между файлами.</returns>
        public byte[][] Execute()
        {
            if (ParamHelper.UnionFileCache.Count == 0) return Array.Empty<byte[]>();
            if (ParamHelper.LastUnionDate.HasValue && !ParamHelper.UnionFileCache.AnyGreater(ParamHelper.LastUnionDate.Value)) return Array.Empty<byte[]>();

            DateTime date = DateTime.Now;
            FileCache[] caches = ParamHelper.UnionFileCache.Values(date);

            string[] excepted = (from k in caches.SelectMany(x => x.Keys()) group k by k into grp where grp.Count() > 1 select grp.Key).ToArray();

            List<byte[]> duplicateData = new List<byte[]>();
            if (excepted.Length == 0)
            {
                _logger.LogInformation("Задание \"{jobName}\" успешно выполнено. Обработано файлов: {count}. Дубликаты между файлами не найдены", SchedulerResource.UnionFile, caches.Length);
            }
            else
            {
                _logger.LogInformation("Задание \"{jobName}\" успешно выполнено. Обработано файлов: {count}. Найдено дубликатов: {exclude}", SchedulerResource.UnionFile, caches.Length, excepted.Length);

                for (int i = 0; i < caches.Length; i++)
                {
                    caches[i].TryRemove(excepted);
                    duplicateData.AddRange(caches[i].DuplicateValues());

                    caches[i].ToFile(_options.Outdir);
                    caches[i].Duplicate2File(Path.Combine(_options.Outdir, "log2"));
                }
            }

            //
            ParamHelper.LastUnionDate = date; // установили
            return duplicateData.Distinct().ToArray();
        }

    }
}
