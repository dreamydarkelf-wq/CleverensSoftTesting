namespace TestTask.ReaderWriterLockTask;

public static class ReaderWriterLock
{
	private static readonly CustomReaderWriterLock _lock = new();
	private static int _count;

	public static int GetCount()
	{
		_lock.EnterRead();
		try
		{
			return _count;
		}
		finally
		{
			_lock.ExitRead();
		}
	}

	public static void AddToCount(int value)
	{
		_lock.EnterWrite();
		try
		{
			_count += value;
		}
		finally
		{
			_lock.ExitWrite();
		}
	}
}
