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
        private const string AutoCreateFlag = "-autoCreate";
        private const string OutputFromMetadata = "-outputFromMetadata";
        private const string MetaTargetAndroidFile = "Meta_TargetAndroidFile";

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
                                  $"with its value being set to its path.");
                return;
            }

            if (!File.Exists(argsList[0]))
            {
                Console.WriteLine($"Source file does not exist.");
                return;
            }

            var resxDocument = XDocument.Load(argsList[0]);

            var autoCreateOutput = argsList.Any(s => s.Equals(AutoCreateFlag));
            var outputFromMetadata = argsList.Any(s => s.Equals(AutoCreateFlag));

            if (outputFromMetadata)
            {
                var metadataDescendant = resxDocument.Descendants("data").FirstOrDefault(element =>
                    element.Attribute("name")?.Value.Equals(MetaTargetAndroidFile) ?? false);
                if (metadataDescendant == null)
                {
                    Console.WriteLine($"No metadata key found. ({MetaTargetAndroidFile})");
                    return;
                }
                argsList.Insert(1, metadataDescendant.Descendants("value").First().Value);
            }

            if (!File.Exists(argsList[1]))
            {
                if (!autoCreateOutput)
                {
                    Console.WriteLine($"Target file does not exist. Create it? (y/n)");
                    var response = Console.ReadLine();
                    if (response != "y")
                    {
                        return;
                    }

                    var directoryPath = Path.GetDirectoryName(argsList[1]);

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

                if(nodeValue.Equals(MetaTargetAndroidFile))
                    continue;

                outputDocument.AppendLine(
                    $"<string name=\"{xElement.Attribute("name").Value}\">{EscapeXml(nodeValue)}</string>");

                counter++;
            }

            outputDocument.AppendLine("</resources>");

            File.WriteAllText(argsList[1], outputDocument.ToString());
            Console.WriteLine($"Done. Wrote {counter} nodes to {argsList[1]}.");
        }

        private static string EscapeXml(string value)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = value;
            return node.InnerXml;
        }
    }
}
