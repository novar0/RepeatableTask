using System.Windows;

namespace RepeatableTask.Sample
{
	public class ConfirmationView : IConfirmationView
	{
		private readonly string _question;
		private readonly string _title;

		public ConfirmationView (string question, string title)
		{
			_question = question;
			_title = title;
		}

		public bool? RequestConfirmation ()
		{
			switch (MessageBox.Show (_question, _title, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					return true;
				case MessageBoxResult.No:
					return false;
			}
			return null;
		}
	}
}
