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
    public class TWordsArea
    {
        public UInt16[] Data; // Данные
        public UInt16 FaultsCount = 0; // Количество ошибок связи
        public UInt16 FaultsCountEvent = 10; // Количество ошибок связи, при котором производится запись ошибки чтения в журнал событий

        private string fIpAddress = ""; // IP-адрес
        private int fPortNum = 0; // Номер порта
        private int fDeviceAddress = 0; // Адрес устройства
        private ushort fStartAddress = 0; // Адрес начала области
        private ushort fLength = 0; // Размер области
        private int Timeout = 300; // Таймоут перезачи данных

        public TWordsArea() // Конструктор простой
        {
            Data = new UInt16[0];
            FaultsCount = 0;
        }

        public TWordsArea(string IpAddress, int PortNum, int DeviceAddress, ushort StartAddress, ushort Length) // Конструктор Modbus TCP
        {
            Data = new UInt16[Length];
            fIpAddress = IpAddress;
            fPortNum = PortNum;
            fDeviceAddress = DeviceAddress;
            fStartAddress = StartAddress;
            fLength = Length;
            FaultsCount = 0;
        }

        public UInt16 GetModbusTcpHoldingRegisters(TLogList Log) // Изменили LogClasses на TLogList
        {
            UInt16[] Result = new UInt16[0];
            try
            {
                TcpClient client;
                client = new TcpClient();
                client.ReceiveTimeout = Timeout;

                client.Connect(fIpAddress, fPortNum);
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);
                Result = master.ReadHoldingRegisters(Convert.ToByte(fDeviceAddress), fStartAddress, fLength);
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
                            "Не удалось считать область Holding Registers у устройства " +
                            fIpAddress + ":" + fPortNum.ToString() + "(" + fDeviceAddress.ToString() +
                            ") по адресу " + fStartAddress.ToString("x4") + "H длиной " +
                            fLength.ToString() + " бит(а)", 3);
                    }
                Thread.Sleep(500);
            }
            Thread.Sleep(10);
            return FaultsCount;
        }

        public UInt16 GetModbusTcpInputRegisters(TLogList Log) // Изменили TLogClasses на TLogList
        {
            UInt16[] Result = new UInt16[0];
            try
            {
                TcpClient client;
                client = new TcpClient();
                client.ReceiveTimeout = Timeout;

                client.Connect(fIpAddress, fPortNum);
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);
                Result = master.ReadInputRegisters(Convert.ToByte(fDeviceAddress), fStartAddress, fLength);
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
                            "Не удалось считать область Input Registers у устройства " +
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