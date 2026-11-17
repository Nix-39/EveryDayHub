using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public static class MainMenu
{
    public static void Run(DataStore<Note> notesStore, DataStore<ShoppingItem> shoppingStore)
    {
        // Skapar mappen "Data" om den inte finns. JSON-filerna sparas där. Om mappen redan finns görs inget.
        Directory.CreateDirectory("Data");

        // Lagrar familjemedlemmarna i en JSON-fil. DataStore är en generisk klass som kan lagra olika typer av objekt.
        var familyStore = new DataStore<FamilyMember>("Data/familymembers.json");
        var boardStore = new DataStore<BoardItem>("Data/schedule.json");

        // Första gången finns ingen fil, då startar listan tom. Finns filen laddas den, och vid fel visas ett rött meddelande.
        if (File.Exists("Data/familymembers.json") && !familyStore.LoadFromJson())
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(familyStore.LastError)}[/]");
            Pause.Wait();
        }

        if (File.Exists("Data/schedule.json") && !boardStore.LoadFromJson())
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(boardStore.LastError)}[/]");
            Pause.Wait();
        }

        // Recept lagras i samma fil som kylskåpet och startskärmen läser från.
        var recipeStore = new DataStore<Recipe>("Data/recipes.json");

        if (File.Exists("Data/recipes.json") && !recipeStore.LoadFromJson())
        {
            AnsiConsole.MarkupLine("[red]{Markup.Escape(recipeStore.LastError)}[/]");
            Pause.Wait();
        }

        // Skapa lapp-menyn och ge den lådan (notesStore) där lapparna ligger.
        var notesMenu = new NotesMenu(notesStore);

        // Schema: en store för arbetspass och aktiviteter. Personerna kommer från familyStore ovan.
        var shoppingMenu = new ShoppingMenu(shoppingStore); 
        bool running = true;

        while (running)  // while (running) upprepar menyn tills någon väljer Avsluta.
        {
                       StartScreen.Show(notesStore, boardStore);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()   // SelectionPrompt är menyn man styr med piltangenter och Enter.
                    .Title("Vad vill du göra?")
                    .AddChoices(

                        "Familjen",     // kort 7
                        "Lappar",       // kort 9
                        "Kylskåpet",    // kort 10
                        "Schema",       // kort 8
                        "Inköpslistan", // kort 19
                        "Recept",       // kort 11
                        "Avsluta"       // ligger alltid sist
                    ));

            switch (choice) // switch skickar dig till rätt ställe beroende på vad du valde.
            {
                case "Familjen":
                    FamilyMenu.Run(familyStore);   // Öppnar familjens rum tills användaren väljer Tillbaka.
                    break;
                case "Lappar":
                    notesMenu.Show();   // Öppnar lapp-menyn.
                    break;
                case "Kylskåpet":
                    FridgeMenu.Run();   // Öppnar kylskåpsmenyn.
                    break;
                case "Schema":
                    ScheduleMenu.Run(boardStore, familyStore);   // Öppnar schemat. Det använder familjens store för personerna.
                    break;
                case "Inköpslistan":
                    shoppingMenu.Show();
                    break;
                case "Recept":
                    RecipeMenu.Run(recipeStore); // Öppnar receptrummet tills användaren väljer tillbaka.
                    break;
                case "Avsluta":
                    running = false;    // Avslutar loopen.
                    break;
            }
        }

        AnsiConsole.MarkupLine("[green]Hej då![/]");
    }
}