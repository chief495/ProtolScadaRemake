using System.IO;

namespace ProtolScadaRemake
{
    public class TCommandList
    {
        public TCommandTag[] Items = new TCommandTag[0];
        public TCommandList() // Конструктор
        {
            Items = new TCommandTag[0];
        }
        public int GetCount() // Возвращает количество элементов
        {
            return Items.Length;
        }
        public TCommandTag Add(string Name, string Plc_IpAddress, int Plc_PortNum, int Plc_DeviceAddress, string AreaType, ushort Address, string Type, string Format, string Description) // Добавление записи
        {
            // Создание нового массива
            TCommandTag[] NewItems = new TCommandTag[Items.Length + 1];
            // Копирование существующих элементов в массив
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) NewItems[i] = Items[i];
            // Добавление нового элемента
            NewItems[Items.Length] = new TCommandTag();
            NewItems[Items.Length].Name = Name;
            NewItems[Items.Length].Plc_IpAddress = Plc_IpAddress;
            NewItems[Items.Length].Plc_PortNum = Plc_PortNum;
            NewItems[Items.Length].Plc_DeviceAddress = Plc_DeviceAddress;
            NewItems[Items.Length].AreaType = AreaType;
            NewItems[Items.Length].Address = Address;
            NewItems[Items.Length].Type = Type;
            NewItems[Items.Length].Format = Format;
            NewItems[Items.Length].Description = Description;
            // Подмена массива
            Items = NewItems;
            // Возвращаем результат
            return Items[Items.Length - 1];
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
                TCommandTag[] NewItems = new TCommandTag[0];

                // Oткрытие файла
                FileStream F = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                F.Position = 0;
                // Получение начального значения количества элементов
                UInt32 BeginCount = TGlobal.LoadUInt32FromStream(F);
                // Загрузка элементов
                bool Good = true;
                if (BeginCount > 0)
                {
                    NewItems = new TCommandTag[BeginCount];
                    for (int i = 0; i < BeginCount; i++)
                        if (Good)
                        {
                            NewItems[i] = new TCommandTag();
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
                                    Add(NewItems[NewItemsIndex].Name, NewItems[NewItemsIndex].Plc_IpAddress, NewItems[NewItemsIndex].Plc_PortNum,
                                        NewItems[NewItemsIndex].Plc_DeviceAddress, NewItems[NewItemsIndex].AreaType, (ushort)NewItems[NewItemsIndex].Address,
                                        NewItems[NewItemsIndex].Type, NewItems[NewItemsIndex].Format, NewItems[NewItemsIndex].Description);
                                } // if (!IsFind)
                            } // for (int NewItemsIndex = 0; NewItemsIndex < NewItems.Length; NewItemsIndex++)
                        Result = true;
                    } // if (EndCount == BeginCount)
                F.Close(); // Закрытие файла
            }
            return Result;
        }
        public void SendToController()
        {
            if(Items.Length > 0) for(int i = 0; i < Items.Length;i++) Items[i].SendToController();
        }
        public TCommandTag GetByName(string Name)
        {
            TCommandTag R = null;
            if (Items.Length > 0)
                for (int i = 0; i < Items.Length; i++)
                    if (Items[i].Name == Name) R = (TCommandTag)Items[i];
            return R;
        }
        public void Clear()
        {
            Items = new TCommandTag[0];
        }
    }
}
