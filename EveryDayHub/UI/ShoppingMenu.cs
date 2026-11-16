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
                    .AddChoices("Visa listan", "Lägg till vara", "Bocka av vara", "Ta bort vara", "Tillbaka"));

            switch (choice)
            {
                case "Visa listan":    ShowList(); break;
                case "Lägg till vara": AddItem();  break;
                case "Bocka av vara":  ToggleItem(); break;
                case "Ta bort vara":   RemoveItem(); break;
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

    private void ToggleItem()
    {
        var item = PickItem("Vilken vara vill du bocka av?");
        if (item == null) return;

        item.IsBought = !item.IsBought;
        
        if (item.IsBought)
        {
            Messages.Success("Varan är avbockad!");
        }
        else
        {
            Messages.Info("Varan är inte längre avbockad.");
        }
    }

     // Tar bort en vara från listan.
    private void RemoveItem()
    {
        var item = PickItem("Vilken vara vill du ta bort?");
        if (item == null) return;

        // RemoveItem returnerar true om varan fanns och togs bort.
        if (_items.RemoveItem(item))
        {
            Messages.Success("Varan togs bort!");
        }
    }

    // Hjälpmetod: låter användaren välja en vara med pilarna.
    // ToggleItem och RemoveItem behöver båda välja en vara, så koden skrivs EN gång här.
    // "ShoppingItem?" betyder att svaret kan vara en vara ELLER null (ingen vald).
    private ShoppingItem? PickItem(string title)
    {
        var all = _items.GetAll();

        // Tom lista: det finns inget att välja.
        if (all.Count == 0)
        {
            Messages.Info("Inköpslistan är tom.");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<ShoppingItem>()
                .Title(title)
                // Så här ser varje vara ut i listan, till exempel "☐ Mjölk (2)".
                // Markup.Escape skyddar mot tecken som [ i varunamnet.
                .UseConverter(i => $"{(i.IsBought ? "✔" : "☐")} {Markup.Escape(i.Name)} ({i.Quantity})")
                .AddChoices(all));
    }
}