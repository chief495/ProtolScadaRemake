using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProtolScadaRemake
{
    public class TTrendList
    {
        public TTrendTag[] Items = new TTrendTag[0];
        public TTrendList() // Конструктор
        {
            Items = new TTrendTag[0];
        }
        public int GetCount() // Возвращает количество элементов
        {
            return Items.Length;
        }
        public void Clear() // Отчистка массива
        {
            Items = new TTrendTag[0];
        }
        public TTrendTag Add(string Name, string Description, UInt16 Period, UInt32 MaxLength)
        {
            // Создание нового массива
            TTrendTag[] NewItems = new TTrendTag[Items.Length + 1];
            // Копирование существующих элементов в массив
            if (Items.Length > 0) for (int i = 0; i < Items.Length; i++) NewItems[i] = Items[i];
            // Добавление нового элемента
            NewItems[Items.Length] = new TTrendTag();
            NewItems[Items.Length].Name = Name;
            NewItems[Items.Length].Description = Description;
            NewItems[Items.Length].Period = Period;
            NewItems[Items.Length].MaxLength = MaxLength;
            // Подмена массива
            Items = NewItems;
            // Возвращение результата
            return Items[Items.Length - 1];
        }
        public void Update(TVariableList variables)
        {
            if (Items.Length > 0)
                for (int i = 0; i < Items.Length; i++)
                    Items[i].Update(variables.GetByName(Items[i].Name));
        }
    }
}
