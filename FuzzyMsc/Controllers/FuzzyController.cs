using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
	public class FuzzyController : Controller
	{
		IUserManager userManager;
		IFuzzyManager _fuzzyManager;
		public FuzzyController(IUserManager userManager,
			IFuzzyManager fuzzyManager)
		{
			this.userManager = userManager;
			_fuzzyManager = fuzzyManager;
		}
		// GET: Fuzzy
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public JsonResult Test()
		{
			_fuzzyManager.Test(12, 12, 12);
			ResultDTO result = userManager.Fetch();
			return Json(new { Success = result.Success, Message = result.Message, ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Results(List<FuzzyDTO> groundList)
		{
			List<FuzzyResultDTO> returningValueList = new List<FuzzyResultDTO>();
			try
			{
				foreach (var item in groundList)
				{
					var returningValue = _fuzzyManager.Test(item.Resistivity, item.Resistance, item.Saturation);
					returningValueList.Add(new FuzzyResultDTO { Result = returningValue });
				}
				return Json(new { Success = true, Message = "Success", ResultObject = returningValueList, }, JsonRequestBehavior.AllowGet);
			}
			catch (System.Exception ex)
			{
				return Json(new { Message = "No Success", Exception = ex }, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult FuzzySet()
		{
			return View();
		}

		[HttpPost]
		public JsonResult SaveSet(RuleSetDTO ruleSet)
		{

			var result = _fuzzyManager.SaveSet(ruleSet);

			var session = (SessionDTO)result.ResultObject;
			Session["groundItem"] = session.groundItem;
			Session["groundVariable"] = session.groundVariable;
			Session["resistivityItem"] = session.resistivityItem;
			Session["resistivityVariable"] = session.resistivityVariable;
			Session["rule"] = session.rule;
			Session["ruleList"] = session.ruleList;
			Session["ruleListItem"] = session.ruleListItem;
			Session["rules"] = session.rules;

			return Json(new { Success = result.Success, ResultObject = result.ResultObject, Message = result.Message, Exception = result.Exception }, JsonRequestBehavior.AllowGet);

		}
	}
}