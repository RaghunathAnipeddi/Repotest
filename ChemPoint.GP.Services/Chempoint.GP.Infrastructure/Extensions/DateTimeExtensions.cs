using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        private static DateTime defaultDate = default(DateTime);

        public static bool IsEqualTo(this DateTime dateTime, DateTime dateTimeToVerifyAgainst)
        {
            return DateTime.Equals(dateTime, dateTimeToVerifyAgainst);
        }

        public static bool IsNotEqualTo(this DateTime dateTime, DateTime dateTimeToVerifyAgainst)
        {
            return !dateTime.IsEqualTo(dateTimeToVerifyAgainst);
        }

        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.IsDateDefined() && dateTime.Date.IsEqualTo(DateTime.Now.Date);
        }

        public static bool IsNotToday(this DateTime dateTime)
        {
            return !dateTime.IsToday();
        }

        /// <summary>
        /// Creates a new datetime instance with the given Date and time components.
        /// </summary>
        /// <param name="dateComponent">The date component.</param>
        /// <param name="timeComponent">The time component.</param>
        /// <returns>Merged Date Time Instance.</returns>
        public static DateTime Create(this DateTime dateComponent, TimeSpan timeComponent)
        {
            DateTime dt = dateComponent.Date.Add(timeComponent);
            return dt;
        }

        public static TimeSpan GetTimeSpan24hr(this DateTime dateTime)
        {
            TimeSpan spanOfTime = dateTime.TimeOfDay;
            return spanOfTime;
        }

        public static string ToTimeSpanString12hr(this DateTime dateTime)
        {
            return dateTime.ToString("hh:mm tt");
        }

        public static TimeSpan ToTimeSpan(this string timeSpanComponentOnly)
        {
            string dateTimestr = string.Format("{0} {1}", DateTime.Now.ToShortDateString(), timeSpanComponentOnly);
            DateTime dt = DateTime.Parse(dateTimestr);

            return dt.TimeOfDay;
        }

        public static TimeSpan GetTimeSpan24hr(this string spanOfTime)
        {
            //ip: "15:30"
            //op: timespan component of "15:30:00"
            return TimeSpan.ParseExact(spanOfTime, @"h\:m", CultureInfo.InvariantCulture);
        }

        public static TimeSpan GetTimeSpan12Hr(this string spanOfTimeIn12Hr)
        {
            //ip: "11:30 AM"
            //op: timespan component of "11:30 AM"
            return TimeSpan.ParseExact(spanOfTimeIn12Hr, "hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static string ToDisplayFormat(this TimeSpan spanOfTime, bool roundOff = true)
        {
            if (roundOff)
            {
                return spanOfTime.Add(new TimeSpan(0, 0, 1)).ToString(@"hh\:mm");
            }
            else
            {
                return spanOfTime.ToString(@"hh\:mm");
            }
        }

        public static bool IsPriorTo(this TimeSpan sourceSpan, TimeSpan targetSpan)
        {
            return sourceSpan < targetSpan;
        }

        public static bool IsInBetween(this TimeSpan targetSpan, TimeSpan startSpan, TimeSpan endSpan)
        {
            return (targetSpan >= startSpan && targetSpan <= endSpan);
        }

        /// <summary>
        /// True, when the date part of the given datetime instance doesn't equal to default Date
        /// </summary>
        /// <param name="dateTime">source datetime</param>
        /// <returns>Boolean</returns>
        public static bool IsDateDefined(this DateTime dateTime)
        {
            return dateTime.Date.IsNotEqualTo(defaultDate.Date);
        }

        /// <summary>
        /// True, when the date part of the given datetime instance equals default Date
        /// </summary>
        /// <param name="dateTime">source datetime</param>
        /// <returns>Boolean</returns>
        public static bool IsDateNotDefined(this DateTime dateTime)
        {
            return !dateTime.Date.IsNotEqualTo(defaultDate.Date);
        }

        public static List<DateTime> CreateDayRange(this DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            if (dateTimeFrom > dateTimeTo)
            {
                throw new InvalidOperationException("From-DateTime should not be greater than To-DateTime");
            }
            List<DateTime> dateTimeList = new List<DateTime>();
            DateTime currentItem = dateTimeFrom;

            while (currentItem <= dateTimeTo)
            {
                dateTimeList.Add(currentItem);

                currentItem = currentItem.AddDays(1);
            }

            return dateTimeList;
        }

        public static string GetTimeFormat(this TimeSpan span)
        {
            if (span != null)
                return DateTime.Parse(span.ToString()).ToString("hh:mm tt");
            else
                return string.Empty;
        }

        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        public static DateTime ToBookingProcessingTime(this TimeSpan dt)
        {
            return DateTime.ParseExact(dt.ToString(), "HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
