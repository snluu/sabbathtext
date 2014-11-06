﻿using System;

public class Clock
{
    static bool fakeClock = false;
    static TimeSpan fakeClockOffset = TimeSpan.Zero;
    static bool frozen = false;

    public static readonly DateTime MinValue = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime MaxValue = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime UtcNow
    {
        get
        {
            if (fakeClock)
            {
                return DateTime.UtcNow + fakeClockOffset;
            }

            return DateTime.UtcNow;
        }
    }

    public static void UseSystemClock()
    {
        if (frozen)
        {
            throw new ApplicationException("Cannot chance clock mode when it is frozen");
        }

        fakeClock = false;
    }

    public static void UseFakeClock()
    {
        if (frozen)
        {
            throw new ApplicationException("Cannot chance clock mode when it is frozen");
        }

        fakeClock = true;
    }

    public static void Freeze()
    {
        frozen = true;
    }

    public static void RollClock(TimeSpan offset)
    {
        if (!fakeClock)
        {
            throw new ApplicationException("Cannot modify offset when using system clock");
        }

        fakeClockOffset += offset;
    }

    public static void ResetClock()
    {
        if (!fakeClock)
        {
            throw new ApplicationException("Cannot modify offset when using system clock");
        }

        fakeClockOffset = TimeSpan.Zero;
    }
}
