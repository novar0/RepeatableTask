using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace BusinessClassLibrary.Tasks
{
	/// <summary>
	/// Повторяемая отменяемая задача на основе фабрики по производству задач.
	/// По команде отмены и при старте новой, уже запущенные задачи отменяются.
	/// </summary>
	/// <remarks>
	/// Подходит для задач типа открытия URL в браузере.
	/// Ключевые характеристики.
	/// Создаётся заранее (а не во время запуска) и потом поддерживает многократный запуск с разным значением параметра,
	/// что удобно для декларативного назначения действий элементам пользовательского интерфейса.
	/// Автоматически создаёт/освобождает необходимые CancellationTokenSorce.
	/// Отслеживает статус предыдущих задач (которым послан сигнал отмены) даже если уже запущены новые.
	/// </remarks>

	// Согласно рекомендациям Stephen Toub (специалист по параллелизму, конкурентности и асинхронности из команды Visual Studio)
	// http://blogs.msdn.com/b/pfxteam/archive/2012/03/25/10287435.aspx
	// для объектов типа Task и CancellationTokenSource не производится освобождение (вызов IDisposable.Dispose()),
	// которое бы значительно усложнило класс.
	[System.Diagnostics.CodeAnalysis.SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class RepeatableTask :
		IDisposable
	{
		private readonly Func<object, CancellationToken, Task> _createTaskFunc;
		private readonly Action<object, CancellationToken> _taskAction;
		private readonly TaskScheduler _taskScheduler;
		private CancellationTokenSource _cancellationTokenSource = null;
		private int _tasksInProgressCount = 0;

		/// <summary>
		/// Инициализирует новый экземпляр RepeatableTask на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		public RepeatableTask (Func<object, CancellationToken, Task> taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException ("taskFactory");
			}
			Contract.EndContractBlock ();

			_createTaskFunc = taskFactory;
		}

		/// <summary>
		/// Инициализирует новый экземпляр RepeatableTask на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.
		/// Укажите null чтобы использовать планировщик по умолчанию.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public RepeatableTask (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler = null)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException ("taskAction");
			}
			Contract.EndContractBlock ();

			_taskScheduler = taskScheduler ?? TaskScheduler.Default;
			_taskAction = taskAction;
		}

		/// <summary>
		/// Запускает задачу с указанным объектом состояния.
		/// Ранее запущенная задача отменяется.
		/// </summary>
		/// <param name="state">Объект-состояние, передаваемый в запускаемую задачу.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "newCts can not be disposed here. It is unclear when it must be disposed. According to recommendations http://blogs.msdn.com/b/pfxteam/archive/2012/03/25/10287435.aspx, disposing may be skipped in this case.")]
		public void Start (object state)
		{
			// уведомляем о запуске задачи. обработчик может установить флаг отмены запуска и поменять объект-состояние
			var startingArgs = new TaskStartingEventArgs (state);
			OnTaskStarting (startingArgs);
			if (startingArgs.Cancel)
			{
				return;
			}
			state = startingArgs.State;

			Interlocked.Increment (ref _tasksInProgressCount);

			// создаём новый токен запроса отмены, запрашивая отмену в предыдущем
			var newCts = new CancellationTokenSource ();
			var lastCancellationTokenSource = Interlocked.Exchange (ref _cancellationTokenSource, newCts);
			if (lastCancellationTokenSource != null)
			{
				lastCancellationTokenSource.Cancel ();
			}
			var cancellationToken = newCts.Token;

			var task = (_createTaskFunc != null) ?
				_createTaskFunc.Invoke (state, cancellationToken) :
				Task.Factory.StartNew (
					st => _taskAction.Invoke (st, cancellationToken),
					state,
					cancellationToken,
					TaskCreationOptions.None,
					_taskScheduler);

			OnTaskStarted (new DataEventArgs<object> (state));

			if (task.IsCompleted)
			{
				Interlocked.Decrement (ref _tasksInProgressCount);
				OnTaskEnded (new DataEventArgs<CompletedTaskData> (new CompletedTaskData (task.Status, task.Exception, state)));
			}
			else
			{
				// для созданной задачи задаём продолжение для уведомления о выполнении, которое будет выполнено в текущем контексте
				var scheduler = (SynchronizationContext.Current != null) ?
					TaskScheduler.FromCurrentSynchronizationContext () :
					TaskScheduler.Current;

				task.ContinueWith (prevTask =>
				{
					Interlocked.Decrement (ref _tasksInProgressCount);
					var taskData = new CompletedTaskData (prevTask.Status, prevTask.Exception, state);
					OnTaskEnded (new DataEventArgs<CompletedTaskData> (taskData));
				}, CancellationToken.None, TaskContinuationOptions.None, scheduler);
			}
		}

		/// <summary>
		/// Отменяет все ранее запущенные задачи.
		/// </summary>
		public void Cancel ()
		{
			var lastCancellationTokenSource = Interlocked.Exchange (ref _cancellationTokenSource, null);
			if (lastCancellationTokenSource != null)
			{
				lastCancellationTokenSource.Cancel ();
			}
		}

		/// <summary>Происходит перед тем, как запустится задача.</summary>
		public event EventHandler<TaskStartingEventArgs> TaskStarting;

		/// <summary>
		/// Вызывает событие TaskStarting с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarting.</param>
		protected virtual void OnTaskStarting (TaskStartingEventArgs args)
		{
			var handler = this.TaskStarting;
			if (handler != null)
			{
				handler.Invoke (this, args);
			}
		}

		/// <summary>Происходит после запуска задачи.</summary>
		public event EventHandler<DataEventArgs<object>> TaskStarted;

		/// <summary>
		/// Вызывает событие TaskStarted с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarted.</param>
		protected virtual void OnTaskStarted (DataEventArgs<object> args)
		{
			var handler = this.TaskStarted;
			if (handler != null)
			{
				handler.Invoke (this, args);
			}
		}

		/// <summary>Происходит после завершения задачи.</summary>
		public event EventHandler<DataEventArgs<CompletedTaskData>> TaskEnded;

		/// <summary>
		/// Вызывает событие TaskEnded с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskEnded.</param>
		protected virtual void OnTaskEnded (DataEventArgs<CompletedTaskData> args)
		{
			var handler = this.TaskEnded;
			if (handler != null)
			{
				handler.Invoke (this, args);
			}
		}

		/// <summary>
		/// Освобождает все занятые ресурсы.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type."),
		System.Diagnostics.CodeAnalysis.SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			TaskStarting = null;
			TaskStarted = null;
			TaskEnded = null;
			Cancel ();
		}

		/// <summary>
		/// Возвращает true если задача в настоящий момент выполняется, иначе false.
		/// </summary>
		public bool IsRunning
		{
			get
			{
				return (_tasksInProgressCount > 0);
			}
		}
	}
}
