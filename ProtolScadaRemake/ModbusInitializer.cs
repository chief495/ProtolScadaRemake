using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ProtolScadaRemake
{
    public static class ModbusInitializer
    {
        // Области памяти Modbus
        public static TWordsArea HR_0000 { get; private set; }
        public static TWordsArea HR_0060 { get; private set; }
        public static TWordsArea HR_00C0 { get; private set; }
        public static TWordsArea HR_0120 { get; private set; }
        public static TWordsArea HR_0180 { get; private set; }
        public static TWordsArea HR_01C0 { get; private set; }
        public static TWordsArea HR_0240 { get; private set; }
        public static TWordsArea HR_02A0 { get; private set; }
        public static TWordsArea HR_0300 { get; private set; }
        public static TWordsArea HR_0360 { get; private set; }
        public static TWordsArea HR_03C0 { get; private set; }
        public static TWordsArea HR_0420 { get; private set; }
        public static TWordsArea HR_0480 { get; private set; }
        public static TWordsArea HR_04E0 { get; private set; }
        public static TWordsArea HR_0540 { get; private set; }
        public static TWordsArea HR_05A0 { get; private set; }
        public static TWordsArea HR_0620 { get; private set; }
        public static TWordsArea HR_0680 { get; private set; }
        public static TWordsArea HR_06F0 { get; private set; }
        public static TWordsArea HR_0750 { get; private set; }
        public static TWordsArea HR_07B0 { get; private set; }
        public static TWordsArea HR_0810 { get; private set; }
        public static TWordsArea HR_0870 { get; private set; }
        public static TWordsArea HR_08D0 { get; private set; }

        private static Thread ReadDeviceDataThread;
        private static Thread WriteDeviceDataThread;
        private static Thread ReadVariablesThread;
        private static bool _isRunning = false;
        private static List<TWordsArea> _modbusAreas = new List<TWordsArea>();

        public static void InitializeAllVariables(TGlobal global)
        {
            try
            {
                Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ MODBUS ДЛЯ WPF ЭЛЕМЕНТОВ ===");

                global.Plc_IpAddress = "192.168.88.64";
                global.Plc_PortNum = 502;
                global.Plc_DeviceAddress = 1;

                global.Clear();

                InitializeMemoryAreas(global);

                // ТОЛЬКО переменные, которые НЕ создаются элементами TElement*
                InitializeSystemVariables(global);
                InitializeT400Sensors(global);
                InitializeT500Sensors(global);
                InitializeT100T150T200T700Sensors(global);
                InitializeA100Screw(global);
                InitializeGroRecipe(global);
                InitializeTcRecipe(global);
                InitializeEmRecipe(global);
                InitializePIDControllers(global);
                InitializeUnloadingSystem(global);
                InitializeAdditionalVariables(global);

                // Создание элементов - ОНИ сами создают все нужные переменные!
                InitializeElements(global);

                Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ MODBUS ЗАВЕРШЕНА ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
            }
        }

        private static void InitializeMemoryAreas(TGlobal global)
        {
            HR_0000 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0000, 0x60);
            HR_0060 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0060, 0x60);
            HR_00C0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x00C0, 0x60);
            HR_0120 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0120, 0x60);
            HR_0180 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0180, 0x60);
            HR_01C0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x01C0, 0x60);
            HR_0240 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0240, 0x60);
            HR_02A0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x02A0, 0x60);
            HR_0300 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0300, 0x60);
            HR_0360 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0360, 0x60);
            HR_03C0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x03C0, 0x60);
            HR_0420 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0420, 0x60);
            HR_0480 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0480, 0x60);
            HR_04E0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x04E0, 0x60);
            HR_0540 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0540, 0x60);
            HR_05A0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x05A0, 0x60);
            HR_0620 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0620, 0x60);
            HR_0680 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0680, 0x60);
            HR_06F0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x06F0, 0x60);
            HR_0750 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0750, 0x60);
            HR_07B0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x07B0, 0x60);
            HR_0810 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0810, 0x60);
            HR_0870 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x0870, 0x60);
            HR_08D0 = new TWordsArea(global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, 0x08D0, 0x60);

            _modbusAreas.Clear();
            _modbusAreas.AddRange(new TWordsArea[] {
                HR_0000, HR_0060, HR_00C0, HR_0120, HR_0180, HR_01C0, HR_0240, HR_02A0,
                HR_0300, HR_0360, HR_03C0, HR_0420, HR_0480, HR_04E0, HR_0540, HR_05A0,
                HR_0620, HR_0680, HR_06F0, HR_0750, HR_07B0, HR_0810, HR_0870, HR_08D0
            });
        }

        public static void StartModbusThreads(TGlobal global)
        {
            if (_isRunning) return;
            _isRunning = true;

            ReadDeviceDataThread = new Thread(() => ReadDeviceDataThreadTask(global));
            ReadDeviceDataThread.Name = "ProtolModbusReader";
            ReadDeviceDataThread.IsBackground = true;
            ReadDeviceDataThread.Start();

            WriteDeviceDataThread = new Thread(() => WriteDeviceDataThreadTask(global));
            WriteDeviceDataThread.Name = "ProtolModbusWriter";
            WriteDeviceDataThread.IsBackground = true;
            WriteDeviceDataThread.Start();

            ReadVariablesThread = new Thread(() => ReadVariablesThreadTask(global));
            ReadVariablesThread.Name = "ProtolReadVariables";
            ReadVariablesThread.IsBackground = true;
            ReadVariablesThread.Start();

            Debug.WriteLine("=== ПОТОКИ MODBUS ЗАПУЩЕНЫ ===");
        }

        public static void StopModbusThreads()
        {
            _isRunning = false;
            try
            {
                ReadDeviceDataThread?.Join(500);
                WriteDeviceDataThread?.Join(500);
                ReadVariablesThread?.Join(500);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при остановке потоков: {ex.Message}");
            }
            Debug.WriteLine("=== ПОТОКИ MODBUS ОСТАНОВЛЕНЫ ===");
        }

        private static void ReadDeviceDataThreadTask(TGlobal global)
        {
            while (_isRunning)
            {
                try
                {
                    HR_0000?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0060?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_00C0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0120?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0180?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_01C0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0240?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_02A0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0300?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0360?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_03C0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0420?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0480?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_04E0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0540?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_05A0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0620?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0680?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_06F0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0750?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_07B0?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0810?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_0870?.GetModbusTcpHoldingRegisters(global.Log);
                    HR_08D0?.GetModbusTcpHoldingRegisters(global.Log);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка чтения Modbus: {ex.Message}");
                }
                Thread.Sleep(50);
            }
        }

        private static void WriteDeviceDataThreadTask(TGlobal global)
        {
            while (_isRunning)
            {
                try
                {
                    global.Commands?.SendToController();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка записи Modbus: {ex.Message}");
                }
                Thread.Sleep(50);
            }
        }

        private static void ReadVariablesThreadTask(TGlobal global)
        {
            while (_isRunning)
            {
                try
                {
                    if (global.Variables == null) continue;

                    global.Variables.ReadGroup(HR_0000?.Data, 0x01);
                    global.Variables.ReadGroup(HR_0060?.Data, 0x02);
                    global.Variables.ReadGroup(HR_00C0?.Data, 0x03);
                    global.Variables.ReadGroup(HR_0120?.Data, 0x04);
                    global.Variables.ReadGroup(HR_0180?.Data, 0x05);
                    global.Variables.ReadGroup(HR_01C0?.Data, 0x06);
                    global.Variables.ReadGroup(HR_0240?.Data, 0x07);
                    global.Variables.ReadGroup(HR_02A0?.Data, 0x08);
                    global.Variables.ReadGroup(HR_0300?.Data, 0x09);
                    global.Variables.ReadGroup(HR_0360?.Data, 0x0A);
                    global.Variables.ReadGroup(HR_03C0?.Data, 0x0B);
                    global.Variables.ReadGroup(HR_0420?.Data, 0x0C);
                    global.Variables.ReadGroup(HR_0480?.Data, 0x0D);
                    global.Variables.ReadGroup(HR_04E0?.Data, 0x0E);
                    global.Variables.ReadGroup(HR_0540?.Data, 0x0F);
                    global.Variables.ReadGroup(HR_05A0?.Data, 0x10);
                    global.Variables.ReadGroup(HR_0620?.Data, 0x11);
                    global.Variables.ReadGroup(HR_0680?.Data, 0x12);
                    global.Variables.ReadGroup(HR_06F0?.Data, 0x13);
                    global.Variables.ReadGroup(HR_0750?.Data, 0x14);
                    global.Variables.ReadGroup(HR_07B0?.Data, 0x15);
                    global.Variables.ReadGroup(HR_0810?.Data, 0x16);
                    global.Variables.ReadGroup(HR_0870?.Data, 0x17);
                    global.Variables.ReadGroup(HR_08D0?.Data, 0x18);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка чтения переменных: {ex.Message}");
                }
                Thread.Sleep(50);
            }
        }

        // ========== ТОЛЬКО ПЕРЕМЕННЫЕ, КОТОРЫЕ НЕ СОЗДАЮТСЯ ЭЛЕМЕНТАМИ ==========

        private static void InitializeSystemVariables(TGlobal global)
        {
            // Счетчик готовой продукции
            global.Variables.Add("SmenaProductCounter_Reset", 0x12, 0x0020, 1, "Bool", "", "Нет;Да", "", "Сброс счетчика произведенной продукции за смену");
            global.Commands.Add("SmenaProductCounter_Reset", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A0, "Bool", "Нет;Да", "Сброс счетчика произведенной продукции за смену");
            global.Variables.Add("SmenaProductCouner_Volume", 0x12, 0x0021, 1, "Float_32", "", "##0.#", " кг.", "Показания счетчика произведенной продукции за смену");
            global.Variables.Add("TotalProductCounter_Reset", 0x12, 0x0023, 1, "Bool", "", "Нет;Да", "", "Сброс счетчика произведенной продукции");
            global.Commands.Add("TotalProductCounter_Reset", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A3, "Bool", "Нет;Да", "Сброс счетчика произведенной продукции");
            global.Variables.Add("TotalProductCouner_Volume", 0x12, 0x0024, 1, "Float_32", "", "##0.#", " кг.", "Показания счетчика произведенной продукции");
        }

        private static void InitializeT400Sensors(TGlobal global)
        {
            // Ёмкость T-400
            global.Variables.Add("T400_StartMixer", 0x12, 0x0026, 1, "Bool", "", "Нет;Да", "", "Включить миксер Т400");
            global.Commands.Add("T400_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A6, "Bool", "Нет;Да", "Включить миксер Т400");
            global.Variables.Add("T400_StartWater", 0x12, 0x0028, 1, "Bool", "", "Нет;Да", "", "Включить наполнение Т400");
            global.Commands.Add("T400_StartWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A8, "Bool", "Нет;Да", "Включить наполнение Т400");
            global.Variables.Add("T400_StopWater", 0x12, 0x0029, 1, "Bool", "", "Нет;Да", "", "Отключить наполнение Т400");
            global.Commands.Add("T400_StopWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A9, "Bool", "Нет;Да", "Включить наполнение Т400");
            global.Variables.Add("T400_SpWater", 0x12, 0x002A, 1, "Float_32", "", "##0.#", " л.", "Объем наполнения Т400");
            global.Commands.Add("T400_SpWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06AA, "Float_32", "##0.#", "Объем наполнения Т400");
        }

        private static void InitializeT500Sensors(TGlobal global)
        {
            // Ёмкость T-500
            global.Variables.Add("T500_StartMixer", 0x12, 0x0027, 1, "Bool", "", "Нет;Да", "", "Включить миксер Т500");
            global.Commands.Add("T500_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06A7, "Bool", "Нет;Да", "Отключить миксер Т500");
            global.Variables.Add("T500_StartWater", 0x12, 0x002C, 1, "Bool", "", "Нет;Да", "", "Включить наполнение Т500");
            global.Commands.Add("T500_StartWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06AC, "Bool", "Нет;Да", "Включить наполнение Т500");
            global.Variables.Add("T500_StopWater", 0x12, 0x002D, 1, "Bool", "", "Нет;Да", "", "Отключить наполнение Т500");
            global.Commands.Add("T500_StopWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06AD, "Bool", "Нет;Да", "Включить наполнение Т500");
            global.Variables.Add("T500_SpWater", 0x12, 0x002E, 1, "Float_32", "", "##0.#", " л.", "Объем наполнения Т500");
            global.Commands.Add("T500_SpWater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x06AE, "Float_32", "##0.#", "Объем наполнения Т500");
        }

        private static void InitializeT100T150T200T700Sensors(TGlobal global)
        {
            // Ёмкость T-100
            global.Variables.Add("T100_StartMixer", 0x13, 0x0030, 1, "Bool", "", "Нет;Да", "", "Включить миксер Т100");
            global.Commands.Add("T100_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0720, "Bool", "Нет;Да", "Отключить миксер Т100");
            // Ёмкость T-150
            global.Variables.Add("T150_StartMixer", 0x13, 0x0031, 1, "Bool", "", "Нет;Да", "", "Включить миксер Т150");
            global.Commands.Add("T150_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0721, "Bool", "Нет;Да", "Отключить миксер Т150");
            // Тензодатчики
            global.Variables.Add("WIT100_Volume", 0x13, 0x0032, 1, "Float_32", "", "##0.#", "", "Вес в ёскости Т100");
            global.Variables.Add("WIT200_Volume", 0x13, 0x0034, 1, "Float_32", "", "##0.#", "", "Вес в ёскости Т200");
            global.Variables.Add("WIT700_Volume", 0x13, 0x0036, 1, "Float_32", "", "##0.#", "", "Вес в ёскости Т700");
        }

        private static void InitializeA100Screw(TGlobal global)
        {
            // A100
            global.Variables.Add("A100_Speed", 0x13, 0x0048, 1, "Float_32", "", "##0.#", "", "Скорость шнека А-100");
            global.Commands.Add("A100_Speed", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0738, "Float_32", "##0.#", "Скорость шнека А-100");

            global.Variables.Add("GRO_Recept_A100BlockTemp", 0x18, 0x0042, 1, "Float_32", "", "##0", " °С", "Температура блокировки А-100");
            global.Commands.Add("GRO_Recept_A100BlockTemp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0912, "Float_32", "##0.###", "Температура блокировки А-100");
            global.Variables.Add("GRO_Recept_A100BlockWeith", 0x18, 0x0044, 1, "Float_32", "", "##0", " кг.", "Масса блокировки А-100");
            global.Commands.Add("GRO_Recept_A100BlockWeith", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0914, "Float_32", "##0.###", "Масса блокировки А-100");
        }

        private static void InitializeGroRecipe(TGlobal global)
        {
            // Рецепт приготовления компонентов ГРО
            global.Variables.Add("GRO_Recept_Selitra", 0x16, 0x0040, 1, "Float_32", "", "##0.##", " %", "Рецепт подготовки ГРО. Объем селитры");
            global.Commands.Add("GRO_Recept_Selitra", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0850, "Float_32", "##0.#", "Рецепт подготовки ГРО. Объем селитры");
            global.Variables.Add("GRO_Recept_Water", 0x16, 0x0042, 1, "Float_32", "", "##0.##", " %", "Рецепт подготовки ГРО. Объем воды");
            global.Commands.Add("GRO_Recept_Water", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0852, "Float_32", "##0.##", "Рецепт подготовки ГРО. Объем воды");
            global.Variables.Add("GRO_Recept_KislotaEnable", 0x16, 0x0044, 1, "Bool", "", "Нет;Да", "", "Рецепт подготовки ГРО. Использовать кислоту");
            global.Commands.Add("GRO_Recept_KislotaEnable", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0854, "Bool", "Нет;Да", "Рецепт подготовки ГРО. Использовать кислоту");
            global.Variables.Add("GRO_Recept_Kislota", 0x16, 0x0045, 1, "Float_32", "", "##0.##", " %", "Рецепт подготовки ГРО. Объем кислоты");
            global.Commands.Add("GRO_Recept_Kislota", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0855, "Float_32", "##0.##", "Рецепт подготовки ГРО. Объем кислоты");
            global.Variables.Add("GRO_Recept_Tmax", 0x16, 0x0047, 1, "Float_32", "", "##0.##", " °С", "Рецепт подготовки ГРО. Максимальная температура.");
            global.Commands.Add("GRO_Recept_Tmax", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0857, "Float_32", "##0.##", "Рецепт подготовки ГРО. Максимальная температура.");
            global.Variables.Add("GRO_Recept_Tmin", 0x16, 0x0049, 1, "Float_32", "", "##0.##", " °С", "Рецепт подготовки ГРО. Минимальная температура.");
            global.Commands.Add("GRO_Recept_Tmin", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0859, "Float_32", "##0.##", "Рецепт подготовки ГРО. Минимальная температура.");
            global.Variables.Add("GRO_Rejim", 0x16, 0x004B, 1, "Int_16", "", "##0", "", "Режим подготовки ГРО");
            global.Variables.Add("GRO_AutoMassSp", 0x16, 0x004C, 1, "Float_32", "", "##0.##", " кг.", "Подготовка ГРО. Автомат. Задание массы.");
            global.Commands.Add("GRO_AutoMassSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x085C, "Float_32", "##0.##", "Подготовка ГРО. Автомат. Задание массы.");
            global.Commands.Add("GRO_RejimToOff", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x085E, "Bool", "Нет;Да", "Подготовка ГРО. Режим OFF.");
            global.Commands.Add("GRO_RejimToManual", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x085F, "Bool", "Нет;Да", "Подготовка ГРО. Режим Полуавтомат.");
            global.Commands.Add("GRO_RejimToAuto", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0860, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат.");
            global.Commands.Add("GRO_Manual_Selitra_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0861, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения селитры.");
            global.Commands.Add("GRO_Manual_Stop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0862, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Стоп.");
            global.Commands.Add("GRO_Manual_Pause", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0863, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пауза.");
            global.Variables.Add("GRO_ManualSelitraCounter", 0x16, 0x0054, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса селитры.");
            global.Variables.Add("GRO_ManualSelitraCounterSp", 0x16, 0x0056, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Нужная масса селитры.");
            global.Commands.Add("GRO_ManualSelitraCounterSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0866, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса селитры.");
            global.Commands.Add("GRO_Manual_Water_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0868, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения воды.");
            global.Commands.Add("GRO_ManualWaterCounterSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0869, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса воды.");
            global.Variables.Add("GRO_ManualWaterCounter", 0x16, 0x005B, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса воды.");
            global.Commands.Add("GRO_Manual_Kislota_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x086D, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения кислоты.");
            global.Commands.Add("GRO_ManualKislotaCounterSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x086E, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса кислоты.");
            global.Variables.Add("GRO_ManualKislotaCounter", 0x17, 0x0000, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса кислоты.");
            global.Commands.Add("GRO_Auto_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0872, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Пуск.");
            global.Commands.Add("GRO_Auto_Stop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0873, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Стоп.");
            global.Commands.Add("GRO_Auto_Pause", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0874, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Пауза.");
            global.Variables.Add("GRO_AutoSelitraSp", 0x17, 0x0005, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса селитры.");
            global.Variables.Add("GRO_AutoSelitraCurrent", 0x17, 0x0007, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса селитры.");
            global.Variables.Add("GRO_AutoWaterSp", 0x17, 0x0009, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса воды.");
            global.Variables.Add("GRO_AutoWaterCurrent", 0x17, 0x000B, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса воды.");
            global.Variables.Add("GRO_AutoKislotaSp", 0x17, 0x000D, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса кислоты.");
            global.Variables.Add("GRO_AutoKislotaCurrent", 0x17, 0x000F, 1, "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса кислоты.");
            global.Commands.Add("GRO_TransportStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0881, "Bool", "Нет;Да", "Подготовка ГРО. Включить перекачку в Т-150.");
            global.Commands.Add("GRO_TransportStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0882, "Bool", "Нет;Да", "Подготовка ГРО. Отключить перекачку в Т-150.");
            global.Variables.Add("GRO_Recept_TmaxDelta", 0x17, 0x0013, 1, "Float_32", "", "##0.#", " °С", "Рецепт подготовки ГРО. Дельта максимальной температуры.");
            global.Commands.Add("GRO_Recept_TmaxDelta", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0883, "Float_32", "##0.#", "Рецепт подготовки ГРО. Дельта максимальной температуры.");
        }

        private static void InitializeTcRecipe(TGlobal global)
        {
            global.Variables.Add("TC_Recept_Disel", 0x17, 0x0015, 1, "Float_32", "", "##0.##", " %", "Рецепт подготовки TC. Объем дизеля");
            global.Commands.Add("TC_Recept_Disel", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0885, "Float_32", "##0.##", "Рецепт подготовки TC. Объем дизеля");
            global.Variables.Add("TC_Recept_Emulgator", 0x17, 0x0017, 1, "Float_32", "", "##0.##", " %", "Рецепт подготовки TC. Объем эмульгатора");
            global.Commands.Add("TC_Recept_Emulgator", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0887, "Float_32", "##0.##", "Рецепт подготовки TC. Объем эмульгатора");
            global.Variables.Add("TC_Recept_Temperature_T200", 0x17, 0x0019, 1, "Float_32", "", "##0.##", " °С", "Рецепт подготовки TC. Температура в Т-200");
            global.Commands.Add("TC_Recept_Temperature_T200", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0889, "Float_32", "##0.##", "Рецепт подготовки TC. Температура в Т-200");
            global.Variables.Add("TC_Recept_Temperature_T250", 0x17, 0x001B, 1, "Float_32", "", "##0.##", " °С", "Рецепт подготовки TC. Температура в Т-250");
            global.Commands.Add("TC_Recept_Temperature_T250", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x088B, "Float_32", "##0.##", "Рецепт подготовки TC. Температура в Т-250");
            global.Variables.Add("TC_Rejim", 0x17, 0x001D, 1, "Int_16", "", "##0", "", "Режим подготовки TC");
            global.Commands.Add("TC_RejimToOff", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x088E, "Bool", "Нет;Да", "Подготовка TC. Режим OFF.");
            global.Commands.Add("TC_RejimToManual", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x088F, "Bool", "Нет;Да", "Подготовка TC. Режим Полуавтомат.");
            global.Commands.Add("TC_RejimToAuto", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0890, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.");
            global.Commands.Add("TC_ManualStartDisel", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0891, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат. Пуск дизеля");
            global.Commands.Add("TC_ManualStartEmulgator", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0892, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат.Пуск эмульгатора");
            global.Commands.Add("TC_ManualStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0893, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат.Останов");
            global.Commands.Add("TC_ManualPause", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0894, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат. Пауза");
            global.Commands.Add("TC_AutolStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0895, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.Пуск");
            global.Commands.Add("TC_AutoStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0896, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.Останов");
            global.Commands.Add("TC_AutoPause", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0897, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат. Пауза");
            global.Commands.Add("TC_TransportStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0898, "Bool", "Нет;Да", "Подготовка TC. Включить перекачку в Т-250.");
            global.Commands.Add("TC_TransportStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0899, "Bool", "Нет;Да", "Подготовка TC. Отключить перекачку в Т-250.");
            global.Variables.Add("TC_ManualDiselCurrent", 0x17, 0x002A, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуавтомат. Текущая масса дизельного топлива.");
            global.Variables.Add("TC_ManualEmulgatorCurrent", 0x17, 0x002C, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуавтомат. Текущая масса эмульгатора.");
            global.Variables.Add("TC_AutoDiselCurrent", 0x17, 0x002E, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Текущая масса дизельного топлива.");
            global.Variables.Add("TC_AutoEmulgatorCurrent", 0x17, 0x0030, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Текущая масса эмульгатора.");
            global.Variables.Add("TC_ManualDiselSp", 0x17, 0x0032, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуатомат. Требуемая масса дизельного топлива.");
            global.Commands.Add("TC_ManualDiselSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08A2, "Float_32", "##0.#", "Подготовка TC. Полуатомат. Требуемая масса дизельного топлива.");
            global.Variables.Add("TC_ManualEmulgatorSp", 0x17, 0x0034, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуатомат. Требуемая масса эмульгатора.");
            global.Commands.Add("TC_ManualEmulgatorSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08A4, "Float_32", "##0.#", "Подготовка TC. Полуатомат. Требуемая масса эмульгатора.");
            global.Variables.Add("TC_AutoDiselSp", 0x17, 0x0036, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса дизельного топлива.");
            global.Variables.Add("TC_AutoEmulgatorSp", 0x17, 0x0038, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса эмульгатора.");
            global.Variables.Add("TC_AutoMassSp", 0x17, 0x003A, 1, "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса.");
            global.Commands.Add("TC_AutoMassSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08AA, "Float_32", "##0.#", "Подготовка TC. Автомат. Требуемая масса.");
        }

        private static void InitializeEmRecipe(TGlobal global)
        {
            global.Variables.Add("Compressor_Start", 0x17, 0x003C, 1, "Bool", "", "Нет;Да", "", "Включить компрессор");
            global.Commands.Add("Compressor_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08AC, "Bool", "Нет;Да", "Включить компрессор");
            global.Variables.Add("EM_Recept_GRO", 0x17, 0x003D, 1, "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Объем ГРО");
            global.Commands.Add("EM_Recept_GRO", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08AD, "Float_32", "##0.##", "Рецепт производства ЭМ. Объем ГРО");
            global.Variables.Add("EM_Recept_Disel", 0x17, 0x003F, 1, "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Объем топливной смеси");
            global.Commands.Add("EM_Recept_Disel", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08AF, "Float_32", "##0.##", "Рецепт производства ЭМ. Объем топливной смеси");
            global.Variables.Add("EM_ReceptDiaeslLast", 0x17, 0x0041, 1, "Float_32", "", "##0.##", " кг.", "Рецепт производства ЭМ. Масса топлива промывки");
            global.Commands.Add("EM_ReceptDiaeslLast", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08B1, "Float_32", "##0.##", "Рецепт производства ЭМ. Масса топлива промывки");
            global.Variables.Add("EM_ReceptZatravkaMass", 0x17, 0x0043, 1, "Float_32", "", "##0.##", " кг.", "Рецепт производства ЭМ. Масса затравки");
            global.Commands.Add("EM_ReceptZatravkaMass", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08B3, "Float_32", "##0.##", "Рецепт производства ЭМ. Масса затравки");
            global.Variables.Add("EM_ReceptZatravkaTime", 0x17, 0x0045, 1, "Int_16", "", "##0", " сек.", "Рецепт производства ЭМ. Время затравки");
            global.Commands.Add("EM_ReceptZatravkaTime", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08B5, "Int_16", "##0", "Рецепт производства ЭМ. Время затравки");
            global.Variables.Add("EM_ReceptWorkLevel", 0x17, 0x0046, 1, "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Рабочий уровень в Т-650");
            global.Commands.Add("EM_ReceptWorkLevel", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08B6, "Float_32", "##0.##", "Рецепт производства ЭМ. Рабочий уровень в Т-650");
            global.Variables.Add("EM_ReceptAlarmPressure", 0x17, 0x0048, 1, "Float_32", "", "##0.##", " Атм.", "Рецепт производства ЭМ. Аварийное давление PT-604");
            global.Commands.Add("EM_ReceptAlarmPressure", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08B8, "Float_32", "##0.##", "Рецепт производства ЭМ. Аварийное давление PT-604");
            global.Variables.Add("EM_Rejim", 0x17, 0x004A, 1, "Int_16", "", "##0", "", "Режим подготовки EM");
            global.Commands.Add("EM_RejimToOff", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08BB, "Bool", "Нет;Да", "Подготовка ЭМ. Режим OFF.");
            global.Commands.Add("EM_RejimToAuto", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08BC, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат.");
            global.Commands.Add("EM_ZatravkaStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08BD, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Пуск затравки.");
            global.Commands.Add("EM_ZatravkaStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08BE, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Останов затравки.");
            global.Commands.Add("EM_AutoStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08BF, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Пуск.");
            global.Commands.Add("EM_AutoDojat", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C0, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Дожать.");
            global.Commands.Add("EM_AutoStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C1, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Стоп.");
            global.Commands.Add("EM_AutoFastStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C2, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Быстрый стоп.");
            global.Variables.Add("EM_AutoMassFlowSp", 0x17, 0x0053, 1, "Float_32", "", "##0", " кг./мин.", "Рецепт производства ЭМ. Режим Автомат. Задание производительности.");
            global.Commands.Add("EM_AutoMassFlowSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C3, "Float_32", "##0", "Рецепт производства ЭМ. Режим Автомат. Задание производительности.");
            global.Variables.Add("M600_EmergensyStop", 0x17, 0x0055, 1, "Bool", "", "Нет;Да", "", "Блокировка работы эмульсификатора");
            global.Variables.Add("EM_Recept_ReverseTime", 0x18, 0x002B, 1, "Int_16", "", "##0", " сек.", "Время реверса Р-700");
            global.Commands.Add("EM_Recept_ReverseTime", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08FB, "Int_16", "##0", "Время реверса Р-700");
            global.Variables.Add("EM_M600_SpeedSp", 0x18, 0x0036, 1, "Float_32", "", "##0", " %", "Скорость эмульсификатора М-600");
            global.Commands.Add("EM_M600_SpeedSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0906, "Float_32", "##0.###", "Скорость эмульсификатора М-600");
        }

        private static void InitializePIDControllers(TGlobal global)
        {
            // PID P601
            global.Variables.Add("P601_PID_P", 0x17, 0x0056, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-601. Коэффициент P");
            global.Commands.Add("P601_PID_P", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C6, "Float_32", "##0.###", "PID-регулятор насоса P-601. Коэффициент P");
            global.Variables.Add("P601_PID_I", 0x17, 0x0058, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-601. Коэффициент I");
            global.Commands.Add("P601_PID_I", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08C8, "Float_32", "##0.###", "PID-регулятор насоса P-601. Коэффициент I");
            global.Variables.Add("P601_PID_D", 0x17, 0x005A, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-601. Коэффициент D");
            global.Commands.Add("P601_PID_D", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08CA, "Float_32", "##0.###", "PID-регулятор насоса P-601. Коэффициент D");
            global.Variables.Add("P601_PID_T", 0x17, 0x005C, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-601. Коэффициент T");
            global.Commands.Add("P601_PID_T", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08CC, "Float_32", "##0.###", "PID-регулятор насоса P-601. Коэффициент T");
            global.Variables.Add("P601_PID_Tune", 0x18, 0x0038, 1, "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-601. Автонастройка");
            global.Commands.Add("P601_PID_Tune", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0908, "Bool", "Нет;Да", "PID-регулятор насоса P-601. Автонастройка");

            // PID P602
            global.Variables.Add("P602_PID_P", 0x17, 0x005E, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент P");
            global.Commands.Add("P602_PID_P", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08CE, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент P");
            global.Variables.Add("P602_PID_I", 0x18, 0x0000, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент I");
            global.Commands.Add("P602_PID_I", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08D0, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент I");
            global.Variables.Add("P602_PID_D", 0x18, 0x0002, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент D");
            global.Commands.Add("P602_PID_D", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08D2, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент D");
            global.Variables.Add("P602_PID_T", 0x18, 0x0004, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент T");
            global.Commands.Add("P602_PID_T", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08D4, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент T");
            global.Variables.Add("P602_PID_Tune", 0x18, 0x003A, 1, "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-602. Автонастройка");
            global.Commands.Add("P602_PID_Tune", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x090A, "Bool", "Нет;Да", "PID-регулятор насоса P-602. Автонастройка");

            // PID M600
            global.Variables.Add("M600_PID_P", 0x18, 0x0006, 1, "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент P");
            global.Commands.Add("M600_PID_P", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08D6, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент P");
            global.Variables.Add("M600_PID_I", 0x18, 0x0008, 1, "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент I");
            global.Commands.Add("M600_PID_I", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08D8, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент I");
            global.Variables.Add("M600_PID_D", 0x18, 0x000A, 1, "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент D");
            global.Commands.Add("M600_PID_D", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08DA, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент D");
            global.Variables.Add("M600_PID_T", 0x18, 0x000C, 1, "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент T");
            global.Commands.Add("M600_PID_T", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08DC, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент T");

            // PID P651
            global.Variables.Add("P651_PID_P", 0x18, 0x000E, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-651. Коэффициент P");
            global.Commands.Add("P651_PID_P", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08DE, "Float_32", "##0.###", "PID-регулятор насоса P-651. Коэффициент P");
            global.Variables.Add("P651_PID_I", 0x18, 0x0010, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-651. Коэффициент I");
            global.Commands.Add("P651_PID_I", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08E0, "Float_32", "##0.###", "PID-регулятор насоса P-651. Коэффициент I");
            global.Variables.Add("P651_PID_D", 0x18, 0x0012, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-651. Коэффициент D");
            global.Commands.Add("P651_PID_D", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08E2, "Float_32", "##0.###", "PID-регулятор насоса P-651. Коэффициент D");
            global.Variables.Add("P651_PID_T", 0x18, 0x0014, 1, "Float_32", "", "##0.###", "", "PID-регулятор насоса P-651. Коэффициент T");
            global.Commands.Add("P651_PID_T", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08E4, "Float_32", "##0.###", "PID-регулятор насоса P-651. Коэффициент T");
            global.Variables.Add("P651_PID_Tune", 0x18, 0x003C, 1, "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-651. Автонастройка");
            global.Commands.Add("P651_PID_Tune", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x090C, "Bool", "Нет;Да", "PID-регулятор насоса P-651. Автонастройка");
        }

        private static void InitializeUnloadingSystem(TGlobal global)
        {
            global.Variables.Add("EM_Unloading_Rejim", 0x18, 0x001F, 1, "Int_16", "", "##0", "", "Режим отгрузки продукта");
            global.Commands.Add("EM_Unloading_PultButton", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08F0, "Bool", "Нет;Да", "Режим отгрузки продукта по командам");
            global.Commands.Add("EM_Unloading_TimeButton", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08F1, "Bool", "Нет;Да", "Режим отгрузки продукта по времени");
            global.Commands.Add("EM_Unloading_MassButton", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08F2, "Bool", "Нет;Да", "Режим отгрузки продукта по массе");
            global.Commands.Add("EM_Unload_Sp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08F3, "Float_32", "##0.###", "Режим отгрузки продукта. Задание");
            global.Variables.Add("EM_Unload_Speed", 0x18, 0x0025, 1, "Float_32", "", "##0.##", " кг./сек.", "Режим отгрузки продукта. Скорость");
            global.Variables.Add("EM_UnloadCounter", 0x18, 0x0027, 1, "Float_32", "", "##0.##", " кг.", "Режим отгрузки продукта. Отгружено");
            global.Commands.Add("EM_Unload_TimeStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08F9, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Пуск");
            global.Commands.Add("EM_Unload_TimeStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08FA, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Стоп");
            global.Variables.Add("EM_UnloadTorirovanieTime", 0x18, 0x002C, 1, "Float_32", "", "##0", " сек.", "Режим отгрузки продукта. Торирование. Время торирования");
            global.Commands.Add("EM_Unload_Torirovanie_Start", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08FE, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Старт");
            global.Commands.Add("EM_Unload_Torirovanie_Pause", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08FF, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Пауза");
            global.Commands.Add("EM_Unload_Torirovanie_Stop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0900, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Стоп");
            global.Commands.Add("EM_Unload_Torirovanie_Mass", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0901, "Float_32", "##0.###", "ежим отгрузки продукта по времени. Тарирование. Масса");
            global.Commands.Add("EM_Unload_Torirovanie_Calculate", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0903, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Пересчитать");
            global.Commands.Add("VLV302_StartButton", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0904, "Bool", "Нет;Да", "Открыть клапан V-302");
            global.Commands.Add("VLV302_StopButton", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0905, "Bool", "Нет;Да", "Закрыть клапан V-302");
            global.Commands.Add("EM_Unload_MassStart", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x090E, "Bool", "Нет;Да", "Режим отгрузки продукта по массе. Старт");
            global.Commands.Add("EM_Unload_MassStop", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x090F, "Bool", "Нет;Да", "Режим отгрузки продукта по массе. Старт");
            global.Commands.Add("EM_Unload_MassSp", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0910, "Float_32", "##0.###", "Режим отгрузки продукта по массе. Масса задания");
        }

        private static void InitializeAdditionalVariables(TGlobal global)
        {
            global.Variables.Add("HE300_StartHeater", 0x14, 0x0040, 1, "Bool", "", "Нет;Да", "", "Включить нагреватель HE300");
            global.Commands.Add("HE300_StartHeater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0790, "Bool", "Нет;Да", "Включить нагреватель HE300");
            global.Variables.Add("HE750_StartHeater", 0x14, 0x0041, 1, "Bool", "", "Нет;Да", "", "Включить нагреватель HE750");
            global.Commands.Add("HE750_StartHeater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0791, "Bool", "Нет;Да", "Включить нагреватель HE750");
            global.Variables.Add("M200_StartMixer", 0x14, 0x0042, 1, "Bool", "", "Нет;Да", "", "Включить миксер M200");
            global.Commands.Add("M200_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0792, "Bool", "Нет;Да", "Включить миксер M200");
            global.Variables.Add("M250_StartMixer", 0x14, 0x0043, 1, "Bool", "", "Нет;Да", "", "Включить миксер M250");
            global.Commands.Add("M250_StartMixer", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0793, "Bool", "Нет;Да", "Включить миксер M250");
            global.Variables.Add("HE800_StartHeater", 0x14, 0x0054, 1, "Bool", "", "Нет;Да", "", "Включить нагреватель HE800");
            global.Commands.Add("HE800_StartHeater", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x07A4, "Bool", "Нет;Да", "Включить нагреватель HE800");
            global.Variables.Add("HE700_Rejim", 0x18, 0x001E, 1, "Int_16", "", "##0", "", "Режим нагревателя HE-100");
            global.Commands.Add("HE700_Rejim", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x08EE, "Int_16", "##0", "Режим нагревателя HE-100");
            global.Variables.Add("P100_SpeedHi", 0x18, 0x0046, 1, "Float_32", "", "##0", " %", "Номинальная скорость P-100");
            global.Commands.Add("P100_SpeedHi", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0916, "Float_32", "##0.###", "Номинальная скорость P-100");
            global.Variables.Add("P100_SpeedLow", 0x18, 0x0048, 1, "Float_32", "", "##0", " %", "Минимальная скорость P-100");
            global.Commands.Add("P100_SpeedLow", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x0918, "Float_32", "##0.###", "Минимальная скорость P-100");
            global.Variables.Add("P100_MinMass", 0x18, 0x004A, 1, "Float_32", "", "##0", " кг.", "Миниальная масса для Р-100");
            global.Commands.Add("P100_MinMass", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x091A, "Float_32", "##0.###", "Миниальная масса для Р-100");
            global.Variables.Add("P400_SpeedHi", 0x18, 0x004C, 1, "Float_32", "", "##0", " %", "Номинальная масса для Р-400");
            global.Commands.Add("P400_SpeedHi", global.Plc_IpAddress, global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers", 0x091C, "Float_32", "##0.###", "Номинальная масса для Р-400");
        }

        private static void InitializeElements(TGlobal global)
        {
            TElementPumpReverse A100 = new TElementPumpReverse(global, "A100", 0x0000, 0x0010, 0x14, 0x0750);
            TElementPt PT103 = new TElementPt(global, "PT103", 0x0000, 0x0018, 0x01, 0x0000);
            TElementPt PT104 = new TElementPt(global, "PT104", 0x0020, 0x0038, 0x01, 0x0020);
            TElementPt PT105 = new TElementPt(global, "PT105", 0x0040, 0x0058, 0x01, 0x0040);
            TElementLt PT205 = new TElementLt(global, "PT205", 0x0020, 0x0038, 0x02, 0x0080);
            TElementLt PT206 = new TElementLt(global, "PT206", 0x0040, 0x0058, 0x02, 0x00A0);
            TElementPt PT304 = new TElementPt(global, "PT304", 0x0000, 0x0018, 0x03, 0x00C0);
            TElementPt PT404 = new TElementPt(global, "PT404", 0x0020, 0x0038, 0x03, 0x00E0);
            TElementPt PT504 = new TElementPt(global, "PT504", 0x0040, 0x0058, 0x03, 0x0100);
            TElementPt PT601 = new TElementPt(global, "PT601", 0x0000, 0x0018, 0x04, 0x0120);
            TElementPt PT606 = new TElementPt(global, "PT606", 0x0020, 0x0038, 0x04, 0x0140);
            TElementPt PT652 = new TElementPt(global, "PT652", 0x0040, 0x0058, 0x04, 0x0160);
            TElementPt PT900 = new TElementPt(global, "PT900", 0x0000, 0x0018, 0x05, 0x0180);
            TElementTt TT102 = new TElementTt(global, "TT102", 0x0020, 0x0038, 0x05, 0x01A0);
            TElementTt TT152 = new TElementTt(global, "TT152", 0x0040, 0x0058, 0x05, 0x01C0);
            TElementTt TT202 = new TElementTt(global, "TT202", 0x0000, 0x0018, 0x06, 0x01E0);
            TElementTt TT252 = new TElementTt(global, "TT252", 0x0020, 0x0038, 0x06, 0x0200);
            TElementTt TT302 = new TElementTt(global, "TT302", 0x0040, 0x0058, 0x06, 0x0220);
            TElementTt TT402 = new TElementTt(global, "TT402", 0x0000, 0x0018, 0x07, 0x0240);
            TElementTt TT502 = new TElementTt(global, "TT502", 0x0020, 0x0038, 0x07, 0x0260);
            TElementTt TT602 = new TElementTt(global, "TT602", 0x0040, 0x0058, 0x07, 0x0280);
            TElementTt TT604 = new TElementTt(global, "TT604", 0x0000, 0x0018, 0x08, 0x02A0);
            TElementLt LT303 = new TElementLt(global, "LT303", 0x0020, 0x0038, 0x08, 0x02C0);
            TElementLt LT150 = new TElementLt(global, "LT150", 0x0040, 0x0058, 0x08, 0x02E0);
            TElementLt LT253 = new TElementLt(global, "LT253", 0x0000, 0x0018, 0x09, 0x0300);
            TElementLt LT403 = new TElementLt(global, "LT403", 0x0020, 0x0038, 0x09, 0x0320);
            TElementLt LT503 = new TElementLt(global, "LT503", 0x0040, 0x0058, 0x09, 0x0340);
            TElementLt LT651 = new TElementLt(global, "LT651", 0x0000, 0x0018, 0x0A, 0x0360);
            TElementPt LALL203 = new TElementPt(global, "PT201", 0x0020, 0x0038, 0x0A, 0x0380);
            TElementTt TT106 = new TElementTt(global, "TT106", 0x0040, 0x0058, 0x0A, 0x03A0);
            TElementLs LAHH101 = new TElementLs(global, "LAHH101", 0x0000, 0x0005, 0x0B, 0x03C0);
            TElementLs LALL103 = new TElementLs(global, "LALL103", 0x0008, 0x000D, 0x0B, 0x03C8);
            TElementLs LAHH151 = new TElementLs(global, "LAHH151", 0x0010, 0x0015, 0x0B, 0x03D0);
            TElementLs LALL153 = new TElementLs(global, "LALL153", 0x0018, 0x001D, 0x0B, 0x03D8);
            TElementLs LAHH653 = new TElementLs(global, "LAHH653", 0x0020, 0x0025, 0x0B, 0x03E0);
            TElementLs LAHH201 = new TElementLs(global, "LAHH201", 0x0028, 0x002D, 0x0B, 0x03E8);
            TElementLs LAHH251 = new TElementLs(global, "LAHH251", 0x0030, 0x0035, 0x0B, 0x03F0);
            TElementLs LAHH301 = new TElementLs(global, "LAHH301", 0x0038, 0x003D, 0x0B, 0x03F8);
            TElementLs LAHH302 = new TElementLs(global, "LAHH302", 0x0040, 0x0045, 0x0B, 0x0400);
            TElementLs LAHH401 = new TElementLs(global, "LAHH401", 0x0048, 0x004D, 0x0B, 0x0408);
            TElementLs LAHH501 = new TElementLs(global, "LAHH501", 0x0050, 0x0055, 0x0B, 0x0410);
            TElementVlv V101 = new TElementVlv(global, "V101", 0x0000, 0x0008, 0x0C, 0x0420);
            TElementVlv V151 = new TElementVlv(global, "V151", 0x0010, 0x0018, 0x0C, 0x0430);
            TElementVlv V152 = new TElementVlv(global, "V152", 0x0020, 0x0028, 0x0C, 0x0440);
            TElementVlv V601 = new TElementVlv(global, "V601", 0x0030, 0x0038, 0x0C, 0x0450);
            TElementVlv V602 = new TElementVlv(global, "V602", 0x0040, 0x0048, 0x0C, 0x0460);
            TElementVlv V302 = new TElementVlv(global, "V302", 0x0050, 0x0058, 0x0C, 0x0470);
            TElementVlv V305 = new TElementVlv(global, "V305", 0x0000, 0x0008, 0x0D, 0x0480);
            TElementVlv V401 = new TElementVlv(global, "V401", 0x0010, 0x0018, 0x0D, 0x0490);
            TElementVlv V501 = new TElementVlv(global, "V501", 0x0020, 0x0028, 0x0D, 0x04A0);
            TElementVlv V801 = new TElementVlv(global, "V801", 0x0030, 0x0038, 0x0D, 0x04B0);
            TElementVlv V803 = new TElementVlv(global, "V803", 0x0040, 0x0048, 0x0D, 0x04C0);
            TElementVlv V505 = new TElementVlv(global, "V505", 0x0050, 0x0058, 0x0D, 0x04D0);
            TElementMixer M100 = new TElementMixer(global, "M100", 0x0000, 0x0008, 0x0E, 0x04E0);
            TElementMixer M150 = new TElementMixer(global, "M150", 0x0010, 0x0018, 0x0E, 0x04F0);
            TElementMixer M200 = new TElementMixer(global, "M200", 0x0020, 0x0028, 0x0E, 0x0500);
            TElementMixer M250 = new TElementMixer(global, "M250", 0x0030, 0x0038, 0x0E, 0x0510);
            TElementMixer M400 = new TElementMixer(global, "M400", 0x0040, 0x0048, 0x0E, 0x0520);
            TElementMixer M500 = new TElementMixer(global, "M500", 0x0050, 0x0058, 0x0E, 0x0530);
            TElementPump P200 = new TElementPump(global, "P200", 0x0000, 0x0008, 0x0F, 0x0540);
            TElementPump P201 = new TElementPump(global, "P201", 0x0010, 0x0018, 0x0F, 0x0550);
            TElementPump P202 = new TElementPump(global, "P202", 0x0020, 0x0028, 0x0F, 0x0560);
            TElementPump P300 = new TElementPump(global, "P300", 0x0030, 0x0038, 0x0F, 0x0570);
            TElementPump P500 = new TElementPump(global, "P500", 0x0040, 0x0048, 0x0F, 0x0580);
            TElementPumpReverse P700 = new TElementPumpReverse(global, "P700", 0x0050, 0x0058, 0x0F, 0x0590);
            TElementQM QM400 = new TElementQM(global, "QM400", 0x0000, 0x0008, 0x10, 0x05A0);
            TElementQM QM500 = new TElementQM(global, "QM500", 0x0010, 0x0018, 0x10, 0x05B0);
            TElementQM2 QM401 = new TElementQM2(global, "QM401", 0x0020, 0x0028, 0x10, 0x05C0);
            TElementFM FM601 = new TElementFM(global, "FM601", 0x0030, 0x0040, 0x10, 0x05D0);
            TElementFM FM602 = new TElementFM(global, "FM602", 0x0000, 0x0010, 0x11, 0x0620);
            TElementLt LT301 = new TElementLt(global, "LT301", 0x0030, 0x0048, 0x11, 0x0650);
            TElementHe HE300 = new TElementHe(global, "HE300", 0x0050, 0x0058, 0x11, 0x0670);
            TElementPumpUz P100 = new TElementPumpUz(global, "P100", 0x0000, 0x0010, 0x12, 0x0680);
            TElementPumpUz P400 = new TElementPumpUz(global, "P400", 0x0030, 0x0040, 0x12, 0x06B0);
            TElementFM FM401 = new TElementFM(global, "FM401", 0x0000, 0x0010, 0x13, 0x06F0);
            TElementHe HE700 = new TElementHe(global, "HE700.1", 0x0020, 0x0028, 0x14, 0x0770);
            TElementHe HE750 = new TElementHe(global, "HE750", 0x0030, 0x0038, 0x14, 0x0780);
            TElementHe HE800 = new TElementHe(global, "HE800", 0x0044, 0x004C, 0x14, 0x0794);
            TElementPumpUz P602 = new TElementPumpUz(global, "P602", 0x0000, 0x0010, 0x15, 0x07B0);
            TElementPumpUz P601 = new TElementPumpUz(global, "P601", 0x0020, 0x0030, 0x15, 0x07D0);
            TElementPumpUz M600 = new TElementPumpUz(global, "M600", 0x0040, 0x0050, 0x15, 0x07F0);
            TElementPumpUz P651 = new TElementPumpUz(global, "P651", 0x0000, 0x0010, 0x16, 0x0810);
            TElementPt PT604 = new TElementPt(global, "PT604", 0x0020, 0x0038, 0x16, 0x0830);
            TElementHe HE700_2 = new TElementHe(global, "HE700.2", 0x000E, 0x0016, 0x18, 0x08DE);
        }

        public static List<TWordsArea> GetModbusAreas()
        {
            return _modbusAreas;
        }
    }
}