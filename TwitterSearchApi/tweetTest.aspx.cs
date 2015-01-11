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
            TwitterSearch TS =  TwitterSearchHandler.TSH.GetTwitterSearchByPersonName("@giel3fm");

            string output = TS.PersonName + "<br />";

            foreach (TweetObject to in TS.SearchResultsList)
            {
                output += to.Text + "br />" + to.Date + "<br /><hr /><br />";
            }

            TestLabel.Text = output;
        }
    }
}