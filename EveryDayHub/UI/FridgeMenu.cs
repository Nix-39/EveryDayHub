
using Spectre.Console;

namespace EveryDayHub.UI;

public static class FridgeMenu
{
	public static void Run()
	{
		bool running = true;

		while (running)
		{
			// Show the fridge menu options
			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("Kylskåpet")
					.AddChoices(
						"Visa alla matvaror",
						"Lägg till matvara",
						"Ändra matvara",
						"Ta bort matvara",
						"Tillbaka"
					));

			// Handle the selected option
			switch (choice)
			{
				case "Visa alla matvaror":
					AnsiConsole.MarkupLine("[yellow]Visa matvaror kommer snart.[/]");
					break;

				case "Lägg till matvara":
					AnsiConsole.MarkupLine("[yellow]Lägg till kommer snart.[/]");
					break;

				case "Ändra matvara":
					AnsiConsole.MarkupLine("[yellow]Ändra kommer snart.[/]");
					break;

				case "Ta bort matvara":
					AnsiConsole.MarkupLine("[yellow]Ta bort kommer snart.[/]");
					break;

				case "Tillbaka":
					running = false;
					break;
			}
		}
	}
}
