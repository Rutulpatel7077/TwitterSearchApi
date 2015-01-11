using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WZWVDLL.Bekende_Nederlanders;

namespace TwitterSearchApi
{
    public partial class tweetTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string Name = Request.QueryString["Name"];
            TestLabel.Text = TwitterSearchHandler.TSH.GetTwitterSearchByPersonName(Name);
        }
    }
}