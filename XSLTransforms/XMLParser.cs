using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace XSLTransforms
{
    public static class XMLParser
    {
        public static async Task<XPathDocument> LoadXML(string sIn)
        {
            return await Task.Run(() =>
            {
                string errorMessage = null;

                try
                {
                    return new XPathDocument(sIn);
                }

                catch (XmlException xmlException)
                {
                    errorMessage = $"Unable to load the xml document! : {sIn}" + $"{Environment.NewLine}" +
                        $"ERROR ({xmlException.HResult}) : {xmlException.Message} at line {xmlException.LineNumber} and position {xmlException.LinePosition}";
                }

                catch (ArgumentNullException)
                {
                    errorMessage = $"Unable to load the xml document! : NULL";
                }

                finally
                {
                    if (errorMessage != null)
                    {
                        Console.Error.WriteLine(errorMessage);

                        Environment.Exit(0);
                    }
                }

                return null;
            });

        }
    }
}
