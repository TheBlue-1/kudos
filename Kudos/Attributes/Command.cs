namespace Kudos.Attributes
{
	[System.AttributeUsage(System.AttributeTargets.Method)
	]
	public class Command : System.Attribute
	{
		public string Name { get; }

		public string Help { get; }

		public Command(string name) => Name = name;
	}
}
