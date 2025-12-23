using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TBitsArea
    {
        public bool[] Data; // Данные
        public UInt16 FaultsCount = 0; // Количество ошибок связи
        public UInt16 FaultsCountEvent = 100; // Количество ошибок связи, при котором производится запись ошибки чтения в журнал событий

        private string fIpAddress = ""; // IP-адрес
        private int fPortNum = 0; // Номер порта
        private int fDeviceAddress = 0; // Адрес устройства
        private ushort fStartAddress = 0; // Адрес начала области
        private ushort fLength = 0; // Размер области
        private int Timeout = 300; // Таймоут перезачи данных

        public TBitsArea() // Конструктор простой
        {
            Data = new bool[0];
            FaultsCount = 0;
        }

        public TBitsArea(string IpAddress, int PortNum, int DeviceAddress, ushort StartAddress, ushort Length) // Конструктор Modbus TCP
        {
            Data = new bool[Length];
            fIpAddress = IpAddress;
            fPortNum = PortNum;
            fDeviceAddress = DeviceAddress;
            fStartAddress = StartAddress;
            fLength = Length;
            FaultsCount = 0;
        }

        // ИЗМЕНИТЕ ЭТОТ МЕТОД: LogClasses -> TLogList
        public UInt16 GetModbusTcpCoils(TLogList Log) // Чтение области Coils из Modbus TCP устройства
        {
            bool[] Result = new bool[0];
            try
            {
                TcpClient client;
                client = new TcpClient();
                client.ReceiveTimeout = Timeout;
                client.Connect(fIpAddress, fPortNum);
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);
                Result = master.ReadCoils(Convert.ToByte(fDeviceAddress), fStartAddress, fLength);
                FaultsCount = 0;
                Data = Result;
                if (Timeout > 300) Timeout = Timeout - 100;
            }
            catch
            {
                if (FaultsCount < (65535)) FaultsCount++;
                if (Timeout < 5000) Timeout = Timeout + 300;
                if (FaultsCount == FaultsCountEvent)
                    if (Log != null)
                    {
                        Log.Add("Связь",
                            "Не удалось считать область Coils у устройства " +
                            fIpAddress + ":" + fPortNum.ToString() + "(" + fDeviceAddress.ToString() +
                            ") по адресу " + fStartAddress.ToString("x4") + "H длиной " +
                            fLength.ToString() + " бит(а)", 3);
                    }
                Thread.Sleep(500);
            }
            Thread.Sleep(300);
            return FaultsCount;
        }
    }
}