using System.IO;

namespace ProtolScadaRemake
{
    public class TLogList
    {
        // Настройки подключения к базе данных
        public string DB_HostName = ""; // Адрес хоста базы данных
        public int DB_Port = 0;                    // Номер порта базы данных
        public string DB_UserLogin = "";         // Имя пользователя базы данных
        public string DB_Password = "";          // Пароль к базе данных
        public string DB_Name = "";         // Имя базы данных
        // Прочие начтройки
        public int NormalMaxLengh = 1024; // Максимальный размер массива при наличии связи с БД


        public TLogRecord[] Items = new TLogRecord[0]; // Массмв элементов
        public TLogList() // Конструктор
        {
            Items = new TLogRecord[0];
        }
        public int GetCount() // Возвращает количество элементов
        {
            return Items.Length;
        }
        public void Clear() // Отчистка массива
        {
            Items = new TLogRecord[0];
        }
        public void Add(string GroupName, string Text, Int16 ImageIndex) // Добавление записи
        {
            // Создание нового массива
            TLogRecord[] NewItems = new TLogRecord[Items.Length + 1];
            // Копирование существующих элементов в массив
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) NewItems[i] = Items[i];
            // Добавление нового элемента
            NewItems[Items.Length] = new TLogRecord();
            NewItems[Items.Length].ID = -1;
            NewItems[Items.Length].Time = DateTime.Now;
            NewItems[Items.Length].GroupName = GroupName;
            NewItems[Items.Length].Text = Text;
            NewItems[Items.Length].ImageIndex = ImageIndex;
            // Подмена массива
            Items = NewItems;
        }
        public void DataBaseUpdate(TGlobal Global) //  Публикация записей в базе данных
        {
            if (Global.Log.GetCount() > 0)
                for(int i = 0  ; i < Global.Log.GetCount() ; i++)
                    if (Global.Log.Items[i].ID < 0)
                    {
                        //// Создание объекта подключения к БД
                        //DBUtils DB = new DBUtils();
                        // Установка параметров подключения
                        //DB.DB_HostName = Global.DB_HostName;
                        //DB.DB_Port = Global.DB_Port;
                        //DB.DB_UserLogin = Global.DB_UserLogin;
                        //DB.DB_Password = Global.DB_Password;
                        //DB.DB_Name = Global.DB_Name;
                        //// Обновление записей в БД
                        //Global.Log.Items[i].ID = DB.Refresh_LogRecord(Global.Log.Items[i]);
                    }
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
                TLogRecord[] NewItems = new TLogRecord[0];
                // Oткрытие файла
                FileStream F = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                F.Position = 0;
                // Получение начального значения количества элементов
                UInt32 BeginCount = TGlobal.LoadUInt32FromStream(F);
                // Загрузка элементов
                bool Good = true;
                if (BeginCount > 0)
                {
                    NewItems = new TLogRecord[BeginCount];
                    for (int i = 0; i < BeginCount; i++)
                        if (Good)
                        {
                            NewItems[i] = new TLogRecord();
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
                                        if (Items[ItemsIndex].ID == NewItems[NewItemsIndex].ID)
                                            if (Items[ItemsIndex].Time == NewItems[NewItemsIndex].Time)
                                                if (Items[ItemsIndex].GroupName == NewItems[NewItemsIndex].GroupName)
                                                    if (Items[ItemsIndex].Text == NewItems[NewItemsIndex].Text)
                                                        if (Items[ItemsIndex].ImageIndex == NewItems[NewItemsIndex].ImageIndex)
                                                        {
                                                            Items[ItemsIndex] = NewItems[NewItemsIndex];
                                                            IsFind = true;
                                                        } // if (Items[ItemsIndex].ID == NewItems[NewItemsIndex].ID)...
                                if (!IsFind)
                                {
                                    Add(NewItems[NewItemsIndex].GroupName, NewItems[NewItemsIndex].Text, NewItems[NewItemsIndex].ImageIndex);
                                    Items[Items.Length - 1].ID = NewItems[NewItemsIndex].ID;
                                    Items[Items.Length - 1].Time = NewItems[NewItemsIndex].Time;
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
