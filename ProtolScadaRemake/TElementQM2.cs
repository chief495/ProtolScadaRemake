using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TElementQM2
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag StartValueVariable; // Начальное значение счетчика
        public TVariableTag PulseSizeVariable; // Цена импульса
        public TVariableTag ResetVariable; // Сброс счетчика
        public TVariableTag TotalVariable; // Значение счетчика
        // Команды
        public TCommandTag StartValueCommand; // Начальное значение счетчика
        public TCommandTag PulseSizeCommand; // Цена импульса
        public TCommandTag ResetCommand; // Сброс счетчика
        // События
        // Тренды
        public TElementQM2(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            StartValueVariable = Global.Variables.Add(Name + "_StartValue", Group, InputAddress, 1, "Float_32", "", "##0.##", " кг.", "Начальное значение счетчика " + Name);
            PulseSizeVariable = Global.Variables.Add(Name + "_PulseSize", Group, (ushort)(InputAddress + 0x02), 1, "Float_32", "", "##0.##", " кг.", "Цена импульса " + Name);
            ResetVariable = Global.Variables.Add(Name + "_Reset", Group, (ushort)(InputAddress + 0x04), 1, "Bool", "", "Нет;Да", "", "Сброс счетчика " + Name);
            TotalVariable = Global.Variables.Add(Name + "_Total", Group, (ushort)(OutputAddress + 0x00), 1, "Float_32", "", "##0.##", " кг.", "Значение счетчика " + Name);
            // Команды
            StartValueCommand = Global.Commands.Add(Name + "_StartValue", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Float_32", "##0.## кг.", "Начальное значение счетчик " + Name);
            PulseSizeCommand = Global.Commands.Add(Name + "_PulseSize", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Float_32", "##0.## кг.", "Цена импульса " + Name);
            ResetCommand = Global.Commands.Add(Name + "_Reset", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x04), "Bool", "Нет;Да", "Сброс счетчика " + Name);
            // События
            // Тренды
        }
    }
}
