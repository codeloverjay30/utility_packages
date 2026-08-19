using System;
namespace DateTimeUtilityServices
{
    public static class DateTimeFormatter
    {
        public static string ToStringWithTaiwanCalender(
            this DateTime dateTime
        )
        {
            System.Globalization.TaiwanCalendar calendar = new System.Globalization.TaiwanCalendar();
            return $"{calendar.GetYear(dateTime)}年{dateTime.Month}月{dateTime.Day}日";
        }
    }
}
