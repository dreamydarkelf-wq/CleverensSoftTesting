namespace TestTask.LogStandardizerTask.Models;

public sealed record LogEntry(
	DateOnly Date,
	string Time,
	LogLevel Level,
	string Caller,
	string Message);
