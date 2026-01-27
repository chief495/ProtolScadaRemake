// TTrendList.cs
namespace ProtolScadaRemake
{
    public class TTrendList
    {
        public TTrendTag[] Items { get; private set; } = new TTrendTag[0];

        public TTrendList() { }

        public int GetCount() => Items.Length;

        public void Clear()
        {
            Items = new TTrendTag[0];
        }

        public TTrendTag Add(string name, string description, string unit = "", ushort period = 60, uint maxLength = 1000)
        {
            var newTag = new TTrendTag(name, description, unit, period, maxLength);

            var newItems = new TTrendTag[Items.Length + 1];
            Array.Copy(Items, newItems, Items.Length);
            newItems[Items.Length] = newTag;
            Items = newItems;

            return newTag;
        }

        public TTrendTag GetByName(string name)
        {
            return Items.FirstOrDefault(t => t.Name == name);
        }

        public void Update(TVariableList variables)
        {
            foreach (var trend in Items)
            {
                var variable = variables.GetByName(trend.Name);
                if (variable != null)
                {
                    trend.Update(variable);
                }
            }
        }
    }
}