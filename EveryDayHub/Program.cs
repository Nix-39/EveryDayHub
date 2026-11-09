using System;                   // AppContext, Console
using System.IO;                // Path, File, Directory
using EveryDayHub.Models;       // klassen Note
using EveryDayHub.Services;     // klassen DataStore<T>
using EveryDayHub.UI;           // klassen MainMenu
using System.Threading;         // Thread.Sleep
using Spectre.Console;          // progress bar, spinner och färger

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

                Thread.Sleep(500);

                // Fyll balken till 100 %.
                notesTask.Increment(100);
            });

            // Om något gick fel: skriv ut felet i rött.
            if (loadError != "")
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(loadError)}[/]");

                // Vänta på en tangent så att användaren hinner läsa felet.
                AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
                Console.ReadKey(true);
            }

            // MENYN 
            // Programmet fortsätter på nästa rad först när användaren väljer Avsluta.
            MainMenu.Run(notesStore);


            //  SPARA VID AVSLUT: spinner 
            // Felmeddelandet sparas här och skrivs ut när spinnern är klar.
            string saveError = "";

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)        // hur symbolen ser ut
                .Start("Sparar data...", ctx =>     // texten bredvid symbolen
                {
                    // DataStore skapar inte mappen själv, så vi gör det.
                    // Finns den redan händer ingenting.
                    Directory.CreateDirectory(dataFolder);

                    // SaveToJson returnerar false om något gick fel.
                    // Felbeskrivningen finns då i notesStore.LastError.
                    if (!notesStore.SaveToJson())
                    {
                        saveError = notesStore.LastError;
                    }

                    Thread.Sleep(800);
                });

            // Rött fel om det gick snett, annars ett grönt klart-meddelande.
            if (saveError != "")
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(saveError)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[green]Allt sparat![/]");
            }

            
        }
    }
}