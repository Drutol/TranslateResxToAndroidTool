using System;
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
        private const string AutoCreateFlag = "autoCreate";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                var versionString = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;


                Console.WriteLine($"TranslateResxToAndroidTool v{versionString}");
                Console.WriteLine($"----------");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"resxtoandroid <input.resx> <output.xml> [-{AutoCreateFlag}]");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Source file does not exist.");
                return;
            }

            var autoCreateOutput = args.Any(s => s.Equals(AutoCreateFlag));

            if (!File.Exists(args[1]))
            {
                if (!autoCreateOutput)
                {
                    Console.WriteLine($"Target file does not exist. Create it? (y/n)");
                    var response = Console.ReadLine();
                    if (response != "y")
                    {
                        return;
                    }

                    var directoryPath = Path.GetDirectoryName(args[1]);
                    // create output file
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }
            }

            var resxDocument = XDocument.Load(args[0]);
            var outputDocument = new StringBuilder();
            outputDocument.AppendLine("<resources>");
            
            foreach (var xElement in resxDocument.Descendants("data"))
            {
                outputDocument.AppendLine(
                    $"<string name=\"{xElement.Attribute("name").Value}\">{EscapeXml(xElement.Descendants("value").First().Value)}</string>");
            }

            outputDocument.AppendLine("</resources>");

            File.WriteAllText(args[1], outputDocument.ToString());
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
