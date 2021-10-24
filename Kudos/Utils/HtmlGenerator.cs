#region

using Kudos.Attributes;
using Kudos.Bot;
using Kudos.Models;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Kudos.Utils {

    public class HtmlGenerator {

        public string LongDescription() {
            string longDescription =
                File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"HTML/Main.html"));
            string commands = CommandModules.Instance.Modules.Where(module => module.Module.Accessibility != Accessibility.Hidden)
                .Aggregate(string.Empty, (current, module) => current + module.CommandListAsHtml);

            string types = ParameterType.KnownParameterTypes.Aggregate(string.Empty, (current, parameterType) => current + parameterType.ToHtml());

            string settings = Settings.SettingsListAsHtml;

            longDescription = longDescription.Replace("<types></types>", types);
            longDescription = longDescription.Replace("<settings></settings>", settings);
            longDescription = longDescription.Replace("<commands></commands>", commands);

            return longDescription;
        }
    }
}