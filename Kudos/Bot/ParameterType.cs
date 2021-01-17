#region
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Bot {
	public abstract class ParameterType {
		private const string Emojis =
			"😀😃😄😁😆😅😂🤣☺️😊😇🙂🙃😉😌😍🥰😘😗😙😚😋😛😝😜🤪🤨🧐🤓😎🤩🥳😏😒😞😔😟😕🙁☹️😣😖😫😩🥺😢😭😤😠😡🤬🤯😳🥵🥶😱😨😰😥😓🤗🤔🤭🤫🤥😶😐😑😬🙄😯😦😧😮😲🥱😴🤤😪😵🤐🥴🤢🤮🤧😷🤒🤕🤑🤠😈👿👹👺🤡💩👻💀☠️👽"
			+ "👾🤖🎃😺😸😹😻😼😽🙀😿😾👋🤚🖐✋🖖👌🤏✌️🤞🤟🤘🤙👈👉👆🖕👇☝️👍👎✊👊🤛🤜👏🙌👐🤲🤝🙏✍️💅🤳💪🦾🦵🦿🦶👂🦻👃🧠🦷🦴👀👁👅👄💋🩸👶🧒👦👧🧑👱👨🧔👨‍🦰👨‍🦱👨‍🦳👨‍🦲👩👩‍🦰🧑‍🦰👩‍🦱🧑‍🦱👩‍🦳🧑‍🦳👩‍🦲🧑‍🦲"
			+ "👱‍♀️👱‍♂️🧓👴👵🙍🙍‍♂️🙍‍♀️🙎🙎‍♂️🙎‍♀️🙅🙅‍♂️🙅‍♀️🙆🙆‍♂️🙆‍♀️💁💁‍♂️💁‍♀️🙋🙋‍♂️🙋‍♀️🧏🧏‍♂️🧏‍♀️🙇🙇‍♂️🙇‍♀️🤦🤦‍♂️🤦‍♀️🤷🤷‍♂️🤷‍♀️🧑‍⚕️👨‍⚕️👩‍⚕️🧑‍🎓👨‍🎓👩‍🎓🧑‍🏫👨‍🏫👩‍🏫🧑‍⚖️👨‍⚖️👩‍⚖️🧑‍�"
			+ "�👨‍🌾👩‍🌾🧑‍🍳👨‍🍳👩‍🍳🧑‍🔧👨‍🔧👩‍🔧🧑‍🏭👨‍🏭👩‍🏭🧑‍💼👨‍💼👩‍💼🧑‍🔬👨‍🔬👩‍🔬🧑‍💻👨‍💻👩‍💻🧑‍🎤👨‍🎤👩‍🎤🧑‍🎨👨‍🎨👩‍🎨🧑‍✈️👨‍✈️👩‍✈️🧑‍🚀👨‍🚀👩‍🚀🧑‍🚒👨‍🚒👩‍🚒👮👮‍♂️👮‍♀️🕵🕵️‍♂️🕵️‍"
			+ "♀️💂💂‍♂️💂‍♀️👷👷‍♂️👷‍♀️🤴👸👳👳‍♂️👳‍♀️👲🧕🤵👰🤰🤱👼🎅🤶🦸🦸‍♂️🦸‍♀️🦹🦹‍♂️🦹‍♀️🧙🧙‍♂️🧙‍♀️🧚🧚‍♂️🧚‍♀️🧛🧛‍♂️🧛‍♀️🧜🧜‍♂️🧜‍♀️🧝🧝‍♂️🧝‍♀️🧞🧞‍♂️🧞‍♀️🧟🧟‍♂️🧟‍♀️💆💆‍♂️💆‍♀️💇💇‍♂️💇‍♀️🚶🚶‍♂️�"
			+ "�‍♀️🧍🧍‍♂️🧍‍♀️🧎🧎‍♂️🧎‍♀️🧑‍🦯👨‍🦯👩‍🦯🧑‍🦼👨‍🦼👩‍🦼🧑‍🦽👨‍🦽👩‍🦽🏃🏃‍♂️🏃‍♀️💃🕺🕴👯👯‍♂️👯‍♀️🧖🧖‍♂️🧖‍♀️🧘🧑‍🤝‍🧑👭👫👬💏👨‍❤️‍💋‍👨👩‍❤️‍💋‍👩💑👨‍❤️‍👨👩‍❤️‍👩👪👨‍👩‍👦👨‍👩‍👧👨‍👩‍👧‍"
			+ "👦👨‍👩‍👦‍👦👨‍👩‍👧‍👧👨‍👨‍👦👨‍👨‍👧👨‍👨‍👧‍👦👨‍👨‍👦‍👦👨‍👨‍👧‍👧👩‍👩‍👦👩‍👩‍👧👩‍👩‍👧‍👦👩‍👩‍👦‍👦👩‍👩‍👧‍👧👨‍👦👨‍👦‍👦👨‍👧👨‍👧‍👦👨‍👧‍👧👩‍👦👩‍👦‍👦👩‍👧👩‍👧‍👦👩‍👧‍👧🗣👤👥👣🧳"
			+ "🌂☂️🧵🧶👓🕶🥽🥼🦺👔👕👖🧣🧤🧥🧦👗👘🥻🩱🩲🩳👙👚👛👜👝🎒👞👟🥾🥿👠👡🩰👢👑👒🎩🎓🧢⛑💄💍💼👋🏻🤚🏻🖐🏻✋🏻🖖🏻👌🏻🤏🏻✌🏻🤞🏻🤟🏻🤘🏻🤙🏻👈🏻👉🏻👆🏻🖕🏻👇🏻☝🏻👍🏻👎🏻✊🏻👊🏻🤛🏻🤜🏻👏🏻🙌🏻👐🏻🤲🏻🙏�"
			+ "�✍🏻💅🏻🤳🏻💪🏻🦵🏻🦶🏻👂🏻🦻🏻👃🏻👶🏻🧒🏻👦🏻👧🏻🧑🏻👨🏻👩🏻🧑🏻‍🦱👨🏻‍🦱👩🏻‍🦱🧑🏻‍🦰👨🏻‍🦰👩🏻‍🦰👱🏻👱🏻‍♂️👱🏻‍♀️🧑🏻‍🦳👩🏻‍🦳👨🏻‍🦳🧑🏻‍🦲👨🏻‍🦲👩🏻‍🦲🧔🏻🧓🏻👴🏻👵🏻🙍🏻🙍🏻‍♂️🙍🏻‍♀️"
			+ "🙎🏻🙎🏻‍♂️🙎🏻‍♀️🙅🏻🙅🏻‍♂️🙅🏻‍♀️🙆🏻🙆🏻‍♂️🙆🏻‍♀️💁🏻💁🏻‍♂️💁🏻‍♀️🙋🏻🙋🏻‍♂️🙋🏻‍♀️🧏🏻🧏🏻‍♂️🧏🏻‍♀️🙇🏻🙇🏻‍♂️🙇🏻‍♀️🤦🏻🤦🏻‍♂️🤦🏻‍♀️🤷🏻🤷🏻‍♂️🤷🏻‍♀️🧑🏻‍⚕️👨🏻‍⚕️👩🏻‍⚕️🧑🏻‍🎓👨🏻‍🎓👩�"
			+ "�‍🎓🧑🏻‍🏫👨🏻‍🏫👩🏻‍🏫🧑🏻‍⚖️👨🏻‍⚖️👩🏻‍⚖️🧑🏻‍🌾👨🏻‍🌾👩🏻‍🌾🧑🏻‍🍳👨🏻‍🍳👩🏻‍🍳🧑🏻‍🔧👨🏻‍🔧👩🏻‍🔧🧑🏻‍🏭👨🏻‍🏭👩🏻‍🏭🧑🏻‍💼👨🏻‍💼👩🏻‍💼🧑🏻‍🔬👨🏻‍🔬👩🏻‍🔬🧑🏻‍💻👨🏻‍💻👩🏻‍💻🧑🏻‍🎤"
			+ "👨🏻‍🎤👩🏻‍🎤🧑🏻‍🎨👨🏻‍🎨👩🏻‍🎨🧑🏻‍✈️👨🏻‍✈️👩🏻‍✈️🧑🏻‍🚀👨🏻‍🚀👩🏻‍🚀🧑🏻‍🚒👨🏻‍🚒👩🏻‍🚒👮🏻👮🏻‍♂️👮🏻‍♀️🕵🏻🕵🏻‍♂️🕵🏻‍♀️💂🏻💂🏻‍♂️💂🏻‍♀️👷🏻👷🏻‍♂️👷🏻‍♀️🤴🏻👸🏻👳🏻👳🏻‍♂️👳🏻‍♀️👲🏻"
			+ "🧕🏻🤵🏻👰🏻🤰🏻🤱🏻👼🏻🎅🏻🤶🏻🦸🏻🦸🏻‍♂️🦸🏻‍♀️🦹🏻🦹🏻‍♂️🦹🏻‍♀️🧙🏻🧙🏻‍♂️🧙🏻‍♀️🧚🏻🧚🏻‍♂️🧚🏻‍♀️🧛🏻🧛🏻‍♂️🧛🏻‍♀️🧜🏻🧜🏻‍♂️🧜🏻‍♀️🧝🏻🧝🏻‍♂️🧝🏻‍♀️💆🏻💆🏻‍♂️💆🏻‍♀️💇🏻💇🏻‍♂️💇🏻‍♀️🚶🏻🚶"
			+ "🏻‍♂️🚶🏻‍♀️🧍🏻🧍🏻‍♂️🧍🏻‍♀️🧎🏻🧎🏻‍♂️🧎🏻‍♀️🧑🏻‍🦯👨🏻‍🦯👩🏻‍🦯🧑🏻‍🦼👨🏻‍🦼👩🏻‍🦼🧑🏻‍🦽👨🏻‍🦽👩🏻‍🦽🏃🏻🏃🏻‍♂️🏃🏻‍♀️💃🏻🕺🏻🕴🏻🧖🏻🧖🏻‍♂️🧖🏻‍♀️🧗🏻🧗🏻‍♂️🧗🏻‍♀️🏇🏻🏂🏻🏌🏻🏌🏻‍♂️🏌🏻"
			+ "‍♀️🏄🏻🏄🏻‍♂️🏄🏻‍♀️🚣🏻🚣🏻‍♂️🚣🏻‍♀️🏊🏻🏊🏻‍♂️🏊🏻‍♀️⛹🏻⛹🏻‍♂️⛹🏻‍♀️🏋🏻🏋🏻‍♂️🏋🏻‍♀️🚴🏻🚴🏻‍♂️🚴🏻‍♀️🚵🏻🚵🏻‍♂️🚵🏻‍♀️🤸🏻🤸🏻‍♂️🤸🏻‍♀️🤽🏻🤽🏻‍♂️🤽🏻‍♀️🤾🏻🤾🏻‍♂️🤾🏻‍♀️🤹🏻🤹🏻‍♂️🤹🏻‍♀️🧘"
			+ "🏻🧘🏻‍♂️🧘🏻‍♀️🛀🏻🛌🏻🧑🏻‍🤝‍🧑🏻👬🏻👭🏻👫🏻👋🏼🤚🏼🖐🏼✋🏼🖖🏼👌🏼🤏🏼✌🏼🤞🏼🤟🏼🤘🏼🤙🏼👈🏼👉🏼👆🏼🖕🏼👇🏼☝🏼👍🏼👎🏼✊🏼👊🏼🤛🏼🤜🏼👏🏼🙌🏼👐🏼🤲🏼🙏🏼✍🏼💅🏼🤳🏼💪🏼🦵🏼🦶🏼👂🏼🦻🏼👃🏼👶🏼�"
			+ "�🏼👦🏼👧🏼🧑🏼👨🏼👩🏼🧑🏼‍🦱👨🏼‍🦱👩🏼‍🦱🧑🏼‍🦰👨🏼‍🦰👩🏼‍🦰👱🏼👱🏼‍♂️👱🏼‍♀️🧑🏼‍🦳👨🏼‍🦳👩🏼‍🦳🧑🏼‍🦲👨🏼‍🦲👩🏼‍🦲🧔🏼🧓🏼👴🏼👵🏼🙍🏼🙍🏼‍♂️🙍🏼‍♀️🙎🏼🙎🏼‍♂️🙎🏼‍♀️🙅🏼🙅🏼‍♂️🙅🏼‍♀️🙆🏼�"
			+ "�🏼‍♂️🙆🏼‍♀️💁🏼💁🏼‍♂️💁🏼‍♀️🙋🏼🙋🏼‍♂️🙋🏼‍♀️🧏🏼🧏🏼‍♂️🧏🏼‍♀️🙇🏼🙇🏼‍♂️🙇🏼‍♀️🤦🏼🤦🏼‍♂️🤦🏼‍♀️🤷🏼🤷🏼‍♂️🤷🏼‍♀️🧑🏼‍⚕️👨🏼‍⚕️👩🏼‍⚕️🧑🏼‍🎓👨🏼‍🎓👩🏼‍🎓🧑🏼‍🏫👨🏼‍🏫👩🏼‍🏫🧑🏼‍⚖️👨🏼‍⚖️👩"
			+ "🏼‍⚖️🧑🏼‍🌾👨🏼‍🌾👩🏼‍🌾🧑🏼‍🍳👨🏼‍🍳👩🏼‍🍳🧑🏼‍🔧👨🏼‍🔧👩🏼‍🔧🧑🏼‍🏭👨🏼‍🏭👩🏼‍🏭🧑🏼‍💼👨🏼‍💼👩🏼‍💼🧑🏼‍🔬👨🏼‍🔬👩🏼‍🔬🧑🏼‍💻👨🏼‍💻👩🏼‍💻🧑🏼‍🎤👨🏼‍🎤👩🏼‍🎤🧑🏼‍🎨👨🏼‍🎨👩🏼‍🎨🧑🏼‍✈"
			+ "️👨🏼‍✈️👩🏼‍✈️🧑🏼‍🚀👨🏼‍🚀👩🏼‍🚀🧑🏼‍🚒👨🏼‍🚒👩🏼‍🚒👮🏼👮🏼‍♂️👮🏼‍♀️🕵🏼🕵🏼‍♂️🕵🏼‍♀️💂🏼💂🏼‍♂️💂🏼‍♀️👷🏼👷🏼‍♂️👷🏼‍♀️🤴🏼👸🏼👳🏼👳🏼‍♂️👳🏼‍♀️👲🏼🧕🏼🤵🏼👰🏼🤰🏼🤱🏼👼🏼🎅🏼🤶🏼🦸🏼🦸🏼‍"
			+ "♂️🦸🏼‍♀️🦹🏼🦹🏼‍♂️🦹🏼‍♀️🧙🏼🧙🏼‍♂️🧙🏼‍♀️🧚🏼🧚🏼‍♂️🧚🏼‍♀️🧛🏼🧛🏼‍♂️🧛🏼‍♀️🧜🏼🧜🏼‍♂️🧜🏼‍♀️🧝🏼🧝🏼‍♂️🧝🏼‍♀️💆🏼💆🏼‍♂️💆🏼‍♀️💇🏼💇🏼‍♂️💇🏼‍♀️🚶🏼🚶🏼‍♂️🚶🏼‍♀️🧍🏼🧍🏼‍♂️🧍🏼‍♀️🧎🏼🧎🏼‍♂️"
			+ "🧎🏼‍♀️🧑🏼‍🦯👨🏼‍🦯👩🏼‍🦯🧑🏼‍🦼👨🏼‍🦼👩🏼‍🦼🧑🏼‍🦽👨🏼‍🦽👩🏼‍🦽🏃🏼🏃🏼‍♂️🏃🏼‍♀️💃🏼🕺🏼🕴🏼🧖🏼🧖🏼‍♂️🧖🏼‍♀️🧗🏼🧗🏼‍♂️🧗🏼‍♀️🏇🏼🏂🏼🏌🏼🏌🏼‍♂️🏌🏼‍♀️🏄🏼🏄🏼‍♂️🏄🏼‍♀️🚣🏼🚣🏼‍♂️🚣🏼‍♀️🏊"
			+ "🏼🏊🏼‍♂️🏊🏼‍♀️⛹🏼⛹🏼‍♂️⛹🏼‍♀️🏋🏼🏋🏼‍♂️🏋🏼‍♀️🚴🏼🚴🏼‍♂️🚴🏼‍♀️🚵🏼🚵🏼‍♂️🚵🏼‍♀️🤸🏼🤸🏼‍♂️🤸🏼‍♀️🤽🏼🤽🏼‍♂️🤽🏼‍♀️🤾🏼🤾🏼‍♂️🤾🏼‍♀️🤹🏼🤹🏼‍♂️🤹🏼‍♀️🧘🏼🧘🏼‍♂️🧘🏼‍♀️🛀🏼🛌🏼🧑🏼‍🤝‍🧑🏼👬🏼�"
			+ "�🏼👫🏼👋🏽🤚🏽🖐🏽✋🏽🖖🏽👌🏽🤏🏽✌🏽🤞🏽🤟🏽🤘🏽🤙🏽👈🏽👉🏽👆🏽🖕🏽👇🏽☝🏽👍🏽👎🏽✊🏽👊🏽🤛🏽🤜🏽👏🏽🙌🏽👐🏽🤲🏽🙏🏽✍🏽💅🏽🤳🏽💪🏽🦵🏽🦶🏽👂🏽🦻🏽👃🏽👶🏽🧒🏽👦🏽👧🏽🧑🏽👨🏽👩🏽🧑🏽‍🦱👨🏽‍🦱👩🏽"
			+ "‍🦱🧑🏽‍🦰👨🏽‍🦰👩🏽‍🦰👱🏽👱🏽‍♂️👱🏽‍♀️🧑🏽‍🦳👨🏽‍🦳👩🏽‍🦳🧑🏽‍🦲👨🏽‍🦲👩🏽‍🦲🧔🏽🧓🏽👴🏽👵🏽🙍🏽🙍🏽‍♂️🙍🏽‍♀️🙎🏽🙎🏽‍♂️🙎🏽‍♀️🙅🏽🙅🏽‍♂️🙅🏽‍♀️🙆🏽🙆🏽‍♂️🙆🏽‍♀️💁🏽💁🏽‍♂️💁🏽‍♀️🙋🏽🙋🏽‍♂"
			+ "️🙋🏽‍♀️🧏🏽🧏🏽‍♂️🧏🏽‍♀️🙇🏽🙇🏽‍♂️🙇🏽‍♀️🤦🏽🤦🏽‍♂️🤦🏽‍♀️🤷🏽🤷🏽‍♂️🤷🏽‍♀️🧑🏽‍⚕️👨🏽‍⚕️👩🏽‍⚕️🧑🏽‍🎓👨🏽‍🎓👩🏽‍🎓🧑🏽‍🏫👨🏽‍🏫👩🏽‍🏫🧑🏽‍⚖️👨🏽‍⚖️👩🏽‍⚖️🧑🏽‍🌾👨🏽‍🌾👩🏽‍🌾🧑🏽‍🍳👨🏽‍🍳�"
			+ "�🏽‍🍳🧑🏽‍🔧👨🏽‍🔧👩🏽‍🔧🧑🏽‍🏭👨🏽‍🏭👩🏽‍🏭🧑🏽‍💼👨🏽‍💼👩🏽‍💼🧑🏽‍🔬👨🏽‍🔬👩🏽‍🔬🧑🏽‍💻👨🏽‍💻👩🏽‍💻🧑🏽‍🎤👨🏽‍🎤👩🏽‍🎤🧑🏽‍🎨👨🏽‍🎨👩🏽‍🎨🧑🏽‍✈️👨🏽‍✈️👩🏽‍✈️🧑🏽‍🚀👨🏽‍🚀👩🏽‍🚀🧑🏽‍"
			+ "🚒👨🏽‍🚒👩🏽‍🚒👮🏽👮🏽‍♂️👮🏽‍♀️🕵🏽🕵🏽‍♂️🕵🏽‍♀️💂🏽💂🏽‍♂️💂🏽‍♀️👷🏽👷🏽‍♂️👷🏽‍♀️🤴🏽👸🏽👳🏽👳🏽‍♂️👳🏽‍♀️👲🏽🧕🏽🤵🏽👰🏽🤰🏽🤱🏽👼🏽🎅🏽🤶🏽🦸🏽🦸🏽‍♂️🦸🏽‍♀️🦹🏽🦹🏽‍♂️🦹🏽‍♀️🧙🏽🧙🏽‍♂️🧙�"
			+ "�‍♀️🧚🏽🧚🏽‍♂️🧚🏽‍♀️🧛🏽🧛🏽‍♂️🧛🏽‍♀️🧜🏽🧜🏽‍♂️🧜🏽‍♀️🧝🏽🧝🏽‍♂️🧝🏽‍♀️💆🏽💆🏽‍♂️💆🏽‍♀️💇🏽💇🏽‍♂️💇🏽‍♀️🚶🏽🚶🏽‍♂️🚶🏽‍♀️🧍🏽🧍🏽‍♂️🧍🏽‍♀️🧎🏽🧎🏽‍♂️🧎🏽‍♀️🧑🏽‍🦯👨🏽‍🦯👩🏽‍🦯🧑🏽‍🦼👨🏽‍�"
			+ "�👩🏽‍🦼🧑🏽‍🦽👨🏽‍🦽👩🏽‍🦽🏃🏽🏃🏽‍♂️🏃🏽‍♀️💃🏽🕺🏽🕴🏽🧖🏽🧖🏽‍♂️🧖🏽‍♀️🧗🏽🧗🏽‍♂️🧗🏽‍♀️🏇🏽🏂🏽🏌🏽🏌🏽‍♂️🏌🏽‍♀️🏄🏽🏄🏽‍♂️🏄🏽‍♀️🚣🏽🚣🏽‍♂️🚣🏽‍♀️🏊🏽🏊🏽‍♂️🏊🏽‍♀️⛹🏽⛹🏽‍♂️⛹🏽‍♀️🏋🏽🏋🏽‍♂"
			+ "️🏋🏽‍♀️🚴🏽🚴🏽‍♂️🚴🏽‍♀️🚵🏽🚵🏽‍♂️🚵🏽‍♀️🤸🏽🤸🏽‍♂️🤸🏽‍♀️🤽🏽🤽🏽‍♂️🤽🏽‍♀️🤾🏽🤾🏽‍♂️🤾🏽‍♀️🤹🏽🤹🏽‍♂️🤹🏽‍♀️🧘🏽🧘🏽‍♂️🧘🏽‍♀️🛀🏽🛌🏽🧑🏽‍🤝‍🧑🏽👬🏽👭🏽👫🏽👋🏾🤚🏾🖐🏾✋🏾🖖🏾👌🏾🤏🏾✌🏾🤞🏾"
			+ "🤟🏾🤘🏾🤙🏾👈🏾👉🏾👆🏾🖕🏾👇🏾☝🏾👍🏾👎🏾✊🏾👊🏾🤛🏾🤜🏾👏🏾🙌🏾👐🏾🤲🏾🙏🏾✍🏾💅🏾🤳🏾💪🏾🦵🏾🦶🏾👂🏾🦻🏾👃🏾👶🏾🧒🏾👦🏾👧🏾🧑🏾👨🏾👩🏾🧑🏾‍🦱👨🏾‍🦱👩🏾‍🦱🧑🏾‍🦰👨🏾‍🦰👩🏾‍🦰👱🏾👱🏾‍♂️👱🏾‍♀"
			+ "️🧑🏾‍🦳👨🏾‍🦳👩🏾‍🦳🧑🏾‍🦲👨🏾‍🦲👩🏾‍🦲🧔🏾🧓🏾👴🏾👵🏾🙍🏾🙍🏾‍♂️🙍🏾‍♀️🙎🏾🙎🏾‍♂️🙎🏾‍♀️🙅🏾🙅🏾‍♂️🙅🏾‍♀️🙆🏾🙆🏾‍♂️🙆🏾‍♀️💁🏾💁🏾‍♂️💁🏾‍♀️🙋🏾🙋🏾‍♂️🙋🏾‍♀️🧏🏾🧏🏾‍♂️🧏🏾‍♀️🙇🏾🙇🏾‍♂️🙇🏾"
			+ "‍♀️🤦🏾🤦🏾‍♂️🤦🏾‍♀️🤷🏾🤷🏾‍♂️🤷🏾‍♀️🧑🏾‍⚕️👨🏾‍⚕️👩🏾‍⚕️🧑🏾‍🎓👨🏾‍🎓👩🏾‍🎓🧑🏾‍🏫👨🏾‍🏫👩🏾‍🏫🧑🏾‍⚖️👨🏾‍⚖️👩🏾‍⚖️🧑🏾‍🌾👨🏾‍🌾👩🏾‍🌾🧑🏾‍🍳👨🏾‍🍳👩🏾‍🍳🧑🏾‍🔧👨🏾‍🔧👩🏾‍🔧🧑🏾‍🏭👨🏾‍🏭"
			+ "👩🏾‍🏭🧑🏾‍💼👨🏾‍💼👩🏾‍💼🧑🏾‍🔬👨🏾‍🔬👩🏾‍🔬🧑🏾‍💻👨🏾‍💻👩🏾‍💻🧑🏾‍🎤👨🏾‍🎤👩🏾‍🎤🧑🏾‍🎨👨🏾‍🎨👩🏾‍🎨🧑🏾‍✈️👨🏾‍✈️👩🏾‍✈️🧑🏾‍🚀👨🏾‍🚀👩🏾‍🚀🧑🏾‍🚒👨🏾‍🚒👩🏾‍🚒👮🏾👮🏾‍♂️👮🏾‍♀️🕵🏾🕵�"
			+ "�‍♂️🕵🏾‍♀️💂🏾💂🏾‍♂️💂🏾‍♀️👷🏾👷🏾‍♂️👷🏾‍♀️🤴🏾👸🏾👳🏾👳🏾‍♂️👳🏾‍♀️👲🏾🧕🏾🤵🏾👰🏾🤰🏾🤱🏾👼🏾🎅🏾🤶🏾🦸🏾🦸🏾‍♂️🦸🏾‍♀️🦹🏾🦹🏾‍♂️🦹🏾‍♀️🧙🏾🧙🏾‍♂️🧙🏾‍♀️🧚🏾🧚🏾‍♂️🧚🏾‍♀️🧛🏾🧛🏾‍♂️🧛🏾‍♀️�"
			+ "�🏾🧜🏾‍♂️🧜🏾‍♀️🧝🏾🧝🏾‍♂️🧝🏾‍♀️💆🏾💆🏾‍♂️💆🏾‍♀️💇🏾💇🏾‍♂️💇🏾‍♀️🚶🏾🚶🏾‍♂️🚶🏾‍♀️🧍🏾🧍🏾‍♂️🧍🏾‍♀️🧎🏾🧎🏾‍♂️🧎🏾‍♀️🧑🏾‍🦯👨🏾‍🦯👩🏾‍🦯🧑🏾‍🦼👨🏾‍🦼👩🏾‍🦼🧑🏾‍🦽👨🏾‍🦽👩🏾‍🦽🏃🏾🏃🏾‍♂️�"
			+ "�🏾‍♀️💃🏾🕺🏾🕴🏾🧖🏾🧖🏾‍♂️🧖🏾‍♀️🧗🏾🧗🏾‍♂️🧗🏾‍♀️🏇🏾🏂🏾🏌🏾🏌🏾‍♂️🏌🏾‍♀️🏄🏾🏄🏾‍♂️🏄🏾‍♀️🚣🏾🚣🏾‍♂️🚣🏾‍♀️🏊🏾🏊🏾‍♂️🏊🏾‍♀️⛹🏾⛹🏾‍♂️⛹🏾‍♀️🏋🏾🏋🏾‍♂️🏋🏾‍♀️🚴🏾🚴🏾‍♂️🚴🏾‍♀️🚵🏾🚵🏾‍♂️🚵🏾"
			+ "‍♀️🤸🏾🤸🏾‍♂️🤸🏾‍♀️🤽🏾🤽🏾‍♂️🤽🏾‍♀️🤾🏾🤾🏾‍♂️🤾🏾‍♀️🤹🏾🤹🏾‍♂️🤹🏾‍♀️🧘🏾🧘🏾‍♂️🧘🏾‍♀️🛀🏾🛌🏾🧑🏾‍🤝‍🧑🏾👬🏾👭🏾👫🏾👋🏿🤚🏿🖐🏿✋🏿🖖🏿👌🏿🤏🏿✌🏿🤞🏿🤟🏿🤘🏿🤙🏿👈🏿👉🏿👆🏿🖕🏿👇🏿☝🏿👍🏿👎"
			+ "🏿✊🏿👊🏿🤛🏿🤜🏿👏🏿🙌🏿👐🏿🤲🏿🙏🏿✍🏿💅🏿🤳🏿💪🏿🦵🏿🦶🏿👂🏿🦻🏿👃🏿👶🏿🧒🏿👦🏿👧🏿🧑🏿👨🏿👩🏿🧑🏿‍🦱👨🏿‍🦱👩🏿‍🦱🧑🏿‍🦰👨🏿‍🦰👩🏿‍🦰👱🏿👱🏿‍♂️👱🏿‍♀️🧑🏿‍🦳👨🏿‍🦳👩🏿‍🦳🧑🏿‍🦲👨🏿‍🦲👩🏿‍"
			+ "🦲🧔🏿🧓🏿👴🏿👵🏿🙍🏿🙍🏿‍♂️🙍🏿‍♀️🙎🏿🙎🏿‍♂️🙎🏿‍♀️🙅🏿🙅🏿‍♂️🙅🏿‍♀️🙆🏿🙆🏿‍♂️🙆🏿‍♀️💁🏿💁🏿‍♂️💁🏿‍♀️🙋🏿🙋🏿‍♂️🙋🏿‍♀️🧏🏿🧏🏿‍♂️🧏🏿‍♀️🙇🏿🙇🏿‍♂️🙇🏿‍♀️🤦🏿🤦🏿‍♂️🤦🏿‍♀️🤷🏿🤷🏿‍♂️🤷🏿‍♀️🧑"
			+ "🏿‍⚕️👨🏿‍⚕️👩🏿‍⚕️🧑🏿‍🎓👨🏿‍🎓👩🏿‍🎓🧑🏿‍🏫👨🏿‍🏫👩🏿‍🏫🧑🏿‍⚖️👨🏿‍⚖️👩🏿‍⚖️🧑🏿‍🌾👨🏿‍🌾👩🏿‍🌾🧑🏿‍🍳👨🏿‍🍳👩🏿‍🍳🧑🏿‍🔧👨🏿‍🔧👩🏿‍🔧🧑🏿‍🏭👨🏿‍🏭👩🏿‍🏭🧑🏿‍💼👨🏿‍💼👩🏿‍💼🧑🏿‍🔬👨🏿‍�"
			+ "�👩🏿‍🔬🧑🏿‍💻👨🏿‍💻👩🏿‍💻🧑🏿‍🎤👨🏿‍🎤👩🏿‍🎤🧑🏿‍🎨👨🏿‍🎨👩🏿‍🎨🧑🏿‍✈️👨🏿‍✈️👩🏿‍✈️🧑🏿‍🚀👨🏿‍🚀👩🏿‍🚀🧑🏿‍🚒👨🏿‍🚒👩🏿‍🚒👮🏿👮🏿‍♂️👮🏿‍♀️🕵🏿🕵🏿‍♂️🕵🏿‍♀️💂🏿💂🏿‍♂️💂🏿‍♀️👷🏿👷🏿‍♂️�"
			+ "�🏿‍♀️🤴🏿👸🏿👳🏿👳🏿‍♂️👳🏿‍♀️👲🏿🧕🏿🤵🏿👰🏿🤰🏿🤱🏿👼🏿🎅🏿🤶🏿🦸🏿🦸🏿‍♂️🦸🏿‍♀️🦹🏿🦹🏿‍♂️🦹🏿‍♀️🧙🏿🧙🏿‍♂️🧙🏿‍♀️🧚🏿🧚🏿‍♂️🧚🏿‍♀️🧛🏿🧛🏿‍♂️🧛🏿‍♀️🧜🏿🧜🏿‍♂️🧜🏿‍♀️🧝🏿🧝🏿‍♂️🧝🏿‍♀️💆🏿💆"
			+ "🏿‍♂️💆🏿‍♀️💇🏿💇🏿‍♂️💇🏿‍♀️🚶🏿🚶🏿‍♂️🚶🏿‍♀️🧍🏿🧍🏿‍♂️🧍🏿‍♀️🧎🏿🧎🏿‍♂️🧎🏿‍♀️🧑🏿‍🦯👨🏿‍🦯👩🏿‍🦯🧑🏿‍🦼👨🏿‍🦼👩🏿‍🦼🧑🏿‍🦽👨🏿‍🦽👩🏿‍🦽🏃🏿🏃🏿‍♂️🏃🏿‍♀️💃🏿🕺🏿🕴🏿🧖🏿🧖🏿‍♂️🧖🏿‍♀️🧗🏿�"
			+ "�🏿‍♂️🧗🏿‍♀️🏇🏿🏂🏿🏌🏿🏌🏿‍♂️🏌🏿‍♀️🏄🏿🏄🏿‍♂️🏄🏿‍♀️🚣🏿🚣🏿‍♂️🚣🏿‍♀️🏊🏿🏊🏿‍♂️🏊🏿‍♀️⛹🏿⛹🏿‍♂️⛹🏿‍♀️🏋🏿🏋🏿‍♂️🏋🏿‍♀️🚴🏿🚴🏿‍♂️🚴🏿‍♀️🚵🏿🚵🏿‍♂️🚵🏿‍♀️🤸🏿🤸🏿‍♂️🤸🏿‍♀️🤽🏿🤽🏿‍♂️🤽🏿‍♀️🤾"
			+ "🏿🤾🏿‍♂️🤾🏿‍♀️🤹🏿🤹🏿‍♂️🤹🏿‍♀️🧘🏿🧘🏿‍♂️🧘🏿‍♀️🛀🏿🛌🏿🧑🏿‍🤝‍🧑🏿👬🏿👭🏿👫🏿🐶🐱🐭🐹🐰🦊🐻🐼🐨🐯🦁🐮🐷🐽🐸🐵🙈🙉🙊🐒🐔🐧🐦🐤🐣🐥🦆🦅🦉🦇🐺🐗🐴🦄🐝🐛🦋🐌🐞🐜🦟🦗🕷🕸🦂🐢🐍🦎🦖🦕🐙🦑🦐🦞🦀🐡🐠🐟"
			+ "🐬🐳🐋🦈🐊🐅🐆🦓🦍🦧🐘🦛🦏🐪🐫🦒🦘🐃🐂🐄🐎🐖🐏🐑🦙🐐🦌🐕🐩🦮🐕‍🦺🐈🐓🦃🦚🦜🦢🦩🕊🐇🦝🦨🦡🦦🦥🐁🐀🐿🦔🐾🐉🐲🌵🎄🌲🌳🌴🌱🌿☘️🍀🎍🎋🍃🍂🍁🍄🐚🌾💐🌷🌹🥀🌺🌸🌼🌻🌞🌝🌛🌜🌚🌕🌖🌗🌘🌑🌒🌓🌔🌙🌎🌍🌏🪐💫⭐️🌟✨"
			+ "⚡️☄️💥🔥🌪🌈☀️🌤⛅️🌥☁️🌦🌧⛈🌩🌨❄️☃️⛄️🌬💨💧💦☔️☂️🌊🌫🍏🍎🍐🍊🍋🍌🍉🍇🍓🍈🍒🍑🥭🍍🥥🥝🍅🍆🥑🥦🥬🥒🌶🌽🥕🧄🧅🥔🍠🥐🥯🍞🥖🥨🧀🥚🍳🧈🥞🧇🥓🥩🍗🍖🦴🌭🍔🍟🍕🥪🥙🧆🌮🌯🥗🥘🥫🍝🍜🍲🍛🍣🍱🥟🦪🍤🍙🍚🍘🍥🥠🥮🍢�"
			+ "�🍧🍨🍦🥧🧁🍰🎂🍮🍭🍬🍫🍿🍩🍪🌰🥜🍯🥛🍼☕️🍵🧃🥤🍶🍺🍻🥂🍷🥃🍸🍹🧉🍾🧊🥄🍴🍽🥣🥡🥢🧂⚽️🏀🏈⚾️🥎🎾🏐🏉🥏🎱🪀🏓🏸🏒🏑🥍🏏🥅⛳️🪁🏹🎣🤿🥊🥋🎽🛹🛷⛸🥌🎿⛷🏂🪂🏋️🏋️‍♂️🏋️‍♀️🤼🤼‍♂️🤼‍♀️🤸‍♀️🤸🤸‍♂️⛹️⛹️‍♂️⛹️‍♀️"
			+ "🤺🤾🤾‍♂️🤾‍♀️🏌️🏌️‍♂️🏌️‍♀️🏇🧘🧘‍♂️🧘‍♀️🏄🏄‍♂️🏄‍♀️🏊🏊‍♂️🏊‍♀️🤽🤽‍♂️🤽‍♀️🚣🚣‍♂️🚣‍♀️🧗🧗‍♂️🧗‍♀️🚵🚵‍♂️🚵‍♀️🚴🚴‍♂️🚴‍♀️🏆🥇🥈🥉🏅🎖🏵🎗🎫🎟🎪🤹🤹‍♂️🤹‍♀️🎭🩰🎨🎬🎤🎧🎼🎹🥁🎷🎺🎸🪕🎻🎲♟🎯🎳🎮🎰"
			+ "🧩🚗🚕🚙🚌🚎🏎🚓🚑🚒🚐🚚🚛🚜🦯🦽🦼🛴🚲🛵🏍🛺🚨🚔🚍🚘🚖🚡🚠🚟🚃🚋🚞🚝🚄🚅🚈🚂🚆🚇🚊🚉✈️🛫🛬🛩💺🛰🚀🛸🚁🛶⛵️🚤🛥🛳⛴🚢⚓️⛽️🚧🚦🚥🚏🗺🗿🗽🗼🏰🏯🏟🎡🎢🎠⛲️⛱🏖🏝🏜🌋⛰🏔🗻🏕⛺️🏠🏡🏘🏚🏗🏭🏢🏬🏣🏤🏥🏦🏨🏪🏫🏩�"
			+ "�🏛⛪️🕌🕍🛕🕋⛩🛤🛣🗾🎑🏞🌅🌄🌠🎇🎆🌇🌆🏙🌃🌌🌉🌁⌚️📱📲💻⌨️🖥🖨🖱🖲🕹🗜💽💾💿📀📼📷📸📹🎥📽🎞📞☎️📟📠📺📻🎙🎚🎛🧭⏱⏲⏰🕰⌛️⏳📡🔋🔌💡🔦🕯🪔🧯🛢💸💵💴💶💷💰💳💎⚖️🧰🔧🔨⚒🛠⛏🔩⚙️🧱⛓🧲🔫💣🧨🪓🔪🗡⚔️🛡🚬⚰️⚱️🏺�"
			+ "�📿🧿💈⚗️🔭🔬🕳🩹🩺💊💉🩸🧬🦠🧫🧪🌡🧹🧺🧻🚽🚰🚿🛁🛀🧼🪒🧽🧴🛎🔑🗝🚪🪑🛋🛏🛌🧸🖼🛍🛒🎁🎈🎏🎀🎊🎉🎎🏮🎐🧧✉️📩📨📧💌📥📤📦🏷📪📫📬📭📮📯📜📃📄📑🧾📊📈📉🗒🗓📆📅🗑📇🗃🗳🗄📋📁📂🗂🗞📰📓📔📒📕📗📘📙📚📖🔖�"
			+ "�🔗📎🖇📐📏🧮📌📍✂️🖊🖋✒️🖌🖍📝✏️🔍🔎🔏🔐🔒🔓❤️🧡💛💚💙💜🖤🤍🤎💔❣️💕💞💓💗💖💘💝💟☮️✝️☪️🕉☸️✡️🔯🕎☯️☦️🛐⛎♈️♉️♊️♋️♌️♍️♎️♏️♐️♑️♒️♓️🆔⚛️🉑☢️☣️📴📳🈶🈚️🈸🈺🈷️✴️🆚💮🉐㊙️㊗️🈴🈵🈹🈲🅰️🅱️🆎🆑🅾️🆘❌⭕️🛑⛔️📛"
			+ "🚫💯💢♨️🚷🚯🚳🚱🔞📵🚭❗️❕❓❔‼️⁉️🔅🔆〽️⚠️🚸🔱⚜️🔰♻️✅🈯️💹❇️✳️❎🌐💠Ⓜ️🌀💤🏧🚾♿️🅿️🈳🈂️🛂🛃🛄🛅🚹🚺🚼🚻🚮🎦📶🈁🔣ℹ️🔤🔡🔠🆖🆗🆙🆒🆕🆓0️⃣1️⃣2️⃣3️⃣4️⃣5️⃣6️⃣7️⃣8️⃣9️⃣🔟🔢#️⃣*️⃣⏏️▶️⏸⏯⏹⏺⏭⏮⏩⏪⏫⏬◀️🔼🔽➡️⬅️⬆️⬇️↗️"
			+ "↘️↙️↖️↕️↔️↪️↩️⤴️⤵️🔀🔁🔂🔄🔃🎵🎶➕➖➗✖️♾💲💱™️©️®️〰️➰➿🔚🔙🔛🔝🔜✔️☑️🔘🔴🟠🟡🟢🔵🟣⚫️⚪️🟤🔺🔻🔸🔹🔶🔷🔳🔲▪️▫️◾️◽️◼️◻️🟥🟧🟨🟩🟦🟪⬛️⬜️🟫🔈🔇🔉🔊🔔🔕📣📢👁‍🗨💬💭🗯♠️♣️♥️♦️🃏🎴🀄️🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚🕛🕜"
			+ "🕝🕞🕟🕠🕡🕢🕣🕤🕥🕦🕧🏳️🏴🏁🚩🏳️‍🌈🏴‍☠️🇦🇫🇦🇽🇦🇱🇩🇿🇦🇸🇦🇩🇦🇴🇦🇮🇦🇶🇦🇬🇦🇷🇦🇲🇦🇼🇦🇺🇦🇹🇦🇿🇧🇸🇧🇭🇧🇩🇧🇧🇧🇾🇧🇪🇧🇿🇧🇯🇧🇲🇧🇹🇧🇴🇧🇦🇧🇼🇧🇷🇮🇴🇻🇬🇧🇳🇧🇬🇧🇫🇧🇮🇰🇭🇨🇲🇨🇦🇮"
			+ "🇨🇨🇻🇧🇶🇰🇾🇨🇫🇹🇩🇨🇱🇨🇳🇨🇽🇨🇨🇨🇴🇰🇲🇨🇬🇨🇩🇨🇰🇨🇷🇨🇮🇭🇷🇨🇺🇨🇼🇨🇾🇨🇿🇩🇰🇩🇯🇩🇲🇩🇴🇪🇨🇪🇬🇸🇻🇬🇶🇪🇷🇪🇪🇪🇹🇪🇺🇫🇰🇫🇴🇫🇯🇫🇮🇫🇷🇬🇫🇵🇫🇹🇫🇬🇦🇬🇲🇬🇪🇩🇪🇬🇭🇬🇮🇬🇷🇬🇱🇬"
			+ "🇩🇬🇵🇬🇺🇬🇹🇬🇬🇬🇳🇬🇼🇬🇾🇭🇹🇭🇳🇭🇰🇭🇺🇮🇸🇮🇳🇮🇩🇮🇷🇮🇶🇮🇪🇮🇲🇮🇱🇮🇹🇯🇲🇯🇵🎌🇯🇪🇯🇴🇰🇿🇰🇪🇰🇮🇽🇰🇰🇼🇰🇬🇱🇦🇱🇻🇱🇧🇱🇸🇱🇷🇱🇾🇱🇮🇱🇹🇱🇺🇲🇴🇲🇰🇲🇬🇲🇼🇲🇾🇲🇻🇲🇱🇲🇹🇲🇭🇲🇶"
			+ "🇲🇷🇲🇺🇾🇹🇲🇽🇫🇲🇲🇩🇲🇨🇲🇳🇲🇪🇲🇸🇲🇦🇲🇿🇲🇲🇳🇦🇳🇷🇳🇵🇳🇱🇳🇨🇳🇿🇳🇮🇳🇪🇳🇬🇳🇺🇳🇫🇰🇵🇲🇵🇳🇴🇴🇲🇵🇰🇵🇼🇵🇸🇵🇦🇵🇬🇵🇾🇵🇪🇵🇭🇵🇳🇵🇱🇵🇹🇵🇷🇶🇦🇷🇪🇷🇴🇷🇺🇷🇼🇼🇸🇸🇲🇸🇦🇸🇳🇷🇸"
			+ "🇸🇨🇸🇱🇸🇬🇸🇽🇸🇰🇸🇮🇬🇸🇸🇧🇸🇴🇿🇦🇰🇷🇸🇸🇪🇸🇱🇰🇧🇱🇸🇭🇰🇳🇱🇨🇵🇲🇻🇨🇸🇩🇸🇷🇸🇿🇸🇪🇨🇭🇸🇾🇹🇼🇹🇯🇹🇿🇹🇭🇹🇱🇹🇬🇹🇰🇹🇴🇹🇹🇹🇳🇹🇷🇹🇲🇹🇨🇹🇻🇻🇮🇺🇬🇺🇦🇦🇪🇬🇧🏴󠁧󠁢󠁥󠁮󠁧󠁿🏴󠁧󠁢"
			+ "󠁳󠁣󠁴󠁿🏴󠁧󠁢󠁷󠁬󠁳󠁿🇺🇳🇺🇸🇺🇾🇺🇿🇻🇺🇻🇦🇻🇪🇻🇳🇼🇫🇪🇭🇾🇪🇿🇲🇿🇼";
		public abstract char Character { get; }

		public abstract string Description { get; }
		public abstract Type Type { get; }

		public static IEnumerable<ParameterType> KnownParameterTypes => ParameterTypes.Values;
		private static ConcurrentDictionary<Type, ParameterType> ParameterTypes { get; } = new ConcurrentDictionary<Type, ParameterType>();

		static ParameterType() {
			ParameterType boolParameter = new ParameterType<bool>('b', "true or false", ParameterAsBool);
			ParameterTypes.TryAdd(boolParameter.Type, boolParameter);
			ParameterType intParameter = new ParameterType<int>('n', "a number", ParameterAsInt);
			ParameterTypes.TryAdd(intParameter.Type, intParameter);
			ParameterType ulongParameter = new ParameterType<ulong>('p', "a positive number", ParameterAsULong);
			ParameterTypes.TryAdd(ulongParameter.Type, ulongParameter);
			ParameterType stringParameter = new ParameterType<string>('t', "a text", ParametersAsString);
			ParameterTypes.TryAdd(stringParameter.Type, stringParameter);
			ParameterType userParameter = new ParameterType<SocketUser>('u', "a user mention or @username#number", ParameterAsSocketUser);
			ParameterTypes.TryAdd(userParameter.Type, userParameter);
			ParameterType roleParameter = new ParameterType<SocketRole>('r', "a role mention", ParameterAsSocketRole);
			ParameterTypes.TryAdd(roleParameter.Type, roleParameter);
			ParameterType emojiParameter = new ParameterType<IEmote>('e', "an emoji", ParameterAsEmote);
			ParameterTypes.TryAdd(emojiParameter.Type, emojiParameter);
			ParameterType wordParameter = new ParameterType<Word>('w', "a word", ParameterAsWord);
			ParameterTypes.TryAdd(wordParameter.Type, wordParameter);
			ParameterType timespanParameter = new ParameterType<TimeSpan>('s', "a timespan in format 30d24h59m59s (also 40d2s)", ParameterAsTimespan);
			ParameterTypes.TryAdd(timespanParameter.Type, timespanParameter);
			ParameterType dateParameter = new ParameterType<DateTime>('d',
				"a date in format \"31.12.2020 23:59:59 +365d24h60m60s\" with every part optional (\"31.12 +2h\",23:59,3.4.2023,+4h)", ParameterAsDate);
			ParameterTypes.TryAdd(dateParameter.Type, dateParameter);
			ParameterType channelParameter = new ParameterType<IMessageChannel>('c', "a text channel mention", ParameterAsMessageChannel);
			ParameterTypes.TryAdd(channelParameter.Type, channelParameter);
			ParameterType timezoneParameter = new ParameterType<Timezone>('z', "a timezone (-12 - 12)", ParameterAsTimezone);
			ParameterTypes.TryAdd(timezoneParameter.Type, timezoneParameter);
			ParameterType settingsParameter = new ParameterType<Settings>();
			ParameterTypes.TryAdd(settingsParameter.Type, settingsParameter);
			ParameterType messageParameter = new ParameterType<SocketMessage>();
			ParameterTypes.TryAdd(messageParameter.Type, messageParameter);
			ParameterType socketChannelParameter = new ParameterType<ISocketMessageChannel>();
			ParameterTypes.TryAdd(socketChannelParameter.Type, socketChannelParameter);
		}

		protected abstract object InterpretParameter(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, DefaultValue<object> value,
			Optional<object> min, Optional<object> max, bool throwOutOfRange, Settings settings);

		[SuppressMessage("ReSharper", "InvertIf")]
		private static T CheckMinMax<T>(T value, int index, Optional<T> min, Optional<T> max, bool throwOutOfRange)
			where T : IComparable {
			if (min.IsSpecified) {
				if (value.CompareTo(min.Value) < 0) {
					if (throwOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {index} must be bigger than {min.Value}");
					}
					return min.Value;
				}
			}
			if (max.IsSpecified) {
				if (value.CompareTo(max.Value) > 0) {
					if (throwOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {index} must be smaller than {max.Value}");
					}
					return max.Value;
				}
			}

			return value;
		}

		public static ParameterType FromType(Type type) {
			if (ParameterTypes.ContainsKey(type)) {
				return ParameterTypes[type];
			}
			foreach ((Type key, ParameterType value) in ParameterTypes) {
				if (type.IsAssignableFrom(key)) {
					return value;
				}
			}

			throw new KudosInternalException($"Unknown ParameterType ({type})");
		}

		public static T InterpretParameter<T>(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue, Optional<T> min,
			Optional<T> max, bool throwOutOfRange, Settings settings) =>
			((ParameterType<T>)FromType(typeof (T))).ParameterInterpreter.Invoke(parameters, indexLess, index, optional, defaultValue, min, max,
				throwOutOfRange, settings);

		public static object InterpretParameter(Type type, string[] parameters, IEnumerable<object> indexLess, int index, bool optional,
			DefaultValue<object> defaultValue, Optional<object> min, Optional<object> max, bool throwOutOfRange, Settings settings) => FromType(type)
			.InterpretParameter(parameters, indexLess, index, optional, defaultValue, min, max,
				throwOutOfRange, settings);

		private static bool ParameterAsBool(string[] parameters, bool indexLess, int index, bool optional, DefaultValue<bool> defaultValue, Optional<bool> min,
			Optional<bool> max, bool throwOutOfRange, Settings settings) {
			if (ParameterPresent(parameters, index) && bool.TryParse(parameters[index], out bool value)) {
				return value;
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be true or false (bool)");
			}
			return value;
		}

		private static DateTime ParameterAsDate(string[] parameters, DateTime indexLess, int index, bool optional, DefaultValue<DateTime> defaultValue,
			Optional<DateTime> min, Optional<DateTime> max, bool throwOutOfRange, Settings settings) {
			Regex dateTimeRegex =
				new Regex(
					@"^(?:(?:(\d{1,2}))(?:\.(\d{1,2}))?(?:\.(\d{1,4}))?)??(?:(?:(?<!.)(?=.))|(?:(?!.)(?<=.))|(?<=.) (?=.))(?:(?:(\d{1,2}))(?::(\d{1,2}))?(?::(\d{1,2}))?)?(?:(?:(?<!.)(?=.))|(?:(?!.)(?<=.))|(?<=[^ ]) (?=.)|(?<= ))(?:\+(?:(\d{1,3})d)?(?:(\d{1,2})h)?(?:(\d{1,2})m)?(?:(\d{1,2})s)?)?$");

			//$3-$2-$1 $4:$5:$6 + $7d$8h$9m$10s
			Match match;
			DateTime value;
			if (ParameterPresent(parameters, index) && (match = dateTimeRegex.Match(parameters[index])).Success) {
				DateTime now = DateTime.Now;
				settings[SettingNames.Timezone].Value(out Timezone timezone);
				now = now.AddHours(timezone);
				int year = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : now.Year;
				int month = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : now.Month;
				int day = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : now.Day;

				int hour = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : now.Hour;
				int minute = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : match.Groups[4].Success ? 0 : now.Minute;
				int second = match.Groups[6].Success ? int.Parse(match.Groups[6].Value) : match.Groups[4].Success ? 0 : now.Second;
				value = new DateTime(year, month, day, hour, minute, second);
				if (!match.Groups[1].Success && match.Groups[4].Success && value < now) {
					value = value.AddDays(1);
				} else {
					if (!match.Groups[3].Success) {
						if (!match.Groups[2].Success) {
							if (match.Groups[1].Success && value < now) {
								value = value.AddMonths(1);
							}
						} else {
							if (value < now) {
								value = value.AddYears(1);
							}
						}
					}
				}
				int dayAdding = match.Groups[7].Success ? int.Parse(match.Groups[7].Value) : 0;
				int hourAdding = match.Groups[8].Success ? int.Parse(match.Groups[8].Value) : 0;
				int minuteAdding = match.Groups[9].Success ? int.Parse(match.Groups[9].Value) : 0;
				int secondAdding = match.Groups[10].Success ? int.Parse(match.Groups[10].Value) : 0;

				value = value.AddDays(dayAdding);
				value = value.AddHours(hourAdding);
				value = value.AddMinutes(minuteAdding);
				value = value.AddSeconds(secondAdding);
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}

			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a date");
			}
			return value;
		}

		private static IEmote ParameterAsEmote(string[] parameters, IEmote indexLess, int index, bool optional, DefaultValue<IEmote> defaultValue,
			Optional<IEmote> min, Optional<IEmote> max, bool throwOutOfRange, Settings settings) {
			if (ParameterPresent(parameters, index) && Emote.TryParse(parameters[index], out Emote value)) {
				return value;
			}
			if (Emojis.Contains(parameters[index])) {
				return new Emoji(parameters[index]);
			}
			if (!optional) {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be an emoji");
			}
			SetToDefaultValue(out IEmote emote, defaultValue, indexLess);
			return emote;
		}

		private static int ParameterAsInt(string[] parameters, int indexLess, int index, bool optional, DefaultValue<int> defaultValue, Optional<int> min,
			Optional<int> max, bool throwOutOfRange, Settings settings) {
			if (ParameterPresent(parameters, index) && int.TryParse(parameters[index], out int value)) {
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a number (int)");
			}
			return value;
		}

		private static IMessageChannel ParameterAsMessageChannel(string[] parameters, IMessageChannel indexLess, int index, bool optional,
			DefaultValue<IMessageChannel> defaultValue, Optional<IMessageChannel> min, Optional<IMessageChannel> max, bool throwOutOfRange, Settings settings) {
			IMessageChannel channel;
			if (ParameterPresent(parameters, index)) {
				channel = parameters[index].ChannelFromMention();
				if (channel != null) {
					return channel;
				}
			}
			if (!optional) {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a mentioned text channel");
			}

			SetToDefaultValue(out channel, defaultValue, indexLess);
			return channel;
		}

		private static SocketRole ParameterAsSocketRole(string[] parameters, SocketRole indexLess, int index, bool optional,
			DefaultValue<SocketRole> defaultValue, Optional<SocketRole> min, Optional<SocketRole> max, bool throwOutOfRange, Settings settings) {
			SocketRole role;
			if (ParameterPresent(parameters, index)) {
				role = parameters[index].RoleFromMention();
				if (role != null) {
					return role;
				}
			}
			if (!optional) {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a mentioned role");
			}

			SetToDefaultValue(out role, defaultValue, indexLess);
			return role;
		}

		private static SocketUser ParameterAsSocketUser(string[] parameters, SocketUser indexLess, int index, bool optional,
			DefaultValue<SocketUser> defaultValue, Optional<SocketUser> min, Optional<SocketUser> max, bool throwOutOfRange, Settings settings) {
			SocketUser user;
			if (ParameterPresent(parameters, index)) {
				user = parameters[index].FromMention();
				if (user != null) {
					return user;
				}

				string[] userData = parameters[index].Split("#");
				if (userData.Length == 2) {
					user = Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
				}
				if (user != null) {
					return user;
				}
			}
			if (!optional) {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a user (described in help)");
			}

			SetToDefaultValue(out user, defaultValue, indexLess);
			return user;
		}

		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		private static TimeSpan ParameterAsTimespan(string[] parameters, TimeSpan indexLess, int index, bool optional, DefaultValue<TimeSpan> defaultValue,
			Optional<TimeSpan> min, Optional<TimeSpan> max, bool throwOutOfRange, Settings settings) {
			string[] formats = {
				"d'd'", "h'h'", "m'm'", "s's'", "d'd'h'h'", "d'd'm'm'", "d'd's's'", "h'h'm'm'", "h'h's's'", "m'm's's'", "d'd'h'h'm'm'", "d'd'h'h's's'",
				"d'd'm'm's's'", "h'h'm'm's's'", "d'd'h'h'm'm's's'"
			};
			if (ParameterPresent(parameters, index) && TimeSpan.TryParseExact(parameters[index], formats, null, out TimeSpan value)) {
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a timespan");
			}
			return value;
		}

		private static Timezone ParameterAsTimezone(string[] parameters, Timezone indexLess, int index, bool optional, DefaultValue<Timezone> defaultValue,
			Optional<Timezone> min, Optional<Timezone> max, bool throwOutOfRange, Settings settings) {
			Timezone value;
			if (ParameterPresent(parameters, index) && double.TryParse(parameters[index], out double doubleValue)) {
				return doubleValue;
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a Timezone (-12 - 12)");
			}
			return value;
		}

		private static ulong ParameterAsULong(string[] parameters, ulong indexLess, int index, bool optional, DefaultValue<ulong> defaultValue,
			Optional<ulong> min, Optional<ulong> max, bool throwOutOfRange, Settings settings) {
			if (ParameterPresent(parameters, index) && ulong.TryParse(parameters[index], out ulong value)) {
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a number (ulong)");
			}
			return value;
		}

		private static Word ParameterAsWord(string[] parameters, Word indexLess, int index, bool optional, DefaultValue<Word> defaultValue, Optional<Word> min,
			Optional<Word> max, bool throwOutOfRange, Settings settings) {
			Word value;
			if (ParameterPresent(parameters, index)) {
				return parameters[index];
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a word (a-z)");
			}
			return value;
		}

		private static bool ParameterPresent(IReadOnlyList<string> parameters, int index) =>
			parameters.Count > index && !string.IsNullOrEmpty(parameters[index]) && parameters[index] != "-";

		private static string ParametersAsString(string[] parameters, string indexLess, int index, bool optional, DefaultValue<string> defaultValue,
			Optional<string> min, Optional<string> max, bool throwOutOfRange, Settings settings) {
			if (ParameterPresent(parameters, index)) {
				return parameters[index];
			}
			if (!optional) {
				throw new KudosArgumentTypeException($"Parameter {index + 1} must be a text");
			}
			SetToDefaultValue(out string value, defaultValue, indexLess);
			return value;
		}

		private static void SetToDefaultValue<T>(out T value, DefaultValue<T> defaultValue, T indexLess) {
			if (defaultValue.Special == SpecialDefaults.IndexLess) {
				value = indexLess;
				return;
			}
			if (defaultValue.Value.IsSpecified) {
				value = defaultValue.Value.Value;
				return;
			}
			value = default;
		}

		public string ToHtml() => Description == null ? string.Empty : $"<tr><td><b>{Character}</b> </td> <td>{Description} </td></tr>";

		public override string ToString() => Description == null ? string.Empty : $"`{Character}` {Description}";

		public class DefaultValue<T> {
			public SpecialDefaults Special { get; }
			public Optional<T> Value { get; }

			public DefaultValue(Optional<T> value = default, SpecialDefaults special = SpecialDefaults.None) {
				Value = value;
				Special = special;
			}

			public static DefaultValue<T> Create(object value) {
				return value switch {
					SpecialDefaults special => new DefaultValue<T>(default, special),
					T defaultValue => new DefaultValue<T>(defaultValue),
					_ => new DefaultValue<T>()
				};
			}

			public static DefaultValue<T> FromObject(DefaultValue<object> defaultValue) {
				T newValue = defaultValue.Value.IsSpecified ? defaultValue.Value.Value is T tValue ? tValue : default : default;
				return new DefaultValue<T>(newValue, defaultValue.Special);
			}
		}

		protected internal delegate T ParameterAsType<T>(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue,
			Optional<T> min, Optional<T> max, bool throwOutOfRange, Settings settings);

		public enum SpecialDefaults {
			None,
			IndexLess
		}
	}

	public class ParameterType<T> : ParameterType {
		public override char Character { get; }
		public override string Description { get; }
		protected internal ParameterAsType<T> ParameterInterpreter { get; }

		public override Type Type { get; }

		protected internal ParameterType(char character = default, string description = null, ParameterAsType<T> parameterInterpreter = null) {
			Character = character;
			Type = typeof (T);
			Description = description;
			ParameterInterpreter = parameterInterpreter ?? NotImplemented;

			static T NotImplemented(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue, Optional<T> min, Optional<T> max,
				bool throwOutOfRange, Settings settings) => throw new NotImplementedException();
		}

		protected override object InterpretParameter(string[] parameters, IEnumerable<object> indexLess, int index, bool optional,
			DefaultValue<object> defaultValue, Optional<object> min, Optional<object> max, bool throwOutOfRange, Settings settings) {
			return ParameterInterpreter.Invoke(parameters, indexLess.FirstOrDefault(obj => obj is T) is T tValue ? tValue : default, index, optional,
				DefaultValue<T>.FromObject(defaultValue), min is Optional<T> tMin ? tMin : default, max is Optional<T> tMax ? tMax : default, throwOutOfRange,
				settings);
		}
	}
}
