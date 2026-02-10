using System;

using DateTime = System.DateTime;

namespace unillm
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// DateTime 转 Unix 时间戳（秒）
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// DateTime 转 Unix 时间戳（毫秒）
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimestampMs(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Unix 时间戳（秒）转 DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        /// <summary>
        /// Unix 时间戳（毫秒）转 DateTime
        /// </summary>
        /// <param name="timestampMs"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestampMs(long timestampMs)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampMs);
        }
    }

}
