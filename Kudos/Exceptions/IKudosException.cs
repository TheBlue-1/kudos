namespace Kudos.Exceptions {
	internal interface IKudosException {
		string Message { get; }
		string UserMessage { get; }
	}
}
