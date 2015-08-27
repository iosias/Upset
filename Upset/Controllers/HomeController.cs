using System.Web.Mvc;
using System.Web.Script.Serialization;
using Upset.Functionality;

namespace Upset.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View(SetBuilder.GetUpsetModel());
        }

        [HttpGet]
        public string RecentBuildsJson(string SummonerName)
        {
            return new JavaScriptSerializer().Serialize(SetBuilder.GetRecentGameBuilds(SummonerName));
        }
        [HttpGet]
        public string GetMatchJson(long MatchId, long SummonerId, int Damage)
        {
            return new JavaScriptSerializer().Serialize(SetBuilder.GetMatchBuild(MatchId, SummonerId, Damage));
        }
    }
}