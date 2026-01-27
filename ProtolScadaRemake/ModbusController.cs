// ModbusController.cs
using System.Diagnostics;
using System.Net.Sockets;

namespace ProtolScadaRemake
{
    public class ModbusController
    {
        private string _ipAddress;
        private int _port;
        private byte _unitId;
        private ushort _transactionId = 0;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _isConnected = false;
        private bool _isPolling = false;
        private Thread _pollingThread;
        private int _pollingInterval = 1000; // мс

        // Кэш регистров
        private Dictionary<ushort, ushort> _registerCache = new Dictionary<ushort, ushort>();

        // События
        public event Action<string> OnStatusChanged;
        public event Action<ushort, ushort> OnRegisterValueChanged;
        public event Action<bool> OnConnectionStateChanged;

        public bool IsConnected => _isConnected;
        public bool IsPolling => _isPolling;

        public ModbusController(string ipAddress = "127.0.0.1", int port = 502, byte unitId = 1)
        {
            _ipAddress = ipAddress;
            _port = port;
            _unitId = unitId;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, _port);
                _stream = _tcpClient.GetStream();
                _isConnected = true;

                OnStatusChanged?.Invoke($"Подключено к Modbus серверу {_ipAddress}:{_port}");
                OnConnectionStateChanged?.Invoke(true);

                Debug.WriteLine($"Modbus подключен: {_ipAddress}:{_port}");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"Ошибка подключения: {ex.Message}");
                OnConnectionStateChanged?.Invoke(false);
                Debug.WriteLine($"Ошибка подключения Modbus: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            _isPolling = false;

            try
            {
                if (_pollingThread != null && _pollingThread.IsAlive)
                {
                    _pollingThread.Join(1000);
                }

                _stream?.Close();
                _tcpClient?.Close();
                _isConnected = false;

                OnStatusChanged?.Invoke("Отключено от Modbus сервера");
                OnConnectionStateChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отключения Modbus: {ex.Message}");
            }
        }

        public void StartPolling(params ushort[] registers)
        {
            if (!_isConnected || _isPolling) return;

            _isPolling = true;
            _pollingThread = new Thread(() => PollRegisters(registers));
            _pollingThread.IsBackground = true;
            _pollingThread.Start();
        }

        public void StopPolling()
        {
            _isPolling = false;
        }

        private void PollRegisters(ushort[] registers)
        {
            while (_isPolling && _isConnected)
            {
                try
                {
                    foreach (var register in registers)
                    {
                        var values = ReadHoldingRegisters(_unitId, register, 1);
                        if (values.Length > 0)
                        {
                            UpdateRegisterCache(register, values[0]);
                        }
                        Thread.Sleep(50); // Небольшая задержка между регистрами
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка опроса Modbus: {ex.Message}");
                    Thread.Sleep(5000); // Задержка при ошибке
                }

                Thread.Sleep(_pollingInterval);
            }
        }

        private void UpdateRegisterCache(ushort address, ushort value)
        {
            if (!_registerCache.ContainsKey(address) || _registerCache[address] != value)
            {
                _registerCache[address] = value;
                OnRegisterValueChanged?.Invoke(address, value);
            }
        }

        public ushort[] ReadHoldingRegisters(byte unitId, ushort startAddress, ushort quantity)
        {
            try
            {
                byte[] request = new byte[12];
                ushort requestTransactionId = ++_transactionId;

                // MBAP заголовок
                request[0] = (byte)(requestTransactionId >> 8);
                request[1] = (byte)(requestTransactionId & 0xFF);
                request[2] = 0; // Protocol ID
                request[3] = 0;
                request[4] = (byte)(0 >> 8);
                request[5] = (byte)(6 & 0xFF); // Length
                request[6] = unitId; // Unit ID
                request[7] = 3; // Function code (Read Holding Registers)
                request[8] = (byte)(startAddress >> 8);
                request[9] = (byte)(startAddress & 0xFF);
                request[10] = (byte)(quantity >> 8);
                request[11] = (byte)(quantity & 0xFF);

                // Отправка запроса
                _stream.Write(request, 0, request.Length);

                // Получение ответа
                byte[] response = new byte[1024];
                int bytesRead = _stream.Read(response, 0, response.Length);

                if (bytesRead < 9 || response[7] != 3)
                {
                    throw new Exception("Неверный ответ от Modbus сервера");
                }

                int dataLength = response[8];
                ushort[] registers = new ushort[dataLength / 2];

                for (int i = 0; i < registers.Length; i++)
                {
                    registers[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
                }

                return registers;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка чтения регистров: {ex.Message}");
                throw;
            }
        }

        public void WriteSingleRegister(byte unitId, ushort address, ushort value)
        {
            try
            {
                byte[] request = new byte[12];
                ushort requestTransactionId = ++_transactionId;

                // MBAP заголовок
                request[0] = (byte)(requestTransactionId >> 8);
                request[1] = (byte)(requestTransactionId & 0xFF);
                request[2] = 0; // Protocol ID
                request[3] = 0;
                request[4] = (byte)(0 >> 8);
                request[5] = (byte)(6 & 0xFF); // Length
                request[6] = unitId; // Unit ID
                request[7] = 6; // Function code (Write Single Register)
                request[8] = (byte)(address >> 8);
                request[9] = (byte)(address & 0xFF);
                request[10] = (byte)(value >> 8);
                request[11] = (byte)(value & 0xFF);

                // Отправка запроса
                _stream.Write(request, 0, request.Length);

                // Получение ответа
                byte[] response = new byte[1024];
                int bytesRead = _stream.Read(response, 0, response.Length);

                if (bytesRead != 12 || response[7] != 6)
                {
                    throw new Exception("Неверный ответ при записи в Modbus");
                }

                OnStatusChanged?.Invoke($"Записано значение {value} в регистр {address}");
                Debug.WriteLine($"Modbus запись: адрес={address}, значение={value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи в Modbus: {ex.Message}");
                throw;
            }
        }

        public ushort GetRegisterValue(ushort address)
        {
            return _registerCache.ContainsKey(address) ? _registerCache[address] : (ushort)0;
        }
    }
}