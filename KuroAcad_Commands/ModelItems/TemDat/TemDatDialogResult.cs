namespace KuroAcad.ModelItems
{
    public class TemDatDialogResult
    {
        public int TagCount { get; set; }

        public string BlockName { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int StartNumber { get; set; }

        public string TagName { get; set; } = string.Empty;
        public string TagArea { get; set; } = string.Empty;
        public string TagDensity { get; set; } = string.Empty;
        public string TagFloors { get; set; } = string.Empty;
        public string TagFAR { get; set; } = string.Empty;

        public string ValueDensity { get; set; } = string.Empty;
        public string ValueFloors { get; set; } = string.Empty;
        public string ValueFAR { get; set; } = string.Empty;
    }
}