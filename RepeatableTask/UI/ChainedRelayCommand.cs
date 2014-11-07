using System;
using System.Diagnostics.Contracts;

namespace BusinessClassLibrary.UI
{
	/// <summary>
	/// Команда, выполняющая указанный делегат.
	/// Может быть частью цепи связанных команд.
	/// </summary>
	public class ChainedRelayCommand : ChainedCommandBase
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат
		/// и использовать другой указанный делегат для получения статуса готовности команды.
		/// Команда не будет являтся частью цепи связанных команд.
		/// </summary>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ChainedRelayCommand (Action execute, Func<bool> canExecute = null)
			: base (null)
		{
			if (execute == null)
			{
				throw new ArgumentNullException ("execute");
			}
			Contract.EndContractBlock ();

			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат,
		/// использовать другой указанный делегат для получения статуса готовности команды и
		/// будет являться звеном указанной цепи команд.
		/// </summary>
		/// <param name="commandChain">Цепь связанных комманд, звеном которого станет текущая команда.</param>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
		"CA1026:DefaultParametersShouldNotBeUsed",
		Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ChainedRelayCommand (CommandChain commandChain, Action execute, Func<bool> canExecute = null)
			: base (commandChain)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Определяет готовность отдельно взятой команды (без учёта цепи) к исполнению.
		/// </summary>
		/// <param name="parameter">Не используется.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		protected override bool CanExecuteThis (object parameter)
		{
			return (_canExecute == null) || _canExecute.Invoke ();
		}

		/// <summary>
		/// Исполняет отдельно взятую команду (без учёта цепи).
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		protected override void ExecuteThis (object parameter)
		{
			_execute.Invoke ();
		}
	}

	/// <summary>
	/// Команда, выполняющая указанный делегат.
	/// Может быть частью цепи связанных команд.
	/// </summary>
	/// <typeparam name="T">Тип параметра команды.</typeparam>
	public class RelayCommand<T> : ChainedCommandBase
	{
		private readonly Action<T> _execute;
		private readonly Func<T, bool> _canExecute;

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат
		/// и использовать другой указанный делегат для получения статуса готовности команды.
		/// </summary>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public RelayCommand (Action<T> execute, Func<T, bool> canExecute = null)
			: base (null)
		{
			if (execute == null)
			{
				throw new ArgumentNullException ("execute");
			}
			Contract.EndContractBlock ();

			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ChainedRelayCommand который будет выполнять указанный делегат,
		/// использовать другой указанный делегат для получения статуса готовности команды и
		/// будет являться звеном указанной цепи команд.
		/// </summary>
		/// <param name="commandChain">Цепь связанных комманд, звеном которого станет текущая команда.</param>
		/// <param name="execute">Делегат, который будет выполнен при выполнении команды.</param>
		/// <param name="canExecute">Делегат, который возвращает готовность команды.
		/// Значение по умолчанию null означает что команда всегда готова к выполнению.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public RelayCommand (CommandChain commandChain, Action<T> execute, Func<T, bool> canExecute)
			: base (commandChain)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Определяет готовность отдельно взятой команды (без учёта цепи) к исполнению с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		protected override bool CanExecuteThis (object parameter)
		{
			return (_canExecute == null) || _canExecute.Invoke ((T)parameter);
		}

		/// <summary>
		/// Исполняет отдельно взятую команду (без учёта цепи) с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		protected override void ExecuteThis (object parameter)
		{
			_execute.Invoke ((T)parameter);
		}
	}
}
