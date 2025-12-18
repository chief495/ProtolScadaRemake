using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtolScadaRemake
{
    public class TTrendTagRecord
    {

        public UInt64 DBId = 0;
        public DateTime datetime = DateTime.MinValue;
        public double ValueReal = 0;
        public string ValueString = "";

        public TTrendTagRecord() // Конструктор
        {
        }

    }
}
