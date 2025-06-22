using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DateTime = System.DateTime;

namespace unillm
{
    public static class DateTimeExtensions
    {
        // DateTime 转 Unix 时间戳（秒）
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        // DateTime 转 Unix 时间戳（毫秒）
        public static long ToUnixTimestampMs(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        // Unix 时间戳（秒）转 DateTime
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        // Unix 时间戳（毫秒）转 DateTime
        public static DateTime FromUnixTimestampMs(long timestampMs)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampMs);
        }
    }

}
