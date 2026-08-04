using TestTask.LogStandardizerTask.Models;

namespace TestTask.LogStandardizerTask.Parsers;

public interface ILogParsingStrategy
{
	bool TryParse(string line, out LogEntry entry);
}
