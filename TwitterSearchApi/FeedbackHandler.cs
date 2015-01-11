using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WZWVAPI
{
    public static class FeedbackHandler
    {
        public static void AddError(string errormsg)
        {
            try
            {
                Feedback feedbackLog = GetFeedbackLog();
                feedbackLog.AddFeedback(WZWVAPI.Feedback.FeedbackType.Error, errormsg);
                HttpContext.Current.Session["feedbackLog"] = feedbackLog;
            }
            catch (Exception)
            {

            }
        }

        public static void AddMessage(string Message)
        {
            try
            { 
                Feedback feedbackLog = GetFeedbackLog();
                feedbackLog.AddFeedback(WZWVAPI.Feedback.FeedbackType.Message, Message);
                HttpContext.Current.Session["feedbackLog"] = feedbackLog;
            }
            catch (Exception)
            {

            }
        }

        public static List<string> GetFeedbackAsText(WZWVAPI.Feedback.FeedbackType feedbackType)
        {
            Feedback feedbackLog = GetFeedbackLog();

            if (feedbackType == Feedback.FeedbackType.Error)
            {
                return feedbackLog.ErrorList;
            }
            else
            {
                HttpContext.Current.Session["feedbackLog"] = null;
                return feedbackLog.MessageList;
            }
            
        }

        private static Feedback GetFeedbackLog()
        {
            Feedback feedbackLog;

            try
            {
                if (HttpContext.Current.Session["feedbackLog"] != null)
                {
                    return feedbackLog = (Feedback)HttpContext.Current.Session["feedbackLog"];
                }
                else
                {
                    return feedbackLog = new Feedback();
                }
            }
            catch
            {
                return feedbackLog = new Feedback();
            }
        }

        public static Feedback GetFeedbackLogAPI()
        {
            Feedback feedbackLog;

            try
            {
                if (HttpContext.Current.Session["feedbackLog"] != null)
                {
                    feedbackLog = (Feedback)HttpContext.Current.Session["feedbackLog"];
                    HttpContext.Current.Session["feedbackLog"] = null;
                    return feedbackLog;
                }
                else
                {
                    return feedbackLog = new Feedback();
                }
            }
            catch
            {
                return feedbackLog = new Feedback();
            }
        }

        public static int GetErrorCount()
        {
            return GetFeedbackLog().ErrorList.Count;
        }
    }
}