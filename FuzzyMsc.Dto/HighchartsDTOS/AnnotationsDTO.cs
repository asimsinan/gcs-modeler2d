using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
  public class AnnotationsDTO
  {
    public AnnotationsDTO()
    {
      labels = new List<AnnotationLabelsDTO>();
    }
    //public AnnotationLabelOptionsDTO labelOptions { get; set; }
    public List<AnnotationLabelsDTO> labels { get; set; }
    public bool? visible { get; set; }

    //public AnnotationLabelOptionsDTO labelOptions { get; set; }
    

  }

  public class AnnotationLabelOptionsDTO
  {
    public string shape { get; set; }
    public string align { get; set; }
    public bool justify { get; set; }
    public bool allowOverlap { get; set; }
    public string backgroundColor { get; set; }
    public string borderColor { get; set; }
    public double borderRadius { get; set; }
    public double borderWidth { get; set; }
    public bool crop { get; set; }
    //public StyleDTO style { get; set; }

  }

  public class AnnotationLabelsDTO
  {
    public PointDTO point { get; set; }
    public string text { get; set; }
    public double? x { get; set; }
    public double? y { get; set; }
    public string shape { get; set; }
    public bool allowOverlap { get; set; }
    //public StyleDTO style { get; set; }
  }
  public class ShapeOptionsDTO
  {
  }

  public class ShapesDTO
  {

  }
  public class StyleDTO
  {
    public string fontSize { get; set; }
    public string color { get; set; }
  }

  public class PointDTO
  {
    public double? xAxis { get; set; }
    public double? yAxis { get; set; }
    public double? x { get; set; }
    public double? y { get; set; }
  }
}
