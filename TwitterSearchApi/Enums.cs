using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WZWVAPI
{
    public enum FilterStatus { Active = 0, Dead = 1, Fictonal = 2, notAllowed = 3 }
    public enum ErrorStatus { Open, Closed, Irrelevant, WaitingForSolution, WaitingForPatch };
    public enum ErrorOrigin { Website, WindowsApp, WindowsPhoneApp, AndroidApp, IOSApp, DataCrawler, other, BekendeNederlanders };
    public enum PersonType { Artist, Guest, GuestPresenter, Presenter }
    public enum CallGroup { Other, ClientCalls, APIBug, APIFeature, CrawlerBug, CrawlerFeature, WindowsAppBug, WindowsAppFeature, WindowsPhoneAppBug, WindowsPhoneAppFeature, IOSAppBug, IOSAppFeature, AndroidAppBug, AndroidAppFeature, Data, NewsItem, Closed, BekendeNederlandersBug, BekendeNederlandersFeature }
}
