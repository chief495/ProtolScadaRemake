using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProtolScadaRemake
{
    public class TLogRecord
    {
        public Int64 ID = -1; // ID в базе данных
        public DateTime Time = DateTime.MinValue; // Время события
        public string GroupName = ""; // Группа
        public string Text = ""; // Текст
        public Int16 ImageIndex = 0; // 0 - Информацирнное сообщение
                                     // 1 - Действие пользователя
                                     // 2 - Предупреждение
                                     // 3 - Событие
        public TLogRecord() // Конструктор
        { 
        }
        public void SaveToStream(FileStream Stream) // Сохраняем данные в поток
        {
            TGlobal.SaveInt64ToStream(Stream, ID); // ID в базе данных
            TGlobal.SaveDateTimeToStream(Stream, Time); // Время события
            TGlobal.SaveStringToStream(Stream, GroupName); // Группа
            TGlobal.SaveStringToStream(Stream, Text); // Текст
            TGlobal.SaveIntToStream(Stream, ImageIndex); // Индекс изображения
        }
        public bool LoadFromStream(FileStream Stream) // Чтение данных из потока
        {
            bool Result = true;
            ID = TGlobal.LoadInt64FromStream(Stream); // ID в базе данных
            Time = TGlobal.LoadDateTimeFromStream(Stream); // Время события
            GroupName = TGlobal.LoadStringFromStream(Stream); // Группа
            Text = TGlobal.LoadStringFromStream(Stream); // Текст
            ImageIndex = (short)TGlobal.LoadIntFromStream(Stream); // Индекс изображения
            if (Stream.Position >= Stream.Length - 1) Result = false; // Защита от неожиданного окончания файла
            return Result;
        }
    }
}
