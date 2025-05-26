using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2
{
    internal class Program
    {
        public enum Color { WHITE = 37, YELLOW = 33, BLUE = 34, RED = 31, GREEN = 32 }
        public class BeautifulConsole : IDisposable
        {
            private readonly Color _COLOR;
            private readonly (int, int) _POS;
            private readonly string _SYMBOL;
            private static readonly Dictionary<char, string[]> _FONT = GetFont("font.txt");

            public BeautifulConsole(Color color, (int, int) pos, string symbol)
            {
                _COLOR = color;
                _POS = pos;
                _SYMBOL = symbol;
            }
            private static string[] TextCreate(string text, string symbol)
            {
                int height = _FONT['A'].Length;
                string[] outputText = new string[height];

                for (int i = 0; i < height; i++)
                {
                    outputText[i] = "";
                }

                foreach (char letter in text)
                {
                    if (!_FONT.ContainsKey(letter)) 
                        continue;

                    for (int i = 0; i < height; i++)
                    {
                        outputText[i] += _FONT[letter][i].Replace("*", symbol) + "   ";
                    }
                }

                return outputText;
            }
            public static void Print(string text, Color color, (int, int) position, string symbol)
            {
                Console.SetCursorPosition(position.Item1, position.Item2);
                Console.Write($"\u001b[{(int)color}m");

                string[] outputText = TextCreate(text.ToUpper(), symbol);
                int lineCount = 0;

                foreach (string line in outputText)
                {
                    Console.SetCursorPosition(position.Item1, position.Item2 + lineCount);
                    Console.WriteLine(line);
                    lineCount++;
                }
            }
            public void Print(string text)
            {
                Console.SetCursorPosition(_POS.Item1, _POS.Item2);
                Console.Write($"\u001b[{(int)_COLOR}m");

                string[] outputText = TextCreate(text.ToUpper(), _SYMBOL);
                int lineCount = 0;

                foreach (string line in outputText)
                {
                    Console.SetCursorPosition(_POS.Item1, _POS.Item2 + lineCount);
                    Console.WriteLine(line);
                    lineCount++;
                }
            }     
            private static Dictionary<char, string[]> GetFont(string fileName)
            {
                Dictionary<char, string[]> font = new Dictionary<char, string[]>();
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\" + fileName);
                int i = 0;
                while (i < lines.Length)
                {
                    if (lines[i] is null || lines[i] == "")
                    {
                        i++;
                        continue;
                    }

                    char letter = lines[i][0];
                    i++;

                    List<string> letterLines = new List<string>();

                    while (i < lines.Length && !(lines[i] is null || lines[i] == ""))
                    {
                        letterLines.Add(lines[i]);
                        i++;
                    }
                    font[letter] = letterLines.ToArray();
                }

                return font;
            }

            public void Dispose()
            {
                Console.Write($"\u001b[u");
                Console.Write($"\u001b[{(int)Color.WHITE}m");
            }
        }        
        static void Main(string[] args)
        {
            BeautifulConsole.Print("Sedyakin", Color.RED, (10,10), "*");
            using (BeautifulConsole beautyCnsl = new BeautifulConsole(Color.YELLOW, (42,5), "S"))
            {
                beautyCnsl.Print("Sergey");
            }
            Console.ReadKey();
        }
    }
}
