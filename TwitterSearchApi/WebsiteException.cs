using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WZWVAPI
{
    public class WebsiteException : DataObject
    {
        public int UserID { get; private set; }
        public string TheException { get; private set; }
        public string TimeStamp { get; private set; }
        public ErrorStatus Status { get; private set; }
        public ErrorOrigin Origin { get; private set; }
        public string ExceptionHash { get; private set; }
        
        public WebsiteException(Exception e, ErrorOrigin Origin = 0) : this(0, 0, e.ToString(), TimeConverter.GetDateTimeAsString(), Encrypt.EncryptPassword(e.ToString()), 0, Origin)
        {

        }

        public WebsiteException(int ID, int UserID, string TheException, string TimeStamp, string ExceptionHash, int Status = 0, int Origin = 0)
            : this(ID, UserID, TheException, TimeStamp, ExceptionHash, (ErrorStatus)Status, (ErrorOrigin)Origin)
        {

        }

        public WebsiteException(int ID, int UserID, string TheException, string TimeStamp, string ExceptionHash, ErrorStatus Status = 0, ErrorOrigin Origin = 0)
        {
            this.ID = ID;
            this.UserID = UserID;
            this.TheException = TheException;
            this.TimeStamp = TimeStamp;
            this.Status = Status;
            this.Origin = Origin;
            this.ExceptionHash = ExceptionHash;

            if (this.ID == 0 && !this.TheException.StartsWith("System.Threading.ThreadAbortException"))
            {
                List<WebsiteException> wList = WebsiteExceptionsHandler.WEH.GetExceptionByHash(Encrypt.EncryptPassword(this.TheException));

                if (wList.Count == 0)
                {
                    this.setAuthor();
                    WebsiteExceptionsHandler.WEH.AddObject(this);
                    FeedbackHandler.AddError("Something went wrong while processing your request, A report has been send to an admin. Please try again.");
                }
                else
                {
                    WebsiteExceptionsHandler.WEH.DeleteObject(wList.First());
                    WebsiteExceptionsHandler.WEH.AddObject(this);
                    FeedbackHandler.AddError("Something went wrong while processing your request but a report has already been sent.");
                }
            }
        }

        public void setAuthor()
        {
            User user = UserHandler.GetLoggedInUser();

            if (user == null)
            {
                this.UserID = 0;
            }
            else
            {
                this.UserID = user.ID;
            }
        }

        public override string ToString()
        {
            return "WebsiteException";
        }
    }
}