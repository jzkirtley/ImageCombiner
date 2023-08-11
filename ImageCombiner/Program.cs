using System;
using System.IO;
using System.Drawing;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private const int RowSize = 10;
        private const int NumRows = 7;

        private static List<string> folders = new List<string> { "Skills" };
        private const string parentFolder = "c:\\Games\\PacRim Skirmish";
        

        static void Main(string[] args)
        {
            foreach (var folder in folders)
            {
                CombineFolder(folder);
            }
        }

        static void CombineFolder(string inputFolderName)
        {
            var fullPath = Path.Combine(parentFolder, inputFolderName);
            var di = new DirectoryInfo(fullPath);
            var counter = 0;
            var files = di.EnumerateFiles().Select(fi => fi.FullName);
            var filesPerPage = RowSize * NumRows;
            while (true)
            {
                var batch = files.Skip(counter * filesPerPage).Take(filesPerPage).ToList();
                if (batch.Count == 0) break;

                var filePath = Path.Combine(parentFolder, inputFolderName);
                if (counter > 0) filePath += $"{counter}";
                filePath += ".png";

                Combine(batch, filePath);
                counter++;
            }
        }

        static void Combine(List<string> fileNames, string outputFilename)
        {
            var image = Image.FromFile(fileNames[0]);
            var width = image.Width;
            var height = image.Height;

            var rowCount = (int)Math.Ceiling((double)fileNames.Count / RowSize);

            using (var bitmap = new Bitmap(width * RowSize, height * rowCount))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    for (var rowNum = 0; rowNum < rowCount; rowNum++)
                    {
                        var hOffset = 0;
                        foreach (var sourceImagePath in fileNames.Skip(RowSize*rowNum).Take(RowSize))
                        {
                            var sourceImage = Image.FromFile(sourceImagePath);
                            canvas.DrawImage(sourceImage, new Rectangle(hOffset, rowNum * height, width, height), new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), GraphicsUnit.Pixel);
                            hOffset += width;
                        }
                    }
                    canvas.Save();
                }
                bitmap.Save(outputFilename, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

    }
}