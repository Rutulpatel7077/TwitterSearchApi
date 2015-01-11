using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WZWVAPI;

namespace WZWVDLL.Bekende_Nederlanders
{
    public class TwitterSearch : DataObject
    {
        public string PersonName { get; private set; }
        public string SearchResults { get; private set; }
        public List<TweetObject> SearchResultsList { get; private set; }
        public string DateRetrieved { get; private set; }

        public TwitterSearch(int ID, string PersonName, string SearchResults, string DateRetrieved)
            : this(ID, PersonName, JsonConvert.DeserializeObject<List<TweetObject>>(SearchResults), DateRetrieved)
        {

        }

        internal TwitterSearch(int ID, string PersonName, List<TweetObject> SearchResultsList, string DateRetrieved)
            : base()
        {
            this.ID = ID;
            this.PersonName = PersonName;
            this.SearchResultsList = SearchResultsList;
            this.DateRetrieved = DateRetrieved;
            this.SearchResults = JsonConvert.SerializeObject(SearchResultsList);
        }

        public bool ShouldSerializeSearchResults()
        {
            return false;
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
