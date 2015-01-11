using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TwitterApi;
using WZWVDLL.Bekende_Nederlanders;

namespace WZWVAPI
{
    /// <summary>
    /// WZWAPI v1.0
    /// </summary>
    public class API : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                context.Response.ContentType = "application/json";

                Action action = null;


                string Name = context.Request.QueryString["Name"];
                context.Response.Write(TwitterSearchHandler.TSH.GetTwitterSearchByPersonName(Name));
            }
            catch (Exception e)
            {
                context.Response.Write(new TwitterActionResult("Name not found", null, DateTime.Now.ToString()).Serialize());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}