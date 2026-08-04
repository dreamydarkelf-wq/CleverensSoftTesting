using Microsoft.Extensions.Hosting;
using TestTask.LogStandardizerTask.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((context, services) =>
	{
		services.AddLogStandardizer();
	})
	.Build();