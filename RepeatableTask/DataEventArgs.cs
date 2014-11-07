using System;

namespace BusinessClassLibrary
{
	/// <summary>
	/// Данные о событии.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки, содержащей данные о событии.</typeparam>
	public class DataEventArgs<T> : EventArgs
	{
		private readonly T _data;

		/// <summary>
		/// Инициализирует новый экземпляр класса DataEventArgs&lt;T&gt;.
		/// </summary>
		/// <param name="data">Данные о событии.</param>
		public DataEventArgs (T data)
			: base ()
		{
			_data = data;
		}

		/// <summary>
		/// Получает объект, содержащий данные о событии.
		/// </summary>
		public T Value { get { return _data; } }
	}
}