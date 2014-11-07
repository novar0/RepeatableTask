using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using BusinessClassLibrary.Tasks;

namespace BusinessClassLibrary.UI
{
	/// <summary>
	/// Управляемая командами повторяемая задача для привязки к элементам интерфейса.
	/// </summary>
	public class CommandedRepeatableTask : RepeatableTask,
		INotifyPropertyChanged
	{
		private readonly RelayCommand<object> _startCommand;
		private readonly ChainedRelayCommand _stopCommand;

		/// <summary>Получает команду запуска задачи.</summary>
		public RelayCommand<object> StartCommand { get { return _startCommand; } }

		/// <summary>Получает команду остановки задачи.</summary>
		public ChainedRelayCommand StopCommand { get { return _stopCommand; } }

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		public CommandedRepeatableTask (Func<object, CancellationToken, Task> taskFactory)
			: base (taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException ("taskFactory");
			}
			Contract.EndContractBlock ();

			var startChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAll);
			_startCommand = new RelayCommand<object> (startChain, StartInternal, CanStart);

			var stopChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAny);
			_stopCommand = new ChainedRelayCommand (stopChain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		public CommandedRepeatableTask (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler)
			: base (taskAction, taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException ("taskAction");
			}
			if (taskScheduler == null)
			{
				throw new ArgumentNullException ("taskScheduler");
			}
			Contract.EndContractBlock ();

			var startChain = new CommandChain (false, ExecutionAbilityChainBehavior.WhenAll);
			_startCommand = new RelayCommand<object> (startChain, StartInternal, CanStart);

			var stopChain = new CommandChain (true, ExecutionAbilityChainBehavior.WhenAny);
			_stopCommand = new ChainedRelayCommand (stopChain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанной фабрики по производству задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		protected CommandedRepeatableTask (Func<object, CancellationToken, Task> taskFactory, CommandedRepeatableTask previousTask)
			: base (taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException ("taskFactory");
			}
			if (previousTask == null)
			{
				throw new ArgumentNullException ("previousTask");
			}
			Contract.EndContractBlock ();

			_startCommand = new RelayCommand<object> (previousTask._startCommand.Chain, StartInternal, CanStart);
			_stopCommand = new ChainedRelayCommand (previousTask._stopCommand.Chain, Cancel, CanCancel);
		}

		/// <summary>
		/// Инициализирует новый экземпляр CommandedRepeatableTask на основе указанного делегата, планировщика задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		protected CommandedRepeatableTask (
			Action<object, CancellationToken> taskAction,
			TaskScheduler taskScheduler,
			CommandedRepeatableTask previousTask)
			: base (taskAction, taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException ("taskAction");
			}
			if (taskScheduler == null)
			{
				throw new ArgumentNullException ("taskScheduler");
			}
			if (previousTask == null)
			{
				throw new ArgumentNullException ("previousTask");
			}
			Contract.EndContractBlock ();

			_startCommand = new RelayCommand<object> (previousTask._startCommand.Chain, StartInternal, CanStart);
			_stopCommand = new ChainedRelayCommand (previousTask._stopCommand.Chain, Cancel, CanCancel);
		}

		/// <summary>
		/// Создаёт связанную задачу на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		/// <returns>Повторяемая задача на основе указанного делегата создания задачи старта.</returns>
		public CommandedRepeatableTask CreateLinked (Func<object, CancellationToken, Task> taskFactory)
		{
			if (taskFactory == null)
			{
				throw new ArgumentNullException ("taskFactory");
			}
			Contract.EndContractBlock ();

			return new CommandedRepeatableTask (taskFactory, this);
		}

		/// <summary>
		/// Создаёт связанную задачу на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">Делегат, который будут вызывать запускаемые задачи.</param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		/// <returns>Повторяемая задача на основе указанного делегата создания задачи старта.</returns>
		public CommandedRepeatableTask CreateLinked (Action<object, CancellationToken> taskAction, TaskScheduler taskScheduler)
		{
			if (taskAction == null)
			{
				throw new ArgumentNullException ("taskAction");
			}
			if (taskScheduler == null)
			{
				throw new ArgumentNullException ("taskScheduler");
			}
			Contract.EndContractBlock ();

			return new CommandedRepeatableTask (taskAction, taskScheduler, this);
		}

		/// <summary>
		/// Запускает задачу. Преобразовывает объект-состояние при переопределении в наследованном классе.
		/// </summary>
		/// <param name="state">Объект-состояние, передаваемый в запускаемую задачу.</param>
		protected virtual void StartInternal (object state)
		{
			base.Start (state);
		}

		private bool CanStart (object notUsed)
		{
			return !this.IsRunning;
		}

		private bool CanCancel ()
		{
			return this.IsRunning;
		}

		/// <summary>
		/// Вызывает событие TaskStarted с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskStarted.</param>
		protected override void OnTaskStarted (DataEventArgs<object> args)
		{
			base.OnTaskStarted (args);
			_startCommand.RaiseCanExecuteChanged ();
			_stopCommand.RaiseCanExecuteChanged ();
			OnPropertyChanged (new PropertyChangedEventArgs ("IsRunning"));
		}

		/// <summary>
		/// Вызывает событие TaskEnded с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события TaskEnded.</param>
		protected override void OnTaskEnded (DataEventArgs<CompletedTaskData> args)
		{
			base.OnTaskEnded (args);
			_startCommand.RaiseCanExecuteChanged ();
			_stopCommand.RaiseCanExecuteChanged ();
			OnPropertyChanged (new PropertyChangedEventArgs ("IsRunning"));
		}

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Вызывает событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события PropertyChanged.</param>
		protected virtual void OnPropertyChanged (PropertyChangedEventArgs args)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler.Invoke (this, args);
			}
		}
	}
}
