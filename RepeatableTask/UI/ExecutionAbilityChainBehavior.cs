
namespace BusinessClassLibrary.UI
{
	/// <summary>
	/// Поведение при запросе готовности выполнения команды, являющейся звеном в цепи связанных команд.
	/// </summary>
	public enum ExecutionAbilityChainBehavior
	{
		/// <summary>Готовность команды определяется только самой командой без учёта цепи.</summary>
		WhenThis,

		/// <summary>Готовность команды наступает только когда готовы все связанные команды в цепи.</summary>
		WhenAll,

		/// <summary>Готовность команды наступает когда готова любая из связанных команд в цепи.</summary>
		WhenAny
	}
}
