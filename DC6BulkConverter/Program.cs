using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DC6BulkConverter
{
    internal class Program
    {
        private enum ConversionMode
        {
            Auto,
            Png,
            Gif,
        }

        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                PrintHelp();
                return;
            }

            string fromPath = args[0];
            string? toPath = args.Length > 1 ? args[1] : null;
            if (toPath?.StartsWith('-') == true)
                toPath = null;

            ConversionMode mode = ConversionMode.Auto;

            string? forceFormat = GetArgsValue(args, "-f", "--force-format");
            if (forceFormat == null)
            {
                PrintError("There was no expected format after -f flag.");
                return;
            }
            else if (forceFormat.Length > 0)
            {
                if (!Enum.TryParse(forceFormat, true, out mode))
                {
                    PrintError($"\"{forceFormat}\" is not a supported format.");
                    return;
                }
            }

            if (File.Exists(fromPath))
            {
                if (toPath != null) Directory.CreateDirectory(toPath);
                ConvertDC6(fromPath, toPath, mode);
            }
            else if (Directory.Exists(fromPath))
            {
                if (toPath != null) Directory.CreateDirectory(toPath);
                BatchConvertDC6(fromPath, toPath, mode);
            }
            else
            {
                PrintError($"The path: {fromPath} does not exist");
            }
        }

        private static void PrintError(string errorMessage)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {errorMessage}");
            Console.ForegroundColor = c;
            Console.WriteLine();
            PrintHelp();
        }

        /// <summary>
        /// Returns null if one of the checked words were present, but there was no value for it.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="checkedWords"></param>
        /// <returns></returns>
        static string? GetArgsValue(string[] args, params string[] checkedWords)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (checkedWords.Contains(args[i]))
                {
                    if (i + 1 >= args.Length)
                        return null;
                    return args[i + 1];
                }
            }
            return string.Empty;
        }

        private static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE: dc6converter <file_or_directory_path> [<output_path>] [-f png|gif]");
            Console.WriteLine();
            Console.WriteLine("This program will look through the <file_or_directory_path> for .dc6 files,");
            Console.WriteLine("and save converted images in the <output_path>.");
            Console.WriteLine();
            Console.WriteLine("If the <output_path> is not given, it will put the files in the directory you're running it from.");
            Console.WriteLine("Additionally, if the <output_path> is given, but directory does not exist, it will be automatically created.");
            Console.WriteLine();
            Console.WriteLine(" Options:");
            Console.WriteLine("\t-f --force-format\tForces the output to have a specific format,");
            Console.WriteLine("\t\t\t\totherwise, it will use png for single-framed images, and gif for animations");
            Console.WriteLine("");
        }

        private static void BatchConvertDC6(string fromPath, string? toPath, ConversionMode mode)
        {
            var files = Directory.GetFiles(fromPath, "*.dc6");
            Console.WriteLine($"Converting {files.Length} files from: {fromPath}{(toPath != null ? $", to: {toPath}" : "")}...");
            foreach (var file in files)
                ConvertDC6(file, toPath, mode);
        }

        private static void ConvertDC6(string filePath, string? toDir, ConversionMode mode)
        {
            Console.WriteLine($"Converting image: {filePath}...");

            DC6Image img = DC6Image.Read(filePath);
            switch (mode)
            {
                case ConversionMode.Auto:
                    if (img.Frames.Length == 1 || mode == ConversionMode.Png)
                        ConvertToPng(filePath, toDir, img);
                    else
                        ConvertToGif(filePath, toDir, img);
                    break;

                case ConversionMode.Png:
                    ConvertToPng(filePath, toDir, img);
                    break;

                case ConversionMode.Gif:
                    ConvertToGif(filePath, toDir, img);
                    break;

                default:
                    throw new NotImplementedException($"Conversion to {mode} has not been implemented.");
            }
        }

        private static void ConvertToGif(string filePath, string? toDir, DC6Image dc6Img)
        {
            int width = dc6Img.Frames.Max(x => x.FrameWidth);
            int height = dc6Img.Frames.Max(y => y.FrameHeight);

            var firstFrame = dc6Img.Frames[0];
            using var img = Image.LoadPixelData<Rgba32>(firstFrame.ToRgba32(width, height), width, height);

            if (firstFrame.Flip == 0)
                img.Mutate(x => x.Flip(FlipMode.Vertical));

            var gifMetadata = img.Frames.RootFrame.Metadata.GetGifMetadata();
            gifMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;

            for (int i = 1; i < dc6Img.Frames.Length; i++)
            {
                var frame = dc6Img.Frames[i];
                using var imgFrame = Image.LoadPixelData<Rgba32>(frame.ToRgba32(width, height), width, height);
                gifMetadata = imgFrame.Frames.RootFrame.Metadata.GetGifMetadata();
                gifMetadata.FrameDelay = 10;
                gifMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;

                if (frame.Flip == 0)
                    imgFrame.Mutate(x => x.Flip(FlipMode.Vertical));

                img.Frames.AddFrame(imgFrame.Frames.RootFrame);
            }

            string newFileName = Path.GetFileNameWithoutExtension(filePath) + ".gif";
            string savePath = toDir == null ? newFileName : Path.Join(toDir, newFileName);

            img.SaveAsGif(savePath);
            Console.WriteLine($"  image saved: {savePath}");
        }

        private static void ConvertToPng(string filePath, string? toDir, DC6Image dc6Img)
        {
            for (int i = 0; i < dc6Img.Frames.Length; i++)
            {
                var frame = dc6Img.Frames[i];
                Rgba32[] pixels = frame.ToRgba32(frame.FrameWidth, frame.FrameHeight);

                using var image = Image.LoadPixelData<Rgba32>(pixels, frame.FrameWidth, frame.FrameHeight);

                string newFileName = Path.GetFileNameWithoutExtension(filePath) +
                    (dc6Img.Frames.Length == 1 ? ".png" : $"_frame{i}.png");

                string savePath = toDir == null ? newFileName : Path.Join(toDir, newFileName);

                if (frame.Flip == 0)
                    image.Mutate(x => x.Flip(FlipMode.Vertical));

                image.SaveAsPng(savePath);
                Console.WriteLine($"  image saved: {savePath}");
            }
        }
    }
}