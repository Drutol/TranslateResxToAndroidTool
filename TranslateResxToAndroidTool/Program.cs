using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TranslateResxToAndroidTool
{
    class Program
    {
        private static bool _autoCreateOutput;
        private static bool _outputFromMetadata;

        private const string AutoCreateFlag = "-autoCreate";
        private const string OutputFromMetadata = "-outputFromMetadata";
        private const string MetaTargetAndroidFile = "Meta_TargetAndroidFile";
        private const string CultureTokenMarker = "{culture}";

        static void Main(string[] args)
        {
            var argsList = new List<string>(args);

            if (args.Length < 2)
            {
                var versionString = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;


                Console.WriteLine($"TranslateResxToAndroidTool v{versionString}");
                Console.WriteLine($"----------");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"resxtoandroid <input.resx> <output.xml> [{AutoCreateFlag} {OutputFromMetadata}]");
                Console.WriteLine($"");
                Console.WriteLine($"If you want to use {OutputFromMetadata} then please make sure your .resx file contains key {MetaTargetAndroidFile}" +
                                  $"with its value being set to its path. If you include \"{CultureTokenMarker}\" inside it will get replaced for each found culture.");
                return;
            }

            if (!File.Exists(argsList[0]))
            {
                Console.WriteLine($"Source file does not exist.");
                return;
            }

            _autoCreateOutput = argsList.Any(s => s.Equals(AutoCreateFlag));
            _outputFromMetadata = argsList.Any(s => s.Equals(OutputFromMetadata));

            var resourceFiles = Directory.GetFiles(Path.GetDirectoryName(argsList[0]));
            foreach (var resourceFile in resourceFiles.Where(s => s.EndsWith(".resx")))
            {
                WriteResourceFile(resourceFile);
            }
        }

        private static void WriteResourceFile(string resourceFilePath)
        {
            var resxDocument = XDocument.Load(resourceFilePath);

            var outputFile = string.Empty;
            var cultureSuffix = string.Empty;

            var tokens = Path.GetFileName(resourceFilePath).Split('.', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 3)
                cultureSuffix = tokens[1];

            if (_outputFromMetadata)
            {
                var metadataDescendant = resxDocument.Descendants("data").FirstOrDefault(element =>
                    element.Attribute("name")?.Value.Equals(MetaTargetAndroidFile) ?? false);
                if (metadataDescendant == null)
                {
                    Console.WriteLine($"No metadata key found. ({MetaTargetAndroidFile})");
                    return;
                }

                outputFile = metadataDescendant.Descendants("value").First().Value;
            }

            if (!string.IsNullOrEmpty(cultureSuffix) && !outputFile.Contains(CultureTokenMarker))
                throw new ArgumentException(
                    $"Localized resx files exist yet the output file does not contain \"{CultureTokenMarker}\". " +
                    $"You have to include it in the template.");

            if (outputFile.Contains(CultureTokenMarker))
            {
                outputFile = outputFile.Replace(CultureTokenMarker,
                    string.IsNullOrEmpty(cultureSuffix) ? string.Empty : $"-{cultureSuffix}");
            }

            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                if (!_autoCreateOutput)
                {
                    Console.WriteLine($"Target file directory does not exist. Create it? (y/n)");
                    var response = Console.ReadLine();
                    if (response != "y")
                    {
                        return;
                    }

                    var directoryPath = Path.GetDirectoryName(outputFile);

                    // create output file
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }
            }

            var outputDocument = new StringBuilder();
            outputDocument.AppendLine("<resources>");

            var counter = 0;
            foreach (var xElement in resxDocument.Descendants("data"))
            {
                var nodeValue = xElement.Descendants("value").First().Value;

                if (nodeValue.Equals(MetaTargetAndroidFile))
                    continue;

                outputDocument.AppendLine(
                    $"<string name=\"{xElement.Attribute("name").Value}\">{EscapeXml(nodeValue)}</string>");

                counter++;
            }

            outputDocument.AppendLine("</resources>");

            File.WriteAllText(outputFile, outputDocument.ToString());
            Console.WriteLine($"Done. Wrote {counter} nodes to {outputFile}.");
        }

        private static string EscapeXml(string value)
        {
            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = value;
            return node.InnerXml.Replace("'", "\\'").Trim();
        }
    }
}
