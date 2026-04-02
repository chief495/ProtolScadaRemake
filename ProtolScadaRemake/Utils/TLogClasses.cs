using ProtolScada;

namespace ProtolScadaRemake
{
    public class TLogRecord
    {
        public DateTime Time { get; set; }
        public string GroupName { get; set; }
        public string Text { get; set; }
        public short ImageIndex { get; set; }

        public TLogRecord()
        {
            GroupName = string.Empty;
            Text = string.Empty;
            Time = DateTime.Now;
        }

        // Метод для получения типа записи по группе
        public string GetRecordType()
        {
            string groupLower = GroupName.ToLower();

            if (groupLower.Contains("авария") || ImageIndex == 4) return "Авария";
            if (groupLower.Contains("предупреждение") || ImageIndex == 2) return "Предупреждение";
            if (groupLower.Contains("отказ") || ImageIndex == 5) return "Отказ";
            if (groupLower.Contains("сбой") || groupLower.Contains("ошибка")) return "Сбой";
            if (groupLower.Contains("пользователь") || ImageIndex == 1) return "Пользователь";
            if (groupLower.Contains("система") || ImageIndex == 0) return "Система";

            return "Другое";
        }
    }

    public class TLogList
    {
        private List<TLogRecord> _records = new List<TLogRecord>();
        private readonly object _lock = new object();
        private DBUtils _dbUtils;

        public TLogList(DBUtils dbUtils)
        {
            _dbUtils = dbUtils;
        }

        public async Task Add(string group, string text, short imageIndex, bool saveToDb = true)
        {
            var record = new TLogRecord
            {
                Time = DateTime.Now,
                GroupName = group,
                Text = text,
                ImageIndex = imageIndex
            };

            lock (_lock)
            {
                _records.Add(record);
                if (_records.Count > 1000) _records.RemoveAt(0);
            }

            // Асинхронно сохраняем в БД
            if (saveToDb && _dbUtils != null)
            {
                await _dbUtils.SaveLogRecordAsync(record);
            }
        }

        public async Task LoadFromDatabaseAsync(int limit = 100)
        {
            if (_dbUtils == null) return;

            var dbRecords = await _dbUtils.LoadLogRecordsAsync(limit);

            lock (_lock)
            {
                _records.Clear();
                _records.AddRange(dbRecords);
            }
        }

        public List<TLogRecord> GetAllRecords()
        {
            lock (_lock)
            {
                // Возвращаем в обратном порядке (новые сверху)
                var result = new List<TLogRecord>(_records);
                result.Reverse();
                return result;
            }
        }

        public int Count
        {
            get { lock (_lock) { return _records.Count; } }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _records.Clear();
            }
        }
    }
}