using System;

namespace BusinessClassLibrary.Tasks
{
	/// <summary>
	/// Содержит данные запускаемой задачи.
	/// </summary>
	public class TaskStartingEventArgs : EventArgs
	{
		/// <summary>
		/// Получает или устанавливает объект-состояние задачи.
		/// </summary>
		public object State { get; set; }

		/// <summary>
		/// Получает или устанавливает необходимость отмены запускаемой задачи.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Инициализирует новый экземпляр TaskStartingEventArgs на основе указанного объекта-состояния задачи.
		/// </summary>
		/// <param name="state">Объект-состояния задачи.</param>
		public TaskStartingEventArgs (object state)
		{
			State = state;
		}
	}
}
