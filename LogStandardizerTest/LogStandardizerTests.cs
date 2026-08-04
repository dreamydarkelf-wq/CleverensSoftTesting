using Microsoft.Extensions.DependencyInjection;
using TestTask.LogStandardizerTask.Infrastructure;
using TestTask.LogStandardizerTask.Services;

namespace LogStandardizerTest;

public sealed class LogStandardizerTests : IDisposable
{
	private readonly ServiceProvider _serviceProvider;
	private readonly string _testDirectory;

	public LogStandardizerTests()
	{
		var services = new ServiceCollection();

		services.AddLogStandardizer();

		_serviceProvider = services.BuildServiceProvider();

		_testDirectory = Path.Combine(
			Path.GetTempPath(),
			Guid.NewGuid().ToString());

		Directory.CreateDirectory(_testDirectory);
	}

	[Theory]
	[InlineData("2025-01-15 12:30:45.123|INFO|something|MyClass.Method|Hello world", "15-01-2025\t12:30:45.123\tINFO\tMyClass.Method\tHello world")]
	[InlineData("2025-01-15 12:30:45.123 | INFO | something | MyClass.Method | Hello world", "15-01-2025\t12:30:45.123\tINFO\tMyClass.Method\tHello world")]
	[InlineData("2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: '@MINDEO-M40-D-410244015546'", "10-03-2025\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tКод устройства: '@MINDEO-M40-D-410244015546'")]
	public void Process_ValidPipeLog_WritesStandardizedLogToOutput(string input, string output)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Equal(
			output +
			Environment.NewLine,
			File.ReadAllText(outputFile));

		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("15.01.2025 12:30:45.123 INFO Hello world", "15-01-2025\t12:30:45.123\tINFO\tDEFAULT\tHello world")]
	[InlineData("10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'", "10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tВерсия программы: '3.4.0.48729'")]
	public void Process_ValidWhitespaceLog_WritesStandardizedLogToOutput(string input, string output)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Equal(
			output +
			Environment.NewLine,
			File.ReadAllText(outputFile));

		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("INFO", "INFO")]
	[InlineData("INFORMATION", "INFO")]
	[InlineData("WARN", "WARN")]
	[InlineData("WARNING", "WARN")]
	[InlineData("ERROR", "ERROR")]
	[InlineData("DEBUG", "DEBUG")]
	public void Process_ValidLogLevel_WritesCorrectLogLevel(
		string inputLevel,
		string expectedLevel)
	{
		var input =
			$"2025-01-15 12:30:45.123|{inputLevel}|something|MyClass.Method|Hello world";

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Equal(
			$"15-01-2025\t12:30:45.123\t{expectedLevel}\tMyClass.Method\tHello world" +
			Environment.NewLine,
			File.ReadAllText(outputFile));

		Assert.Empty(File.ReadAllText(problemsFile));
	}

	private string CreateFile(
		string fileName,
		string content)
	{
		var path = GetPath(fileName);

		File.WriteAllText(path, content);

		return path;
	}

	private string GetPath(string fileName)
	{
		return Path.Combine(
			_testDirectory,
			fileName);
	}

	public void Dispose()
	{
		_serviceProvider.Dispose();

		if (Directory.Exists(_testDirectory))
			Directory.Delete(_testDirectory, recursive: true);
	}

	[Fact]
	public void Process_InvalidLogLevel_WritesLineToProblems()
	{
		const string input =
			"2025-01-15 12:30:45.123|TRACE|something|MyClass.Method|Hello world";

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("2025-99-99 12:30:45.123|INFO|something|MyClass.Method|Hello world")]
	[InlineData("99.99.2025 12:30:45.123 INFO Hello world")]
	public void Process_InvalidDate_WritesLineToProblems(
	string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer = _serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("2025-01-15 25:30:45.123|INFO|something|MyClass.Method|Hello world")]
	[InlineData("2025-01-15 12:70:45.123|INFO|something|MyClass.Method|Hello world")]
	[InlineData("2025-01-15 12:30:70.123|INFO|something|MyClass.Method|Hello world")]
	[InlineData("2025-01-15 12:30:45.|INFO|something|MyClass.Method|Hello world")]
	[InlineData("2025-01-15 12:30:45.abc|INFO|something|MyClass.Method|Hello world")]
	[InlineData("15.01.2025 25:30:45.123 INFO Hello world")]
	[InlineData("15.01.2025 12:70:45.123 INFO Hello world")]
	[InlineData("15.01.2025 12:30:70.123 INFO Hello world")]
	[InlineData("15.01.2025 12:30:70. INFO Hello world")]
	[InlineData("15.01.2025 12:30:45.abc INFO Hello world")]
	public void Process_InvalidTime_WritesLineToProblems(
	string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("2025-01-15 12:30:45.123|INFO|something||Hello world")]
	[InlineData("2025-01-15 12:30:45.123|INFO|something|   |Hello world")]
	public void Process_EmptyCaller_WritesLineToProblems(
	string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("2025-01-15 12:30:45.123|INFO|something|MyClass.Method|")]
	[InlineData("2025-01-15 12:30:45.123|INFO|something|MyClass.Method|   ")]
	[InlineData("15.01.2025 12:30:45.123 INFO")]
	[InlineData("15.01.2025 12:30:45.123 INFO    ")]
	public void Process_EmptyMessage_WritesLineToProblems(
	string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Fact]
	public void Process_AllValidLines_WritesAllLinesToOutput()
	{
		const string firstLine =
			"10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'";

		const string secondLine =
			"2025-03-10 15:14:51.5882|INFO|11|MobileComputer.GetDeviceId|Код устройства";

		const string thirdLine =
			"11.03.2025 10:15:20.123 ERROR Произошла ошибка";

		const string fourthLine =
			"2025-03-11 11:20:30.4567|DEBUG|12|Some.Method|Debug message";

		var input = string.Join(
			Environment.NewLine,
			firstLine,
			secondLine,
			thirdLine,
			fourthLine);

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		var outputLines = File.ReadAllLines(outputFile);

		Assert.Equal(4, outputLines.Length);

		Assert.Equal(
			"10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tВерсия программы: '3.4.0.48729'",
			outputLines[0]);

		Assert.Equal(
			"10-03-2025\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tКод устройства",
			outputLines[1]);

		Assert.Equal(
			"11-03-2025\t10:15:20.123\tERROR\tDEFAULT\tПроизошла ошибка",
			outputLines[2]);

		Assert.Equal(
			"11-03-2025\t11:20:30.4567\tDEBUG\tSome.Method\tDebug message",
			outputLines[3]);

		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Fact]
	public void Process_EmptyInput_CreatesEmptyOutputAndProblemsFiles()
	{
		var inputFile = CreateFile("input.txt", string.Empty);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.True(File.Exists(outputFile));
		Assert.True(File.Exists(problemsFile));

		Assert.Empty(File.ReadAllText(outputFile));
		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Fact]
	public void Process_AllInvalidLines_WritesAllLinesToProblems()
	{
		const string firstLine = "invalid line";
		const string secondLine = "another invalid line";
		const string thirdLine = "completely invalid";

		var input = string.Join(
			Environment.NewLine,
			firstLine,
			secondLine,
			thirdLine);

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("12:30:45.1")]
	[InlineData("12:30:45.12")]
	[InlineData("12:30:45.123")]
	[InlineData("12:30:45.123456")]
	public void Process_TimePreservesOriginalFormat(string time)
	{
		var input =
			$"10.03.2025 {time} INFO Test message";

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Equal(
			$"10-03-2025\t{time}\tINFO\tDEFAULT\tTest message" +
			Environment.NewLine,
			File.ReadAllText(outputFile));

		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Fact]
	public void Process_MixedFormats_SeparatesValidAndInvalidLines()
	{
		const string firstLine =
			"10.03.2025 15:14:49.523 INFO First message";

		const string secondLine =
			"2025-03-10 15:14:51.5882|ERROR|11|Some.Method|Second message";

		const string invalidLine =
			"This is not a valid log";

		const string fourthLine =
			"11.03.2025 10:15:20.123 DEBUG Third message";

		var input = string.Join(
			Environment.NewLine,
			firstLine,
			secondLine,
			invalidLine,
			fourthLine);

		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		var outputLines = File.ReadAllLines(outputFile);

		Assert.Equal(3, outputLines.Length);

		Assert.Equal(
			"10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tFirst message",
			outputLines[0]);

		Assert.Equal(
			"10-03-2025\t15:14:51.5882\tERROR\tSome.Method\tSecond message",
			outputLines[1]);

		Assert.Equal(
			"11-03-2025\t10:15:20.123\tDEBUG\tDEFAULT\tThird message",
			outputLines[2]);

		Assert.Equal(
			invalidLine + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("    ")]
	[InlineData("\t")]
	public void Process_EmptyOrWhitespaceLine_WritesLineToProblems(string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));
		Assert.Empty(File.ReadAllText(problemsFile));
	}

	[Theory]
	[InlineData("2025-03-10 15:14:51.5882|INFO|11|MobileComputer.GetDeviceId")]
	[InlineData("2025-03-10 15:14:51.5882|INFO|11")]
	[InlineData("10.03.2025 15:14:49.523")]
	[InlineData("10.03.2025 15:14:49.523 INFO")]
	public void Process_InvalidLogStructure_WritesLineToProblems(string input)
	{
		var inputFile = CreateFile("input.txt", input);
		var outputFile = GetPath("output.txt");
		var problemsFile = GetPath("problems.txt");

		var standardizer =
			_serviceProvider.GetRequiredService<LogStandardizer>();

		standardizer.Process(
			inputFile,
			outputFile,
			problemsFile);

		Assert.Empty(File.ReadAllText(outputFile));

		Assert.Equal(
			input + Environment.NewLine,
			File.ReadAllText(problemsFile));
	}
}
