namespace F1_Aggregator
{
    public class Data
    {
        public DateTime Expiration { get; set; }
        public string Name { get; set; }
        public List<string> Labels { get; set; }
        public List<int> Widths { get; set; }
        public List<List<string>> Info { get; set; }

        public Data()
        {
            Expiration = DateTime.MinValue;
            Name = string.Empty;
            Labels = new();
            Widths = new();
            Info = new();
        }
    }
}