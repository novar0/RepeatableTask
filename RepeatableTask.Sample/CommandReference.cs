using System;
using System.Windows;
using System.Windows.Input;

namespace RepeatableTask.Sample
{
	/// <summary>
	/// Ссылка на команду.
	/// </summary>
	/// <remarks>
	/// This class facilitates associating a key binding in XAML markup to a command
	/// defined in a View Model by exposing a Command dependency property.
	/// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
	/// </remarks>
	public class CommandReference : Freezable,
		ICommand
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса CommandReference.
		/// </summary>
		public CommandReference ()
		{
		}

		/// <summary>
		/// Команда, хранящаяся в ссылке.
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register (
			"Command",
			typeof (ICommand),
			typeof (CommandReference),
			new PropertyMetadata (OnCommandChanged));

		/// <summary>
		/// Получает или устанавливает команду, хранящуюся в ссылке.
		/// </summary>
		public ICommand Command
		{
			get { return (ICommand)GetValue (CommandProperty); }
			set { SetValue (CommandProperty, value); }
		}

		#region ICommand Members

		/// <summary>Определяет метод, который определяет, может ли данная команда выполняться в ее текущем состоянии.</summary>
		/// <param name="parameter">Данные, используемые данной командой.</param>
		/// <returns>Значение true, если команда может быть выполнена; в противном случае — значение false.</returns>
		public bool CanExecute (object parameter)
		{
			var cmd = this.Command;
			return (cmd != null) && cmd.CanExecute (parameter);
		}

		/// <summary>Определяет метод, вызываемый при вызове данной команды.</summary>
		/// <param name="parameter">Данные, используемые данной командой.</param>
		public void Execute (object parameter)
		{
			this.Command.Execute (parameter);
		}

		private EventHandler _commandCanExecuteChangedHandler;

		/// <summary>Происходит при изменениях, влияющих на то, должна ли выполняться данная команда.</summary>
		public event EventHandler CanExecuteChanged;

		private static void OnCommandChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var commandReference = d as CommandReference;
			var oldCommand = e.OldValue as ICommand;
			var newCommand = e.NewValue as ICommand;

			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= commandReference._commandCanExecuteChangedHandler;
			}
			if (newCommand != null)
			{
				commandReference._commandCanExecuteChangedHandler = commandReference.CommandCanExecuteChanged;
				newCommand.CanExecuteChanged += commandReference._commandCanExecuteChangedHandler;
			}
		}

		private void CommandCanExecuteChanged (object sender, EventArgs e)
		{
			var handler = this.CanExecuteChanged;
			if (handler != null)
			{
				handler.Invoke (this, e);
			}
		}

		#endregion

		/// <summary>Не реализовано.</summary>
		protected override Freezable CreateInstanceCore ()
		{
			throw new NotImplementedException ();
		}
	}
}
