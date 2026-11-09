using Spectre.Console;

namespace EveryDayHub.UI;

public static class FamilyMenu
{
    public static void Run()
    {
        bool running = true;
        while (running)
        {
            AnsiConsole.Clear();
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Familjemedlemmar - Vad vill du göra?")
                    .AddChoices(
                       "Tillbaka"
                    ));
            switch (choice)
            {
                case "Tillbaka":
                    running = false;
                    break;
            }

            if (running)
            {
                Pause.Wait();
            }
        }
    }
}
