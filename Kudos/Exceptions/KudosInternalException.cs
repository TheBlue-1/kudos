using System;
using System.Collections.Generic;
using System.Text;

namespace Kudos.Exceptions
{
    public class KudosInternalException:Exception,IKudosException
    {
		public string UserMessage { get; }
		public KudosInternalException(string message) : base(message) => UserMessage = "internal error";

    }
}
