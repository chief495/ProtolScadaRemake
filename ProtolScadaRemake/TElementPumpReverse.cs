using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TElementPumpReverse
    {
        public TGlobal Global; // Глобальная область данных
        public string Name;
        public ushort InputAddress;
        public ushort OutputAddress;
        public int Group;
        public ushort CommandAddress;
        // Переменные
        public TVariableTag ManualVariable; // Ручной режим
        public TVariableTag ManualNormalStartVariable; // Ручной запуск вперед
        public TVariableTag ManualReverselStartVariable; // Ручной запуск назад
        public TVariableTag NormalStartTimeVariable; // Время включения вперед
        public TVariableTag ReverselStartTimeVariable; // Время включения назад
        public TVariableTag StopTimeVariable; // Время отключения
        public TVariableTag IsNormalWorkVariable; // Насос включен вперед
        public TVariableTag IsReverselWorkVariable; // Насос включен назад
        public TVariableTag FeedbackOkVariable; // Состояние подтверждено
        public TVariableTag StatusVariable; // Статус работы
        public TVariableTag FaultVariable; // Авария
        // Команды
        public TCommandTag ManualCommand; // Ручной режим
        public TCommandTag ManualNormalStartCommand; // Ручной запуск вперед
        public TCommandTag ManualReverseStartCommand; // Ручной запуск назад
        public TCommandTag NormalStartTimeCommand; // Время включения вперед
        public TCommandTag ReverseStartTimeCommand; // Время включения назад
        public TCommandTag StopTimeCommand; // Время отключения
        public TElementPumpReverse(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим насоса " + Name);
            ManualNormalStartVariable = Global.Variables.Add(Name + "_ManualNormalStart", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Останов;Запуск", "", "Ручная команда вперед насосу " + Name);
            ManualReverselStartVariable = Global.Variables.Add(Name + "_ManualReverseStart", Group, (ushort)(InputAddress + 0x02), 1, "Bool", "", "Останов;Запуск", "", "Ручная команда назад насосу " + Name);
            NormalStartTimeVariable = Global.Variables.Add(Name + "_NormalStartTime", Group, (ushort)(InputAddress + 0x03), 1, "Int_16", "", "##0", " сек.", "Время включения вперед насоса " + Name);
            ReverselStartTimeVariable = Global.Variables.Add(Name + "_ReverseStartTime", Group, (ushort)(InputAddress + 0x04), 1, "Int_16", "", "##0", " сек.", "Время включения назад насоса " + Name);
            StopTimeVariable = Global.Variables.Add(Name + "_StopTime", Group, (ushort)(InputAddress + 0x05), 1, "Int_16", "", "##0", " сек.", "Время отключения насоса " + Name);
            IsNormalWorkVariable = Global.Variables.Add(Name + "_IsNormalWork", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Насос " + Name + " включен вперед");
            IsReverselWorkVariable = Global.Variables.Add(Name + "_IsReverseWork", Group, (ushort)(OutputAddress + 0x01), 1, "Bool", "", "Нет;Да", "", "Насос " + Name + " включен назад");
            FeedbackOkVariable = Global.Variables.Add(Name + "_FeedbackOk", Group, (ushort)(OutputAddress + 0x02), 1, "Bool", "", "Нет;Да", "", "Состояние " + Name + " подтверждено");
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x03), 1, "Int_16", "", "##0", "", "Статус работы насоса " + Name);
            FaultVariable = Global.Variables.Add(Name + "_Fault", Group, (ushort)(OutputAddress + 0x04), 1, "Bool", "", "Норма;Авария", "", "Авария насоса " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим насоса " + Name);
            ManualNormalStartCommand = Global.Commands.Add(Name + "_ManualNormalStart", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Останов;Запуск", "Ручная команда вперед насосу " + Name);
            ManualReverseStartCommand = Global.Commands.Add(Name + "_ManualReverseStart", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Bool", "Останов;Запуск", "Ручная команда назад насосу " + Name);
            NormalStartTimeCommand = Global.Commands.Add(Name + "_NormalStartTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x03), "Int_16", "##0 сек.", "Время включения вперед насоса " + Name);
            ReverseStartTimeCommand = Global.Commands.Add(Name + "_ReverseStartTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x04), "Int_16", "##0 сек.", "Время включения назад насоса " + Name);
            StopTimeCommand = Global.Commands.Add(Name + "_StopTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x05), "Int_16", "##0 сек.", "Время отключения насоса " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим насоса " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
            Global.Faults.Add(Name + "_Fault", "Отказ", "Авария насоса " + Name, "==", "Авария", "Норма", "Сбой", true, "Произошла авария насоса " + Name, "Пропала авария насоса " + Name, "Сбой", 3, true, true);
        }
    }
}
