using MySql.Data.MySqlClient;
using ProtolScadaRemake;
using System;
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

        public async Task<long> SaveLogRecordAsync(TLogRecord record)
        {
            long result = -1;

            try
            {
                using (var connection = GetDBConnection(DB_HostName, DB_Port, DB_Name, DB_UserLogin, DB_Password))
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
                // Логируем ошибку, но не прерываем работу приложения
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения лога: {ex.Message}");
            }

            return result;
        }

        public async Task<List<TLogRecord>> LoadLogRecordsAsync(int limit = 100, DateTime? fromDate = null)
        {
            var records = new List<TLogRecord>();

            try
            {
                using (var connection = GetDBConnection(DB_HostName, DB_Port, DB_Name, DB_UserLogin, DB_Password))
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
    }
}