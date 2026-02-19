using NModbus;
using System.IO;
using System.Net.Sockets;

namespace ProtolScadaRemake
{
    public class TCommandTag
    {
        public string Name = "";
        public string Description = "";
        public string Plc_IpAddress = "";
        public int Plc_PortNum = 0;
        public int Plc_DeviceAddress = 0;
        public string AreaType = "";
        public int Address = 0;
        public string Type = "";
        public string Format = "";
        public bool NeedToWrite = false;
        public string WriteValue = "";

        public TCommandTag() { }

        public void SetLocate(int NewAddress)
        {
            Address = NewAddress;
        }

        public void SaveToStream(FileStream Stream)
        {
            TGlobal.SaveStringToStream(Stream, Name);
            TGlobal.SaveStringToStream(Stream, Description);
            TGlobal.SaveStringToStream(Stream, Plc_IpAddress);
            TGlobal.SaveIntToStream(Stream, Plc_PortNum);
            TGlobal.SaveIntToStream(Stream, Plc_DeviceAddress);
            TGlobal.SaveStringToStream(Stream, AreaType);
            TGlobal.SaveIntToStream(Stream, Address);
            TGlobal.SaveStringToStream(Stream, Type);
            TGlobal.SaveStringToStream(Stream, Format);
            TGlobal.SaveStringToStream(Stream, WriteValue);
        }

        public bool LoadFromStream(FileStream Stream)
        {
            bool Result = true;
            Name = TGlobal.LoadStringFromStream(Stream);
            Description = TGlobal.LoadStringFromStream(Stream);
            Plc_IpAddress = TGlobal.LoadStringFromStream(Stream);
            Plc_PortNum = TGlobal.LoadIntFromStream(Stream);
            Plc_DeviceAddress = TGlobal.LoadIntFromStream(Stream);
            AreaType = TGlobal.LoadStringFromStream(Stream);
            Address = TGlobal.LoadIntFromStream(Stream);
            Type = TGlobal.LoadStringFromStream(Stream);
            Format = TGlobal.LoadStringFromStream(Stream);
            WriteValue = TGlobal.LoadStringFromStream(Stream);
            NeedToWrite = false;
            if (WriteValue != "") NeedToWrite = true;
            if (Stream.Position >= Stream.Length - 1) Result = false;
            return Result;
        }

        public void SendToController()
        {
            TcpClient client;
            bool[] BB = new bool[1];
            if (NeedToWrite)
            {
                switch (Type)
                {
                    case "Bool":
                        try
                        {
                            ushort[] W = new ushort[1];
                            W[0] = 0;
                            if (WriteValue == "true") W[0] = 1;
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
                            BitConverter.GetBytes(D).CopyTo(HR, 0);
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