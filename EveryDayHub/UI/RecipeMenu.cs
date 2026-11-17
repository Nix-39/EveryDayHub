using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
                    "Tillbaka"
                    ));

            switch (choice)
            {
                case "Visa":
                    ShowAll(store);
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
        foreach (Recipe recipe in recipes.OrderBy(recept => recept.Id))
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
}

