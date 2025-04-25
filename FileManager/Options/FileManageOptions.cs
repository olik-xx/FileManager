namespace FileManager.Options
{
    /// <summary>
    /// Представляет параметры обработки файлов.
    /// </summary>
    internal class FileManageOptions
    {
        /// <summary>
        /// Представляет интервал перечитывания файлового каталога, в минутах.
        /// </summary>
        private int _timeoutIdle = 20;

        /// <summary>
        /// Возвращает или задаёт интервал перечитывания файлового каталога, в минутах.
        /// </summary>
        /// <value>
        /// Значение по умолчанию - 20 минут.
        /// Минимальное значение - 2 минуты.
        /// </value>
        public int TimeoutIdle
        {
            get
            {
                return _timeoutIdle;
            }
            set
            {
                _timeoutIdle = (value < 2) ? 2 : value;
            }
        }

        /// <summary>
        /// Представляет  интервал поиска пересечений между обработанными файлами, в минутах.
        /// </summary>
        private int _interFileTimeoutIdle = 20;

        /// <summary>
        /// Возвращает или задаёт интервал поиска пересечений между обработанными файлами, в минутах.
        /// </summary>
        /// <value>
        /// Значение по умолчанию - 5 минут.
        /// Минимальное значение - 1 минута.
        /// </value>
        public int InterFileTimeoutIdle
        {
            get
            {
                return _interFileTimeoutIdle;
            }
            set
            {
                _interFileTimeoutIdle = (value < 1) ? 5 : value;
            }
        }

        /// <summary>
        /// Представляет количество одновременно обрабатываемых файлов.
        /// </summary>
        private int _count;

        /// <summary>
        /// Возвращает или задаёт количество одновременно обрабатываемых файлов.
        /// </summary>
        /// <value>
        /// Значение по умолчанию: 5 файлов
        /// Минимальное значение: 1 файл.
        /// Максимальное значение: 25 файлов.
        /// </value>
        public int Count
        {
            get
            {
                return _count; 
            }
            set
            {
                _count = value < 1 
                    ? 5 
                    : (value > 25 ? 25 : value); 
            }
        }

        /// <summary>
        /// Представляет размер буфера чтения данных.
        /// </summary>
        private int _buffer = 8;

        /// <summary>
        /// Представляет размер буфера чтения данных.
        /// </summary>
        /// <value>
        /// Минимальное значение: 8,
        /// Значение по умолчанию: 2048
        /// </value>
        public int Buffer
        {
            get 
            {
                return _buffer; 
            }
            set 
            {
                _buffer = value < 8 ? 8 : value; 
            }
        }


        /// <summary>
        /// Представляет каталог поиска файлов.
        /// </summary>
        private string _dir;

        /// <summary>
        /// Возвращает или задаёт каталог поиска файлов.
        /// </summary>
        /// <value>Значение по умолчанию: example\in</value>
        public string Dir
        {
            get
            {
                return _dir; 
            }
            set
            {
                _dir = string.IsNullOrWhiteSpace(value) ? Path.Combine(AppContext.BaseDirectory, @"example\in") : value; 
            }
        }

        /// <summary>
        /// Представляет каталог обработанных файлов.
        /// </summary>
        private string _outdir;

        /// <summary>
        /// Возвращает или задаёт каталог обработанных файлов.
        /// </summary>
        /// <value>Значение по умолчанию: example\out</value>
        public string Outdir
        {
            get
            {
                return _outdir;
            }
            set
            {
                _outdir = string.IsNullOrWhiteSpace(value) ? Path.Combine(AppContext.BaseDirectory, @"example\out") : value;
            }
        }

    }
}
