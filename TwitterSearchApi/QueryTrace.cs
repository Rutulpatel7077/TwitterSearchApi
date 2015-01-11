using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZWVAPI
{
    public static class QueryTrace
    {
        private static List<string> ActiveQueries = new List<string>();

        internal static int GetActiveQueryCount
        {
            get
            {
                return ActiveQueries.Count;
            }
        }

        internal static void AddQuery(string Query)
        {
            ActiveQueries.Add(Query.Trim());

            if (GetActiveQueryCount > 1)
            {
                System.Diagnostics.Debug.WriteLine(GetActiveQueryCount);
            }
        }

        internal static void RemoveQuery(string Query)
        {
            Query = Query.Trim();
            foreach (string s in ActiveQueries)
            {
                if (s == Query)
                {
                    ActiveQueries.Remove(s);
                    break;
                }
            }
        }

        public static string GetTrace()
        {
            string Output = "Active queryies are: \n";

            foreach (string s in ActiveQueries)
            {
                Output += s + "\n";
            }

            return Output;
        }
    }
}
