using System.Globalization;
using System.Text.RegularExpressions;
using TestTask.LogStandardizerTask.Mappings;
using TestTask.LogStandardizerTask.Models;
using TestTask.LogStandardizerTask.ParsingPattern;

namespace TestTask.LogStandardizerTask.Parsers;

public sealed class PipeDelimitedLogParserStrategy : LogParsingStrategyBase<PipeDelimitedLogParsingPattern>
{
	protected override bool TryParseMatch(Match match, out LogEntry entry)
	{
		entry = null!;

		var dateTimeParts = match.Groups["dateTime"]
			.Value
			.Trim()
			.Split(' ', 2);

		if (dateTimeParts.Length != 2)
		{
			return false;
		}

		var datePart = dateTimeParts[0];

		if (!DateOnly.TryParseExact(
			datePart,
			"yyyy-MM-dd",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var date))
		{
			return false;
		}

		var timePart = dateTimeParts[1];

		var timeParts = timePart.Split('.', 2);

		if (timeParts.Length != 2)
		{
			return false;
		}

		if (!TimeOnly.TryParseExact(
				timeParts[0],
				"HH:mm:ss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out _))
		{
			return false;
		}

		if (string.IsNullOrEmpty(timeParts[1]) || !timeParts[1].All(char.IsDigit))
		{
			return false;
		}

		LogLevel level;

		try
		{
			level = LogLevelMapping.Map(match.Groups["level"].Value.Trim());
		}
		catch (FormatException)
		{
			return false;
		}

		var caller = match.Groups["caller"]
			.Value
			.Trim();

		if (string.IsNullOrWhiteSpace(caller))
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
			timePart,
			level,
			caller,
			message);

		return true;
	}
}