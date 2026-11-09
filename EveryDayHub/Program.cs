using System;                   // AppContext
using System.IO;                // Path
using EveryDayHub.Models;       // klassen Note
using EveryDayHub.Services;     // klassen DataStore<T>
using EveryDayHub.UI;           // klassen MainMenu
using System.Threading;         // Thread.Sleep
using Spectre.Console;          // progress bar och färger
namespace EveryDayHub
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");

            // Sökvägen till filen där lapparna sparas.
            string notesPath = Path.Combine(dataFolder, "notes.json");

            var notesStore = new DataStore<Note>(notesPath);

            AnsiConsole.Progress().Start(ctx =>
            {
                var notesTask = ctx.AddTask("Laddar lappar...");

                // Första körningen finns ingen fil. Det är normalt, 
                // laddningen och lådan förblir tom.
                if (File.Exists(notesPath))
                {
                    // LoadFromJson läser filen in i lådan. Returnerar false om något gick fel.
                    notesStore.LoadFromJson();
                }

                // Kort paus så att du hinner se balken. Tas bort i steg 6.
                Thread.Sleep(500);

                // Fyll balken till 100 %.
                notesTask.Increment(100);
            });
            MainMenu.Run(notesStore);
        }
    }
}