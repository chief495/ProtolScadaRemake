using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TElementPumpUz
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
        public TVariableTag ManualSpeedVariable; // Рученое задание скорости
        public TVariableTag StartTimeVariable; // Время включения
        public TVariableTag StopTimeVariable; // Время отключения
        public TVariableTag IsWorkVariable; // Миксер включен
        public TVariableTag IsManualVariable; // Миксер в ручном режиме
        public TVariableTag SpeedVariable; // Текущее задание скоторси
        public TVariableTag FeedbackOkVariable; // Состояние подтверждено
        public TVariableTag StatusVariable; // Статус работы
        public TVariableTag FaultVariable; // Авария
                                           // Команды
        public TCommandTag ManualCommand; // Ручной режим
        public TCommandTag ManualStartCommand; // Ручной запуск
        public TCommandTag ManualSpeedCommand; // Рученое задание скорости
        public TCommandTag StartTimeCommand; // Время включения
        public TCommandTag StopTimeCommand; // Время отключения
        public TElementPumpUz(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим насоса " + Name);
            ManualStartVariable = Global.Variables.Add(Name + "_ManualStart", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Останов;Запуск", "", "Ручная команда насосу " + Name);
            ManualSpeedVariable = Global.Variables.Add(Name + "_ManualSpeed", Group, (ushort)(InputAddress + 0x02), 1, "Float_32", "", "##0", " %", "Рученое задание скорости насоса " + Name);
            StartTimeVariable = Global.Variables.Add(Name + "_StartTime", Group, (ushort)(InputAddress + 0x04), 1, "Int_16", "", "##0", " сек.", "Время включения насоса " + Name);
            StopTimeVariable = Global.Variables.Add(Name + "_StopTime", Group, (ushort)(InputAddress + 0x05), 1, "Int_16", "", "##0", " сек.", "Время отключения насоса " + Name);
            IsWorkVariable = Global.Variables.Add(Name + "_IsWork", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Насос " + Name + " включен");
            IsManualVariable = Global.Variables.Add(Name + "_IsManual", Group, (ushort)(OutputAddress + 0x01), 1, "Bool", "", "Нет;Да", "", "Насос " + Name + " в ручном режиме");
            FeedbackOkVariable = Global.Variables.Add(Name + "_FeedbackOk", Group, (ushort)(OutputAddress + 0x02), 1, "Bool", "", "Нет;Да", "", "Состояние " + Name + " подтверждено");
            FaultVariable = Global.Variables.Add(Name + "_Fault", Group, (ushort)(OutputAddress + 0x03), 1, "Bool", "", "Норма;Авария", "", "Авария насоса " + Name);
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x04), 1, "Int_16", "", "##0", "", "Статус работы насоса " + Name);
            SpeedVariable = Global.Variables.Add(Name + "_Speed", Group, (ushort)(OutputAddress + 0x05), 1, "Float_32", "", "##0", " %", "Текущее задание скоторси насоса " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим насоса " + Name);
            ManualStartCommand = Global.Commands.Add(Name + "_ManualStart", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Останов;Запуск", "Ручная команда насосу " + Name);
            ManualSpeedCommand = Global.Commands.Add(Name + "_ManualSpeed", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Float_32", "##0", "Рученое задание скорости насоса " + Name);
            StartTimeCommand = Global.Commands.Add(Name + "_StartTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x04), "Int_16", "##0 сек.", "Время включения насоса " + Name);
            StopTimeCommand = Global.Commands.Add(Name + "_StopTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x05), "Int_16", "##0 сек.", "Время отключения насоса " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим насоса " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
        }
    }
}
