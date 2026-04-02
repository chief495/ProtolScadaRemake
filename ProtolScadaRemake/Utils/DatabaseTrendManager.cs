using ProtolScada;

namespace ProtolScadaRemake
{
    public class DatabaseTrendManager
    {
        private readonly DBUtils _dbUtils;
        private readonly Dictionary<string, TTrendTag> _trendsCache = new();
        private bool _isInitialized = false;

        public DatabaseTrendManager(DBUtils dbUtils)
        {
            _dbUtils = dbUtils ?? throw new ArgumentNullException(nameof(dbUtils));
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_isInitialized) return;

                bool tablesExist = await _dbUtils.CheckTrendTablesExistAsync();

                if (!tablesExist)
                {
                    System.Diagnostics.Debug.WriteLine("Таблицы трендов не найдены в БД");
                    return;
                }

                var configs = await _dbUtils.LoadTrendConfigsAsync();

                foreach (var config in configs)
                {
                    var trend = new TTrendTag(config.Name, config.Description, config.Unit,
                        (ushort)config.Period, (uint)config.MaxLength, _dbUtils)
                    {
                        TrendType = config.TrendType
                    };
                    trend.SetTagId(config.TagID);

                    _trendsCache[config.TagID] = trend;
                }

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine($"Загружено {_trendsCache.Count} трендов из БД");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации DatabaseTrendManager: {ex.Message}");
            }
        }

        public List<TTrendTag> GetAllTrends()
        {
            return new List<TTrendTag>(_trendsCache.Values);
        }

        public TTrendTag GetTrend(string tagId)
        {
            return _trendsCache.TryGetValue(tagId, out var trend) ? trend : null;
        }

        public async Task<bool> UpdateTrendValueAsync(string tagId, TVariableTag variable)
        {
            try
            {
                if (!_trendsCache.TryGetValue(tagId, out var trend))
                {
                    return false;
                }

                bool updated = trend.Update(variable, true);
                return updated;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления тренда '{tagId}': {ex.Message}");
                return false;
            }
        }

        public async Task<List<TTrendTagRecord>> LoadTrendHistoryAsync(string tagId,
            DateTime fromDate, DateTime toDate, int maxPoints = 1000)
        {
            try
            {
                if (!_trendsCache.TryGetValue(tagId, out var trend))
                {
                    return new List<TTrendTagRecord>();
                }

                await trend.LoadFromDatabaseAsync(fromDate, toDate, maxPoints);
                return trend.GetRecordsByTimeRange(fromDate, toDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории тренда '{tagId}': {ex.Message}");
                return new List<TTrendTagRecord>();
            }
        }

        public async Task UpdateAllTrendsAsync(TVariableList variables)
        {
            try
            {
                foreach (var kvp in _trendsCache)
                {
                    var variable = variables.GetByName(kvp.Value.Name);
                    if (variable != null)
                    {
                        await UpdateTrendValueAsync(kvp.Key, variable);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления всех трендов: {ex.Message}");
            }
        }

        public bool IsInitialized => _isInitialized;
        public int TrendCount => _trendsCache.Count;
    }
}