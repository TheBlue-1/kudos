using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace Kudos.Extensions
{
  public static class SocketUserExtension
    {
	public	static string Mention(this SocketUser user) => $"<@ {user.Id}>";
	}
}
