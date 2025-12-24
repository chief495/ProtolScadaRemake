// TrendDbManager.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TrendDbManager
    {
        private readonly TGlobal _global;

        public TrendDbManager(TGlobal global)
        {
            _global = global;
            // Здесь будет инициализация подключения к БД
        }

        // Временные заглушки для методов
        public Task<int> SaveTrendAsync(TTrendTag trend)
        {
            return Task.FromResult(1);
        }

        public Task<bool> SaveValueAsync(int trendId, TTrendTagRecord record)
        {
            return Task.FromResult(true);
        }

        public Task<List<TTrendTagRecord>> LoadHistoryAsync(string trendName, DateTime from, DateTime to)
        {
            // Временная заглушка - возвращаем пустой список
            return Task.FromResult(new List<TTrendTagRecord>());
        }

        public Task<List<PumpData>> LoadPumpHistoryAsync(string pumpName, DateTime from, DateTime to)
        {
            return Task.FromResult(new List<PumpData>());
        }

        public Task<TrendStats> GetStatsAsync(string trendName, DateTime from, DateTime to)
        {
            return Task.FromResult(new TrendStats());
        }
    }

    public class PumpData
    {
        public DateTime Timestamp { get; set; }
        public double Frequency { get; set; }
        public double Current { get; set; }
        public double Power { get; set; }
        public double Temperature { get; set; }
        public int Status { get; set; }
    }

    public class TrendStats
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Avg { get; set; }
        public int Count { get; set; }
    }
}