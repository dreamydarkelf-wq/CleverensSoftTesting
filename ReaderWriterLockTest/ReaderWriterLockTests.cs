using ReaderWriterLockTest.Extensions;
using System.Reflection;
using TestTask.ReaderWriterLockTask;
using ReaderWriterLock = TestTask.ReaderWriterLockTask.ReaderWriterLock;

namespace ReaderWriterLockTest;

public class ReaderWriterLockTests
{
	public ReaderWriterLockTests() => ResetCount();

	private static void ResetCount()
	{
		var field = typeof(ReaderWriterLock)
			.GetField("_count", BindingFlags.Static | BindingFlags.NonPublic)!;

		field.SetValue(null, 0);
	}

	[Fact]
	public void GetCount_ShouldReturnZero_Initially()
	{
		var result = ReaderWriterLock.GetCount();

		Assert.Equal(0, result);
	}

	[Fact]
	public void AddToCount_ShouldIncreaseCount()
	{
		ReaderWriterLock.AddToCount(5);

		Assert.Equal(5, ReaderWriterLock.GetCount());
	}

	[Fact]
	public void AddToCount_ShouldAccumulateValues()
	{
		ReaderWriterLock.AddToCount(5);
		ReaderWriterLock.AddToCount(10);
		ReaderWriterLock.AddToCount(-3);

		Assert.Equal(12, ReaderWriterLock.GetCount());
	}

	[Fact]
	public async Task AddToCount_ShouldNotLoseUpdates_WhenCalledConcurrently()
	{
		const int threadCount = 100;

		using var start = new ManualResetEventSlim(false);

		var tasks = Enumerable.Range(0, threadCount)
			.Select(_ => Task.Run(() =>
			{
				start.Wait();
				ReaderWriterLock.AddToCount(1);
			}))
			.ToArray();

		start.Set();

		await Task.WhenAll(tasks);

		Assert.Equal(threadCount, ReaderWriterLock.GetCount());
	}

	[Fact]
	public async Task GetCount_ShouldReturnSameValue_WhenCalledConcurrently()
	{
		const int readerCount = 100;

		ReaderWriterLock.AddToCount(42);

		using var start = new ManualResetEventSlim(false);

		var tasks = Enumerable.Range(0, readerCount)
			.Select(_ => Task.Run(() =>
			{
				start.Wait();
				return ReaderWriterLock.GetCount();
			}))
			.ToArray();

		start.Set();

		var results = await Task.WhenAll(tasks);

		Assert.All(results, value => Assert.Equal(42, value));
	}

	[Fact]
	public async Task EnterRead_ShouldWait_WhenWriterIsActive()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		using var writerStarted = new ManualResetEventSlim(false);
		using var releaseWriter = new ManualResetEventSlim(false);

		var writerTask = Task.Run(() =>
		{
			readerWriterLock.EnterWrite();

			try
			{
				writerStarted.Set();

				releaseWriter.Wait();
			}
			finally
			{
				readerWriterLock.ExitWrite();
			}
		});

		writerStarted.Wait();

		var readerCompleted = false;

		var readerTask = Task.Run(() =>
		{
			readerWriterLock.EnterRead();

			try
			{
				readerCompleted = true;
			}
			finally
			{
				readerWriterLock.ExitRead();
			}
		});


		await Task.Delay(100);

		Assert.False(readerCompleted);

		releaseWriter.Set();

		await writerTask;
		await readerTask;

		Assert.True(readerCompleted);
	}

	[Fact]
	public async Task EnterWrite_ShouldWait_WhenReaderIsActive()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		using var readerStarted = new ManualResetEventSlim(false);
		using var releaseReader = new ManualResetEventSlim(false);

		var readerTask = Task.Run(() =>
		{
			readerWriterLock.EnterRead();

			try
			{
				readerStarted.Set();

				releaseReader.Wait();
			}
			finally
			{
				readerWriterLock.ExitRead();
			}
		});

		readerStarted.Wait();

		var writerCompleted = false;

		var writerTask = Task.Run(() =>
		{
			readerWriterLock.EnterWrite();

			try
			{
				writerCompleted = true;
			}
			finally
			{
				readerWriterLock.ExitWrite();
			}
		});

		await Task.Delay(100);

		Assert.False(writerCompleted);

		releaseReader.Set();

		await readerTask;
		await writerTask;

		Assert.True(writerCompleted);
	}

	[Fact]
	public async Task EnterRead_ShouldAllowMultipleReaders()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		const int readerCount = 10;

		using var start = new ManualResetEventSlim(false);
		using var releaseReaders = new ManualResetEventSlim(false);

		int activeReaders = 0;
		int maxActiveReaders = 0;

		var tasks = Enumerable.Range(0, readerCount)
			.Select(_ => Task.Run(() =>
			{
				start.Wait();

				readerWriterLock.EnterRead();

				try
				{
					var current = Interlocked.Increment(ref activeReaders);

					InterlockedExtensions.Max(
						ref maxActiveReaders,
						current);

					releaseReaders.Wait();
				}
				finally
				{
					Interlocked.Decrement(ref activeReaders);
					readerWriterLock.ExitRead();
				}
			}))
			.ToArray();

		start.Set();

		await Task.Delay(100);

		Assert.True(maxActiveReaders > 1);

		releaseReaders.Set();

		await Task.WhenAll(tasks);
	}

	[Fact]
	public async Task EnterWrite_ShouldAllowOnlyOneWriterAtTime()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		using var writerStarted = new ManualResetEventSlim(false);
		using var releaseWriter = new ManualResetEventSlim(false);

		var firstWriterTask = Task.Run(() =>
		{
			readerWriterLock.EnterWrite();

			try
			{
				writerStarted.Set();

				releaseWriter.Wait();
			}
			finally
			{
				readerWriterLock.ExitWrite();
			}
		});

		writerStarted.Wait();

		var secondWriterCompleted = false;

		var secondWriterTask = Task.Run(() =>
		{
			readerWriterLock.EnterWrite();

			try
			{
				secondWriterCompleted = true;
			}
			finally
			{
				readerWriterLock.ExitWrite();
			}
		});



		await Task.Delay(100);



		Assert.False(secondWriterCompleted);


		releaseWriter.Set();

		await firstWriterTask;
		await secondWriterTask;


		Assert.True(secondWriterCompleted);
	}

	[Fact]
	public async Task EnterRead_ShouldWait_WhenWriterIsWaiting()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		using var readerStarted = new ManualResetEventSlim(false);
		using var releaseReader = new ManualResetEventSlim(false);

		using var writerStartedWaiting = new ManualResetEventSlim(false);
		using var releaseWriter = new ManualResetEventSlim(false);


		var firstReaderTask = Task.Run(() =>
		{
			readerWriterLock.EnterRead();

			try
			{
				readerStarted.Set();

				releaseReader.Wait();
			}
			finally
			{
				readerWriterLock.ExitRead();
			}
		});


		readerStarted.Wait();


		var writerTask = Task.Run(() =>
		{
			readerWriterLock.EnterWrite();

			try
			{
				writerStartedWaiting.Set();

				releaseWriter.Wait();
			}
			finally
			{
				readerWriterLock.ExitWrite();
			}
		});


		await Task.Delay(100);


		var secondReaderCompleted = false;


		var secondReaderTask = Task.Run(() =>
		{
			readerWriterLock.EnterRead();

			try
			{
				secondReaderCompleted = true;
			}
			finally
			{
				readerWriterLock.ExitRead();
			}
		});



		await Task.Delay(100);



		Assert.False(secondReaderCompleted);


		releaseReader.Set();

		await firstReaderTask;


		await Task.Delay(100);

		Assert.True(writerStartedWaiting.IsSet);


		releaseWriter.Set();

		await writerTask;
		await secondReaderTask;

		Assert.True(secondReaderCompleted);
	}

	[Fact]
	public async Task ReaderWriterLock_ShouldWorkUnderHeavyConcurrency()
	{
		var readerWriterLock = new CustomReaderWriterLock();

		const int writerCount = 20;
		const int readerCount = 50;
		const int writesPerWriter = 1000;

		int value = 0;

		var tasks = new List<Task>();


		for (int i = 0; i < writerCount; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				for (int j = 0; j < writesPerWriter; j++)
				{
					readerWriterLock.EnterWrite();

					try
					{
						value++;
					}
					finally
					{
						readerWriterLock.ExitWrite();
					}
				}
			}));
		}


		for (int i = 0; i < readerCount; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				for (int j = 0; j < writesPerWriter; j++)
				{
					readerWriterLock.EnterRead();

					try
					{
						_ = value;
					}
					finally
					{
						readerWriterLock.ExitRead();
					}
				}
			}));
		}



		await Task.WhenAll(tasks);



		Assert.Equal(
			writerCount * writesPerWriter,
			value);
	}

	[Fact]
	public async Task LastReader_ShouldReleaseWaitingWriter()
	{
		var rwLock = new CustomReaderWriterLock();

		using var reader1Entered = new ManualResetEventSlim(false);
		using var reader2Entered = new ManualResetEventSlim(false);

		using var releaseReaders = new ManualResetEventSlim(false);


		var reader1 = Task.Run(() =>
		{
			rwLock.EnterRead();

			try
			{
				reader1Entered.Set();
				releaseReaders.Wait();
			}
			finally
			{
				rwLock.ExitRead();
			}
		});


		var reader2 = Task.Run(() =>
		{
			rwLock.EnterRead();

			try
			{
				reader2Entered.Set();
				releaseReaders.Wait();
			}
			finally
			{
				rwLock.ExitRead();
			}
		});


		reader1Entered.Wait();
		reader2Entered.Wait();


		var writerEntered = false;


		var writer = Task.Run(() =>
		{
			rwLock.EnterWrite();

			try
			{
				writerEntered = true;
			}
			finally
			{
				rwLock.ExitWrite();
			}
		});


		await Task.Delay(100);

		Assert.False(writerEntered);


		releaseReaders.Set();


		await Task.WhenAll(reader1, reader2, writer);


		Assert.True(writerEntered);
	}

	[Fact]
	public async Task WriterExit_ShouldReleaseWaitingReaders()
	{
		var rwLock = new CustomReaderWriterLock();


		rwLock.EnterWrite();


		int readersCompleted = 0;


		var readers = Enumerable.Range(0, 5)
			.Select(_ => Task.Run(() =>
			{
				rwLock.EnterRead();

				try
				{
					Interlocked.Increment(ref readersCompleted);
				}
				finally
				{
					rwLock.ExitRead();
				}
			}))
			.ToArray();


		await Task.Delay(100);


		Assert.Equal(0, readersCompleted);


		rwLock.ExitWrite();


		await Task.WhenAll(readers);


		Assert.Equal(5, readersCompleted);
	}
}