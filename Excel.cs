using System;
using System.Data.Common;
using System.IO.Compression;
using System.Xml;

namespace PseudoExcelReader
{
    public static class Funcs
    {
        public static string SelectFile(string currentDir, string message, string fileType = ".xlsx")
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
                    List<string> dirs = Directory.GetDirectories(currentDir).ToList();
                    List<string> files = Directory.GetFiles(currentDir).ToList();
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
                            if (!options[n].Contains('.'))
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
                            currentDir = previousDir.Last();
                            previousDir.RemoveAt(previousDir.Count - 1);
                            n = 0;
                            newDir = true;
                        }
                        break;
                }
            }
        }

        public static string SelectSheet(ZipArchive archive, string message)
        {
            List<string> IDs = new List<string>();
            List<string> names = new List<string>();

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.Contains("workbook"))
                {
                    StreamReader sr = new StreamReader(entry.Open());
                    XmlReader xr = XmlReader.Create(sr);
                    while (xr.Read())
                    {
                        if (xr.Name == "sheet")
                        {
                            names.Add(xr.GetAttribute("name"));
                            IDs.Add("sheet" + xr.GetAttribute("sheetId"));
                        }
                    }
                    xr.Close();
                    sr.Close();
                    break;
                }
            }

            int n = 0;      // Will keep track of the selected option
            int prevN = 0;  // And will be used to erase highlights
            Console.CursorVisible = false;

            // Write options to the console:
            Console.Clear();
            Console.SetCursorPosition(1, 0);
            Console.WriteLine(message + "\n");
            foreach (string option in names)
            {
                Console.WriteLine(option);
                Thread.Sleep(4);
            }

            while (true)
            {
                Highlight(prevN + 2, names[prevN], ConsoleColor.Black, ConsoleColor.White);
                Highlight(n + 2, names[n]);

                // Get user input:
                ConsoleKey input = Console.ReadKey().Key;
                prevN = n;
                switch (input)
                {
                    case ConsoleKey.UpArrow: if (n > 0) n--; break;
                    case ConsoleKey.DownArrow: if (n < names.Count - 1) n++; break;
                    case ConsoleKey.LeftArrow: n = 0; break;
                    case ConsoleKey.RightArrow: n = names.Count - 1; break;

                    case ConsoleKey.Enter:
                        Console.CursorVisible = true;
                        return IDs[n];
                }
            }
        }

        /// <summary>
        /// Reads the data directly from the ZipArchive
        /// </summary>
        /// <param name="archive">The ZipArchive in which the data is located</param>
        /// <returns>A List comprised of LærerPar</returns>
        public static List<LærerPar> GetPairs(ZipArchive archive)
        {
            List<int> raw = new List<int>();
            List<bool> isText = new List<bool>();
            List<string> sharedStrings = new List<string>();

            string sheetID = SelectSheet(archive, "På hvilket ark ligger dataene?");
            Console.Clear();

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.Contains(sheetID))
                {
                    StreamReader sr = new StreamReader(entry.Open());
                    XmlReader xr = XmlReader.Create(sr);
                    while (xr.Read())
                    {
                        string val = xr.Value;
                        int res;
                        if (Int32.TryParse(val, out res))
                        {
                            raw.Add(res);
                            if (xr.GetAttribute("t") != null) isText.Add(true);
                            else isText.Add(false);
                        }
                    }
                    xr.Close();
                    sr.Close();
                }
                else if (entry.FullName.Contains("sharedStrings"))
                {
                    StreamReader sr = new StreamReader(entry.Open());
                    XmlReader xr = XmlReader.Create(sr);
                    xr.MoveToContent();
                    while (xr.Read())
                    {
                        string val = xr.Value;
                        if (val != string.Empty) sharedStrings.Add(val);
                    }
                    xr.Close();
                    sr.Close();
                }
            }

            List<LærerPar> lærerPar = new List<LærerPar>();

            for (int i = 0; i < raw.Count; i += 3)
            {
                if (sharedStrings[raw[i]].Length < 5 && sharedStrings[raw[i + 1]].Length < 5)
                    lærerPar.Add(new LærerPar(raw[i + 2], sharedStrings[raw[i]], sharedStrings[raw[i + 1]]));
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

        public static List<List<string>> PlanToList(List<Dictionary<LærerPar, bool>> plan)
        {
            List<List<string>> newPlan = new List<List<string>>();
            List<LærerPar> pairs = plan[0].Keys.ToList();

            for (int i = 0; i < plan[0].Count; i++) newPlan.Add(new List<string>());
            foreach (List<string> row in newPlan)
            {
                int index = newPlan.IndexOf(row);
                row.AddRange(new List<string> { pairs[index].lærer1, pairs[index].lærer2 });
                for (int i = 0; i < plan.Count; i++)
                {
                    if (plan[i][pairs[index]]) row.Add("Optaget");
                    else row.Add("");
                }
            }

            return newPlan;
        }
    }
    public static class XmlFuncs
    {
        public static string XmlString(string name, Dictionary<string, string> atts, string value)
        {
            string output = $"<{name} ";
            foreach (KeyValuePair<string, string> att in atts)
            {
                output += $"{att.Key}=\"{att.Value}\" ";
            }
            output.Remove(output.Count() - 1);
            output += $">{value}</{name}>";
            return output;
        }
        public static string XmlString(string name, Dictionary<string, string> atts)
        {
            string output = $"<{name} ";
            foreach (KeyValuePair<string, string> att in atts)
            {
                output += $"{att.Key}=\"{att.Value}\" ";
            }
            output.Remove(output.Count() - 1);
            output += $"/>";
            return output;
        }
        public static string XmlString(string name, string value)
        {
            string output = $"<{name}>{value}</{name}>";
            return output;
        }
        public static string XmlString(string name, List<string> vals)
        {
            string output = $"<{name}>";
            foreach (string val in vals)
            {
                output += val;
            }
            output += $"</{name}>";
            return output;
        }
        public static string XmlString(string name, Dictionary<string, string> atts, List<string> vals)
        {
            string output = $"<{name} ";
            foreach (KeyValuePair<string, string> att in atts)
            {
                output += $"{att.Key}=\"{att.Value}\" ";
            }
            output.Remove(output.Count() - 1);
            output += ">";
            foreach (string val in vals)
            {
                output += val;
            }
            output += $"</{name}>";
            return output;
        }

        public static XmlElement AddSheet(string content, XmlDocument doc)
        {
            // Create a new book element.
            XmlElement sheet = doc.CreateElement("sheet");

            int sheetN = 3;

            // Create attributes for book and append them to the book element.
            XmlAttribute name = doc.CreateAttribute("name");
            XmlAttribute ID = doc.CreateAttribute("sheetId");
            XmlAttribute id = doc.CreateAttribute("r", "id", null);

            name.Value = "Resultat";
            ID.Value = sheetN.ToString();
            id.Value = "rId" + sheetN.ToString();

            sheet.Attributes.Append(name);
            sheet.Attributes.Append(ID);
            sheet.Attributes.Append(id);

            sheet.InnerXml = content;

            return sheet;
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