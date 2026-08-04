using System.Text.RegularExpressions;
using TestTask.LogStandardizerTask.ParsingPattern;

namespace TestTask.LogStandardizerTask.ParsingPatterns;

public sealed partial class WhitespaceDelimitedLogParsingPattern
	: ILogParsingPattern
{
	public Regex Regex => LogRegex();

	[GeneratedRegex(
		@"^(?<date>[^ ]*) " +
		@"(?<time>[^ ]*) " +
		@"(?<level>[^ ]*) " +
		@"(?<message>.*)$")]
	private static partial Regex LogRegex();
}
