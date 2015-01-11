using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZWVAPI
{
    public static class TimeConverter
    {
        public static DateTime GetDateTime()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "W. Europe Standard Time");
        }

        public static string GetDateTimeAsString()
        {
            return GetDateTime().ToString("d-M-yyyy HH:mm:ss");
        }

    }
}
