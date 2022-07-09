using System.Web;
using System.Web.Mvc;

namespace ChemPoint.GP.APIServices
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
