namespace F1_Aggregator
{
    public class MultiData
    {
        public DateTime Expiration { get; set; } = default;
        public Dictionary<string, Data> DataCollection { get; set; } = new();

        public class Data
        {
            public string Output { get; set; } = string.Empty;
            public List<List<string>> Info { get; set; } = new();
            public List<string>? Labels { get; set; } = default;
            public List<int>? Widths { get; set; } = default;
        }
    }
}