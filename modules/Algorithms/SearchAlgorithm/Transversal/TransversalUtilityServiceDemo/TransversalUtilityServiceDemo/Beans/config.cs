namespace TransversalUtilityServiceDemo.Beans
{
    public class JobConfig
    {
        public List<JobItem> Jobs { get; set; }
    }

    public class JobItem
    {
        public string Type { get; set; }
        public EffectsConfig Effects { get; set; }
    }

    public class EffectsConfig
    {
        public string To { get; set; }
        public BuffConfig Buff { get; set; }
    }

    public class BuffConfig
    {
        public string Type { get; set; }
        public string Func { get; set; } // JSON 中是字串形式的表達式
    }
}