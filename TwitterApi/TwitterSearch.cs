using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZWVDLL.Bekende_Nederlanders
{
    public class TwitterSearch
    {
        public string PersonName { get; private set; }
        public string SearchResults { get; private set; }
        public List<TweetObject> SearchResultsList { get; private set; }
        public string DateRetrieved { get; private set; }

        internal TwitterSearch(string PersonName, List<TweetObject> SearchResultsList, string DateRetrieved)
        {
            this.PersonName = PersonName;
            this.SearchResultsList = SearchResultsList;
            this.DateRetrieved = DateRetrieved;
            this.SearchResults = string.Empty;
        }

        public override string ToString()
        {
            return "TwitterSearch";
        }
    }
    public class TweetObject
    {
        public string Date { get; private set; }
        public string Text { get; private set; }

        public TweetObject(string Date, string Text)
        {
            this.Date = Date;
            this.Text = Text;
        }
    }
}
