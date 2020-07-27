using FuzzyMsc.Bll;
using FuzzyMsc.Dto;
using System.Web.Mvc;

namespace FuzzyMsc.Controllers
{
    public class HomeController : Controller
    {
        IUserManager _userManager;
        IFuzzyManager _fuzzyManager;
        public HomeController(IUserManager userManager,
            IFuzzyManager fuzzyManager)
        {
            _userManager = userManager;
            _fuzzyManager = fuzzyManager;
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Save() {

            _fuzzyManager.Test(12,12,12);
            ResultDTO result = _userManager.Fetch();
            return Json(new { Success = result.Success, Message = result.Message,ResultObject = result.ResultObject, Exception = result.Exception }, JsonRequestBehavior.AllowGet);
        }
    }
}