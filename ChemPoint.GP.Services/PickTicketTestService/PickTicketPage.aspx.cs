using System;
using System.Net;

namespace PickTicketTestService
{
    public partial class PickTicketPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ClearHeaders();
            Response.ClearContent();
            Response.StatusCode = (int)HttpStatusCode.OK;
            Response.StatusDescription = "Success";
            Response.Flush();
        }
    }
}