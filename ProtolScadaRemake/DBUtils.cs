using MySql.Data.MySqlClient;
using ProtolScadaRemake;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProtolScada
{
    public class DBUtils
    {
        public string DB_HostName = "";
        public int DB_Port = 0;
        public string DB_UserLogin = "";
        public string DB_Password = "";
        public string DB_Name = "";

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

        private MySqlConnection GetConnection()
        {
            return GetDBConnection(DB_HostName, DB_Port, DB_Name, DB_UserLogin, DB_Password);
        }

        // ============ МЕТОДЫ ДЛЯ ЖУРНАЛА ============

        public async Task<long> SaveLogRecordAsync(TLogRecord record)
        {
            long result = -1;

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    // Проверяем, есть ли уже такая запись
                    string checkSql = @"SELECT `ID` FROM `log` 
                                      WHERE `Time` = @Time 
                                      AND `GroupName` = @GroupName 
                                      AND `Text` = @Text 
                                      LIMIT 1";

                    using (var checkCommand = new MySqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Time", record.Time);
                        checkCommand.Parameters.AddWithValue("@GroupName", record.GroupName);
                        checkCommand.Parameters.AddWithValue("@Text", record.Text);

                        var existingId = await checkCommand.ExecuteScalarAsync();
                        if (existingId != null)
                        {
                            return Convert.ToInt64(existingId);
                        }
                    }

                    // Добавляем новую запись
                    string insertSql = @"INSERT INTO `log` (`Time`, `GroupName`, `Text`, `ImageIndex`) 
                                       VALUES (@Time, @GroupName, @Text, @ImageIndex);
                                       SELECT LAST_INSERT_ID();";

                    using (var insertCommand = new MySqlCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Time", record.Time);
                        insertCommand.Parameters.AddWithValue("@GroupName", record.GroupName);
                        insertCommand.Parameters.AddWithValue("@Text", record.Text);
                        insertCommand.Parameters.AddWithValue("@ImageIndex", record.ImageIndex);

                        result = Convert.ToInt64(await insertCommand.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения лога: {ex.Message}");
            }

            return result;
        }

        public async Task<List<TLogRecord>> LoadLogRecordsAsync(int limit = 100, DateTime? fromDate = null)
        {
            var records = new List<TLogRecord>();

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT `ID`, `Time`, `GroupName`, `Text`, `ImageIndex` 
                                 FROM `log` 
                                 WHERE (@FromDate IS NULL OR `Time` >= @FromDate)
                                 ORDER BY `Time` DESC 
                                 LIMIT @Limit";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", limit);
                        command.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                records.Add(new TLogRecord
                                {
                                    Time = reader.GetDateTime(1),
                                    GroupName = reader.GetString(2),
                                    Text = reader.GetString(3),
                                    ImageIndex = reader.GetInt16(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки логов: {ex.Message}");
            }

            return records;
        }
        // ============ ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ЖУРНАЛА ============

        public async Task<List<TLogRecord>> LoadLogRecordsByDateAsync(DateTime? fromDate, DateTime? toDate, int limit = 1000)
        {
            var records = new List<TLogRecord>();

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    // Строим динамический запрос
                    string sql = @"SELECT `ID`, `Time`, `GroupName`, `Text`, `ImageIndex` 
                         FROM `log` 
                         WHERE 1=1";

                    // Добавляем условия фильтрации по дате
                    if (fromDate.HasValue)
                    {
                        sql += " AND `Time` >= @FromDate";
                    }

                    if (toDate.HasValue)
                    {
                        sql += " AND `Time` <= @ToDate";
                    }

                    sql += " ORDER BY `Time` DESC LIMIT @Limit";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", limit);

                        if (fromDate.HasValue)
                            command.Parameters.AddWithValue("@FromDate", fromDate.Value);
                        else
                            command.Parameters.AddWithValue("@FromDate", DBNull.Value);

                        if (toDate.HasValue)
                            command.Parameters.AddWithValue("@ToDate", toDate.Value);
                        else
                            command.Parameters.AddWithValue("@ToDate", DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                records.Add(new TLogRecord
                                {
                                    // ID = reader.GetInt64(0), // Если нужно ID
                                    Time = reader.GetDateTime(1),
                                    GroupName = reader.GetString(2),
                                    Text = reader.GetString(3),
                                    ImageIndex = reader.GetInt16(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки логов по дате: {ex.Message}");
            }

            return records;
        }

        public async Task<List<TLogRecord>> LoadLogRecordsByPeriodAsync(string periodType, int limit = 1000)
        {
            DateTime? fromDate = null;
            var now = DateTime.Now;

            switch (periodType)
            {
                case "За сутки":
                    fromDate = now.AddDays(-1);
                    break;
                case "За неделю":
                    fromDate = now.AddDays(-7);
                    break;
                case "За месяц":
                    fromDate = now.AddDays(-30);
                    break;
                default:
                    fromDate = now.AddDays(-1); // По умолчанию сутки
                    break;
            }

            return await LoadLogRecordsByDateAsync(fromDate, now, limit);
        }

        // ============ ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ЭКСПОРТА ============

        public async Task<List<TLogRecord>> LoadAllLogRecordsForExportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var records = new List<TLogRecord>();

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT `Time`, `GroupName`, `Text`, `ImageIndex` 
                         FROM `log` 
                         WHERE 1=1";

                    if (fromDate.HasValue)
                    {
                        sql += " AND `Time` >= @FromDate";
                    }

                    if (toDate.HasValue)
                    {
                        sql += " AND `Time` <= @ToDate";
                    }

                    sql += " ORDER BY `Time` DESC";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        if (fromDate.HasValue)
                            command.Parameters.AddWithValue("@FromDate", fromDate.Value);

                        if (toDate.HasValue)
                            command.Parameters.AddWithValue("@ToDate", toDate.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                records.Add(new TLogRecord
                                {
                                    Time = reader.GetDateTime(0),
                                    GroupName = reader.GetString(1),
                                    Text = reader.GetString(2),
                                    ImageIndex = reader.GetInt16(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки логов для экспорта: {ex.Message}");
            }

            return records;
        }

        // ============ МЕТОДЫ ДЛЯ ТРЕНДОВ ============

        // Проверка существования таблиц трендов
        public async Task<bool> CheckTrendTablesExistAsync()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT COUNT(*) 
                                 FROM information_schema.TABLES 
                                 WHERE TABLE_SCHEMA = @Database 
                                 AND TABLE_NAME IN ('trends', 'trend_config')";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Database", DB_Name);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) == 2;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // Загрузка конфигурации трендов
        public async Task<List<TrendConfig>> LoadTrendConfigsAsync()
        {
            var configs = new List<TrendConfig>();

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT `TagID`, `Name`, `Description`, `Unit`, 
                                         `Period`, `MaxLength`, `MinValue`, `MaxValue`,
                                         `TrendType`, `Color`, `IsActive`
                                  FROM `trend_config`
                                  WHERE `IsActive` = TRUE
                                  ORDER BY `TagID`";

                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            configs.Add(new TrendConfig
                            {
                                TagID = reader.GetString(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Unit = reader.GetString(3),
                                Period = reader.GetInt16(4),
                                MaxLength = reader.GetInt32(5),
                                MinValue = reader.GetDouble(6),
                                MaxValue = reader.GetDouble(7),
                                TrendType = reader.GetString(8),
                                Color = reader.GetString(9),
                                IsActive = reader.GetBoolean(10)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки конфигурации трендов: {ex.Message}");
            }

            return configs;
        }

        // Сохранение конфигурации тренда
        public async Task<bool> SaveTrendConfigAsync(TrendConfig config)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"
                    INSERT INTO `trend_config` 
                    (`TagID`, `Name`, `Description`, `Unit`, `Period`, `MaxLength`, 
                     `MinValue`, `MaxValue`, `TrendType`, `Color`, `IsActive`)
                    VALUES (@TagID, @Name, @Description, @Unit, @Period, @MaxLength,
                            @MinValue, @MaxValue, @TrendType, @Color, @IsActive)
                    ON DUPLICATE KEY UPDATE
                        `Name` = VALUES(`Name`),
                        `Description` = VALUES(`Description`),
                        `Unit` = VALUES(`Unit`),
                        `Period` = VALUES(`Period`),
                        `MaxLength` = VALUES(`MaxLength`),
                        `MinValue` = VALUES(`MinValue`),
                        `MaxValue` = VALUES(`MaxValue`),
                        `TrendType` = VALUES(`TrendType`),
                        `Color` = VALUES(`Color`),
                        `IsActive` = VALUES(`IsActive`)";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TagID", config.TagID);
                        command.Parameters.AddWithValue("@Name", config.Name);
                        command.Parameters.AddWithValue("@Description", config.Description);
                        command.Parameters.AddWithValue("@Unit", config.Unit);
                        command.Parameters.AddWithValue("@Period", config.Period);
                        command.Parameters.AddWithValue("@MaxLength", config.MaxLength);
                        command.Parameters.AddWithValue("@MinValue", config.MinValue);
                        command.Parameters.AddWithValue("@MaxValue", config.MaxValue);
                        command.Parameters.AddWithValue("@TrendType", config.TrendType);
                        command.Parameters.AddWithValue("@Color", config.Color);
                        command.Parameters.AddWithValue("@IsActive", config.IsActive);

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения конфигурации тренда: {ex.Message}");
                return false;
            }
        }

        // Загрузка данных тренда
        public async Task<List<TrendDataPoint>> LoadTrendDataAsync(string tagId,
            DateTime fromDate, DateTime toDate, int maxPoints = 1000)
        {
            var points = new List<TrendDataPoint>();

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"
                    SELECT `DateTime`, `ValueReal`, `Quality`
                    FROM `trends` 
                    WHERE `TagID` = @TagID 
                    AND `DateTime` BETWEEN @FromDate AND @ToDate
                    ORDER BY `DateTime` ASC
                    LIMIT @MaxPoints";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TagID", tagId);
                        command.Parameters.AddWithValue("@FromDate", fromDate);
                        command.Parameters.AddWithValue("@ToDate", toDate);
                        command.Parameters.AddWithValue("@MaxPoints", maxPoints);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                points.Add(new TrendDataPoint
                                {
                                    DateTime = reader.GetDateTime(0),
                                    ValueReal = reader.GetDouble(1),
                                    Quality = reader.GetInt16(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных тренда: {ex.Message}");
            }

            return points;
        }

        // Сохранение точки тренда
        public async Task<long> SaveTrendPointAsync(string tagId, DateTime timestamp,
            double valueReal, int? valueInt = null, short quality = 192)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"
                    INSERT INTO `trends` (`TagID`, `DateTime`, `ValueReal`, `ValueInt`, `Quality`)
                    VALUES (@TagID, @DateTime, @ValueReal, @ValueInt, @Quality)
                    ON DUPLICATE KEY UPDATE 
                        `ValueReal` = VALUES(`ValueReal`),
                        `ValueInt` = VALUES(`ValueInt`),
                        `Quality` = VALUES(`Quality`)";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TagID", tagId);
                        command.Parameters.AddWithValue("@DateTime", timestamp);
                        command.Parameters.AddWithValue("@ValueReal", valueReal);
                        command.Parameters.AddWithValue("@ValueInt", valueInt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Quality", quality);

                        await command.ExecuteNonQueryAsync();

                        // Получаем ID
                        string getIdSql = "SELECT LAST_INSERT_ID()";
                        using (var getIdCmd = new MySqlCommand(getIdSql, connection))
                        {
                            var result = await getIdCmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                return Convert.ToInt64(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения точки тренда: {ex.Message}");
            }

            return -1;
        }
    }

    // ============ ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ============

    public class TrendConfig
    {
        public string TagID { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public short Period { get; set; } = 60;
        public int MaxLength { get; set; } = 1000;
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 100;
        public string TrendType { get; set; } = "analog";
        public string Color { get; set; } = "#2196F3";
        public bool IsActive { get; set; } = true;
    }

    public class TrendDataPoint
    {
        public string TagID { get; set; } = "";
        public DateTime DateTime { get; set; }
        public double ValueReal { get; set; }
        public int? ValueInt { get; set; }
        public short Quality { get; set; } = 192;
        public int PointsInGroup { get; set; }
    }
}