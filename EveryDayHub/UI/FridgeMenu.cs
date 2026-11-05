using EveryDayHub.Models;
using EveryDayHub.Services;
using Spectre.Console;

namespace EveryDayHub.UI;

public static class FridgeMenu
{// Stores food items for the fridge menu
	private static readonly DataStore<FoodItem> _foodStore =
		new DataStore<FoodItem>("Data/fooditems.json");
	public static void Run()
	{// Load saved food items when opening the fridge menu
		_foodStore.LoadFromJson();
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
					ShowAllItems();
					Pause.Wait();
					break;

				case "Lägg till matvara":
					AddFoodItem();
					Pause.Wait();
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
	private static void ShowAllItems()
	{
		// Get all food items from the DataStore
		var items = _foodStore.GetAll();

		// Check if the fridge is empty
		if (items.Count == 0)
		{
			AnsiConsole.MarkupLine("[yellow]Inga matvaror finns ännu.[/]");
			return;
		}
		// Create a table to display food items
		var table = new Table();

		table.AddColumn("Namn");
		table.AddColumn("Typ");
		table.AddColumn("Tillgänglig");

		foreach (var item in items)
		{
			table.AddRow(
				item.Name,
				item.GetType().Name,
				item.IsAvailable ? "Ja" : "Nej"
			);
		}

		AnsiConsole.Write(table);
	}
	private static void AddFoodItem()
	{
		// Ask the user to select a food category
		var category = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title("Välj typ av matvara:")
				.AddChoices(
					"Mejeri",
					"Grönsak",
					"Kött",
					"Skafferi"
				)
		);

		// Ask for a valid food item name
		string name;

		do
		{
			name = AnsiConsole.Ask<string>("Ange matvarans namn:").Trim();

			if (string.IsNullOrWhiteSpace(name) || !name.Any(char.IsLetter))
			{
				AnsiConsole.MarkupLine(
					"[red]Namnet måste innehålla minst en bokstav.[/]"
				);
			}
		}
		while (string.IsNullOrWhiteSpace(name) || !name.Any(char.IsLetter));


		// Ask for quantity only for fridge items

		int quantity = 0;
		if (category != "Skafferi")
		{
			// Keep asking until the quantity is valid
			do
			{
				quantity = AnsiConsole.Ask<int>("Ange antal:");

				if (quantity < 0)
				{
					AnsiConsole.MarkupLine(
						"[red]Antalet kan inte vara negativt.[/]"
					);
				}
			}
			while (quantity < 0);
		}


		// Ask for expiration date only for fridge items
		DateTime expirationDate = DateTime.MinValue;

		if (category != "Skafferi")
		{
			expirationDate = AnsiConsole.Ask<DateTime>(
				"Ange utgångsdatum (åååå-mm-dd):"
			);
		}

		// Check if a pantry item is in stock
		bool inStock = false;

		if (category == "Skafferi")
		{
			inStock = AnsiConsole.Confirm(
				"Finns matvaran hemma?"
			);
		}

		// Generate a unique ID for the new food item
		var items = _foodStore.GetAll();

		int newId = items.Count == 0
			? 1
			: items.Max(item => item.Id) + 1;

		if (category == "Skafferi")
		{
			// Create a new pantry item
			var pantryItem = new Pantry
			{
				Id = newId,
				Name = name,
				InStock = inStock
			};

			_foodStore.AddItem(pantryItem);
		}
		else if (category == "Mejeri")
		{
			// Create a new dairy item
			var dairyItem = new Dairy
			{
				Id = newId,
				Name = name,
				Quantity = quantity,
				ExpirationDate = expirationDate
			};

			_foodStore.AddItem(dairyItem);
		}
		else if (category == "Grönsak")
		{
			// Create a new vegetable item
			var vegetableItem = new Vegetable
			{
				Id = newId,
				Name = name,
				Quantity = quantity,
				ExpirationDate = expirationDate
			};

			_foodStore.AddItem(vegetableItem);
		}
		else if (category == "Kött")
		{
			// Create a new meat item
			var meatItem = new Meat
			{
				Id = newId,
				Name = name,
				Quantity = quantity,
				ExpirationDate = expirationDate
			};

			_foodStore.AddItem(meatItem);
		}
		// Save all food items to the JSON file
		if (_foodStore.SaveToJson())
		{
			AnsiConsole.MarkupLine("[green]Matvaran har sparats![/]");
		}
		else
		{
			AnsiConsole.MarkupLine(
				$"[red]Kunde inte spara matvaran: {Markup.Escape(_foodStore.LastError)}[/]"
			);
		}
	}

}