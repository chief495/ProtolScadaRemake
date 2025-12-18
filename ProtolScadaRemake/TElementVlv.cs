using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TElementVlv
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag ManualVariable; // Ручной режим клапана
        public TVariableTag ManualOpenVariable; // Ручное открытие клапана
        public TVariableTag ManualCloseVariable; // Ручное закрытие клапана
        public TVariableTag OpenTimeVariable; // Время открытия клапана
        public TVariableTag CloseTimeVariable; // Время закрытия клапана
        public TVariableTag IsOpenVariable; // Клапан открыт
        public TVariableTag IsCloseVariable; // Клапан закрыт
        public TVariableTag IsMovingVariable; // Клапан в движении
        public TVariableTag StatusVariable; // Статус работы клапана
        public TVariableTag FaultVariable; // Заклинивание клапана
        // Команды
        public TCommandTag ManualCommand; // Ручной режим датчика
        public TCommandTag ManualOpenCommand; // Ручное открытие клапана
        public TCommandTag ManualCloseCommand; // Ручное закрытие клапана
        public TCommandTag OpenTimeCommand; // Время открытия клапана
        public TCommandTag CloseTimeCommand; // Время закрытия клапана
        public TElementVlv(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим клапана " + Name);
            ManualOpenVariable = Global.Variables.Add(Name + "_ManualOpen", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Нет;Открыть", "", "Ручное открытие клапана " + Name);
            ManualCloseVariable = Global.Variables.Add(Name + "_ManualClose", Group, (ushort)(InputAddress + 0x02), 1, "Bool", "", "Нет;Закрыть", "", "Ручное закрытие клапана " + Name);
            OpenTimeVariable = Global.Variables.Add(Name + "_OpenTime", Group, (ushort)(InputAddress + 0x03), 1, "Int_16", "", "##0", " сек.", "Время открытия клапана " + Name);
            CloseTimeVariable = Global.Variables.Add(Name + "_CloseTime", Group, (ushort)(InputAddress + 0x04), 1, "Int_16", "", "##0", " сек.", "Время закрытия клапана " + Name);
            IsOpenVariable = Global.Variables.Add(Name + "_IsOpen", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Клапан " + Name + " открыт");
            IsCloseVariable = Global.Variables.Add(Name + "_IsClose", Group, (ushort)(OutputAddress + 0x01), 1, "Bool", "", "Нет;Да", "", "Клапан " + Name + " закрыт");
            IsMovingVariable = Global.Variables.Add(Name + "_IsMoving", Group, (ushort)(OutputAddress + 0x02), 1, "Bool", "", "Нет;Да", "", "Клапан " + Name + " в движении");
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x03), 1, "Int_16", "", "##0", "", "Статус работы клапана " + Name);
            FaultVariable = Global.Variables.Add(Name + "_Fault", Group, (ushort)(OutputAddress + 0x04), 1, "Bool", "", "Норма;Авария", "", "Заклинивание клапана " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим клапана " + Name);
            ManualOpenCommand = Global.Commands.Add(Name + "_ManualOpen", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Нет;Открыть", "Ручное открытие клапана " + Name);
            ManualCloseCommand = Global.Commands.Add(Name + "_ManualClose", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Bool", "Нет;Закрыть", "Ручное закрытие клапана " + Name);
            OpenTimeCommand = Global.Commands.Add(Name + "_OpenTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x03), "Int_16", "##0 сек.", "Время открытия клапана " + Name);
            CloseTimeCommand = Global.Commands.Add(Name + "_CloseTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x04), "Int_16", "##0 сек.", "Время закрытия клапана " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим клапана " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
            Global.Faults.Add(Name + "_Fault", "Отказ", "Заклинивание клапана " + Name, "==", "Авария", "Норма", "Отказ", true, "Произошло заклинивание клапана " + Name, "Пропало заклинивание клапана " + Name, "Отказ", 3, true, true);
        }
    }
}
