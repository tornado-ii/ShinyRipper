using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using ICSharpCode.SharpZipLib.GZip;

namespace EversionRipper
{
    class Program
    {
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        static void Main(string[] args)
        {
            string GZipResource = "EversionRipper.ICSharpCode.SharpZipLib.dll";
            string DrawingResource = "EversionRipper.System.Drawing.Common.dll";
            EmbeddedAssembly.Load(GZipResource, "ICSharpCode.SharpZipLib.dll");
            EmbeddedAssembly.Load(DrawingResource, "System.Drawing.Common.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Console.WriteLine(
                //"NOTE: THIS PROGRAM IS STILL A WORK IN PROGRESS.\n" +
                "Eversion Ripper v1.0 by [hy]\n" +
                "with research provided by shrubbyfrog\n\n" +
                "FILES SUPPORTED ARE:\n" +
                "-.cha\n" +
                "-.zrs\n\n" +
                "GAMES SUPPORTED:\n" +
                "-Eversion\n" +
                "-Eversion HD\n" +
                "-Eversion HD (Steam)\n\n" +
                //"*indicates partial support\n" +
                "====\n"
                );
            if (args.Length < 1)
            {
                Console.WriteLine("Enter the path to the Eversion graphic file.");
                args = new string[1] { Console.ReadLine() };
            }
            if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("Do you want to convert all transparent pixels? y/n");
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Func(args[0], true);
                }
                if (keyInfo.Key == ConsoleKey.N)
                {
                    Func(args[0], false);
                }
            }
            Console.WriteLine("\nOK!");
            Thread.Sleep(1000);
        }

        static void Func(string path, bool convertTransparent)
        {
            var InvalidChars = Path.GetInvalidPathChars();
            foreach(char invalid in InvalidChars)
            {
                path = path.Replace(invalid.ToString(), "");
            }
            //Unpack the GZip file
            MemoryStream compressedStream = new MemoryStream(File.ReadAllBytes(path));
            var extention = Path.GetExtension(path);
            MemoryStream uncompressedStream = new MemoryStream();
            ICSharpCode.SharpZipLib.GZip.GZipInputStream GZipOut = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(compressedStream);
            GZipOut.CopyTo(uncompressedStream);
            compressedStream.Close();
            GZipOut.Close();

            //This is the extracted file from the GZip buffer.
            //Otherwise known as the Eversion character file.
            var outStreamArray = uncompressedStream.ToArray();
            
            //This is the header of the Eversion character file.
            byte[] header = new byte[0x40];
            Array.Copy(outStreamArray, header, 0x40);

            //Grab the width, height, of the sprites from the header
            byte[] SpriteWidthBytes = new byte[2];
            byte[] SpriteHeightBytes = new byte[2];
            Array.Copy(header, 0x4, SpriteWidthBytes, 0, 2);

            //This is the rest of the Eversion character file, minus the header.
            byte[] SpriteBuffer = new byte[outStreamArray.Length - header.Length];
            Array.Copy(outStreamArray, header.Length, SpriteBuffer, 0, outStreamArray.Length - header.Length);

            //This is the length of data found between sprites.
            //At the time of writing, we don't know what this
            //data actually represents.
            int SpriteSeparatorLength = 0;

            //The transparency colour of sprites are 
            //defined in character sprites but not background sprites
            byte[] SpriteTransparentColour = new byte[3];

            if (extention == ".cha")
            {
                SpriteSeparatorLength = 0x10;
                Array.Copy(header, 0x24, SpriteTransparentColour, 0, 3);
                Array.Copy(header, 0x6, SpriteHeightBytes, 0, 2);
            }

            if (extention == ".zrs")
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

            int SpriteCount = SpriteBuffer.Length / (SpriteRegionLength + SpriteSeparatorLength);

            for (int i = 0; i < SpriteCount; i++)
            {
                //This is the buffer of the actual sprite we want to rip.
                var separatoroffset = i * SpriteSeparatorLength + SpriteSeparatorLength;
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
                Directory.CreateDirectory(directory + filename);
                OutImage.Save($"{directory+filename}{Path.DirectorySeparatorChar}{filename}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            uncompressedStream.Close();
        }
    }

}
