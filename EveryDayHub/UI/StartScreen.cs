using Spectre.Console;

namespace EveryDayHub.UI;

public static class StartScreen   //static betyder att du kan skriva StartScreen.Show() direkt, utan att skapa ett objekt.
{
    public static void Show()
    {
        AnsiConsole.Clear();  //AnsiConsole.Clear() tömmer skärmen så bannern alltid hamnar överst.

        AnsiConsole.Write(new FigletText("EveryDayHub").Centered().Color(Color.Aqua)); //FigletText är den stora texten, som en skylt över restaurangen.


         AnsiConsole.MarkupLine("[grey]Familjens gemensamma anslagstavla[/]");
         AnsiConsole.WriteLine();
    }



}