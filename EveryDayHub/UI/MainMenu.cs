using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public static class MainMenu
{
    public static void Run()
    {
        // Flagga som styr loopen. När den blir false stängs programmet.
        bool running = true;

        // Skapa lådan där lapparna ligger. Den skapas EN gång, före loopen,
        // annars blir den tom varje gång menyn visas.
        var notesStore = new DataStore<Note>("");

        // Skapa lapp-menyn och ge den lådan.
        var notesMenu = new NotesMenu(notesStore);

        // Schedule: one store for work shifts and activities, one for family members.
        // They are created once, before the loop, so the data is kept between menu visits.
        var boardStore = new DataStore<BoardItem>("board.json");
        var memberStore = new DataStore<FamilyMember>("members.json");

        while (running)  // upprepar menyn tills någon väljer Avsluta
        {
            StartScreen.Show();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()   // menyn man styr med piltangenter och Enter
                    .Title("Vad vill du göra?")
                    .AddChoices(
                        "Lappar",       // ditt val (kort 9)
                        "Kylskåpet",    // P3:s val (kort 10)
                        "Schema",       // P1:s val (kort 8)
                        "Avsluta"       // ligger alltid sist
                    ));

            switch (choice) // skickar dig till rätt ställe beroende på valet
            {
                case "Lappar":
                    notesMenu.Show();   // öppnar lapp-menyn
                    break;
                case "Kylskåpet":
                    FridgeMenu.Run();   // öppnar P3:s kylskåpsmeny
                    break;
                case "Schema":
                    ScheduleMenu.Run(boardStore, memberStore);   // opens the schedule menu
                    break;
                case "Avsluta":
                    running = false;    // avslutar loopen
                    break;
            }
        }

        AnsiConsole.MarkupLine("[green]Hej då![/]");
    }
}