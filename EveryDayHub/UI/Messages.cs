using Spectre.Console;

namespace EveryDayHub.UI;

// Färgade meddelanden som alla i gruppen använder, till exempel i sina catch-block
public static class Messages
{
    // Rött meddelande när något gick fel
    public static void Error(string message)
    {
        // Markup.Escape gör att text med hakparenteser inte kraschar programmet
        AnsiConsole.MarkupLine($"[red]Fel:[/] {Markup.Escape(message)}");
    }

    // Grönt meddelande när något gick bra
    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]Klart:[/] {Markup.Escape(message)}");
    }

    // Turkost meddelande med vanlig information
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[aqua]Info:[/] {Markup.Escape(message)}");
    }
}