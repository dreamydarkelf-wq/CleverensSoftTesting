using TestTask.CompressionTask;

namespace StringCompressorTest;

public class StringCompressorTests
{
	[Theory]
	[InlineData("", "")]
	[InlineData("a", "a")]
	[InlineData("aa", "a2")]
	[InlineData("aaa", "a3")]
	[InlineData("abcdef", "abcdef")]
	[InlineData("aaabbcccdde", "a3b2c3d2e")]
	[InlineData("aaaaaaaaa", "a9")]
	[InlineData("aaaaaaaaaa", "a10")]
	[InlineData("aaaaaaaaaaaaaaaaaaa", "a19")]
	[InlineData("aaaaaaaaaaaaaaaaaaaa", "a20")]
	[InlineData("aaaaaaaaaaaa", "a12")]
	[InlineData("aaabaaa", "a3ba3")]
	public void Compress_ShouldReturnExpectedResult(string input, string expected)
	{
		var actual = StringCompressor.Compress(input);

		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("a", "a")]
	[InlineData("a2", "aa")]
	[InlineData("a3", "aaa")]
	[InlineData("a3ba3", "aaabaaa")]
	[InlineData("abcdef", "abcdef")]
	[InlineData("a3b2c3d2e", "aaabbcccdde")]
	[InlineData("a12", "aaaaaaaaaaaa")]
	[InlineData("a2b", "aab")]
	[InlineData("ab2", "abb")]
	[InlineData("a9", "aaaaaaaaa")]
	[InlineData("a10", "aaaaaaaaaa")]
	public void Decompress_ShouldReturnExpectedResult(string input, string expected)
	{
		var actual = StringCompressor.Decompress(input);

		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("")]
	[InlineData("a")]
	[InlineData("aa")]
	[InlineData("aaa")]
	[InlineData("abcdef")]
	[InlineData("aaabbcccdde")]
	[InlineData("aaaaaaaaaaaa")]
	[InlineData("aabbaa")]
	public void CompressThenDecompress_ShouldReturnOriginalString(string input)
	{
		var compressed = StringCompressor.Compress(input);
		var decompressed = StringCompressor.Decompress(compressed);

		Assert.Equal(input, decompressed);
	}

	[Fact]
	public void Compress_NullInput_ShouldThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			() => StringCompressor.Compress(null!));
	}

	[Fact]
	public void Decompress_NullInput_ShouldThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			() => StringCompressor.Decompress(null!));
	}

	[Theory]
	[InlineData("A")]
	[InlineData("abc2")]
	[InlineData("hello!")]
	[InlineData("a#")]
	[InlineData("a0")]
	[InlineData("нелатынь")]
	[InlineData("`")]
	[InlineData("{")]
	public void Compress_InvalidInput_ShouldThrow(string input)
	{
		Assert.Throws<ArgumentException>(
			() => StringCompressor.Compress(input));
	}

	[Theory]
	[InlineData("a#")]
	[InlineData("A")]
	[InlineData("hello!")]
	[InlineData("h٣")]
	public void Decompress_InvalidInput_ShouldThrow(string input)
	{
		Assert.Throws<ArgumentException>(
			() => StringCompressor.Decompress(input));
	}

	[Theory]
	[InlineData("3a")]
	[InlineData("a0")]
	[InlineData("a00")]
	[InlineData("a01")]
	[InlineData("a02")]
	[InlineData("a0010")]
	[InlineData("a1")]
	[InlineData("a2b1")]
	public void Decompress_InvalidFormat_ShouldThrow(string input)
	{
		Assert.Throws<FormatException>(
			() => StringCompressor.Decompress(input));
	}

	[Fact]
	public void Decompress_LargeCount_ShouldWork()
	{
		var result = StringCompressor.Decompress("a100");

		Assert.Equal(100, result.Length);
		Assert.All(result, c => Assert.Equal('a', c));
	}
}