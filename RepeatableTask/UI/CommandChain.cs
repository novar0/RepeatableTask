using System;
using System.Diagnostics.Contracts;

namespace BusinessClassLibrary.UI
{
	/// <summary>
	/// Цепь связанных команд.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "This collection is more chain than collection.")]
	public class CommandChain
	{
		private readonly bool _executionChained;
		private readonly ExecutionAbilityChainBehavior _canExecuteChainBehavior;
		private SingleLinkedListNode<ChainedCommandBase> _firstCommand = null;

		/// <summary>
		/// Получает признак выполнения всех команд цепи при выполнении одной.
		/// </summary>
		public bool ExecutionChained { get { return _executionChained; } }

		/// <summary>
		/// Получает поведение при запросе готовности выполнения команды связанное с другими командами цепи.
		/// </summary>
		public ExecutionAbilityChainBehavior ExecutionAbilityChainBehavior { get { return _canExecuteChainBehavior; } }

		/// <summary>
		/// Получает начальный узел односвязного списка команд цепи.
		/// </summary>
		public SingleLinkedListNode<ChainedCommandBase> FirstCommand { get { return _firstCommand; } }

		/// <summary>
		/// Инициализирует новый экземпляр класса RelayCommandChain
		/// с указанным поведением в цепи связанных команд.
		/// </summary>
		/// <param name="executionChained">Признак выполнения всех команд цепи при выполнении одной.</param>
		/// <param name="canExecuteChainBehavior">Поведение при запросе готовности выполнения команды связанное с другими командами цепи.</param>
		public CommandChain (bool executionChained, ExecutionAbilityChainBehavior canExecuteChainBehavior)
		{
			_executionChained = executionChained;
			_canExecuteChainBehavior = canExecuteChainBehavior;
		}

		/// <summary>Очищает цепь.</summary>
		public void Clear ()
		{
			_firstCommand = null;
		}

		/// <summary>
		/// Добавляет указанную команду в цепь.
		/// </summary>
		/// <param name="command">Команда для добавления в цепь.</param>
		public void Add (ChainedCommandBase command)
		{
			if (command == null)
			{
				throw new ArgumentNullException ("command");
			}
			Contract.EndContractBlock ();

			_firstCommand = new SingleLinkedListNode<ChainedCommandBase> (command, _firstCommand);
		}
	}
}
