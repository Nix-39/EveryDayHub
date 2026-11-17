using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
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
                    "Visa",
                    "Lägg till",
                    "Tillbaka"
                    ));

            switch (choice)
            {
                case "Visa":
                    ShowAll(store);
                    break;
                case "Lägg till":
                    AddRecipe(store);
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
        }

        var ingredientText = AnsiConsole.Prompt(
            new TextPrompt<string>("Ingredienser (separera med komma):").AllowEmpty());

        // Split delar på texten vid varje komma. TrimEntries tar bort mellanslag runt varje del och RemoveEmptyEntries tar bort tomma delar.
        // ToLowerInvariant ger små bokstäver, som i recipes.json. Distinct tar bort dubletter.
        var ingredients = ingredientText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(i => i.ToLowerInvariant())
            .Distinct()
            .ToList();

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

    private static bool Save(DataStore<Recipe> store)
    {
        if (store.SaveToJson())
        {
            return true;
        }

        AnsiConsole.MarkupLine("[red]{Markup.Escape(store.LastError)}[/]");
        return false;
    }
}

