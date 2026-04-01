using ProtolScada;
using System.IO;

namespace ProtolScadaRemake
{
    public class TGlobal
    {
        // Настройки подключения к базе данных
        public string DB_HostName = "localhost";
        public int DB_Port = 3306;
        public string DB_UserLogin = "root";
        public string DB_Password = "";
        public string DB_Name = "protolscadadb";

        // Настройки безопасности
        public string Password = "Protol251121";
        public bool Access = false;
        public DateTime PassTime;

        // Настройки Modbus
        public string Plc_IpAddress = "192.168.88.139";
        public int Plc_PortNum = 502;
        public int Plc_DeviceAddress = 1;

        // Объекты данных
        public TVariableList Variables { get; private set; }
        public TCommandList Commands { get; private set; }
        public TFaultList Faults { get; private set; }
        public TLogList Log { get; private set; }
        public TTrendList Trends { get; private set; }

        // Менеджеры
        private DBUtils _dbUtils;
        private DatabaseTrendManager _trendManager;
        private System.Timers.Timer _updateTimer;

        // События
        public event EventHandler<string> OnModbusStatusChanged;
        public event EventHandler<bool> OnModbusConnectionChanged;
        public event EventHandler<string> OnCommandExecuted;
        public event EventHandler OnVariablesUpdated;

        // Конструктор
        public TGlobal()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Создаем списки
            Variables = new TVariableList();
            Commands = new TCommandList();
            Faults = new TFaultList();

            // Инициализируем DBUtils
            _dbUtils = new DBUtils
            {
                DB_HostName = DB_HostName,
                DB_Port = DB_Port,
                DB_Name = DB_Name,
                DB_UserLogin = DB_UserLogin,
                DB_Password = DB_Password
            };

            // Создаем Log с DBUtils
            Log = new TLogList(_dbUtils);

            // Создаем тренды
            Trends = new TTrendList();
            _trendManager = new DatabaseTrendManager(_dbUtils);

            // Инициализируем таймер обновления
            InitializeUpdateTimer();

            // Инициализируем тренды из БД в фоне
            _ = InitializeTrendsFromDatabaseAsync();
        }

        private void InitializeUpdateTimer()
        {
            _updateTimer = new System.Timers.Timer(10.0); // 100ms = 10Hz (используем double)
            _updateTimer.Elapsed += async (sender, e) =>
            {
                try
                {
                    await UpdateAllAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка в таймере обновления: {ex.Message}");
                }
            };
            _updateTimer.AutoReset = true;
            _updateTimer.Enabled = false;
        }

        private async Task InitializeTrendsFromDatabaseAsync()
        {
            try
            {
                await _trendManager.InitializeAsync();

                if (_trendManager.IsInitialized && _trendManager.TrendCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Загружено {_trendManager.TrendCount} трендов из БД");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации трендов из БД: {ex.Message}");
            }
        }

        public void StartUpdateTimer()
        {
            if (_updateTimer != null && !_updateTimer.Enabled)
            {
                _updateTimer.Enabled = true;
            }
        }

        public void StopUpdateTimer()
        {
            if (_updateTimer != null && _updateTimer.Enabled)
            {
                _updateTimer.Enabled = false;
            }
        }

        public async Task UpdateAllAsync()
        {
            try
            {
                // Обновляем аварии
                UpdateFaults();

                // Обновляем тренды
                await UpdateTrendsWithSaveAsync();

                // Генерируем событие обновления переменных
                OnVariablesUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateAllAsync: {ex.Message}");
            }
        }

        private void UpdateVariablesFromModbus(ushort address, ushort value)
        {
            // Находим переменную по адресу регистра
            var variable = Variables.Items.FirstOrDefault(v => v.Address == address);

            if (variable != null)
            {
                // Обновляем значение переменной
                variable.ValueReal = value;
                variable.LastRead = DateTime.Now;

                // Преобразуем в строковое значение
                switch (variable.Type)
                {
                    case "Bool":
                        variable.ValueString = value > 0 ? "true" : "false";
                        break;
                    case "Int_16":
                        variable.ValueString = (value * variable.Multiplier).ToString(variable.Format);
                        break;
                    case "Float_32":
                        // Для Float нужно 2 регистра, обрабатывается отдельно
                        break;
                    default:
                        variable.ValueString = value.ToString();
                        break;
                }
            }
        }

        public void UpdateFaults()
        {
            if (Faults.GetCount() > 0 && Variables.GetCount() > 0)
            {
                for (int faultIndex = 0; faultIndex < Faults.GetCount(); faultIndex++)
                {
                    for (int variableIndex = 0; variableIndex < Variables.GetCount(); variableIndex++)
                    {
                        if (Faults.Items[faultIndex].Name == Variables.Items[variableIndex].Name)
                        {
                            Faults.Items[faultIndex].Update(Variables.Items[variableIndex], Log);
                        }
                    }
                }
            }
        }

        public async Task UpdateTrendsWithSaveAsync()
        {
            if (_trendManager.IsInitialized)
            {
                await _trendManager.UpdateAllTrendsAsync(Variables);
            }
            else
            {
                // Используем старый метод если БД не инициализирована
                Trends.Update(Variables);
            }
        }

        // Утилитарные методы
        public DBUtils GetDbUtils()
        {
            return _dbUtils;
        }

        public DatabaseTrendManager GetTrendManager()
        {
            return _trendManager;
        }

        public void DisconnectAll()
        {
            StopUpdateTimer();
        }

        public void Clear()
        {
            Variables.Clear();
            Commands.Clear();
            Log.Clear();
            Faults.Clear();
            Trends.Clear();

            if (_trendManager != null)
            {
                try
                {
                    // Проверяем, есть ли метод Clear у DatabaseTrendManager
                    var clearMethod = _trendManager.GetType().GetMethod("Clear");
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(_trendManager, null);
                    }
                    else
                    {
                        // Если метода нет, создаем новый менеджер
                        _trendManager = new DatabaseTrendManager(_dbUtils);
                    }
                }
                catch
                {
                    // В случае ошибки создаем новый менеджер
                    _trendManager = new DatabaseTrendManager(_dbUtils);
                }
            }
        }

        // Метод для отправки команды 
        public void SendCommand(string commandName, string value)
        {
            var command = Commands.GetByName(commandName);
            if (command != null)
            {
                command.WriteValue = value;
                command.NeedToWrite = true;
                command.SendToController();
            }
        }

        // Статические методы для работы с потоками
        public static void SaveUInt32ToStream(FileStream Stream, UInt32 Variable)
        {
            UInt32 Ost = Variable;
            for (int i = 0; i < 4; i++)
            {
                uint B0 = Convert.ToUInt32(Ost - (Math.Abs(Ost / 256) * 256));
                if (B0 > 255) B0 = 255;
                Ost = (Ost - B0) / 256;
                Stream.WriteByte(Convert.ToByte(B0));
            }
        }

        public static UInt32 LoadUInt32FromStream(FileStream Stream)
        {
            UInt32 Value = 0;
            byte[] Data = { 0, 0, 0, 0 };
            UInt32[] Data2 = { 0, 0, 0, 0 };
            Stream.Read(Data, 0, 4);
            for (int i = 0; i < 4; i++) Data2[i] = Data[i];
            Value = (Data2[3] * 16777216) + (Data2[2] * 65536) + (Data2[1] * 256) + Data2[0];
            return Value;
        }

        public static void SaveStringToStream(FileStream Stream, string Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                Stream.WriteByte(0);
                Stream.WriteByte(0);
                return;
            }

            UInt16 Len = Convert.ToUInt16(Value.Length);
            byte[] lenBytes = BitConverter.GetBytes(Len);
            Stream.Write(lenBytes, 0, 2);

            foreach (char c in Value)
            {
                byte[] charBytes = BitConverter.GetBytes(c);
                Stream.Write(charBytes, 0, 2);
            }
        }

        public static String LoadStringFromStream(FileStream Stream)
        {
            byte[] lenBytes = new byte[2];
            if (Stream.Read(lenBytes, 0, 2) != 2)
                return string.Empty;

            UInt16 len = BitConverter.ToUInt16(lenBytes, 0);
            if (len == 0) return string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                byte[] charBytes = new byte[2];
                if (Stream.Read(charBytes, 0, 2) != 2)
                    break;

                char c = BitConverter.ToChar(charBytes, 0);
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static void SaveDoubleToStream(FileStream Stream, double Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static double LoadDoubleFromStream(FileStream Stream)
        {
            byte[] bytes = new byte[8];
            Stream.Read(bytes, 0, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static void SaveDateTimeToStream(FileStream Stream, DateTime Value)
        {
            long binary = Value.ToBinary();
            byte[] bytes = BitConverter.GetBytes(binary);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static DateTime LoadDateTimeFromStream(FileStream Stream)
        {
            byte[] bytes = new byte[8];
            Stream.Read(bytes, 0, 8);
            long binary = BitConverter.ToInt64(bytes, 0);
            return DateTime.FromBinary(binary);
        }

        public static void SaveIntToStream(FileStream Stream, int Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static int LoadIntFromStream(FileStream Stream)
        {
            byte[] bytes = new byte[4];
            Stream.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static void SaveBoolToStream(FileStream Stream, bool Value)
        {
            byte b = Value ? (byte)1 : (byte)0;
            Stream.WriteByte(b);
        }

        public static bool LoadBoolFromStream(FileStream Stream)
        {
            int b = Stream.ReadByte();
            return b > 0;
        }

        public static void SaveInt64ToStream(FileStream Stream, Int64 Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static Int64 LoadInt64FromStream(FileStream Stream)
        {
            byte[] bytes = new byte[8];
            Stream.Read(bytes, 0, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static void SaveShortToStream(FileStream Stream, short Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static short LoadShortFromStream(FileStream Stream)
        {
            byte[] bytes = new byte[2];
            Stream.Read(bytes, 0, 2);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static void SaveByteToStream(FileStream Stream, byte Value)
        {
            Stream.WriteByte(Value);
        }

        public static byte LoadByteFromStream(FileStream Stream)
        {
            return (byte)Stream.ReadByte();
        }

        public static void SaveBytesToStream(FileStream Stream, byte[] Value)
        {
            SaveIntToStream(Stream, Value.Length);
            Stream.Write(Value, 0, Value.Length);
        }

        public static byte[] LoadBytesFromStream(FileStream Stream)
        {
            int length = LoadIntFromStream(Stream);
            byte[] bytes = new byte[length];
            Stream.Read(bytes, 0, length);
            return bytes;
        }

        public static void SaveFloatToStream(FileStream Stream, float Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public static float LoadFloatFromStream(FileStream Stream)
        {
            byte[] bytes = new byte[4];
            Stream.Read(bytes, 0, 4);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}