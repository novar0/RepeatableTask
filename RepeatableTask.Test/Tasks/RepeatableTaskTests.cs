using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessClassLibrary.Tasks;

namespace BusinessClassLibrary.Test
{
	[TestClass]
	public class RepeatableTaskTests
	{
		private enum EventKind
		{
			Creation,
			Starting,
			Started,
			Ended
		}

		private static List<Tuple<EventKind, object>> _events;
		private static ManualResetEvent _taskEndedSignaler;

		[TestMethod]
		[TestCategory ("Tasks")]
		public void RepeatableTask_UsingContextAndScheduler ()
		{
			var cts = new CancellationTokenSource ();
			var context = new SynchronizationContextMock (cts.Token);
			var scheduler = new TaskSchedulerMock (cts.Token);
			var oldContext = SynchronizationContext.Current;
			int currentThreadId = Thread.CurrentThread.ManagedThreadId;
			int contextThreadId = context.ThreadId;
			int schedulerThreadId = scheduler.ThreadId;
			try
			{
				SynchronizationContext.SetSynchronizationContext (context);
				_events = new List<Tuple<EventKind, object>> ();
				int nnn = 0;
				using (_taskEndedSignaler = new ManualResetEvent (false))
				{
					using (_endTaskSignaler = new AutoResetEvent (false))
					{
						using (var task = new RepeatableTask (CreateNewTaskContext))
						{
							task.TaskStarting += StartingTaskContext;
							task.TaskStarted += StartedTaskContext;
							task.TaskEnded += EndedTaskContext;

							task.Start (90);
							Assert.IsTrue (task.IsRunning);
							Thread.Sleep (50);
							Assert.AreEqual (nnn + 3, _events.Count);
							Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
							Assert.AreEqual (currentThreadId, _events[nnn + 0].Item2);
							Assert.AreEqual (EventKind.Creation, _events[nnn + 1].Item1);
							var ddd = (Tuple<int, TaskCompletionSource<int>>)_events[nnn + 1].Item2;
							Assert.AreEqual (currentThreadId, ddd.Item1);
							var tcs = ddd.Item2;
							Assert.AreEqual (EventKind.Started, _events[nnn + 2].Item1);
							Assert.AreEqual (currentThreadId, _events[nnn + 2].Item2);
							_taskEndedSignaler.Reset ();
							tcs.SetResult (0);
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.AreEqual (nnn + 4, _events.Count);
							Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
							Assert.AreEqual (contextThreadId, _events[nnn + 3].Item2);
						}
						nnn += 4;
						using (var task = new RepeatableTask (TaskActionContext, scheduler))
						{
							task.TaskStarting += StartingTaskContext;
							task.TaskStarted += StartedTaskContext;
							task.TaskEnded += EndedTaskContext;

							task.Start (90);
							Assert.IsTrue (task.IsRunning);
							Thread.Sleep (50);
							Assert.AreEqual (nnn + 3, _events.Count);
							Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
							Assert.AreEqual (currentThreadId, _events[nnn + 0].Item2);
							Assert.AreEqual (EventKind.Started, _events[nnn + 1].Item1);
							Assert.AreEqual (currentThreadId, _events[nnn + 1].Item2);
							Assert.AreEqual (EventKind.Creation, _events[nnn + 2].Item1);
							Assert.AreEqual (schedulerThreadId, _events[nnn + 2].Item2);
							_taskEndedSignaler.Reset ();
							_endTaskSignaler.Set ();
							Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
							Assert.AreEqual (nnn + 4, _events.Count);
							Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
							Assert.AreEqual (contextThreadId, _events[nnn + 3].Item2);
						}
					}
				}
			}
			finally
			{
				cts.Cancel ();
				SynchronizationContext.SetSynchronizationContext (oldContext);
			}
		}
		private static Task CreateNewTaskContext (object state, CancellationToken cToken)
		{
			var tcs = new TaskCompletionSource<int> (state);
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Creation, Tuple.Create (Thread.CurrentThread.ManagedThreadId, tcs)));
			return tcs.Task;
		}
		private static void TaskActionContext (object state, CancellationToken cToken)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Creation, Thread.CurrentThread.ManagedThreadId));
			_endTaskSignaler.WaitOne ();
		}
		private static void StartingTaskContext (object sender, TaskStartingEventArgs args)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Starting, Thread.CurrentThread.ManagedThreadId));
		}
		private static void StartedTaskContext (object sender, DataEventArgs<object> args)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Started, Thread.CurrentThread.ManagedThreadId));
		}
		private static void EndedTaskContext (object sender, DataEventArgs<CompletedTaskData> args)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Ended, Thread.CurrentThread.ManagedThreadId));
			_taskEndedSignaler.Set ();
		}

		[TestMethod]
		[TestCategory ("Tasks")]
		public void RepeatableTask_RunningTaskFactory ()
		{
			_events = new List<Tuple<EventKind, object>> ();
			using (_taskEndedSignaler = new ManualResetEvent (false))
			{
				using (var task = new RepeatableTask (CreateNewTask))
				{
					task.TaskStarting += StartingTask;
					task.TaskStarted += StartedTask;
					task.TaskEnded += EndedTask;
					Assert.AreEqual (0, _events.Count);
					Assert.IsFalse (task.IsRunning);
					int nnn = 0;

					// нормальный запуск
					task.Start (123);
					Assert.IsTrue (task.IsRunning);
					Assert.AreEqual (nnn + 3, _events.Count);
					Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
					Assert.AreEqual (124, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
					Assert.AreEqual (EventKind.Creation, _events[nnn + 1].Item1);
					var tcs = (TaskCompletionSource<int>)_events[nnn + 1].Item2;
					Assert.AreEqual (124, tcs.Task.AsyncState);
					Assert.AreEqual (EventKind.Started, _events[nnn + 2].Item1);
					Assert.AreEqual (124, ((DataEventArgs<object>)_events[nnn + 2].Item2).Value);
					_taskEndedSignaler.Reset ();
					tcs.SetResult (0);
					Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
					Assert.AreEqual (nnn + 4, _events.Count);
					Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
					var tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
					Assert.IsNull (tcd.Exception);
					Assert.AreEqual (124, tcd.State);
					Assert.AreEqual (TaskStatus.RanToCompletion, tcd.Status);
					nnn += 4;

					// исключение
					task.Start (10);
					Assert.IsTrue (task.IsRunning);
					Assert.AreEqual (nnn + 3, _events.Count);
					Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
					Assert.AreEqual (11, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
					Assert.AreEqual (EventKind.Creation, _events[nnn + 1].Item1);
					tcs = (TaskCompletionSource<int>)_events[nnn + 1].Item2;
					Assert.AreEqual (11, tcs.Task.AsyncState);
					Assert.AreEqual (EventKind.Started, _events[nnn + 2].Item1);
					Assert.AreEqual (11, ((DataEventArgs<object>)_events[nnn + 2].Item2).Value);
					var excpt = new ApplicationException ("---");
					_taskEndedSignaler.Reset ();
					tcs.SetException (excpt);
					Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
					Assert.AreEqual (nnn + 4, _events.Count);
					Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
					tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
					Assert.IsNotNull (tcd.Exception);
					var aExcpt = (AggregateException)tcd.Exception;
					Assert.AreEqual (1, aExcpt.InnerExceptions.Count);
					Assert.AreEqual (excpt, aExcpt.InnerExceptions[0]);
					Assert.AreEqual (11, tcd.State);
					Assert.AreEqual (TaskStatus.Faulted, tcd.Status);
					nnn += 4;

					// отмена из за запуска новой
					task.Start (-111);
					Assert.IsTrue (task.IsRunning);
					Assert.AreEqual (nnn + 3, _events.Count);
					Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
					Assert.AreEqual (-110, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
					Assert.AreEqual (EventKind.Creation, _events[nnn + 1].Item1);
					Assert.AreEqual (-110, ((TaskCompletionSource<int>)_events[nnn + 1].Item2).Task.AsyncState);
					Assert.AreEqual (EventKind.Started, _events[nnn + 2].Item1);
					Assert.AreEqual (-110, ((DataEventArgs<object>)_events[nnn + 2].Item2).Value);
					nnn += 3;
					_taskEndedSignaler.Reset ();
					task.Start (33); // запуск новой задачи пока не заверена предыдущая
					Assert.IsTrue (task.IsRunning);
					Assert.IsTrue (_taskEndedSignaler.WaitOne (1000)); // ждём когда произойдёт отмена предыдущей
					Assert.AreEqual (nnn + 4, _events.Count);
					var last4events = System.Linq.Enumerable.Skip (_events, _events.Count - 4);
					var taskEndedEvent = System.Linq.Enumerable.First (last4events, evnt => evnt.Item1 == EventKind.Ended);
					tcd = ((DataEventArgs<CompletedTaskData>)taskEndedEvent.Item2).Value;
					Assert.IsNull (tcd.Exception);
					Assert.AreEqual (-110, tcd.State);
					Assert.AreEqual (TaskStatus.Canceled, tcd.Status);
					var taskStartedEvents = System.Linq.Enumerable.ToArray (System.Linq.Enumerable.Where (last4events, evnt => evnt != taskEndedEvent));
					Assert.AreEqual (3, taskStartedEvents.Length);
					Assert.AreEqual (EventKind.Starting, taskStartedEvents[0].Item1);
					Assert.AreEqual (34, ((TaskStartingEventArgs)taskStartedEvents[0].Item2).State);
					Assert.AreEqual (EventKind.Creation, taskStartedEvents[1].Item1);
					tcs = (TaskCompletionSource<int>)taskStartedEvents[1].Item2;
					Assert.AreEqual (34, tcs.Task.AsyncState);
					Assert.AreEqual (EventKind.Started, taskStartedEvents[2].Item1);
					Assert.AreEqual (34, ((DataEventArgs<object>)taskStartedEvents[2].Item2).Value);
					_taskEndedSignaler.Reset ();
					tcs.SetResult (0);
					Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
					Assert.AreEqual (nnn + 5, _events.Count);
					Assert.AreEqual (EventKind.Ended, _events[nnn + 4].Item1);
					tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 4].Item2).Value;
					Assert.IsNull (tcd.Exception);
					Assert.AreEqual (34, tcd.State);
					Assert.AreEqual (TaskStatus.RanToCompletion, tcd.Status);
					nnn += 5;

					// отмена в процессе
					task.Start (-111);
					Assert.IsTrue (task.IsRunning);
					Assert.AreEqual (nnn + 3, _events.Count);
					Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
					Assert.AreEqual (-110, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
					Assert.AreEqual (EventKind.Creation, _events[nnn + 1].Item1);
					Assert.AreEqual (-110, ((TaskCompletionSource<int>)_events[nnn + 1].Item2).Task.AsyncState);
					Assert.AreEqual (EventKind.Started, _events[nnn + 2].Item1);
					Assert.AreEqual (-110, ((DataEventArgs<object>)_events[nnn + 2].Item2).Value);
					_taskEndedSignaler.Reset ();
					task.Cancel ();
					Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
					Assert.AreEqual (nnn + 4, _events.Count);
					Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
					tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
					Assert.IsNull (tcd.Exception);
					Assert.AreEqual (-110, tcd.State);
					Assert.AreEqual (TaskStatus.Canceled, tcd.Status);
					nnn += 4;

					// отмена при подтверждении
					task.TaskStarting -= StartingTask;
					task.TaskStarting += StartingAndCancelTask;
					task.Start (0);
					Assert.IsFalse (task.IsRunning);
					Assert.AreEqual (nnn + 1, _events.Count);
					Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
					Assert.AreEqual (1, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
				}
			}
		}

		private static AutoResetEvent _endTaskSignaler;
		[TestMethod]
		[TestCategory ("Tasks")]
		public void RepeatableTask_RunningAction ()
		{
			_events = new List<Tuple<EventKind, object>> ();
			using (_taskEndedSignaler = new ManualResetEvent (false))
			{
				using (_endTaskSignaler = new AutoResetEvent (false))
				{
					using (var task = new RepeatableTask (TaskAction, null))
					{
						task.TaskStarting += StartingTask;
						task.TaskStarted += StartedTask;
						task.TaskEnded += EndedTask;
						Assert.AreEqual (0, _events.Count);
						Assert.IsFalse (task.IsRunning);
						int nnn = 0;

						// нормальный запуск
						task.Start (90);
						Assert.IsTrue (task.IsRunning);
						Thread.Sleep (50);
						Assert.AreEqual (nnn + 3, _events.Count);
						Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
						Assert.AreEqual (91, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
						Assert.AreEqual (EventKind.Started, _events[nnn + 1].Item1);
						Assert.AreEqual (91, ((DataEventArgs<object>)_events[nnn + 1].Item2).Value);
						Assert.AreEqual (EventKind.Creation, _events[nnn + 2].Item1);
						Assert.AreEqual (91, _events[nnn + 2].Item2);
						_taskEndedSignaler.Reset ();
						_endTaskSignaler.Set ();
						Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
						Assert.AreEqual (nnn + 4, _events.Count);
						Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
						var tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
						Assert.IsNull (tcd.Exception);
						Assert.AreEqual (91, tcd.State);
						Assert.AreEqual (TaskStatus.RanToCompletion, tcd.Status);
						nnn += 4;

						// исключение
						task.Start (-9);
						Assert.IsTrue (task.IsRunning);
						Thread.Sleep (50);
						Assert.AreEqual (nnn + 3, _events.Count);
						Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
						Assert.AreEqual (-8, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
						Assert.AreEqual (EventKind.Started, _events[nnn + 1].Item1);
						Assert.AreEqual (-8, ((DataEventArgs<object>)_events[nnn + 1].Item2).Value);
						Assert.AreEqual (EventKind.Creation, _events[nnn + 2].Item1);
						Assert.AreEqual (-8, _events[nnn + 2].Item2);
						_taskEndedSignaler.Reset ();
						_endTaskSignaler.Set ();
						Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
						Assert.AreEqual (nnn + 4, _events.Count);
						Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
						tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
						Assert.IsNotNull (tcd.Exception);
						var aExcpt = (AggregateException)tcd.Exception;
						Assert.AreEqual (1, aExcpt.InnerExceptions.Count);
						Assert.IsInstanceOfType (aExcpt.InnerExceptions[0], typeof (ApplicationException));
						Assert.AreEqual ("===", aExcpt.InnerExceptions[0].Message);
						Assert.AreEqual (-8, tcd.State);
						Assert.AreEqual (TaskStatus.Faulted, tcd.Status);
						nnn += 4;

						// отмена из за запуска новой
						task.Start (-111);
						Assert.IsTrue (task.IsRunning);
						Thread.Sleep (50);
						Assert.AreEqual (nnn + 3, _events.Count);
						Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
						Assert.AreEqual (-110, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
						Assert.AreEqual (EventKind.Started, _events[nnn + 1].Item1);
						Assert.AreEqual (-110, ((DataEventArgs<object>)_events[nnn + 1].Item2).Value);
						Assert.AreEqual (EventKind.Creation, _events[nnn + 2].Item1);
						Assert.AreEqual (-110, _events[nnn + 2].Item2);
						nnn += 3;
						task.Start (33); // запуск новой задачи пока не заверена предыдущая
						Assert.IsTrue (task.IsRunning);
						_taskEndedSignaler.Reset ();
						_endTaskSignaler.Set ();
						Assert.IsTrue (_taskEndedSignaler.WaitOne (1000)); // ждём когда произойдёт отмена предыдущей
						Assert.AreEqual (nnn + 4, _events.Count);
						var last4events = System.Linq.Enumerable.Skip (_events, _events.Count - 4);
						var taskEndedEvent = System.Linq.Enumerable.First (last4events, evnt => evnt.Item1 == EventKind.Ended);
						tcd = ((DataEventArgs<CompletedTaskData>)taskEndedEvent.Item2).Value;
						Assert.IsNull (tcd.Exception);
						Assert.AreEqual (-110, tcd.State);
						Assert.AreEqual (TaskStatus.Canceled, tcd.Status);
						var taskStartedEvents = System.Linq.Enumerable.ToArray (System.Linq.Enumerable.Where (last4events, evnt => evnt != taskEndedEvent));
						Assert.AreEqual (3, taskStartedEvents.Length);
						Assert.AreEqual (EventKind.Starting, taskStartedEvents[0].Item1);
						Assert.AreEqual (34, ((TaskStartingEventArgs)taskStartedEvents[0].Item2).State);
						Assert.AreEqual (EventKind.Started, taskStartedEvents[1].Item1);
						Assert.AreEqual (34, ((DataEventArgs<object>)taskStartedEvents[1].Item2).Value);
						Assert.AreEqual (EventKind.Creation, taskStartedEvents[2].Item1);
						Assert.AreEqual (34, taskStartedEvents[2].Item2);
						_taskEndedSignaler.Reset ();
						_endTaskSignaler.Set ();
						Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
						Assert.AreEqual (nnn + 5, _events.Count);
						Assert.AreEqual (EventKind.Ended, _events[nnn + 4].Item1);
						tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 4].Item2).Value;
						Assert.IsNull (tcd.Exception);
						Assert.AreEqual (34, tcd.State);
						Assert.AreEqual (TaskStatus.RanToCompletion, tcd.Status);
						nnn += 5;

						// отмена в процессе
						task.Start (66);
						Assert.IsTrue (task.IsRunning);
						Thread.Sleep (50);
						Assert.AreEqual (nnn + 3, _events.Count);
						Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
						Assert.AreEqual (67, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
						Assert.AreEqual (EventKind.Started, _events[nnn + 1].Item1);
						Assert.AreEqual (67, ((DataEventArgs<object>)_events[nnn + 1].Item2).Value);
						Assert.AreEqual (EventKind.Creation, _events[nnn + 2].Item1);
						Assert.AreEqual (67, _events[nnn + 2].Item2);
						task.Cancel ();
						_taskEndedSignaler.Reset ();
						_endTaskSignaler.Set ();
						Assert.IsTrue (_taskEndedSignaler.WaitOne (1000));
						Assert.AreEqual (nnn + 4, _events.Count);
						Assert.AreEqual (EventKind.Ended, _events[nnn + 3].Item1);
						tcd = ((DataEventArgs<CompletedTaskData>)_events[nnn + 3].Item2).Value;
						Assert.IsNull (tcd.Exception);
						Assert.AreEqual (67, tcd.State);
						Assert.AreEqual (TaskStatus.Canceled, tcd.Status);
						nnn += 4;

						// отмена при подтверждении
						task.TaskStarting -= StartingTask;
						task.TaskStarting += StartingAndCancelTask;
						task.Start (0);
						Assert.IsFalse (task.IsRunning);
						Assert.AreEqual (nnn + 1, _events.Count);
						Assert.AreEqual (EventKind.Starting, _events[nnn + 0].Item1);
						Assert.AreEqual (1, ((TaskStartingEventArgs)_events[nnn + 0].Item2).State);
					}
				}
			}
		}
		private static Task CreateNewTask (object state, CancellationToken cToken)
		{
			var tcs = new TaskCompletionSource<int> (state);
			cToken.Register (() => tcs.TrySetCanceled ());
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Creation, tcs));
			return tcs.Task;
		}
		private static void TaskAction (object state, CancellationToken cToken)
		{
			var n = (int)state;
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Creation, state));
			_endTaskSignaler.WaitOne ();
			cToken.ThrowIfCancellationRequested ();
			if (n < 0) throw new ApplicationException ("===");
		}
		private static void StartingTask (object sender, TaskStartingEventArgs args)
		{
			var n = (int)args.State;
			args.State = n+1;
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Starting, args));
		}
		private static void StartingAndCancelTask (object sender, TaskStartingEventArgs args)
		{
			var n = (int)args.State;
			args.State = n + 1;
			args.Cancel = true;
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Starting, args));
		}
		private static void StartedTask (object sender, DataEventArgs<object> args)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Started, args));
		}
		private static void EndedTask (object sender, DataEventArgs<CompletedTaskData> args)
		{
			_events.Add (Tuple.Create<EventKind, object> (EventKind.Ended, args));
			_taskEndedSignaler.Set ();
		}
	}
}
