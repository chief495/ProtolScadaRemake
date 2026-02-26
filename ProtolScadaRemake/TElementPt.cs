namespace ProtolScadaRemake
{
    public class TElementPt
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
        public TVariableTag LowPressVariable; // Нижняя граница измеряемого давления датчика
        public TVariableTag HiPressVariable; // Верхняя граница измеряемого давления датчика
        public TVariableTag LowCurrVariable; // Нижняя граница токовой петли датчика
        public TVariableTag HiCurrVariable; // Верхняя граница токовой петли датчика
        public TVariableTag LWVariable; // Нижнее предаварийное значение датчика
        public TVariableTag HWVariable; // Верхнее предаварийное значение датчика
        public TVariableTag LFVariable; // Нижнее даварийное значение датчика
        public TVariableTag HFVariable; // Верхнее даварийное значение датчика
        public TVariableTag ValueVariable; // Текущее значение датчика
        public TVariableTag StatusVariable; // Статус работы датчика
        public TVariableTag FaultVariable; // Ошибка датчика
        public TVariableTag Warning_LowVariable; // Нижнее предаварийное значение датчика
        public TVariableTag Warning_HiVariable; // Верхнее предаварийное значение датчика 
        public TVariableTag Fault_LowVariable; // Нижнее аварийное значение датчика
        public TVariableTag Fault_HiVariable; // Верхнее аварийное значение датчика
        // Команды
        public TCommandTag ManualCommand; // Ручной режим датчика
        public TCommandTag ManualValueCommand; // Ручное значение датчика
        public TCommandTag LowPressCommand; // Нижняя граница измеряемого давления датчика
        public TCommandTag HiPressCommand; // Верхняя граница измеряемого давления датчика
        public TCommandTag LowCurrCommand; // Нижняя граница токовой петли датчика
        public TCommandTag HiCurrCommand; // Верхняя граница токовой петли датчика
        public TCommandTag LWCommand; // Нижнее предаварийное значение датчика
        public TCommandTag HWCommand; // Верхнее предаварийное значение датчика
        public TCommandTag LFCommand; // Нижнее аварийное значение датчика
        public TCommandTag HFCommand; // Верхнее аварийное значение датчика
        // События
        // Тренды
        public TElementPt(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим датчика " + Name);
            ManualValueVariable = Global.Variables.Add(Name + "_ManualValue", Group, (ushort)(InputAddress + 0x01), 1, "Float_32", "", "##0.#", " Атм", "Ручное значение датчика " + Name);
            LowPressVariable = Global.Variables.Add(Name + "_LowPress", Group, (ushort)(InputAddress + 0x03), 1, "Float_32", "", "##0.#", " Атм", "Нижняя граница измеряемого давления датчика " + Name);
            HiPressVariable = Global.Variables.Add(Name + "_HiPress", Group, (ushort)(InputAddress + 0x05), 1, "Float_32", "", "##0.#", " Атм", "Верхняя граница измеряемого давления датчика " + Name);
            LowCurrVariable = Global.Variables.Add(Name + "_LowCurr", Group, (ushort)(InputAddress + 0x07), 1, "Float_32", "", "##0.##", " mА", "Нижняя граница токовой петли датчика " + Name);
            HiCurrVariable = Global.Variables.Add(Name + "_HiCurr", Group, (ushort)(InputAddress + 0x09), 1, "Float_32", "", "##0.##", " mА", "Верхняя граница токовой петли датчика " + Name);
            LWVariable = Global.Variables.Add(Name + "_LW", Group, (ushort)(InputAddress + 0x0B), 1, "Float_32", "", "##0.#", " Атм", "Нижнее предаварийное значение датчика " + Name);
            HWVariable = Global.Variables.Add(Name + "_HW", Group, (ushort)(InputAddress + 0x0D), 1, "Float_32", "", "##0.#", " Атм", "Верхнее предаварийное значение датчика " + Name);
            LFVariable = Global.Variables.Add(Name + "_LF", Group, (ushort)(InputAddress + 0x0F), 1, "Float_32", "", "##0.#", " Атм", "Нижнее аварийное значение датчика " + Name);
            HFVariable = Global.Variables.Add(Name + "_HF", Group, (ushort)(InputAddress + 0x11), 1, "Float_32", "", "##0.#", " Атм", "Верхнее аварийное значение датчика " + Name);
            ValueVariable = Global.Variables.Add(Name + "_Value", Group, (ushort)(OutputAddress + 0x00), 1, "Float_32", "", "##0.#", " Атм", "Текущее значение датчика " + Name);
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x02), 1, "Int_16", "", "##0", "", "Статус работы датчика " + Name);
            FaultVariable = Global.Variables.Add(Name + "_Fault", Group, (ushort)(OutputAddress + 0x03), 1, "Bool", "", "Норма;Авария", "", "Ошибка датчика " + Name);
            Warning_LowVariable = Global.Variables.Add(Name + "_Warning_Low", Group, (ushort)(OutputAddress + 0x04), 1, "Bool", "", "Норма;Предупреждение", "", "Нижнее предаварийное значение датчика " + Name);
            Warning_HiVariable = Global.Variables.Add(Name + "_Warning_Hi", Group, (ushort)(OutputAddress + 0x05), 1, "Bool", "", "Норма;Предупреждение", "", "Верхнее предаварийное значение датчика " + Name);
            Fault_LowVariable = Global.Variables.Add(Name + "_Fault_Low", Group, (ushort)(OutputAddress + 0x06), 1, "Bool", "", "Норма;Авария", "", "Нижнее аварийное значение датчика " + Name);
            Fault_HiVariable = Global.Variables.Add(Name + "_Fault_Hi", Group, (ushort)(OutputAddress + 0x07), 1, "Bool", "", "Норма;Авария", "", "Верхнее аварийное значение датчика " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим датчика " + Name);
            ManualValueCommand = Global.Commands.Add(Name + "_ManualValue", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Float_32", "##0.## Атм", "Ручное значение датчика " + Name);
            LowPressCommand = Global.Commands.Add(Name + "_LowPress", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x03), "Float_32", "##0.## Атм", "Нижняя граница измеряемого давления датчика " + Name);
            HiPressCommand = Global.Commands.Add(Name + "_HiPress", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x05), "Float_32", "##0.## Атм", "Верхняя граница измеряемого давления датчика " + Name);
            LowCurrCommand = Global.Commands.Add(Name + "_LowCurr", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x07), "Float_32", "##0.## mА", "Нижняя граница токовой петли датчика " + Name);
            HiCurrCommand = Global.Commands.Add(Name + "_HiCurr", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x09), "Float_32", "##0.## mА", "Верхняя граница токовой петли датчика " + Name);
            LWCommand = Global.Commands.Add(Name + "_LW", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x0B), "Float_32", "##0.## Атм", "Нижнее предаварийное значение датчика " + Name);
            HWCommand = Global.Commands.Add(Name + "_HW", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x0D), "Float_32", "##0.## Атм", "Верхнее предаварийное значение датчика " + Name);
            LFCommand = Global.Commands.Add(Name + "_LF", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x0F), "Float_32", "##0.## Атм", "Нижнее аварийное значение датчика " + Name);
            HFCommand = Global.Commands.Add(Name + "_HF", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x11), "Float_32", "##0.## Атм", "Верхнее аварийное значение датчика " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим датчика " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
            Global.Faults.Add(Name + "_Fault", "Отказ", "Ошибка датчика " + Name, "==", "Авария", "Норма", "Отказ", true, "Получена ошибка датчика " + Name, "Пропала ошибка датчика " + Name, "Отказ", 3, true, true);
            Global.Faults.Add(Name + "_Warning_Low", "Предупреждение", "Нижнее предаварийное значение датчика " + Name, "==", "Предупреждение", "Норма", "Предупреждение", true, "Нижнее предаварийное значение датчика " + Name + " = Норма", "Нижнее предаварийное значение датчика " + Name + " = Предупреждение", "Предупреждение", 2, false, true);
            Global.Faults.Add(Name + "_Warning_Hi", "Предупреждение", "Верхнее предаварийное значение датчика " + Name, "==", "Предупреждение", "Норма", "Предупреждение", true, "Верхнее предаварийное значение датчика " + Name + " = Норма", "Верхнее предаварийное значение датчика " + Name + " = Предупреждение", "Предупреждение", 2, false, true);
            Global.Faults.Add(Name + "_Fault_Low", "Авария", "Нижнее аварийное значение датчика " + Name, "==", "Авария", "Норма", "Авария", true, "Нижнее аварийное значение датчика " + Name + " = Норма", "Нижнее аварийное значение датчика " + Name + " = Авария", "Авария", 3, false, true);
            Global.Faults.Add(Name + "_Fault_Hi", "Авария", "Верхнее аварийное значение датчика " + Name, "==", "Авария", "Норма", "Авария", true, "Верхнее аварийное значение датчика " + Name + " = Норма", "Нижнее аварийное значение датчика " + Name + " = Авария", "Авария", 3, false, true);
            // Тренды
            Global.Trends.Add(Name + "_Value", "Текущее значение датчика " + Name, "Атм", 10, 32000);
        }

    }
}
