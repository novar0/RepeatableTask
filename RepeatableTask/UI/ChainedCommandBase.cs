using System;
using System.Windows.Input;

namespace BusinessClassLibrary.UI
{
	/// <summary>
	/// Базовый класс команды, которая может быть частью цепи связанных команд.
	/// </summary>
	public abstract class ChainedCommandBase :
		ICommand
	{
		private readonly CommandChain _commandChain;

		/// <summary>
		/// Получает цепь связанных команд, частью которой является команда.
		/// </summary>
		public CommandChain Chain { get { return _commandChain; } }

		/// <summary>
		/// Инициализирует новый экземпляр ChainedCommandBase, являющийся частью указанной цепи связанных команд.
		/// </summary>
		/// <param name="commandChain">
		/// Цепь команд, частью которых станет текущая команда.
		/// Укажите null если команда не должна быть частью цепи.
		/// </param>
		protected ChainedCommandBase (CommandChain commandChain)
		{
			_commandChain = commandChain;
			if (commandChain != null)
			{
				commandChain.Add (this);
			}
		}

		/// <summary>
		/// Происходит когда изменяются факторы, влияющие на готовность команды к исполнению.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Вызывает событие CanExecuteChanged для всех команд в цепи.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design",
			"CA1030:UseEventsWhereAppropriate",
			Justification = "Already event")]
		public void RaiseCanExecuteChanged ()
		{
			if (_commandChain == null)
			{
				RaiseCanExecuteChangedThis ();
			}
			else
			{
				var cmd = _commandChain.FirstCommand;
				while (cmd != null)
				{
					cmd.Value.RaiseCanExecuteChangedThis ();
					cmd = cmd.Next;
				}
			}
		}

		private void RaiseCanExecuteChangedThis ()
		{
			var handler = this.CanExecuteChanged;
			if (handler != null)
			{
				handler.Invoke (this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Определяет готовность команды  к исполнению с указанным параметром как часть цепи команд.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		public bool CanExecute (object parameter)
		{
			if (_commandChain != null)
			{
				var cmd = _commandChain.FirstCommand;
				switch (_commandChain.ExecutionAbilityChainBehavior)
				{
					case ExecutionAbilityChainBehavior.WhenAll:
						while (cmd != null)
						{
							if (!cmd.Value.CanExecuteThis (parameter))
							{
								return false;
							}
							cmd = cmd.Next;
						}
						return true;
					case ExecutionAbilityChainBehavior.WhenAny:
						while (cmd != null)
						{
							if (cmd.Value.CanExecuteThis (parameter))
							{
								return true;
							}
							cmd = cmd.Next;
						}
						return false;
				}
			}
			return CanExecuteThis (parameter);
		}

		/// <summary>
		/// Исполняет команду как часть цепи команд.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		public void Execute (object parameter)
		{
			if ((_commandChain != null) && (_commandChain.ExecutionChained))
			{
				var cmd = _commandChain.FirstCommand;
				while (cmd != null)
				{
					cmd.Value.ExecuteThis (parameter);
					cmd = cmd.Next;
				}
			}
			else
			{
				ExecuteThis (parameter);
			}
		}

		/// <summary>
		/// В наследованных классах определяет готовность отдельно взятой команды (без учёта цепи) к исполнению с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		/// <returns>Признак готовности команды к исполнению.</returns>
		protected abstract bool CanExecuteThis (object parameter);

		/// <summary>
		/// В наследованных классах исполняет отдельно взятую команду (без учёта цепи) с указанным параметром.
		/// </summary>
		/// <param name="parameter">Параметр команды.</param>
		protected abstract void ExecuteThis (object parameter);
	}
}
