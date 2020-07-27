namespace FuzzyMsc.Dto.VisualizationDTOS
{
    public class ResistivityDTO
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double? K { get; set; }
        public double? R { get; set; }
        public bool Checked { get; set; }
        public byte? TypeID { get; set; }
    }
}
