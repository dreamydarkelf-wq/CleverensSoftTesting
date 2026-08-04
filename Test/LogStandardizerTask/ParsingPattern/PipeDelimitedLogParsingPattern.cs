using System.Text.RegularExpressions;

namespace TestTask.LogStandardizerTask.ParsingPattern;

public sealed partial class PipeDelimitedLogParsingPattern : ILogParsingPattern
{
	public Regex Regex => LogRegex();

	[GeneratedRegex(
		@"^(?<dateTime>[^|]+)\|" +
		@"(?<level>[^|]+)\|" +
		@"(?<unused>[^|]*)\|" +
		@"(?<caller>[^|]+)\|" +
		@"(?<message>.*)$")]
	private static partial Regex LogRegex();
}
