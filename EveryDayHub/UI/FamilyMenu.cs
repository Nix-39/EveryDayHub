using EveryDayHub.Models;
using EveryDayHub.Services;
using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace EveryDayHub.UI;

// Rummet där familjemedlemmar hanteras. En undermeny där man kan visa alla, lägga till, ändra namn eller ta bort familjemedlemmar.

public static class FamilyMenu
{
    // Visar menyn och körs tills användaren väljer tillbaka
    public static void Run(DataStore<FamilyMember> store)
    {
        bool running = true;
        while (running)
        {
            // Rensar skärmen så rummet alltid börjar rent
            AnsiConsole.Clear();

            // Menyn styrs med piltangenter. Texterna måste vara likadana i switch.
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Familjemedlemmar - Vad vill du göra?")
                    .AddChoices(
                        "Visa",
                        "Lägg till",
                        "Ändra",
                        "Ta bort",
                        "Tillbaka"
                    ));

            switch (choice)
            {
                case "Visa":
                    ShowAll(store);
                    break;
                case "Lägg till":
                    AddMember(store);
                    break;
                case "Ändra":
                    EditMember(store);
                    break;
                case "Ta bort":
                    RemoveMember(store);
                    break;
                case "Tillbaka":
                    running = false;
                    break;
            }

            // Väntar på en tangenttryckning efter varje val förutom tillbaka, så resultatet visas i skärmen utan att studsa tillbaka till menyn.
            if (running)
            {
                Pause.Wait();
            }
        }
    }

    // Visar alla familjemedlemmar i en tabell, om listan är tom visas ett meddelande.
    private static void ShowAll(DataStore<FamilyMember> store)
    {
        var members = store.GetAll();

        if (members.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Inga familjemedlemmar hittades.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Namn");

        foreach (FamilyMember member in members)
        {
            // Markup.Escape åtsidoser specialtecken som [] och skriver ut tecknen.
            table.AddRow(Markup.Escape(member.Name));
        }

        AnsiConsole.Write(table);
    }

    private static void AddMember(DataStore<FamilyMember> store)
    {
        // AllowEmpty tillåter att användaren trycker Enter utan att skriva något. Trim tar bort mellanslag i början och slutet av strängen.
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Namn:").AllowEmpty()).Trim();

        if (name.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Namn kan inte vara tomt.[/]");
            return;
        }

        // Nästa lediga id: Högsta nuvarande och +1, startar på 1 om det är tomt.
        var members = store.GetAll();
        int nextId = members.Count == 0 ? 1 : members.Max(m => m.Id) + 1;

        store.AddItem(new FamilyMember { Id = nextId, Name = name });
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(name)} har lagts till.[/]");
    }

    // Menyval som ändrar namn på en familjemedlem. 
    private static void EditMember(DataStore<FamilyMember> store)
    {
        var member = ChooseMember(store, "Vem vill du ändra?");

        // Saknas användare avbryts operationen.
        if (member == null)
        {
            return;
        }

        var newName = AnsiConsole.Prompt(
            new TextPrompt<string>($"Nytt namn för {Markup.Escape(member.Name)} (Tryck Enter för att behålla):")
                .AllowEmpty()).Trim();

        // Om inget skrivs ändras inte namnet.
        if (newName.Length == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Namnet ändrades inte.[/]");
            return;
        }

        // Ändrar namnet på familjemedlemmen och sparar i lagret.
        member.Name = newName;
        AnsiConsole.MarkupLine("[green]Namnet har ändrats.[/]");
    }

    // Menyval för att radera en familjemedlem ur listan.
    private static void RemoveMember(DataStore<FamilyMember> store)
    {
        var member = ChooseMember(store, "Vem vill du ta bort?");
        if (member == null)
        {
            return;
        }

        // Standardsvar vid radering är Nej så ingen raderas av misstag.
        if (!AnsiConsole.Confirm($"Ta bort {Markup.Escape(member.Name)}?", false))
        {
            AnsiConsole.MarkupLine("[yellow]Ingenting togs bort.[/]");
            return;
        }

        store.RemoveItem(member);
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(member.Name)} har tagits bort.[/]");
    }
    
    // Visar en lista med familjemedlemmar att välja från. Saknas medlemmar visas ett meddelande.
    private static FamilyMember? ChooseMember(DataStore<FamilyMember> store, string title)
    {
        var members = store.GetAll();

        if (members.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Inga familjemedlemmar än.[/]");
            return null;
        }

        // UseConverter bestämmer vilken text som visas för varje alternativ i listan.
        return AnsiConsole.Prompt(
            new SelectionPrompt<FamilyMember>()
            .Title(title)
            .UseConverter(m => Markup.Escape(m.Name))
            .AddChoices(members));
    }
}
