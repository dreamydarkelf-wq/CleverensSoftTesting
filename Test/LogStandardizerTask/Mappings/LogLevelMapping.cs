using TestTask.LogStandardizerTask.Models;

namespace TestTask.LogStandardizerTask.Mappings;

public static class LogLevelMapping
{
	public static LogLevel Map(string value)
	{
		return value.Trim().ToUpperInvariant() switch
		{
			"INFO" or "INFORMATION" => LogLevel.INFO,
			"WARN" or "WARNING" => LogLevel.WARN,
			"ERROR" => LogLevel.ERROR,
			"DEBUG" => LogLevel.DEBUG,

			_ => throw new FormatException(
				$"Неизветсный уровень логирования: {value}")
		};
	}
}
