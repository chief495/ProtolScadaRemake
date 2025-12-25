// TTrendTag.cs
using System;
using System.Collections.Generic;
using System.Linq;

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

        public TTrendTag() { }

        public TTrendTag(string name, string description, string unit = "", ushort period = 60, uint maxLength = 1000)
        {
            Name = name;
            Description = description;
            Unit = unit;
            Period = period;
            MaxLength = maxLength;
        }

        public bool Update(TVariableTag variable)
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
                return true;
            }

            return false;
        }

        private void AddRecord(TTrendTagRecord record)
        {
            Records.Add(record);

            if (Records.Count > MaxLength)
            {
                Records.RemoveAt(0);
            }
        }

        public List<TTrendTagRecord> GetRecordsByTimeRange(DateTime from, DateTime to)
        {
            return Records.Where(r => r.DateTime >= from && r.DateTime <= to).ToList();
        }
    }
}