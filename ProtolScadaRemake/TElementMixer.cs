using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TElementMixer
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
        public TVariableTag StartTimeVariable; // Время включения
        public TVariableTag StopTimeVariable; // Время отключения
        public TVariableTag IsWorkVariable; // Миксер включен
        public TVariableTag FeedbackOkVariable; // Состояние подтверждено
        public TVariableTag StatusVariable; // Статус работы клапана
        public TVariableTag FaultVariable; // Авария миксера
        // Команды
        public TCommandTag ManualCommand; // Ручной режим
        public TCommandTag ManualStartCommand; // Ручной запуск
        public TCommandTag StartTimeCommand; // Время включения
        public TCommandTag StopTimeCommand; // Время отключения
        public TElementMixer(TGlobal G, string N, ushort AddressIn, ushort AddressOut, int VarGroup, ushort CommAddr) // Конструктор
        {
            Global = G;
            Name = N;
            InputAddress = AddressIn;
            OutputAddress = AddressOut;
            CommandAddress = CommAddr;
            Group = VarGroup;
            // Переменные
            ManualVariable = Global.Variables.Add(Name + "_Manual", Group, InputAddress, 1, "Bool", "", "Автомат;Ручной", "", "Ручной режим миксера " + Name);
            ManualStartVariable = Global.Variables.Add(Name + "_ManualStart", Group, (ushort)(InputAddress + 0x01), 1, "Bool", "", "Останов;Запуск", "", "Ручная команда миксеру " + Name);
            StartTimeVariable = Global.Variables.Add(Name + "_StartTime", Group, (ushort)(InputAddress + 0x02), 1, "Int_16", "", "##0", " сек.", "Время включения миксера " + Name);
            StopTimeVariable = Global.Variables.Add(Name + "_StopTime", Group, (ushort)(InputAddress + 0x03), 1, "Int_16", "", "##0", " сек.", "Время отключения миксера " + Name);
            IsWorkVariable = Global.Variables.Add(Name + "_IsWork", Group, (ushort)(OutputAddress + 0x00), 1, "Bool", "", "Нет;Да", "", "Миксер " + Name + " включен");
            FeedbackOkVariable = Global.Variables.Add(Name + "_FeedbackOk", Group, (ushort)(OutputAddress + 0x01), 1, "Bool", "", "Нет;Да", "", "Состояние " + Name + " подтверждено");
            StatusVariable = Global.Variables.Add(Name + "_Status", Group, (ushort)(OutputAddress + 0x02), 1, "Int_16", "", "##0", "", "Статус работы миксера " + Name);
            FaultVariable = Global.Variables.Add(Name + "_Fault", Group, (ushort)(OutputAddress + 0x03), 1, "Bool", "", "Норма;Авария", "", "Авария миксера " + Name);
            // Команды
            ManualCommand = Global.Commands.Add(Name + "_Manual", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", CommandAddress, "Bool", "Автомат;Ручной", "Ручной режим миксера " + Name);
            ManualStartCommand = Global.Commands.Add(Name + "_ManualStart", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x01), "Bool", "Останов;Запуск", "Ручная команда миксеру " + Name);
            StartTimeCommand = Global.Commands.Add(Name + "_StartTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x02), "Int_16", "##0 сек.", "Время включения миксера " + Name);
            StopTimeCommand = Global.Commands.Add(Name + "_StopTime", Global.Plc_IpAddress, Global.Plc_PortNum, Global.Plc_DeviceAddress, "Holding Registers", (ushort)(CommandAddress + 0x03), "Int_16", "##0 сек.", "Время отключения миксера " + Name);
            // События
            Global.Faults.Add(Name + "_Manual", "Предупреждение", "Ручной режим миксера " + Name, "==", "Ручной", "Норма", "ручной режим", false, "", "", "", 0, false, false);
            Global.Faults.Add(Name + "_Fault", "Отказ", "Авария миксера " + Name, "==", "Авария", "Норма", "Сбой", true, "Произошла авария миксера " + Name, "Пропала авария миксера " + Name, "Сбой", 3, true, true);
        }
    }
}
