using System.Globalization;
using System.Text.RegularExpressions;
using TestTask.LogStandardizerTask.Mappings;
using TestTask.LogStandardizerTask.Models;
using TestTask.LogStandardizerTask.ParsingPatterns;

namespace TestTask.LogStandardizerTask.Parsers;

public sealed class WhitespaceDelimitedLogParserStrategy : LogParsingStrategyBase<WhitespaceDelimitedLogParsingPattern>
{
	protected override bool TryParseMatch(Match match, out LogEntry entry)
	{
		entry = null!;

		if (!DateOnly.TryParseExact(
				match.Groups["date"].Value,
				"dd.MM.yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date))
		{
			return false;
		}

		var time = match.Groups["time"].Value;

		var timeParts = time.Split('.', 2);

		if (timeParts.Length != 2)
			return false;

		if (!TimeOnly.TryParseExact(
				timeParts[0],
				"HH:mm:ss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out _))
		{
			return false;
		}

		if (string.IsNullOrEmpty(timeParts[1]) ||
			!timeParts[1].All(char.IsDigit))
		{
			return false;
		}

		LogLevel level;

		try
		{
			level = LogLevelMapping.Map(match.Groups["level"].Value);
		}
		catch (FormatException)
		{
			return false;
		}

		var message = match.Groups["message"]
			.Value
			.Trim();

		if (string.IsNullOrWhiteSpace(message))
		{
			return false;
		}

		entry = new LogEntry(
			date,
			time,
			level,
			"DEFAULT",
			message);

		return true;
	}
}