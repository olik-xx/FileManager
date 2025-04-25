using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("FileManagerTest")]
namespace FileManager.Scheduler
{
    using System.IO;
    using FileManager.Cache;
    using FileManager.Helpers;
    using FileManager.Options;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис по исполнению задания "Поиск дубликатов".
    /// </summary>
    internal class DistinctFileService
    {
        /// <summary>
        /// Представляет логер сервиса.
        /// </summary>
        private readonly ILogger<DistinctFileService> _logger;

        /// <summary>
        /// Представляет параметры обработки файлов.
        /// </summary>
        private readonly FileManageOptions _options;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным логером и параметрами обработки файлов.
        /// </summary>
        /// <param name="options">Параметры обработки файлов.</param>
        /// <param name="logger">Файловый логер задания.</param>
        public DistinctFileService(FileManageOptions options, ILogger<DistinctFileService> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет само задание по поиску и обработке дубликатов в файле. 
        /// </summary>
        /// <param name="fileInfo">Информация по файлу.</param>
        /// <param name="jobName">Ключ задания.</param>
        /// <returns>Возвращает набор дубликатов.</returns>
        public async Task<byte[][]> Execute(FileInfo fileInfo, string jobName)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            FileCache fileCache = new FileCache(fileInfo.Name);

            //HashSet<string> set = new HashSet<string>(); //оптимален диапазон 100-10000 элементов).
            using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[_options.Buffer];
                int bytes = 0;
                while ((bytes = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] indata = new byte[bytes];
                    Array.Copy(buffer, indata, bytes);

                    //string str = Encoding.UTF8.GetString(buffer);

                    string hash = Helper.ComputeContentHash(indata); 
                    fileCache.Set(hash, indata);
                }
            }
           
            _logger.LogInformation("[{jobName}] Файл успешно обработан. Найдено дубликатов: {count}.", jobName, fileCache.DuplicateCount);
            byte[][] duplicateData = fileCache.DuplicateValues().ToArray();

            // сбрасываем
            fileCache.ToFile(_options.Outdir);
            fileCache.Duplicate2File(Path.Combine(_options.Outdir, "log"));

            // добавляем в кеш пересечений
            ParamHelper.UnionFileCache.Set(fileCache);

            // удаляем - переносим (?)
            File.Delete(fileInfo.FullName);

            return duplicateData;
        }
    }
}
