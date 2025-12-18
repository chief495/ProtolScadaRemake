using NModbus;
using System.Net.Sockets;
using System.IO;

namespace ProtolScadaRemake
{
    public class TCommandTag
    {
        public string Name = ""; // Имя тега
        public string Description = ""; // Описание тега
        public string Plc_IpAddress = ""; // IP-адрес контроллера
        public int Plc_PortNum = 0; // Порт контроллера
        public int Plc_DeviceAddress = 0; // Адрес устройства в сети типа Modbus 
        public string AreaType = ""; // Тип области памяти
        public int Address = 0; // Адрес в области
        public string Type = ""; // Тип данных
        public string Format = ""; // Формат
        public bool NeedToWrite = false; // Требуется отправка команды
        public string WriteValue = ""; 
        public TCommandTag() // Конструктор
        {
        }
        public void SetLocate(int NewAddress)
        {
            Address = NewAddress;
        }
        public void SaveToStream(FileStream Stream) // Сохраняем данные в поток
        {
            TGlobal.SaveStringToStream(Stream, Name); // Имя тега
            TGlobal.SaveStringToStream(Stream, Description); // Описание тега
            TGlobal.SaveStringToStream(Stream, Plc_IpAddress); // IP-адрес контроллера
            TGlobal.SaveIntToStream(Stream, Plc_PortNum); // Порт контроллера
            TGlobal.SaveIntToStream(Stream, Plc_DeviceAddress); // Адрес устройства в сети типа Modbus 
            TGlobal.SaveStringToStream(Stream, AreaType); // Тип области памяти
            TGlobal.SaveIntToStream(Stream, Address); // Адрес в области
            TGlobal.SaveStringToStream(Stream, Type); // Тип данных
            TGlobal.SaveStringToStream(Stream, Format); // Формат
            TGlobal.SaveStringToStream(Stream, WriteValue); // Значение для записи
        }
        public bool LoadFromStream(FileStream Stream) // Чтение данных из потока
        {
            bool Result = true;
            Name = TGlobal.LoadStringFromStream(Stream); // Имя тега
            Description = TGlobal.LoadStringFromStream(Stream); // Описание тега
            Plc_IpAddress = TGlobal.LoadStringFromStream(Stream); // IP-адрес контроллера
            Plc_PortNum = TGlobal.LoadIntFromStream(Stream); // Порт контроллера
            Plc_DeviceAddress = TGlobal.LoadIntFromStream(Stream); // Адрес устройства в сети типа Modbus
            AreaType = TGlobal.LoadStringFromStream(Stream); // Тип области памяти
            Address = TGlobal.LoadIntFromStream(Stream); // Адрес в области
            Type = TGlobal.LoadStringFromStream(Stream);  // Тип данных
            Format = TGlobal.LoadStringFromStream(Stream);  // Формат
            WriteValue = TGlobal.LoadStringFromStream(Stream); // Значение для записи
            NeedToWrite = false;
            if(WriteValue != "") NeedToWrite = true;
            if (Stream.Position >= Stream.Length - 1) Result = false; // Защита от неожиданного окончания файла
            return Result;
        }
        public void SendToController()
        {
            TcpClient client;
            bool[] BB = new bool[1];
            if (NeedToWrite)
            {
                switch(Type)
                {
                    case "Bool":
                        try
                        {
                            ushort[] W = new ushort[1];
                            W[0]= 0;
                            if(WriteValue == "true") W[0] = 1;
                            client = new TcpClient();
                            client.Connect(Plc_IpAddress, Plc_PortNum);
                            var factory = new ModbusFactory();
                            IModbusMaster master = factory.CreateMaster(client);
                            master.WriteMultipleRegisters(Convert.ToByte(Plc_DeviceAddress), Convert.ToUInt16(Address), W);
                            client.Close();
                            NeedToWrite = false;
                        }
                        catch { }
                        break;
                    case "Float_32":
                        try
                        {
                            float D = Convert.ToSingle(WriteValue);
                            byte[] HR = new byte[8];
                            ushort[] HR2 = new ushort[2];
                            BitConverter.GetBytes(D).CopyTo(HR,0);
                            HR2[0] = Convert.ToUInt16(HR[1] * 256 + HR[0]);
                            HR2[1] = Convert.ToUInt16(HR[3] * 256 + HR[2]);
                            client = new TcpClient();
                            client.Connect(Plc_IpAddress, Plc_PortNum);
                            var factory = new ModbusFactory();
                            IModbusMaster master = factory.CreateMaster(client);
                            master.WriteMultipleRegisters(Convert.ToByte(Plc_DeviceAddress), Convert.ToUInt16(Address), HR2);
                            client.Close();
                            NeedToWrite = false;
                        }
                        catch { }
                        break;
                    case "Int_16":
                        try
                        {
                            Int16 I16 = Convert.ToInt16(WriteValue);
                            client = new TcpClient();
                            client.Connect(Plc_IpAddress, Plc_PortNum);
                            var factory = new ModbusFactory();
                            IModbusMaster master = factory.CreateMaster(client);
                            master.WriteSingleRegister(Convert.ToByte(Plc_DeviceAddress), Convert.ToUInt16(Address), (ushort)I16);
                            client.Close();
                            NeedToWrite = false;
                        }
                        catch { }
                        break;



                }
            }


        }

    }
}
