using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;

namespace PseudoExcelReader
{
    public static class Funcs
    {
        public static string SelectFile(string currentDir, string message, string fileType = ".txt")
        {
            Console.CursorVisible = false;
            int firstOption = 0;
            List<string> previousDir = new List<string>();
            List<string> options = new List<string>();
            bool newDir = true;
            bool newBounds = true;
            string path = "";
            int n = 0;

            while (true)
            {
                int visibleCount = Console.WindowHeight - 3;
                if (newDir)
                {
                    string[] files = Directory.GetFiles(currentDir);
                    options = [.. Directory.GetDirectories(currentDir)];
                    options.AddRange(from file in files where file.Contains(fileType) select file);
                    firstOption = 0;
                }

                if (newBounds || newDir)
                {
                    Console.Clear();
                    Console.WriteLine(message);
                    Console.SetCursorPosition(0, 2);
                    for (int i = 0; i < visibleCount && i + firstOption < options.Count; i++)
                        Console.WriteLine("  " + options[i + firstOption].Replace(currentDir, ""));
                }

                Console.SetCursorPosition(0, n - firstOption + 2);
                Console.Write('>');
                Console.SetCursorPosition(0, n - firstOption + 3);
                Console.Write(' ');
                Console.SetCursorPosition(0, n - firstOption + 1);
                Console.Write(' ');

                newDir = false;
                newBounds = false;
                // Get user input:
                ConsoleKey input = Console.ReadKey().Key;
                switch (input)
                {
                    case ConsoleKey.UpArrow: 
                        if (n > 0) n--;
                        break;
                    case ConsoleKey.DownArrow: 
                        if (n < options.Count - 1) n++;
                        break;
                    case ConsoleKey.LeftArrow: 
                        n = 0; 
                        firstOption = 0; 
                        newBounds = true; 
                        break;
                    case ConsoleKey.RightArrow: 
                        n = options.Count - 1;
                        firstOption = Math.Max(options.Count - visibleCount, 0); 
                        newBounds = true; 
                        break;

                    case ConsoleKey.Enter:
                        if (options[n].Substring(options[n].Length - fileType.Length) == fileType)
                        {
                            path = options[n];
                            Console.CursorVisible = true;
                            return path;
                        }
                        else
                        {
                            if (!options[n].Contains("."))
                            {
                                previousDir.Add(currentDir);
                                currentDir = options[n];
                                n = 0;
                                newDir = true;
                            }
                        }
                        break;

                    case ConsoleKey.Escape:
                        if (previousDir.Count > 0)
                        {
                            currentDir = previousDir[previousDir.Count - 1];
                            previousDir.RemoveAt(previousDir.Count - 1);
                            n = 0;
                            newDir = true;
                        }
                        break;
                }
                if (n - firstOption >= visibleCount) { firstOption++; newBounds = true; }
                else if (n - firstOption < 0) { firstOption--; newBounds = true; }
            }
        }
        public static List<LærerPar> GetPairs(string path)
        {
            List<LærerPar> lærerPar = new List<LærerPar>();
            lærerPar.Clear();
            string content = File.ReadAllText(path);
            string[] rowsArray = content.Split('\n');
            List<string> rows = [.. rowsArray];
            foreach (string row in rows)
            {
                if (row.Length < 5) continue;
                string[] col = row.Split('\t');
                lærerPar.Add(new LærerPar(Int32.Parse(col[2].Trim()), col[0], col[1]));
            }
            return lærerPar;
        }
    }
    public class LærerPar
    {
        public int møder;
        public string lærer1;
        public string lærer2;
        public LærerPar(int møder, string lærer1, string lærer2)
        {
            this.møder = møder;
            this.lærer1 = lærer1;
            this.lærer2 = lærer2;
        }
    }
}