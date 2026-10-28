using Spectre.Console;

namespace EveryDayHub.UI;


public static class MainMenu
{
    public static void Run()
    {
        
        bool running = true;

        while (running)  //while (running) upprepar menyn tills någon väljer Avsluta. Utan loopen stänger programmet efter första valet.
        {
            StartScreen.Show();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()   //SelectionPrompt är menyn man styr med piltangenter och Enter.
                    .Title("Va vill du göra?")
                    .AddChoices(
                        "avsluta"
                    ));

            switch (choice) //switch är servitören: den tittar på vad du valde och skickar dig till rätt ställe. Varje kort lägger senare till ett eget case.
            {
                case "avsluta":
                    running = false;
                    break;
            }        

        }

        AnsiConsole.MarkupLine("[green]Hej då![/]");

    }


}