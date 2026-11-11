static class ServiceReport
{
    public static void Run(string[] args)
    {
        if (args.Length != 2)
            throw new FormatException("Usage: service study.tsv");
        var events = Rows(args[1], 5).Select(r =>
        {
            var fields = r[4].Split(':');
            Require(fields.Length == 2 && fields.All(s => s.Length == 2 && s.All(char.IsAsciiDigit)), "Expected service time HH:mm.");
            var hour = int.Parse(fields[0]);
            var minute = int.Parse(fields[1]);
            Require(minute < 60, "Invalid minute.");
            return (Day: Day(r[0]), Stop: r[1], Route: r[2], Direction: r[3], At: 60 * hour + minute);
        });
        foreach (var g in events.GroupBy(e => (e.Day, e.Stop, e.Route, e.Direction)))
        {
            var times = g.Select(e => e.At).ToArray();
            var gaps = times.Skip(1).Zip(times, (b, a) => b - a).ToArray();
            Require(gaps.All(h => h > 0), "Times must increase. Use 24:xx after midnight.");
            var key = $"{g.Key.Day:yyyy-MM-dd} {g.Key.Stop} {g.Key.Route} {g.Key.Direction}";
            if (gaps.Length == 0)
            {
                Console.WriteLine($"{key}: one departure, wait undefined");
                continue;
            }

            Console.WriteLine($"{key}: gaps {string.Join(", ", gaps)} min; expected wait {gaps.Sum(h => (double)h * h) / (2 * gaps.Sum(h => (double)h)):0.##} min");
        }
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static DateOnly Day(string s) => DateOnly.ParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
