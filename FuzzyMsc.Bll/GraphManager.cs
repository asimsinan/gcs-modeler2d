using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.VisualizationDTOS;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Dto.HighchartsDTOS;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FuzzyMsc.Entity.Model;

namespace FuzzyMsc.Bll
{
  public class GraphManager : IGraphManager
  {
    IUnitOfWorkAsync _unitOfWork;
    IInputManager _inputManager;
    IUserService _userService;
    IRuleService _ruleService;
    IRuleListService _ruleListService;
    IRuleListItemService _ruleListItemService;
    IRuleListTextService _ruleListTextService;
    IVariableService _variableService;
    IVariableItemService _variableItemService;
    IFuzzyManager _fuzzyManager;

    SessionDTO session = new SessionDTO();
    private List<List<ResistivityDTO>> resList;
    private List<List<SeismicDTO>> sisList;
    private List<List<DrillingDTO>> drillList;
    private VisualizationCountDTO visualizationCount = new VisualizationCountDTO();
    private List<VisualizationDetailDTO> visualizationDetailList = new List<VisualizationDetailDTO>();
    private List<SeriesDTO> datasetList = new List<SeriesDTO>();
    private int id;
    Microsoft.Office.Interop.Excel.Application xl;
    Microsoft.Office.Interop.Excel.Workbook xlWorkbook;

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

    public GraphManager(
        IUnitOfWorkAsync unitOfWork,
        IUserService userService,
        IInputManager inputManager,
        IRuleService ruleService,
        IRuleListService ruleListService,
        IRuleListItemService ruleListItemService,
        IRuleListTextService ruleListTextService,
        IVariableService variableService,
        IVariableItemService variableItemService,
        IFuzzyManager fuzzyManager)
    {
      _unitOfWork = unitOfWork;
      _inputManager = inputManager;
      _userService = userService;
      _ruleService = ruleService;
      _ruleListService = ruleListService;
      _ruleListTextService = ruleListTextService;
      _variableService = variableService;
      _variableItemService = variableItemService;
      _ruleListItemService = ruleListItemService;
      _fuzzyManager = fuzzyManager;
    }

    public ResultDTO CheckExcel(ExcelModelDTO excel, string path)
    {
      ResultDTO result = new ResultDTO();
      try
      {
        result.Success = true;
        File.WriteAllBytes(path, Convert.FromBase64String(excel.data));
      }
      catch (Exception ex)
      {
        result.Success = false;
      }
      return result;


    }
    public ResultDTO Visualize(GraphDTO graph, string path, SessionDTO sessionItem)
    {
      session = sessionItem;
      try
      {
        ResultDTO result = new ResultDTO();
        File.WriteAllBytes(path, Convert.FromBase64String(graph.excel.data));
        xl = new Microsoft.Office.Interop.Excel.Application();
        xlWorkbook = xl.Workbooks.Open(path);
        GetWindowThreadProcessId(xl.Hwnd, out id);
        HighchartsDTO highcharts = new HighchartsDTO();

        #region Resistivity
        CreateResistivity(highcharts, xlWorkbook);
        #endregion

        #region Seismic
        CreateSeismic(highcharts, xlWorkbook);
        #endregion

        #region Drilling
        CreateDrilling(highcharts, xlWorkbook);
        #endregion

        //highcharts.series.AddRange(GraphDataOlustur(sisGenelList));
        //highcharts.series.AddRange(GraphDataOlustur(sonGenelList));
        //highcharts.series.AddRange(GraphDataOlustur(rezGenelList));
        CrossSectionDTO crossSectionDTO = new CrossSectionDTO { ResList = resList, SisList = sisList, DrillList = drillList };
        highcharts.series.AddRange(CreateGraphData(graph.ruleID, crossSectionDTO, graph.parameters));

        bool isFault = highcharts.series.Any(s => s.name == "Fault");

        if (isFault)
          highcharts.series.AddRange(CreateGraphData(graph.ruleID, crossSectionDTO, graph.parameters));

        highcharts.series = highcharts.series.Distinct().ToList();

        double minX = CalculateMin(highcharts);
        highcharts.xAxis = new AxisDTO { min = 0, minTickInterval = (int)graph.parameters.ScaleX, offset = 20, title = new AxisTitleDTO { text = "Width" }, labels = new AxisLabelsDTO { format = "{value} m" } };
        highcharts.yAxis = new AxisDTO { min = (int)minX - 5, minTickInterval = (int)graph.parameters.ScaleY, offset = 20, title = new AxisTitleDTO { text = "Height" }, labels = new AxisLabelsDTO { format = "{value} m" } };

        highcharts.parameters = graph.parameters;

        highcharts.visualizationInfo = visualizationDetailList;

        result.ResultObject = highcharts;
        result.Success = true;
        return result;
      }
      finally
      {
        xlWorkbook.Close();
        xl.Quit();
        Process process = Process.GetProcessById(id);
        process.Kill();

      }
    }

    private double CalculateSuccess(VisualizationCountDTO visualizationCount, VisualizationCountDTO defaultCount)
    {
      double ratio = 100.0;

      int normalDiff = Math.Abs(visualizationCount.Normal - defaultCount.Normal);
      int closureDiff = Math.Abs(visualizationCount.Closure - defaultCount.Closure);
      int faultDiff = Math.Abs(visualizationCount.Fault - defaultCount.Fault);

      for (int i = 0; i < faultDiff; i++)
      {
        ratio = ratio - (ratio * 15 / 100);
      }
      for (int i = 0; i < closureDiff; i++)
      {
        ratio = ratio - (ratio * 7.5 / 100);
      }
      for (int i = 0; i < normalDiff; i++)
      {
        ratio = ratio - (ratio * 1 / 100);
      }

      ratio = ratio - (ratio * 1 / 100);

      ratio = ratio - (ratio * (normalDiff + closureDiff + faultDiff) / 100);

      return ratio;
    }

    private double CalculateMin(HighchartsDTO highcharts)
    {
      double min = Double.MaxValue;

      foreach (var item in highcharts.annotations)
      {
        var minItem = (double)item.labels.Min(m => m.point.y);
        if (minItem != 0 && minItem < min)
          min = minItem;
      }
      return min;
    }

    private void CreateResistivity(HighchartsDTO highcharts, Workbook xlWorkbook)
    {
      resList = new List<List<ResistivityDTO>>();
      List<ResistivityDTO> rezList = new List<ResistivityDTO>();
      ResistivityDTO rezItem = new ResistivityDTO();
      Microsoft.Office.Interop.Excel._Worksheet xlWorksheetResistivity = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];
      Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetResistivity.UsedRange;
      #region Table Raw Col
      int rowCount = xlRange.Rows.Count;
      int colCount = xlRange.Columns.Count;
      for (int i = 1; i <= rowCount; i++)
      {
        if (string.IsNullOrEmpty((xlWorksheetResistivity.Cells[i + 1, 1]).Value))
        {
          rowCount = i;
          break;
        }
      }
      #endregion

      #region Depth Equality

      List<List<ExcelDTO>> rezExcel = new List<List<ExcelDTO>>();
      List<ExcelDTO> rezExcelItem;
      #region Data
      for (int i = 2; i < rowCount + 1; i++)
      {
        rezExcelItem = new List<ExcelDTO>();
        for (int j = 1; j < colCount + 1; j++)
        {
          ExcelDTO Instance;
          if ((xlWorksheetResistivity.Cells[i, j]).Value == null)
          {
            Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
            rezExcelItem.Add(Instance);
          }
          else
          {
            var value = (string)(xlWorksheetResistivity.Cells[i, j]).Value.ToString();
            Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
            rezExcelItem.Add(Instance);
          }

        }
        rezExcel.Add(rezExcelItem);
      }
      #endregion

      #region Add Data
      foreach (var item in rezExcel)
      {
        if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "")
        {
          for (int i = 0; i < item.Count; i++)
          {
            if (item[i].Value == "" && item[i + 1].Value == "")
            {
              item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
              item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

              List<ExcelDTO> finalItem = new List<ExcelDTO>();
              finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
              finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

              for (int j = i; j < item.Count; j = j + 2)
              {
                item[j - 2].JSONData = JsonConvert.SerializeObject(item[i - 4]);
                item[j - 1].JSONData = JsonConvert.SerializeObject(item[i - 3]);
                item[j - 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 2].JSONData);
                item[j - 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 1].JSONData);
                item[j - 4].TypeID = (byte)Enums.ExcelDataType.Artificial;
                item[j - 3].TypeID = (byte)Enums.ExcelDataType.Artificial;

                if (j == item.Count - 2)
                {
                  item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]);
                  item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
                  item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
                  item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
                  item[j - 2].TypeID = (byte)Enums.ExcelDataType.Real;
                  item[j - 1].TypeID = (byte)Enums.ExcelDataType.Real;
                  continue;
                }
              }
              break;
            }
          }
        }
      }
      #endregion

      #endregion



      for (int i = 0; i < rowCount - 1; i++)
      {
        rezItem = new ResistivityDTO();
        rezItem.ID = i + 1;
        rezItem.Name = rezExcel[i][0].Value.ToString();
        rezItem.X = Convert.ToDouble(rezExcel[i][1].Value);
        rezItem.K = Convert.ToDouble(rezExcel[i][3].Value);
        rezItem.TypeID = rezExcel[i][0].TypeID;
        rezList.Add(rezItem);
      }
      resList.Add(rezList);

      int count = 0;
      for (int j = 4; j < colCount; j = j + 2)
      {
        count++;
        rezList = new List<ResistivityDTO>();
        for (int i = 0; i < rowCount - 1; i++)
        {
          var rezExcelInstance = rezExcel[i];

          if (rezExcelInstance[j].Value == "" && rezExcelInstance[j + 1].Value == "")
          {
            continue;
          }
          if (rezExcelInstance[j + 1].Value == "")
          {
            continue;
          }
          if (rezExcelInstance[j].Value == "" && rezExcelInstance[j + 1].Value != "")
          {
            rezItem = new ResistivityDTO();
            rezItem.ID = i + 1;
            rezItem.Name = rezExcelInstance[0].Value.ToString() + count.ToString();
            rezItem.X = Convert.ToDouble(rezExcelInstance[1].Value);
            var value = "";
            for (int k = 0; k < rezExcelInstance.Count; k = k + 2)
            {
              if (rezExcelInstance[j - (2 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
              {
                value = rezExcelInstance[j - (2 + k)].Value;
                break;
              }
            }
            rezItem.K = (Convert.ToDouble(rezExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
            rezItem.R = rezExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(rezExcelInstance[j + 1].Value);
            rezItem.TypeID = rezExcelInstance[j].TypeID;
            rezList.Add(rezItem);
            continue;
          }
          rezItem = new ResistivityDTO();
          rezItem.ID = i + 1;
          rezItem.Name = rezExcelInstance[0].Value.ToString() + count.ToString();
          rezItem.X = Convert.ToDouble(rezExcelInstance[1].Value);
          rezItem.K = rezExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(rezExcelInstance[3].Value) - Convert.ToDouble(rezExcelInstance[j].Value);
          rezItem.R = rezExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(rezExcelInstance[j + 1].Value);
          rezItem.TypeID = rezExcelInstance[j].TypeID;
          rezList.Add(rezItem);

        }
        resList.Add(rezList);
      }

      highcharts = CreateChart(highcharts, resList);
    }
    private void CreateSeismic(HighchartsDTO highcharts, Workbook xlWorkbook)
    {
      this.sisList = new List<List<SeismicDTO>>();
      List<SeismicDTO> sisList = new List<SeismicDTO>();
      SeismicDTO sisItem = new SeismicDTO();
      Microsoft.Office.Interop.Excel._Worksheet xlWorksheetSismik = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[2];
      Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheetSismik.UsedRange;
      #region Table Row Col
      int rowCount = xlRange.Rows.Count;
      int colCount = xlRange.Columns.Count;
      for (int i = 1; i <= rowCount; i++)
      {
        if (string.IsNullOrEmpty((xlWorksheetSismik.Cells[i + 1, 1]).Value))
        {
          rowCount = i;
          break;
        }
      }
      #endregion

      #region Depth Equality

      List<List<ExcelDTO>> sisExcel = new List<List<ExcelDTO>>();
      List<ExcelDTO> sisExcelItem;
      #region Data
      for (int i = 2; i < rowCount + 1; i++)
      {
        sisExcelItem = new List<ExcelDTO>();
        for (int j = 1; j < colCount + 1; j++)
        {
          ExcelDTO Instance;
          if ((xlWorksheetSismik.Cells[i, j]).Value == null)
          {
            Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = "" };
            sisExcelItem.Add(Instance);
          }
          else
          {
            var value = (string)(xlWorksheetSismik.Cells[i, j]).Value.ToString();
            Instance = new ExcelDTO { TypeID = (byte)Enums.ExcelDataType.Real, Value = value };
            sisExcelItem.Add(Instance);
          }

        }
        sisExcel.Add(sisExcelItem);
      }
      #endregion

      #region Add Data
      foreach (var item in sisExcel)
      {
        if (item[item.Count - 1].Value == "" && item[item.Count - 2].Value == "" && item[item.Count - 3].Value == "")
        {
          for (int i = 0; i < item.Count; i++)
          {
            if (item[i].Value == "" && item[i + 1].Value == "" && item[i + 2].Value == "")
            {
              item[i - 3].JSONData = JsonConvert.SerializeObject(item[i - 3]);
              item[i - 2].JSONData = JsonConvert.SerializeObject(item[i - 2]);
              item[i - 1].JSONData = JsonConvert.SerializeObject(item[i - 1]);

              List<ExcelDTO> finalItem = new List<ExcelDTO>();
              finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 3].JSONData));
              finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 2].JSONData));
              finalItem.Add(JsonConvert.DeserializeObject<ExcelDTO>(item[i - 1].JSONData));

              for (int j = i; j < item.Count; j = j + 3)
              {
                item[j - 3].JSONData = JsonConvert.SerializeObject(item[i - 6]);
                item[j - 2].JSONData = JsonConvert.SerializeObject(item[i - 5]);
                item[j - 1].JSONData = JsonConvert.SerializeObject(item[i - 4]);
                item[j - 3] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 3].JSONData);
                item[j - 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 2].JSONData);
                item[j - 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j - 1].JSONData);
                item[j - 6].TypeID = (byte)Enums.ExcelDataType.Artificial;
                item[j - 5].TypeID = (byte)Enums.ExcelDataType.Artificial;
                item[j - 4].TypeID = (byte)Enums.ExcelDataType.Artificial;

                if (j == item.Count - 3)
                {
                  item[j].JSONData = JsonConvert.SerializeObject(finalItem[0]);
                  item[j + 1].JSONData = JsonConvert.SerializeObject(finalItem[1]);
                  item[j + 2].JSONData = JsonConvert.SerializeObject(finalItem[2]);
                  item[j] = JsonConvert.DeserializeObject<ExcelDTO>(item[j].JSONData);
                  item[j + 1] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 1].JSONData);
                  item[j + 2] = JsonConvert.DeserializeObject<ExcelDTO>(item[j + 2].JSONData);
                  item[j - 3].TypeID = (byte)Enums.ExcelDataType.Real;
                  item[j - 2].TypeID = (byte)Enums.ExcelDataType.Real;
                  item[j - 1].TypeID = (byte)Enums.ExcelDataType.Real;
                  continue;
                }
              }
              break;
            }

          }
        }
      }
      #endregion

      #endregion



      for (int i = 0; i < rowCount - 1; i++)
      {
        sisItem = new SeismicDTO();
        sisItem.ID = i + 1;
        sisItem.Name = sisExcel[i][0].Value.ToString();
        sisItem.X = Convert.ToDouble(sisExcel[i][1].Value);
        sisItem.K = Convert.ToDouble(sisExcel[i][3].Value);
        sisList.Add(sisItem);
      }
      this.sisList.Add(sisList);

      int count = 0;
      for (int j = 4; j < colCount; j = j + 3)
      {
        count++;
        sisList = new List<SeismicDTO>();
        for (int i = 0; i < rowCount - 1; i++)
        {
          var sisExcelInstance = sisExcel[i];
          if (sisExcelInstance[j].TypeID == (byte)Enums.ExcelDataType.Real)
          {
            if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value == "" && sisExcelInstance[j + 2].Value == "")
            {
              continue;
            }
            if (sisExcelInstance[j].Value == "" && sisExcelInstance[j + 1].Value != "" && sisExcelInstance[j + 2].Value != "")
            {
              sisItem = new SeismicDTO();
              sisItem.ID = i + 1;
              sisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
              sisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
              var value = "";
              for (int k = 0; k < sisExcelInstance.Count; k = k + 3)
              {
                if (sisExcelInstance[j - (3 + k)].TypeID == (byte)Enums.ExcelDataType.Real)
                {
                  value = sisExcelInstance[j - (3 + k)].Value;
                  break;
                }
              }
              sisItem.K = (Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(value)) * 0.99;
              sisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
              sisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
              sisList.Add(sisItem);
              continue;
            }
            sisItem = new SeismicDTO();
            sisItem.ID = i + 1;
            sisItem.Name = sisExcelInstance[0].Value.ToString() + count.ToString();
            sisItem.X = Convert.ToDouble(sisExcelInstance[1].Value);
            sisItem.K = sisExcelInstance[j].Value == "" ? 0 : Convert.ToDouble(sisExcelInstance[3].Value) - Convert.ToDouble(sisExcelInstance[j].Value);
            sisItem.Vp = sisExcelInstance[j + 1].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 1].Value);
            sisItem.Vs = sisExcelInstance[j + 2].Value == "" ? Convert.ToDouble("") : Convert.ToDouble(sisExcelInstance[j + 2].Value);
            sisList.Add(sisItem);
          }
        }
        this.sisList.Add(sisList);
      }

      highcharts = CreateChart(highcharts, this.sisList);
    }
    private void CreateDrilling(HighchartsDTO highcharts, Workbook xlWorkbook)
    {
      drillList = new List<List<DrillingDTO>>();
      List<DrillingDTO> drillingList = new List<DrillingDTO>();
      DrillingDTO drillingItem = new DrillingDTO();
      Microsoft.Office.Interop.Excel._Worksheet xlWorkSheetDrilling = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[3];
      Microsoft.Office.Interop.Excel.Range xlRange = xlWorkSheetDrilling.UsedRange;
      #region Table Row Col
      int rowCount = xlRange.Rows.Count;
      int colCount = xlRange.Columns.Count;
      for (int i = 1; i <= rowCount; i++)
      {
        if (string.IsNullOrEmpty((xlWorkSheetDrilling.Cells[i + 1, 1]).Value))
        {
          rowCount = i;
          break;
        }
      }
      #endregion

      for (int i = 1; i < rowCount; i++)
      {
        drillingItem = new DrillingDTO();
        drillingItem.ID = i;
        drillingItem.Name = (string)(xlWorkSheetDrilling.Cells[i + 1, 1]).Value.ToString();
        drillingItem.X = (double)(xlWorkSheetDrilling.Cells[i + 1, 2]).Value;
        drillingItem.K = (double)(xlWorkSheetDrilling.Cells[i + 1, 4]).Value;
        drillingList.Add(drillingItem);
      }
      drillList.Add(drillingList);

      int count = 0;
      for (int j = 5; j <= colCount; j = j + 2)
      {
        count++;
        drillingList = new List<DrillingDTO>();
        for (int i = 1; i <= rowCount; i++)
        {

          drillingItem = new DrillingDTO();
          if ((xlWorkSheetDrilling.Cells[i + 1, j]).Value == null && (xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value == null)
          {
            continue;
          }
          if ((xlWorkSheetDrilling.Cells[i + 1, j]).Value == null && (xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value != null)
          {
            drillingItem.ID = i;
            drillingItem.Name = (string)(xlWorkSheetDrilling.Cells[i + 1, 1]).Value.ToString() + count.ToString();
            drillingItem.X = (double)(xlWorkSheetDrilling.Cells[i + 1, 2]).Value;
            drillingItem.K = ((double)(xlWorkSheetDrilling.Cells[i + 1, 4]).Value - (double)(xlWorkSheetDrilling.Cells[i + 1, j - 2]).Value) * 0.99;
            drillingItem.T = (xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value == null ? "" : (xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value;
            drillingList.Add(drillingItem);
            continue;
          }
          drillingItem.ID = i;
          drillingItem.Name = (string)(xlWorkSheetDrilling.Cells[i + 1, 1]).Value.ToString() + count.ToString();
          drillingItem.X = (double)(xlWorkSheetDrilling.Cells[i + 1, 2]).Value;
          drillingItem.K = (xlWorkSheetDrilling.Cells[i + 1, j]).Value == null ? 0 : (double)(xlWorkSheetDrilling.Cells[i + 1, 4]).Value - (double)(xlWorkSheetDrilling.Cells[i + 1, j]).Value;
          drillingItem.T = (xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value == null ? "" : ((xlWorkSheetDrilling.Cells[i + 1, j + 1]).Value).ToString();
          drillingList.Add(drillingItem);
        }
        drillList.Add(drillingList);
      }

      highcharts = ChartOlustur(highcharts, drillList);
    }

    private HighchartsDTO CreateChart(HighchartsDTO highcharts, List<List<ResistivityDTO>> resList)
    {

      highcharts.annotations.AddRange(CreateGraphAnnotations(resList));

      return highcharts;
    }
    private HighchartsDTO CreateChart(HighchartsDTO highcharts, List<List<SeismicDTO>> sisList)
    {

      highcharts.annotations.AddRange(CreateGraphAnnotations(sisList));

      return highcharts;
    }
    private HighchartsDTO ChartOlustur(HighchartsDTO highcharts, List<List<DrillingDTO>> drillingList)
    {

      highcharts.annotations.AddRange(CreateGraphAnnotations(drillingList));

      return highcharts;
    }

    private List<AnnotationsDTO> CreateGraphAnnotations(List<List<ResistivityDTO>> resList)
    {
      List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
      AnnotationsDTO annotations;
      AnnotationLabelsDTO label;

      for (int i = 0; i < resList.Count; i++)
      {
        annotations = new AnnotationsDTO();
        annotations.visible = true;
        foreach (var rezItem in resList[i].Where(k => k.TypeID == (byte)Enums.ExcelDataType.Real))
        {
          if (i == 0)
            label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Name, shape = "connector", allowOverlap = true };
          //label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.Adi + "<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
          else
            label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm", shape = "connector", allowOverlap = true };
          //label = new AnnotationLabelsDTO { x = -20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = rezItem.X, y = rezItem.K }, text = rezItem.R + " ohm<br>X:" + rezItem.X + " Y:" + rezItem.K, shape = "connector", allowOverlap = true };
          annotations.labels.Add(label);
        }
        annotationsList.Add(annotations);
      }

      return annotationsList;
    }
    private List<AnnotationsDTO> CreateGraphAnnotations(List<List<SeismicDTO>> sisList)
    {
      List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
      AnnotationsDTO annotations;
      AnnotationLabelsDTO label;

      for (int i = 0; i < sisList.Count; i++)
      {
        annotations = new AnnotationsDTO();
        annotations.visible = true;
        //annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
        foreach (var sisItem in sisList[i])
        {
          if (i == 0)
            label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Name, shape = "connector", allowOverlap = true, style = new StyleDTO { fontSize = "16px", color = "contrast" } };
          //label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = sisItem.Adi + "<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
          else
            label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s", shape = "connector", allowOverlap = true, style = new StyleDTO { fontSize = "16px", color = "contrast" } };
          //label = new AnnotationLabelsDTO { x = 20, y = -20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sisItem.X, y = sisItem.K }, text = "Vp = " + sisItem.Vp + "m/s<br>Vs =" + sisItem.Vs + "m/s<br>X:" + sisItem.X + " Y:" + sisItem.K, shape = "connector", allowOverlap = true };
          annotations.labels.Add(label);
        }
        annotationsList.Add(annotations);
      }

      return annotationsList;
    }
    private List<AnnotationsDTO> CreateGraphAnnotations(List<List<DrillingDTO>> drillingList)
    {
      List<AnnotationsDTO> annotationsList = new List<AnnotationsDTO>();
      AnnotationsDTO annotations;
      AnnotationLabelsDTO label;

      for (int i = 0; i < drillingList.Count; i++)
      {
        annotations = new AnnotationsDTO();
        annotations.visible = true;
        // annotations.labelOptions = new AnnotationLabelOptionsDTO { shape = "connector", align = "right", justify = false, crop = true, style = new StyleDTO { fontSize = "0.8em", textOutline = "1px white" } };
        foreach (var sonItem in drillingList[i])
        {
          if (i == 0)
            //label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Adi + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
            label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.Name, shape = "connector", allowOverlap = true };
          else
            //label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T + "<br>X:" + sonItem.X + " Y:" + sonItem.K, shape = "connector", allowOverlap = true };
            label = new AnnotationLabelsDTO { x = 20, y = 20, point = new PointDTO { xAxis = 0, yAxis = 0, x = sonItem.X, y = sonItem.K }, text = sonItem.T, shape = "connector", allowOverlap = true };
          annotations.labels.Add(label);
        }
        annotationsList.Add(annotations);
      }

      return annotationsList;
    }

    public List<SeriesDTO> CreateGraphData(long ruleID, CrossSectionDTO crossSectionDTO, ParametersDTO parameters)
    {
      FetchRuleDTO fecthRule = _fuzzyManager.FetchRule(ruleID, session);
      VisualizationDetailDTO visualizationDetail = new VisualizationDetailDTO();

      SeriesDTO dataset;
      var name = "Set-";
      int count = 0;
      var random = new Random();
      for (int i = 0; i < crossSectionDTO.ResList.Count - 1; i++)
      {
        count++;
        dataset = new SeriesDTO();
        dataset.name = name + count.ToString();
        if ((bool)parameters.IsChecked)
          dataset.lineWidth = 0;
        dataset.lineWidth = 2;
        dataset.color = GenerateColor(i, crossSectionDTO.ResList.Count); // String.Format("#{0:X6}", random.Next(0x1000000));
        dataset.showInLegend = false;
        dataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
        dataset.tooltip = new ToolTipDTO
        {
          useHTML = true,
          headerFormat = "<small>{series.name}</small><table style='color: {series.color}'><br />",
          pointFormat = "<tr><td style='text-align: right'><b>{point.x}, {point.y}</b></td></tr>",
          footerFormat = "</table>",
          valueDecimals = 2
        };
        dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
        //dataset.enableMouseTracking = false;
        dataset.draggableY = true;
        dataset.draggableX = true;
        for (int j = 0; j < crossSectionDTO.ResList[i].Count; j++)
        {
          List<double> coordinates = new List<double>();

          if (i == 0 && j == 0 && !crossSectionDTO.ResList[i][j].Checked)
          {
            AddSeismicVisualization(crossSectionDTO.ResList[i][j].X, crossSectionDTO.SisList[i], crossSectionDTO.ResList[i], dataset, (byte)Enums.Direction.Left, j);
          }

          #region Topograph
          if (i == 0)
          {
            if (!crossSectionDTO.ResList[i][j].Checked)
            {
              coordinates.Add(crossSectionDTO.ResList[i][j].X);
              coordinates.Add((double)crossSectionDTO.ResList[i][j].K);
              dataset.data.Add(coordinates);
              crossSectionDTO.ResList[i][j].Checked = true;

              if (j + 1 < crossSectionDTO.ResList[i].Count)
              {
                visualizationDetail = new VisualizationDetailDTO { FirstNode = crossSectionDTO.ResList[i][j].Name, SecondNode = crossSectionDTO.ResList[i][j + 1].Name, Normal = true, Connection = "Normal" };
                visualizationDetailList.Add(visualizationDetail);
              }
              visualizationCount.Normal++;

              if (j == crossSectionDTO.ResList[i].Count - 1)
              {
                AddSeismicVisualization(crossSectionDTO.ResList[i][j].X, crossSectionDTO.SisList[i], crossSectionDTO.ResList[i], dataset, (byte)Enums.Direction.Right, j);
              }
            }
            continue;
          }
          #endregion

          NodeDTO firstNode = new NodeDTO();
          NodeDTO seconNode = new NodeDTO();
          if (j != crossSectionDTO.ResList[i].Count - 1) //Son sıra kontrolü
          {

            firstNode = FirstNodeControl(crossSectionDTO.ResList, i, j);
            seconNode = SecondNodeControl(fecthRule, crossSectionDTO, crossSectionDTO.ResList, i, j + 1, parameters);
            if ((firstNode.Node.TypeID == (byte)Enums.ExcelDataType.Real && seconNode.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
                (firstNode.Node.TypeID == (byte)Enums.ExcelDataType.Artificial && seconNode.Node.TypeID == (byte)Enums.ExcelDataType.Real) ||
                (firstNode.Node.TypeID == (byte)Enums.ExcelDataType.Real && seconNode.Node.TypeID == (byte)Enums.ExcelDataType.Artificial))
            {
              if ((!firstNode.Node.Checked && !seconNode.Node.Checked) ||
                  (firstNode.Node.Checked && !seconNode.Node.Checked) ||
                  (!firstNode.Node.Checked && seconNode.Node.Checked))
              {
                if (firstNode.Node.R != null && seconNode.Node.R != null && firstNode.Node.R != 0 && seconNode.Node.R != 0)
                {

                  var checkNodes = _fuzzyManager.CreateFuzzyRuleAndCompare(fecthRule, (double)firstNode.Node.R, (double)seconNode.Node.R, (int)parameters.ResistivityRatio);

                  if (checkNodes)
                  {
                    bool VpOkay = SeismicVpCheck(crossSectionDTO, firstNode.IndexI, firstNode.IndexJ, (int)parameters.SeismicRatio);
                    bool VsOkay = SeismicVsCheck(crossSectionDTO, firstNode.IndexI, firstNode.IndexJ, (int)parameters.SeismicRatio);
                    if (VpOkay && VsOkay) //Vp Vs ve Özdirenç değerleri uygunsa birleştirme yapılır
                    {
                      #region Seismic Check
                      if (j == 0)
                      {
                        if (!crossSectionDTO.ResList[i][j].Checked && i < crossSectionDTO.SisList.Count)
                        {
                          AddSeismicVisualization(crossSectionDTO.ResList[i][j].X, crossSectionDTO.SisList[i], crossSectionDTO.ResList[i], dataset, (byte)Enums.Direction.Left, j);
                        }
                      }
                      #endregion

                      var FirstNodeLeftFault = FirstNodeLeftFaultCheck(crossSectionDTO, firstNode, seconNode, datasetList);
                      if (FirstNodeLeftFault != null)
                      {
                        List<double> coordinatesNull = new List<double>();
                        dataset.data.Add(coordinatesNull);

                        List<double> coordinatesLeftFault = new List<double>();
                        coordinatesLeftFault.Add(FirstNodeLeftFault.data[0][0]);
                        coordinatesLeftFault.Add((double)firstNode.Node.K);
                        dataset.data.Add(coordinatesLeftFault);

                        visualizationDetail = new VisualizationDetailDTO { FirstNode = "Fault", SecondNode = seconNode.Node.Name, Normal = true, Connection = "Normal" };
                        visualizationDetailList.Add(visualizationDetail);
                        visualizationCount.Normal++;
                      }
                      if (!crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked)
                      {
                        coordinates = new List<double>();
                        coordinates.Add(firstNode.Node.X);
                        coordinates.Add((double)firstNode.Node.K);
                        dataset.data.Add(coordinates);
                        crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                      }
                      var FirstNodeRightFault = FirstNodeRightFaultCheck(crossSectionDTO, firstNode, seconNode, datasetList);
                      if (FirstNodeRightFault != null)
                      {
                        List<double> coordinatesRightFault = new List<double>();
                        coordinatesRightFault.Add(FirstNodeRightFault.data[0][0]);
                        coordinatesRightFault.Add((double)firstNode.Node.K);
                        dataset.data.Add(coordinatesRightFault);

                        visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = "Fault", Normal = true, Connection = "Normal" };
                        visualizationDetailList.Add(visualizationDetail);
                        visualizationCount.Normal++;
                        continue;
                      }
                      coordinates = new List<double>();
                      coordinates.Add(seconNode.Node.X);
                      coordinates.Add((double)seconNode.Node.K);
                      dataset.data.Add(coordinates);
                      crossSectionDTO.ResList[seconNode.IndexI][seconNode.IndexJ].Checked = true;

                      visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Normal = true, Connection = "Normal" };
                      visualizationDetailList.Add(visualizationDetail);
                      visualizationCount.Normal++;
                    }
                    else
                    {
                      if (j == 0)
                      {

                        bool ClosureOkay = ClosureCheck(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                        coordinates.Add(firstNode.Node.X);
                        coordinates.Add((double)firstNode.Node.K);
                        dataset.data.Add(coordinates);
                        crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                        CreateClosure(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, firstNode.IndexJ);

                        visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Closure = true, Connection = "Closure" };
                        visualizationDetailList.Add(visualizationDetail);
                        visualizationCount.Closure++;

                        break;
                      }
                      else
                      {
                        var previousNode = _fuzzyManager.CreateFuzzyRuleAndReturnResult(fecthRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].R);
                        var pitCheck = _fuzzyManager.CreateFuzzyRuleAndCompare(fecthRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].R, (double)seconNode.Node.R, (int)parameters.ResistivityRatio);

                        bool ClosureOkay = ClosureCheck(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                        coordinates.Add(firstNode.Node.X);
                        coordinates.Add((double)firstNode.Node.K);
                        dataset.data.Add(coordinates);
                        crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                        CreateClosure(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                        visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Closure = true, Connection = "Closure" };
                        visualizationDetailList.Add(visualizationDetail);
                        visualizationCount.Closure++;
                        //}
                        break;
                        //}
                      }
                    }
                  }
                  else
                  {
                    if (j == 0)
                    {
                      bool ClosureOkay = ClosureCheck(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                      coordinates.Add(firstNode.Node.X);
                      coordinates.Add((double)firstNode.Node.K);
                      dataset.data.Add(coordinates);
                      crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                      CreateClosure(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                      visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Closure = true, Connection = "Closure" };
                      visualizationDetailList.Add(visualizationDetail);
                      visualizationCount.Closure++;
                      //}
                      break;
                    }
                    else
                    {
                      var faultCheck = CheckFault(fecthRule, crossSectionDTO, firstNode, seconNode, parameters);

                      if (faultCheck && i > 1)
                      {
                        if (!crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked)
                        {

                          coordinates.Add(firstNode.Node.X);
                          coordinates.Add((double)firstNode.Node.K);
                          dataset.data.Add(coordinates);
                          crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                        }


                        SeriesDTO faultDataset = CreateFault(fecthRule, crossSectionDTO, firstNode, seconNode, parameters);
                        datasetList.Add(faultDataset);

                        if (!(crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].TypeID == (byte)Enums.ExcelDataType.Artificial && crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].TypeID == (byte)Enums.ExcelDataType.Artificial))
                        {
                          List<double> faultCoordinates = new List<double>();
                          faultCoordinates.Add(faultDataset.data[0][0]);
                          faultCoordinates.Add((double)firstNode.Node.K);
                          dataset.data.Add(faultCoordinates);
                        }

                        visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Fault = true, Connection = "Fault" };
                        visualizationDetailList.Add(visualizationDetail);
                        visualizationCount.Fault++;

                        continue;
                      }
                      else
                      {

                        var previousNode = _fuzzyManager.CreateFuzzyRuleAndReturnResult(fecthRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].R);
                        var pitCheck = _fuzzyManager.CreateFuzzyRuleAndCompare(fecthRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].R, (double)seconNode.Node.R, (int)parameters.ResistivityRatio);


                        bool ClosureOkay = ClosureCheck(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                        if (ClosureOkay)
                        {
                          if (!crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked)
                          {
                            coordinates.Add(firstNode.Node.X);
                            coordinates.Add((double)firstNode.Node.K);
                            dataset.data.Add(coordinates);
                            crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
                          }

                          CreateClosure(datasetList, dataset, crossSectionDTO.ResList, firstNode.IndexI, seconNode.IndexJ);

                          visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = seconNode.Node.Name, Closure = true, Connection = "Closure" };
                          visualizationDetailList.Add(visualizationDetail);
                          visualizationCount.Closure++;
                          break;
                        }
                        var FirstNodeRightFault = FirstNodeRightFaultCheck(crossSectionDTO, firstNode, seconNode, datasetList);
                        if (FirstNodeRightFault != null)
                        {
                          List<double> coordinatesRightFault = new List<double>();
                          coordinatesRightFault.Add(FirstNodeRightFault.data[0][0]);
                          coordinatesRightFault.Add((double)firstNode.Node.K);
                          dataset.data.Add(coordinatesRightFault);

                          visualizationDetail = new VisualizationDetailDTO { FirstNode = firstNode.Node.Name, SecondNode = "Fault", Normal = true, Connection = "Normal" };
                          visualizationDetailList.Add(visualizationDetail);
                          visualizationCount.Normal++;
                          continue;
                        }
                        //}
                      }
                    }
                  }
                  //}
                }
              }
            }
            else
            {
              crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].Checked = true;
              crossSectionDTO.ResList[seconNode.IndexI][seconNode.IndexJ].Checked = true;
              continue;
            }
          }
          else
          {
            #region Resistivity Right Seismic Check
            if (crossSectionDTO.SisList.Count >= crossSectionDTO.ResList.Count)
            {
              AddSeismicVisualization(crossSectionDTO.ResList[i][j].X, crossSectionDTO.SisList[i], crossSectionDTO.ResList[i], dataset, (byte)Enums.Direction.Right, j);
            }


            #endregion
          }
        }
        if (dataset.data.Count > 0)
          datasetList.Add(dataset);
      }

      return datasetList;
    }

    private bool ClosureCheck(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> resList, int indexI, int indexJ)
    {
      var topNode = resList[indexI - 1][indexJ];
      var topLeftNode = resList[indexI - 1][indexJ - 1];
      foreach (var item in datasetList)
      {
        for (int i = 0; i < item.data.Count; i++)
        {
          if (i + 1 < item.data.Count)
          {
            if (item.data[i].Count > 0 && item.data[i][0] == topLeftNode.X && item.data[i][1] == topLeftNode.K && item.data[i + 1][0] == topNode.X && item.data[i + 1][1] == topNode.K)
            {
              return true;
            }
          }

        }
      }

      return false;
    }

    private SeriesDTO CreateClosure(List<SeriesDTO> datasetList, SeriesDTO dataset, List<List<ResistivityDTO>> resList, int i, int j)
    {
      if (resList.Count > i)
      {
        if (resList[i - 1].Count > j)
        {
          List<double> coordinates;

          double commonPointX = 0, commonPointK = 0;

          commonPointX = (resList[i - 1][j - 1].X + resList[i - 1][j].X) / 2;
          commonPointK = (double)(resList[i - 1][j - 1].K + resList[i - 1][j].K) / 2;

          var previousPointX = resList[i - 1][j].X;
          var previousCountK = resList[i - 1][j].K;

          coordinates = new List<double>();
          coordinates.Add(commonPointX);
          coordinates.Add((double)commonPointK);
          dataset.data.Add(coordinates);
        }
      }
      return dataset;
    }

    private SeriesDTO FirstNodeLeftFaultCheck(CrossSectionDTO crossSectionDTO, NodeDTO firstNode, NodeDTO secondNode, List<SeriesDTO> datasetList)
    {

      if (firstNode.IndexJ > 0 && firstNode.IndexI > 0)
      {
        var fay = datasetList.Where(f => f.name == "Fault" && f.data[0][0] < firstNode.Node.X && crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ - 1].X < f.data[0][0]).FirstOrDefault();
        if (fay != null)
        {
          return fay;
        }
      }

      return null;
    }


    private SeriesDTO FirstNodeRightFaultCheck(CrossSectionDTO crossSectionDTO, NodeDTO firstNode, NodeDTO seconNode, List<SeriesDTO> datasetList)
    {
      if (firstNode.IndexJ > 0 && firstNode.IndexI > 0)
      {
        var fault = datasetList.Where(f => f.name == "Fay" && f.data[0][0] > firstNode.Node.X && seconNode.Node.X > f.data[0][0]).FirstOrDefault();
        if (fault != null)
        {
          return fault;
        }
      }

      return null;
    }
    private bool CheckFault(FetchRuleDTO fetchRule, CrossSectionDTO crossSectionDTO, NodeDTO firstNode, NodeDTO secondNode, ParametersDTO parameters)
    {
      bool firstResistivityBelowCompatible = false, secondResistivityBelowCompatible = false;
      bool resistivityComparisonOkay = true, VpOkay = false, VsOkay = false, resistivityBelowOkay = true, VpBelowOkay = false, VsBelowOkay = false;
      int i = firstNode.IndexI;
      int j = firstNode.IndexJ;


      resistivityComparisonOkay = _fuzzyManager.CreateFuzzyRuleAndCompare(fetchRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].R, (double)crossSectionDTO.ResList[secondNode.IndexI][secondNode.IndexJ].R, (int)parameters.ResistivityRatio);
      VpOkay = SeismicVpCheck(crossSectionDTO, i, j, (int)parameters.SeismicRatio);
      VsOkay = SeismicVsCheck(crossSectionDTO, i, j, (int)parameters.SeismicRatio);

      if (firstNode.IndexI + 1 + 1 < (double)crossSectionDTO.ResList.Count && secondNode.IndexI + 1 + 1 < (double)crossSectionDTO.ResList.Count)
      {

        resistivityBelowOkay = _fuzzyManager.CreateFuzzyRuleAndCompare(fetchRule, (double)crossSectionDTO.ResList[firstNode.IndexI + 1][firstNode.IndexJ].R, (double)crossSectionDTO.ResList[secondNode.IndexI + 1][secondNode.IndexJ].R, (int)parameters.ResistivityRatio);
        VpBelowOkay = SeismicVpCheck(crossSectionDTO, i + 1, j, (int)parameters.SeismicRatio);
        VsBelowOkay = SeismicVsCheck(crossSectionDTO, i + 1, j, (int)parameters.SeismicRatio);
      }

      if (secondNode.IndexI + 2 < (double)crossSectionDTO.ResList.Count)
      {
        for (int k = i; k < (double)crossSectionDTO.ResList[i].Count; k++)
        {
          if ((double)crossSectionDTO.ResList[i][k].TypeID == (byte)Enums.ExcelDataType.Real)
          {
            firstResistivityBelowCompatible = _fuzzyManager.CreateFuzzyRuleAndCompare(fetchRule, (double)crossSectionDTO.ResList[firstNode.IndexI][firstNode.IndexJ].R, (double)crossSectionDTO.ResList[secondNode.IndexI + 2][secondNode.IndexJ].R, (int)parameters.ResistivityRatio);
            break;
          }
        }
      }
      if (firstNode.IndexI + 1 < (double)crossSectionDTO.ResList.Count && secondNode.IndexI + 3 < (double)crossSectionDTO.ResList.Count)
      {
        for (int k = i + 1; k < (double)crossSectionDTO.ResList[i + 1].Count; k++)
        {
          if ((double)crossSectionDTO.ResList[i + 1][k].TypeID == (byte)Enums.ExcelDataType.Real)
          {
            secondResistivityBelowCompatible = _fuzzyManager.CreateFuzzyRuleAndCompare(fetchRule, (double)crossSectionDTO.ResList[firstNode.IndexI + 1][firstNode.IndexJ].R, (double)crossSectionDTO.ResList[secondNode.IndexI + 3][secondNode.IndexJ].R, (int)parameters.ResistivityRatio);
            break;
          }
        }
      }

      if (!resistivityComparisonOkay &&
          VpOkay &&
          VsOkay &&
          !resistivityBelowOkay &&
          VpBelowOkay &&
          VsBelowOkay &&
          firstResistivityBelowCompatible &&
          secondResistivityBelowCompatible)
        return true;

      return false;
    }

    private SeriesDTO CreateFault(FetchRuleDTO fetchRule, CrossSectionDTO crossSectionDTO, NodeDTO firstNode, NodeDTO secondNode, ParametersDTO parameters)
    {


      var FaultStartX = (crossSectionDTO.ResList[firstNode.IndexI - 2][firstNode.IndexJ].X + crossSectionDTO.ResList[secondNode.IndexI - 1][secondNode.IndexJ].X) / 2;
      var FaultStartY = (crossSectionDTO.ResList[firstNode.IndexI - 2][firstNode.IndexJ].K + crossSectionDTO.ResList[secondNode.IndexI - 1][secondNode.IndexJ].K) / 2;


      var FaultEndX = (crossSectionDTO.ResList[firstNode.IndexI + 1][firstNode.IndexJ].X + crossSectionDTO.ResList[secondNode.IndexI + 3][secondNode.IndexJ].X) / 2;
      var FaultEndY = crossSectionDTO.ResList[secondNode.IndexI + 3][secondNode.IndexJ].K;

      SeriesDTO faultDataset = new SeriesDTO();
      faultDataset.name = "Fault";
      if ((bool)parameters.IsChecked)
        faultDataset.lineWidth = 2;
      faultDataset.color = "#000000";
      faultDataset.showInLegend = false;
      faultDataset.marker = new MarkerDTO { enabled = false };
      faultDataset.tooltip = new ToolTipDTO { useHTML = true };
      faultDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
      //fayDataset.enableMouseTracking = false;
      faultDataset.draggableY = true;
      faultDataset.draggableX = true;

      List<double> coordinates = new List<double>();
      coordinates.Add(FaultStartX);
      coordinates.Add((double)FaultStartY);
      faultDataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(FaultEndX);
      coordinates.Add((double)FaultEndY);
      faultDataset.data.Add(coordinates);

      return faultDataset;
    }

    private string GenerateColor(int i, int count)
    {

      var color = Color.Black;
      if (i == 0)
      {
        color = Color.FromArgb(0, 0, 255);
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
      }
      else if (count - 1 == i) //Son Çizgi Kırmızı
      {
        color = Color.FromArgb(255, 0, 0);
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
      }
      else
      {
        List<RGBDTO> RGBList = ColorsFromBlueToRed();
        double percent = (double)i / (double)count * 100;
        int index = (int)Math.Round(percent * RGBList.Count / 100);
        RGBDTO RGBItem = RGBList[index];

        return "#" + RGBItem.R.ToString("X2") + RGBItem.G.ToString("X2") + RGBItem.B.ToString("X2");
      }
    }

    private List<RGBDTO> ColorsFromBlueToRed()
    {
      List<RGBDTO> RGBList = new List<RGBDTO>();

      RGBList.Add(new RGBDTO
      {
        R = 0,
        G = 0,
        B = 255
      });


      for (int i = 1; i < 256; i++) //Maviden Açık Maviye
      {
        RGBList.Add(new RGBDTO
        {
          R = 0,
          G = i,
          B = 255
        });
      }

      for (int i = 254; i >= 0; i--) //Açık Maviden Yeşile
      {
        RGBList.Add(new RGBDTO
        {
          R = 0,
          G = 255,
          B = i
        });
      }

      for (int i = 1; i < 256; i++) //Yeşilden Sarıya
      {
        RGBList.Add(new RGBDTO
        {
          R = i,
          G = 255,
          B = 0
        });
      }

      for (int i = 254; i >= 0; i--) //Sarıdan Kırmızıya
      {
        RGBList.Add(new RGBDTO
        {
          R = 255,
          G = i,
          B = 0
        });
      }

      return RGBList;
    }

    public ResultDTO FetchSetList()
    {
      ResultDTO result = new ResultDTO();
      try
      {
        var ruleList = _ruleService.Queryable().Where(k => k.isActive == true).Select(k => new RuleEntityDTO
        {
          RuleID = k.ruleID,
          RuleName = k.ruleName,
          AddDate = k.addDate,
          IsActive = k.isActive
        }).ToList();
        result.ResultObject = ruleList;
        result.Success = true;
        result.Message = "Success.";
        return result;
      }
      catch (Exception ex)
      {

        result.ResultObject = null;
        result.Success = false;
        result.Message = "Başarısız.";
        result.Exception = ex;
        return result;
      }
    }


    public ResultDTO FetchSetListLite(Rule rule)
    {
      ResultDTO result = new ResultDTO();
      var rules = new List<Rule>() { rule };
      try
      {

        var ruleList = rules.Select(k => new RuleEntityDTO
        {
          RuleID = k.ruleID,
          RuleName = k.ruleName,
          AddDate = k.addDate,
          IsActive = k.isActive
        }).ToList();
        result.ResultObject = ruleList;
        result.Success = true;
        result.Message = "Success.";
        return result;
      }
      catch (Exception ex)
      {

        result.ResultObject = null;
        result.Success = false;
        result.Message = "Başarısız.";
        result.Exception = ex;
        return result;
      }
    }

    public ResultDTO FetchRule(long ruleID)
    {
      ResultDTO result = new ResultDTO();
      try
      {
        var ruleList = _ruleListTextService.Queryable().Where(k => k.ruleID == ruleID).Select(k => new RuleTextEntityDTO
        {
          RuleID = k.ruleID,
          RuleText = k.ruleText
        }).ToList();
        result.ResultObject = ruleList;
        result.Success = true;
        result.Message = "Success.";
        return result;
      }
      catch (Exception ex)
      {

        result.ResultObject = null;
        result.Success = false;
        result.Message = "Başarısız.";
        result.Exception = ex;
        return result;
      }
    }

    public ResultDTO FetchRuleTextAndResistivity(long ruleID)
    {
      ResultDTO result = new ResultDTO();
      try
      {
        var ruleList = _ruleListTextService.Queryable().Where(k => k.ruleID == ruleID).Select(k => new RuleTextEntityDTO
        {
          RuleID = k.ruleID,
          RuleText = k.ruleText
        }).ToList();
        var resistivityList = _variableItemService.Queryable().Where(d => d.variable.ruleID == ruleID && d.variable.variableTypeID == (byte)Enums.VariableType.Input).Select(d => new VariableDTO
        {
          Name = d.variableItemName,
          MinValue = d.minValue,
          MaxValue = d.maxValue,
        }).ToList();
        result.ResultObject = new RuleTextAndResistivityDTO { ruleTextList = ruleList, resistivityList = resistivityList };
        result.Success = true;
        result.Message = "Success.";
        return result;
      }
      catch (Exception ex)
      {

        result.ResultObject = null;
        result.Success = false;
        result.Message = "Error.";
        result.Exception = ex;
        return result;
      }
    }

    private bool SeismicVpCheck(CrossSectionDTO crossSectionDTO, int i, int j, int ratio)
    {
      if (i < crossSectionDTO.SisList.Count && j < crossSectionDTO.SisList[i].Count)
      {
        if (crossSectionDTO.SisList[i][j].Vp != null && crossSectionDTO.SisList[i][j].Vp != 0 && crossSectionDTO.SisList[i][j].Vs != null && crossSectionDTO.SisList[i][j].Vs != 0)
        {
          if ((i + 1) < crossSectionDTO.SisList.Count)
          {
            if ((crossSectionDTO.SisList[i][j].X > crossSectionDTO.ResList[i][j].X && crossSectionDTO.SisList[i][j].X < crossSectionDTO.ResList[i + 1][j].X) && (crossSectionDTO.SisList[i + 1][j].X > crossSectionDTO.ResList[i][j].X && crossSectionDTO.SisList[i + 1][j].X < crossSectionDTO.ResList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
            {
              if (crossSectionDTO.SisList[i][j].Vp > crossSectionDTO.SisList[i + 1][j].Vp)
              {
                if (crossSectionDTO.SisList[i][j].Vp * (ratio / 100) > crossSectionDTO.SisList[i + 1][j].Vp)
                {
                  return false;
                }
              }
              else
              {
                if (crossSectionDTO.SisList[i + 1][j].Vp * (ratio / 100) > crossSectionDTO.SisList[i][j].Vp)
                {
                  return false;
                }
              }
            }
            else
            {
              if (j + 1 < crossSectionDTO.SisList[i].Count)
              {
                if (crossSectionDTO.SisList[i][j].Vp * (ratio / 100) > crossSectionDTO.SisList[i][j + 1].Vp)
                {
                  return false;
                }
              }
            }
          }
        }
      }

      return true;
    }

    private bool SeismicVsCheck(CrossSectionDTO crossSectionDTO, int i, int j, int ratio)
    {
      if (i < crossSectionDTO.SisList.Count && j < crossSectionDTO.SisList[i].Count)
      {
        if (crossSectionDTO.SisList[i][j].Vs != null && crossSectionDTO.SisList[i][j].Vs != 0 && crossSectionDTO.SisList[i][j].Vs != null && crossSectionDTO.SisList[i][j].Vs != 0)
        {
          if ((i + 1) < crossSectionDTO.SisList.Count)
          {
            if ((crossSectionDTO.SisList[i][j].X > crossSectionDTO.ResList[i][j].X && crossSectionDTO.SisList[i][j].X < crossSectionDTO.ResList[i + 1][j].X) && (crossSectionDTO.SisList[i + 1][j].X > crossSectionDTO.ResList[i][j].X && crossSectionDTO.SisList[i + 1][j].X < crossSectionDTO.ResList[i + 1][j].X)) //iki özdirenç arasında birden fazla sismik ölçüm olma durumu
            {
              if (crossSectionDTO.SisList[i][j].Vs > crossSectionDTO.SisList[i + 1][j].Vs)
              {
                if (crossSectionDTO.SisList[i][j].Vs * (ratio / 100) > crossSectionDTO.SisList[i + 1][j].Vs)
                {
                  return false;
                }
              }
              else
              {
                if (crossSectionDTO.SisList[i + 1][j].Vs * (ratio / 100) > crossSectionDTO.SisList[i][j].Vs)
                {
                  return false;
                }
              }
            }
            else
            {
              if (j + 1 < crossSectionDTO.SisList[i].Count)
              {
                if (crossSectionDTO.SisList[i][j].Vs * (ratio / 100) > crossSectionDTO.SisList[i][j + 1].Vs)
                {
                  return false;
                }
              }
            }
          }
        }
      }

      return true;
    }

    private SeriesDTO CreatePit(SeriesDTO dataset, List<List<ResistivityDTO>> resList, int i, int j)
    {
      SeriesDTO pitDataset = new SeriesDTO();
      pitDataset.name = "Pit";
      pitDataset.lineWidth = 2;
      pitDataset.color = dataset.color;
      pitDataset.showInLegend = false;
      pitDataset.marker = new MarkerDTO { enabled = false };
      pitDataset.tooltip = new ToolTipDTO { useHTML = true };
      pitDataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
      pitDataset.enableMouseTracking = false;

      var pitStartX = resList[i][j].X - (Math.Abs(resList[i][j].X - resList[i][j - 1].X) / 5);
      var pitStartK = resList[i][j].K - (Math.Abs((double)resList[i][j].K - (double)resList[i][j - 1].K) / 5);

      var pitEndX = resList[i][j].X + (Math.Abs(resList[i][j].X - resList[i][j + 1].X) / 5);
      var pitEndK = resList[i][j].K + (Math.Abs((double)resList[i][j].K - (double)resList[i][j + 1].K) / 5);

      List<double> coordinates = new List<double>();
      coordinates.Add(resList[i][j - 1].X);
      coordinates.Add((double)resList[i][j - 1].K);
      dataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(pitStartX);
      coordinates.Add((double)pitStartK);
      dataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(resList[i][j].X);
      coordinates.Add((double)resList[i][j].K);
      dataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(pitEndX);
      coordinates.Add((double)pitEndK);
      dataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(resList[i][j + 1].X);
      coordinates.Add((double)resList[i][j + 1].K);
      dataset.data.Add(coordinates);



      coordinates = new List<double>();
      coordinates.Add(pitStartX);
      coordinates.Add((double)pitStartK);
      pitDataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(resList[i][j].X + 1);
      coordinates.Add((double)resList[i][j].K);
      pitDataset.data.Add(coordinates);

      coordinates = new List<double>();
      coordinates.Add(pitEndX);
      coordinates.Add((double)pitEndK);
      pitDataset.data.Add(coordinates);

      return pitDataset;
    }


    private void AddSeismicVisualization(double ResistivityX, List<SeismicDTO> seismicList, List<ResistivityDTO> resistivityList, SeriesDTO dataset, byte Yon, int j)
    {

      var SeismicList = seismicList.Where(s => Yon == (byte)Enums.Direction.Left ? s.X < ResistivityX : s.X > ResistivityX).ToList();
      for (int i = 0; i < SeismicList.Count; i++)
      {
        VisualizationDetailDTO visualizationDetail = new VisualizationDetailDTO();

        List<double> coordinates = new List<double>();
        coordinates.Add(SeismicList[i].X);
        coordinates.Add((double)SeismicList[i].K);
        dataset.data.Add(coordinates);


        if (Yon == (byte)Enums.Direction.Left)
        {
          if (SeismicList.Count > 1)
          {
            if (i < SeismicList.Count - 1)
            {
              visualizationDetail = new VisualizationDetailDTO { FirstNode = SeismicList[i].Name, SecondNode = SeismicList[i + 1].Name, Normal = true, Connection = "Normal" };
              visualizationDetailList.Add(visualizationDetail);
            }
            else
            {
              visualizationDetail = new VisualizationDetailDTO { FirstNode = SeismicList[i].Name, SecondNode = resistivityList[j].Name, Normal = true, Connection = "Normal" };
              visualizationDetailList.Add(visualizationDetail);
            }
          }
          else
          {
            visualizationDetail = new VisualizationDetailDTO { FirstNode = SeismicList[i].Name, SecondNode = resistivityList[j].Name, Normal = true, Connection = "Normal" };
            visualizationDetailList.Add(visualizationDetail);
          }
        }
        else
        {
          if (SeismicList.Count > 1)
          {
            if (i < SeismicList.Count - 1)
            {
              visualizationDetail = new VisualizationDetailDTO { FirstNode = SeismicList[i].Name, SecondNode = SeismicList[i + 1].Name, Normal = true, Connection = "Normal" };
              visualizationDetailList.Add(visualizationDetail);
            }
            else
            {
              visualizationDetail = new VisualizationDetailDTO { FirstNode = resistivityList[j].Name, SecondNode = SeismicList[i].Name, Normal = true, Connection = "Normal" };
              visualizationDetailList.Add(visualizationDetail);
            }
          }
          else
          {
            visualizationDetail = new VisualizationDetailDTO { FirstNode = resistivityList[j].Name, SecondNode = SeismicList[i].Name, Normal = true, Connection = "Normal" };
            visualizationDetailList.Add(visualizationDetail);
          }
        }



        visualizationCount.Normal++;
      }
    }


    private NodeDTO FirstNodeControl(List<List<ResistivityDTO>> resList, int indexI, int indexJ)
    {
      NodeDTO node = new NodeDTO { Node = resList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };

      for (int i = indexI - 1; i >= 0; i--)
      {
        if (!resList[i][indexJ].Checked && resList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
        {
          if (!resList[i - 1][indexJ].Checked)
          {
            return node;
          }
          node.IndexI = i;
          node.IndexJ = indexJ;
          node.Node = resList[i][indexJ];
          break;
        }
      }


      return node;
    }

    private NodeDTO SecondNodeControl(FetchRuleDTO fetchRule, CrossSectionDTO crossSectionDTO, List<List<ResistivityDTO>> resList, int indexI, int indexJ, ParametersDTO parameters)
    {
      NodeDTO node = new NodeDTO { Node = resList[indexI][indexJ], IndexI = indexI, IndexJ = indexJ };



      for (int i = indexI - 1; i >= 0; i--)
      {
        if (!resList[i][indexJ].Checked && resList[i][indexJ].TypeID == (byte)Enums.ExcelDataType.Real)
        {

          node.IndexI = i;
          node.IndexJ = indexJ;
          node.Node = resList[i][indexJ];
        }
      }


      return node;
    }

    #region Control
    private List<SeriesDTO> CreateGraphData(List<List<ResistivityDTO>> resList)
    {
      List<SeriesDTO> datasetList = new List<SeriesDTO>();
      SeriesDTO dataset;
      var name = "Set-";
      int count = 0;
      var random = new Random();
      foreach (var rezList in resList)
      {
        count++;
        dataset = new SeriesDTO();
        dataset.name = name + count.ToString();
        dataset.lineWidth = 2;
        dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
        dataset.showInLegend = false;
        dataset.marker = new MarkerDTO { symbol = "circle", radius = 2, enabled = true };
        dataset.tooltip = new ToolTipDTO { useHTML = true };
        dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
        dataset.enableMouseTracking = false;
        for (int i = 0; i < rezList.Count; i++)
        {
          List<double> coordinates = new List<double>();

          coordinates.Add(rezList[i].X);
          coordinates.Add((double)rezList[i].K);
          dataset.data.Add(coordinates);
        }
        datasetList.Add(dataset);
      }
      return datasetList;
    }
    private List<SeriesDTO> CreateGraphData(List<List<SeismicDTO>> sisList)
    {
      List<SeriesDTO> datasetList = new List<SeriesDTO>();
      SeriesDTO dataset;
      var name = "Set-";
      int count = 0;
      var random = new Random();
      foreach (var sis in sisList)
      {
        count++;
        dataset = new SeriesDTO();
        dataset.name = name + count.ToString();
        dataset.lineWidth = 2;
        dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
        dataset.showInLegend = false;
        dataset.marker = new MarkerDTO { symbol = "triangle-down", radius = 2, enabled = true };
        dataset.tooltip = new ToolTipDTO { useHTML = true };
        dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 3 } };
        dataset.enableMouseTracking = false;
        foreach (var rezItem in sis)
        {
          List<double> coordinates = new List<double>();
          coordinates.Add(rezItem.X);
          coordinates.Add((double)rezItem.K);
          dataset.data.Add(coordinates);
        }
        datasetList.Add(dataset);
      }
      return datasetList;
    }
    private List<SeriesDTO> CreateGraphData(List<List<DrillingDTO>> drillingList)
    {
      List<SeriesDTO> datasetList = new List<SeriesDTO>();
      SeriesDTO dataset;
      var name = "Set-";
      int count = 0;
      var random = new Random();
      foreach (var rezList in drillingList)
      {
        count++;
        dataset = new SeriesDTO();
        dataset.name = name + count.ToString();
        dataset.lineWidth = 0;
        dataset.color = String.Format("#{0:X6}", random.Next(0x1000000));
        dataset.showInLegend = false;
        dataset.marker = new MarkerDTO { symbol = "triangle", radius = 2, enabled = true };
        dataset.tooltip = new ToolTipDTO { useHTML = true };
        dataset.states = new StatesDTO { hover = new HoverDTO { lineWidthPlus = 0 } };
        dataset.enableMouseTracking = false;
        foreach (var rezItem in rezList)
        {
          List<double> coordinates = new List<double>();
          coordinates.Add(rezItem.X);
          coordinates.Add((double)rezItem.K);
          dataset.data.Add(coordinates);
        }
        datasetList.Add(dataset);
      }
      return datasetList;
    }

    #endregion
  }

  public interface IGraphManager : IBaseManager
  {
    ResultDTO CheckExcel(ExcelModelDTO excel, string path);
    ResultDTO Visualize(GraphDTO graph, string path, SessionDTO session);
    List<SeriesDTO> CreateGraphData(long ruleID, CrossSectionDTO sectionDTO, ParametersDTO parameters);
    ResultDTO FetchSetList();
    ResultDTO FetchSetListLite(Rule rule);
    ResultDTO FetchRule(long ruleID);
    ResultDTO FetchRuleTextAndResistivity(long ruleID);
  }
}
