using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZWVAPI
{
    public class DatabaseStats : DataObject
    {
        private static DatabaseStats _CurrentStats = null;
        public static DatabaseStats CurrentStats
        {
            get
            {
                if (_CurrentStats == null)
                {
                    _CurrentStats = new DatabaseStats(0, 0, TimeConverter.GetDateTime().ToString("d-M-yyyy"));
                    DatabaseStats RetrievedStats = DatabaseStatsHandler.DSH.GetStatsByDate(TimeConverter.GetDateTime().ToString("d-M-yyyy"));

                    if (RetrievedStats != null)
                    {
                        _CurrentStats = RetrievedStats;
                    }
                    else
                    {
                        DatabaseStatsHandler.DSH.AddObject(_CurrentStats);
                    }
                }

                if (_CurrentStats.Date != TimeConverter.GetDateTime().ToString("d-M-yyyy"))
                {
                    DatabaseStats DatabaseStats = _CurrentStats;
                    _CurrentStats = new DatabaseStats(0, 0, TimeConverter.GetDateTime().ToString("d-M-yyyy"));
                    DatabaseStatsHandler.DSH.UpdateObject(DatabaseStats);
                    DatabaseStatsHandler.DSH.AddObject(_CurrentStats);
                }

                if (TimeConverter.GetDateTime().Subtract(_CurrentStats.LastUpdated).Minutes > 15)
                {
                    _CurrentStats.SetlastUpdated();
                    DatabaseStatsHandler.DSH.UpdateObject(_CurrentStats);
                }

                return _CurrentStats;
            }
        }

        public int Requests { get; private set; }
        public string Date { get; private set; }
        private DateTime LastUpdated { get; set; }

        public DatabaseStats(int ID, int Requests, string Date)
        {
            this.ID = ID;
            this.Requests = Requests;
            this.Date = Date;
            this.SetlastUpdated();
        }

        private void SetlastUpdated()
        {
            this.LastUpdated = TimeConverter.GetDateTime();
        }

        internal int AddDatabaseHit()
        {
            this.Requests++;

            return this.Requests;
        }

        public override string ToString()
        {
            return "DatabaseStats";
        }
    }
}
