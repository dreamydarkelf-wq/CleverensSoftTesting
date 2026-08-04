using System.Globalization;
 using TestTask.LogStandardizerTask.Models;
using TestTask.LogStandardizerTask.Parsers;

namespace TestTask.LogStandardizerTask.Services;

public sealed class LogStandardizer
{
	private readonly IReadOnlyCollection<ILogParsingStrategy> _parsers;

	public LogStandardizer(IEnumerable<ILogParsingStrategy> parsers)
	{
		_parsers = parsers.ToArray();
	}

	public void Process(string inputFile, string outputFile, string problemsFile)
	{
		using var reader = new StreamReader(inputFile);
		using var writer = new StreamWriter(outputFile, append: true);
		using var problemsWriter = new StreamWriter(problemsFile, append: true);

		string? line;

		while ((line = reader.ReadLine()) is not null)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			if (TryParse(line, out var entry))
			{
				writer.WriteLine(Format(entry));
			}
			else
			{
				problemsWriter.WriteLine(line);
			}
		}
	}

	private bool TryParse(string line, out LogEntry entry)
	{
		foreach (var parser in _parsers)
		{
			if (parser.TryParse(line, out entry))
			{
				return true;
			}
		}

		entry = null!;
		return false;
	}

	private static string Format(LogEntry entry)
	{
		return string.Join(
			'\t',
			entry.Date.ToString(
				"dd-MM-yyyy",
				CultureInfo.InvariantCulture),
			entry.Time,
			entry.Level,
			entry.Caller,
			entry.Message);
	}
}
