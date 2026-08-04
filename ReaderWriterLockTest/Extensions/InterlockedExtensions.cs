namespace ReaderWriterLockTest.Extensions;

public static class InterlockedExtensions
{
	public static void Max(ref int location, int value)
	{
		int current;

		do
		{
			current = location;

			if (current >= value)
				return;

		} while (Interlocked.CompareExchange(
			ref location,
			value,
			current) != current);
	}
}
