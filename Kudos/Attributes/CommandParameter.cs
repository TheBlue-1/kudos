namespace Kudos.Attributes
{
	[System.AttributeUsage(System.AttributeTargets.Parameter)]
	public class CommandParameter : System.Attribute
	{
		public string Name { get; }
		//type,min,max usw
		public CommandParameter(string name) => Name = name;
	}
}
