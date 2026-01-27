using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class ModbusManager
    {
        private ModbusController _modbusController;
        private TGlobal _global;

        // Карта соответствия тегов SCADA и Modbus регистров
        private Dictionary<string, ushort> _tagToRegisterMap;

        // Публичные события (должны быть public)
        public event Action<string> OnStatusChanged;
        public event Action<bool> OnConnectionStateChanged;
        public event Action<ushort, ushort> OnRegisterValueChanged;
        public event EventHandler<string> OnCommandExecuted;

        public ModbusManager(TGlobal global, string ip = "127.0.0.1", int port = 502)
        {
            _global = global;
            _modbusController = new ModbusController(ip, port);

            // Инициализируем карту регистров
            InitializeTagMap();

            // Подписка на события
            _modbusController.OnRegisterValueChanged += OnRegisterValueChanged;
            _modbusController.OnStatusChanged += OnModbusStatusChanged;
            _modbusController.OnConnectionStateChanged += OnConnectionStateChanged;
        }

        private void InitializeTagMap()
        {
            _tagToRegisterMap = new Dictionary<string, ushort>
            {
                // EM режимы и управление
                { "EM_MODE", 100 },
                { "EM_Rejim", 100 },
                { "EM_StartCommand", 101 },
                { "EM_StopCommand", 102 },
                { "EM_EmergencyStop", 103 },
                
                // Производительность
                { "EM_AutoMassFlowSp", 200 },
                { "FM601_Value", 201 },
                
                // Затравка
                { "EM_ReceptZatravkaMass", 300 },
                { "EM_ReceptZatravkaTime", 301 },
                { "EM_ZatravkaStart", 302 },
                { "EM_ZatravkaStop", 303 },
                
                // Отгрузка
                { "EM_Unload_Speed", 400 },
                { "EM_UnloadCounter", 401 },
                { "EM_Unloading_Rejim", 402 },
                
                // Датчики
                { "LAHH151_Value", 500 },
                { "LAHH151_Manual", 501 },
                
                // Насосы
                { "P651_IsWork", 600 },
                { "P651_Manual", 601 },
                { "P651_Speed", 602 }
            };
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                bool connected = await _modbusController.ConnectAsync();
                if (connected)
                {
                    // Запускаем опрос нужных регистров
                    var registersToPoll = _tagToRegisterMap.Values.ToArray();
                    _modbusController.StartPolling(registersToPoll);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
                OnStatusChanged?.Invoke($"Ошибка инициализации: {ex.Message}");
                return false;
            }
        }

        private void OnModbusStatusChanged(string message)
        {
            Debug.WriteLine($"Modbus: {message}");
            OnStatusChanged?.Invoke(message);
        }

        // Метод для записи в Modbus
        public bool WriteToModbus(string tagName, ushort value)
        {
            if (_tagToRegisterMap.TryGetValue(tagName, out ushort address))
            {
                try
                {
                    _modbusController.WriteSingleRegister(1, address, value);

                    // Уведомляем о выполнении команды
                    string message = $"Записано в {tagName}: {value} (регистр {address})";
                    OnCommandExecuted?.Invoke(this, message);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка записи в Modbus: {ex.Message}");
                    OnStatusChanged?.Invoke($"Ошибка записи в {tagName}: {ex.Message}");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Тег {tagName} не найден в карте регистров");
                OnStatusChanged?.Invoke($"Тег {tagName} не найден");
                return false;
            }
        }

        // Метод для записи в произвольный регистр
        public bool WriteToRegister(byte unitId, ushort address, ushort value, string description = "")
        {
            try
            {
                _modbusController.WriteSingleRegister(unitId, address, value);

                string message = string.IsNullOrEmpty(description)
                    ? $"Записано значение {value} в регистр {address}"
                    : description;

                OnCommandExecuted?.Invoke(this, message);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи в регистр {address}: {ex.Message}");
                OnStatusChanged?.Invoke($"Ошибка записи в регистр {address}: {ex.Message}");
                return false;
            }
        }

        // Метод для обработки команды от ModePanel
        public bool ProcessModeCommand(byte unitId, ushort registerAddress, ushort value, string description)
        {
            return WriteToRegister(unitId, registerAddress, value, description);
        }

        public void Disconnect()
        {
            _modbusController?.Disconnect();
        }

        // Метод для получения значения регистра
        public ushort GetRegisterValue(ushort address)
        {
            return _modbusController?.GetRegisterValue(address) ?? 0;
        }

        // Метод для получения значения тега
        public ushort GetTagValue(string tagName)
        {
            if (_tagToRegisterMap.TryGetValue(tagName, out ushort address))
            {
                return GetRegisterValue(address);
            }
            return 0;
        }
    }
}