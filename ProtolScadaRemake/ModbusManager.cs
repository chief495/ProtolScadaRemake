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
        private Dictionary<string, ushort> _tagToRegisterMap = new Dictionary<string, ushort>
        {
            // Конвейер (из вашего Modbus сервера)
            { "CONVEYOR_STATUS", 0 },
            { "CONVEYOR_SPEED", 1 },
            { "ITEM_COUNT", 2 },
            { "EMERGENCY_STOP", 3 },
            
            // Примеры 
            { "LAHH151_Value", 10 },
            { "LAHH151_Manual", 11 },
            { "P651_IsWork", 12 },
            { "P651_Manual", 13 }
        };

        // Событие для уведомления о командах
        public event EventHandler<string> OnCommandExecuted;

        public ModbusManager(TGlobal global, string ip = "127.0.0.1", int port = 502)
        {
            _global = global;
            _modbusController = new ModbusController(ip, port);

            // Подписка на события
            _modbusController.OnRegisterValueChanged += OnRegisterValueChanged;
            _modbusController.OnStatusChanged += OnModbusStatusChanged;
            _modbusController.OnConnectionStateChanged += OnConnectionStateChanged;
        }

        public async Task InitializeAsync()
        {
            try
            {
                bool connected = await _modbusController.ConnectAsync();
                if (connected)
                {
                    // Запускаем опрос нужных регистров
                    var registersToPoll = _tagToRegisterMap.Values.ToArray();
                    _modbusController.StartPolling(registersToPoll);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
            }
        }

        private void OnRegisterValueChanged(ushort address, ushort value)
        {
            // Находим тег по адресу регистра
            var tagName = _tagToRegisterMap.FirstOrDefault(x => x.Value == address).Key;

            if (!string.IsNullOrEmpty(tagName))
            {
                // Обновляем переменную в SCADA
                var variable = _global.Variables.GetByName(tagName);
                if (variable != null)
                {
                    variable.ValueReal = value;
                    Debug.WriteLine($"Обновлен тег {tagName}: {value}");
                }
            }
        }

        private void OnModbusStatusChanged(string message)
        {
            Debug.WriteLine($"Modbus: {message}");

            // Можно обновлять статус в UI
            _global.Log.Add("Modbus", message, 0);
        }

        private void OnConnectionStateChanged(bool isConnected)
        {
            Debug.WriteLine($"Modbus соединение: {(isConnected ? "Установлено" : "Разорвано")}");
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
                    _global.Log.Add("Команда", message, 0);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка записи в Modbus: {ex.Message}");
                    _global.Log.Add("Ошибка", $"Не удалось записать в {tagName}: {ex.Message}", 2);
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"Тег {tagName} не найден в карте регистров");
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
                _global.Log.Add("Команда", message, 0);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи в регистр {address}: {ex.Message}");
                _global.Log.Add("Ошибка", $"Не удалось записать в регистр {address}: {ex.Message}", 2);
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
            _modbusController.Disconnect();
        }
    }
}