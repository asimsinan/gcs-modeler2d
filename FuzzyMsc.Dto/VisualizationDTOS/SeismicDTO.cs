namespace FuzzyMsc.Dto.VisualizationDTOS
{
    public class SeismicDTO
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double? K { get; set; }
        public double? Vp { get; set; }
        public double? Vs { get; set; }
        public bool Checked { get; set; }
    }
}
