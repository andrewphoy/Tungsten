namespace Tungsten; 
public static class Current {

    public static DateOnly WeddingDate => new DateOnly(2026, 8, 22);

    public static int DaysToGo {
        get {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var dateOnly = new DateOnly(local.Year, local.Month, local.Day);
            if (WeddingDate <= dateOnly) {
                return 0;
            }
            return WeddingDate.DayNumber - dateOnly.DayNumber;
        }
    }

    public static string DaysToGoString {
        get {
            int days = DaysToGo;
            string plural = days == 1 ? "day" : "days";
            return $"{days} {plural} to go!";
        }
    }

}
