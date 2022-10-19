//
// @Description: 与系统时间计算有关的工具类
// @Author: 文若
// @CreateDate: 2022-10-19
// 

using System;
using UnityEngine;

public class DateTimeUtil
{
    private DateTimeUtil(){}

    static readonly string localId = TimeZoneInfo.Local.Id;

    /// <summary>
    /// 获取秒时间戳
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static long GetSecondUnixTime(DateTime time)
    {
        long unixTime = (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        return unixTime > 0 ? unixTime : 0;
    }

    /// <summary>
    /// 时间戳转时间
    /// </summary>
    /// <param name="d">应为服务器时间戳</param>
    /// <returns></returns>
    public static DateTime ConvertLongToDateTime(long d)
    {
        DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1),
            TimeZoneInfo.FindSystemTimeZoneById(localId));
        long lTime = long.Parse(d + "0000");
        TimeSpan toNow = new TimeSpan(lTime);
        DateTime dtResult = dtStart.Add(toNow);
        return dtResult;
    }

    /// <summary>
    /// 时间戳转UTC时间
    /// </summary>
    /// <param name="d"></param>
    /// <param name="offset">时区偏移量</param>
    /// <returns></returns>
    public static DateTime ConvertLongToUTCDateTime(long d, long offset)
    {
        DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
        long lTime = d + offset;
        DateTime dtTime = dtStart.AddMilliseconds(lTime);
        return dtTime;
    }

    /// <summary>
    /// 时间转时间戳
    /// </summary>
    /// <param name="dt">服务器时间</param>
    /// <returns></returns>
    public static long ConvertDateTimeToLong(DateTime dt)
    {
        // DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1),
            TimeZoneInfo.FindSystemTimeZoneById(localId));
        TimeSpan toNow = dt.Subtract(dtStart);
        long timeStamp = toNow.Ticks;
        timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 4));
        return timeStamp;
    }

    /// <summary>
    /// 将Unix时间转换为DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime ConvertUnixTimeToNow(long timestamp)
    {
        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
        DateTime date = start.AddMilliseconds(timestamp);
        return date;
    }

    /// <summary>
    /// 将Unix时间转换为本地时间
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime ConvertUnixTimeToLocalNow(long timestamp)
    {
        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
        DateTime date = start.AddMilliseconds(timestamp);
        return date.ToLocalTime();
    }

    /// <summary>
    /// 将Unix时间转换为本地时间戳
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static long ConvertUnixTimeToLocal(DateTime timestamp)
    {
        var ts = timestamp - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        var time = (long)(ts.TotalMilliseconds);
        return time;
    }

    /// <summary>
    /// 计算时钟模式的旋转量（Rotation分量） model 1.时针 0.分秒针
    /// </summary>
    /// <param name="cValue">当前值</param>
    /// <param name="tValue">一圈值</param>
    /// <returns></returns>
    public static float GetClockAngle(float cValue, float tValue)
    {
        if (tValue == 0) return 0;

        float angle = 0f;
        if (cValue > tValue)
        {
            cValue = cValue % tValue;
        }

        angle = cValue * -(360 / tValue);
        return angle;
    }

    /// <summary>
    /// 获取倒计时
    /// </summary>
    /// <param name="totalSeconds"></param>
    /// <returns></returns>
    public static string GetCountDownSeconds(int totalSeconds)
    {
        //TimeSpan timeSpan = new TimeSpan(totalSeconds * 1000000);
        TimeSpan timeSpan = new TimeSpan(0, 0, totalSeconds);
        return PadLeft(2, timeSpan.Hours) + ":" + PadLeft(2, timeSpan.Minutes) + ":" + PadLeft(2, timeSpan.Seconds);
    }

    /// <summary>
    /// 左边补0
    /// </summary>
    private static string PadLeft(int num, System.Object value)
    {
        return value.ToString().PadLeft(num, '0');
    }

    /// <summary>
    /// 计算得到倒计时时间 
    /// </summary>
    /// <param name="restTime">倒计时时差</param>
    /// <returns></returns>
    public static string GetCountDown(long restTime)
    {
        var time = restTime / 1000;

        var day = "0";
        if (time / (60 * 60 * 24) > 0)
        {
            day = (time / (60 * 60 * 24)).ToString();
            time = time - (int.Parse(day) * 24 * 60 * 60);
        }

        var hour = "0";
        if (time / (60 * 60) > 0)
        {
            hour = (time / (60 * 60)).ToString();
            time = time - (int.Parse(hour) * 60 * 60);
        }

        var min = "0";
        if (time / (60) > 0)
        {
            min = (time / (60)).ToString();
            time = time - (int.Parse(min) * 60);
        }

        var sec = "0";
        sec = time.ToString();
        return string.Format("{0}:{1}:{2}:{3}", day, hour, min, sec);
    }

    /// <summary>
    /// 是否在某一时间段
    /// 时间格式：12:00 13:01 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// /// <param name="ServerNow">服务器时间</param>
    /// <returns> 0间隔之前 1 间隔中 2 间隔之后</returns>
    public static int IsInTime(string start, string end, DateTime ServerNow)
    {
        int now = ServerNow.Hour * 60 + ServerNow.Minute;
        //int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
        string[] stimes = start.Split(':');
        string[] etimes = end.Split(':');
        int s = int.Parse(stimes[0]) * 60 + int.Parse(stimes[1]);
        int e = int.Parse(etimes[0]) * 60 + int.Parse(etimes[1]);
        if (now < s && now < e) return 0;
        else if (now >= s && now < e) return 1;
        else return 2;
    }
}