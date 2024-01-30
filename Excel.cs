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
            // Declare some variables to use later:
            List<string> previousDir = new List<string>();
            List<string> options = new List<string>();
            int n = 0;      // Will keep track of the selected option
            int prevN = 0;  // And will be used to erase highlights
            string path;    // The final result
            Console.CursorVisible = false;
            bool newDir = true;
            while (true)
            {
                if (newDir)
                {
                    string[] fileArray = Directory.GetFiles(currentDir);
                    string[] dirsArray = Directory.GetDirectories(currentDir);
                    List<string> dirs = new List<string>();
                    foreach (string dir in dirsArray)
                    {
                        dirs.Add(dir);
                    }
                    List<string> files = new List<string>();
                    foreach(string file in fileArray)
                    {
                        files.Add(file);
                    }
                    List<string> remove = new List<string>();
                    foreach (string file in files)
                    {
                        if (!file.Contains(fileType)) remove.Add(file);
                    }
                    foreach (string item in remove)
                    {
                        files.Remove(item);
                    }
                    options.Clear();
                    options.AddRange(dirs.ToArray());
                    options.AddRange(files.ToArray());

                    // Write options to the console:
                    Console.Clear();
                    Console.SetCursorPosition(1, 0);
                    Console.WriteLine(message + "\n");
                    foreach (string option in options)
                    {
                        Console.WriteLine(option.Replace(currentDir, ""));
                        Thread.Sleep(4);
                    }
                }
                else Highlight(prevN + 2, options[prevN].Replace(currentDir, ""), ConsoleColor.Black, ConsoleColor.White);
                newDir = false;

                Highlight(n + 2, options[n].Replace(currentDir, ""));

                // Get user input:
                ConsoleKey input = Console.ReadKey().Key;
                prevN = n;
                switch (input)
                {
                    case ConsoleKey.UpArrow: if (n > 0) n--; break;
                    case ConsoleKey.DownArrow: if (n < options.Count - 1) n++; break;
                    case ConsoleKey.LeftArrow: n = 0; break;
                    case ConsoleKey.RightArrow: n = options.Count - 1; break;

                    case ConsoleKey.Enter:
                        string end = options[n].Substring(options[n].Length - fileType.Length);
                        if (end == fileType)
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
            }
        }

        public static List<LærerPar> GetPairs(string path)
        {
            List<LærerPar> lærerPar = new List<LærerPar>();
            lærerPar.Clear();
            string content = File.ReadAllText(path);
            string[] rowsArray = content.Split('\n');
            List<string> rows = new List<string>();
            foreach(string row in rowsArray)
            {
                rows.Add(row);
            }
            rows.RemoveAt(rows.Count - 1);
            foreach (string row in rows)
            {
                string[] col = row.Split('\t');
                lærerPar.Add(new LærerPar(Int32.Parse(col[2].Trim()), col[0], col[1]));
            }
            return lærerPar;
        }

        public static void Highlight(int line, string option, ConsoleColor hColor = ConsoleColor.White, ConsoleColor tColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = hColor;
            Console.ForegroundColor = tColor;
            Console.SetCursorPosition(0, line);
            Console.Write(option);
            Console.ResetColor();
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
        public LærerPar() { }

    }
}