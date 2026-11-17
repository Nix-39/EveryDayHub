using System;                   // StringComparison
using System.Collections.Generic;
using System.Linq;             
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public class NotesMenu
{
    // Lådan där alla lappar ligger.
    private readonly DataStore<Note> _notes;

    public NotesMenu(DataStore<Note> notes)
    {
        _notes = notes;
    }

    public void Show()
    {
        bool back = false;
        while (!back)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Lappar[/]")
                    .AddChoices("Visa lappar", "Sök och filtrera", "Lägg till", "Ändra", "Ta bort", "Tillbaka"));

            switch (choice)
            {
                case "Visa lappar":      ShowNotes();   break;
                case "Sök och filtrera": SearchMenu();  break;  
                case "Lägg till":        AddNote();     break;
                case "Ändra":            EditNote();    break;
                case "Ta bort":          DeleteNote();  break;
                case "Tillbaka":         back = true;   break;
            }
        }
    }

    // Undermenyn för sökning
    // Egen meny så att lapp-menyn inte blir för lång. 
    private void SearchMenu()
    {
        bool back = false;
        while (!back)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Sök och filtrera lappar[/]")
                    .AddChoices("Sök på text", "Filtrera på person", "Filtrera på typ", "Sortera på datum", "Tillbaka"));

            switch (choice)
            {
                case "Sök på text": SearchByText(); break;
                case "Filtrera på person": FilterBy("vilken person?", n => n.Author); break;
                case "Filtrera på typ": FilterBy("vilken typ?", n => n.Type); break;
                case "Sortera på datum": SortByDate(); break;
                case "Tillbaka":    back = true;    break;

            }
        }
    }

    //sök på text med LINQ 
    private void SearchByText()
    {
        // Fråga vad användaren vill söka efter.
        string query = AnsiConsole.Ask<string>("Sök efter text:");

        // Where = sil: släpper bara igenom lappar där texten innehåller sökordet.
        // n => betyder "för varje lapp n, kolla om det här stämmer".
        // OrdinalIgnoreCase gör att "mjölk" också hittar "Mjölk".
        // ToList() gör resultatet till en lista som tabellen kan visa.
        var result = _notes.GetAll()
            .Where(n => n.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        DisplayTable(result);
    }

     // Filtrera på person eller typ.
    // "Func<Note, string> selector" är en liten instruktion som säger VILKEN egenskap
    // vi ska titta på. Då fungerar samma metod för både person och typ.
    private void FilterBy(string title, Func<Note, string> selector)
    {
        var all = _notes.GetAll();

        // DEL 1 + 2: plocka ut värdena, ta bort dubbletter och sortera dem.
        // Tips: all.Select(selector) ... och sedan Distinct() och OrderBy(v => v).
        // Avsluta med ToList().
        var values = all
            .Select(selector)
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        // Finns inga lappar finns inget att välja bland.
        if (values.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Det finns inga lappar än.[/]");
            return;
        }

        // Låt användaren välja ett av värdena med piltangenterna.
        string chosen = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .AddChoices(values));

        // DEL 3: behåll bara lappar där egenskapen är det valda värdet.
        // Tips: Where(n => selector(n) == chosen) och sedan ToList().
        var result = all
            .Where(n => selector(n) == chosen)
            .ToList();

        DisplayTable(result);
    }

        // Sortera på datum.
    private void SortByDate()
    {
        // Låt användaren välja i vilken ordning lapparna ska visas.
        string order = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sortera hur?")
                .AddChoices("Nyast först", "Äldst först"));

        var all = _notes.GetAll();

        // OrderByDescending = största värdet först (nyaste datumet överst).
        // OrderBy = minsta värdet först (äldsta datumet överst).
        // "villkor ? A : B" betyder: om villkoret stämmer, använd A, annars B.
        var sorted = order == "Nyast först"
            ? all.OrderByDescending(n => n.Date).ToList()
            : all.OrderBy(n => n.Date).ToList();

        DisplayTable(sorted);
    }
    private void ShowNotes()
    {
        DisplayTable(_notes.GetAll());
    }

    // Hjälpmetod som ritar tabellen för vilka lappar som helst 
    // Både "Visa lappar" och alla sökningar behöver en tabell, så vi skriver den bara en gång.
    private void DisplayTable(List<Note> notes)
    {
        // Inga lappar att visa: berätta det i stället för att rita en tom tabell.
        if (notes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Inga lappar hittades.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Typ");
        table.AddColumn("Text");
        table.AddColumn("Av");
        table.AddColumn("Datum");

        foreach (var n in notes)
        {
            // Markup.Escape skyddar mot att tecken som [ i en lapp tolkas som färgkoder.
            table.AddRow(
                Markup.Escape(n.Type),
                Markup.Escape(n.Text),
                Markup.Escape(n.Author),
                n.Date.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }

   
    private void AddNote()
    {
        var note = new Note
        {
            Type   = AnsiConsole.Ask<string>("Typ (t.ex. Viktigt, Påminnelse):"),
            Text   = AnsiConsole.Ask<string>("Text:"),
            Author = AnsiConsole.Ask<string>("Vem skriver lappen?"),
            Date   = DateTime.Now
        };
        _notes.AddItem(note);
    }

    private void EditNote()
    {
        var note = PickNote("Vilken lapp vill du ändra?");
        if (note == null) return;

        note.Text = AnsiConsole.Ask<string>("Ny text:");
    }

    private void DeleteNote()
    {
        var note = PickNote("Vilken lapp vill du ta bort?");
        if (note == null) return;

        _notes.RemoveItem(note);
    }

    private Note? PickNote(string title)
    {
        var all = _notes.GetAll();
        if (all.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Det finns inga lappar än.[/]");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<Note>()
                .Title(title)
                .UseConverter(n => $"{n.Type}: {n.Text}")
                .AddChoices(all));
    }
}