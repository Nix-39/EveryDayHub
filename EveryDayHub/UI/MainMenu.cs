using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;


public static class MainMenu
{
    public static void Run()
    {
        bool running = true;

        // Skapa lådan där lapparna ligger.
        var notesStore = new DataStore<Note>("");

        var notesMenu = new NotesMenu(notesStore);

        while (running)  //while (running) upprepar menyn tills någon väljer Avsluta. Utan loopen stänger programmet efter första valet.
        {
            StartScreen.Show();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()   //SelectionPrompt är menyn man styr med piltangenter och Enter.
                    .Title("Vad vill du göra?")
                    .AddChoices(
                        "Lappar", // NYTT: valet som leder till lapp-menyn
                        "Avsluta" // Avsluta ligger alltid sist i menyn, så man kan trycka Enter direkt om man vill stänga programmet.
                    ));

            switch (choice) //switch är servitören: den tittar på vad du valde och skickar dig till rätt ställe. Varje kort lägger senare till ett eget case.
            {
                case "Lappar": // NYTT: valet som leder till lapp-menyn
                    notesMenu.Show();
                    break;
                case "Avsluta":
                    running = false;
                    break;
            }        

        }

        AnsiConsole.MarkupLine("[green]Hej då![/]");

    }
    

}