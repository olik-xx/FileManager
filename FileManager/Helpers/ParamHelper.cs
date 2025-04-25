[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FileManagerTest")]
namespace FileManager.Helpers
{
    using System.Reflection;
    using FileManager.Cache;
    using FileManager.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Представляет вспомогательный класс по настройкам и параметрам.
    /// </summary>
    internal static class ParamHelper
    {
        /// <summary>
        /// Возвращает или задаёт веб-приложение.
        /// </summary>
        public static WebApplication WebApplication { get; set; }

        /// <summary>
        /// Возвращает или задаёт конфигурационные настройки сервиса.
        /// </summary>
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// Представляет версию сборки.
        /// </summary>
        private static string _version;

        /// <summary>
        /// Возвращает версию сборки.
        /// </summary>
        public static string Version
        {
            get
            {
                if (!string.IsNullOrEmpty(_version))
                    return _version;
                return _version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Представляет количество дней хранения файлов лога.
        /// </summary>
        private static int? _keepLogDay;

        /// <summary>
        /// Возвращает количество дней хранения файлов лога.
        /// </summary>
        /// <value>
        /// Значение по умолчанию - 3 дня
        /// При значении 0 файлы будут храниться неограниченное количество времени.
        /// </value>
        internal static int KeepLogDay
        {
            get
            {
                if (_keepLogDay.HasValue)
                    return _keepLogDay.Value;

                int value = Configuration.GetValue<int>("KeepLogDay");
                if (value < 0)
                    return (_keepLogDay = 3).Value; // значение по умолчанию

                return (_keepLogDay = value).Value;
            }
        }

        /// <summary>
        /// Представляет базовый адрес службы.
        /// </summary>
        private static string _baseUri;

        /// <summary>
        /// Возвращает базовый адрес службы.
        /// </summary>
        public static string BaseUri
        {
            get
            {
                if (!string.IsNullOrEmpty(_baseUri)) return _baseUri;

                string value = Configuration.GetValue<string>("Urls");
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                    value = value + "/";

                return (_baseUri = value);
            }
        }

        /// <summary>
        /// Представляет параметры обработки файлов.
        /// </summary>
        internal static FileManageOptions FileManageOptions { get; set; } = new FileManageOptions();

        /// <summary>
        /// Представляет кеш предварительно обработанных файлов.
        /// </summary>
        private static UnionFileCache _unionFileCache = null;

        /// <summary>
        /// Возвращает кеш предварительно обработанных файлов для поиска пересечений между ними.
        /// </summary>
        internal static UnionFileCache UnionFileCache
        {
            get
            {
                return _unionFileCache ?? (_unionFileCache = new UnionFileCache());
            }
        }

        /// <summary>
        /// Возвращает или задаёт дату последней операции поиска пересечений между файлами.
        /// </summary>
        public static DateTime? LastUnionDate { get; set; }

    }
}
