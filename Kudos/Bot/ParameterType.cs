#region
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Bot
{
    public abstract class ParameterType
    {
        private const string Emojis =
            "😀😃😄😁😆😅😂🤣☺️😊😇🙂🙃😉😌😍🥰😘😗😙😚😋😛😝😜🤪🤨🧐🤓😎🤩🥳😏😒😞😔😟😕🙁☹️😣😖😫😩🥺😢😭😤😠😡🤬🤯😳🥵🥶😱😨😰😥😓🤗🤔🤭🤫🤥😶😐😑😬🙄😯😦😧😮😲🥱😴🤤😪😵🤐🥴🤢🤮🤧😷🤒🤕🤑🤠😈👿👹👺🤡💩👻💀☠️👽👾🤖🎃😺😸😹😻😼😽🙀😿😾👋🤚🖐✋🖖👌🤏✌️🤞🤟🤘🤙👈👉👆🖕👇☝️👍👎✊👊🤛🤜👏🙌👐🤲🤝🙏✍️💅🤳💪🦾🦵🦿🦶👂🦻👃🧠🦷🦴👀👁👅👄💋🩸👶🧒👦👧🧑👱👨🧔👨‍🦰👨‍🦱👨‍🦳👨‍🦲👩👩‍🦰🧑‍🦰👩‍🦱🧑‍🦱👩‍🦳🧑‍🦳👩‍🦲🧑‍🦲👱‍♀️👱‍♂️🧓👴👵🙍🙍‍♂️🙍‍♀️🙎🙎‍♂️🙎‍♀️🙅🙅‍♂️🙅‍♀️🙆🙆‍♂️🙆‍♀️💁💁‍♂️💁‍♀️🙋🙋‍♂️🙋‍♀️🧏🧏‍♂️🧏‍♀️🙇🙇‍♂️🙇‍♀️🤦🤦‍♂️🤦‍♀️🤷🤷‍♂️🤷‍♀️🧑‍⚕️👨‍⚕️👩‍⚕️🧑‍🎓👨‍🎓👩‍🎓🧑‍🏫👨‍🏫👩‍🏫🧑‍⚖️👨‍⚖️👩‍⚖️🧑‍🌾👨‍🌾👩‍🌾🧑‍🍳👨‍🍳👩‍🍳🧑‍🔧👨‍🔧👩‍🔧🧑‍🏭👨‍🏭👩‍🏭🧑‍💼👨‍💼👩‍💼🧑‍🔬👨‍🔬👩‍🔬🧑‍💻👨‍💻👩‍💻🧑‍🎤👨‍🎤👩‍🎤🧑‍🎨👨‍🎨👩‍🎨🧑‍✈️👨‍✈️👩‍✈️🧑‍🚀👨‍🚀👩‍🚀🧑‍🚒👨‍🚒👩‍🚒👮👮‍♂️👮‍♀️🕵🕵️‍♂️🕵️‍♀️💂💂‍♂️💂‍♀️👷👷‍♂️👷‍♀️🤴👸👳👳‍♂️👳‍♀️👲🧕🤵👰🤰🤱👼🎅🤶🦸🦸‍♂️🦸‍♀️🦹🦹‍♂️🦹‍♀️🧙🧙‍♂️🧙‍♀️🧚🧚‍♂️🧚‍♀️🧛🧛‍♂️🧛‍♀️🧜🧜‍♂️🧜‍♀️🧝🧝‍♂️🧝‍♀️🧞🧞‍♂️🧞‍♀️🧟🧟‍♂️🧟‍♀️💆💆‍♂️💆‍♀️💇💇‍♂️💇‍♀️🚶🚶‍♂️🚶‍♀️🧍🧍‍♂️🧍‍♀️🧎🧎‍♂️🧎‍♀️🧑‍🦯👨‍🦯👩‍🦯🧑‍🦼👨‍🦼👩‍🦼🧑‍🦽👨‍🦽👩‍🦽🏃🏃‍♂️🏃‍♀️💃🕺🕴👯👯‍♂️👯‍♀️🧖🧖‍♂️🧖‍♀️🧘🧑‍🤝‍🧑👭👫👬💏👨‍❤️‍💋‍👨👩‍❤️‍💋‍👩💑👨‍❤️‍👨👩‍❤️‍👩👪👨‍👩‍👦👨‍👩‍👧👨‍👩‍👧‍👦👨‍👩‍👦‍👦👨‍👩‍👧‍👧👨‍👨‍👦👨‍👨‍👧👨‍👨‍👧‍👦👨‍👨‍👦‍👦👨‍👨‍👧‍👧👩‍👩‍👦👩‍👩‍👧👩‍👩‍👧‍👦👩‍👩‍👦‍👦👩‍👩‍👧‍👧👨‍👦👨‍👦‍👦👨‍👧👨‍👧‍👦👨‍👧‍👧👩‍👦👩‍👦‍👦👩‍👧👩‍👧‍👦👩‍👧‍👧🗣👤👥👣🧳🌂☂️🧵🧶👓🕶🥽🥼🦺👔👕👖🧣🧤🧥🧦👗👘🥻🩱🩲🩳👙👚👛👜👝🎒👞👟🥾🥿👠👡🩰👢👑👒🎩🎓🧢⛑💄💍💼👋🏻🤚🏻🖐🏻✋🏻🖖🏻👌🏻🤏🏻✌🏻🤞🏻🤟🏻🤘🏻🤙🏻👈🏻👉🏻👆🏻🖕🏻👇🏻☝🏻👍🏻👎🏻✊🏻👊🏻🤛🏻🤜🏻👏🏻🙌🏻👐🏻🤲🏻🙏🏻✍🏻💅🏻🤳🏻💪🏻🦵🏻🦶🏻👂🏻🦻🏻👃🏻👶🏻🧒🏻👦🏻👧🏻🧑🏻👨🏻👩🏻🧑🏻‍🦱👨🏻‍🦱👩🏻‍🦱🧑🏻‍🦰👨🏻‍🦰👩🏻‍🦰👱🏻👱🏻‍♂️👱🏻‍♀️🧑🏻‍🦳👩🏻‍🦳👨🏻‍🦳🧑🏻‍🦲👨🏻‍🦲👩🏻‍🦲🧔🏻🧓🏻👴🏻👵🏻🙍🏻🙍🏻‍♂️🙍🏻‍♀️🙎🏻🙎🏻‍♂️🙎🏻‍♀️🙅🏻🙅🏻‍♂️🙅🏻‍♀️🙆🏻🙆🏻‍♂️🙆🏻‍♀️💁🏻💁🏻‍♂️💁🏻‍♀️🙋🏻🙋🏻‍♂️🙋🏻‍♀️🧏🏻🧏🏻‍♂️🧏🏻‍♀️🙇🏻🙇🏻‍♂️🙇🏻‍♀️🤦🏻🤦🏻‍♂️🤦🏻‍♀️🤷🏻🤷🏻‍♂️🤷🏻‍♀️🧑🏻‍⚕️👨🏻‍⚕️👩🏻‍⚕️🧑🏻‍🎓👨🏻‍🎓👩🏻‍🎓🧑🏻‍🏫👨🏻‍🏫👩🏻‍🏫🧑🏻‍⚖️👨🏻‍⚖️👩🏻‍⚖️🧑🏻‍🌾👨🏻‍🌾👩🏻‍🌾🧑🏻‍🍳👨🏻‍🍳👩🏻‍🍳🧑🏻‍🔧👨🏻‍🔧👩🏻‍🔧🧑🏻‍🏭👨🏻‍🏭👩🏻‍🏭🧑🏻‍💼👨🏻‍💼👩🏻‍💼🧑🏻‍🔬👨🏻‍🔬👩🏻‍🔬🧑🏻‍💻👨🏻‍💻👩🏻‍💻🧑🏻‍🎤👨🏻‍🎤👩🏻‍🎤🧑🏻‍🎨👨🏻‍🎨👩🏻‍🎨🧑🏻‍✈️👨🏻‍✈️👩🏻‍✈️🧑🏻‍🚀👨🏻‍🚀👩🏻‍🚀🧑🏻‍🚒👨🏻‍🚒👩🏻‍🚒👮🏻👮🏻‍♂️👮🏻‍♀️🕵🏻🕵🏻‍♂️🕵🏻‍♀️💂🏻💂🏻‍♂️💂🏻‍♀️👷🏻👷🏻‍♂️👷🏻‍♀️🤴🏻👸🏻👳🏻👳🏻‍♂️👳🏻‍♀️👲🏻🧕🏻🤵🏻👰🏻🤰🏻🤱🏻👼🏻🎅🏻🤶🏻🦸🏻🦸🏻‍♂️🦸🏻‍♀️🦹🏻🦹🏻‍♂️🦹🏻‍♀️🧙🏻🧙🏻‍♂️🧙🏻‍♀️🧚🏻🧚🏻‍♂️🧚🏻‍♀️🧛🏻🧛🏻‍♂️🧛🏻‍♀️🧜🏻🧜🏻‍♂️🧜🏻‍♀️🧝🏻🧝🏻‍♂️🧝🏻‍♀️💆🏻💆🏻‍♂️💆🏻‍♀️💇🏻💇🏻‍♂️💇🏻‍♀️🚶🏻🚶🏻‍♂️🚶🏻‍♀️🧍🏻🧍🏻‍♂️🧍🏻‍♀️🧎🏻🧎🏻‍♂️🧎🏻‍♀️🧑🏻‍🦯👨🏻‍🦯👩🏻‍🦯🧑🏻‍🦼👨🏻‍🦼👩🏻‍🦼🧑🏻‍🦽👨🏻‍🦽👩🏻‍🦽🏃🏻🏃🏻‍♂️🏃🏻‍♀️💃🏻🕺🏻🕴🏻🧖🏻🧖🏻‍♂️🧖🏻‍♀️🧗🏻🧗🏻‍♂️🧗🏻‍♀️🏇🏻🏂🏻🏌🏻🏌🏻‍♂️🏌🏻‍♀️🏄🏻🏄🏻‍♂️🏄🏻‍♀️🚣🏻🚣🏻‍♂️🚣🏻‍♀️🏊🏻🏊🏻‍♂️🏊🏻‍♀️⛹🏻⛹🏻‍♂️⛹🏻‍♀️🏋🏻🏋🏻‍♂️🏋🏻‍♀️🚴🏻🚴🏻‍♂️🚴🏻‍♀️🚵🏻🚵🏻‍♂️🚵🏻‍♀️🤸🏻🤸🏻‍♂️🤸🏻‍♀️🤽🏻🤽🏻‍♂️🤽🏻‍♀️🤾🏻🤾🏻‍♂️🤾🏻‍♀️🤹🏻🤹🏻‍♂️🤹🏻‍♀️🧘🏻🧘🏻‍♂️🧘🏻‍♀️🛀🏻🛌🏻🧑🏻‍🤝‍🧑🏻👬🏻👭🏻👫🏻👋🏼🤚🏼🖐🏼✋🏼🖖🏼👌🏼🤏🏼✌🏼🤞🏼🤟🏼🤘🏼🤙🏼👈🏼👉🏼👆🏼🖕🏼👇🏼☝🏼👍🏼👎🏼✊🏼👊🏼🤛🏼🤜🏼👏🏼🙌🏼👐🏼🤲🏼🙏🏼✍🏼💅🏼🤳🏼💪🏼🦵🏼🦶🏼👂🏼🦻🏼👃🏼👶🏼🧒🏼👦🏼👧🏼🧑🏼👨🏼👩🏼🧑🏼‍🦱👨🏼‍🦱👩🏼‍🦱🧑🏼‍🦰👨🏼‍🦰👩🏼‍🦰👱🏼👱🏼‍♂️👱🏼‍♀️🧑🏼‍🦳👨🏼‍🦳👩🏼‍🦳🧑🏼‍🦲👨🏼‍🦲👩🏼‍🦲🧔🏼🧓🏼👴🏼👵🏼🙍🏼🙍🏼‍♂️🙍🏼‍♀️🙎🏼🙎🏼‍♂️🙎🏼‍♀️🙅🏼🙅🏼‍♂️🙅🏼‍♀️🙆🏼🙆🏼‍♂️🙆🏼‍♀️💁🏼💁🏼‍♂️💁🏼‍♀️🙋🏼🙋🏼‍♂️🙋🏼‍♀️🧏🏼🧏🏼‍♂️🧏🏼‍♀️🙇🏼🙇🏼‍♂️🙇🏼‍♀️🤦🏼🤦🏼‍♂️🤦🏼‍♀️🤷🏼🤷🏼‍♂️🤷🏼‍♀️🧑🏼‍⚕️👨🏼‍⚕️👩🏼‍⚕️🧑🏼‍🎓👨🏼‍🎓👩🏼‍🎓🧑🏼‍🏫👨🏼‍🏫👩🏼‍🏫🧑🏼‍⚖️👨🏼‍⚖️👩🏼‍⚖️🧑🏼‍🌾👨🏼‍🌾👩🏼‍🌾🧑🏼‍🍳👨🏼‍🍳👩🏼‍🍳🧑🏼‍🔧👨🏼‍🔧👩🏼‍🔧🧑🏼‍🏭👨🏼‍🏭👩🏼‍🏭🧑🏼‍💼👨🏼‍💼👩🏼‍💼🧑🏼‍🔬👨🏼‍🔬👩🏼‍🔬🧑🏼‍💻👨🏼‍💻👩🏼‍💻🧑🏼‍🎤👨🏼‍🎤👩🏼‍🎤🧑🏼‍🎨👨🏼‍🎨👩🏼‍🎨🧑🏼‍✈️👨🏼‍✈️👩🏼‍✈️🧑🏼‍🚀👨🏼‍🚀👩🏼‍🚀🧑🏼‍🚒👨🏼‍🚒👩🏼‍🚒👮🏼👮🏼‍♂️👮🏼‍♀️🕵🏼🕵🏼‍♂️🕵🏼‍♀️💂🏼💂🏼‍♂️💂🏼‍♀️👷🏼👷🏼‍♂️👷🏼‍♀️🤴🏼👸🏼👳🏼👳🏼‍♂️👳🏼‍♀️👲🏼🧕🏼🤵🏼👰🏼🤰🏼🤱🏼👼🏼🎅🏼🤶🏼🦸🏼🦸🏼‍♂️🦸🏼‍♀️🦹🏼🦹🏼‍♂️🦹🏼‍♀️🧙🏼🧙🏼‍♂️🧙🏼‍♀️🧚🏼🧚🏼‍♂️🧚🏼‍♀️🧛🏼🧛🏼‍♂️🧛🏼‍♀️🧜🏼🧜🏼‍♂️🧜🏼‍♀️🧝🏼🧝🏼‍♂️🧝🏼‍♀️💆🏼💆🏼‍♂️💆🏼‍♀️💇🏼💇🏼‍♂️💇🏼‍♀️🚶🏼🚶🏼‍♂️🚶🏼‍♀️🧍🏼🧍🏼‍♂️🧍🏼‍♀️🧎🏼🧎🏼‍♂️🧎🏼‍♀️🧑🏼‍🦯👨🏼‍🦯👩🏼‍🦯🧑🏼‍🦼👨🏼‍🦼👩🏼‍🦼🧑🏼‍🦽👨🏼‍🦽👩🏼‍🦽🏃🏼🏃🏼‍♂️🏃🏼‍♀️💃🏼🕺🏼🕴🏼🧖🏼🧖🏼‍♂️🧖🏼‍♀️🧗🏼🧗🏼‍♂️🧗🏼‍♀️🏇🏼🏂🏼🏌🏼🏌🏼‍♂️🏌🏼‍♀️🏄🏼🏄🏼‍♂️🏄🏼‍♀️🚣🏼🚣🏼‍♂️🚣🏼‍♀️🏊🏼🏊🏼‍♂️🏊🏼‍♀️⛹🏼⛹🏼‍♂️⛹🏼‍♀️🏋🏼🏋🏼‍♂️🏋🏼‍♀️🚴🏼🚴🏼‍♂️🚴🏼‍♀️🚵🏼🚵🏼‍♂️🚵🏼‍♀️🤸🏼🤸🏼‍♂️🤸🏼‍♀️🤽🏼🤽🏼‍♂️🤽🏼‍♀️🤾🏼🤾🏼‍♂️🤾🏼‍♀️🤹🏼🤹🏼‍♂️🤹🏼‍♀️🧘🏼🧘🏼‍♂️🧘🏼‍♀️🛀🏼🛌🏼🧑🏼‍🤝‍🧑🏼👬🏼👭🏼👫🏼👋🏽🤚🏽🖐🏽✋🏽🖖🏽👌🏽🤏🏽✌🏽🤞🏽🤟🏽🤘🏽🤙🏽👈🏽👉🏽👆🏽🖕🏽👇🏽☝🏽👍🏽👎🏽✊🏽👊🏽🤛🏽🤜🏽👏🏽🙌🏽👐🏽🤲🏽🙏🏽✍🏽💅🏽🤳🏽💪🏽🦵🏽🦶🏽👂🏽🦻🏽👃🏽👶🏽🧒🏽👦🏽👧🏽🧑🏽👨🏽👩🏽🧑🏽‍🦱👨🏽‍🦱👩🏽‍🦱🧑🏽‍🦰👨🏽‍🦰👩🏽‍🦰👱🏽👱🏽‍♂️👱🏽‍♀️🧑🏽‍🦳👨🏽‍🦳👩🏽‍🦳🧑🏽‍🦲👨🏽‍🦲👩🏽‍🦲🧔🏽🧓🏽👴🏽👵🏽🙍🏽🙍🏽‍♂️🙍🏽‍♀️🙎🏽🙎🏽‍♂️🙎🏽‍♀️🙅🏽🙅🏽‍♂️🙅🏽‍♀️🙆🏽🙆🏽‍♂️🙆🏽‍♀️💁🏽💁🏽‍♂️💁🏽‍♀️🙋🏽🙋🏽‍♂️🙋🏽‍♀️🧏🏽🧏🏽‍♂️🧏🏽‍♀️🙇🏽🙇🏽‍♂️🙇🏽‍♀️🤦🏽🤦🏽‍♂️🤦🏽‍♀️🤷🏽🤷🏽‍♂️🤷🏽‍♀️🧑🏽‍⚕️👨🏽‍⚕️👩🏽‍⚕️🧑🏽‍🎓👨🏽‍🎓👩🏽‍🎓🧑🏽‍🏫👨🏽‍🏫👩🏽‍🏫🧑🏽‍⚖️👨🏽‍⚖️👩🏽‍⚖️🧑🏽‍🌾👨🏽‍🌾👩🏽‍🌾🧑🏽‍🍳👨🏽‍🍳👩🏽‍🍳🧑🏽‍🔧👨🏽‍🔧👩🏽‍🔧🧑🏽‍🏭👨🏽‍🏭👩🏽‍🏭🧑🏽‍💼👨🏽‍💼👩🏽‍💼🧑🏽‍🔬👨🏽‍🔬👩🏽‍🔬🧑🏽‍💻👨🏽‍💻👩🏽‍💻🧑🏽‍🎤👨🏽‍🎤👩🏽‍🎤🧑🏽‍🎨👨🏽‍🎨👩🏽‍🎨🧑🏽‍✈️👨🏽‍✈️👩🏽‍✈️🧑🏽‍🚀👨🏽‍🚀👩🏽‍🚀🧑🏽‍🚒👨🏽‍🚒👩🏽‍🚒👮🏽👮🏽‍♂️👮🏽‍♀️🕵🏽🕵🏽‍♂️🕵🏽‍♀️💂🏽💂🏽‍♂️💂🏽‍♀️👷🏽👷🏽‍♂️👷🏽‍♀️🤴🏽👸🏽👳🏽👳🏽‍♂️👳🏽‍♀️👲🏽🧕🏽🤵🏽👰🏽🤰🏽🤱🏽👼🏽🎅🏽🤶🏽🦸🏽🦸🏽‍♂️🦸🏽‍♀️🦹🏽🦹🏽‍♂️🦹🏽‍♀️🧙🏽🧙🏽‍♂️🧙🏽‍♀️🧚🏽🧚🏽‍♂️🧚🏽‍♀️🧛🏽🧛🏽‍♂️🧛🏽‍♀️🧜🏽🧜🏽‍♂️🧜🏽‍♀️🧝🏽🧝🏽‍♂️🧝🏽‍♀️💆🏽💆🏽‍♂️💆🏽‍♀️💇🏽💇🏽‍♂️💇🏽‍♀️🚶🏽🚶🏽‍♂️🚶🏽‍♀️🧍🏽🧍🏽‍♂️🧍🏽‍♀️🧎🏽🧎🏽‍♂️🧎🏽‍♀️🧑🏽‍🦯👨🏽‍🦯👩🏽‍🦯🧑🏽‍🦼👨🏽‍🦼👩🏽‍🦼🧑🏽‍🦽👨🏽‍🦽👩🏽‍🦽🏃🏽🏃🏽‍♂️🏃🏽‍♀️💃🏽🕺🏽🕴🏽🧖🏽🧖🏽‍♂️🧖🏽‍♀️🧗🏽🧗🏽‍♂️🧗🏽‍♀️🏇🏽🏂🏽🏌🏽🏌🏽‍♂️🏌🏽‍♀️🏄🏽🏄🏽‍♂️🏄🏽‍♀️🚣🏽🚣🏽‍♂️🚣🏽‍♀️🏊🏽🏊🏽‍♂️🏊🏽‍♀️⛹🏽⛹🏽‍♂️⛹🏽‍♀️🏋🏽🏋🏽‍♂️🏋🏽‍♀️🚴🏽🚴🏽‍♂️🚴🏽‍♀️🚵🏽🚵🏽‍♂️🚵🏽‍♀️🤸🏽🤸🏽‍♂️🤸🏽‍♀️🤽🏽🤽🏽‍♂️🤽🏽‍♀️🤾🏽🤾🏽‍♂️🤾🏽‍♀️🤹🏽🤹🏽‍♂️🤹🏽‍♀️🧘🏽🧘🏽‍♂️🧘🏽‍♀️🛀🏽🛌🏽🧑🏽‍🤝‍🧑🏽👬🏽👭🏽👫🏽👋🏾🤚🏾🖐🏾✋🏾🖖🏾👌🏾🤏🏾✌🏾🤞🏾🤟🏾🤘🏾🤙🏾👈🏾👉🏾👆🏾🖕🏾👇🏾☝🏾👍🏾👎🏾✊🏾👊🏾🤛🏾🤜🏾👏🏾🙌🏾👐🏾🤲🏾🙏🏾✍🏾💅🏾🤳🏾💪🏾🦵🏾🦶🏾👂🏾🦻🏾👃🏾👶🏾🧒🏾👦🏾👧🏾🧑🏾👨🏾👩🏾🧑🏾‍🦱👨🏾‍🦱👩🏾‍🦱🧑🏾‍🦰👨🏾‍🦰👩🏾‍🦰👱🏾👱🏾‍♂️👱🏾‍♀️🧑🏾‍🦳👨🏾‍🦳👩🏾‍🦳🧑🏾‍🦲👨🏾‍🦲👩🏾‍🦲🧔🏾🧓🏾👴🏾👵🏾🙍🏾🙍🏾‍♂️🙍🏾‍♀️🙎🏾🙎🏾‍♂️🙎🏾‍♀️🙅🏾🙅🏾‍♂️🙅🏾‍♀️🙆🏾🙆🏾‍♂️🙆🏾‍♀️💁🏾💁🏾‍♂️💁🏾‍♀️🙋🏾🙋🏾‍♂️🙋🏾‍♀️🧏🏾🧏🏾‍♂️🧏🏾‍♀️🙇🏾🙇🏾‍♂️🙇🏾‍♀️🤦🏾🤦🏾‍♂️🤦🏾‍♀️🤷🏾🤷🏾‍♂️🤷🏾‍♀️🧑🏾‍⚕️👨🏾‍⚕️👩🏾‍⚕️🧑🏾‍🎓👨🏾‍🎓👩🏾‍🎓🧑🏾‍🏫👨🏾‍🏫👩🏾‍🏫🧑🏾‍⚖️👨🏾‍⚖️👩🏾‍⚖️🧑🏾‍🌾👨🏾‍🌾👩🏾‍🌾🧑🏾‍🍳👨🏾‍🍳👩🏾‍🍳🧑🏾‍🔧👨🏾‍🔧👩🏾‍🔧🧑🏾‍🏭👨🏾‍🏭👩🏾‍🏭🧑🏾‍💼👨🏾‍💼👩🏾‍💼🧑🏾‍🔬👨🏾‍🔬👩🏾‍🔬🧑🏾‍💻👨🏾‍💻👩🏾‍💻🧑🏾‍🎤👨🏾‍🎤👩🏾‍🎤🧑🏾‍🎨👨🏾‍🎨👩🏾‍🎨🧑🏾‍✈️👨🏾‍✈️👩🏾‍✈️🧑🏾‍🚀👨🏾‍🚀👩🏾‍🚀🧑🏾‍🚒👨🏾‍🚒👩🏾‍🚒👮🏾👮🏾‍♂️👮🏾‍♀️🕵🏾🕵🏾‍♂️🕵🏾‍♀️💂🏾💂🏾‍♂️💂🏾‍♀️👷🏾👷🏾‍♂️👷🏾‍♀️🤴🏾👸🏾👳🏾👳🏾‍♂️👳🏾‍♀️👲🏾🧕🏾🤵🏾👰🏾🤰🏾🤱🏾👼🏾🎅🏾🤶🏾🦸🏾🦸🏾‍♂️🦸🏾‍♀️🦹🏾🦹🏾‍♂️🦹🏾‍♀️🧙🏾🧙🏾‍♂️🧙🏾‍♀️🧚🏾🧚🏾‍♂️🧚🏾‍♀️🧛🏾🧛🏾‍♂️🧛🏾‍♀️🧜🏾🧜🏾‍♂️🧜🏾‍♀️🧝🏾🧝🏾‍♂️🧝🏾‍♀️💆🏾💆🏾‍♂️💆🏾‍♀️💇🏾💇🏾‍♂️💇🏾‍♀️🚶🏾🚶🏾‍♂️🚶🏾‍♀️🧍🏾🧍🏾‍♂️🧍🏾‍♀️🧎🏾🧎🏾‍♂️🧎🏾‍♀️🧑🏾‍🦯👨🏾‍🦯👩🏾‍🦯🧑🏾‍🦼👨🏾‍🦼👩🏾‍🦼🧑🏾‍🦽👨🏾‍🦽👩🏾‍🦽🏃🏾🏃🏾‍♂️🏃🏾‍♀️💃🏾🕺🏾🕴🏾🧖🏾🧖🏾‍♂️🧖🏾‍♀️🧗🏾🧗🏾‍♂️🧗🏾‍♀️🏇🏾🏂🏾🏌🏾🏌🏾‍♂️🏌🏾‍♀️🏄🏾🏄🏾‍♂️🏄🏾‍♀️🚣🏾🚣🏾‍♂️🚣🏾‍♀️🏊🏾🏊🏾‍♂️🏊🏾‍♀️⛹🏾⛹🏾‍♂️⛹🏾‍♀️🏋🏾🏋🏾‍♂️🏋🏾‍♀️🚴🏾🚴🏾‍♂️🚴🏾‍♀️🚵🏾🚵🏾‍♂️🚵🏾‍♀️🤸🏾🤸🏾‍♂️🤸🏾‍♀️🤽🏾🤽🏾‍♂️🤽🏾‍♀️🤾🏾🤾🏾‍♂️🤾🏾‍♀️🤹🏾🤹🏾‍♂️🤹🏾‍♀️🧘🏾🧘🏾‍♂️🧘🏾‍♀️🛀🏾🛌🏾🧑🏾‍🤝‍🧑🏾👬🏾👭🏾👫🏾👋🏿🤚🏿🖐🏿✋🏿🖖🏿👌🏿🤏🏿✌🏿🤞🏿🤟🏿🤘🏿🤙🏿👈🏿👉🏿👆🏿🖕🏿👇🏿☝🏿👍🏿👎🏿✊🏿👊🏿🤛🏿🤜🏿👏🏿🙌🏿👐🏿🤲🏿🙏🏿✍🏿💅🏿🤳🏿💪🏿🦵🏿🦶🏿👂🏿🦻🏿👃🏿👶🏿🧒🏿👦🏿👧🏿🧑🏿👨🏿👩🏿🧑🏿‍🦱👨🏿‍🦱👩🏿‍🦱🧑🏿‍🦰👨🏿‍🦰👩🏿‍🦰👱🏿👱🏿‍♂️👱🏿‍♀️🧑🏿‍🦳👨🏿‍🦳👩🏿‍🦳🧑🏿‍🦲👨🏿‍🦲👩🏿‍🦲🧔🏿🧓🏿👴🏿👵🏿🙍🏿🙍🏿‍♂️🙍🏿‍♀️🙎🏿🙎🏿‍♂️🙎🏿‍♀️🙅🏿🙅🏿‍♂️🙅🏿‍♀️🙆🏿🙆🏿‍♂️🙆🏿‍♀️💁🏿💁🏿‍♂️💁🏿‍♀️🙋🏿🙋🏿‍♂️🙋🏿‍♀️🧏🏿🧏🏿‍♂️🧏🏿‍♀️🙇🏿🙇🏿‍♂️🙇🏿‍♀️🤦🏿🤦🏿‍♂️🤦🏿‍♀️🤷🏿🤷🏿‍♂️🤷🏿‍♀️🧑🏿‍⚕️👨🏿‍⚕️👩🏿‍⚕️🧑🏿‍🎓👨🏿‍🎓👩🏿‍🎓🧑🏿‍🏫👨🏿‍🏫👩🏿‍🏫🧑🏿‍⚖️👨🏿‍⚖️👩🏿‍⚖️🧑🏿‍🌾👨🏿‍🌾👩🏿‍🌾🧑🏿‍🍳👨🏿‍🍳👩🏿‍🍳🧑🏿‍🔧👨🏿‍🔧👩🏿‍🔧🧑🏿‍🏭👨🏿‍🏭👩🏿‍🏭🧑🏿‍💼👨🏿‍💼👩🏿‍💼🧑🏿‍🔬👨🏿‍🔬👩🏿‍🔬🧑🏿‍💻👨🏿‍💻👩🏿‍💻🧑🏿‍🎤👨🏿‍🎤👩🏿‍🎤🧑🏿‍🎨👨🏿‍🎨👩🏿‍🎨🧑🏿‍✈️👨🏿‍✈️👩🏿‍✈️🧑🏿‍🚀👨🏿‍🚀👩🏿‍🚀🧑🏿‍🚒👨🏿‍🚒👩🏿‍🚒👮🏿👮🏿‍♂️👮🏿‍♀️🕵🏿🕵🏿‍♂️🕵🏿‍♀️💂🏿💂🏿‍♂️💂🏿‍♀️👷🏿👷🏿‍♂️👷🏿‍♀️🤴🏿👸🏿👳🏿👳🏿‍♂️👳🏿‍♀️👲🏿🧕🏿🤵🏿👰🏿🤰🏿🤱🏿👼🏿🎅🏿🤶🏿🦸🏿🦸🏿‍♂️🦸🏿‍♀️🦹🏿🦹🏿‍♂️🦹🏿‍♀️🧙🏿🧙🏿‍♂️🧙🏿‍♀️🧚🏿🧚🏿‍♂️🧚🏿‍♀️🧛🏿🧛🏿‍♂️🧛🏿‍♀️🧜🏿🧜🏿‍♂️🧜🏿‍♀️🧝🏿🧝🏿‍♂️🧝🏿‍♀️💆🏿💆🏿‍♂️💆🏿‍♀️💇🏿💇🏿‍♂️💇🏿‍♀️🚶🏿🚶🏿‍♂️🚶🏿‍♀️🧍🏿🧍🏿‍♂️🧍🏿‍♀️🧎🏿🧎🏿‍♂️🧎🏿‍♀️🧑🏿‍🦯👨🏿‍🦯👩🏿‍🦯🧑🏿‍🦼👨🏿‍🦼👩🏿‍🦼🧑🏿‍🦽👨🏿‍🦽👩🏿‍🦽🏃🏿🏃🏿‍♂️🏃🏿‍♀️💃🏿🕺🏿🕴🏿🧖🏿🧖🏿‍♂️🧖🏿‍♀️🧗🏿🧗🏿‍♂️🧗🏿‍♀️🏇🏿🏂🏿🏌🏿🏌🏿‍♂️🏌🏿‍♀️🏄🏿🏄🏿‍♂️🏄🏿‍♀️🚣🏿🚣🏿‍♂️🚣🏿‍♀️🏊🏿🏊🏿‍♂️🏊🏿‍♀️⛹🏿⛹🏿‍♂️⛹🏿‍♀️🏋🏿🏋🏿‍♂️🏋🏿‍♀️🚴🏿🚴🏿‍♂️🚴🏿‍♀️🚵🏿🚵🏿‍♂️🚵🏿‍♀️🤸🏿🤸🏿‍♂️🤸🏿‍♀️🤽🏿🤽🏿‍♂️🤽🏿‍♀️🤾🏿🤾🏿‍♂️🤾🏿‍♀️🤹🏿🤹🏿‍♂️🤹🏿‍♀️🧘🏿🧘🏿‍♂️🧘🏿‍♀️🛀🏿🛌🏿🧑🏿‍🤝‍🧑🏿👬🏿👭🏿👫🏿🐶🐱🐭🐹🐰🦊🐻🐼🐨🐯🦁🐮🐷🐽🐸🐵🙈🙉🙊🐒🐔🐧🐦🐤🐣🐥🦆🦅🦉🦇🐺🐗🐴🦄🐝🐛🦋🐌🐞🐜🦟🦗🕷🕸🦂🐢🐍🦎🦖🦕🐙🦑🦐🦞🦀🐡🐠🐟🐬🐳🐋🦈🐊🐅🐆🦓🦍🦧🐘🦛🦏🐪🐫🦒🦘🐃🐂🐄🐎🐖🐏🐑🦙🐐🦌🐕🐩🦮🐕‍🦺🐈🐓🦃🦚🦜🦢🦩🕊🐇🦝🦨🦡🦦🦥🐁🐀🐿🦔🐾🐉🐲🌵🎄🌲🌳🌴🌱🌿☘️🍀🎍🎋🍃🍂🍁🍄🐚🌾💐🌷🌹🥀🌺🌸🌼🌻🌞🌝🌛🌜🌚🌕🌖🌗🌘🌑🌒🌓🌔🌙🌎🌍🌏🪐💫⭐️🌟✨⚡️☄️💥🔥🌪🌈☀️🌤⛅️🌥☁️🌦🌧⛈🌩🌨❄️☃️⛄️🌬💨💧💦☔️☂️🌊🌫🍏🍎🍐🍊🍋🍌🍉🍇🍓🍈🍒🍑🥭🍍🥥🥝🍅🍆🥑🥦🥬🥒🌶🌽🥕🧄🧅🥔🍠🥐🥯🍞🥖🥨🧀🥚🍳🧈🥞🧇🥓🥩🍗🍖🦴🌭🍔🍟🍕🥪🥙🧆🌮🌯🥗🥘🥫🍝🍜🍲🍛🍣🍱🥟🦪🍤🍙🍚🍘🍥🥠🥮🍢🍡🍧🍨🍦🥧🧁🍰🎂🍮🍭🍬🍫🍿🍩🍪🌰🥜🍯🥛🍼☕️🍵🧃🥤🍶🍺🍻🥂🍷🥃🍸🍹🧉🍾🧊🥄🍴🍽🥣🥡🥢🧂⚽️🏀🏈⚾️🥎🎾🏐🏉🥏🎱🪀🏓🏸🏒🏑🥍🏏🥅⛳️🪁🏹🎣🤿🥊🥋🎽🛹🛷⛸🥌🎿⛷🏂🪂🏋️🏋️‍♂️🏋️‍♀️🤼🤼‍♂️🤼‍♀️🤸‍♀️🤸🤸‍♂️⛹️⛹️‍♂️⛹️‍♀️🤺🤾🤾‍♂️🤾‍♀️🏌️🏌️‍♂️🏌️‍♀️🏇🧘🧘‍♂️🧘‍♀️🏄🏄‍♂️🏄‍♀️🏊🏊‍♂️🏊‍♀️🤽🤽‍♂️🤽‍♀️🚣🚣‍♂️🚣‍♀️🧗🧗‍♂️🧗‍♀️🚵🚵‍♂️🚵‍♀️🚴🚴‍♂️🚴‍♀️🏆🥇🥈🥉🏅🎖🏵🎗🎫🎟🎪🤹🤹‍♂️🤹‍♀️🎭🩰🎨🎬🎤🎧🎼🎹🥁🎷🎺🎸🪕🎻🎲♟🎯🎳🎮🎰🧩🚗🚕🚙🚌🚎🏎🚓🚑🚒🚐🚚🚛🚜🦯🦽🦼🛴🚲🛵🏍🛺🚨🚔🚍🚘🚖🚡🚠🚟🚃🚋🚞🚝🚄🚅🚈🚂🚆🚇🚊🚉✈️🛫🛬🛩💺🛰🚀🛸🚁🛶⛵️🚤🛥🛳⛴🚢⚓️⛽️🚧🚦🚥🚏🗺🗿🗽🗼🏰🏯🏟🎡🎢🎠⛲️⛱🏖🏝🏜🌋⛰🏔🗻🏕⛺️🏠🏡🏘🏚🏗🏭🏢🏬🏣🏤🏥🏦🏨🏪🏫🏩💒🏛⛪️🕌🕍🛕🕋⛩🛤🛣🗾🎑🏞🌅🌄🌠🎇🎆🌇🌆🏙🌃🌌🌉🌁⌚️📱📲💻⌨️🖥🖨🖱🖲🕹🗜💽💾💿📀📼📷📸📹🎥📽🎞📞☎️📟📠📺📻🎙🎚🎛🧭⏱⏲⏰🕰⌛️⏳📡🔋🔌💡🔦🕯🪔🧯🛢💸💵💴💶💷💰💳💎⚖️🧰🔧🔨⚒🛠⛏🔩⚙️🧱⛓🧲🔫💣🧨🪓🔪🗡⚔️🛡🚬⚰️⚱️🏺🔮📿🧿💈⚗️🔭🔬🕳🩹🩺💊💉🩸🧬🦠🧫🧪🌡🧹🧺🧻🚽🚰🚿🛁🛀🧼🪒🧽🧴🛎🔑🗝🚪🪑🛋🛏🛌🧸🖼🛍🛒🎁🎈🎏🎀🎊🎉🎎🏮🎐🧧✉️📩📨📧💌📥📤📦🏷📪📫📬📭📮📯📜📃📄📑🧾📊📈📉🗒🗓📆📅🗑📇🗃🗳🗄📋📁📂🗂🗞📰📓📔📒📕📗📘📙📚📖🔖🧷🔗📎🖇📐📏🧮📌📍✂️🖊🖋✒️🖌🖍📝✏️🔍🔎🔏🔐🔒🔓❤️🧡💛💚💙💜🖤🤍🤎💔❣️💕💞💓💗💖💘💝💟☮️✝️☪️🕉☸️✡️🔯🕎☯️☦️🛐⛎♈️♉️♊️♋️♌️♍️♎️♏️♐️♑️♒️♓️🆔⚛️🉑☢️☣️📴📳🈶🈚️🈸🈺🈷️✴️🆚💮🉐㊙️㊗️🈴🈵🈹🈲🅰️🅱️🆎🆑🅾️🆘❌⭕️🛑⛔️📛🚫💯💢♨️🚷🚯🚳🚱🔞📵🚭❗️❕❓❔‼️⁉️🔅🔆〽️⚠️🚸🔱⚜️🔰♻️✅🈯️💹❇️✳️❎🌐💠Ⓜ️🌀💤🏧🚾♿️🅿️🈳🈂️🛂🛃🛄🛅🚹🚺🚼🚻🚮🎦📶🈁🔣ℹ️🔤🔡🔠🆖🆗🆙🆒🆕🆓0️⃣1️⃣2️⃣3️⃣4️⃣5️⃣6️⃣7️⃣8️⃣9️⃣🔟🔢#️⃣*️⃣⏏️▶️⏸⏯⏹⏺⏭⏮⏩⏪⏫⏬◀️🔼🔽➡️⬅️⬆️⬇️↗️↘️↙️↖️↕️↔️↪️↩️⤴️⤵️🔀🔁🔂🔄🔃🎵🎶➕➖➗✖️♾💲💱™️©️®️〰️➰➿🔚🔙🔛🔝🔜✔️☑️🔘🔴🟠🟡🟢🔵🟣⚫️⚪️🟤🔺🔻🔸🔹🔶🔷🔳🔲▪️▫️◾️◽️◼️◻️🟥🟧🟨🟩🟦🟪⬛️⬜️🟫🔈🔇🔉🔊🔔🔕📣📢👁‍🗨💬💭🗯♠️♣️♥️♦️🃏🎴🀄️🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚🕛🕜🕝🕞🕟🕠🕡🕢🕣🕤🕥🕦🕧🏳️🏴🏁🚩🏳️‍🌈🏴‍☠️🇦🇫🇦🇽🇦🇱🇩🇿🇦🇸🇦🇩🇦🇴🇦🇮🇦🇶🇦🇬🇦🇷🇦🇲🇦🇼🇦🇺🇦🇹🇦🇿🇧🇸🇧🇭🇧🇩🇧🇧🇧🇾🇧🇪🇧🇿🇧🇯🇧🇲🇧🇹🇧🇴🇧🇦🇧🇼🇧🇷🇮🇴🇻🇬🇧🇳🇧🇬🇧🇫🇧🇮🇰🇭🇨🇲🇨🇦🇮🇨🇨🇻🇧🇶🇰🇾🇨🇫🇹🇩🇨🇱🇨🇳🇨🇽🇨🇨🇨🇴🇰🇲🇨🇬🇨🇩🇨🇰🇨🇷🇨🇮🇭🇷🇨🇺🇨🇼🇨🇾🇨🇿🇩🇰🇩🇯🇩🇲🇩🇴🇪🇨🇪🇬🇸🇻🇬🇶🇪🇷🇪🇪🇪🇹🇪🇺🇫🇰🇫🇴🇫🇯🇫🇮🇫🇷🇬🇫🇵🇫🇹🇫🇬🇦🇬🇲🇬🇪🇩🇪🇬🇭🇬🇮🇬🇷🇬🇱🇬🇩🇬🇵🇬🇺🇬🇹🇬🇬🇬🇳🇬🇼🇬🇾🇭🇹🇭🇳🇭🇰🇭🇺🇮🇸🇮🇳🇮🇩🇮🇷🇮🇶🇮🇪🇮🇲🇮🇱🇮🇹🇯🇲🇯🇵🎌🇯🇪🇯🇴🇰🇿🇰🇪🇰🇮🇽🇰🇰🇼🇰🇬🇱🇦🇱🇻🇱🇧🇱🇸🇱🇷🇱🇾🇱🇮🇱🇹🇱🇺🇲🇴🇲🇰🇲🇬🇲🇼🇲🇾🇲🇻🇲🇱🇲🇹🇲🇭🇲🇶🇲🇷🇲🇺🇾🇹🇲🇽🇫🇲🇲🇩🇲🇨🇲🇳🇲🇪🇲🇸🇲🇦🇲🇿🇲🇲🇳🇦🇳🇷🇳🇵🇳🇱🇳🇨🇳🇿🇳🇮🇳🇪🇳🇬🇳🇺🇳🇫🇰🇵🇲🇵🇳🇴🇴🇲🇵🇰🇵🇼🇵🇸🇵🇦🇵🇬🇵🇾🇵🇪🇵🇭🇵🇳🇵🇱🇵🇹🇵🇷🇶🇦🇷🇪🇷🇴🇷🇺🇷🇼🇼🇸🇸🇲🇸🇦🇸🇳🇷🇸🇸🇨🇸🇱🇸🇬🇸🇽🇸🇰🇸🇮🇬🇸🇸🇧🇸🇴🇿🇦🇰🇷🇸🇸🇪🇸🇱🇰🇧🇱🇸🇭🇰🇳🇱🇨🇵🇲🇻🇨🇸🇩🇸🇷🇸🇿🇸🇪🇨🇭🇸🇾🇹🇼🇹🇯🇹🇿🇹🇭🇹🇱🇹🇬🇹🇰🇹🇴🇹🇹🇹🇳🇹🇷🇹🇲🇹🇨🇹🇻🇻🇮🇺🇬🇺🇦🇦🇪🇬🇧🏴󠁧󠁢󠁥󠁮󠁧󠁿🏴󠁧󠁢󠁳󠁣󠁴󠁿🏴󠁧󠁢󠁷󠁬󠁳󠁿🇺🇳🇺🇸🇺🇾🇺🇿🇻🇺🇻🇦🇻🇪🇻🇳🇼🇫🇪🇭🇾🇪🇿🇲🇿🇼";

        public abstract char Character { get; }

        public abstract string Description { get; }
        public abstract Type Type { get; }

        public static IEnumerable<ParameterType> KnownParameterTypes => ParameterTypes.Values;
        private static ConcurrentDictionary<Type, ParameterType> ParameterTypes { get; } = new ConcurrentDictionary<Type, ParameterType>();

        static ParameterType()
        {
            ParameterType boolParameter = new ParameterType<bool>('b', "true or false", ParameterAsBool);
            ParameterTypes.TryAdd(boolParameter.Type, boolParameter);
            ParameterType intParameter = new ParameterType<int>('n', "a number", ParameterAsInt);
            ParameterTypes.TryAdd(intParameter.Type, intParameter);
            ParameterType ulongParameter = new ParameterType<ulong>('p', "a positive number", ParameterAsULong);
            ParameterTypes.TryAdd(ulongParameter.Type, ulongParameter);
            ParameterType stringParameter = new ParameterType<string>('t', "a text to the end of the message o surrounded with  like \"this\"",
                ParametersAsString);
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
            ParameterType dateParameter = new ParameterType<DateTime>('d', "a date in format 30d24h59m59s (also 40d2s)", ParameterAsDate);
            ParameterTypes.TryAdd(dateParameter.Type, dateParameter);
            ParameterType channelParameter = new ParameterType<IMessageChannel>('c', "a text channel mention", ParameterAsMessageChannel);
            ParameterTypes.TryAdd(channelParameter.Type, channelParameter);
            ParameterType settingsParameter = new ParameterType<Settings>();
            ParameterTypes.TryAdd(settingsParameter.Type, settingsParameter);
            ParameterType messageParameter = new ParameterType<SocketMessage>();
            ParameterTypes.TryAdd(messageParameter.Type, messageParameter);
            ParameterType socketChannelParameter = new ParameterType<ISocketMessageChannel>();
            ParameterTypes.TryAdd(socketChannelParameter.Type, socketChannelParameter);
        }

        protected abstract object InterpretParameter(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, DefaultValue<object> value,
            Optional<object> min, Optional<object> max, bool throwOutOfRange);

        [SuppressMessage("ReSharper", "InvertIf")]
        private static T CheckMinMax<T>(T value, int index, Optional<T> min, Optional<T> max, bool throwOutOfRange)
            where T : IComparable
        {
            if (min.IsSpecified)
            {
                if (value.CompareTo(min.Value) < 0)
                {
                    if (throwOutOfRange)
                    {
                        throw new KudosArgumentOutOfRangeException($"parameter {index} must be bigger than {min.Value}");
                    }
                    return min.Value;
                }
            }
            if (max.IsSpecified)
            {
                if (value.CompareTo(max.Value) > 0)
                {
                    if (throwOutOfRange)
                    {
                        throw new KudosArgumentOutOfRangeException($"parameter {index} must be smaller than {max.Value}");
                    }
                    return max.Value;
                }
            }

            return value;
        }

        public static ParameterType FromType(Type type)
        {
            if (ParameterTypes.ContainsKey(type))
            {
                return ParameterTypes[type];
            }
            foreach ((Type key, ParameterType value) in ParameterTypes)
            {
                if (type.IsAssignableFrom(key))
                {
                    return value;
                }
            }

            throw new KudosInternalException($"Unknown ParameterType ({type})");
        }

        public static T InterpretParameter<T>(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue, Optional<T> min,
            Optional<T> max, bool throwOutOfRange) =>
            ((ParameterType<T>)FromType(typeof(T))).ParameterInterpreter.Invoke(parameters, indexLess, index, optional, defaultValue, min, max,
                throwOutOfRange);

        public static object InterpretParameter(Type type, string[] parameters, IEnumerable<object> indexLess, int index, bool optional,
            DefaultValue<object> defaultValue, Optional<object> min, Optional<object> max, bool throwOutOfRange) =>
            FromType(type)
            .InterpretParameter(parameters, indexLess, index, optional, defaultValue, min, max,
                throwOutOfRange);

        private static bool ParameterAsBool(string[] parameters, bool indexLess, int index, bool optional, DefaultValue<bool> defaultValue, Optional<bool> min,
            Optional<bool> max, bool throwOutOfRange)
        {
            if (ParameterPresent(parameters, index) && bool.TryParse(parameters[index], out bool value))
            {
                return value;
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be true or false (bool)");
            }
            return value;
        }
        private static DateTime ParameterAsDate(string[] parameters, DateTime indexLess, int index, bool optional, DefaultValue<DateTime> defaultValue, Optional<DateTime> min,
            Optional<DateTime> max, bool throwOutOfRange)
        {


            if (ParameterPresent(parameters, index) && DateTime.TryParse(parameters[index], out DateTime value))
            {
                return CheckMinMax(value, index, min, max, throwOutOfRange);
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a date");
            }
            return value;
        }

        private static IEmote ParameterAsEmote(string[] parameters, IEmote indexLess, int index, bool optional, DefaultValue<IEmote> defaultValue,
            Optional<IEmote> min, Optional<IEmote> max, bool throwOutOfRange)
        {
            if (ParameterPresent(parameters, index) && Emote.TryParse(parameters[index], out Emote value))
            {
                return value;
            }
            if (Emojis.Contains(parameters[index]))
            {
                return new Emoji(parameters[index]);
            }
            if (!optional)
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be an emoji");
            }
            SetToDefaultValue(out IEmote emote, defaultValue, indexLess);
            return emote;
        }

        private static int ParameterAsInt(string[] parameters, int indexLess, int index, bool optional, DefaultValue<int> defaultValue, Optional<int> min,
            Optional<int> max, bool throwOutOfRange)
        {
            if (ParameterPresent(parameters, index) && int.TryParse(parameters[index], out int value))
            {
                return CheckMinMax(value, index, min, max, throwOutOfRange);
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a number (int)");
            }
            return value;
        }
        private static IMessageChannel ParameterAsMessageChannel(string[] parameters, IMessageChannel indexLess, int index, bool optional,
            DefaultValue<IMessageChannel> defaultValue, Optional<IMessageChannel> min, Optional<IMessageChannel> max, bool throwOutOfRange)
        {
            IMessageChannel channel;
            if (ParameterPresent(parameters, index))
            {
                channel = parameters[index].ChannelFromMention();
                if (channel != null)
                {
                    return channel;
                }
            }
            if (!optional)
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a mentioned text channel");
            }

            SetToDefaultValue(out channel, defaultValue, indexLess);
            return channel;
        }
        private static SocketRole ParameterAsSocketRole(string[] parameters, SocketRole indexLess, int index, bool optional,
            DefaultValue<SocketRole> defaultValue, Optional<SocketRole> min, Optional<SocketRole> max, bool throwOutOfRange)
        {
            SocketRole role;
            if (ParameterPresent(parameters, index))
            {
                role = parameters[index].RoleFromMention();
                if (role != null)
                {
                    return role;
                }
            }
            if (!optional)
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a mentioned role");
            }

            SetToDefaultValue(out role, defaultValue, indexLess);
            return role;
        }

        private static SocketUser ParameterAsSocketUser(string[] parameters, SocketUser indexLess, int index, bool optional,
            DefaultValue<SocketUser> defaultValue, Optional<SocketUser> min, Optional<SocketUser> max, bool throwOutOfRange)
        {
            SocketUser user;
            if (ParameterPresent(parameters, index))
            {
                user = parameters[index].FromMention();
                if (user != null)
                {
                    return user;
                }

                string[] userData = parameters[index].Split("#");
                if (userData.Length == 2)
                {
                    user = Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
                }
                if (user != null)
                {
                    return user;
                }
            }
            if (!optional)
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a user (described in help)");
            }

            SetToDefaultValue(out user, defaultValue, indexLess);
            return user;
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static TimeSpan ParameterAsTimespan(string[] parameters, TimeSpan indexLess, int index, bool optional, DefaultValue<TimeSpan> defaultValue,
            Optional<TimeSpan> min, Optional<TimeSpan> max, bool throwOutOfRange)
        {
            string[] formats = {
                "d'd'", "h'h'", "m'm'", "s's'", "d'd'h'h'", "d'd'm'm'", "d'd's's'", "h'h'm'm'", "h'h's's'", "m'm's's'", "d'd'h'h'm'm'", "d'd'h'h's's'",
                "d'd'm'm's's'", "h'h'm'm's's'", "d'd'h'h'm'm's's'"
            };
            if (ParameterPresent(parameters, index) && TimeSpan.TryParseExact(parameters[index], formats, null, out TimeSpan value))
            {
                return CheckMinMax(value, index, min, max, throwOutOfRange);
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a timespan");
            }
            return value;
        }

        private static ulong ParameterAsULong(string[] parameters, ulong indexLess, int index, bool optional, DefaultValue<ulong> defaultValue,
            Optional<ulong> min, Optional<ulong> max, bool throwOutOfRange)
        {
            if (ParameterPresent(parameters, index) && ulong.TryParse(parameters[index], out ulong value))
            {
                return CheckMinMax(value, index, min, max, throwOutOfRange);
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a number (ulong)");
            }
            return value;
        }

        private static Word ParameterAsWord(string[] parameters, Word indexLess, int index, bool optional, DefaultValue<Word> defaultValue, Optional<Word> min,
            Optional<Word> max, bool throwOutOfRange)
        {
            Word value;
            if (ParameterPresent(parameters, index) && (value = Word.Create(parameters[index])) != null)
            {
                return value;
            }
            if (optional)
            {
                SetToDefaultValue(out value, defaultValue, indexLess);
            }
            else
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a word (a-z)");
            }
            return value;
        }

        private static bool ParameterPresent(IReadOnlyList<string> parameters, int index) =>
            parameters.Count > index && !string.IsNullOrEmpty(parameters[index]) && parameters[index] != "-";

        private static string ParametersAsString(string[] parameters, string indexLess, int index, bool optional, DefaultValue<string> defaultValue,
            Optional<string> min, Optional<string> max, bool throwOutOfRange)
        {
            if (ParameterPresent(parameters, index))
            {
                if (parameters[index].StartsWith('"') && parameters[index].EndsWith('"'))
                {
                    return parameters[index].Substring(1, parameters[index].Length - 2);
                }

                return string.Join(" ", parameters.Skip(index));
            }
            if (!optional)
            {
                throw new KudosArgumentTypeException($"Parameter {index + 1} must be a text");
            }
            SetToDefaultValue(out string value, defaultValue, indexLess);
            return value;
        }

        private static void SetToDefaultValue<T>(out T value, DefaultValue<T> defaultValue, T indexLess)
        {
            if (defaultValue.Special == SpecialDefaults.IndexLess)
            {
                value = indexLess;
                return;
            }
            if (defaultValue.Value.IsSpecified)
            {
                value = defaultValue.Value.Value;
                return;
            }
            value = default;
        }

        public string ToHtml() => Description == null ? string.Empty : $"<tr><td><b>{Character}</b> </td> <td>{Description} </td></tr>";

        public override string ToString() => Description == null ? string.Empty : $"`{Character}` {Description}";

        public class DefaultValue<T>
        {
            public SpecialDefaults Special { get; }
            public Optional<T> Value { get; }

            public DefaultValue(Optional<T> value = default, SpecialDefaults special = SpecialDefaults.None)
            {
                Value = value;
                Special = special;
            }

            public static DefaultValue<T> Create(object value)
            {
                return value switch
                {
                    SpecialDefaults special => new DefaultValue<T>(default, special),
                    T defaultValue => new DefaultValue<T>(defaultValue),
                    _ => new DefaultValue<T>()
                };
            }

            public static DefaultValue<T> FromObject(DefaultValue<object> defaultValue)
            {
                T newValue = defaultValue.Value.IsSpecified ? defaultValue.Value.Value is T tValue ? tValue : default : default;
                return new DefaultValue<T>(newValue, defaultValue.Special);
            }
        }

        protected internal delegate T ParameterAsType<T>(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue,
            Optional<T> min, Optional<T> max, bool throwOutOfRange);

        public enum SpecialDefaults
        {
            None,
            IndexLess
        }
    }

    public class ParameterType<T> : ParameterType
    {
        public override char Character { get; }
        public override string Description { get; }
        protected internal ParameterAsType<T> ParameterInterpreter { get; }


        public override Type Type { get; }

        protected internal ParameterType(char character = default, string description = null, ParameterAsType<T> parameterInterpreter = null)
        {
            Character = character;
            Type = typeof(T);
            Description = description;
            ParameterInterpreter = parameterInterpreter ?? NotImplemented;

            static T NotImplemented(string[] parameters, T indexLess, int index, bool optional, DefaultValue<T> defaultValue, Optional<T> min, Optional<T> max,
                bool throwOutOfRange) => throw new NotImplementedException();
        }

        protected override object InterpretParameter(string[] parameters, IEnumerable<object> indexLess, int index, bool optional,
            DefaultValue<object> defaultValue, Optional<object> min, Optional<object> max, bool throwOutOfRange)
        {
            return ParameterInterpreter.Invoke(parameters, indexLess.FirstOrDefault(obj => obj is T) is T tValue ? tValue : default, index, optional,
                DefaultValue<T>.FromObject(defaultValue), min is Optional<T> tMin ? tMin : default, max is Optional<T> tMax ? tMax : default, throwOutOfRange);
        }
    }
}
