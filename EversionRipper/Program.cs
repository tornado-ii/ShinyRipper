using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using ICSharpCode.SharpZipLib.GZip;
using ShinyTools;

namespace ShinyRipper
{
    class Program
    {
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        static byte[] outStreamArray;
        static string extension = ".cha";
        static string QuitResult = "This program is dedicated to Su Tolias.";

        static void Main(string[] args)
        {
            string GZipResource = "EversionRipper.ICSharpCode.SharpZipLib.dll";
            string DrawingResource = "EversionRipper.System.Drawing.Common.dll";
            EmbeddedAssembly.Load(GZipResource, "ICSharpCode.SharpZipLib.dll");
            EmbeddedAssembly.Load(DrawingResource, "System.Drawing.Common.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            ShinyTools.Helpers.Console.WriteColor("S", ConsoleColor.Red, false);
            ShinyTools.Helpers.Console.WriteColor("H", ConsoleColor.Yellow, false);
            ShinyTools.Helpers.Console.WriteColor("I", ConsoleColor.Green, false);
            ShinyTools.Helpers.Console.WriteColor("N", ConsoleColor.Cyan, false);
            ShinyTools.Helpers.Console.WriteColor("Y", ConsoleColor.Blue, false);
                Console.Write(" Ripper v1.2.0 by ");
                ShinyTools.Helpers.Console.WriteColor("[hy]\n", ConsoleColor.Magenta);
                Console.Write("with research provided by ");
                ShinyTools.Helpers.Console.WriteColor("shrubbyfrog\n\n", ConsoleColor.Green);
                Console.WriteLine("FILES SUPPORTED ARE:\n" +
                "\x10 .cha (compressed)\n" +
                "\x10 .cha (uncompressed)\n" +
                "\x10 .zrs (compressed)\n\n" +
                "GAMES FULLY SUPPORTED:\n" +
                "\x10 Eversion\n" +
                "\x10 Eversion HD\n" +
                "\x10 Eversion HD (Steam)\n\n" +
                "GAMES PARTIALLY SUPPORTED\n" +
                "\x10 4Four\n" +
                "\x10 Pac-Shark\n" +
                "\x10 Castle Awesome\n" +
                "\x10 Zeta's World\n" +
                "====\n"
                );
            if (args.Length < 1)
            {
                Console.WriteLine("Enter the path to the Shiny graphic archive.");
                args = new string[1] { Console.ReadLine() };
            }
            if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
            {
                ConsoleKeyInfo keyInfo;
                Console.WriteLine("Do you want to convert all transparent pixels? y/n");
                do
                {
                    while (!Console.KeyAvailable) Thread.Sleep(10);
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Y)
                    {
                        Func(args[0], true);
                    }
                    if (keyInfo.Key == ConsoleKey.N)
                    {
                        Func(args[0], false);
                    }
                    if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.X)
                    {
                        QuitResult = "Cancelling application...";
                        ShinyTools.Helpers.Application.Quit(QuitResult, 1200, 0);
                    }
                } while (keyInfo.Key != ConsoleKey.Y || keyInfo.Key != ConsoleKey.N || keyInfo.Key != ConsoleKey.Escape || keyInfo.Key != ConsoleKey.X);
            }
        }

        static void Func(string path, bool convertTransparent)
        {
            var InvalidChars = Path.GetInvalidPathChars();
            foreach(char invalid in InvalidChars)
            {
                path = path.Replace(invalid.ToString(), "");
            }

            //This block is to give a little extra compatibility
            //for older Shiny games and extracted .cha files
            try
			{
                //Unpack the GZip file
                MemoryStream compressedStream = new MemoryStream(File.ReadAllBytes(path));
                MemoryStream uncompressedStream = new MemoryStream();
                ICSharpCode.SharpZipLib.GZip.GZipInputStream GZipOut = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(compressedStream);
                GZipOut.CopyTo(uncompressedStream);
                compressedStream.Close();
                GZipOut.Close();

                //This is the extracted file from the GZip buffer.
                //Otherwise known as the Eversion character file.
                outStreamArray = uncompressedStream.ToArray();

                //Clear the streams and dispose of them.
                uncompressedStream.Close();
                compressedStream.Close();
            }
			catch
			{
                outStreamArray = File.ReadAllBytes(path);
			}
            
            //This is the header of the Shiny character archive.
            byte[] header = new byte[0x40];
            Array.Copy(outStreamArray, header, 0x40);

            //Grab the width, height, of the sprites from the header
            byte[] SpriteWidthBytes = new byte[2];
            byte[] SpriteHeightBytes = new byte[2];
            Array.Copy(header, 0x4, SpriteWidthBytes, 0, 2);

            //This is the rest of the Shiny character archive, minus the header.
            byte[] SpriteBuffer = new byte[outStreamArray.Length - header.Length];
            Array.Copy(outStreamArray, header.Length, SpriteBuffer, 0, outStreamArray.Length - header.Length);

            //This is the visual XY and collision XY offset for the sprite.
            //It's not used here, since we only care about the sprite data.
            //So instead we only care about its length.
            int SpriteDataHeaderLength = 0;

            //The transparency colour of sprites are 
            //defined in character sprites but not background sprites
            byte[] SpriteTransparentColour = new byte[3];

            if (extension == ".cha")
            {
                SpriteDataHeaderLength = 0x10;
                Array.Copy(header, 0x24, SpriteTransparentColour, 0, 3);
                Array.Copy(header, 0x6, SpriteHeightBytes, 0, 2);
            }

            if (extension == ".zrs")
            {
                SpriteTransparentColour = new byte[3] { 0x40, 0, 0x40 };
                Array.Copy(header, 0x8, SpriteHeightBytes, 0, 2);
            }

            //Initialise width and height to value
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(SpriteWidthBytes);
                Array.Reverse(SpriteHeightBytes);
            }
            ushort SpriteWidth = BitConverter.ToUInt16(SpriteWidthBytes, 0);
            ushort SpriteHeight = BitConverter.ToUInt16(SpriteHeightBytes, 0);

            //Some helpful stuff for the loop bounds
            int SpriteRegionLength = SpriteWidth * SpriteHeight * 3;
            bool IsPaletted = false;
            byte[] PaletteBytes = new byte[256 * 4];
            if (SpriteRegionLength > SpriteBuffer.Length)
            {
                IsPaletted = true;
                SpriteRegionLength /= 3;
                Array.Copy(SpriteBuffer, PaletteBytes, PaletteBytes.Length);
                var temp = new byte[SpriteRegionLength];
                Array.Copy(SpriteBuffer, PaletteBytes.Length, temp, 0, SpriteRegionLength);
                SpriteBuffer = temp;
            }

            int SpriteCount = SpriteBuffer.Length / (SpriteRegionLength + SpriteDataHeaderLength);

            for (int i = 0; i < SpriteCount; i++)
            {
                //This is the buffer of the actual sprite we want to rip.
                var separatoroffset = i * SpriteDataHeaderLength + SpriteDataHeaderLength;
                var completeoffset = separatoroffset + i * SpriteRegionLength;
                byte[] sprite = new byte[SpriteRegionLength];
                Array.Copy(SpriteBuffer, completeoffset, sprite, 0, SpriteRegionLength);

                int ColourBufferLength = IsPaletted
                    ? PaletteBytes.Length
                    : sprite.Length;

                //Create the image from the bytes
                //We start by converting the bytes into a colour proper
                List<Color> Colours = new List<Color>();
                for (int j = 0; j < ColourBufferLength; j += 3)
                {
                    Color TransparentColour = Color.FromArgb(SpriteTransparentColour[0], SpriteTransparentColour[1], SpriteTransparentColour[2]);
                    Color PixelColour = IsPaletted
                        ? Color.FromArgb((int)PaletteBytes[j], (int)PaletteBytes[j + 1], (int)PaletteBytes[j + 2])
                        : Color.FromArgb((int)sprite[j], (int)sprite[j + 1], (int)sprite[j + 2]);
                    if (convertTransparent && PixelColour == TransparentColour) PixelColour = Color.FromArgb(0, PixelColour.R, PixelColour.G, PixelColour.B);
                    Colours.Add(PixelColour);
                    if (IsPaletted) j++;
                }

                //Now we draw the image !!
                var OutImage = new Bitmap(SpriteWidth, SpriteHeight);
                
                for (int k = 0; k < SpriteWidth * SpriteHeight; k++)
                {
                    int x = k % SpriteWidth;
                    int y = k / SpriteWidth;
                    var WriteThis = IsPaletted
                        ? Colours[SpriteBuffer[k]]
                        : Colours[k];

                    OutImage.SetPixel(x, y, WriteThis);
                }

                var graphic = Graphics.FromImage(OutImage);

                //A few tidbits to drop the files in a folder next to where they came from.
                var filename = Path.GetFileNameWithoutExtension(path);
                var file = Path.GetFileName(path);
                var directory = path.Replace(file, "");
                if (i == 0 && (File.Exists(directory + filename) || Directory.Exists(directory+filename)))
                {
                    ShinyTools.Helpers.Console.WriteColor("ERROR: Could not create the directory or file as it already exists.\nPlease remove the file and try again.\n", ConsoleColor.Red);
                    Console.WriteLine("Press any key to quit the application.");
                    Console.ReadKey(true);
                    ShinyTools.Helpers.Application.Quit(QuitResult, 100, 80);
                }
                else
                {
                    Directory.CreateDirectory(directory + filename);
                    OutImage.Save($"{directory+filename}{Path.DirectorySeparatorChar}{filename}_{i.ToString().PadLeft(SpriteCount.ToString().Length, '0')}.png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            QuitResult = "OK!";
            ShinyTools.Helpers.Application.Quit(QuitResult, 1200);

        }
    }

}
