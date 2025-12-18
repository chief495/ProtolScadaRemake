using System.IO;

namespace ProtolScadaRemake
{
    public class TVariableList
    {
        public TVariableTag[] Items = new TVariableTag[0];
        public TVariableList() // Конструктор
        {
            Items = new TVariableTag[0];
        }

        public int GetCount() // Возвращает количество элементов
        {
            return Items.Length;
        }
        public void Clear() // Отчистка массива
        {
            Items = new TVariableTag[0];
        }
        public TVariableTag Add(string Name, int AreaGroup, ushort Address, double Multiplier, string Type, string TextBefore, string Format, string TextAfter, string Description) // Добавление записи
        {
            // Создание нового массива
            TVariableTag[] NewItems = new TVariableTag[Items.Length + 1];
            // Копирование существующих элементов в массив
            if(Items.Length > 0 ) for( int i = 0; i < Items.Length; i++) NewItems[i] = Items[i];
            // Добавление нового элемента
            NewItems[Items.Length] = new TVariableTag();
            NewItems[Items.Length].Name = Name;
            NewItems[Items.Length].AreaGroup = AreaGroup;
            NewItems[Items.Length].Address = Address;
            NewItems[Items.Length].Multiplier = Multiplier;
            NewItems[Items.Length].Type = Type;
            NewItems[Items.Length].TextBefore = TextBefore;
            NewItems[Items.Length].Format = Format;
            NewItems[Items.Length].TextAfter = TextAfter;
            NewItems[Items.Length].Description = Description;
            // Подмена массива
            Items = NewItems;
            // Возвращение результата
            return Items[Items.Length - 1];
        }
        public void ReadGroup(UInt16[] Data, int Group)
        {
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) if (Items[i].AreaGroup == Group) Items[i].Read(Data);
        }
        public void ReadGroup(bool[] Data, int Group)
        {
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) if (Items[i].AreaGroup == Group) Items[i].Read(Data);
        }
        public TVariableTag GetByName(string Name)
        {
            TVariableTag R = null;
            if(Items.Length > 0)
                for(int i = 0;i < Items.Length;i++)
                    if (Items[i].Name == Name) R = (TVariableTag)Items[i];
            return R;
        }
        public void SaveToFile(string FileName)
        {
            // Создание или открытие файла
            FileStream F = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            F.Position = 0;
            // Сохранение количества записей
            TGlobal.SaveUInt32ToStream(F, Convert.ToUInt32(GetCount()));
            // Сохранение элементов
            if(Items.Length > 0) for(int i = 0;i < Items.Length;i++) Items[i].SaveToStream(F);
            // Сохранение количества записей
            TGlobal.SaveUInt32ToStream(F, Convert.ToUInt32(GetCount()));
            // Закрытие файла
            F.Close();
        }
        public bool LoadFromFile(string FileName)
        {
            bool Result = false;
            if(File.Exists(FileName))
            {
                // Создание нового массива данных
                TVariableTag[] NewItems = new TVariableTag[0];
                // Oткрытие файла
                FileStream F = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                F.Position = 0;
                // Получение начального значения количества элементов
                UInt32 BeginCount = TGlobal.LoadUInt32FromStream(F);
                // Загрузка элементов
                bool Good = true;
                if (BeginCount > 0)
                {
                    NewItems = new TVariableTag[BeginCount];
                    for(int i = 0;i < BeginCount;i++)
                    if(Good)
                    {
                        NewItems[i] = new TVariableTag();
                        Good = NewItems[i].LoadFromStream(F);
                    }
                }
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
                                    Add(NewItems[NewItemsIndex].Name, NewItems[NewItemsIndex].AreaGroup, NewItems[NewItemsIndex].Address,
                                        NewItems[NewItemsIndex].Multiplier, NewItems[NewItemsIndex].Type, NewItems[NewItemsIndex].TextBefore,
                                        NewItems[NewItemsIndex].Format, NewItems[NewItemsIndex].TextAfter, NewItems[NewItemsIndex].Description);
                                }
                            } // for (int NewItemsIndex = 0; NewItemsIndex < NewItems.Length; NewItemsIndex++)
                        Result = true;
                    } // if (Good) if (EndCount == BeginCount)
                // Закрытие файла
                F.Close();
            }
            return Result;
        }

    }
}
