using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TFaultRecord
    {
        public string Name = ""; // Уникальное имя
        public string Group = ""; // Имя группы, например warnings или faults
        public string Description = ""; // Текстовое описание аварии
        public string Type = ""; // Тип аварии ( <, >, <=, >=, <>, ==)
        public string FaultValue = ""; // Значение срабатывания
        public bool IsActive = false; // Событие в режиме сработки
        public string ActiveValue = ""; // Текстовое значение при сработке
        public string PassiveValue = ""; // Текстовое значение при норме
        public DateTime LastActivationTime = DateTime.MinValue; // Время последнего срабатывания
        public DateTime LastDeactivationTime = DateTime.MinValue; // Время последней деактивации
        public bool WriteToLog = false; // Сохранять ли событие в лог
        public string LogActivationText = ""; // Текст события активации в логе
        public string LogDeactivationText = ""; // Текст события деактивации в логе
        public string LogGroupName = ""; // Имя группы в логе
        public Int16 LogImageIndex = 0; // Индекс изображения иконки в логе
        public bool ShowPopupActivation = false; // Отображать форму уведомления при активации события
        public bool ShowPopupDeactivation = false; // Отображать форму уведомления при деактивации события
        public TFaultRecord() // Конструктор
        {
        }
        public void SaveToStream(FileStream Stream) // Сохраняем данные в поток
        {
            TGlobal.SaveStringToStream(Stream, Name); // Имя тега
            TGlobal.SaveStringToStream(Stream, Group); // Имя группы, например warnings или faults
            TGlobal.SaveStringToStream(Stream, Description); // Текстовое описание аварии
            TGlobal.SaveStringToStream(Stream, Type); // Тип аварии ( <, >, <=, >=, <>, ==)
            TGlobal.SaveStringToStream(Stream, FaultValue); // Значение срабатывания
            TGlobal.SaveBoolToStream(Stream, IsActive); // Событие в режиме сработки
            TGlobal.SaveStringToStream(Stream, ActiveValue); // Текстовое значение при сработке
            TGlobal.SaveStringToStream(Stream, PassiveValue); // Текстовое значение при норме
            TGlobal.SaveDateTimeToStream(Stream, LastActivationTime); // Время последнего срабатывания
            TGlobal.SaveDateTimeToStream(Stream, LastDeactivationTime); // Время последней деактивации
            TGlobal.SaveBoolToStream(Stream, WriteToLog); // Сохранять ли событие в лог
            TGlobal.SaveStringToStream(Stream, LogActivationText); // Текст события активации в логе
            TGlobal.SaveStringToStream(Stream, LogDeactivationText); // Текст события деактивации в логе
            TGlobal.SaveStringToStream(Stream, LogGroupName); // Имя группы в логе
            TGlobal.SaveIntToStream(Stream, LogImageIndex); // Индекс изображения иконки в логе
            TGlobal.SaveBoolToStream(Stream, ShowPopupActivation); // Отображать форму уведомления при активации события
            TGlobal.SaveBoolToStream(Stream, ShowPopupDeactivation); // Отображать форму уведомления при деактивации события
        }
        public bool LoadFromStream(FileStream Stream) // Чтение данных из потока
        {
            bool Result = true;
            Name = TGlobal.LoadStringFromStream(Stream); // Имя тега
            Group = TGlobal.LoadStringFromStream(Stream); // Имя группы, например warnings или faults
            Description = TGlobal.LoadStringFromStream(Stream); // Текстовое описание аварии
            Type = TGlobal.LoadStringFromStream(Stream); // Тип аварии ( <, >, <=, >=, <>, ==)
            FaultValue = TGlobal.LoadStringFromStream(Stream); // Значение срабатывания
            IsActive = TGlobal.LoadBoolFromStream(Stream); // Событие в режиме сработки
            ActiveValue = TGlobal.LoadStringFromStream(Stream); // Текстовое значение при сработке
            PassiveValue = TGlobal.LoadStringFromStream(Stream); // Текстовое значение при норме
            LastActivationTime = TGlobal.LoadDateTimeFromStream(Stream); // Время последнего срабатывания
            LastDeactivationTime = TGlobal.LoadDateTimeFromStream(Stream); // Время последней деактивации
            WriteToLog = TGlobal.LoadBoolFromStream(Stream); // Сохранять ли событие в лог
            LogActivationText = TGlobal.LoadStringFromStream(Stream); // Текст события активации в логе
            LogDeactivationText = TGlobal.LoadStringFromStream(Stream); // Текст события деактивации в логе
            LogGroupName = TGlobal.LoadStringFromStream(Stream); // Имя группы в логе
            LogImageIndex = (short)TGlobal.LoadIntFromStream(Stream); // Индекс изображения иконки в логе
            ShowPopupActivation = TGlobal.LoadBoolFromStream(Stream); // Отображать форму уведомления при активации события
            ShowPopupDeactivation = TGlobal.LoadBoolFromStream(Stream); // Отображать форму уведомления при деактивации события
            if (Stream.Position >= Stream.Length - 1) Result = false; // Защита от неожиданного окончания файла
            return Result;
        }
        public void Update(TVariableTag Variable, TLogList Log)
        {
            switch(Type)
            {
                case "==":
                    UpdateEqual(Variable, Log);
                    break;
                case "<":
                    break;
                case ">":
                    break;
                case "<=":
                    break;
                case ">=":
                    break;
                case "<>":
                    break;
            }
        }
        private void UpdateEqual(TVariableTag Variable, TLogList Log)
        {
            // Активация
            if(!IsActive)
                if(Variable.ValueString == FaultValue)
                {
                    LastActivationTime = DateTime.Now;
                    IsActive = true;
                    // Регистрация в журнале
                    if(WriteToLog) Log.Add(LogGroupName,LogActivationText,LogImageIndex);
                    // Отображение формы сообщения
                    //DialogFaultEvent EventForm = new DialogFaultEvent();
                    //EventForm.FaultRecord =this;
                    //EventForm.Initialize();
                    //EventForm.ShowDialog();
                }
            // Деактивация
            if (IsActive)
                if (Variable.ValueString != FaultValue)
                {
                    LastDeactivationTime = DateTime.Now;
                    IsActive = false;
                    // Регистрация в журнале
                    if (WriteToLog) Log.Add(LogGroupName, LogDeactivationText, LogImageIndex);
                    // Отображение формы сообщения
                    //DialogFaultEvent EventForm = new DialogFaultEvent();
                    //EventForm.FaultRecord = this;
                    //EventForm.Initialize();
                    //EventForm.ShowDialog();
                }

        }
    }
}
