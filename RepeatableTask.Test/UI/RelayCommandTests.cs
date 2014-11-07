using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessClassLibrary.UI;

namespace BusinessClassLibrary.Test
{
	[TestClass]
	public class RelayCommandTests
	{
		private int _canExecuteCount;
		private int _executeCount;
		[TestMethod]
		[TestCategory ("UI")]
		public void RelayCommand_CallDelegates ()
		{
			_canExecuteCount = 0;
			_executeCount = 0;
			var cmd = new ChainedRelayCommand (Execute1, CanExecute1);
			Assert.AreEqual (0, _canExecuteCount);
			Assert.AreEqual (0, _executeCount);
			Assert.IsTrue (cmd.CanExecute (null));
			Assert.AreEqual (1, _canExecuteCount);
			Assert.AreEqual (0, _executeCount);
			Assert.IsTrue (cmd.CanExecute (null));
			Assert.AreEqual (2, _canExecuteCount);
			Assert.AreEqual (0, _executeCount);
			cmd.Execute (null);
			Assert.AreEqual (2, _canExecuteCount);
			Assert.AreEqual (1, _executeCount);
			cmd.Execute (null);
			Assert.AreEqual (2, _canExecuteCount);
			Assert.AreEqual (2, _executeCount);
		}
		private bool CanExecute1 ()
		{
			_canExecuteCount++;
			return true;
		}
		private void Execute1 ()
		{
			_executeCount++;
		}

		private List<string> _canExecuteCalls;
		private List<string> _executeCalls;
		[TestMethod]
		[TestCategory ("UI")]
		public void RelayCommand_CallDelegatesParam ()
		{
			_canExecuteCalls = new List<string> ();
			_executeCalls = new List<string> ();
			var cmd = new RelayCommand<string> (Execute2, CanExecute2);
			Assert.AreEqual (0, _canExecuteCalls.Count);
			Assert.AreEqual (0, _executeCalls.Count);
			Assert.IsTrue (cmd.CanExecute ("123"));
			Assert.AreEqual (1, _canExecuteCalls.Count);
			Assert.AreEqual ("123", _canExecuteCalls[0]);
			Assert.AreEqual (0, _executeCalls.Count);
			Assert.IsFalse (cmd.CanExecute (null));
			Assert.AreEqual (2, _canExecuteCalls.Count);
			Assert.IsNull (_canExecuteCalls[1]);
			Assert.AreEqual (0, _executeCalls.Count);
			cmd.Execute ("abc");
			Assert.AreEqual (2, _canExecuteCalls.Count);
			Assert.AreEqual (1, _executeCalls.Count);
			Assert.AreEqual ("abc", _executeCalls[0]);
			cmd.Execute ("01234ABC");
			Assert.AreEqual (2, _canExecuteCalls.Count);
			Assert.AreEqual (2, _executeCalls.Count);
			Assert.AreEqual ("01234ABC", _executeCalls[1]);
		}
		private bool CanExecute2 (string parameter)
		{
			_canExecuteCalls.Add (parameter);
			return parameter != null;
		}
		private void Execute2 (string parameter)
		{
			_executeCalls.Add (parameter);
		}
	}
}
