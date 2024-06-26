﻿// Avoid using COM dependencies (Not included in build!)
using PseudoExcelReader;
using System;
using System.IO;
using System.Collections.Generic;

// Find a .txt file:
string path = Funcs.SelectFile("C:\\", "Vælg venligst tekst-filen (Brug piletasterne):");

// -------------- The Program --------------
Console.CursorVisible = false;
Console.ResetColor();
Console.Clear();
int i;
Dictionary<string, bool> tilgængeligeLærere = new Dictionary<string, bool>();
List<LærerPar> pairs = Funcs.GetPairs(path);
Console.WriteLine("Tænker...");
List<Dictionary<LærerPar, bool>> plan = new List<Dictionary<LærerPar, bool>>();

// Calculate available teachers:
foreach (LærerPar pair in pairs)
{
    if (!tilgængeligeLærere.ContainsKey(pair.lærer1)) tilgængeligeLærere.Add(pair.lærer1, true);
    if (!tilgængeligeLærere.ContainsKey(pair.lærer2)) tilgængeligeLærere.Add(pair.lærer2, true);
}

// Make a plan:
bool planlægning = false;
for (int blok = 0; !planlægning; blok++)
{
    planlægning = true;
    plan.Add(new Dictionary<LærerPar, bool>());

    // Gør alle lærere tilgængelige igen:
    foreach (KeyValuePair<string, bool> pair in tilgængeligeLærere)
    {
        tilgængeligeLærere[pair.Key] = true;
    }

    // Lav en blok
    plan[blok].Clear();
    while (true)
    {
        LærerPar prioPar = new LærerPar(-1, "", "");
        foreach (LærerPar pair in pairs)
        {
            try
            {
                plan[blok].Add(pair, false);
            }
            catch
            {

            }
        }
        foreach (LærerPar pair in pairs)
        {
            if (pair.møder > prioPar.møder && pair.møder > 0 && tilgængeligeLærere[pair.lærer1] && tilgængeligeLærere[pair.lærer2])
            {
                prioPar = pair;
            }
        }
        if (prioPar.møder == -1) break;
        else
        {
            tilgængeligeLærere[prioPar.lærer1] = false;
            tilgængeligeLærere[prioPar.lærer2] = false;
            plan[blok][prioPar] = true;
            pairs[pairs.IndexOf(prioPar)].møder--;
        }
    }

    // Tjek om alle lærere er blevet tildelt møder:
    foreach (LærerPar pair in pairs)
    {
        if (pair.møder > 0) planlægning = false;
    }
}

// Print the plan:
string[] parts = path.Split('\\');
string outPath = "";
foreach(string part in parts)
{
    if(!part.Contains(".txt")) outPath += part + "\\";
}

DateTime dateMod = DateTime.Now;
outPath += "Resultat - " + dateMod.Millisecond.ToString() + ".txt";

string output = "";

foreach(LærerPar pair in pairs)
{
    string row = pair.lærer1 + '\t' + pair.lærer2 + '\t';
    for(i = 0; i < plan.Count; i++)
    {
        if (plan[i][pair]) row += "Optaget\t";
        else row += "\t";
    }
    row += "\n";
    output += row;
}

File.WriteAllText(outPath, output);

Console.Clear();
Console.WriteLine("Tryk på en knap for at afslutte programmet");
Console.ReadKey();