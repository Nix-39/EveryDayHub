using System;                   // AppContext
using System.IO;                // Path
using EveryDayHub.Models;       // klassen Note
using EveryDayHub.Services;     // klassen DataStore<T>
using EveryDayHub.UI;           // klassen MainMenu

namespace EveryDayHub
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Mappen där JSON-filerna sparas. AppContext.BaseDirectory är mappen där
            // programmet körs (bin/Debug/net10.0/), så sparad data hamnar utanför git.
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");

            // Sökvägen till filen där lapparna sparas.
            string notesPath = Path.Combine(dataFolder, "notes.json");

            // Skapa lådan för lappar EN gång här, så att laddning, meny och sparning
            // delar på SAMMA låda.
            var notesStore = new DataStore<Note>(notesPath);

            // Skicka in lådan till menyn.
            MainMenu.Run(notesStore);
        }
    }
}