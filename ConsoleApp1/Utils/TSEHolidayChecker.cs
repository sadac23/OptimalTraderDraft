// See https://aka.ms/new-console-template for more information

internal class TSEHolidayChecker
{
    // 日本の祝日を取得するメソッド
    private static HashSet<DateTime> GetJapaneseHolidays(int year)
    {
        var holidays = new HashSet<DateTime>();

        // 元日
        holidays.Add(new DateTime(year, 1, 1));

        // 成人の日 (1月第2月曜日)
        holidays.Add(GetNthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 2));

        // 建国記念の日
        holidays.Add(new DateTime(year, 2, 11));

        // 春分の日
        holidays.Add(GetSpringEquinoxDay(year));

        // 昭和の日
        holidays.Add(new DateTime(year, 4, 29));

        // 憲法記念日
        holidays.Add(new DateTime(year, 5, 3));

        // みどりの日
        holidays.Add(new DateTime(year, 5, 4));

        // こどもの日
        holidays.Add(new DateTime(year, 5, 5));

        // 海の日 (7月第3月曜日)
        holidays.Add(GetNthWeekdayOfMonth(year, 7, DayOfWeek.Monday, 3));

        // 山の日
        holidays.Add(new DateTime(year, 8, 11));

        // 敬老の日 (9月第3月曜日)
        holidays.Add(GetNthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 3));

        // 秋分の日
        holidays.Add(GetAutumnEquinoxDay(year));

        // 体育の日 (10月第2月曜日)
        holidays.Add(GetNthWeekdayOfMonth(year, 10, DayOfWeek.Monday, 2));

        // 文化の日
        holidays.Add(new DateTime(year, 11, 3));

        // 勤労感謝の日
        holidays.Add(new DateTime(year, 11, 23));

        // 天皇誕生日
        holidays.Add(new DateTime(year, 2, 23));

        return holidays;
    }

    // 春分の日を計算するメソッド
    private static DateTime GetSpringEquinoxDay(int year)
    {
        int day;
        if (year <= 1947)
        {
            day = 21;
        }
        else if (year <= 1979)
        {
            day = (int)(20.8357 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        else if (year <= 2099)
        {
            day = (int)(20.8431 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        else
        {
            day = (int)(21.851 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        return new DateTime(year, 3, day);
    }

    // 秋分の日を計算するメソッド
    private static DateTime GetAutumnEquinoxDay(int year)
    {
        int day;
        if (year <= 1947)
        {
            day = 23;
        }
        else if (year <= 1979)
        {
            day = (int)(23.2588 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        else if (year <= 2099)
        {
            day = (int)(23.2488 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        else
        {
            day = (int)(24.2488 + 0.242194 * (year - 1980) - (year - 1983) / 4);
        }
        return new DateTime(year, 9, day);
    }

    // 月の第n番目の特定の曜日の日付を取得するメソッド
    private static DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int n)
    {
        DateTime firstDay = new DateTime(year, month, 1);
        int offset = (int)dayOfWeek - (int)firstDay.DayOfWeek;
        if (offset < 0)
        {
            offset += 7;
        }
        return firstDay.AddDays(offset + 7 * (n - 1));
    }

    // 東証の休業日かどうかを判定するメソッド
    public static bool IsTSEHoliday(DateTime date)
    {
        // 土曜日と日曜日は休業日
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return true;
        }

        // 日本の祝日を取得
        var holidays = GetJapaneseHolidays(date.Year);

        // 祝日が休業日
        if (holidays.Contains(date))
        {
            return true;
        }

        // 年末年始の休業日（12月31日から1月3日まで）
        if ((date.Month == 12 && date.Day == 31) || (date.Month == 1 && date.Day <= 3))
        {
            return true;
        }

        return false;
    }
}