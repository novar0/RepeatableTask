using System;

namespace RepeatableTask.Sample
{
	public class DataRecord
	{
		private static readonly Random _rnd = new Random ();
		private int _id;
		public bool IsMarked { get; set; }
		public string Name { get; set; }
		public int ID { get { return _id; } }

		public DataRecord ()
		{
			_id = _rnd.Next ();
		}
	}
}
