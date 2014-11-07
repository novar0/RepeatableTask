
namespace BusinessClassLibrary
{
	/// <summary>
	/// Узел односвязного списка.
	/// </summary>
	/// <typeparam name="TItem">Тип значений элементов списка.</typeparam>
	/// <remarks>Значение null является корректным и означает пустой список.</remarks>
	public class SingleLinkedListNode<TItem>
	{
		private readonly TItem _value;
		private readonly SingleLinkedListNode<TItem> _next;

		/// <summary>
		/// Получает значение узла.
		/// </summary>
		public TItem Value { get { return _value; } }

		/// <summary>
		/// Получает следующий узел списка.
		/// </summary>
		public SingleLinkedListNode<TItem> Next { get { return _next; } }

		internal SingleLinkedListNode (TItem value, SingleLinkedListNode<TItem> next)
		{
			_value = value;
			_next = next;
		}
	}
}
