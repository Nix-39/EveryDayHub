using Spectre.Console;

namespace EveryDayHub.UI;

public static class Messages
{
    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]Done:[/] {Markup.Escape(message)}");
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[aqua]Info:[/] {Markup.Escape(message)}");
    }
}