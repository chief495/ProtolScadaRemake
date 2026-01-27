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

        private ModbusManager _modbusManager;

        public TCommandTag() { }

        public void SetModbusManager(ModbusManager manager)
        {
            _modbusManager = manager;
        }

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
            if (NeedToWrite && _modbusManager != null)
            {
                try
                {
                    ushort value = 0;

                    switch (Type)
                    {
                        case "Bool":
                            value = (ushort)(WriteValue.ToLower() == "true" ? 1 : 0);
                            break;
                        case "Int_16":
                            value = ushort.Parse(WriteValue);
                            break;
                        case "Float_32":
                            // Для Float нужно 2 регистра
                            float floatValue = float.Parse(WriteValue);
                            byte[] bytes = BitConverter.GetBytes(floatValue);
                            ushort[] registers = new ushort[2];
                            registers[0] = (ushort)((bytes[1] << 8) | bytes[0]);
                            registers[1] = (ushort)((bytes[3] << 8) | bytes[2]);

                            // Отправляем оба регистра
                            for (int i = 0; i < 2; i++)
                            {
                                _modbusManager.WriteToRegister(
                                    (byte)Plc_DeviceAddress,
                                    (ushort)(Address + i),
                                    registers[i],
                                    $"{Description} (часть {i + 1})");
                            }
                            NeedToWrite = false;
                            return;
                    }

                    // Отправляем команду через ModbusManager
                    bool success = _modbusManager.WriteToRegister(
                        (byte)Plc_DeviceAddress,
                        (ushort)Address,
                        value,
                        Description);

                    if (success)
                    {
                        NeedToWrite = false;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды {Name}: {ex.Message}");
                }
            }
        }
    }
}