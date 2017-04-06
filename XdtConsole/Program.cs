using System;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Web.XmlTransform;

namespace XdtConsole
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Required Arguments: [ConfigPath] [TransformPath] [optional:TargetPath, if not set source=destination]");
                return 400;
            }
            
            var configPath = args[0];
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Config not found");
                return 404;
            }

            var transformPath = args[1];
            if (!File.Exists(transformPath))
            {
                Console.WriteLine("Transform not found");
                return 404;
            }

            try
            {
                var targetPath = args[0];
                if (args.Length > 2)
                {
                    targetPath = args[2];
                }

                Console.WriteLine("1> Read config " + configPath);
                var configXml = File.ReadAllText(configPath);

                Console.WriteLine("2> Read transformation " + transformPath);
                var transformXml = File.ReadAllText(transformPath);

                using (var document = new XmlTransformableDocument())
                {
                    document.PreserveWhitespace = true;
                    document.LoadXml(configXml);

                    using (var transform = new XmlTransformation(transformXml, false, null))
                    {
                        Console.WriteLine("3> Apply changes");
                        if (transform.Apply(document))
                        {
                            var stringBuilder = new StringBuilder();
                            var xmlWriterSettings = new XmlWriterSettings
                            {
                                Indent = true,
                                IndentChars = "  ",
                            };

                            Console.WriteLine("4> Write to output file " + targetPath);
                            using (var xmlTextWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
                            {
                                document.WriteTo(xmlTextWriter);
                            }

                            var resultXml = stringBuilder.ToString();
                            File.WriteAllText(targetPath, resultXml);

                            Console.WriteLine("<3 finshed... have a nice day :)");

                            return 0;
                        }

                        Console.WriteLine("Transformation failed for unknown reason");
                    }
                }
            }

            catch (XmlTransformationException xmlTransformationException)
            {
                Console.WriteLine(xmlTransformationException.Message);
            }
            catch (XmlException xmlException)
            {
                Console.WriteLine(xmlException.Message);
            }

            return 500;
        }
    }
}