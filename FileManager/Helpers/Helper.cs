namespace FileManager.Helpers
{
    using System.Security.Cryptography;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    /// <summary>
    /// Представляет вспомогательный класс по общим методам и функциям.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Перечитывает конфигурационный файл.
        /// </summary>
        internal static void Rebind()
        {
            ParamHelper.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);
        }

        /// <summary>
        /// Возвращает путь к файлу лога для указанного названия таргета <paramref name="targetName"/>.
        /// </summary>
        /// <param name="targetName">Название таргета.</param>
        /// <returns>Абсолютный путь к текущему файлу логирования.</returns>
        internal static string GetLogFilePath(string targetName = "logfile")
        {
            if (LogManager.Configuration != null) // && LogManager.Configuration.ConfiguredNamedTargets.Count != 0
            {
                Target target = LogManager.Configuration.FindTargetByName(targetName); // todo вернуться и забирать массив директорий
                if (target == null) return null;

                if (target is WrapperTargetBase wrapperTarget)
                {
                    if (wrapperTarget.WrappedTarget is FileTarget fwtarget)
                        return fwtarget.FileName.Render(new LogEventInfo() { TimeStamp = DateTime.Now });

                    if (wrapperTarget.WrappedTarget is AsyncTargetWrapper afwtarget)
                        return ((FileTarget)afwtarget.WrappedTarget).FileName.Render(new LogEventInfo() { TimeStamp = DateTime.Now });
                }

                if (target is FileTarget ftarget)
                    return ftarget.FileName.Render(new LogEventInfo() { TimeStamp = DateTime.Now });

            }

            return null;
        }

        /// <summary>
        /// Вычисляет хеш для указанной строки.
        /// </summary>
        /// <param name="content">Входная строка.</param>
        /// <returns>Хеш значения.</returns>
        internal static string ComputeContentHash(byte[] content)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(content);
                return Convert.ToBase64String(data); 
            }
        }
    }
}
