using Microsoft.Extensions.DependencyInjection;
using TestTask.LogStandardizerTask.Infrastructure;
using TestTask.LogStandardizerTask.Parsers;
using TestTask.LogStandardizerTask.Services;

namespace LogStandardizerTest;

public sealed class ServiceCollectionExtensionsTests
{
	[Fact]
	public void AddLogStandardizer_ShouldRegisterLogStandardizer()
	{
		var services = new ServiceCollection();

		services.AddLogStandardizer();

		using var serviceProvider = services.BuildServiceProvider();

		var standardizer = serviceProvider.GetService<LogStandardizer>();

		Assert.NotNull(standardizer);
	}

	[Fact]
	public void AddLogStandardizer_ShouldRegisterAllParsingStrategies()
	{
		var services = new ServiceCollection();

		services.AddLogStandardizer();

		using var serviceProvider = services.BuildServiceProvider();

		var parsers = serviceProvider
			.GetServices<ILogParsingStrategy>()
			.ToArray();

		Assert.Collection(
			parsers,
			parser =>
				Assert.IsType<PipeDelimitedLogParserStrategy>(
					parser),
			parser =>
				Assert.IsType<WhitespaceDelimitedLogParserStrategy>(
					parser));
	}

	[Fact]
	public void AddLogStandardizer_ShouldRegisterServicesAsSingleton()
	{
		var services = new ServiceCollection();

		services.AddLogStandardizer();

		using var serviceProvider =
			services.BuildServiceProvider();

		var firstStandardizer =
			serviceProvider.GetRequiredService<LogStandardizer>();

		var secondStandardizer =
			serviceProvider.GetRequiredService<LogStandardizer>();

		Assert.Same(
			firstStandardizer,
			secondStandardizer);

		var firstParsers =
			serviceProvider
				.GetServices<ILogParsingStrategy>()
				.ToArray();

		var secondParsers =
			serviceProvider
				.GetServices<ILogParsingStrategy>()
				.ToArray();

		Assert.Equal(
			firstParsers.Length,
			secondParsers.Length);

		for (var i = 0; i < firstParsers.Length; i++)
		{
			Assert.Same(
				firstParsers[i],
				secondParsers[i]);
		}
	}
}