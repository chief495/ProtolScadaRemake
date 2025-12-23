using System;
using System.IO;

namespace ProtolScadaRemake
{
    public class TFaultRecord
    {
        public string Name = "";
        public string Group = "";
        public string Description = "";
        public string Type = "";
        public string FaultValue = "";
        public bool IsActive = false;
        public string ActiveValue = "";
        public string PassiveValue = "";
        public DateTime LastActivationTime = DateTime.MinValue;
        public DateTime LastDeactivationTime = DateTime.MinValue;
        public bool WriteToLog = false;
        public string LogActivationText = "";
        public string LogDeactivationText = "";
        public string LogGroupName = "";
        public Int16 LogImageIndex = 0;
        public bool ShowPopupActivation = false;
        public bool ShowPopupDeactivation = false;

        public TFaultRecord()
        {
        }

        public void SaveToStream(FileStream Stream)
        {
            TGlobal.SaveStringToStream(Stream, Name);
            TGlobal.SaveStringToStream(Stream, Group);
            TGlobal.SaveStringToStream(Stream, Description);
            TGlobal.SaveStringToStream(Stream, Type);
            TGlobal.SaveStringToStream(Stream, FaultValue);
            TGlobal.SaveBoolToStream(Stream, IsActive);
            TGlobal.SaveStringToStream(Stream, ActiveValue);
            TGlobal.SaveStringToStream(Stream, PassiveValue);
            TGlobal.SaveDateTimeToStream(Stream, LastActivationTime);
            TGlobal.SaveDateTimeToStream(Stream, LastDeactivationTime);
            TGlobal.SaveBoolToStream(Stream, WriteToLog);
            TGlobal.SaveStringToStream(Stream, LogActivationText);
            TGlobal.SaveStringToStream(Stream, LogDeactivationText);
            TGlobal.SaveStringToStream(Stream, LogGroupName);
            TGlobal.SaveIntToStream(Stream, LogImageIndex);
            TGlobal.SaveBoolToStream(Stream, ShowPopupActivation);
            TGlobal.SaveBoolToStream(Stream, ShowPopupDeactivation);
        }

        public bool LoadFromStream(FileStream Stream)
        {
            bool Result = true;
            Name = TGlobal.LoadStringFromStream(Stream);
            Group = TGlobal.LoadStringFromStream(Stream);
            Description = TGlobal.LoadStringFromStream(Stream);
            Type = TGlobal.LoadStringFromStream(Stream);
            FaultValue = TGlobal.LoadStringFromStream(Stream);
            IsActive = TGlobal.LoadBoolFromStream(Stream);
            ActiveValue = TGlobal.LoadStringFromStream(Stream);
            PassiveValue = TGlobal.LoadStringFromStream(Stream);
            LastActivationTime = TGlobal.LoadDateTimeFromStream(Stream);
            LastDeactivationTime = TGlobal.LoadDateTimeFromStream(Stream);
            WriteToLog = TGlobal.LoadBoolFromStream(Stream);
            LogActivationText = TGlobal.LoadStringFromStream(Stream);
            LogDeactivationText = TGlobal.LoadStringFromStream(Stream);
            LogGroupName = TGlobal.LoadStringFromStream(Stream);
            LogImageIndex = (short)TGlobal.LoadIntFromStream(Stream);
            ShowPopupActivation = TGlobal.LoadBoolFromStream(Stream);
            ShowPopupDeactivation = TGlobal.LoadBoolFromStream(Stream);

            if (Stream.Position >= Stream.Length - 1) Result = false;
            return Result;
        }

        public void Update(TVariableTag Variable, TLogList Log) // Изменили LogClasses на TLogList
        {
            switch (Type)
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

        private void UpdateEqual(TVariableTag Variable, TLogList Log) // Изменили LogClasses на TLogList
        {
            // Активация
            if (!IsActive)
                if (Variable.ValueString == FaultValue)
                {
                    LastActivationTime = DateTime.Now;
                    IsActive = true;
                    // Регистрация в журнале
                    if (WriteToLog) Log.Add(LogGroupName, LogActivationText, LogImageIndex);
                }
            // Деактивация
            if (IsActive)
                if (Variable.ValueString != FaultValue)
                {
                    LastDeactivationTime = DateTime.Now;
                    IsActive = false;
                    // Регистрация в журнале
                    if (WriteToLog) Log.Add(LogGroupName, LogDeactivationText, LogImageIndex);
                }
        }
    }
}