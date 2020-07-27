namespace FuzzyMsc.Dto.VisualizationDTOS
{
    public class GraphDTO
    {
        public ExcelModelDTO excel { get; set; }
        public long ruleID { get; set; }
        public ScaleDTO scale { get; set; }
        public ParametersDTO parameters { get; set; }
        public VisualizationCountDTO count { get; set; }
    }
}
