namespace TestTask.ReaderWriterLockTask;

/*
 * Я выбрал приоритет писателей, поскольку бесконечная задержка записи обычно критичнее, чем задержка чтения.
 * Это исключает голодание писателей. Недостаток такого подхода — при непрерывном потоке писателей читатели могут долго ждать.
 * Если бы требовалась справедливость, потребовалась бы другая политика синхронизации.
 * 
 * Аналогично работает ReaderWriterLockSlim, но думаю суть задачи была не в использовании готовых конструкций
 */
public sealed class CustomReaderWriterLock
{
	private readonly object _sync = new();

	private int _activeReaders;
	private bool _activeWriter;
	private int _waitingWriters;

	public void EnterRead()
	{
		lock (_sync)
		{
			while (_activeWriter || _waitingWriters > 0)
			{
				Monitor.Wait(_sync);
			}

			_activeReaders++;
		}
	}

	public void ExitRead()
	{
		lock (_sync)
		{
			_activeReaders--;

			if (_activeReaders == 0)
			{
				Monitor.PulseAll(_sync);
			}
		}
	}

	public void EnterWrite()
	{
		lock (_sync)
		{
			_waitingWriters++;

			try
			{
				while (_activeReaders > 0 || _activeWriter)
				{
					Monitor.Wait(_sync);
				}

				_activeWriter = true;
			}
			finally
			{
				_waitingWriters--;
			}
		}
	}

	public void ExitWrite()
	{
		lock (_sync)
		{
			_activeWriter = false;
			Monitor.PulseAll(_sync);
		}
	}
}
