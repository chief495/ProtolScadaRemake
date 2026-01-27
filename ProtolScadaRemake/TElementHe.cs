namespace ProtolScadaRemake
{
    public class TElementHe
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag ManualVariable; // Ручной режим 
        public TVariableTag ManualStartVariable; // Ручной запуск
        public TVariableTag IsWorkVariable; // В работе
        // Команды
        public TCommandTag ManualCommand; // Ручной режим датчика
        public TCommandTag ManualStartCommand; // Ручное открытие клапана
        public TElementHe(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим нагревателя " + Name);
            ManualStartVariable = Global.Variables.Add(Name + "_ManualStart", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Нет;Открыть", "", "Ручное включение нагревателя " + Name);
            IsWorkVariable = Global.Variables.Add(Name + "_IsWork", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Нагреватель " + Name + " включен");
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим нагревателя " + Name);
            ManualStartCommand = Global.Commands.Add(Name + "_ManualStart", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Нет;Открыть", "Ручное включение нагревателя " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим нагревателя " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
        }
    }
}
