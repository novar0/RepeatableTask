
namespace RepeatableTask.Sample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class AppView : System.Windows.Window
	{
		public AppView (object viewModel)
		{
			this.DataContext = viewModel;
			InitializeComponent ();
		}
	}
}
