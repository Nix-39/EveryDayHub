using System;                   // AppContext, Console
using System.IO;                // Path, File
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
            // Mappen där JSON-filerna sparas (bredvid programmet, utanför git).
            string dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");

            // Sökvägen till filen där lapparna sparas.
            string notesPath = Path.Combine(dataFolder, "notes.json");

            // Lådan för lappar. Skapas en gång så att alla delar samma låda.
            var notesStore = new DataStore<Note>(notesPath);

            // LADDA VID START: progress bar 
            // Felmeddelandet sparas här och skrivs ut när balken är klar.
            string loadError = "";

            AnsiConsole.Progress().Start(ctx =>
            {
                var notesTask = ctx.AddTask("Laddar lappar...");

                // Första körningen finns ingen fil. Det är normalt, så då hoppar vi över laddningen.
                if (File.Exists(notesPath))
                {
                    // LoadFromJson returnerar false om något gick fel.
                    // Felbeskrivningen finns då i notesStore.LastError.
                    if (!notesStore.LoadFromJson())
                    {
                        loadError = notesStore.LastError;
                    }
                }

                // Kort paus så att du hinner se balken. Tas bort i steg 6.
                Thread.Sleep(500);

                // Fyll balken till 100 %.
                notesTask.Increment(100);
            });

            // Om något gick fel: skriv ut felet i rött.
            // Markup.Escape skyddar mot att tecken som [ i felmeddelandet tolkas som färgkoder.
            if (loadError != "")
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(loadError)}[/]");

                // Vänta på en tangent så att användaren hinner läsa felet.
                AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
                Console.ReadKey(true);
            }

            // Visa menyn. Den använder lådan som precis laddades.
            MainMenu.Run(notesStore);
        }
    }
}