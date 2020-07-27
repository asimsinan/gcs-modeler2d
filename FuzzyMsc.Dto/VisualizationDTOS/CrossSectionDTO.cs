using System.Collections.Generic;

namespace FuzzyMsc.Dto.VisualizationDTOS
{
    public class CrossSectionDTO
    {
        public CrossSectionDTO()
        {
            ResList = new List<List<ResistivityDTO>>();
            SisList = new List<List<SeismicDTO>>();
            DrillList = new List<List<DrillingDTO>>();
        }
        public List<List<ResistivityDTO>> ResList { get; set; }
        public List<List<SeismicDTO>> SisList { get; set; }
        public List<List<DrillingDTO>> DrillList { get; set; }
    }
}
