using System;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace BusinessClassLibrary.Tasks
{
	/// <summary>
	/// Параметры завершённой задачи.
	/// </summary>
	public class CompletedTaskData
	{
		private readonly TaskStatus _status;
		private readonly AggregateException _exception;
		private readonly object _state;

		/// <summary>
		/// Получает статус задачи: RanToCompletion, Canceled или Faulted.
		/// </summary>
		public TaskStatus Status { get { return _status; } }

		/// <summary>
		/// Получает исключение, возникшее в ходе выполнения задачи. Null если исключений не было.
		/// </summary>
		public AggregateException Exception { get { return _exception; } }

		/// <summary>
		/// Получает объект-состояние задачи.
		/// </summary>
		public object State { get { return _state; } }

		/// <summary>
		/// Инициализирует новый экземпляр CompletedTaskData на основе указанного состояния, исключения и состояния.
		/// </summary>
		/// <param name="status">Состояние задачи.</param>
		/// <param name="exception">Исключения задачи.</param>
		/// <param name="state">Объект-состояние задачи.</param>
		public CompletedTaskData (TaskStatus status, AggregateException exception, object state)
		{
			if ((status != TaskStatus.RanToCompletion) &&
				(status != TaskStatus.Canceled) &&
				(status != TaskStatus.Faulted))
			{
				throw new ArgumentOutOfRangeException ("status");
			}
			Contract.EndContractBlock ();

			_status = status;
			_exception = exception;
			_state = state;
		}

		/// <summary>
		/// Инициализирует новый экземпляр CompletedTaskData на основе указанной задачи.
		/// </summary>
		/// <param name="task">Завершённая задача, параметры которой будет содержать создаваемый экземпляр.</param>
		public CompletedTaskData (Task task)
		{
			if (task == null)
			{
				throw new ArgumentNullException ("task");
			}
			if (!task.IsCompleted)
			{
				throw new ArgumentOutOfRangeException ("task");
			}
			Contract.EndContractBlock ();

			_status = task.Status;
			_exception = task.Exception;
			_state = task.AsyncState;
		}
	}
}
