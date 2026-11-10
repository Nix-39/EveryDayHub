using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;


public static class MainMenu
{
	public static void Run()
	{

        // Lagrar familjemedlemmarna i en JSON-fil. DataStore är en generisk klass som kan lagra olika typer av objekt.
        var familyStore = new DataStore<FamilyMember>("Data/familymembers.json");
        bool running = true;

		while (running)  //while (running) upprepar menyn tills någon väljer Avsluta. Utan loopen stänger programmet efter första valet.
		{
			StartScreen.Show();

			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()   //SelectionPrompt är menyn man styr med piltangenter och Enter.
					.Title("Vad vill du göra?")
					.AddChoices(
						"Familjen",
                        "Kylskåpet",
						"Avsluta"
					));

			switch (choice) //switch är servitören: den tittar på vad du valde och skickar dig till rätt ställe. Varje kort lägger senare till ett eget case.
			{
				case "Familjen":
                    FamilyMenu.Run(familyStore); // Öppnar familjens rum tills användaren väljer tillbaka.
                    break;
                case "Kylskåpet":
					FridgeMenu.Run();
					break;
				case "Avsluta":
					running = false;
					break;
			}

		}

		AnsiConsole.MarkupLine("[green]Hej då![/]");

	}


}