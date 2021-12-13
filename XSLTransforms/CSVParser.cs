using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace XSLTransforms
{
    public static class CSVParser
    {
        public static async Task CSV2XCSV(string sCSV, string sXCSV)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var inputFileStream = new FileStream(sCSV, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var streamReader = new StreamReader(inputFileStream))
                    using (var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false, BadDataFound = null }))
                    using (var outputFileStream = new FileStream(sXCSV, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var streamWriter = new StreamWriter(outputFileStream))
                    using (var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
                    {
                        Indent = true,
                        NewLineChars = Environment.NewLine,
                        IndentChars = "\t"
                    }))
                    {
                        xmlWriter.WriteStartDocument();

                        xmlWriter.WriteStartElement("WorkSheet");

                        foreach (IDictionary<string, object> record in csvReader.GetRecords<dynamic>())
                        {
                            xmlWriter.WriteStartElement("Row");

                            foreach (string value in record.Values)
                            {
                                xmlWriter.WriteStartElement("Column");
                                xmlWriter.WriteString(value.RemoveNewLinesAndConsecutiveSpaces());
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();

                        outputFileStream?.SetLength(outputFileStream?.Position ?? 0); // Trunkates existing file
                    }
                }

                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Failed converting file! : {sCSV}" + $"{Environment.NewLine}" +
                        $"ERROR ({exception.HResult}) : {exception.Source} : {exception.Message}");

                    Environment.Exit(0);
                }
            });
        }

        public static async Task XCSV2CSV(string sXCSV, string sCSV) => await XML2Delimited(sXCSV, sCSV, ",");

        static async Task XML2Delimited(string sXCSV, string sCSV, string sDelimiter)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var inputFileStream = new FileStream(sXCSV, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var xmlReader = XmlReader.Create(inputFileStream))
                    using (var outputFileStream = new FileStream(sCSV, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var streamWriter = new StreamWriter(outputFileStream))
                    using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = sDelimiter,
                        HasHeaderRecord = false
                    }))
                    {
                        while (xmlReader.Read())
                        {
                            var nodeType = xmlReader.NodeType;
                            var elementName = xmlReader.Name;

                            if (nodeType == XmlNodeType.Element && elementName == "Column")
                            {
                                var value = xmlReader.ReadElementContentAsString();
                                csvWriter.WriteField(value.RemoveNewLinesAndConsecutiveSpaces());
                            }

                            else if (nodeType == XmlNodeType.EndElement && elementName == "Row")
                                csvWriter.NextRecord();
                        }

                        outputFileStream?.SetLength(outputFileStream?.Position ?? 0); // Trunkates existing file
                    }
                }

                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Failed converting file! : {sXCSV}" + $"{Environment.NewLine}" +
                        $"ERROR ({exception.HResult}) : {exception.Source} : {exception.Message}");

                    Environment.Exit(0);
                }
            });
        }
    }
}
