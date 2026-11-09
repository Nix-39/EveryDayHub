using Spectre.Console;


namespace EveryDayHub.UI;

// En paus-klass för att pausa programmet och vänta på användarens input.
// Anropas i slutet av varje meny för att ge användaren tid att läsa meddelanden innan programmet fortsätter.
public static class Pause
{
    public static void Wait()
    {
        AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
        Console.ReadKey(true);
    }
}
