using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ImageCombiner
{
    internal class Arguments
    {
        public bool ShowHelp { get; }
        public string? ErrorText { get; }

        public string? SourceFolder { get; }
        public string? TargetFolder { get; }

        public int RowSize { get; } = 10;
        public int MaxRows { get; } = 5;

        public int MarginX { get; } = 0;
        public int MarginY { get; } = 0;

        internal Arguments(string[] args)
        {
            try
            {
                if (args.Length == 1 && (string.Equals(args[0], "-h") || string.Equals(args[0], "-help")))
                {
                    ShowHelp = true;
                    return;
                }

                for (var currentArg = 0; currentArg < args.Length; currentArg++)
                {
                    if (string.Equals(args[currentArg], "-RowSize", StringComparison.CurrentCultureIgnoreCase))
                    {
                        RowSize = ParseInt(args, currentArg + 1, "-RowSize");
                        currentArg++;
                        continue;
                    }

                    if (string.Equals(args[currentArg], "-MaxRows", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MaxRows = ParseInt(args, currentArg + 1, "-MaxRows");
                        currentArg++;
                        continue;
                    }

                    if (string.Equals(args[currentArg], "-MarginX", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MarginX = ParseInt(args, currentArg + 1, "-MarginX");
                        currentArg++;
                        continue;
                    }

                    if (string.Equals(args[currentArg], "-MarginY", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MarginY = ParseInt(args, currentArg + 1, "-MarginY");
                        currentArg++;
                        continue;
                    }

                    // If we aren't parsing a flag, then it must be the source and then target folders.
                    if (SourceFolder == null)
                    {
                        SourceFolder = args[currentArg];
                    }
                    else TargetFolder = args[currentArg];
                }

                if (SourceFolder == null)
                {
                    ErrorText = "Source Folder not specified";
                    return;
                }

                if (!Path.IsPathFullyQualified(SourceFolder))
                {
                    SourceFolder = Path.Join(Directory.GetCurrentDirectory(), SourceFolder);
                }

                if (TargetFolder != null)
                {
                    if (!Path.IsPathFullyQualified(TargetFolder))
                    {
                        TargetFolder = Path.Join(Directory.GetCurrentDirectory(), TargetFolder);
                    }
                }
                else
                {
                    var targetFolderDI = new DirectoryInfo(SourceFolder);
                    if (targetFolderDI == null || targetFolderDI.Parent == null) throw new ArgumentException("Somehow the Source Folder doesn't exist");
                    TargetFolder = targetFolderDI.Parent.FullName;
                    Console.WriteLine($"Target Folder not specified, using Parent of source folder: {TargetFolder}");
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private int ParseInt(string[] args, int valueArg, string argName) 
        {
            if (valueArg >= args.Length)
            {
                throw new ArgumentException($"{argName} flag used without value");
            }

            try
            {
                return int.Parse(args[valueArg]);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error parsing value {args[valueArg]} for Argument {argName}", ex);
            }
        }
    }
}
