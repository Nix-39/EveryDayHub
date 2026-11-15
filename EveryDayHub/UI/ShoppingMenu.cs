using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public class ShoppingMenu
{
    // Lådan där alla varor ligger. Den skapas i Program.cs och skickas in hit,
    // precis som med lapparna.
    private readonly DataStore<ShoppingItem> _items;

    public ShoppingMenu(DataStore<ShoppingItem> items)
    {
        _items = items;
    }

    // Show() anropas från huvudmenyn när användaren väljer Inköpslistan.
    public void Show()
    {
        bool back = false;
        while (!back)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Inköpslistan[/]")
                    .AddChoices("Visa listan", "Lägg till vara", "Tillbaka"));

            switch (choice)
            {
                case "Visa listan":    ShowList(); break;
                case "Lägg till vara": AddItem();  break;
                case "Tillbaka":       back = true; break;
            }
        }
    }

    // Visar alla varor i en tabell.
    private void ShowList()
    {
        var all = _items.GetAll();

        // Tom lista: berätta det i stället för att rita en tom tabell.
        if (all.Count == 0)
        {
            Messages.Info("Inköpslistan är tom.");
            return;
        }

        var table = new Table();
        table.AddColumn("Klar");
        table.AddColumn("Vara");
        table.AddColumn("Antal");

        foreach (var item in all)
        {
            // En grön bock om varan är köpt, annars en tom ruta.
            string status = item.IsBought ? "[green]✔[/]" : "[grey]☐[/]";

            // Markup.Escape skyddar mot att tecken som [ i en vara tolkas som färgkoder.
            // Quantity är ett tal, så vi gör om det till text med ToString().
            table.AddRow(status, Markup.Escape(item.Name), item.Quantity.ToString());
        }

        AnsiConsole.Write(table);
    }

    // Lägger till en ny vara på listan.
    private void AddItem()
    {
        // Nästa lediga Id: största Id som finns + 1 (eller 1 om listan är tom).
        // Max() kraschar på en tom lista, därför Select(...).DefaultIfEmpty(0).
        int nextId = _items.GetAll().Select(i => i.Id).DefaultIfEmpty(0).Max() + 1;

        var item = new ShoppingItem
        {
            Id = nextId,
            Name = AnsiConsole.Ask<string>("Vilken vara?"),
            // Ask<int> kräver ett heltal och frågar igen om användaren skriver något annat.
            Quantity = AnsiConsole.Ask<int>("Hur många? (t.ex. 2):"),
            IsBought = false   // en ny vara är aldrig köpt än
        };

        _items.AddItem(item);
        Messages.Success("Varan lades till!");
    }
}