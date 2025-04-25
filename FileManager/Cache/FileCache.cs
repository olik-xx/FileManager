namespace FileManager.Cache
{
    using System.Collections.Concurrent;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Кеш предварительной обработки файла.
    /// </summary>
    internal class FileCache
    {
        /// <summary>
        /// Представляет кеш данных на основе пар значений "ключ - значение".
        /// </summary>
        private ConcurrentDictionary<string, byte[]> _cache;

        /// <summary>
        /// Представляет кеш дубликатов данных на основе пар значений "ключ - значение".
        /// </summary>
        private ConcurrentDictionary<string, byte[]> _duplicateCache;

        protected object _locker = new object();

        /// <summary>
        /// Возвращает или задаёт название файла.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным файлом хранения данных.
        /// </summary>
        /// <param name="fileName">Название файла.</param>
        public FileCache(string fileName)
        {
            _fileName = fileName;

            _cache = new ConcurrentDictionary<string, byte[]>();
            _duplicateCache = new ConcurrentDictionary<string, byte[]>();
            //FillFromFile();

        }

        /// <summary>
        /// Возвращает название файла.
        /// </summary>
        public string Filename
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Возвращает или задаёт дату добавления обработанного файла в кеш <see cref="UnionFileCache"/>.
        /// </summary>
        public DateTime AddDate { get; set; }

        /// <summary>
        /// Добавляет новую или обновляет существующую запись кеша по указанной паре "ключ-значение".
        /// </summary>
        /// <param name="key">Хеш данных.</param>
        /// <param name="value">Набор данных.</param>
        public void Set(string key, byte[] value)
        {
            lock (_locker)
            {
                try
                {
                    if (_cache.ContainsKey(key))
                    {
                        string s = Encoding.UTF8.GetString(value);
                        _duplicateCache.AddOrUpdate(key, value, (k, v) => value);
                    }
                    _cache.AddOrUpdate(key, value, (k, v) => value);
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Удаляет значения для указанного набора ключей <paramref name="key"/>
        /// </summary>
        /// <param name="keys">Набор удаляемых ключей.</param>
        public byte[][] TryRemove(string[] keys)
        {
            lock (this._locker)
            {
                List<byte[]> removed = new List<byte[]>();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (_cache.TryRemove(keys[i], out byte[] value))
                    {
                        removed.Add(value);
                        _duplicateCache.AddOrUpdate(keys[i], value, (k, v) => value);
                    }
                }

                return removed.ToArray();
            }
        }

        /// <summary>
        /// Выполняет сброс кеша в файл.
        /// </summary>
        public void ToFile(string path)
        {
            lock (this._locker)
            {
                // если нужно будет загружать данные
                //string filePath = Path.Combine(path, $"raw_{_fileName}");
                //File.WriteAllText(filePath, JsonConvert.SerializeObject(_cache.Values, Formatting.Indented));

                using (FileStream fs = new FileStream(Path.Combine(path, _fileName), FileMode.Create, FileAccess.Write))
                {
                    foreach (byte[] item in _cache.Values)
                        fs.Write(item, 0, item.Length);
                }
            }
        }

        /// <summary>
        /// Выполняет сброс кеша дубликатов в файл.
        /// </summary>
        public void Duplicate2File(string logPath)
        {
            lock (this._locker)
            {
                string logFile = Path.Combine(logPath, $"Exclude_{Path.GetFileNameWithoutExtension(_fileName)}_{DateTime.Now:yyyy-MM-dd-HHmmss}.log"); //{Path.GetExtension(FileName)}
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

                if (_duplicateCache.Count == 0)
                {
                    File.WriteAllText(logFile, "Дубликаты не были обнаружены");
                }
                else
                {
                    using (FileStream fs = new FileStream(logFile, FileMode.Create, FileAccess.Write))
                    {
                        foreach (byte[] item in _duplicateCache.Values)
                        {
                            fs.Write(item, 0, item.Length);
                            fs.Write(Encoding.UTF8.GetBytes("\r\n\r\n"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Сбрасывает все значения из кеша.
        /// </summary>
        public void ClearDuplicate()
        {
            lock (_locker)
            {
                _duplicateCache.Clear();
            }
        }

        /// <summary>
        /// Возвращает количество записей кеша.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Возвращает количество записей кеша дубликатов.
        /// </summary>
        public int DuplicateCount
        {
            get
            {
                lock (_locker)
                {
                    return _duplicateCache.Count;
                }
            }
        }



        /// <summary>
        /// Возвращает коллекцию ключей.
        /// </summary>
        /// <returns>Коллекция ключей.</returns>
        public ICollection<string> Keys()
        {
            lock (_locker)
            {
                return _cache.Keys;
            }
        }

        /// <summary>
        /// Возвращает коллекцию значений.
        /// </summary>
        /// <returns>Коллекция элементов кеша.</returns>
        public ICollection<byte[]> Values()
        {
            lock (_locker)
            {
                return _cache.Values;
            }
        }

        /// <summary>
        /// Возвращает коллекцию значений.
        /// </summary>
        /// <returns>Коллекция элементов кеша.</returns>
        public ICollection<byte[]> DuplicateValues()
        {
            lock (_locker)
            {
                return _duplicateCache.Values;
            }
        }

        /// <summary>
        /// Возвращает признак, содержится ли кеш указанное значение ключа.
        /// </summary>
        /// <param name="key">Хеш данных.</param>
        /// <returns>Значение <see langword="true"/>, если кеш содержит данный ключ, иначе - значение <see langword="false"/>.</returns>
        public bool ContainsKey(string key)
        {
            lock (_locker)
            {
                return _cache.ContainsKey(key);
            }
        }



        ///// <summary>
        ///// Заполняет кеш данными из файла.
        ///// </summary>
        //protected void FillFromFile()
        //{
        //    try
        //    {
        //        string filePath = Path.Combine(Path.GetDirectoryName(CommonHelper.AppFilePath), _fileName);
        //        string content = File.ReadAllText(filePath);
        //        if (!string.IsNullOrWhiteSpace(content))
        //        {
        //            ICollection<TValue> values = JsonConvert.DeserializeObject<ICollection<TValue>>(content);
        //            if (values != null)
        //                Fill(values);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // ignore
        //    }
        //}


        ///// <summary>
        ///// Перезаполняет кеш новыми данными.
        ///// </summary>
        ///// <param name="values">Коллекция значений.</param>
        //protected override void Fill(ICollection<byte[]> values)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
