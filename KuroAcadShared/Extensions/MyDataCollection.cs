namespace KuroAcad.Extensions
{
    public class MyDataCollection
    {
        private List<MyData> _dataItems;

        public MyDataCollection()
        {
            _dataItems = new List<MyData>();
        }

        public void AddDataItem(string category, int value, string blockAttribute)
        {
            MyData item = _dataItems.Find(x => x.Category == category);
            if (item == null)
            {
                item = new MyData(category, value, blockAttribute);
                _dataItems.Add(item);
            }
            else
            {
                item.Value += value;
                item.BlockAttribute = blockAttribute;
            }
        }

        public List<MyData> GetDataItems()
        {
            return _dataItems;
        }

    }

    public class MyData
    {
        public string Category { get; private set; }
        public int Value { get; set; }
        public string BlockAttribute { get; set; }

        public MyData(string category, int value, string blockAttribute)
        {
            Category = category;
            Value = value;
            BlockAttribute = blockAttribute;
        }
    }
}
