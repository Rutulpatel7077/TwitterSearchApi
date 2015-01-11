using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WZWVAPI
{
    public class WebsiteExceptionsHandler : DataHandler , IDoNotRegisterError
    {
        private static WebsiteExceptionsHandler _WEH = null;
        public static WebsiteExceptionsHandler WEH
        {
            get
            {
                if (_WEH == null)
                {
                    _WEH = new WebsiteExceptionsHandler();
                }

                return _WEH;
            }
        }

        private static Field TheExceptionField = new Field("TheException", typeof(string), 3000);
        private static Field StatusField =  new Field("Status", typeof(int), 1);

        private WebsiteExceptionsHandler() : base("Errorlog", new Field[] { 
                new Field("UserID", typeof(int), 1),
                WebsiteExceptionsHandler.TheExceptionField,
                new Field("TimeStamp", typeof(string), 45),
                new Field("ExceptionHash", typeof(string), 250),
                WebsiteExceptionsHandler.StatusField,
                new Field("Origin", typeof(int), 1)
                    }, typeof(WebsiteException) )
        {
            this.customQueries = new string[] { 
                "SELECT * FROM " + this.tableName + " WHERE " + WebsiteExceptionsHandler.StatusField.FieldName + " = @Status AND Origin = @Origin ORDER BY ID DESC",
                "SELECT * FROM " + this.tableName + " WHERE " + WebsiteExceptionsHandler.StatusField.FieldName + " = 0 AND ExceptionHash = @ExceptionHash",
                "SELECT COUNT(*) AS Count FROM " + this.tableName + " WHERE " + WebsiteExceptionsHandler.StatusField.FieldName + " = 0"
            };

            this.LogDatabaseStats = false;
        }

        public WebsiteException GetWebsiteExceptionByID(int ID)
        {
            return base.GetObjectByID(ID) as WebsiteException;
        }

        public List<WebsiteException> getWebsiteExcptionsByStatusAndOrigin(int Status, int Origin)
        {
            return base.CustomQuery(0, new string[] { "@Status", "@Origin" }, new object[] { Status, Origin }).Cast<WebsiteException>().ToList();
        }

        public List<WebsiteException> GetWebsiteExceptionList()
        {
            return base.GetObjectList(0, OrderBy.DESC, new Field("ID", typeof(int), 1)).Cast<WebsiteException>().ToList();
        }

        public List<WebsiteException> GetWebsiteExcptionByText(string Text, bool Exact)
        {
            return base.GetObjectByFieldsAndSearchQuery(new Field[] { WebsiteExceptionsHandler.TheExceptionField }, Text, Exact, 25, OrderBy.ASC, new Field("ID", typeof(int), 1)).Cast<WebsiteException>().ToList();
        }

        public List<WebsiteException> GetExceptionByHash(string Hash)
        {
            return base.CustomQuery(1, new string[] { "@ExceptionHash" }, new object[] { Hash }).Cast<WebsiteException>().ToList();
        }

        public WebsiteException UpdateWebsiteException(int ID, int Status, int origin, string ExceptionText)
        {
            if (ID == 0)
            {
                FeedbackHandler.AddError("Value 0 is not allowed.");
                return null;
            }

            return base.UpdateObject(new WebsiteException(ID, UserHandler.GetLoggedInUser().ID, ExceptionText, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), Encrypt.EncryptPassword(ExceptionText.ToString()), Status, origin)) as WebsiteException;
        }

        public bool DeleteWebsiteException(int ID)
        {
            return base.DeleteObjectByID(ID);
        }

        public int GetUnhandledExceptionsCount()
        {
            return base.GetCountWithCustomQuery(2, new string[] { }, new object[] { });
        }

        public override string ToString()
        {
            return "WebsiteExceptionsHandler";
        }
    }
}