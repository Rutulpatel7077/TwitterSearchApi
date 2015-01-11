using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZWVAPI
{
    public class DatabaseStatsHandler : DataHandler, IDoNotRegisterError
    {
        private static DatabaseStatsHandler _DSH = null;
        public static DatabaseStatsHandler DSH
        {
            get
            {
                if (_DSH == null)
                {
                    _DSH = new DatabaseStatsHandler();
                }

                return _DSH;
            }
        }

        private static Field DateField = new Field("Date", typeof(string), 25);

        private DatabaseStatsHandler()
            : base("DatabaseStatsHandler", new Field[] { new Field("Requests", typeof(int), 1), DatabaseStatsHandler.DateField }, typeof(DatabaseStats))
        {
            this.customQueries = new string[] { };
        }

        public DatabaseStats GetStatsByDate(string Date)
        {
            List<DatabaseStats> DataList = base.GetObjectByFieldsAndSearchQuery(new Field[] { DatabaseStatsHandler.DateField }, Date, true).Cast<DatabaseStats>().ToList();

            if (DataList.Count > 0)
            {
                return DataList[0];
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return "DatabaseStatsHandler";
        }
    }
}
