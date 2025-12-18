using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TTrendTag
    {
        public string Name = ""; // Имя тега
        public string Description = ""; // Описание тега
        public UInt16 Period = 60; // Период ваполнения записи в секундах
        public UInt32 MaxLength = 1000; // Максимальное количество записей
        public TTrendTagRecord[] Records = new TTrendTagRecord[0]; // Массив значений

        public TTrendTag() // Конструктор
        {
        }
        public void Update(TVariableTag Variable)
        {
            if(Variable != null)
            {
                bool NedUpdate = true;
                // Определяем, требуется ли запись
                if(Records.Length > 0) if (Records[Records.Length - 1].datetime.AddSeconds(Period) > DateTime.Now) NedUpdate = false;
                if (NedUpdate) Add(Variable);
            } // if(Variable != null)
        }
        public TTrendTagRecord Add(TVariableTag Variable)
        {
            // Создание нового массива
            TTrendTagRecord[] NewItems = new TTrendTagRecord[Records.Length + 1];
            if (Records.Length >= MaxLength) NewItems = new TTrendTagRecord[MaxLength];
            // Копирование существующих элементов в массив
            if (Records.Length > 0)
            {
                if(Records.Length < MaxLength) for (int i = 0; i < Records.Length; i++) NewItems[i] = Records[i];
                else for (int i = 0; i < MaxLength - 1; i++) NewItems[i] = Records[i + Records.Length - MaxLength];
            } // if (Records.Length > 0)
            // Добавление запмсм
            int index = NewItems.Length - 1;
            NewItems[index] = new TTrendTagRecord();
            NewItems[index].ValueReal = Variable.ValueReal;
            NewItems[index].ValueString = Variable.ValueString;
            NewItems[index].datetime = DateTime.Now;
            NewItems[index].DBId = 0;
            // Подмена массива
            Records = NewItems;
            // Возвращение результата
            return Records[Records.Length - 1];
        }
        public int GetCount()
        {
            return Records.Length;
        }

    }
}
