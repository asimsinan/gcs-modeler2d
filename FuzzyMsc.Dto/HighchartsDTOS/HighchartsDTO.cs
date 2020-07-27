using FuzzyMsc.Dto.VisualizationDTOS;
using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class HighchartsDTO
    {
        public HighchartsDTO()
        {
            series = new List<SeriesDTO>();
            annotations = new List<AnnotationsDTO>();
            visualizationInfo = new List<VisualizationDetailDTO>();
        }
        public List<SeriesDTO> series { get; set; }
        public AxisDTO xAxis { get; set; }
        public AxisDTO yAxis { get; set; }
        public List<AnnotationsDTO> annotations { get; set; }
        public ParametersDTO parameters { get; set; }
        public VisualizationCountDTO sayilar { get; set; }
        public List<VisualizationDetailDTO> visualizationInfo { get; set; }
    }
}
