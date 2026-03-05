using NModbus;
using System.Diagnostics;
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

        // Таймаут подключения
        private const int CONNECTION_TIMEOUT_MS = 3000;

        // Событие завершения команды (для UI)
        public event Action<string, bool, string> OnCommandCompleted;

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

        /// <summary>
        /// Отправка команды в ФОНОВОМ потоке (не блокирует UI)
        /// </summary>
        public void SendToControllerInBackground()
        {
            if (!NeedToWrite) return;

            string type = Type;
            string value = WriteValue;
            string ip = Plc_IpAddress;
            int port = Plc_PortNum;
            int deviceAddr = Plc_DeviceAddress;
            int address = Address;
            string name = Name;

            // ДИАГНОСТИКА: показываем что отправляем
            Debug.WriteLine($"→→→ [{name}] Отправка: {value} (Type={type}, Addr={address}, IP={ip}:{port})");

            Task.Run(() =>
            {
                bool success = false;
                string errorMessage = "";

                try
                {
                    success = SendToControllerInternal(type, value, ip, port, deviceAddr, address);

                    if (success)
                    {
                        NeedToWrite = false;
                        Debug.WriteLine($"✓ [{name}] УСПЕХ");
                    }
                    else
                    {
                        Debug.WriteLine($"✗ [{name}] НЕ УДАЛОСЬ (без исключения)");
                    }
                }
                catch (IOException ioEx)
                {
                    errorMessage = $"IO: {ioEx.Message}";
                    Debug.WriteLine($"✗ [{name}] IO EXCEPTION: Контроллер не отвечает!");
                }
                catch (SocketException sockEx)
                {
                    errorMessage = $"Socket: {sockEx.Message}";
                    Debug.WriteLine($"✗ [{name}] SOCKET EXCEPTION: {sockEx.SocketErrorCode}");
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Debug.WriteLine($"✗ [{name}] EXCEPTION: {ex.Message}");
                }

                OnCommandCompleted?.Invoke(name, success, errorMessage);
            });
        }
        /// <summary>
        /// Синхронная отправка (для вызова из фонового потока)
        /// </summary>
        private bool SendToControllerInternal(
            string type, string value, string ip, int port, int deviceAddr, int address)
        {
            TcpClient client = null;

            try
            {
                client = new TcpClient();

                // Подключение с таймаутом
                var result = client.BeginConnect(ip, port, null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(CONNECTION_TIMEOUT_MS);

                if (!connected)
                {
                    throw new TimeoutException($"Таймаут подключения к {ip}:{port}");
                }

                client.EndConnect(result);

                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                switch (type)
                {
                    case "Bool":
                        ushort[] boolData = new ushort[1];
                        boolData[0] = (ushort)(value.ToLower() == "true" || value == "1" ? 1 : 0);
                        master.WriteMultipleRegisters(
                            Convert.ToByte(deviceAddr),
                            Convert.ToUInt16(address),
                            boolData);
                        break;

                    case "Float_32":
                        float floatValue = Convert.ToSingle(
                            value.Replace(',', '.'),
                            System.Globalization.CultureInfo.InvariantCulture);
                        byte[] bytes = BitConverter.GetBytes(floatValue);
                        ushort[] floatData = new ushort[2];
                        floatData[0] = (ushort)(bytes[1] * 256 + bytes[0]);
                        floatData[1] = (ushort)(bytes[3] * 256 + bytes[2]);
                        master.WriteMultipleRegisters(
                            Convert.ToByte(deviceAddr),
                            Convert.ToUInt16(address),
                            floatData);
                        break;

                    case "Int_16":
                        short intValue = Convert.ToInt16(value);
                        master.WriteSingleRegister(
                            Convert.ToByte(deviceAddr),
                            Convert.ToUInt16(address),
                            (ushort)intValue);
                        break;

                    default:
                        throw new NotSupportedException($"Неподдерживаемый тип: {type}");
                }

                return true;
            }
            finally
            {
                try
                {
                    client?.Close();
                    client?.Dispose();
                }
                catch { }
            }
        }

        /// <summary>
        /// СТАРЫЙ метод — теперь вызывает фоновую версию
        /// </summary>
        public void SendToController()
        {
            SendToControllerInBackground();
        }
    }
}