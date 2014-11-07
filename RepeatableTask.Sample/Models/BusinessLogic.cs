using System;
using System.Threading;

namespace RepeatableTask.Sample
{
	public class BusinessLogic
	{
		public BusinessLogic ()
		{
		}

		public void Work1 (object state, CancellationToken cancellationToken)
		{
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (2);
			}
		}

		public void Work2 (object state, CancellationToken cancellationToken)
		{
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (3);
			}
		}

		public void Work3 (object state, CancellationToken cancellationToken)
		{
			for (int i = 0; i < 5; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (3000);
			}
		}

		public void LoadData (object state, CancellationToken cancellationToken)
		{
			for (int i = 0; i < 1000; i++)
			{
				cancellationToken.ThrowIfCancellationRequested ();
				System.Threading.Thread.Sleep (5);
			}
			throw new ApplicationException ("Имитация исключительной ситуации");
		}
	}
}
