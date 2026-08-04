using System.Text.RegularExpressions;
using TestTask.LogStandardizerTask.Models;
using TestTask.LogStandardizerTask.ParsingPattern;

namespace TestTask.LogStandardizerTask.Parsers;

public abstract class LogParsingStrategyBase<TPattern> : ILogParsingStrategy
	where TPattern : ILogParsingPattern, new()
{
	private static readonly TPattern Pattern = new();

	public bool TryParse(string line, out LogEntry entry)
	{
		entry = null!;

		var match = Pattern.Regex.Match(line);

		if (!match.Success)
		{
			return false;
		}

		return TryParseMatch(match, out entry);
	}

	protected abstract bool TryParseMatch(Match match, out LogEntry entry);
}
