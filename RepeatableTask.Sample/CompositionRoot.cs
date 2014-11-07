namespace RepeatableTask.Sample
{
	public interface IConfirmationView
	{
		bool? RequestConfirmation ();
	}

	public static class CompositionRoot
	{
		[System.STAThreadAttribute ()]
		public static int Main (string[] args)
		{
			var businessLogicService = new BusinessLogic ();
			object appViewModel = new AppViewModel (
				businessLogicService,
				() => new DataRecord (),
				() => (IConfirmationView)new ConfirmationView ("Вы уверены?", "Важный вопрос"));
			var view = new AppView (appViewModel);

			return new System.Windows.Application ().Run (view);
		}
	}
}
