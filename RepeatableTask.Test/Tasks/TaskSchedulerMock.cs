using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessClassLibrary.Test
{
	internal class TaskSchedulerMock : TaskScheduler
	{
		private readonly Thread _thread;
		private readonly CancellationToken _cToken;
		private BlockingCollection<Task> _tasks = new BlockingCollection<Task> ();

		internal int ThreadId { get { return _thread.ManagedThreadId; } }

		internal TaskSchedulerMock (CancellationToken cToken)
		{
			_cToken = cToken;
			_thread = new Thread (ExecuteTaskFromQueue);
			_thread.Start ();
		}
		protected override IEnumerable<Task> GetScheduledTasks ()
		{
			return null;
		}
		protected override void QueueTask (Task task)
		{
			_tasks.Add (task);
		}
		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			return false;
		}
		private void ExecuteTaskFromQueue ()
		{
			foreach (var task in _tasks.GetConsumingEnumerable (_cToken))
			{
				TryExecuteTask (task);
			}
		}
	}
}
