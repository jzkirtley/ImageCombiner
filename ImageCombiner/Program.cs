using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageCombiner;

internal class Program
{
    private const float MAX_WIDTH = 4096;

    private static Arguments? arguments;

    static void Main(string[] args)
    {
        arguments = new Arguments(args);
        if (arguments.ShowHelp)
        {
            Help();
            return;
        }

        if (arguments.ErrorText != null)
        {
            Console.WriteLine("Error parsing Arguments: " + arguments.ErrorText);
            Usage();
            return;
        }

        if (arguments.SourceFolder == null || arguments.TargetFolder == null)
        {
            Console.WriteLine("Source or Target Folder isn't specified");
            Usage();
            return;
        }

        if (!EnsureFolders(arguments))
        {
            Console.WriteLine("Source or Target folder doesn't exist or isn't specified");
            Usage();
            return;
        }

        Console.WriteLine($"Creating {arguments.RowSize} cards per row, max {arguments.MaxRows} rows");

        CombineFolder(arguments.SourceFolder, arguments.TargetFolder);
    }

    static void Usage()
    {
        Console.WriteLine("Usage: ImageCombiner (-RowSize <row-size>) (-MaxRows <max-rows>) (-MarginX <margin-x>) (-MarginY <margin-y>) <source-folder> (<output-folder>)");
        Console.WriteLine("If <output-folder> is omitted, output files are left in the parent of the source folder");
        Console.WriteLine("Default number of cards per row is 10. Default max number of rows is 5");
        Console.WriteLine("If Margin_X or Margin_Y are nonzero, that many pixels will be removed from the top and bottom of each card image");
    }

    static void Help()
    {
        Usage();

        Console.WriteLine("ImageCombiner is designed to work with Tabletop Simulator to combine multiple images from a single folder into one or more");
        Console.WriteLine("Image files with 10 images per row and a maximum of 5 rows. If there are more than 50 cards, it will create multiple output");
        Console.WriteLine("Files.");
        Console.WriteLine("If the total image size is too big for Tabletop Simulator, it will scale the cards down to an acceptable size");
    }

    static private bool EnsureFolders(Arguments arguments)
    {
        if (arguments.SourceFolder == null || arguments.TargetFolder == null) return false;

        Console.WriteLine($"Using Source Folder {arguments.SourceFolder}");
        if (!Directory.Exists(arguments.SourceFolder))
        {
            Console.WriteLine($"Source Folder {arguments.SourceFolder} Doesn't Exist. Creating");
            Directory.CreateDirectory(arguments.SourceFolder);

        }

        Console.WriteLine($"Using Target Folder {arguments.TargetFolder}");
        if (!Directory.Exists(arguments.TargetFolder))
        {
            Console.WriteLine($"Target Folder {arguments.TargetFolder} Doesn't Exist. Creating");
            Directory.CreateDirectory(arguments.TargetFolder);
        }
        return true;
    }


    static private void CombineFolder(string sourceFolder, string targetFolder)
    {
        var di = new DirectoryInfo(sourceFolder);
        var counter = 0;
        var files = di.EnumerateFiles().Select(fi => fi.FullName);
        var filesPerPage = arguments.RowSize * arguments.MaxRows;
        while (true)
        {
            var batch = files.Skip(counter * filesPerPage).Take(filesPerPage).ToList();
            if (batch.Count == 0) break;

            var filePath = Path.Combine(targetFolder, di.Name);
            if (counter > 0) filePath += $"{counter}";
            filePath += ".png";

            Console.WriteLine($"Creating {filePath}");

            Combine(batch, filePath);
            counter++;
        }
    }

    static void Combine(List<string> fileNames, string outputFilename)
    {
        var image = Image.FromFile(fileNames[0]);
        var width = image.Width - (2 * arguments.MarginX);
        var height = image.Height - (2 * arguments.MarginY);

        var rowCount = (int)Math.Ceiling((double)fileNames.Count / arguments.RowSize);

        using (var bitmap = new Bitmap(width * arguments.RowSize, height * rowCount))
        {
            using (var canvas = Graphics.FromImage(bitmap))
            {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                for (var rowNum = 0; rowNum < rowCount; rowNum++)
                {
                    var hOffset = 0;
                    foreach (var sourceImagePath in fileNames.Skip(arguments.RowSize *rowNum).Take(arguments.RowSize))
                    {
                        var sourceImage = Image.FromFile(sourceImagePath);
                        canvas.DrawImage(sourceImage, new Rectangle(hOffset, rowNum * height, width, height), new Rectangle(arguments.MarginX, arguments.MarginY, sourceImage.Width - (2* arguments.MarginX), sourceImage.Height - (2*arguments.MarginY)), GraphicsUnit.Pixel);
                        hOffset += width;
                    }
                }
                canvas.Save();
            }

            // If the results are more than 4k pixels wide, we need to scale it down.
            var totalWidth = width * arguments.RowSize;
            var totalHeight = height * rowCount;
            if (totalWidth > MAX_WIDTH)
            {
                var ratio = .25; //MAX_WIDTH / totalWidth;
                var newWidth = (int)(totalWidth * ratio);
                var newHeight = (int)(totalHeight * ratio);

                var resizedBitmap = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(resizedBitmap))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;

                    // Draw the original image onto the resized bitmap
                    graphics.DrawImage(bitmap, 0, 0, newWidth, newHeight);
                }
                resizedBitmap.Save(outputFilename, System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                bitmap.Save(outputFilename, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

}