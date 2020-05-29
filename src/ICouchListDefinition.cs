namespace Divan
{
	public interface ICouchListDefinition : ICouchViewDefinition, ICouchViewDefinitionBase
	{
		string List { get; set; }

		string ViewName { get; set; }
	}
}
