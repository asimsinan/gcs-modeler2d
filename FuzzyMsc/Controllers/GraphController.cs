using FuzzyMsc.Bll;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Dto.VisualizationDTOS;
using FuzzyMsc.Entity.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
  public class GraphController : Controller
  {
    IGraphManager _graphManager;
    IInputManager _inputManager;

    string uploadTmpFolder = "~/App_Data/Tmp";
    string uploadFolder = "~/App_Data/Tmp/FileUploads";

    public GraphController(IGraphManager graphManager,
      IInputManager ortakManager)
    {
      _graphManager = graphManager;
      _inputManager = ortakManager;
    }
    // GET: Graph
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Visualize()
    {
      return View();
    }
    [HttpGet]
    public JsonResult FetchSetList()
    {
      //var result = _graphManager.FetchSetList();
      Rule rule = (Rule)Session["rule"];
      var result = _graphManager.FetchSetListLite(rule);
      return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult FetchRule(long ruleID)
    {
      ResultDTO result = new ResultDTO();
      var ruleList = ((List<RuleListText>)Session["rules"]).ToList().Where(k => k.ruleID == ruleID).Select(k => new RuleTextEntityDTO
      {
        RuleID = k.ruleID,
        RuleText = k.ruleText
      }).ToList();
      result.ResultObject = ruleList;
      result.Success = true;
      result.Message = "Success.";
      //var result = _graphManager.FetchRule(ruleID);
      return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult FetchRuleTextAndResistivity(long ruleID)
    {
      ResultDTO result = new ResultDTO();
      var ruleList = ((List<RuleListText>)Session["rules"]).ToList().Where(k => k.ruleID == ruleID).Select(k => new RuleTextEntityDTO
      {
        RuleID = k.ruleID,
        RuleText = k.ruleText
      }).ToList();
      var resistivityList = ((List<VariableItem>)Session["variableItems"]).ToList().Where(d => d.variable.ruleID == ruleID && d.variable.variableTypeID == (byte)Enums.VariableType.Input).Select(d => new VariableDTO
      {
        Name = d.variableItemName,
        MinValue = d.minValue,
        MaxValue = d.maxValue,
      }).ToList();
      result.ResultObject = new RuleTextAndResistivityDTO { ruleTextList = ruleList, resistivityList = resistivityList };
      result.Success = true;
      result.Message = "Success.";

      //var result = _graphManager.FetchRuleTextAndResistivity(ruleID);
      return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult UploadExcel()
    {
      var file = Request.Files[0];
      var fileName = Path.GetFileName(file.FileName);

      var path = Path.Combine(Server.MapPath(uploadTmpFolder), fileName);
      file.SaveAs(path);
      var entityItem = new byte[file.ContentLength];
      file.InputStream.Read(entityItem, 0, file.ContentLength);
      file.InputStream.Close();
      DirectoryInfo di = new DirectoryInfo(Server.MapPath(uploadTmpFolder));
      return Json(new { Success = true, Message = "Success", ResultObject = new ExcelModelDTO { name = fileName, data = Convert.ToBase64String(entityItem), path = path } }, JsonRequestBehavior.AllowGet);

    }

    [AllowAnonymous]
    [HttpPost]
    public JsonResult CheckExcel(ExcelModelDTO excel)
    {
      var path = Path.Combine(Server.MapPath(uploadFolder), excel.name);
      var result = _graphManager.CheckExcel(excel, path);
      return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult Visualize(GraphDTO graph)
    {
      ResultDTO result = new ResultDTO();
      try
      {
        var path = Path.Combine(Server.MapPath(uploadFolder), graph.excel.name);
        SessionDTO session = new SessionDTO
        {
          groundItem = (List<VariableItem>)Session["groundItem"],
          groundVariable = (Variable)Session["groundVariable"],
          resistivityItem = (List<VariableItem>)Session["resistivityItem"],
          resistivityVariable = (Variable)Session["resistivityVariable"],
          rule = (Rule)Session["rule"],
          ruleList = (List<RuleList>)Session["ruleList"],
          ruleListItem = (List<RuleListItem>)Session["ruleListItem"],
          rules = (List<RuleListText>)Session["rules"],
          variables = (List<Variable>)Session["variables"],
          variableItems = (List<VariableItem>)Session["variableItems"]
        };
        Rule rule = (Rule)Session["rule"];
        List<VariableItem> variableItems = (List<VariableItem>)Session["variableItems"];
        result = _graphManager.VisualizeEDR(graph, path, session);
        //result = _graphManager.Visualize(graph, path, session);
        return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject }, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
        return Json(new { Success = result.Success, Message = ex.Message, ResultObject = result.ResultObject, Exception = ex.ToString() }, JsonRequestBehavior.AllowGet);
      }
    }
  }
}