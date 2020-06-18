namespace Kudos.Attributes {
	[System.AttributeUsage(System.AttributeTargets.Class)
	]
	public class CommandModule : System.Attribute
	{
		public string Name { get; }

		public CommandModule(string name) => Name = name;
	}
}
