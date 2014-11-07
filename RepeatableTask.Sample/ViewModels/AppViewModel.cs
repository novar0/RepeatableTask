using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using BusinessClassLibrary;
using BusinessClassLibrary.Tasks;
using BusinessClassLibrary.UI;

namespace RepeatableTask.Sample
{
	public class AppViewModel
	{
		#region private readonly fields

		private readonly BusinessLogic _businessLogicService;
		private readonly ObservableCollection<string> _eventLog = new ObservableCollection<string> ();
		private readonly CommandedRepeatableTask _loadDataTask;
		private readonly CommandedRepeatableTask _workTask1;
		private readonly CommandedRepeatableTask _workTask2;
		private readonly CommandedRepeatableTask _workTask3;
		private readonly CommandedRepeatableTask _contextTask1;
		private readonly CommandedRepeatableTask _contextTask2;
		private readonly CommandedRepeatableTask _contextTask3;
		private readonly RelayCommand<object> _sortListCommand;
		private readonly Func<DataRecord> _dataRecordFactory;
		private readonly Func<IConfirmationView> _confirmationViewFactory;
		private readonly IList<DataRecord> _dataList;

		#endregion

		#region data-bound properties

		public ObservableCollection<string> EventLog { get { return _eventLog; } }
		public CommandedRepeatableTask LoadDataTask { get { return _loadDataTask; } }
		public CommandedRepeatableTask WorkTask1 { get { return _workTask1; } }
		public CommandedRepeatableTask WorkTask2 { get { return _workTask2; } }
		public CommandedRepeatableTask WorkTask3 { get { return _workTask3; } }
		public CommandedRepeatableTask ContextTask1 { get { return _contextTask1; } }
		public CommandedRepeatableTask ContextTask2 { get { return _contextTask2; } }
		public CommandedRepeatableTask ContextTask3 { get { return _contextTask3; } }
		public RelayCommand<object> SortListCommand { get { return _sortListCommand; } }
		public IEnumerable<DataRecord> DataList { get { return _dataList; } }

		#endregion

		#region constructor

		public AppViewModel (
			BusinessLogic businessLogicService,
			Func<DataRecord> dataRecordFactory,
			Func<IConfirmationView> confirmationViewFactory)
		{
			_businessLogicService = businessLogicService;
			_dataRecordFactory = dataRecordFactory;
			_confirmationViewFactory = confirmationViewFactory;

			_loadDataTask = new CommandedRepeatableTask (_businessLogicService.LoadData, TaskScheduler.Default);
			_loadDataTask.TaskStarted += (sender, args) => GeneralTaskStarted (sender, args, "load data");
			_loadDataTask.TaskEnded += (sender, args) => GeneralTaskEnded (sender, args, "load data");
			_loadDataTask.TaskStarting += (sender, e) =>
			{
				if (_confirmationViewFactory.Invoke ().RequestConfirmation () != true)
				{
					e.Cancel = true;
				}
			};

			_workTask1 = new CommandedRepeatableTask (_businessLogicService.Work1, TaskScheduler.Default);
			_workTask1.TaskStarted += (sender, args) => GeneralTaskStarted (sender, args, "work 1");
			_workTask1.TaskEnded += (sender, args) => GeneralTaskEnded (sender, args, "work 1");

			_workTask2 = _workTask1.CreateLinked (_businessLogicService.Work2, TaskScheduler.Default);
			_workTask2.TaskStarted += (sender, args) => GeneralTaskStarted (sender, args, "work 2");
			_workTask2.TaskEnded += (sender, args) => GeneralTaskEnded (sender, args, "work 2");

			_workTask3 = _workTask1.CreateLinked (_businessLogicService.Work3, TaskScheduler.Default);
			_workTask3.TaskStarted += (sender, args) => GeneralTaskStarted (sender, args, "work 3");
			_workTask3.TaskEnded += (sender, args) => GeneralTaskEnded (sender, args, "work 3");

			_contextTask1 = new CommandedRepeatableTask (ContextAction1, TaskScheduler.Default);
			_contextTask1.TaskStarted += (sender, args) => ContextTaskStarted (sender, args, "action 1");
			_contextTask1.TaskEnded += (sender, args) => ContextTaskEnded (sender, args, "action 1");

			_contextTask2 = _contextTask1.CreateLinked (ContextAction2, TaskScheduler.Default);
			_contextTask2.TaskStarted += (sender, args) => ContextTaskStarted (sender, args, "action 2");
			_contextTask2.TaskEnded += (sender, args) => ContextTaskEnded (sender, args, "action 3");

			_contextTask3 = _contextTask1.CreateLinked (ContextAction3, TaskScheduler.Default);
			_contextTask3.TaskStarted += (sender, args) => ContextTaskStarted (sender, args, "action 3");
			_contextTask3.TaskEnded += (sender, args) => ContextTaskEnded (sender, args, "action 3");

			_sortListCommand = new RelayCommand<object> (arg => SortList ((string)arg));

			_dataList = new List<DataRecord> ();

			// создаём данные для примера
			var record = _dataRecordFactory.Invoke ();
			record.IsMarked = true;
			record.Name = "Some Name 1";
			_dataList.Add (record);
			record = _dataRecordFactory.Invoke ();
			record.IsMarked = false;
			record.Name = "Some Name 2";
			_dataList.Add (record);
			record = _dataRecordFactory.Invoke ();
			record.IsMarked = true;
			record.Name = "Some Name 3";
			_dataList.Add (record);

			_eventLog.Add ("Запущена программа");
		}

		#endregion

		private void SortList (string column)
		{
			_eventLog.Add (string.Format ("Сортировка списка по столбцу \"{0}\"", column));
		}
		
		#region logging methods

		private void ContextTaskStarted (object sender, DataEventArgs<object> args, string title)
		{
			var task = sender as CommandedRepeatableTask;
			var record = (DataRecord)args.Value;
			_eventLog.Add (string.Format ("Контекстное действие <{0}> запущено для элемента \"{1}\"", title, record.ID));
		}

		private void ContextTaskEnded (object sender, DataEventArgs<CompletedTaskData> args, string title)
		{
			var task = sender as CommandedRepeatableTask;
			var record = (DataRecord)args.Value.State;
			_eventLog.Add (string.Format ("Контекстное действие <{0}> завершено для элемента \"{1}\"", title, record.ID));
		}

		private void GeneralTaskStarted (object sender, DataEventArgs<object> args, string title)
		{
			var task = sender as CommandedRepeatableTask;
			_eventLog.Add (string.Format ("Задача <{0}> запущена", title));
		}

		private void GeneralTaskEnded (object sender, DataEventArgs<CompletedTaskData> args, string title)
		{
			var task = sender as CommandedRepeatableTask;
			switch (args.Value.Status)
			{
				case TaskStatus.Faulted:
					_eventLog.Add (string.Format ("Задача <{0}> прервалась с ошибкой {1}", title, args.Value.Exception));
					break;
				case TaskStatus.Canceled:
					_eventLog.Add (string.Format ("Задача <{0}> отменена", title));
					break;
				case TaskStatus.RanToCompletion:
					_eventLog.Add (string.Format ("Задача <{0}> завершена", title));
					break;
			}
		}

		#endregion

		#region context methods

		private void ContextAction1 (object data, CancellationToken cancellationToken)
		{
			var record = (DataRecord)data;
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (3);
			}
		}

		private void ContextAction2 (object data, CancellationToken cancellationToken)
		{
			var record = (DataRecord)data;
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (3);
			}
		}

		private void ContextAction3 (object data, CancellationToken cancellationToken)
		{
			var record = (DataRecord)data;
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (3);
			}
		}

		#endregion
	}
}
