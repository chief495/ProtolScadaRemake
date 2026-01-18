// Добавьте в TGlobal.cs или создайте ModbusManager.cs
using ProtolScadaRemake;
using System.Diagnostics;

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
        
        // Примеры для ваших тегов SCADA
        { "LAHH151_Value", 10 },
        { "LAHH151_Manual", 11 },
        { "P651_IsWork", 12 },
        { "P651_Manual", 13 }
    };

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
    public void WriteToModbus(string tagName, ushort value)
    {
        if (_tagToRegisterMap.TryGetValue(tagName, out ushort address))
        {
            try
            {
                _modbusController.WriteSingleRegister(1, address, value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи в Modbus: {ex.Message}");
            }
        }
    }

    public void Disconnect()
    {
        _modbusController.Disconnect();
    }
}