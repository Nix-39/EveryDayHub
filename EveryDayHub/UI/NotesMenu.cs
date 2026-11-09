using Spectre.Console;          // ger oss tabeller, menyer och färger
using EveryDayHub.Models;       // ger oss klassen Note
using EveryDayHub.Services;     // ger oss klassen DataStore<T>

// namespace är mappen/adressen där klassen "bor". Den ska matcha resten av projektet.
namespace EveryDayHub.UI;

// En klass är en ritning. NotesMenu är ritningen för lapp-menyn (receptionisten).
public class NotesMenu
{
    private readonly DataStore<Note> _notes;

    public NotesMenu(DataStore<Note> notes)
    {
        _notes = notes;   // spara lådan i vår egen variabel
    }

    // Show() är den metod MainMenu anropar när användaren väljer "Lappar".
    public void Show()
    {
        // En flagga som säger "ska vi gå tillbaka till huvudmenyn?". Börjar som false (nej).
        bool back = false;

        // Loopen upprepar menyn om och om igen tills användaren väljer "Tillbaka".
        // !back betyder "så länge back INTE är true".
        while (!back)
        {
            // Visar en meny där man väljer med piltangenterna och trycker Enter.
            // choice blir den text användaren valde, t.ex. "Lägg till".
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Lappar[/]")   // [bold]...[/] gör texten fet
                    .AddChoices("Visa lappar", "Lägg till", "Ändra", "Ta bort", "Tillbaka"));

            // switch jämför choice med varje "case" och kör den som matchar.
            // Som en växel på en järnväg: beroende på valet åker vi åt olika håll.
            switch (choice)
            {
                case "Visa lappar": ShowNotes();  break;   // R = Read
                case "Lägg till":   AddNote();    break;   // C = Create
                case "Ändra":       EditNote();   break;   // U = Update
                case "Ta bort":     DeleteNote(); break;   // D = Delete
                case "Tillbaka":    back = true;  break;   // avsluta loopen
            }
            // "break" betyder "klart med det här fallet, hoppa ut ur switch".
        }
    }

    // R = Read: visa alla lappar i en tabell
    private void ShowNotes()
    {
        // Skapa en tom tabell och lägg till kolumnrubriker.
        var table = new Table();
        table.AddColumn("Typ");
        table.AddColumn("Text");
        table.AddColumn("Av");
        table.AddColumn("Datum");

        // foreach = "för varje lapp i lådan, gör följande".
        // GetAll() hämtar alla lappar från DataStore.
        foreach (var n in _notes.GetAll())
        {
            // Lägg till en rad i tabellen för just den här lappen.
            // ToString("yyyy-MM-dd") formaterar datumet som t.ex. 2026-11-09.
            table.AddRow(n.Type, n.Text, n.Author, n.Date.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }

    // C = Create: lägg till en ny lapp
    private void AddNote()
    {
        // Skapa ett nytt Note-objekt och fyll i egenskaperna.
        // AnsiConsole.Ask ställer en fråga och väntar på att användaren skriver ett svar.
        var note = new Note
        {
            Type   = AnsiConsole.Ask<string>("Typ (t.ex. Viktigt, Påminnelse):"),
            Text   = AnsiConsole.Ask<string>("Text:"),
            Author = AnsiConsole.Ask<string>("Vem skriver lappen?"),
            Date   = DateTime.Now   // datum och tid just nu, sätts automatiskt
        };

        // Stoppa in lappen i kartotekslådan.
        _notes.AddItem(note);
    }

    // U = Update: ändra en lapp
    private void EditNote()
    {
        // Låt användaren välja vilken lapp som ska ändras.
        var note = PickNote("Vilken lapp vill du ändra?");

        // Om inget valdes (t.ex. inga lappar finns) avslutar vi metoden direkt.
        if (note == null) return;

        // Ändra texten på den valda lappen. Eftersom note är samma objekt som ligger
        // i lådan ändras den direkt där också.
        note.Text = AnsiConsole.Ask<string>("Ny text:");
    }

    // D = Delete: ta bort en lapp 
    private void DeleteNote()
    {
        // Välj vilken lapp som ska bort.
        var note = PickNote("Vilken lapp vill du ta bort?");
        if (note == null) return;   // inget valt, avbryt

        // Ta ut lappen ur kartotekslådan.
        _notes.RemoveItem(note);
    }

    // Hjälpmetod: låter användaren välja en lapp med pilarna
    // Både EditNote och DeleteNote behöver välja en lapp, så vi skriver koden EN gång här
    // och återanvänder den. Det kallas att undvika duplicerad kod.
    // "Note?" betyder att svaret kan vara en Note eller null (ingenting).
    private Note? PickNote(string title)
    {
        // Hämta alla lappar.
        var all = _notes.GetAll();

        // Count = antal lappar.
        if (all.Count == 0)
        {
            // [yellow][/] gör texten gul.
            AnsiConsole.MarkupLine("[yellow]Det finns inga lappar än.[/]");
            return null;   // null = "ingen lapp valdes"
        }

        // Visa en pilmeny med alla lappar och returnera den som väljs.
        return AnsiConsole.Prompt(
            new SelectionPrompt<Note>()
                .Title(title)                                   // rubriken vi skickade in
                .UseConverter(n => $"{n.Type}: {n.Text}")       // hur varje lapp ska se ut i listan
                .AddChoices(all));                              // alla lappar som val
    }
}