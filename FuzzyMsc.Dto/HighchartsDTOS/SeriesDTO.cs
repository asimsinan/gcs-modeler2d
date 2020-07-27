using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class SeriesDTO
    {
        public SeriesDTO()
        {
            data = new List<List<double>>();
        }
        public string name { get; set; }
        public double lineWidth { get; set; }
        public List<string> keys { get; set; }
        public string color { get; set; }
        public List<List<double>> data { get; set; }
        public MarkerDTO marker { get; set; }
        public ToolTipDTO tooltip { get; set; }
        public bool showInLegend { get; set; }
        public StatesDTO states { get; set; }
        public bool? enableMouseTracking { get; set; }
        public bool draggableY { get; set; }
        public bool draggableX { get; set; }
        public bool? visible { get; set; }
    }

    public class ToolTipDTO
    {
        public bool useHTML { get; set; }
        public string headerFormat { get; set; }
        public string pointFormat { get; set; }
        public string footerFormat { get; set; }
        public int valueDecimals { get; set; }
    }

    public class MarkerDTO
    {
        public bool enabled { get; set; }
        public string symbol { get; set; }
        public double radius { get; set; }
    }

    public class StatesDTO
    {
        public HoverDTO hover { get; set; }
    }
    public class HoverDTO
    {
        public int lineWidthPlus { get; set; }
    }
}
