using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProtolScadaRemake
{
    public static class ModbusInitializer
    {
        // Создает ВСЕ переменные Modbus, которые нужны вашим WPF элементам
        public static void InitializeAllVariables(TGlobal global)
        {
            try
            {
                Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ MODBUS ДЛЯ WPF ЭЛЕМЕНТОВ ===");

                // Настройки Modbus (из вашего старого проекта)
                global.Plc_IpAddress = "192.168.100.5";
                global.Plc_PortNum = 502;
                global.Plc_DeviceAddress = 1;

                // Группа 0x01 - 0x18 для разных систем (как в старом проекте)

                // 1. Системные переменные и счетчики (Группа 0x12)
                InitializeSystemVariables(global);

                // 2. Датчики T-400 (ГГД система)
                InitializeT400Sensors(global);

                // 3. Датчики T-500 (ГГД система)  
                InitializeT500Sensors(global);

                // 4. Датчики T-100, T-150, T-200, T-700
                InitializeT100T150T200T700Sensors(global);

                // 5. Датчики давления PT
                InitializePTSensors(global);

                // 6. Датчики температуры TT
                InitializeTTSensors(global);

                // 7. Датчики уровня LT
                InitializeLTSensors(global);

                // 8. Дискретные датчики уровня LAHH/LALL
                InitializeDiscreteLevelSensors(global);

                // 9. Клапаны V
                InitializeValves(global);

                // 10. Миксеры M
                InitializeMixers(global);

                // 11. Насосы P
                InitializePumps(global);

                // 12. Счетчики QM, расходомеры FM
                InitializeCountersAndFlowMeters(global);

                // 13. Нагреватели HE
                InitializeHeaters(global);

                // 14. Шнек A100
                InitializeA100Screw(global);

                // 15. Эмульсификатор M600
                InitializeM600Emulsifier(global);

                // 16. Рецепты ГРО, ТС, ЭМ
                InitializeRecipes(global);

                // 17. PID регуляторы
                InitializePIDControllers(global);

                // 18. Отгрузка продукции
                InitializeUnloadingSystem(global);

                // 19. Дополнительные переменные
                InitializeAdditionalVariables(global);

                Debug.WriteLine($"=== СОЗДАНО ВСЕГО ПЕРЕМЕННЫХ: {GetVariableCount(global.Variables)} ===");
                Debug.WriteLine($"=== СОЗДАНО ВСЕГО КОМАНД: {GetCommandCount(global.Commands)} ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
            }
        }

        private static void InitializeSystemVariables(TGlobal global)
        {
            // Счетчик продукции (как в старом проекте)
            global.Variables.Add("SmenaProductCounter_Reset", 0x12, 0x0020, 1,
                "Bool", "", "Нет;Да", "", "Сброс счетчика продукции за смену");
            global.Commands.Add("SmenaProductCounter_Reset", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A0, "Bool", "Нет;Да", "Сброс счетчика продукции за смену");

            global.Variables.Add("SmenaProductCouner_Volume", 0x12, 0x0021, 1,
                "Float_32", "", "##0.#", " кг.", "Счетчик продукции за смену");

            global.Variables.Add("TotalProductCounter_Reset", 0x12, 0x0023, 1,
                "Bool", "", "Нет;Да", "", "Сброс общего счетчика продукции");
            global.Commands.Add("TotalProductCounter_Reset", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A3, "Bool", "Нет;Да", "Сброс общего счетчика продукции");

            global.Variables.Add("TotalProductCouner_Volume", 0x12, 0x0024, 1,
                "Float_32", "", "##0.#", " кг.", "Общий счетчик продукции");
        }

        private static void InitializeT100T150T200T700Sensors(TGlobal global)
        {
            // Ёмкость T-100
            global.Variables.Add("T100_StartMixer", 0x13, 0x0030, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер Т100");
            global.Commands.Add("T100_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0720, "Bool", "Нет;Да", "Включить миксер Т100");

            // Ёмкость T-150
            global.Variables.Add("T150_StartMixer", 0x13, 0x0031, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер Т150");
            global.Commands.Add("T150_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0721, "Bool", "Нет;Да", "Включить миксер Т150");

            // Тензодатчики
            global.Variables.Add("WIT100_Volume", 0x13, 0x0032, 1,
                "Float_32", "", "##0.#", " кг.", "Вес в ёмкости Т100");
            global.Variables.Add("WIT200_Volume", 0x13, 0x0034, 1,
                "Float_32", "", "##0.#", " кг.", "Вес в ёмкости Т200");
            global.Variables.Add("WIT700_Volume", 0x13, 0x0036, 1,
                "Float_32", "", "##0.#", " кг.", "Вес в ёмкости Т700");
        }

        private static void InitializePTSensors(TGlobal global)
        {
            // Датчики давления PT (все из старого проекта)
            CreateAnalogSensor(global, "PT103", "Float_32", "бар", 0x01, 0x0000,
                "Датчик давления PT103", 0x0000, 0x0018);
            CreateAnalogSensor(global, "PT104", "Float_32", "бар", 0x01, 0x0020,
                "Датчик давления PT104", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT105", "Float_32", "бар", 0x01, 0x0040,
                "Датчик давления PT105", 0x0040, 0x0058);
            CreateAnalogSensor(global, "PT205", "Float_32", "бар", 0x02, 0x0080,
                "Датчик давления PT205", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT206", "Float_32", "бар", 0x02, 0x00A0,
                "Датчик давления PT206", 0x0040, 0x0058);
            CreateAnalogSensor(global, "PT304", "Float_32", "бар", 0x03, 0x00C0,
                "Датчик давления PT304", 0x0000, 0x0018);
            CreateAnalogSensor(global, "PT404", "Float_32", "бар", 0x03, 0x00E0,
                "Датчик давления PT404", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT504", "Float_32", "бар", 0x03, 0x0100,
                "Датчик давления PT504", 0x0040, 0x0058);
            CreateAnalogSensor(global, "PT601", "Float_32", "бар", 0x04, 0x0120,
                "Датчик давления PT601", 0x0000, 0x0018);
            CreateAnalogSensor(global, "PT606", "Float_32", "бар", 0x04, 0x0140,
                "Датчик давления PT606", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT652", "Float_32", "бар", 0x04, 0x0160,
                "Датчик давления PT652", 0x0040, 0x0058);
            CreateAnalogSensor(global, "PT900", "Float_32", "бар", 0x05, 0x0180,
                "Датчик давления PT900", 0x0000, 0x0018);
            CreateAnalogSensor(global, "PT201", "Float_32", "бар", 0x0A, 0x0380,
                "Датчик давления PT201", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT604", "Float_32", "бар", 0x16, 0x0830,
                "Датчик давления PT604", 0x0020, 0x0038);
        }

        private static void InitializeTTSensors(TGlobal global)
        {
            // Датчики температуры TT
            CreateAnalogSensor(global, "TT102", "Float_32", "°C", 0x05, 0x01A0,
                "Датчик температуры TT102", 0x0020, 0x0038);
            CreateAnalogSensor(global, "TT152", "Float_32", "°C", 0x05, 0x01C0,
                "Датчик температуры TT152", 0x0040, 0x0058);
            CreateAnalogSensor(global, "TT202", "Float_32", "°C", 0x06, 0x01E0,
                "Датчик температуры TT202", 0x0000, 0x0018);
            CreateAnalogSensor(global, "TT252", "Float_32", "°C", 0x06, 0x0200,
                "Датчик температуры TT252", 0x0020, 0x0038);
            CreateAnalogSensor(global, "TT302", "Float_32", "°C", 0x06, 0x0220,
                "Датчик температуры TT302", 0x0040, 0x0058);
            CreateAnalogSensor(global, "TT402", "Float_32", "°C", 0x07, 0x0240,
                "Датчик температуры TT402", 0x0000, 0x0018);
            CreateAnalogSensor(global, "TT502", "Float_32", "°C", 0x07, 0x0260,
                "Датчик температуры TT502", 0x0020, 0x0038);
            CreateAnalogSensor(global, "TT602", "Float_32", "°C", 0x07, 0x0280,
                "Датчик температуры TT602", 0x0040, 0x0058);
            CreateAnalogSensor(global, "TT604", "Float_32", "°C", 0x08, 0x02A0,
                "Датчик температуры TT604", 0x0000, 0x0018);
            CreateAnalogSensor(global, "TT106", "Float_32", "°C", 0x0A, 0x03A0,
                "Датчик температуры TT106", 0x0040, 0x0058);
        }

        private static void InitializeLTSensors(TGlobal global)
        {
            // Датчики уровня LT
            CreateAnalogSensor(global, "LT303", "Float_32", "%", 0x08, 0x02C0,
                "Датчик уровня LT303", 0x0020, 0x0038);
            CreateAnalogSensor(global, "LT150", "Float_32", "%", 0x08, 0x02E0,
                "Датчик уровня LT150", 0x0040, 0x0058);
            CreateAnalogSensor(global, "LT253", "Float_32", "%", 0x09, 0x0300,
                "Датчик уровня LT253", 0x0000, 0x0018);
            CreateAnalogSensor(global, "LT403", "Float_32", "%", 0x09, 0x0320,
                "Датчик уровня LT403", 0x0020, 0x0038);
            CreateAnalogSensor(global, "LT503", "Float_32", "%", 0x09, 0x0340,
                "Датчик уровня LT503", 0x0040, 0x0058);
            CreateAnalogSensor(global, "LT651", "Float_32", "%", 0x0A, 0x0360,
                "Датчик уровня LT651", 0x0000, 0x0018);
            CreateAnalogSensor(global, "LT301", "Float_32", "%", 0x11, 0x0650,
                "Датчик уровня LT301", 0x0030, 0x0048);
        }

        private static void InitializeDiscreteLevelSensors(TGlobal global)
        {
            // Дискретные датчики уровня
            CreateDiscreteSensor(global, "LAHH101", 0x0B, 0x03C0,
                "Датчик уровня LAHH101", 0x0000, 0x0005);
            CreateDiscreteSensor(global, "LALL103", 0x0B, 0x03C8,
                "Датчик уровня LALL103", 0x0008, 0x000D);
            CreateDiscreteSensor(global, "LAHH151", 0x0B, 0x03D0,
                "Датчик уровня LAHH151", 0x0010, 0x0015);
            CreateDiscreteSensor(global, "LALL153", 0x0B, 0x03D8,
                "Датчик уровня LALL153", 0x0018, 0x001D);
            CreateDiscreteSensor(global, "LAHH653", 0x0B, 0x03E0,
                "Датчик уровня LAHH653", 0x0020, 0x0025);
            CreateDiscreteSensor(global, "LAHH201", 0x0B, 0x03E8,
                "Датчик уровня LAHH201", 0x0028, 0x002D);
            CreateDiscreteSensor(global, "LAHH251", 0x0B, 0x03F0,
                "Датчик уровня LAHH251", 0x0030, 0x0035);
            CreateDiscreteSensor(global, "LAHH301", 0x0B, 0x03F8,
                "Датчик уровня LAHH301", 0x0038, 0x003D);
            CreateDiscreteSensor(global, "LAHH302", 0x0B, 0x0400,
                "Датчик уровня LAHH302", 0x0040, 0x0045);
            CreateDiscreteSensor(global, "LAHH401", 0x0B, 0x0408,
                "Датчик уровня LAHH401", 0x0048, 0x004D);
            CreateDiscreteSensor(global, "LAHH501", 0x0B, 0x0410,
                "Датчик уровня LAHH501", 0x0050, 0x0055);
        }

        private static void InitializeValves(TGlobal global)
        {
            // Клапаны V
            CreateValve(global, "V101", 0x0C, 0x0420,
                "Клапан V101", 0x0000, 0x0008);
            CreateValve(global, "V151", 0x0C, 0x0430,
                "Клапан V151", 0x0010, 0x0018);
            CreateValve(global, "V152", 0x0C, 0x0440,
                "Клапан V152", 0x0020, 0x0028);
            CreateValve(global, "V601", 0x0C, 0x0450,
                "Клапан V601", 0x0030, 0x0038);
            CreateValve(global, "V602", 0x0C, 0x0460,
                "Клапан V602", 0x0040, 0x0048);
            CreateValve(global, "V302", 0x0C, 0x0470,
                "Клапан V302", 0x0050, 0x0058);
            CreateValve(global, "V305", 0x0D, 0x0480,
                "Клапан V305", 0x0000, 0x0008);
            CreateValve(global, "V401", 0x0D, 0x0490,
                "Клапан V401", 0x0010, 0x0018);
            CreateValve(global, "V501", 0x0D, 0x04A0,
                "Клапан V501", 0x0020, 0x0028);
            CreateValve(global, "V801", 0x0D, 0x04B0,
                "Клапан V801", 0x0030, 0x0038);
            CreateValve(global, "V803", 0x0D, 0x04C0,
                "Клапан V803", 0x0040, 0x0048);
            CreateValve(global, "V505", 0x0D, 0x04D0,
                "Клапан V505", 0x0050, 0x0058);
        }

        private static void InitializeMixers(TGlobal global)
        {
            // Миксеры M
            CreateMixer(global, "M100", 0x0E, 0x04E0,
                "Миксер M100", 0x0000, 0x0008);
            CreateMixer(global, "M150", 0x0E, 0x04F0,
                "Миксер M150", 0x0010, 0x0018);
            CreateMixer(global, "M200", 0x0E, 0x0500,
                "Миксер M200", 0x0020, 0x0028);
            CreateMixer(global, "M250", 0x0E, 0x0510,
                "Миксер M250", 0x0030, 0x0038);
            CreateMixer(global, "M400", 0x0E, 0x0520,
                "Миксер M400", 0x0040, 0x0048);
            CreateMixer(global, "M500", 0x0E, 0x0530,
                "Миксер M500", 0x0050, 0x0058);
        }

        private static void InitializePumps(TGlobal global)
        {
            // Насосы P (обычные)
            CreatePump(global, "P200", 0x0F, 0x0540,
                "Насос P200", 0x0000, 0x0008);
            CreatePump(global, "P201", 0x0F, 0x0550,
                "Насос P201", 0x0010, 0x0018);
            CreatePump(global, "P202", 0x0F, 0x0560,
                "Насос P202", 0x0020, 0x0028);
            CreatePump(global, "P300", 0x0F, 0x0570,
                "Насос P300", 0x0030, 0x0038);
            CreatePump(global, "P500", 0x0F, 0x0580,
                "Насос P500", 0x0040, 0x0048);

            // Реверсивные насосы
            CreatePumpReverse(global, "P700", 0x0F, 0x0590,
                "Насос P700", 0x0050, 0x0058);

            // Насосы с управлением
            CreatePumpWithControl(global, "P100", 0x12, 0x0680,
                "Насос P100", 0x0000, 0x0010);
            CreatePumpWithControl(global, "P400", 0x12, 0x06B0,
                "Насос P400", 0x0030, 0x0040);
            CreatePumpWithControl(global, "P602", 0x15, 0x07B0,
                "Насос P602", 0x0000, 0x0010);
            CreatePumpWithControl(global, "P601", 0x15, 0x07D0,
                "Насос P601", 0x0020, 0x0030);
            CreatePumpWithControl(global, "P651", 0x16, 0x0810,
                "Насос P651", 0x0000, 0x0010);
        }

        private static void InitializeCountersAndFlowMeters(TGlobal global)
        {
            // Счетчики
            CreateCounter(global, "QM400", "Float_32", "л", 0x10, 0x05A0,
                "Счетчик воды QM-400", 0x0000, 0x0008);
            CreateCounter(global, "QM500", "Float_32", "л", 0x10, 0x05B0,
                "Счетчик воды QM-500", 0x0010, 0x0018);

            // Расходомеры
            CreateAnalogSensor(global, "QM401", "Float_32", "кг/ч", 0x10, 0x05C0,
                "Расходомер QM401", 0x0020, 0x0028);
            CreateAnalogSensor(global, "FM601", "Float_32", "кг/ч", 0x10, 0x05D0,
                "Расходомер FM601", 0x0030, 0x0040);
            CreateAnalogSensor(global, "FM602", "Float_32", "кг/ч", 0x11, 0x0620,
                "Расходомер FM602", 0x0000, 0x0010);
            CreateAnalogSensor(global, "FM401", "Float_32", "кг/ч", 0x13, 0x06F0,
                "Расходомер FM401", 0x0000, 0x0010);
        }

        private static void InitializeHeaters(TGlobal global)
        {
            // Нагреватели HE
            CreateHeater(global, "HE300", 0x11, 0x0670,
                "Нагреватель HE300", 0x0050, 0x0058);
            CreateHeater(global, "HE700", 0x14, 0x0770,
                "Нагреватель HE700.1", 0x0020, 0x0028);
            CreateHeater(global, "HE750", 0x14, 0x0780,
                "Нагреватель HE750", 0x0030, 0x0038);
            CreateHeater(global, "HE800", 0x14, 0x0794,
                "Нагреватель HE800", 0x0044, 0x004C);
            CreateHeater(global, "HE700_2", 0x18, 0x08DE,
                "Нагреватель HE700.2", 0x000E, 0x0016);

            // Команды управления нагревателями
            global.Variables.Add("HE300_StartHeater", 0x14, 0x0040, 1,
                "Bool", "", "Нет;Да", "", "Включить нагреватель HE300");
            global.Commands.Add("HE300_StartHeater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0790, "Bool", "Нет;Да", "Включить нагреватель HE300");

            global.Variables.Add("HE750_StartHeater", 0x14, 0x0041, 1,
                "Bool", "", "Нет;Да", "", "Включить нагреватель HE750");
            global.Commands.Add("HE750_StartHeater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0791, "Bool", "Нет;Да", "Включить нагреватель HE750");

            global.Variables.Add("HE800_StartHeater", 0x14, 0x0054, 1,
                "Bool", "", "Нет;Да", "", "Включить нагреватель HE800");
            global.Commands.Add("HE800_StartHeater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x07A4, "Bool", "Нет;Да", "Включить нагреватель HE800");
        }

        private static void InitializeA100Screw(TGlobal global)
        {
            // A100 - Шнек
            CreateAnalogSensor(global, "A100_Speed", "Float_32", "%", 0x14, 0x0750,
                "Скорость шнека А-100", 0x0000, 0x0010);
            global.Commands.Add("A100_Speed", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0738, "Float_32", "##0.#", "Скорость шнека А-100");

            // Дополнительные переменные для A100
            global.Variables.Add("GRO_Recept_A100BlockTemp", 0x18, 0x0042, 1,
                "Float_32", "", "##0", " °С", "Температура блокировки А-100");
            global.Commands.Add("GRO_Recept_A100BlockTemp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0912, "Float_32", "##0.###", "Температура блокировки А-100");

            global.Variables.Add("GRO_Recept_A100BlockWeith", 0x18, 0x0044, 1,
                "Float_32", "", "##0", " кг.", "Масса блокировки А-100");
            global.Commands.Add("GRO_Recept_A100BlockWeith", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0914, "Float_32", "##0.###", "Масса блокировки А-100");
        }

        private static void InitializeM600Emulsifier(TGlobal global)
        {
            // Эмульсификатор M600
            CreatePumpWithControl(global, "M600", 0x15, 0x07F0,
                "Эмульсификатор M600", 0x0040, 0x0050);

            global.Variables.Add("EM_M600_SpeedSp", 0x18, 0x0036, 1,
                "Float_32", "", "##0", " %", "Скорость эмульсификатора М-600");
            global.Commands.Add("EM_M600_SpeedSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0906, "Float_32", "##0.###", "Скорость эмульсификатора М-600");
        }

        private static void InitializeT400Sensors(TGlobal global)
        {
            // Т-400 Управление
            global.Variables.Add("T400_StartMixer", 0x12, 0x0026, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер Т400");
            global.Commands.Add("T400_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A6, "Bool", "Нет;Да", "Включить миксер Т400");

            global.Variables.Add("T400_StartWater", 0x12, 0x0028, 1,
                "Bool", "", "Нет;Да", "", "Включить наполнение Т400");
            global.Commands.Add("T400_StartWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A8, "Bool", "Нет;Да", "Включить наполнение Т400");

            global.Variables.Add("T400_StopWater", 0x12, 0x0029, 1,
                "Bool", "", "Нет;Да", "", "Отключить наполнение Т400");
            global.Commands.Add("T400_StopWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A9, "Bool", "Нет;Да", "Отключить наполнение Т400");

            global.Variables.Add("T400_SpWater", 0x12, 0x002A, 1,
                "Float_32", "", "##0.#", " л.", "Объем наполнения Т400");
            global.Commands.Add("T400_SpWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06AA, "Float_32", "##0.#", "Объем наполнения Т400");

            // Датчики T-400
            CreateAnalogSensor(global, "TT402", "Float_32", "°C", 0x07, 0x0240,
                "Датчик температуры TT402", 0x0000, 0x0018);
            CreateAnalogSensor(global, "LT403", "Float_32", "%", 0x09, 0x0320,
                "Датчик уровня LT403", 0x0020, 0x0038);
            CreateAnalogSensor(global, "PT404", "Float_32", "бар", 0x03, 0x00E0,
                "Датчик давления PT404", 0x0020, 0x0038);
            CreateDiscreteSensor(global, "LAHH401", 0x0B, 0x0408,
                "Датчик уровня LAHH401", 0x0048, 0x004D);
            CreateValve(global, "V401", 0x0D, 0x0490,
                "Клапан V-401", 0x0010, 0x0018);
            CreatePumpWithControl(global, "P400", 0x12, 0x06B0,
                "Насос P-400", 0x0030, 0x0040);
            CreateMixer(global, "M400", 0x0E, 0x0520,
                "Миксер M-400", 0x0040, 0x0048);
            CreateCounter(global, "QM400", "Float_32", "л", 0x10, 0x05A0,
                "Счетчик воды QM-400", 0x0000, 0x0008);
        }

        private static void InitializeT500Sensors(TGlobal global)
        {
            // Т-500 Управление
            global.Variables.Add("T500_StartMixer", 0x12, 0x0027, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер Т500");
            global.Commands.Add("T500_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06A7, "Bool", "Нет;Да", "Включить миксер Т500");

            global.Variables.Add("T500_StartWater", 0x12, 0x002C, 1,
                "Bool", "", "Нет;Да", "", "Включить наполнение Т500");
            global.Commands.Add("T500_StartWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06AC, "Bool", "Нет;Да", "Включить наполнение Т500");

            global.Variables.Add("T500_StopWater", 0x12, 0x002D, 1,
                "Bool", "", "Нет;Да", "", "Отключить наполнение Т500");
            global.Commands.Add("T500_StopWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06AD, "Bool", "Нет;Да", "Отключить наполнение Т500");

            global.Variables.Add("T500_SpWater", 0x12, 0x002E, 1,
                "Float_32", "", "##0.#", " л.", "Объем наполнения Т500");
            global.Commands.Add("T500_SpWater", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x06AE, "Float_32", "##0.#", "Объем наполнения Т500");

            // Датчики T-500
            CreateAnalogSensor(global, "TT502", "Float_32", "°C", 0x07, 0x0260,
                "Датчик температуры TT502", 0x0020, 0x0038);
            CreateAnalogSensor(global, "LT503", "Float_32", "%", 0x09, 0x0340,
                "Датчик уровня LT503", 0x0040, 0x0058);
            CreateAnalogSensor(global, "PT504", "Float_32", "бар", 0x03, 0x0100,
                "Датчик давления PT504", 0x0040, 0x0058);
            CreateDiscreteSensor(global, "LAHH501", 0x0B, 0x0410,
                "Датчик уровня LAHH501", 0x0050, 0x0055);
            CreateValve(global, "V501", 0x0D, 0x04A0,
                "Клапан V-501", 0x0020, 0x0028);
            CreatePumpReverse(global, "P500", 0x0F, 0x0580,
                "Насос P-500", 0x0040, 0x0048);
            CreateMixer(global, "M500", 0x0E, 0x0530,
                "Миксер M-500", 0x0050, 0x0058);
            CreateCounter(global, "QM500", "Float_32", "л", 0x10, 0x05B0,
                "Счетчик воды QM-500", 0x0010, 0x0018);
        }

        private static void InitializeRecipes(TGlobal global)
        {
            // Рецепт ГРО
            InitializeGroRecipe(global);

            // Рецепт ТС
            InitializeTcRecipe(global);

            // Рецепт ЭМ
            InitializeEmRecipe(global);
        }

        private static void InitializeGroRecipe(TGlobal global)
        {
            global.Variables.Add("GRO_Recept_Selitra", 0x16, 0x0040, 1,
                "Float_32", "", "##0.##", " %", "Рецепт ГРО. Объем селитры");
            global.Commands.Add("GRO_Recept_Selitra", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0850, "Float_32", "##0.#", "Рецепт ГРО. Объем селитры");

            global.Variables.Add("GRO_Recept_Water", 0x16, 0x0042, 1,
                "Float_32", "", "##0.##", " %", "Рецепт ГРО. Объем воды");
            global.Commands.Add("GRO_Recept_Water", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0852, "Float_32", "##0.##", "Рецепт ГРО. Объем воды");

            global.Variables.Add("GRO_Recept_KislotaEnable", 0x16, 0x0044, 1,
                "Bool", "", "Нет;Да", "", "Рецепт ГРО. Использовать кислоту");
            global.Commands.Add("GRO_Recept_KislotaEnable", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0854, "Bool", "Нет;Да", "Рецепт ГРО. Использовать кислоту");

            global.Variables.Add("GRO_Recept_Kislota", 0x16, 0x0045, 1,
                "Float_32", "", "##0.##", " %", "Рецепт ГРО. Объем кислоты");
            global.Commands.Add("GRO_Recept_Kislota", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0855, "Float_32", "##0.##", "Рецепт ГРО. Объем кислоты");

            global.Variables.Add("GRO_Recept_Tmax", 0x16, 0x0047, 1,
                "Float_32", "", "##0.##", " °С", "Рецепт ГРО. Максимальная температура.");
            global.Commands.Add("GRO_Recept_Tmax", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0857, "Float_32", "##0.##", "Рецепт ГРО. Максимальная температура.");

            global.Variables.Add("GRO_Recept_Tmin", 0x16, 0x0049, 1,
                "Float_32", "", "##0.##", " °С", "Рецепт ГРО. Минимальная температура.");
            global.Commands.Add("GRO_Recept_Tmin", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0859, "Float_32", "##0.##", "Рецепт ГРО. Минимальная температура.");

            global.Variables.Add("GRO_Rejim", 0x16, 0x004B, 1,
                "Int_16", "", "##0", "", "Режим подготовки ГРО");
            global.Variables.Add("GRO_AutoMassSp", 0x16, 0x004C, 1,
                "Float_32", "", "##0.##", " кг.", "Подготовка ГРО. Автомат. Задание массы.");
            global.Commands.Add("GRO_AutoMassSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x085C, "Float_32", "##0.##", "Подготовка ГРО. Автомат. Задание массы.");

            // Управление режимами ГРО
            global.Commands.Add("GRO_RejimToOff", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x085E, "Bool", "Нет;Да", "Подготовка ГРО. Режим OFF.");
            global.Commands.Add("GRO_RejimToManual", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x085F, "Bool", "Нет;Да", "Подготовка ГРО. Режим Полуавтомат.");
            global.Commands.Add("GRO_RejimToAuto", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0860, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат.");

            // Полуавтоматический режим ГРО
            global.Commands.Add("GRO_Manual_Selitra_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0861, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения селитры.");
            global.Commands.Add("GRO_Manual_Stop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0862, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Стоп.");
            global.Commands.Add("GRO_Manual_Pause", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0863, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пауза.");

            global.Variables.Add("GRO_ManualSelitraCounter", 0x16, 0x0054, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса селитры.");
            global.Variables.Add("GRO_ManualSelitraCounterSp", 0x16, 0x0056, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Нужная масса селитры.");
            global.Commands.Add("GRO_ManualSelitraCounterSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0866, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса селитры.");

            global.Commands.Add("GRO_Manual_Water_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0868, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения воды.");
            global.Commands.Add("GRO_ManualWaterCounterSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0869, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса воды.");
            global.Variables.Add("GRO_ManualWaterCounter", 0x16, 0x005B, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса воды.");

            global.Commands.Add("GRO_Manual_Kislota_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x086D, "Bool", "Нет;Да", "Подготовка ГРО. Режим полуавто. Пуск наполнения кислоты.");
            global.Commands.Add("GRO_ManualKislotaCounterSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x086E, "Float_32", "##0.#", "Подготовка ГРО. Полуатомат. Нужная масса кислоты.");
            global.Variables.Add("GRO_ManualKislotaCounter", 0x17, 0x0000, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Полуатомат. Набранная масса кислоты.");

            // Автоматический режим ГРО
            global.Commands.Add("GRO_Auto_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0872, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Пуск.");
            global.Commands.Add("GRO_Auto_Stop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0873, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Стоп.");
            global.Commands.Add("GRO_Auto_Pause", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0874, "Bool", "Нет;Да", "Подготовка ГРО. Режим Автомат. Пауза.");

            global.Variables.Add("GRO_AutoSelitraSp", 0x17, 0x0005, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса селитры.");
            global.Variables.Add("GRO_AutoSelitraCurrent", 0x17, 0x0007, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса селитры.");
            global.Variables.Add("GRO_AutoWaterSp", 0x17, 0x0009, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса воды.");
            global.Variables.Add("GRO_AutoWaterCurrent", 0x17, 0x000B, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса воды.");
            global.Variables.Add("GRO_AutoKislotaSp", 0x17, 0x000D, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Нужная масса кислоты.");
            global.Variables.Add("GRO_AutoKislotaCurrent", 0x17, 0x000F, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка ГРО. Автомат. Текущая масса кислоты.");

            // Транспортировка ГРО
            global.Commands.Add("GRO_TransportStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0881, "Bool", "Нет;Да", "Подготовка ГРО. Включить перекачку в Т-150.");
            global.Commands.Add("GRO_TransportStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0882, "Bool", "Нет;Да", "Подготовка ГРО. Отключить перекачку в Т-150.");

            global.Variables.Add("GRO_Recept_TmaxDelta", 0x17, 0x0013, 1,
                "Float_32", "", "##0.#", " °С", "Рецепт подготовки ГРО. Дельта максимальной температуры.");
            global.Commands.Add("GRO_Recept_TmaxDelta", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0883, "Float_32", "##0.#", "Рецепт подготовки ГРО. Дельта максимальной температуры.");
        }

        private static void InitializeTcRecipe(TGlobal global)
        {
            global.Variables.Add("TC_Recept_Disel", 0x17, 0x0015, 1,
                "Float_32", "", "##0.##", " %", "Рецепт подготовки TC. Объем дизеля");
            global.Commands.Add("TC_Recept_Disel", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0885, "Float_32", "##0.##", "Рецепт подготовки TC. Объем дизеля");

            global.Variables.Add("TC_Recept_Emulgator", 0x17, 0x0017, 1,
                "Float_32", "", "##0.##", " %", "Рецепт подготовки TC. Объем эмульгатора");
            global.Commands.Add("TC_Recept_Emulgator", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0887, "Float_32", "##0.##", "Рецепт подготовки TC. Объем эмульгатора");

            global.Variables.Add("TC_Recept_Temperature_T200", 0x17, 0x0019, 1,
                "Float_32", "", "##0.##", " °С", "Рецепт подготовки TC. Температура в Т-200");
            global.Commands.Add("TC_Recept_Temperature_T200", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0889, "Float_32", "##0.##", "Рецепт подготовки TC. Температура в Т-200");

            global.Variables.Add("TC_Recept_Temperature_T250", 0x17, 0x001B, 1,
                "Float_32", "", "##0.##", " °С", "Рецепт подготовки TC. Температура в Т-250");
            global.Commands.Add("TC_Recept_Temperature_T250", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x088B, "Float_32", "##0.##", "Рецепт подготовки TC. Температура в Т-250");

            global.Variables.Add("TC_Rejim", 0x17, 0x001D, 1,
                "Int_16", "", "##0", "", "Режим подготовки TC");

            // Управление режимами ТС
            global.Commands.Add("TC_RejimToOff", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x088E, "Bool", "Нет;Да", "Подготовка TC. Режим OFF.");
            global.Commands.Add("TC_RejimToManual", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x088F, "Bool", "Нет;Да", "Подготовка TC. Режим Полуавтомат.");
            global.Commands.Add("TC_RejimToAuto", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0890, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.");

            // Полуавтоматический режим ТС
            global.Commands.Add("TC_ManualStartDisel", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0891, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат. Пуск дизеля");
            global.Commands.Add("TC_ManualStartEmulgator", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0892, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат.Пуск эмульгатора");
            global.Commands.Add("TC_ManualStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0893, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат.Останов");
            global.Commands.Add("TC_ManualPause", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0894, "Bool", "Нет;Да", "Подготовка TC. Режим Полуатомат. Пауза");

            // Автоматический режим ТС
            global.Commands.Add("TC_AutolStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0895, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.Пуск");
            global.Commands.Add("TC_AutoStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0896, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат.Останов");
            global.Commands.Add("TC_AutoPause", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0897, "Bool", "Нет;Да", "Подготовка TC. Режим Автомат. Пауза");

            // Транспортировка ТС
            global.Commands.Add("TC_TransportStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0898, "Bool", "Нет;Да", "Подготовка TC. Включить перекачку в Т-250.");
            global.Commands.Add("TC_TransportStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0899, "Bool", "Нет;Да", "Подготовка TC. Отключить перекачку в Т-250.");

            // Переменные ТС
            global.Variables.Add("TC_ManualDiselCurrent", 0x17, 0x002A, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуавтомат. Текущая масса дизельного топлива.");
            global.Variables.Add("TC_ManualEmulgatorCurrent", 0x17, 0x002C, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуавтомат. Текущая масса эмульгатора.");
            global.Variables.Add("TC_AutoDiselCurrent", 0x17, 0x002E, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Текущая масса дизельного топлива.");
            global.Variables.Add("TC_AutoEmulgatorCurrent", 0x17, 0x0030, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Текущая масса эмульгатора.");
            global.Variables.Add("TC_ManualDiselSp", 0x17, 0x0032, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуатомат. Требуемая масса дизельного топлива.");
            global.Commands.Add("TC_ManualDiselSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08A2, "Float_32", "##0.#", "Подготовка TC. Полуатомат. Требуемая масса дизельного топлива.");
            global.Variables.Add("TC_ManualEmulgatorSp", 0x17, 0x0034, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Полуатомат. Требуемая масса эмульгатора.");
            global.Commands.Add("TC_ManualEmulgatorSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08A4, "Float_32", "##0.#", "Подготовка TC. Полуатомат. Требуемая масса эмульгатора.");
            global.Variables.Add("TC_AutoDiselSp", 0x17, 0x0036, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса дизельного топлива.");
            global.Variables.Add("TC_AutoEmulgatorSp", 0x17, 0x0038, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса эмульгатора.");
            global.Variables.Add("TC_AutoMassSp", 0x17, 0x003A, 1,
                "Float_32", "", "##0.#", " кг.", "Подготовка TC. Автомат. Требуемая масса.");
            global.Commands.Add("TC_AutoMassSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08AA, "Float_32", "##0.#", "Подготовка TC. Автомат. Требуемая масса.");
        }

        private static void InitializeEmRecipe(TGlobal global)
        {
            global.Variables.Add("EM_Recept_GRO", 0x17, 0x003D, 1,
                "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Объем ГРО");
            global.Commands.Add("EM_Recept_GRO", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08AD, "Float_32", "##0.##", "Рецепт производства ЭМ. Объем ГРО");

            global.Variables.Add("EM_Recept_Disel", 0x17, 0x003F, 1,
                "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Объем топливной смеси");
            global.Commands.Add("EM_Recept_Disel", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08AF, "Float_32", "##0.##", "Рецепт производства ЭМ. Объем топливной смеси");

            global.Variables.Add("EM_ReceptDiaeslLast", 0x17, 0x0041, 1,
                "Float_32", "", "##0.##", " кг.", "Рецепт производства ЭМ. Масса топлива промывки");
            global.Commands.Add("EM_ReceptDiaeslLast", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08B1, "Float_32", "##0.##", "Рецепт производства ЭМ. Масса топлива промывки");

            global.Variables.Add("EM_ReceptZatravkaMass", 0x17, 0x0043, 1,
                "Float_32", "", "##0.##", " кг.", "Рецепт производства ЭМ. Масса затравки");
            global.Commands.Add("EM_ReceptZatravkaMass", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08B3, "Float_32", "##0.##", "Рецепт производства ЭМ. Масса затравки");

            global.Variables.Add("EM_ReceptZatravkaTime", 0x17, 0x0045, 1,
                "Int_16", "", "##0", " сек.", "Рецепт производства ЭМ. Время затравки");
            global.Commands.Add("EM_ReceptZatravkaTime", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08B5, "Int_16", "##0", "Рецепт производства ЭМ. Время затравки");

            global.Variables.Add("EM_ReceptWorkLevel", 0x17, 0x0046, 1,
                "Float_32", "", "##0.##", " %", "Рецепт производства ЭМ. Рабочий уровень в Т-650");
            global.Commands.Add("EM_ReceptWorkLevel", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08B6, "Float_32", "##0.##", "Рецепт производства ЭМ. Рабочий уровень в Т-650");

            global.Variables.Add("EM_ReceptAlarmPressure", 0x17, 0x0048, 1,
                "Float_32", "", "##0.##", " Атм.", "Рецепт производства ЭМ. Аварийное давление PT-604");
            global.Commands.Add("EM_ReceptAlarmPressure", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08B8, "Float_32", "##0.##", "Рецепт производства ЭМ. Аварийное давление PT-604");

            global.Variables.Add("EM_Rejim", 0x17, 0x004A, 1,
                "Int_16", "", "##0", "", "Режим подготовки EM");

            // Управление режимами ЭМ
            global.Commands.Add("EM_RejimToOff", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08BB, "Bool", "Нет;Да", "Подготовка ЭМ. Режим OFF.");
            global.Commands.Add("EM_RejimToAuto", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08BC, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат.");

            // Затравка ЭМ
            global.Commands.Add("EM_ZatravkaStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08BD, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Пуск затравки.");
            global.Commands.Add("EM_ZatravkaStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08BE, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Останов затравки.");

            // Автоматический режим ЭМ
            global.Commands.Add("EM_AutoStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08BF, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Пуск.");
            global.Commands.Add("EM_AutoDojat", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C0, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Дожать.");
            global.Commands.Add("EM_AutoStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C1, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Стоп.");
            global.Commands.Add("EM_AutoFastStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C2, "Bool", "Нет;Да", "Подготовка ЭМ. Режим Автомат. Быстрый стоп.");

            global.Variables.Add("EM_AutoMassFlowSp", 0x17, 0x0053, 1,
                "Float_32", "", "##0", " кг./мин.", "Рецепт производства ЭМ. Режим Автомат. Задание производительности.");
            global.Commands.Add("EM_AutoMassFlowSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C3, "Float_32", "##0", "Рецепт производства ЭМ. Режим Автомат. Задание производительности.");

            global.Variables.Add("EM_Recept_ReverseTime", 0x18, 0x002B, 1,
                "Int_16", "", "##0", " сек.", "Время реверса Р-700");
            global.Commands.Add("EM_Recept_ReverseTime", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08FB, "Int_16", "##0", "Время реверса Р-700");
        }

        private static void InitializePIDControllers(TGlobal global)
        {
            // PID P601
            global.Variables.Add("P601_PID_P", 0x17, 0x0056, 1,
                "Float_32", "", "##0.###", "", "PID P601. Коэффициент P");
            global.Commands.Add("P601_PID_P", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C6, "Float_32", "##0.###", "PID P601. Коэффициент P");

            global.Variables.Add("P601_PID_I", 0x17, 0x0058, 1,
                "Float_32", "", "##0.###", "", "PID P601. Коэффициент I");
            global.Commands.Add("P601_PID_I", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08C8, "Float_32", "##0.###", "PID P601. Коэффициент I");

            global.Variables.Add("P601_PID_D", 0x17, 0x005A, 1,
                "Float_32", "", "##0.###", "", "PID P601. Коэффициент D");
            global.Commands.Add("P601_PID_D", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08CA, "Float_32", "##0.###", "PID P601. Коэффициент D");

            global.Variables.Add("P601_PID_T", 0x17, 0x005C, 1,
                "Float_32", "", "##0.###", "", "PID P601. Коэффициент T");
            global.Commands.Add("P601_PID_T", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08CC, "Float_32", "##0.###", "PID P601. Коэффициент T");

            global.Variables.Add("P601_PID_Tune", 0x18, 0x0038, 1,
                "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-601. Автонастройка");
            global.Commands.Add("P601_PID_Tune", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0908, "Bool", "Нет;Да", "PID-регулятор насоса P-601. Автонастройка");

            // PID P602
            global.Variables.Add("P602_PID_P", 0x17, 0x005E, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент P");
            global.Commands.Add("P602_PID_P", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08CE, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент P");

            global.Variables.Add("P602_PID_I", 0x18, 0x0000, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент I");
            global.Commands.Add("P602_PID_I", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08D0, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент I");

            global.Variables.Add("P602_PID_D", 0x18, 0x0002, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент D");
            global.Commands.Add("P602_PID_D", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08D2, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент D");

            global.Variables.Add("P602_PID_T", 0x18, 0x0004, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор насоса P-602. Коэффициент T");
            global.Commands.Add("P602_PID_T", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08D4, "Float_32", "##0.###", "PID-регулятор насоса P-602. Коэффициент T");

            global.Variables.Add("P602_PID_Tune", 0x18, 0x003A, 1,
                "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-602. Автонастройка");
            global.Commands.Add("P602_PID_Tune", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x090A, "Bool", "Нет;Да", "PID-регулятор насоса P-602. Автонастройка");

            // PID M600
            global.Variables.Add("M600_PID_P", 0x18, 0x0006, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент P");
            global.Commands.Add("M600_PID_P", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08D6, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент P");

            global.Variables.Add("M600_PID_I", 0x18, 0x0008, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент I");
            global.Commands.Add("M600_PID_I", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08D8, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент I");

            global.Variables.Add("M600_PID_D", 0x18, 0x000A, 1,
                "Float_32", "", "##0.###", "", "PID-регулятор эмульсификатора M-600. Коэффициент D");
            global.Commands.Add("M600_PID_D", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08DA, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент D");

            global.Variables.Add("M600_PID_T", 0x18, 0x000C, 1,
                "Float_32", "", "##0.###", "", "PID -регулятор эмульсификатора M-600. Коэффициент T");
            global.Commands.Add("M600_PID_T", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08DC, "Float_32", "##0.###", "PID-регулятор эмульсификатора M-600. Коэффициент T");

            // PID P651
            global.Variables.Add("P651_PID_Tune", 0x18, 0x003C, 1,
                "Bool", "", "Нет;Да", "", "PID-регулятор насоса P-651. Автонастройка");
            global.Commands.Add("P651_PID_Tune", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x090C, "Bool", "Нет;Да", "PID-регулятор насоса P-651. Автонастройка");
        }

        private static void InitializeUnloadingSystem(TGlobal global)
        {
            global.Variables.Add("EM_Unloading_Rejim", 0x18, 0x001F, 1,
                "Int_16", "", "##0", "", "Режим отгрузки продукта");

            global.Commands.Add("EM_Unloading_PultButton", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08F0, "Bool", "Нет;Да", "Режим отгрузки продукта по командам");
            global.Commands.Add("EM_Unloading_TimeButton", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08F1, "Bool", "Нет;Да", "Режим отгрузки продукта по времени");
            global.Commands.Add("EM_Unloading_MassButton", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08F2, "Bool", "Нет;Да", "Режим отгрузки продукта по массе");

            global.Commands.Add("EM_Unload_Sp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08F3, "Float_32", "##0.###", "Режим отгрузки продукта. Задание");

            global.Variables.Add("EM_Unload_Speed", 0x18, 0x0025, 1,
                "Float_32", "", "##0.##", " кг./сек.", "Режим отгрузки продукта. Скорость");
            global.Variables.Add("EM_UnloadCounter", 0x18, 0x0027, 1,
                "Float_32", "", "##0.##", " кг.", "Режим отгрузки продукта. Отгружено");

            // Отгрузка по времени
            global.Commands.Add("EM_Unload_TimeStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08F9, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Пуск");
            global.Commands.Add("EM_Unload_TimeStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08FA, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Стоп");

            // Отгрузка по массе
            global.Commands.Add("EM_Unload_MassStart", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x090E, "Bool", "Нет;Да", "Режим отгрузки продукта по массе. Старт");
            global.Commands.Add("EM_Unload_MassStop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x090F, "Bool", "Нет;Да", "Режим отгрузки продукта по массе. Стоп");
            global.Commands.Add("EM_Unload_MassSp", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0910, "Float_32", "##0.###", "Режим отгрузки продукта по массе. Масса задания");

            // Тарирование
            global.Variables.Add("EM_UnloadTorirovanieTime", 0x18, 0x002C, 1,
                "Float_32", "", "##0", " сек.", "Режим отгрузки продукта. Торирование. Время торирования");
            global.Commands.Add("EM_Unload_Torirovanie_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08FE, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Старт");
            global.Commands.Add("EM_Unload_Torirovanie_Pause", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08FF, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Пауза");
            global.Commands.Add("EM_Unload_Torirovanie_Stop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0900, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Стоп");
            global.Commands.Add("EM_Unload_Torirovanie_Mass", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0901, "Float_32", "##0.###", "Режим отгрузки продукта по времени. Тарирование. Масса");
            global.Commands.Add("EM_Unload_Torirovanie_Calculate", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0903, "Bool", "Нет;Да", "Режим отгрузки продукта по времени. Тарирование. Пересчитать");
        }

        private static void InitializeAdditionalVariables(TGlobal global)
        {
            global.Variables.Add("M200_StartMixer", 0x14, 0x0042, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер M200");
            global.Commands.Add("M200_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0792, "Bool", "Нет;Да", "Включить миксер M200");

            global.Variables.Add("M250_StartMixer", 0x14, 0x0043, 1,
                "Bool", "", "Нет;Да", "", "Включить миксер M250");
            global.Commands.Add("M250_StartMixer", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0793, "Bool", "Нет;Да", "Включить миксер M250");

            global.Variables.Add("Compressor_Start", 0x17, 0x003C, 1,
                "Bool", "", "Нет;Да", "", "Включить компрессор");
            global.Commands.Add("Compressor_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08AC, "Bool", "Нет;Да", "Включить компрессор");

            global.Variables.Add("M600_EmergensyStop", 0x17, 0x0055, 1,
                "Bool", "", "Нет;Да", "", "Блокировка работы эмульсификатора");

            // Управление клапаном V-302
            global.Commands.Add("VLV302_StartButton", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0904, "Bool", "Нет;Да", "Открыть клапан V-302");
            global.Commands.Add("VLV302_StopButton", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x0905, "Bool", "Нет;Да", "Закрыть клапан V-302");

            global.Variables.Add("HE700_Rejim", 0x18, 0x001E, 1,
                "Int_16", "", "##0", "", "Режим нагревателя HE-100");
            global.Commands.Add("HE700_Rejim", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x08EE, "Int_16", "##0", "Режим нагревателя HE-100");

            // Скорость P400
            global.Variables.Add("P400_SpeedHi", 0x18, 0x004C, 1,
                "Float_32", "", "##0", " %", "Номинальная скорость Р-400");
            global.Commands.Add("P400_SpeedHi", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                0x091C, "Float_32", "##0.###", "Номинальная скорость Р-400");
        }

        private static void CreatePumpWithControl(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Насос с дополнительным управлением
            CreatePump(global, name, group, commandAddr, description, inputAddr, outputAddr);

            // Дополнительные переменные управления скоростью
            if (name == "P100")
            {
                global.Variables.Add("P100_SpeedHi", 0x18, 0x0046, 1,
                    "Float_32", "", "##0", " %", $"Номинальная скорость {name}");
                global.Commands.Add("P100_SpeedHi", global.Plc_IpAddress,
                    global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                    0x0916, "Float_32", "##0.###", $"Номинальная скорость {name}");

                global.Variables.Add("P100_SpeedLow", 0x18, 0x0048, 1,
                    "Float_32", "", "##0", " %", $"Минимальная скорость {name}");
                global.Commands.Add("P100_SpeedLow", global.Plc_IpAddress,
                    global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                    0x0918, "Float_32", "##0.###", $"Минимальная скорость {name}");

                global.Variables.Add("P100_MinMass", 0x18, 0x004A, 1,
                    "Float_32", "", "##0", " кг.", $"Минимальная масса для {name}");
                global.Commands.Add("P100_MinMass", global.Plc_IpAddress,
                    global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                    0x091A, "Float_32", "##0.###", $"Минимальная масса для {name}");
            }
        }

        // ========== МЕТОДЫ СОЗДАНИЯ ТИПОВЫХ УСТРОЙСТВ ==========

        private static void CreateAnalogSensor(TGlobal global, string name, string type,
            string unit, int group, ushort commandAddr, string description,
            ushort inputAddr, ushort outputAddr)
        {
            // Переменные для чтения из ПЛК (Input - то что пишет PLC)
            global.Variables.Add(name + "_Manual", group, inputAddr, 1,
                "Bool", "", "Автомат;Ручной", "", $"Ручной режим {name}");
            global.Variables.Add(name + "_ManualValue", group, (ushort)(inputAddr + 0x01), 2,
                "Float_32", "", "##0.#", unit, $"Ручное значение {name}");

            // Переменные для записи в ПЛК (Output - то что читает SCADA)
            global.Variables.Add(name + "_Value", group, outputAddr, 2,
                type, "", "##0.#", unit, $"Текущее значение {name}");
            global.Variables.Add(name + "_Status", group, (ushort)(outputAddr + 0x02), 1,
                "Int_16", "", "##0", "", $"Статус {name}");
            global.Variables.Add(name + "_Fault", group, (ushort)(outputAddr + 0x03), 1,
                "Bool", "", "Норма;Авария", "", $"Ошибка {name}");
            global.Variables.Add(name + "_Warning_Low", group, (ushort)(outputAddr + 0x04), 1,
                "Bool", "", "Норма;Предупреждение", "", $"Нижнее предупр. {name}");
            global.Variables.Add(name + "_Warning_Hi", group, (ushort)(outputAddr + 0x05), 1,
                "Bool", "", "Норма;Предупреждение", "", $"Верхнее предупр. {name}");
            global.Variables.Add(name + "_Fault_Low", group, (ushort)(outputAddr + 0x06), 1,
                "Bool", "", "Норма;Авария", "", $"Нижнее аварийное {name}");
            global.Variables.Add(name + "_Fault_Hi", group, (ushort)(outputAddr + 0x07), 1,
                "Bool", "", "Норма;Авария", "", $"Верхнее аварийное {name}");

            // Команды для записи в ПЛК
            global.Commands.Add(name + "_Manual", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                commandAddr, "Bool", "Автомат;Ручной", $"Ручной режим {name}");
            global.Commands.Add(name + "_ManualValue", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                (ushort)(commandAddr + 0x01), "Float_32", $"##0.## {unit}",
                $"Ручное значение {name}");
        }

        private static void CreateDiscreteSensor(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Дискретный датчик (только состояние)
            global.Variables.Add(name, group, outputAddr, 1,
                "Bool", "", "Норма;Сработал", "", description);
            global.Variables.Add(name + "_Fault", group, (ushort)(outputAddr + 0x01), 1,
                "Bool", "", "Норма;Авария", "", $"Ошибка {name}");

            // Команда сброса
            global.Commands.Add(name + "_Reset", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                commandAddr, "Bool", "Нет;Да", $"Сброс {name}");
        }

        private static void CreateValve(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Клапан
            global.Variables.Add(name + "_Open", group, outputAddr, 1,
                "Bool", "", "Закрыт;Открыт", "", $"Состояние {name}");
            global.Variables.Add(name + "_Closed", group, (ushort)(outputAddr + 0x01), 1,
                "Bool", "", "Открыт;Закрыт", "", $"Состояние {name}");
            global.Variables.Add(name + "_Fault", group, (ushort)(outputAddr + 0x02), 1,
                "Bool", "", "Норма;Авария", "", $"Ошибка {name}");

            // Команды управления
            global.Commands.Add(name + "_Command", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                commandAddr, "Bool", "Закрыть;Открыть", $"Управление {name}");
        }

        private static void CreatePump(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Насос
            global.Variables.Add(name + "_IsWork", group, outputAddr, 1,
                "Bool", "", "Стоп;Работает", "", $"Состояние {name}");
            global.Variables.Add(name + "_Fault", group, (ushort)(outputAddr + 0x01), 1,
                "Bool", "", "Норма;Авария", "", $"Ошибка {name}");
            global.Variables.Add(name + "_Manual", group, (ushort)(outputAddr + 0x02), 1,
                "Bool", "", "Автомат;Ручной", "", $"Ручной режим {name}");

            // Команды управления
            global.Commands.Add(name + "_Start", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                commandAddr, "Bool", "Стоп;Пуск", $"Пуск {name}");
            global.Commands.Add(name + "_Stop", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                (ushort)(commandAddr + 0x01), "Bool", "Пуск;Стоп", $"Стоп {name}");
        }

        private static void CreatePumpReverse(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Реверсивный насос
            CreatePump(global, name, group, commandAddr, description, inputAddr, outputAddr);

            // Дополнительные переменные для реверса
            global.Variables.Add(name + "_Direction", group, (ushort)(outputAddr + 0x03), 1,
                "Bool", "", "Прямой;Реверс", "", $"Направление {name}");

            // Команда реверса
            global.Commands.Add(name + "_Reverse", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                (ushort)(commandAddr + 0x02), "Bool", "Прямой;Реверс", $"Реверс {name}");
        }

        private static void CreateMixer(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Миксер (аналогично насосу)
            CreatePump(global, name, group, commandAddr, description, inputAddr, outputAddr);
        }

        private static void CreateCounter(TGlobal global, string name, string type,
            string unit, int group, ushort commandAddr, string description,
            ushort inputAddr, ushort outputAddr)
        {
            // Счетчик
            global.Variables.Add(name + "_Total", group, outputAddr, 2,
                type, "", "##0.##", unit, description);
            global.Variables.Add(name + "_Manual", group, (ushort)(outputAddr + 0x02), 1,
                "Bool", "", "Автомат;Ручной", "", $"Ручной режим {name}");

            // Команда сброса
            global.Commands.Add(name + "_Reset", global.Plc_IpAddress,
                global.Plc_PortNum, global.Plc_DeviceAddress, "Holding Registers",
                commandAddr, "Bool", "Нет;Да", $"Сброс {name}");
        }

        private static void CreateHeater(TGlobal global, string name,
            int group, ushort commandAddr, string description, ushort inputAddr,
            ushort outputAddr)
        {
            // Нагреватель
            global.Variables.Add(name + "_IsWork", group, outputAddr, 1,
                "Bool", "", "Выключен;Включен", "", $"Состояние {name}");
            global.Variables.Add(name + "_Temperature", group, (ushort)(outputAddr + 0x01), 2,
                "Float_32", "", "##0.#", "°C", $"Температура {name}");
            global.Variables.Add(name + "_Fault", group, (ushort)(outputAddr + 0x03), 1,
                "Bool", "", "Норма;Авария", "", $"Ошибка {name}");
        }

        // ========== МЕТОДЫ ДЛЯ РАБОТЫ С MODBUS ==========

        public static void StartModbusConnection(TGlobal global)
        {
            // Здесь будет запуск реального Modbus-клиента
            // Пока просто устанавливаем настройки
            Debug.WriteLine($"Modbus настроен: {global.Plc_IpAddress}:{global.Plc_PortNum}");
        }

        public static void UpdateModbusData(TGlobal global)
        {
            // Этот метод будет вызываться из таймера для обновления данных
            // В реальном проекте здесь будет чтение из Modbus

            // Эмуляция данных для тестирования
            //UpdateEmulatedData(global);
        }

        //private static void UpdateEmulatedData(TGlobal global)
        //{
        //    if (global?.Variables == null) return;

        //    Random rnd = new Random();

        //    foreach (var variable in global.Variables.All)
        //    {
        //        if (!variable.LastRead.HasValue ||
        //            (DateTime.Now - variable.LastRead.Value).TotalSeconds > 1)
        //        {
        //            // Эмуляция обновления данных
        //            if (variable.Type == "Float_32")
        //            {
        //                // Разные типы датчиков - разные диапазоны значений
        //                float baseValue = 0;

        //                if (variable.Name.Contains("TT")) // Температура
        //                    baseValue = 20 + (float)rnd.NextDouble() * 10;
        //                else if (variable.Name.Contains("PT")) // Давление
        //                    baseValue = 1 + (float)rnd.NextDouble() * 5;
        //                else if (variable.Name.Contains("LT")) // Уровень
        //                    baseValue = 50 + (float)rnd.NextDouble() * 30;
        //                else if (variable.Name.Contains("FM")) // Расход
        //                    baseValue = 100 + (float)rnd.NextDouble() * 200;
        //                else if (variable.Name.Contains("QM")) // Счетчик
        //                    baseValue = Math.Max(0, variable.ValueReal + (float)rnd.NextDouble() * 10);
        //                else if (variable.Name.Contains("WIT")) // Вес
        //                    baseValue = 1000 + (float)rnd.NextDouble() * 500;
        //                else
        //                    baseValue = (float)rnd.NextDouble() * 100;

        //                variable.ValueReal = baseValue;
        //                variable.ValueString = variable.ValueReal.ToString("F1");

        //                // Добавляем единицы измерения если есть
        //                if (!string.IsNullOrEmpty(variable.TextAfter))
        //                    variable.ValueString += " " + variable.TextAfter;
        //            }
        //            else if (variable.Type == "Bool")
        //            {
        //                // Для булевых - случайное изменение с малой вероятностью
        //                if (rnd.Next(100) < 5) // 5% вероятность изменения
        //                {
        //                    variable.ValueReal = variable.ValueReal > 0 ? 0 : 1;

        //                    // Форматирование для перечислений
        //                    if (!string.IsNullOrEmpty(variable.Format))
        //                    {
        //                        string[] values = variable.Format.Split(';');
        //                        if (values.Length == 2)
        //                        {
        //                            variable.ValueString = values[(int)variable.ValueReal];
        //                        }
        //                        else
        //                        {
        //                            variable.ValueString = variable.ValueReal > 0 ? "true" : "false";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        variable.ValueString = variable.ValueReal > 0 ? "true" : "false";
        //                    }
        //                }
        //            }
        //            else if (variable.Type == "Int_16")
        //            {
        //                variable.ValueReal = rnd.Next(0, 100);
        //                variable.ValueString = ((int)variable.ValueReal).ToString();
        //            }

        //            variable.LastRead = DateTime.Now;
        //        }
        //    }
        //}

        private static int GetVariableCount(TVariableList variables)
        {
            // В зависимости от реализации TVariableList, используйте соответствующий метод
            // Если есть свойство Count
            if (variables.GetType().GetProperty("Count") != null)
            {
                return (int)variables.GetType().GetProperty("Count").GetValue(variables);
            }
            // Или если есть метод GetCount
            else if (variables.GetType().GetMethod("GetCount") != null)
            {
                return (int)variables.GetType().GetMethod("GetCount").Invoke(variables, null);
            }
            // Или если есть свойство Items
            else if (variables.GetType().GetProperty("Items") != null)
            {
                var items = variables.GetType().GetProperty("Items").GetValue(variables);
                if (items is System.Collections.ICollection collection)
                {
                    return collection.Count;
                }
            }

            // Если ничего не найдено, вернем 0
            return 0;
        }

        private static int GetCommandCount(TCommandList commands)
        {
            // Аналогично для команд
            if (commands.GetType().GetProperty("Count") != null)
            {
                return (int)commands.GetType().GetProperty("Count").GetValue(commands);
            }
            else if (commands.GetType().GetMethod("GetCount") != null)
            {
                return (int)commands.GetType().GetMethod("GetCount").Invoke(commands, null);
            }
            else if (commands.GetType().GetProperty("Items") != null)
            {
                var items = commands.GetType().GetProperty("Items").GetValue(commands);
                if (items is System.Collections.ICollection collection)
                {
                    return collection.Count;
                }
            }

            return 0;
        }
    }
}