using System.Text.RegularExpressions;

namespace TestTask.LogStandardizerTask.ParsingPattern;

public interface ILogParsingPattern
{
	Regex Regex { get; }
}

