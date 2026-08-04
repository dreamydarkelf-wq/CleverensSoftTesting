using Microsoft.Extensions.DependencyInjection;
using TestTask.LogStandardizerTask.Parsers;
using TestTask.LogStandardizerTask.Services;

namespace TestTask.LogStandardizerTask.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLogStandardizer(this IServiceCollection services)
	{
		services.AddSingleton<
			ILogParsingStrategy,
			PipeDelimitedLogParserStrategy>();

		services.AddSingleton<
			ILogParsingStrategy,
			WhitespaceDelimitedLogParserStrategy>();

		services.AddSingleton<LogStandardizer>();

		return services;
	}
}
