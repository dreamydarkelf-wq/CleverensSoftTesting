using System.Text;

namespace TestTask.CompressionTask;

public static class StringCompressor
{
	public static string Compress(string input)
	{
		ArgumentNullException.ThrowIfNull(input);

		if (!input.All(c => c is >= 'a' and <= 'z'))
		{
			throw new ArgumentException(
				"Входная строка должна содержать только строчные латинские буквы.",
				nameof(input));
		}

		if (input.Length == 0)
		{
			return string.Empty;
		}

		var result = new StringBuilder();

		var currentChar = input[0];
		var count = 1;

		for (var i = 1; i < input.Length; i++)
		{
			if (input[i] == currentChar)
			{
				count++;
				continue;
			}

			AppendGroup(result, currentChar, count);

			currentChar = input[i];
			count = 1;
		}

		AppendGroup(result, currentChar, count);

		return result.ToString();
	}

	public static string Decompress(string input)
	{
		ArgumentNullException.ThrowIfNull(input);

		if (!input.All(c => (c is >= 'a' and <= 'z') || (c >= '0' && c <= '9')))
		{
			throw new ArgumentException(
				"Входная строка должна содержать только строчные латинские буквы и цифры.",
				nameof(input));
		}

		if (input.Length == 0)
		{
			return string.Empty;
		}

		var result = new StringBuilder();

		for (var i = 0; i < input.Length; i++)
		{
			var symbol = input[i];

			if (!char.IsLetter(symbol))
			{
				throw new FormatException($"Ошибка форматирования: '{symbol}' на позиции {i}.");
			}

			var nextCharIndex = i + 1;

			if (nextCharIndex < input.Length && char.IsDigit(input[nextCharIndex]))
			{
				var firstDigit = input[nextCharIndex];

				if (firstDigit == '0')
				{
					throw new FormatException(
						"Количество не может начинаться с нуля.");
				}

				if (firstDigit == '1' &&
					(nextCharIndex + 1 >= input.Length || !char.IsDigit(input[nextCharIndex + 1])))
				{
					throw new FormatException(
						"Количество 1 не должно быть указано явно.");
				}
			}

			var count = 0;

			while (i + 1 < input.Length && char.IsDigit(input[i + 1]))
			{
				i++;
				count = count * 10 + (input[i] - '0');
			}

			if (count == 0)
			{
				count = 1;
			}

			result.Append(symbol, count);
		}

		return result.ToString();
	}

	private static void AppendGroup(StringBuilder builder, char symbol, int count)
	{
		builder.Append(symbol);

		if (count > 1)
		{
			builder.Append(count);
		}
	}
}
