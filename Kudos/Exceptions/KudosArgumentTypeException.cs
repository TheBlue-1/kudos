namespace Kudos.Exceptions {
	internal class KudosArgumentTypeException : KudosArgumentException {
		public KudosArgumentTypeException(string userMessage, string message = null) : base(userMessage, message) { }
	}
}
