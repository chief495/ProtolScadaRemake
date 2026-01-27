namespace ProtolScadaRemake
{
    public class TElementFM
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag ResetMassVariable; // Сброс массового счетчика
        public TVariableTag ResetVolumeVariable; // Сброс объемного счетчика
        public TVariableTag MassStartPositionVariable; // Начальная позиция массового счетчика
        public TVariableTag VolumeStartPositionVariable; // Начальная позиция объемного счетчика
        public TVariableTag MassFlowVariable; // Массовый расход
        public TVariableTag MassTotalVariable; // Значение массового накопительного счетчика
        public TVariableTag MassSubTotalVariable; // Значение массового счетчика
        public TVariableTag VolumeFlowVariable; // Объемноый расход
        public TVariableTag VolumeTotalVariable; // Значение объемного накопительного счетчика
        public TVariableTag VolumeSubTotalVariable; // Значение объемного счетчика
        public TVariableTag TempVariable; // Значение объемного счетчика
        public TVariableTag PressureVariable; // Значение объемного счетчика
        public TVariableTag DensityVariable; // Значение объемного счетчика
        // Команды
        public TCommandTag ResetMassCommand; // Сброс массового счетчика
        public TCommandTag ResetVolumeCommand; // Сброс объемного счетчика
        // События
        // Тренды
        public TElementFM(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ResetMassVariable = Global.Variables.Add(Name + "_ResetMass", Group, (ushort)(InputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Сброс массового счетчика " + Name);
            ResetVolumeVariable = Global.Variables.Add(Name + "_ResetVolume", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Нет;Да", "", "Сброс объемного счетчика " + Name);
            MassStartPositionVariable = Global.Variables.Add(Name + "_MassStartPosition", Group, (ushort)(InputAddress + 0x02), 1, "Float_32", "", "##0.##", " кг.", "Начальная позиция массового счетчика " + Name);
            VolumeStartPositionVariable = Global.Variables.Add(Name + "_VolumeStartPosition", Group, (ushort)(InputAddress + 0x04), 1, "Float_32", "", "##0.##", " л.", "Начальная позиция объемного счетчика " + Name);
            MassFlowVariable = Global.Variables.Add(Name + "_MassFlow", Group, (ushort)(OutputAddress + 0x00), 1, "Float_32", "", "##0.##", " кг/мин", "Массовый расход " + Name);
            MassTotalVariable = Global.Variables.Add(Name + "_MassTotal", Group, (ushort)(OutputAddress + 0x02), 1, "Float_32", "", "##0.##", " кг.", "Значение массового накопительного счетчика " + Name);
            MassSubTotalVariable = Global.Variables.Add(Name + "_MassSubTotal", Group, (ushort)(OutputAddress + 0x04), 1, "Float_32", "", "##0.##", " кг.", "Значение массового счетчика " + Name);
            VolumeFlowVariable = Global.Variables.Add(Name + "_VolumeFlow", Group, (ushort)(OutputAddress + 0x06), 1, "Float_32", "", "##0.##", " л/мин.", "Объемноый расход " + Name);
            VolumeTotalVariable = Global.Variables.Add(Name + "_VolumeTotal", Group, (ushort)(OutputAddress + 0x08), 1, "Float_32", "", "##0.##", " л.", "Значение объемного накопительного счетчика " + Name);
            VolumeSubTotalVariable = Global.Variables.Add(Name + "_VolumeSubTotal", Group, (ushort)(OutputAddress + 0x0A), 1, "Float_32", "", "##0.##", " л.", "Значение объемного счетчика " + Name);
            TempVariable = Global.Variables.Add(Name + "_Temp", Group, (ushort)(OutputAddress + 0x0C), 1, "Float_32", "", "##0.##", " °C", "Температура " + Name);
            DensityVariable = Global.Variables.Add(Name + "_Density", Group, (ushort)(OutputAddress + 0x10), 1, "Float_32", "", "##0.####", " г/cм³.", "Плотность " + Name);
            // Команды
            ResetMassCommand = Global.Commands.Add(Name + "_ResetMass", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x00), "Bool", "Нет;Да", "Сброс массового счетчика " + Name);
            ResetVolumeCommand = Global.Commands.Add(Name + "_ResetVolume", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Нет;Да", "Сброс объемного счетчика " + Name);
            // События
            // Тренды
        }
    }
}
