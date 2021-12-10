using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace XSLTransforms
{
    public static class Transformer
    {
        public static async Task ApplyTransform(string sXIn, XsltArgumentList dParams, string sXsl, string sOut)
        {
            await Task.Run(() =>
            {
                try
                {
                    var xslCompiledTransform = new XslCompiledTransform();

                    xslCompiledTransform.Load(sXsl, new XsltSettings { EnableScript = true }, null);

                    using (var outputFileStream = new FileStream(sOut, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var streamWriter = new StreamWriter(outputFileStream))
                    using (var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
                    {
                        Indent = true,
                        NewLineChars = "\n",
                        IndentChars = "\t"
                    }))
                    {
                        xslCompiledTransform.Transform(sXIn, dParams, xmlWriter);

                        outputFileStream?.SetLength(outputFileStream?.Position ?? 0); // Trunkates existing file
                    }
                }

                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Failed converting file! : {sXIn}" + $"{Environment.NewLine}" +
                        $"ERROR ({exception.HResult}) : {exception.Source} : {exception.Message}");

                    Environment.Exit(0);
                }
            });
        }
    }
}
