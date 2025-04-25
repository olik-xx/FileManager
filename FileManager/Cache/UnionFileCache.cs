namespace FileManager.Cache
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Кеш предварительно обработанных файлов.
    /// </summary>
    internal class UnionFileCache
    {
        /// <summary>
        /// Представляет кеш предварительно обработанных файлов на основе пар значений "ключ - значение".
        /// </summary>
        private ConcurrentDictionary<string, FileCache> _cache;

        private object _locker = new object();

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public UnionFileCache()
        {
            _cache = new ConcurrentDictionary<string, FileCache>();
        }

        /// <summary>
        /// Добавляет новую или обновляет существующую запись кеша по указанной паре "ключ-значение".
        /// </summary>
        /// <param name="value">Кеш файла.</param>
        public void Set(FileCache value)
        {
            lock (_locker)
            {
                try
                {
                    value.AddDate = DateTime.Now;
                    value.ClearDuplicate(); // сбрасываем предыдущие дубликаты обработки файла, чтобы хранить дубликаты пересечений файлов
                                            
                    _cache.AddOrUpdate(value.Filename, value, (keyItem, valueItem) => value);
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Вычисляет, было ли добавлены новые значения на указанную дату <paramref name="date"/>.
        /// </summary>
        /// <param name="date">Дата добавления файлового кеша</param>
        /// <returns>Значение <see langword="true"/>, если кеш содержит новые значения относительно указанной даты, иначе - значение <see langword="false"/>.</returns>
        public bool AnyGreater(DateTime date)
        {
            lock (this._locker)
            {
                return _cache.Values.Any(x=> x.AddDate > date);
            }
        }

        /// <summary>
        /// Возвращает коллекцию значений на дату <paramref name="date"/>.
        /// </summary>
        /// <returns>Набор элементов кеша, чья дата добавления меньше указанной даты.</returns>
        public FileCache[] Values(DateTime date)
        {
            lock (_locker)
            {
                return _cache.Values.Where(x => x.AddDate <= date).OrderByDescending(x=> x.AddDate).ToArray();
            }
        }

        /// <summary>
        /// Возвращает количество записей кеша.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this._locker)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Возвращает признак, содержится ли кеш указанное значение ключа.
        /// </summary>
        /// <param name="key">Название файла.</param>
        /// <returns>Значение <see langword="true"/>, если кеш содержит данный ключ, иначе - значение <see langword="false"/>.</returns>
        public bool ContainsKey(string key)
        {
            lock (this._locker)
            {
                return _cache.ContainsKey(key);
            }
        }

        /// <summary>
        /// Получает значение по ключу.
        /// </summary>
        /// <param name="key">Название файла.</param>
        /// <returns>Кеш файла.</returns>
        /// <value>Кеш файла, если указанный ключ был найден и содержит значение; иначе - значение <see langword="null"/>.</value>
        public FileCache TryGet(string key)
        {
            lock (_locker)
            {
                if (_cache.TryGetValue(key, out FileCache value))
                    return value;

                return null;
            }
        }

        /// <summary>
        /// Удаляет значение по ключу.
        /// </summary>
        /// <param name="key">Название файла.</param>
        public void TryRemove(string key)
        {
            lock (this._locker)
            {
                this._cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Возвращает коллекцию ключей.
        /// </summary>
        /// <returns>Коллекция ключей параметров.</returns>
        public ICollection<string> Keys()
        {
            lock (this._locker)
            {
                return _cache.Keys;
            }
        }

        /// <summary>
        /// Возвращает коллекцию значений.
        /// </summary>
        /// <returns>Коллекция элементов кеша.</returns>
        public ICollection<FileCache> Values()
        {
            lock (_locker)
            {
                return _cache.Values;
            }
        }

        /// <summary>
        /// Сбрасывает все значения из кеша.
        /// </summary>
        public void Clear()
        {
            lock (this._locker)
            {
                _cache.Clear();
            }
        }
    }
}
