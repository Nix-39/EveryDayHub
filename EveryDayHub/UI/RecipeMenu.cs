using EveryDayHub.Models;
using EveryDayHub.Services;
using Spectre.Console;

namespace EveryDayHub.UI;

// Rummet där recept hanteras. En undermeny där man kan visa alla, lägga till, ändra eller ta bort recept.
public static class RecipeMenu
{

    // Visar menyn och körs tills användaren väljer tillbaka
    public static void Run(DataStore<Recipe> store)
    {
        bool running = true;

        while (running)
        {
            // Rensar skärmen så rummet alltid startar rent
            AnsiConsole.Clear();

            // Menyn styrs med piltangenter. Texterna måste vara likadana i switch.
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Recept - Vad vill du göra?")
                .AddChoices(
                    "Visa alla",
                    "Visa ett recept",
                    "Lägg till",
                    "Ändra",
                    "Ta bort",
                    "Tillbaka"
                    ));

            switch (choice)
            {
                case "Visa alla":
                    ShowAll(store);
                    break;
                case "Visa ett recept":
                    ShowRecipe(store);
                    break;
                case "Lägg till":
                    AddRecipe(store);
                    break;
                case "Ändra":
                    EditRecipe(store);
                    break;
                case "Ta bort":
                    RemoveRecipe(store);
                    break;
                case "Tillbaka":
                    running = false;
                    break;
            }

            // Väntar på en tangent efter val utom tillbaka, så resultatet syns.
            if (running)
            {
                Pause.Wait();
            }
        }
    }

    // Visar alla recept i en tabell. Om listan är tom visas ett meddelande.
    private static void ShowAll(DataStore<Recipe> store)
    {
        var recipes = store.GetAll();

        if (recipes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Inga recept hittades.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Nr");
        table.AddColumn("Recept");
        table.AddColumn("Tid");
        table.AddColumn("Ingredienser");

        // OrderBy sorterar på Id så att ordningen är densamma varje gång.
        foreach (Recipe recipe in recipes.OrderBy(r => r.Id))
        {
            table.AddRow(
                recipe.Id.ToString(),
                Markup.Escape(recipe.Name),
                $"{recipe.CookingTimeMinutes} min",
                // string.Join sätter ihop ingredienslistan till en text med kommatecken emellan.
                Markup.Escape(string.Join(", ", recipe.Ingredients)));
        }

        AnsiConsole.Write(table);
    }

    // Menyval som visar ett recept med tidsåtgång, ingredienser och beskrivning.
    private static void ShowRecipe(DataStore<Recipe> store)
    {
        var recipe = ChooseRecipe(store, "Vilket recept vill du visa?");

        // Saknas recept avbryts operationen.
        if (recipe == null)
        {
            return;
        }

        // Varje ingrediens i receptet hamnar på en egen rad med ett streck framför.
        var ingredientLines = string.Join("\n", recipe.Ingredients.Select(i => $"- {Markup.Escape(i)}"));

            var content =
            $"[bold]Tillagningstid:[/] {recipe.CookingTimeMinutes} min\n\n" +
            $"[bold]Ingredienser:[/]\n{ingredientLines}\n\n" +
            $"[bold]Gör så här:[/]\n{Markup.Escape(recipe.Instructions)}";

            // Panel ritar en ram runt texten, med receptets namn som rubrik.
            var panel = new Panel(content)
            .Header(Markup.Escape(recipe.Name))
            .Border(BoxBorder.Rounded);

            AnsiConsole.Write(panel);
    }


    // Menyval som lägger till recept. Namn, tidsåtgång, ingredienser och beskrivning behövs fyllas i.
    private static void AddRecipe(DataStore<Recipe> store)
    {
        // Allow Empty släpper igenom Enter utan text, så att vi kan visa ett eget rött meddelande. Trim tar bort mellanslag i början och slut.
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Namn:").AllowEmpty()).Trim();

        if (name.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Namn kan inte vara tomt![/]");
            return;
        }

        var recipes = store.GetAll();

        if (recipes.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine("[red]Det finns redan ett recept med det namnet![/]");
            return;
        }

        // Tiden läses som text och görs sedan om till heltal, så att vi kan visa ett eget felmeddelande.
        var timeText = AnsiConsole.Prompt(
            new TextPrompt<string>("Tillagningstid i minuter:").AllowEmpty()).Trim();

        if (!int.TryParse(timeText, out int minutes) || minutes <= 0)
        {
            AnsiConsole.MarkupLine("[red]Tillagningstiden måste vara ett heltal större än 0![/]");
            return;
        }

        var ingredientText = AnsiConsole.Prompt(
            new TextPrompt<string>("Ingredienser (separera med komma):").AllowEmpty());

        var ingredients = ParseIngredients(ingredientText);

        if (ingredients.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Ange minst en ingrediens![/]");
            return;
        }

        var instructions = AnsiConsole.Prompt(
            new TextPrompt<string>("Beskrivning:").AllowEmpty()).Trim();

        if (instructions.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Beskrivningen kan inte vara tom![/]");
            return;
        }

        // Nästa lediga ID: Högsta nuvarande +1, eller 1 om listan är tom.
        int nextId = recipes.Count == 0 ? 1 : recipes.Max(r => r.Id) + 1;

        store.AddItem(new Recipe
        {
            Id = nextId,
            Name = name,
            CookingTimeMinutes = minutes,
            Ingredients = ingredients,
            Instructions = instructions
        });

        if (Save(store))
        {
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(name)} har lagts till.[/]");
        }
    }

    // Menyval som ändrar ett recept. Tryck enter för att behålla ett värde som det är.
    private static void EditRecipe(DataStore<Recipe> store)
    {
        var recipe = ChooseRecipe(store, "Vilket recept vill du ändra?");

        // Finns inga recept avbryts operationen.
        if (recipe == null)
        {
            return;
        }

        // Alla svar kontrolleras innan något ändras, så att ett fel inte lämnar ett recept halvändrat.
        var newName = AnsiConsole.Prompt(
            new TextPrompt<string>($"Nytt namn för {Markup.Escape(recipe.Name)} (Tryck enter för att behålla):")
            .AllowEmpty()).Trim();

        // Ett nytt namn får inte vara samma som ett annat recepts namn. Det egna receptet (samma Id) räknas inte.
        if (newName.Length > 0 && store.GetAll().Any(r => r.Id != recipe.Id && r.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine("[red]Det finns redan ett recept med det namnet![/]");
            return;
        }

        var timeText = AnsiConsole.Prompt(
            new TextPrompt<string>($"Ny tillagningstid i minuter (nu {recipe.CookingTimeMinutes}, Tryck enter för att behålla):")
            .AllowEmpty()).Trim();

        // Behåller den gamla tiden om inget skrivs.
        int newMinutes = recipe.CookingTimeMinutes;

        if (timeText.Length > 0 && (!int.TryParse(timeText, out newMinutes) || newMinutes <= 0))
        {
            AnsiConsole.MarkupLine("[red]Tillagningstiden måste vara ett heltal och större än 0![/]");
            return;
        }

        var ingredientText = AnsiConsole.Prompt(
            new TextPrompt<string>(
                $"Nya ingredienser, separera med komma (nu: {Markup.Escape(string.Join(", ", recipe.Ingredients))}). Tryck Enter för att behålla:")
            .AllowEmpty());

        // Behåller de gamla ingredienserna om inget skrivs.
        var newIngredients = recipe.Ingredients;

        if (ingredientText.Trim().Length > 0)
        {
            newIngredients = ParseIngredients(ingredientText);

            if (newIngredients.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Ange minst en ingrediens![/]");
                return;
            }
        }

        var newInstructions = AnsiConsole.Prompt(
            new TextPrompt<string>("Ny beskrivning (Tryck Enter för att behålla):").AllowEmpty()).Trim();

        // Om inget skrivs i något fält ändras ingenting.
        if (newName.Length == 0 && timeText.Length == 0 && ingredientText.Trim().Length == 0 && newInstructions.Length == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Ingenting ändrades.[/]");
            return;
        }

        // Härifrån ändras receptet, men bara om något skrevs.
        if (newName.Length > 0)
        {
            recipe.Name = newName;
        }

        recipe.CookingTimeMinutes = newMinutes;
        recipe.Ingredients = newIngredients;

        if (newInstructions.Length > 0)
        {
            recipe.Instructions = newInstructions;
        }

        if (Save(store))
        {
            AnsiConsole.MarkupLine("[green]Receptet har ändrats.[/]");
        }
    }

    // Menyval som tar bort ett recept ur listan.
    private static void RemoveRecipe(DataStore<Recipe> store)
    {
        var recipe = ChooseRecipe(store, "Vilket recept vill du ta bort?");

        // Saknas recept avbryts operationen
        if (recipe == null)
        {
            return;
        }

        // Standardval är Nej, så inget recept tas bort av misstag.
        if (!AnsiConsole.Confirm($"Ta bort {Markup.Escape(recipe.Name)}?", false))
        {
            AnsiConsole.MarkupLine("[yellow]Ingenting togs bort.[/]");
            return;
        }

        // RemoveItem tar bort samma objekt som valdes i ChooseRecipe. Save sparar ändringar i JSON-filen.
        store.RemoveItem(recipe);

        if (Save(store))
        {
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(recipe.Name)} har tagits bort.[/]");
        }
    }

    // Sparar ändringar i JSON-filen och visar eventuella felmeddelanden.
    private static bool Save(DataStore<Recipe> store)
    {
        if (store.SaveToJson())
        {
            return true;
        }

        AnsiConsole.MarkupLine($"[red]{Markup.Escape(store.LastError)}[/]");
        return false;
    }

    // Gör om en text med kommatecken till en lista med ingredienser. Split delar texten vid varje komma.
    // TrimEntries tar bort mellanslag runt varje del och RemoveEmptyEntries tar bort tomma delar.
    // ToLowerInvariant ger små bokstäver, som i recipes.json. Distinct tar bort dubbletter.
    private static List<string> ParseIngredients(string text)
    {
        return text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(i => i.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    // Visar en lista med recept att välja från. Är listan med recept tom visas ett meddelande.
    private static Recipe? ChooseRecipe(DataStore<Recipe> store, string title)
    {
        var recipes = store.GetAll();

        if (recipes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Inga recept hittades.[/]");
            return null;
        }

        // UseConverter bestämmer vilken text som visas för varje alternativ. OrderBy sorterar listan på Id.
        return AnsiConsole.Prompt(
            new SelectionPrompt<Recipe>()
            .Title(title)
            .UseConverter(r => Markup.Escape(r.Name))
            .AddChoices(recipes.OrderBy(r => r.Id)));
    }
}


