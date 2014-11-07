using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessClassLibrary.UI;

namespace BusinessClassLibrary.Test
{
	[TestClass]
	public class CommandedRepeatableTaskTests
	{
		private static ManualResetEvent _taskEndedSignaler;
		private static int _propertyChangedCount1;
		private static int _propertyChangedCount2;
		private static int _propertyChangedCount3;
		private static int _startCommanCanExecuteChanged1;
		private static int _startCommanCanExecuteChanged2;
		private static int _startCommanCanExecuteChanged3;
		private static int _stopCommanCanExecuteChanged1;
		private static int _stopCommanCanExecuteChanged2;
		private static int _stopCommanCanExecuteChanged3;
		[TestMethod]
		[TestCategory ("UI")]
		public void CommandedRepeatableTask_Commands ()
		{
			_propertyChangedCount1 = 0;
			_propertyChangedCount2 = 0;
			_propertyChangedCount3 = 0;
			_startCommanCanExecuteChanged1 = 0;
			_startCommanCanExecuteChanged2 = 0;
			_startCommanCanExecuteChanged3 = 0;
			_stopCommanCanExecuteChanged1 = 0;
			_stopCommanCanExecuteChanged2 = 0;
			_stopCommanCanExecuteChanged3 = 0;
			using (var task1 = new CommandedRepeatableTask (CreateNewTask))
			{
				task1.PropertyChanged += OnPropertyChanged1;
				task1.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
				task1.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged1;
				task1.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged1;
				using (var task2 = task1.CreateLinked (CreateNewTask))
				{
					task2.PropertyChanged += OnPropertyChanged2;
					task2.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
					task2.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged2;
					task2.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged2;
					using (var task3 = task2.CreateLinked (CreateNewTask))
					{
						using (_taskEndedSignaler = new ManualResetEvent (false))
						{
							task3.PropertyChanged += OnPropertyChanged3;
							task3.TaskEnded += (e, args) => _taskEndedSignaler.Set ();
							task3.StartCommand.CanExecuteChanged += OnStartCommandCanExecuteChanged3;
							task3.StopCommand.CanExecuteChanged += OnStopCommandCanExecuteChanged3;

							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.IsTrue (task1.StartCommand.CanExecute (null));
							Assert.IsTrue (task2.StartCommand.CanExecute (null));
							Assert.IsTrue (task3.StartCommand.CanExecute (null));
							Assert.IsFalse (task1.StopCommand.CanExecute (null));
							Assert.IsFalse (task2.StopCommand.CanExecute (null));
							Assert.IsFalse (task3.StopCommand.CanExecute (null));
							Assert.AreEqual (0, _propertyChangedCount1);
							Assert.AreEqual (0, _propertyChangedCount2);
							Assert.AreEqual (0, _propertyChangedCount3);
							Assert.AreEqual (0, _startCommanCanExecuteChanged1);
							Assert.AreEqual (0, _startCommanCanExecuteChanged2);
							Assert.AreEqual (0, _startCommanCanExecuteChanged3);
							Assert.AreEqual (0, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (0, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (0, _stopCommanCanExecuteChanged3);

							var tcs = new TaskCompletionSource<int> (0);
							task1.StartCommand.Execute (tcs);
							Assert.IsTrue (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (1, _propertyChangedCount1);
							Assert.AreEqual (0, _propertyChangedCount2);
							Assert.AreEqual (0, _propertyChangedCount3);
							Assert.AreEqual (1, _startCommanCanExecuteChanged1);
							Assert.AreEqual (1, _startCommanCanExecuteChanged2);
							Assert.AreEqual (1, _startCommanCanExecuteChanged3);
							Assert.AreEqual (1, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (1, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (1, _stopCommanCanExecuteChanged3);
							Assert.IsFalse (task1.StartCommand.CanExecute (null));
							Assert.IsFalse (task2.StartCommand.CanExecute (null));
							Assert.IsFalse (task3.StartCommand.CanExecute (null));
							Assert.IsTrue (task1.StopCommand.CanExecute (null));
							Assert.IsTrue (task2.StopCommand.CanExecute (null));
							Assert.IsTrue (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (0, _propertyChangedCount2);
							Assert.AreEqual (0, _propertyChangedCount3);
							Assert.AreEqual (2, _startCommanCanExecuteChanged1);
							Assert.AreEqual (2, _startCommanCanExecuteChanged2);
							Assert.AreEqual (2, _startCommanCanExecuteChanged3);
							Assert.AreEqual (2, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (2, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (2, _stopCommanCanExecuteChanged3);
							Assert.IsTrue (task1.StartCommand.CanExecute (null));
							Assert.IsTrue (task2.StartCommand.CanExecute (null));
							Assert.IsTrue (task3.StartCommand.CanExecute (null));
							Assert.IsFalse (task1.StopCommand.CanExecute (null));
							Assert.IsFalse (task2.StopCommand.CanExecute (null));
							Assert.IsFalse (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task2.StartCommand.Execute (tcs);
							Assert.IsFalse (task1.IsRunning);
							Assert.IsTrue (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (1, _propertyChangedCount2);
							Assert.AreEqual (0, _propertyChangedCount3);
							Assert.AreEqual (3, _startCommanCanExecuteChanged1);
							Assert.AreEqual (3, _startCommanCanExecuteChanged2);
							Assert.AreEqual (3, _startCommanCanExecuteChanged3);
							Assert.AreEqual (3, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (3, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (3, _stopCommanCanExecuteChanged3);
							Assert.IsFalse (task1.StartCommand.CanExecute (null));
							Assert.IsFalse (task2.StartCommand.CanExecute (null));
							Assert.IsFalse (task3.StartCommand.CanExecute (null));
							Assert.IsTrue (task1.StopCommand.CanExecute (null));
							Assert.IsTrue (task2.StopCommand.CanExecute (null));
							Assert.IsTrue (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (2, _propertyChangedCount2);
							Assert.AreEqual (0, _propertyChangedCount3);
							Assert.AreEqual (4, _startCommanCanExecuteChanged1);
							Assert.AreEqual (4, _startCommanCanExecuteChanged2);
							Assert.AreEqual (4, _startCommanCanExecuteChanged3);
							Assert.AreEqual (4, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (4, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (4, _stopCommanCanExecuteChanged3);
							Assert.IsTrue (task1.StartCommand.CanExecute (null));
							Assert.IsTrue (task2.StartCommand.CanExecute (null));
							Assert.IsTrue (task3.StartCommand.CanExecute (null));
							Assert.IsFalse (task1.StopCommand.CanExecute (null));
							Assert.IsFalse (task2.StopCommand.CanExecute (null));
							Assert.IsFalse (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task3.StartCommand.Execute (tcs);
							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsTrue (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (2, _propertyChangedCount2);
							Assert.AreEqual (1, _propertyChangedCount3);
							Assert.AreEqual (5, _startCommanCanExecuteChanged1);
							Assert.AreEqual (5, _startCommanCanExecuteChanged2);
							Assert.AreEqual (5, _startCommanCanExecuteChanged3);
							Assert.AreEqual (5, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (5, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (5, _stopCommanCanExecuteChanged3);
							Assert.IsFalse (task1.StartCommand.CanExecute (null));
							Assert.IsFalse (task2.StartCommand.CanExecute (null));
							Assert.IsFalse (task3.StartCommand.CanExecute (null));
							Assert.IsTrue (task1.StopCommand.CanExecute (null));
							Assert.IsTrue (task2.StopCommand.CanExecute (null));
							Assert.IsTrue (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (2, _propertyChangedCount2);
							Assert.AreEqual (2, _propertyChangedCount3);
							Assert.AreEqual (6, _startCommanCanExecuteChanged1);
							Assert.AreEqual (6, _startCommanCanExecuteChanged2);
							Assert.AreEqual (6, _startCommanCanExecuteChanged3);
							Assert.AreEqual (6, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (6, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (6, _stopCommanCanExecuteChanged3);
							Assert.IsTrue (task1.StartCommand.CanExecute (null));
							Assert.IsTrue (task2.StartCommand.CanExecute (null));
							Assert.IsTrue (task3.StartCommand.CanExecute (null));
							Assert.IsFalse (task1.StopCommand.CanExecute (null));
							Assert.IsFalse (task2.StopCommand.CanExecute (null));
							Assert.IsFalse (task3.StopCommand.CanExecute (null));

							tcs = new TaskCompletionSource<int> (0);
							task2.StartCommand.Execute (tcs);
							Assert.IsFalse (task1.IsRunning);
							Assert.IsTrue (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (3, _propertyChangedCount2);
							Assert.AreEqual (2, _propertyChangedCount3);
							Assert.AreEqual (7, _startCommanCanExecuteChanged1);
							Assert.AreEqual (7, _startCommanCanExecuteChanged2);
							Assert.AreEqual (7, _startCommanCanExecuteChanged3);
							Assert.AreEqual (7, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (7, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (7, _stopCommanCanExecuteChanged3);
							Assert.IsFalse (task1.StartCommand.CanExecute (null));
							Assert.IsFalse (task2.StartCommand.CanExecute (null));
							Assert.IsFalse (task3.StartCommand.CanExecute (null));
							Assert.IsTrue (task1.StopCommand.CanExecute (null));
							Assert.IsTrue (task2.StopCommand.CanExecute (null));
							Assert.IsTrue (task3.StopCommand.CanExecute (null));
							_taskEndedSignaler.Reset ();
							task2.StopCommand.Execute (null);
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.IsFalse (task1.IsRunning);
							Assert.IsFalse (task2.IsRunning);
							Assert.IsFalse (task3.IsRunning);
							Assert.AreEqual (2, _propertyChangedCount1);
							Assert.AreEqual (4, _propertyChangedCount2);
							Assert.AreEqual (2, _propertyChangedCount3);
							Assert.AreEqual (8, _startCommanCanExecuteChanged1);
							Assert.AreEqual (8, _startCommanCanExecuteChanged2);
							Assert.AreEqual (8, _startCommanCanExecuteChanged3);
							Assert.AreEqual (8, _stopCommanCanExecuteChanged1);
							Assert.AreEqual (8, _stopCommanCanExecuteChanged2);
							Assert.AreEqual (8, _stopCommanCanExecuteChanged3);
							Assert.IsTrue (task1.StartCommand.CanExecute (null));
							Assert.IsTrue (task2.StartCommand.CanExecute (null));
							Assert.IsTrue (task3.StartCommand.CanExecute (null));
							Assert.IsFalse (task1.StopCommand.CanExecute (null));
							Assert.IsFalse (task2.StopCommand.CanExecute (null));
							Assert.IsFalse (task3.StopCommand.CanExecute (null));
						}
					}
				}
			}
		}

		void OnStartCommandCanExecuteChanged1 (object sender, System.EventArgs e)
		{
			_startCommanCanExecuteChanged1++;
		}
		void OnStartCommandCanExecuteChanged2 (object sender, System.EventArgs e)
		{
			_startCommanCanExecuteChanged2++;
		}
		void OnStartCommandCanExecuteChanged3 (object sender, System.EventArgs e)
		{
			_startCommanCanExecuteChanged3++;
		}
		void OnStopCommandCanExecuteChanged1 (object sender, System.EventArgs e)
		{
			_stopCommanCanExecuteChanged1++;
		}
		void OnStopCommandCanExecuteChanged2 (object sender, System.EventArgs e)
		{
			_stopCommanCanExecuteChanged2++;
		}
		void OnStopCommandCanExecuteChanged3 (object sender, System.EventArgs e)
		{
			_stopCommanCanExecuteChanged3++;
		}
		void OnPropertyChanged1 (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning") _propertyChangedCount1++;
		}
		void OnPropertyChanged2 (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning") _propertyChangedCount2++;
		}
		void OnPropertyChanged3 (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsRunning") _propertyChangedCount3++;
		}
		private static Task CreateNewTask (object state, CancellationToken cToken)
		{
			var tcs = (TaskCompletionSource<int>)state;
			cToken.Register (() => tcs.TrySetCanceled ());
			return tcs.Task;
		}
	}
}
