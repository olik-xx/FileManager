using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("FileManagerTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace FileManager.Scheduler.FindFiles
{
    using FileManager.Helpers;
    using FileManager.Resources;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис по исполнению задания "Поиск файлов".
    /// </summary>
    internal class FindFilesService
    {
        /// <summary>
        /// Представляет логер сервиса.
        /// </summary>
        private readonly ILogger<FindFilesService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным логером.
        /// </summary>
        /// <param name="logger">Файловый логер задания.</param>
        public FindFilesService(ILogger<FindFilesService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Выполняет само задание по поиску файлов.
        /// </summary>
        public FileInfo[] Execute()
        {
            if (!Directory.Exists(ParamHelper.FileManageOptions.Dir))
                throw new DirectoryNotFoundException(string.Format(ExceptionResource.DirectoryNotFound, ParamHelper.FileManageOptions.Dir));

            if (!Directory.Exists(Path.Combine(ParamHelper.FileManageOptions.Outdir,"log")))
                Directory.CreateDirectory(ParamHelper.FileManageOptions.Outdir);

            FileInfo[] data = new DirectoryInfo(ParamHelper.FileManageOptions.Dir).GetFiles();
            if (data.Length > 0)
            {
                int fileCount = new[] { data.Length, ParamHelper.FileManageOptions.Count }.Min();

                FileInfo[] outdata = new FileInfo[fileCount];
                Array.Copy(data, outdata, outdata.Length);

                _logger.LogInformation("Задание \"{jobName}\" успешно выполнено. Обработано файлов: {count}.", SchedulerResource.FindFiles, fileCount);

                return outdata;
            }

            _logger.LogWarning("Задание \"{jobName}\" успешно выполнено. Файлы не были найдены", SchedulerResource.FindFiles);
            return Array.Empty<FileInfo>();

        }
    }
}
