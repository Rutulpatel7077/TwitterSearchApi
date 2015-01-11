using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using TwitterApi;

namespace WZWVDLL.Bekende_Nederlanders
{
    public class TwitterSearchHandler
    {
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
        private ObjectCache cache = MemoryCache.Default;
        private CacheItemPolicy policy = new CacheItemPolicy();

        private TwitterSearchHandler()
        {
            TwitterCredentials.SetCredentials("134963002-H5YDx2JOBzHhwpkMADOuzC8sB65HHBvOQJWbmxh3", "Jfaem7V3KhslSWGPwOUaNNfnkyQNcpnl7aTM1kqaul3r2", "DJHYpo2bDOEYJP5ryCjG9yPpZ", "gWj7qOfEIyQSnwo5yQiFzYORJqkmU8dwEgkCPnYJVzH1BbKvpl");
        }

        public string GetTwitterSearchByPersonName(string Name)
        {
            try
            {
                return (cache[Name] as TwitterActionResult).Serialize();
            }
            catch(Exception)
            {

            }

            TwitterSearch ts = null;

            try
            {
                this.SearchCounter++;

                List<TweetObject> SearchResults = this.GetSearchResultsByPersonName(Name);

                ts = new TwitterSearch(Name, SearchResults, DateTime.Now.ToString("d-M-yyyy"));
            }
            catch (Exception e)
            {
                ts = new TwitterSearch(Name, new List<TweetObject>(), DateTime.Now.ToString("d-M-yyyy"));
            }

            TwitterActionResult TAR = new TwitterActionResult(Name, ts, DateTime.Now.ToString("d-M-yyyy"));

            policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(3);
            cache.Set(Name, TAR, policy);

            return TAR.Serialize();
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
