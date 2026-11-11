using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length > 0 && args[0] == "service")
{
    try { ServiceReport.Run(args); return 0; }
    catch (Exception e) when (e is FormatException or IOException or ArgumentException or OverflowException or UnauthorizedAccessException)
    { Console.Error.WriteLine(e.Message); return 1; }
}

if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- service study.tsv  |  original format: <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    foreach (var g in lines.Select(s => Fields(s, 2)).GroupBy(f => f[0]))
    {
        var times = g.Select(f => TimeOnly.ParseExact(f[1], "HH:mm").ToTimeSpan().TotalMinutes).ToArray();
        Check(times.Skip(1).Zip(times, (b,a)=>b>=a).All(v=>v), "Clock times went backwards. Use service mode with an explicit service day and 24:xx after midnight.");
        Check(times.Length >= 2, "Each route needs two departures.");
        double sum = 0, squares = 0;
        var gaps = new List<double>();
        for (int i = 1; i < times.Length; i++)
        {
            var h = times[i] - times[i - 1];
            Check(h > 0, "Duplicate departure.");
            sum += h;
            squares += h * h;
            gaps.Add(h);
        }

        Console.WriteLine($"{g.Key}: gaps {string.Join(", ", gaps)} min; expected wait {squares / (2 * sum):0.##} min");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}
