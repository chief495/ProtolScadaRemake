using System.IO;

namespace ProtolScadaRemake
{
    public class TVariableTag
    {
        public string Name = ""; // Имя тега
        public string Description = ""; // Описание тега
        public string ValueString = "???"; // Строчное значение тега
        public double ValueReal = 0; // Реальное значение тега
        public DateTime LastRead = DateTime.MinValue; // Время последнего чтения
        public string Type = ""; // Тпи тега
        public string TextBefore = ""; // Подпись до значения
        public string Format = ""; // Формат данных тега
        public string TextAfter = ""; // Подпись после значения
        public double Multiplier = 1; // Множитель значения
        public ushort Address = 0; // Адрес переменной в области памяти
        public int AreaGroup = 0; // Идентификационный номер области памяти

        public TVariableTag() // Конструктор
        {
        }
        public void SaveToStream(FileStream Stream) // Сохраняем данные в поток
        {
            TGlobal.SaveStringToStream(Stream, Name); // Имя тега
            TGlobal.SaveStringToStream(Stream, Description); // Описание тега
            TGlobal.SaveStringToStream(Stream, ValueString); // Строчное значение тега
            TGlobal.SaveDoubleToStream(Stream, ValueReal); // Реальное значение тега
            TGlobal.SaveDateTimeToStream(Stream, LastRead); // Время последнего чтения
            TGlobal.SaveStringToStream(Stream, Type); // Тпи тега
            TGlobal.SaveStringToStream(Stream, TextBefore); // Подпись до значения
            TGlobal.SaveStringToStream(Stream, Format); // Формат данных тега
            TGlobal.SaveStringToStream(Stream, TextAfter); // Подпись после значения
            TGlobal.SaveDoubleToStream(Stream, Multiplier); // Множитель значения
            TGlobal.SaveUInt32ToStream(Stream, Address); // Адрес переменной в области памяти
            TGlobal.SaveIntToStream(Stream, AreaGroup); // Идентификационный номер области памяти
        }
        public bool LoadFromStream(FileStream Stream) // Чтение данных из потока
        {
            bool Result = true;
            Name = TGlobal.LoadStringFromStream(Stream); // Имя тега
            Description = TGlobal.LoadStringFromStream(Stream); // Описание тега
            ValueString = TGlobal.LoadStringFromStream(Stream); // Строчное значение тега
            ValueReal = TGlobal.LoadDoubleFromStream(Stream); // Реальное значение тега
            LastRead = TGlobal.LoadDateTimeFromStream(Stream); // Время последнего чтения
            Type = TGlobal.LoadStringFromStream(Stream); // Тип тега
            TextBefore = TGlobal.LoadStringFromStream(Stream); // Подпись до значения
            Format = TGlobal.LoadStringFromStream(Stream); // Формат данных тега
            TextAfter = TGlobal.LoadStringFromStream(Stream); // Подпись после значения
            Multiplier = TGlobal.LoadDoubleFromStream(Stream); // Множитель значения
            Address = (ushort)TGlobal.LoadUInt32FromStream(Stream); // Адрес переменной в области памяти
            AreaGroup = TGlobal.LoadIntFromStream(Stream); // Идентификационный номер области памяти
            if (Stream.Position >= Stream.Length - 1) Result = false; // Защита от неожиданного окончания файла
            return Result;
        }
        public void Read(UInt16[] Data) // Читает из буфера переменную
        {
            if (Data.Length > 0)
            {
                switch (Type)
                {
                    case "Bool":
                        Read_Bool(Data);
                        break;
                    case "Float_32":
                        Read_Float32(Data);
                        break;
                    case "Int_16":
                        Read_Int16(Data);
                        break;
                } // switch (Type)
            } // if (Data.Length > 0)
        }
        public void Read(bool[] Data) // Читает из буфера переменную
        {
            if (Data.Length > 0)
            {
                switch (Type)
                {
                    case "Bool":
                        Read_Bool(Data);
                        break;
                }
            }
        }
        public void Read_Float32(UInt16[] Data) // Читает из буфера переменную типа Float_32
        {
            // Получение области
            byte[] bin = new byte[4];
            ValueString = "---";
            if (Address < (Data.Length - 1))
            {
                bin[1] = (byte)(Data[Address] / 0x100);
                bin[0] = (byte)(Data[Address] % 0x100);
                bin[3] = (byte)(Data[Address + 1] / 0x100);
                bin[2] = (byte)(Data[Address + 1] % 0x100);
                ValueReal = BitConverter.ToSingle(bin, 0);
                ValueReal = ValueReal * Multiplier;
                ValueString = TextBefore + ValueReal.ToString(Format) + TextAfter;
                LastRead = DateTime.Now;
            } // if (StartAddress < (AreaRecord.Data.Length - 1))
        }
        public void Read_Int16(UInt16[] Data) // Читает из буфера переменную типа Int_16
        {
            // Получение области
            ValueString = "---";
            if (Address < (Data.Length))
            {
                byte[] bin = new byte[2];
                bin[1] = (byte)(Data[Address] / 0x100);
                bin[0] = (byte)(Data[Address] % 0x100);
                ValueReal = BitConverter.ToInt16(bin, 0);
                ValueReal = ValueReal * Multiplier;
                ValueString = TextBefore + ValueReal.ToString(Format) + TextAfter;
                LastRead = DateTime.Now;
            } // if (StartAddress < (AreaRecord.Data.Length))
        }
        public void Read_Bool(UInt16[] Data) // Читает из буфера переменную типа Bool
        {
            ValueString = "---";
            if (Address < (Data.Length))
            {
                ValueReal = 0;
                if (Data[Address] > 0) ValueReal = 1;
                string[] values = Format.Split(';');
                ValueString = TextBefore + "false" + TextAfter;
                if (Data[Address] > 0) ValueString = TextBefore + "true" + TextAfter;
                if (values.Length == 2)
                {
                    ValueString = TextBefore + values[0] + TextAfter;
                    if (Data[Address] > 0) ValueString = TextBefore + values[1] + TextAfter;
                }
                LastRead = DateTime.Now;
            } // if (Address < (Data.Length))
        }
        public void Read_Bool(bool[] Data) // Читает из буфера переменную типа Bool
        {
            ValueString = "---";
            if (Address < (Data.Length))
            {
                ValueReal = 0;
                if (Data[Address]) ValueReal = 1;
                string[] values = Format.Split(';');
                ValueString = TextBefore + "false" + TextAfter;
                if (Data[Address]) ValueString = TextBefore + "true" + TextAfter;
                if (values.Length == 2)
                {
                    ValueString = TextBefore + values[0] + TextAfter;
                    if (Data[Address]) ValueString = TextBefore + values[1] + TextAfter;
                }
                LastRead = DateTime.Now;
            }
        }
    }
}