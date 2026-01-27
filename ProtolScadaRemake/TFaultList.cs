using System.IO;

namespace ProtolScadaRemake
{
    public class TFaultList
    {
        public TFaultRecord[] Items = new TFaultRecord[0]; // Массмв элементов
        public TFaultList() // Конструктор
        {
            Items = new TFaultRecord[0];
        }
        public int GetCount() // Возвращает количество элементов
        {
            return Items.Length;
        }
        public void Clear() // Отчистка массива
        {
            Items = new TFaultRecord[0];
        }
        public void Add(string Name, string Group, string Description, string Type, string FaultValue, string PassiveValue, string ActiveValue, bool WriteToLog,
                string LogDeactivationText, string LogActivationText, string LogGroupName, Int16 LogImageIndex, bool ShowPopupDeactivation,
                bool ShowPopupActivation) // Добавление записи
        {
            // Создание нового массива
            TFaultRecord[] NewItems = new TFaultRecord[Items.Length + 1];
            // Копирование существующих элементов в массив
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) NewItems[i] = Items[i];
            // Добавление нового элемента
            NewItems[Items.Length] = new TFaultRecord();
            NewItems[Items.Length].Name = Name;
            NewItems[Items.Length].Group = Group;
            NewItems[Items.Length].Description = Description;
            NewItems[Items.Length].Type = Type;
            NewItems[Items.Length].FaultValue = FaultValue;
            NewItems[Items.Length].PassiveValue = PassiveValue;
            NewItems[Items.Length].ActiveValue = ActiveValue;
            NewItems[Items.Length].WriteToLog = WriteToLog;
            NewItems[Items.Length].LogDeactivationText = LogDeactivationText;
            NewItems[Items.Length].LogActivationText = LogActivationText;
            NewItems[Items.Length].LogGroupName = LogGroupName;
            NewItems[Items.Length].LogImageIndex = LogImageIndex;
            NewItems[Items.Length].ShowPopupDeactivation = ShowPopupDeactivation;
            NewItems[Items.Length].ShowPopupActivation = ShowPopupActivation;
            // Подмена массива
            Items = NewItems;
        }
        public void SaveToFile(string FileName)
        {
            // Создание или открытие файла
            FileStream F = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            F.Position = 0;
            // Сохранение количества записей
            TGlobal.SaveUInt32ToStream(F, Convert.ToUInt32(GetCount()));
            // Сохранение элементов
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) Items[i].SaveToStream(F);
            // Сохранение количества записей
            TGlobal.SaveUInt32ToStream(F, Convert.ToUInt32(GetCount()));
            // Закрытие файла
            F.Close();
        }
        public bool LoadFromFile(string FileName)
        {
            bool Result = false;
            if (File.Exists(FileName))
            {
                // Создание нового массива данных
                TFaultRecord[] NewItems = new TFaultRecord[0];
                // Oткрытие файла
                FileStream F = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                F.Position = 0;
                // Получение начального значения количества элементов
                UInt32 BeginCount = TGlobal.LoadUInt32FromStream(F);
                // Загрузка элементов
                bool Good = true;
                if (BeginCount > 0)
                {
                    NewItems = new TFaultRecord[BeginCount];
                    for (int i = 0; i < BeginCount; i++)
                        if (Good)
                        {
                            NewItems[i] = new TFaultRecord();
                            Good = NewItems[i].LoadFromStream(F);
                        } // if (Good)
                } // if (BeginCount > 0)
                // Получение конечного значения количества элементов
                UInt32 EndCount = TGlobal.LoadUInt32FromStream(F);
                // Подготовка результатов
                Result = false;
                if (Good)
                    if (EndCount == BeginCount)
                    {
                        if (NewItems.Length > 0)
                            for (int NewItemsIndex = 0; NewItemsIndex < NewItems.Length; NewItemsIndex++)
                            {
                                bool IsFind = false;
                                if (Items.Length > 0)
                                    for (int ItemsIndex = 0; ItemsIndex < Items.Length; ItemsIndex++)
                                        if (Items[ItemsIndex].Name == NewItems[NewItemsIndex].Name)
                                        {
                                            Items[ItemsIndex] = NewItems[NewItemsIndex];
                                            IsFind = true;
                                        } // if (Items[ItemsIndex].Name == NewItems[NewItemsIndex].Name)
                                if (!IsFind)
                                {
                                    Add(NewItems[NewItemsIndex].Name, NewItems[NewItemsIndex].Group, NewItems[NewItemsIndex].Description, NewItems[NewItemsIndex].Type,
                                        NewItems[NewItemsIndex].FaultValue, NewItems[NewItemsIndex].PassiveValue, NewItems[NewItemsIndex].ActiveValue,
                                        NewItems[NewItemsIndex].WriteToLog, NewItems[NewItemsIndex].LogDeactivationText, NewItems[NewItemsIndex].LogActivationText,
                                        NewItems[NewItemsIndex].LogGroupName, NewItems[NewItemsIndex].LogImageIndex, NewItems[NewItemsIndex].ShowPopupDeactivation,
                                        NewItems[NewItemsIndex].ShowPopupActivation);

                                } // if (!IsFind)
                            } // for (int NewItemsIndex = 0; NewItemsIndex < NewItems.Length; NewItemsIndex++)
                        Result = true;
                    } // if (EndCount == BeginCount)
                // Закрытие файла
                F.Close();
            } // if (File.Exists(FileName))
            return Result;
        }
    }
}
