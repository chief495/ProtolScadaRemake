using ProtolScada;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
        public TrendDbManager TrendDb { get; private set; }

        public TGlobal()
        {
            Log = new TLogList();
            Trends = new TTrendList();
            TrendDb = new TrendDbManager(this);

            InitTestTrends();
        }

        private void InitTestTrends()
        {
            Trends.Add("Temp1", "Температура бак 1", "°C", 10, 1000);
            Trends.Add("Temp2", "Температура бак 2", "°C", 10, 1000);
            Trends.Add("Pressure1", "Давление", "бар", 5, 2000);
            Trends.Add("Level1", "Уровень", "%", 30, 1000);

            Trends.Add("Pump1_Freq", "Частота насоса 1", "Гц", 1, 5000);
            Trends.Add("Pump1_Curr", "Ток насоса 1", "А", 2, 5000);
            Trends.Add("Pump2_Freq", "Частота насоса 2", "Гц", 1, 5000);
            Trends.Add("Pump2_Temp", "Температура насоса 2", "°C", 10, 2000);
        }

        public async Task UpdateTrendsAsync()
        {
            foreach (var trend in Trends.Items)
            {
                var variable = Variables.GetByName(trend.Name);
                if (variable != null)
                {
                    // Используйте синхронный метод Update вместо UpdateAsync
                    // или реализуйте UpdateAsync в классе TTrendTag
                    trend.Update(variable); // Измените на синхронный вызов
                }
            }
            await Task.CompletedTask; // Чтобы соответствовать сигнатуре async
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

        // ============== СТАТИЧЕСКИЕ МЕТОДЫ ДЛЯ РАБОТЫ С ПОТОКАМИ ==============
        // Эти методы необходимы для TFaultRecord.cs

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
            UInt16 Len = Convert.ToUInt16(Value.Length);
            // Запись длины
            byte[] intBytes = BitConverter.GetBytes(Len);
            Stream.WriteByte(Convert.ToByte(intBytes[0]));
            Stream.WriteByte(Convert.ToByte(intBytes[1]));
            // Запись строки
            for (int i = 0; i < Len; i++)
            {
                int B = Value[i];
                byte[] intBytes2 = BitConverter.GetBytes(B);
                Stream.Write(intBytes2, 0, 2);
            }
        }

        public static String LoadStringFromStream(FileStream Stream)
        {
            string Result = "";
            // Чтение длины
            UInt16 Len = 0;
            byte[] Data = { 0, 0 };
            UInt16[] Data2 = { 0, 0 };
            Stream.Read(Data, 0, 2);
            for (int i = 1; i >= 0; i--)
            {
                Data2[i] = Data[i];
                Len = Convert.ToUInt16((Len * 256) + Data2[i]);
            }
            // Чтение строки
            for (int i = 0; i < Len; i++)
            {
                byte[] B = { 0, 0, 0, 0 };
                Stream.Read(B, 0, 2);
                char C = BitConverter.ToChar(B, 0);
                if (C != 0x00) Result = Result + C;
            }
            return Result;
        }

        public static void SaveDoubleToStream(FileStream Stream, double Value)
        {
            byte[] intBytes = BitConverter.GetBytes(Value);
            if (intBytes.Length > 0) Stream.Write(intBytes, 0, intBytes.Length);
        }

        public static double LoadDoubleFromStream(FileStream Stream)
        {
            double D = 0;
            byte[] Bytes = BitConverter.GetBytes(D);
            Stream.Read(Bytes, 0, Bytes.Length);
            D = BitConverter.ToDouble(Bytes, 0);
            return D;
        }

        public static void SaveDateTimeToStream(FileStream Stream, DateTime Value)
        {
            Int64 Ost = Value.ToBinary();
            byte[] intBytes = BitConverter.GetBytes(Ost);
            for (int i = 0; i < 8; i++)
            {
                Stream.WriteByte(Convert.ToByte(intBytes[i]));
            }
        }

        public static DateTime LoadDateTimeFromStream(FileStream Stream)
        {
            DateTime Result = DateTime.Now;
            Int64 Binary = 0;
            byte[] Data = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Int64[] Data2 = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Stream.Read(Data, 0, 8);
            for (int i = 7; i >= 0; i--)
            {
                Data2[i] = Data[i];
                Binary = (Binary * 256) + Data2[i];
            }
            Result = DateTime.Now;
            try
            {
                Result = DateTime.FromBinary(Binary);
            }
            catch { }
            return Result;
        }

        public static void SaveIntToStream(FileStream Stream, int Value)
        {
            byte[] intBytes = BitConverter.GetBytes(Value);
            if (intBytes.Length > 0) Stream.Write(intBytes, 0, intBytes.Length);
        }

        public static int LoadIntFromStream(FileStream Stream)
        {
            int I = 0;
            byte[] Bytes = BitConverter.GetBytes(I);
            Stream.Read(Bytes, 0, Bytes.Length);
            I = BitConverter.ToInt32(Bytes, 0);
            return I;
        }

        public static void SaveBoolToStream(FileStream Stream, bool Value)
        {
            byte B = 0;
            if (Value) B = 1;
            Stream.WriteByte(B);
        }

        public static bool LoadBoolFromStream(FileStream Stream)
        {
            int B = Stream.ReadByte();
            bool Result = false;
            if (B > 0) Result = true;
            return Result;
        }

        public static void SaveInt64ToStream(FileStream Stream, Int64 Value)
        {
            byte[] intBytes = BitConverter.GetBytes(Value);
            if (intBytes.Length > 0) Stream.Write(intBytes, 0, intBytes.Length);
        }

        public static Int64 LoadInt64FromStream(FileStream Stream)
        {
            Int64 Result = 0;
            byte[] Bytes = BitConverter.GetBytes(Result);
            Stream.Read(Bytes, 0, Bytes.Length);
            Result = BitConverter.ToInt64(Bytes, 0);
            return Result;
        }
    }
}