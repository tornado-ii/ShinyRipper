using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ShinyTools.Helpers
{
    #region Console printing helpers
    public static class Console
    {
        /// <summary>
        /// Write text to the console using colours.
        /// </summary>
        /// <param name="text">The text to write to the console.</param>
        /// <param name="textColour">The colour the font should be when written.</param>
        /// <param name="backgroundColour">The colour the background should be behind the text.</param>
        /// <param name="WriteOnNewLine">If true, will write the text on a new line. If false, will write the text inline.</param>
        public static string WriteColor(string text, ConsoleColor textColour, ConsoleColor backgroundColour, bool WriteOnNewLine)
        {
            System.Console.ForegroundColor = textColour;
            System.Console.BackgroundColor = backgroundColour;
            if (WriteOnNewLine) System.Console.WriteLine(text);
            else System.Console.Write(text);
            System.Console.ResetColor();
            return null;
        }
        public static string WriteColor(string text, ConsoleColor textColour, ConsoleColor backgroundColour) => WriteColor(text, textColour, backgroundColour, false);
        public static string WriteColor(string text, ConsoleColor textColour, bool WriteOnNewLine) => WriteColor(text, textColour, System.Console.BackgroundColor, WriteOnNewLine);
        public static string WriteColor(string text, ConsoleColor textColour) => WriteColor(text, textColour, System.Console.BackgroundColor, false);
        public static string WriteColor(string text, bool WriteOnNewLine) => WriteColor(text, System.Console.ForegroundColor, System.Console.BackgroundColor, WriteOnNewLine);
        public static string WriteColor(string text) => WriteColor(text, System.Console.ForegroundColor, System.Console.BackgroundColor, false);
    }
    #endregion
    #region Application helpers
    public static class Application
    {
        /// <summary>
        /// Displays a message before exiting the program.
        /// </summary>
        /// <param name="result">The text to be displayed when exiting.</param>
        /// <param name="timeout">The time in milliseconds before the program closes.</param>
        /// <param name="exitCode">Environment exit code (for debugging and logging).</param>
        public static void Quit(string result, int timeout, int exitCode)
        {
            System.Console.WriteLine(result);
            Thread.Sleep(timeout);
            Environment.Exit(exitCode);
        }
        public static void Quit(string result, int timeout) => Quit(result, timeout, 0);
        public static void Quit(string result) => Quit(result, 1200, 0);
        public static void Quit() => Quit("", 1200, 0);
    }
    #endregion
    #region I/O helpers
    public static class IO
    {
        public static void Write(byte[] data, string directory, string filename, string extension)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var f = File.Create(directory + filename + extension);
            f.Write(data, 0, data.Length);
            f.Close();
        }
    }
    #endregion
    #region Comparison helpers
    public static class Comparers
    {
        public class IndexComparer : IComparer<string>
        {
            Regex _reg = new Regex(@"\Z_(?<index>[\S]*).*");
            public int Compare(string first, string second)
            {
                var _1st = _reg.Match(first).Groups["index"].Value;
                var _2nd = _reg.Match(second).Groups["index"].Value;
                return _1st.CompareTo(_2nd);
            }
        }
    }
    #endregion
}
