// TTrendTagRecord.cs
namespace ProtolScadaRemake
{
    public class TTrendTagRecord
    {
        public UInt64 DBId { get; set; } = 0;
        public DateTime DateTime { get; set; } = DateTime.MinValue;
        public double ValueReal { get; set; } = 0;
        public string ValueString { get; set; } = "";

        public TTrendTagRecord() { }
    }
}