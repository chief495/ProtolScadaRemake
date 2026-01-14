using ProtolScada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TTrendTag
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public ushort Period { get; set; } = 60;
        public uint MaxLength { get; set; } = 1000;
        public string TrendType { get; set; } = "analog";
        public List<TTrendTagRecord> Records { get; private set; } = new List<TTrendTagRecord>();

        private DBUtils _dbUtils;
        private DateTime _lastDbSave = DateTime.MinValue;
        private string _tagId;

        public TTrendTag() { }

        public TTrendTag(string name, string description, string unit = "",
                        ushort period = 60, uint maxLength = 1000, DBUtils dbUtils = null)
        {
            Name = name;
            Description = description;
            Unit = unit;
            Period = period;
            MaxLength = maxLength;
            _dbUtils = dbUtils;
            _tagId = name;
        }

        public void SetDbUtils(DBUtils dbUtils)
        {
            _dbUtils = dbUtils;
        }

        public void SetTagId(string tagId)
        {
            _tagId = tagId;
        }

        public bool Update(TVariableTag variable, bool saveToDb = true)
        {
            if (variable == null) return false;

            bool needUpdate = true;
            if (Records.Count > 0)
            {
                var lastRecord = Records.Last();
                needUpdate = (DateTime.Now - lastRecord.DateTime).TotalSeconds >= Period;
            }

            if (needUpdate)
            {
                var record = new TTrendTagRecord
                {
                    ValueReal = variable.ValueReal,
                    ValueString = variable.ValueString,
                    DateTime = DateTime.Now
                };

                AddRecord(record);

                if (saveToDb && _dbUtils != null && (DateTime.Now - _lastDbSave).TotalSeconds >= 1)
                {
                    _ = SaveToDatabaseAsync(record);
                    _lastDbSave = DateTime.Now;
                }

                return true;
            }

            return false;
        }

        private async Task SaveToDatabaseAsync(TTrendTagRecord record)
        {
            try
            {
                if (_dbUtils != null && !string.IsNullOrEmpty(_tagId))
                {
                    await _dbUtils.SaveTrendPointAsync(_tagId, record.DateTime,
                        record.ValueReal, null, 192);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения тренда в БД: {ex.Message}");
            }
        }

        private void AddRecord(TTrendTagRecord record)
        {
            Records.Add(record);

            if (Records.Count > MaxLength)
            {
                Records.RemoveAt(0);
            }
        }

        public async Task LoadFromDatabaseAsync(DateTime from, DateTime to, int maxPoints = 1000)
        {
            if (_dbUtils == null || string.IsNullOrEmpty(_tagId)) return;

            try
            {
                var dbPoints = await _dbUtils.LoadTrendDataAsync(_tagId, from, to, maxPoints);

                Records.Clear();
                foreach (var point in dbPoints)
                {
                    Records.Add(new TTrendTagRecord
                    {
                        DateTime = point.DateTime,
                        ValueReal = point.ValueReal,
                        ValueString = point.ValueReal.ToString("F2")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки тренда из БД: {ex.Message}");
            }
        }

        public List<TTrendTagRecord> GetRecordsByTimeRange(DateTime from, DateTime to)
        {
            return Records.Where(r => r.DateTime >= from && r.DateTime <= to).ToList();
        }
    }
}