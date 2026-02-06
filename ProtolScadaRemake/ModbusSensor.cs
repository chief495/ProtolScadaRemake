using System;

namespace ProtolScadaRemake
{
    public class ModbusSensor
    {
        public TGlobal Global { get; }
        public string Name { get; }
        public ushort InputAddress { get; }
        public ushort OutputAddress { get; }
        public ushort CommandAddress { get; }
        public string Unit { get; }

        public ModbusSensor(TGlobal global, string name, string unit,
                           ushort inputAddr, ushort outputAddr, ushort commandAddr)
        {
            Global = global;
            Name = name;
            Unit = unit;
            InputAddress = inputAddr;
            OutputAddress = outputAddr;
            CommandAddress = commandAddr;

            CreateVariables();
            CreateCommands();
        }

        private void CreateVariables()
        {
            // Создаем ВСЕ переменные, которые использует Element_AI
            Global.Variables.Add(Name + "_Manual", 1, InputAddress, 1,
                "Bool", "", "Автомат;Ручной", "", $"Ручной режим {Name}");

            Global.Variables.Add(Name + "_ManualValue", 1, (ushort)(InputAddress + 0x01), 2,
                "Float_32", "", "##0.#", Unit, $"Ручное значение {Name}");

            Global.Variables.Add(Name + "_Value", 1, OutputAddress, 2,
                "Float_32", "", "##0.#", Unit, $"Текущее значение {Name}");

            Global.Variables.Add(Name + "_Warning_Low", 1, (ushort)(OutputAddress + 0x04), 1,
                "Bool", "", "Норма;Предупреждение", "", $"Нижнее предупр. {Name}");

            Global.Variables.Add(Name + "_Warning_Hi", 1, (ushort)(OutputAddress + 0x05), 1,
                "Bool", "", "Норма;Предупреждение", "", $"Верхнее предупр. {Name}");

            Global.Variables.Add(Name + "_Fault_Low", 1, (ushort)(OutputAddress + 0x06), 1,
                "Bool", "", "Норма;Авария", "", $"Нижнее аварийное {Name}");

            Global.Variables.Add(Name + "_Fault_Hi", 1, (ushort)(OutputAddress + 0x07), 1,
                "Bool", "", "Норма;Авария", "", $"Верхнее аварийное {Name}");
        }

        private void CreateCommands()
        {
            // Создаем команды для записи
            Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum,
                Global.Plc_DeviceAddress, "Holding Registers", CommandAddress,
                "Bool", "Автомат;Ручной", $"Ручной режим {Name}");

            Global.Commands.Add(Name + "_ManualValue", Global.Plc_IpAddress, Global.Plc_PortNum,
                Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01),
                "Float_32", $"##0.## {Unit}", $"Ручное значение {Name}");
        }
    }
}