namespace ProtolScadaRemake
{
    public class TElementLs
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag ManualVariable; // Ручной режим датчика
        public TVariableTag ManualValueVariable; // Ручное значение датчика
        public TVariableTag ReverseVariable; // Инверсия значения входа
        public TVariableTag OnDelayVariable; // Задержка включения
        public TVariableTag OffDelayVariable; // Задержка отключения
        public TVariableTag ValueVariable; // Текущее значение датчика
        public TVariableTag StatusVariable; // Статус работы датчика
        // Команды
        public TCommandTag ManualCommand; // Ручной режим датчика
        public TCommandTag ManualValueCommand; // Ручное значение датчика
        public TCommandTag ReverseCommand; // Инверсия значения входа
        public TCommandTag OnDelayCommand; // Задержка включения
        public TCommandTag OffDelayCommand; // Задержка отключения
        public TElementLs(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим датчика " + Name);
            ManualValueVariable = Global.Variables.Add(Name + "_ManualValue", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Норма;Сработка", "", "Ручное значение датчика " + Name);
            ReverseVariable = Global.Variables.Add(Name + "_Reverse", Group, (ushort)(InputAddress + 0x02), 1, "Bool", "", "Нет;Инверсия", "", "Инверсия значение датчика " + Name);
            OnDelayVariable = Global.Variables.Add(Name + "_OnDelay", Group, (ushort)(InputAddress + 0x03), 1, "Int_16", "", "##0", " сек.", "Задержка включения " + Name);
            OffDelayVariable = Global.Variables.Add(Name + "_OffDelay", Group, (ushort)(InputAddress + 0x04), 1, "Int_16", "", "##0", " сек.", "Задержка отключения " + Name);
            ValueVariable = Global.Variables.Add(Name + "_Value", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Норма;Сработка", "", "Текущее значение датчика " + Name);
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x01), 1, "Int_16", "", "##0", "", "Статус работы датчика " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим датчика " + Name);
            ManualValueCommand = Global.Commands.Add(Name + "_ManualValue", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Норма;Сработка", "Ручное значение датчика " + Name);
            ReverseCommand = Global.Commands.Add(Name + "_Reverse", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Bool", "Нет;Инверсия", "Инверсия значение датчика " + Name);
            OnDelayCommand = Global.Commands.Add(Name + "_OnDelay", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x03), "Int_16", "##0", "Задержка включения " + Name);
            OffDelayCommand = Global.Commands.Add(Name + "_OffDelay", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x04), "Int_16", "##0", "Задержка отключения " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим датчика " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
            Global.Faults.Add(Name + "_Fault", "Авария", "Сработка датчика " + Name, "==", "Сработка", "Норма", "Авария", true, "Получена сработка датчика " + Name, "Пропала сработка датчика " + Name, "Авария", 3, true, true);
        }
    }
}
