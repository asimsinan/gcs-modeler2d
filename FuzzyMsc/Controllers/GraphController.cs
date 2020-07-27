using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.VisualizationDTOS;
using System;
using System.IO;
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
			var result = _graphManager.FetchSetList();
			return Json(new { Success = result.Success, Message = result.Message, ResultObject= result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult FetchRule(long ruleID)
		{
			var result = _graphManager.FetchRule(ruleID);
			return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult FetchRuleTextAndResistivity(long ruleID)
		{
			var result = _graphManager.FetchRuleTextAndResistivity(ruleID);
			return Json(new { Success = result.Success, Message = result.Message, ResultObject= result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
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
			return Json(new { Succcess = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Visualize(GraphDTO graph)
		{
			ResultDTO result = new ResultDTO();
			try
			{
				var path = Path.Combine(Server.MapPath(uploadFolder), graph.excel.name);
				result = _graphManager.Visualize(graph, path);
				return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception.ToString() }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Success = result.Success, Message = ex.Message, ResultObject = result.ResultObject, Exception = ex.ToString() }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}