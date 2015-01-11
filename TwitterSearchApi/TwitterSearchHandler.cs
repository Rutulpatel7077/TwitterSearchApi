using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using WZWVAPI;

namespace WZWVDLL.Bekende_Nederlanders
{
    public class TwitterSearchHandler : DataHandler 
    {
        private static Field PersonNameField = new Field("PersonName", typeof(string), 100);

        private static TwitterSearchHandler _TSH = null;
        public static TwitterSearchHandler TSH
        {
            get
            {
                if (_TSH == null)
                {
                    _TSH = new TwitterSearchHandler();
                }

                return _TSH;
            }
        }

        private int SearchCounter = 0;

        private TwitterSearchHandler()
            : base("BekendeNederlanders_TwitterSearchHandler", new Field[]
            {
                TwitterSearchHandler.PersonNameField,
                new Field("SearchResults", typeof(string), 40000),
                new Field("DateRetrieved", typeof(string), 75)
            }, typeof(TwitterSearch))
        {
            this.customQueries = new string[] { };
        }

        public TwitterSearch GetTwitterSearchByPersonName(string Name)
        {
            List<TwitterSearch> SearchList = (base.GetObjectByFieldsAndSearchQuery(new Field[] {TwitterSearchHandler.PersonNameField }, Name, true, 0).Cast<TwitterSearch>().ToList());
            TwitterSearch ts = null;

            if (SearchList.Count > 0)
            {
                ts = SearchList.First();
                DateTime tsDateTime;
                DateTime.TryParse(ts.DateRetrieved, CultureInfo.CreateSpecificCulture("nl-NL"), DateTimeStyles.AssumeLocal, out tsDateTime);

                if (TimeConverter.GetDateTime().Subtract(tsDateTime).Hours < 3)
                {
                    return ts;
                }
                else
                {
                    base.DeleteObject(ts);
                    ts = null;
                }
            }

            this.SearchCounter++;

            List<TweetObject> SearchResults = this.GetSearchResultsByPersonName(Name);

            ts = new TwitterSearch(0, Name, SearchResults, TimeConverter.GetDateTime().ToString("d-M-yyyy"));
            this.AddObject(ts);

            return ts;
        }

        private List<TweetObject> GetSearchResultsByPersonName(string Name)
        {
            List<TweetObject> CurrentTweets = new List<TweetObject>();

            if (!(Name.Length > 0 && Name != "Onbekend"))
            {
                return CurrentTweets;
            }

            try
            {
                string UserName = Name;

                if (UserName.Contains('/'))
                {
                    UserName = UserName.Split('/').Last();
                }

                TwitterCredentials.SetCredentials("134963002-H5YDx2JOBzHhwpkMADOuzC8sB65HHBvOQJWbmxh3", "Jfaem7V3KhslSWGPwOUaNNfnkyQNcpnl7aTM1kqaul3r2", "DJHYpo2bDOEYJP5ryCjG9yPpZ", "gWj7qOfEIyQSnwo5yQiFzYORJqkmU8dwEgkCPnYJVzH1BbKvpl");


                IUser user = Tweetinvi.User.GetUserFromScreenName(UserName);

                if (user != null)
                {
                    // Create a parameter for queries with specific parameters
                    var timelineParameter = Timeline.CreateUserTimelineRequestParameter(user);
                    timelineParameter.ExcludeReplies = true;
                    timelineParameter.TrimUser = true;
                    timelineParameter.IncludeRTS = true;

                    var tweets = Timeline.GetUserTimeline(timelineParameter);

                    if (tweets != null)
                    {
                        foreach (var t in tweets)
                        {
                            CurrentTweets.Add(new TweetObject(t.CreatedAt.ToString(new CultureInfo("nl-NL")), t.Text));
                        }
                    }
                }

                return CurrentTweets;
            }
            catch (Exception)
            {
                return CurrentTweets;
            }
        }

        public override string ToString()
        {
            return "TwitterSearchHandler";
        }
    }
}
