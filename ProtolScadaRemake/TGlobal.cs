using ProtolScada;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TGlobal
    {
        // Настройки подключения к базе данных
        public string DB_HostName = "localhost";
        public int DB_Port = 3306;
        public string DB_UserLogin = "root";
        public string DB_Password = "advengauser";
        public string DB_Name = "protolscadadb";
        public string Password = "Protol251121";
        public bool Access = false;
        public DateTime PassTime;

        // Настройка параметров опрашиваемого оборудования
        public string Plc_IpAddress = "192.168.100.5";
        public int Plc_PortNum = 502;
        public int Plc_DeviceAddress = 1;

        // Объекты данных
        public TVariableList Variables = new TVariableList();
        public TCommandList Commands = new TCommandList();
        public TFaultList Faults = new TFaultList();

        // Только одно объявление Trends
        public TLogList Log { get; private set; }
        public TTrendList Trends { get; private set; }

        // Добавьте новое свойство:
        public DBUtils TrendDb { get; private set; }

        // Добавляем поле _dbUtils
        private DBUtils _dbUtils;

        // ДВА конструктора для разных случаев использования:
        public TGlobal()
        {
            Log = new TLogList(_dbUtils);
            Trends = new TTrendList();
        }

        public TGlobal(DBUtils dbUtils)
        {
            _dbUtils = dbUtils ?? throw new ArgumentNullException(nameof(dbUtils));
            Log = new TLogList(_dbUtils);
            Trends = new TTrendList();
        }

        // Метод для обновления настроек БД
        public void UpdateDatabaseSettings(string host, int port, string database, string user, string password)
        {
            DB_HostName = host;
            DB_Port = port;
            DB_Name = database;
            DB_UserLogin = user;
            DB_Password = password;

            if (_dbUtils != null)
            {
                _dbUtils.DB_HostName = host;
                _dbUtils.DB_Port = port;
                _dbUtils.DB_Name = database;
                _dbUtils.DB_UserLogin = user;
                _dbUtils.DB_Password = password;
            }
        }

        public void SetDatabaseUtils(DBUtils dbUtils)
        {
            _dbUtils = dbUtils ?? throw new ArgumentNullException(nameof(dbUtils));
            Log = new TLogList(_dbUtils);
        }

        public async Task UpdateTrendsAsync()
        {
            foreach (var trend in Trends.Items)
            {
                var variable = Variables.GetByName(trend.Name);
                if (variable != null)
                {
                    trend.Update(variable);
                }
            }
            await Task.CompletedTask;
        }

        public void UpdateFaults()
        {
            if (Faults.GetCount() > 0 && Variables.GetCount() > 0)
            {
                for (int FaultIndex = 0; FaultIndex < Faults.GetCount(); FaultIndex++)
                {
                    for (int VariableIndex = 0; VariableIndex < Variables.GetCount(); VariableIndex++)
                    {
                        if (Faults.Items[FaultIndex].Name == Variables.Items[VariableIndex].Name)
                        {
                            Faults.Items[FaultIndex].Update(Variables.Items[VariableIndex], Log);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            Variables.Clear();
            Commands.Clear();
            Log.Clear();
            Faults.Clear();
            Trends.Clear();
        }

        public void UpdateTrends()
        {
            Trends.Update(Variables);
        }

        // ============== ВСЕ СТАТИЧЕСКИЕ МЕТОДЫ ДЛЯ РАБОТЫ С ПОТОКАМИ ==============

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
                // Сохраняем длину 0
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