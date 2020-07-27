namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class AxisDTO
    {
        public int min { get; set; }
        public double minTickInterval { get; set; }
        public int offset { get; set; }
        public AxisTitleDTO title { get; set; }
        public AxisLabelsDTO labels { get; set; }
    }

    public class AxisTitleDTO
    {
        public string text { get; set; }
    }
    public class AxisLabelsDTO
    {
        public string format { get; set; }
    }
}

