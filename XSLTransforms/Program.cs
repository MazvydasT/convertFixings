using NDesk.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace XSLTransforms
{
    class Program
    {
        const double MAX_PROGRESS = 7;

        static DateTime startTime;

        static async Task Main(string[] args)
        {
            string sFile = null;

            string sProd = null;
            string sVars = null;

            string sPrefix = null;

            string sOut = null;

            string csvTransformer = null;
            string eMSTransformer = null;

            bool showHelp = false;

            var cacheKey = new object();

            var options = new OptionSet()
            {
                { "csv=", "{PATH} to source interim file", v => sFile = Utils.PathToUNC(v.Trim(), cacheKey) },

                { "prod=", "{PATH} to Product Structure Export XML file", v => sProd = Utils.PathToUNC(v.Trim(), cacheKey) },
                { "vars=", "{PATH} to eMS VariantSetLibrary Export XML file", v => sVars = Utils.PathToUNC(v.Trim(), cacheKey) },

                { "prefix:", "ExternalID prefix {VARIABLE}", v => sPrefix = v?.Trim() },

                { "out=", "{PATH} to eBop Import file", v => sOut = Utils.PathToUNC(v.Trim(), cacheKey) },

                { "csvxsl=", "{PATH} to csv_transformer.xsl", v => csvTransformer = Utils.PathToUNC(v.Trim(), cacheKey) },
                { "emsxsl=", "{PATH} to ems_transformer.xsl", v => eMSTransformer = Utils.PathToUNC(v.Trim(), cacheKey) },

                { "h|?|help", "Shows this help message", v => showHelp = v != null }
            };

            try
            {
                options.Parse(args);
            }

            catch (OptionException e)
            {
                Console.Error.WriteLine($"Error:\n{Process.GetCurrentProcess().ProcessName} {string.Join(" ", args)}");
                Console.Error.WriteLine(e.Message);

                Console.Error.WriteLine();
                ShowHelp(options);

                return;
            }

            if (showHelp)
            {
                ShowHelp(options);
                return;
            }


            ReportPorgress(0, MAX_PROGRESS, "Checking inputs");

            if (string.IsNullOrEmpty(sFile))
            {
                Console.Error.WriteLine("Path to source CSV file is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(sProd))
            {
                Console.Error.WriteLine("Path to Product Structure Export XML file is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(sVars))
            {
                Console.Error.WriteLine("Path to eMS VariantSetLibrary Export XML file is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(sOut))
            {
                Console.Error.WriteLine("Path to eBop Import file is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(csvTransformer))
            {
                Console.Error.WriteLine("Path to csv_transformer.xsl is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(eMSTransformer))
            {
                Console.Error.WriteLine("Path to ems_transformer.xsl is not provided.\n");
                ShowHelp(options);
                return;
            }

            if (!File.Exists(sFile))
            {
                Console.Error.WriteLine($"Input file not found! : {sFile}");
                return;
            }

            if (!File.Exists(sProd))
            {
                Console.Error.WriteLine($"Input file not found! : {sProd}");
                return;
            }

            if (!File.Exists(sVars))
            {
                Console.Error.WriteLine($"Input file not found! : {sVars}");
                return;
            }

            if (!File.Exists(csvTransformer))
            {
                Console.Error.WriteLine($"Transformation Stylesheet not found!" + $"{Environment.NewLine}" +
                    $"{csvTransformer}");
                return;
            }

            if (!File.Exists(eMSTransformer))
            {
                Console.Error.WriteLine($"Transformation Stylesheet not found!" + $"{Environment.NewLine}" +
                    $"{eMSTransformer}");
                return;
            }


            ReportPorgress(1, MAX_PROGRESS, $"Loading {Path.GetFileName(sProd)}");

            var xProd = await XMLParser.LoadXML(sProd);



            var sXFile = Path.Combine(Path.GetDirectoryName(sOut), Path.GetFileNameWithoutExtension(sFile) + ".xcsv");

            ReportPorgress(2, MAX_PROGRESS, $"{Path.GetFileName(sFile)} → {Path.GetFileName(sXFile)}");

            await CSVParser.CSV2XCSV(sFile, sXFile);


            ReportPorgress(3, MAX_PROGRESS, $"Loading {Path.GetFileName(sVars)}");

            var xVars = await XMLParser.LoadXML(sVars);


            ReportPorgress(4, MAX_PROGRESS, $"{Path.GetFileName(sXFile)} → {Path.GetFileName(sOut)}");

            var dParams = new XsltArgumentList();
            dParams.AddParam("xProd", string.Empty, xProd); // PRODUCT.xml
            dParams.AddParam("xVars", string.Empty, xVars); // VARIANTS.xml
            dParams.AddParam("prefix", string.Empty, sPrefix ?? ""); // prefix


            await Transformer.ApplyTransform(sXFile, dParams, csvTransformer, sOut);


            ReportPorgress(5, MAX_PROGRESS, $"{Path.GetFileName(sOut)} → {Path.GetFileName(sXFile)}");

            await Transformer.ApplyTransform(sOut, null, eMSTransformer, sXFile);


            ReportPorgress(6, MAX_PROGRESS, $"{Path.GetFileName(sXFile)} → {Path.GetFileName(sFile)}");

            await CSVParser.XCSV2CSV(sXFile, sFile);


            ReportPorgress(MAX_PROGRESS, MAX_PROGRESS, "Done");
        }

        static void ShowHelp(OptionSet options)
        {
            Console.Error.WriteLine("Usage:\n");

            var start = $"{Process.GetCurrentProcess().ProcessName} ";
            var padding = "".PadLeft(start.Length + 1);

            Console.Error.WriteLine($" {start}/csv:OUTPUT_FILES/REPORT.csv" +
                $"\n{padding}/prod:INPUT_FILES/PRODUCT.xml" +
                $"\n{padding}/vars:INPUT_FILES/VARIANTS.xml" +
                $"\n{padding}/out:OUTPUT_FILES/EMS_IMPORT_FILE.xml" +
                $"\n{padding}/csvxsl:P:/path/csv_transformer.xsl" +
                $"\n{padding}/emsxsl:P:/path/ems_transformer.xsl" +
                $"\n{padding}/prefix:X123_22_");
            Console.Error.WriteLine("\n");

            Console.Error.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Error);
        }

        static void ReportPorgress(double value, double max, string message)
        {
            var currentTime = DateTime.Now;

            if (value == 0) startTime = currentTime;

            var elapsedTime = currentTime - startTime;
            elapsedTime = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);

            Console.Clear();

            Console.Write($"{(value / max):P0}".PadLeft($"{1:P0}".Length) + $" | {elapsedTime:c} | {message}\n\n");
        }
    }
}
