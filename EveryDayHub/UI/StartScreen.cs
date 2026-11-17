using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public static class StartScreen
{
    public static void Show(
        DataStore<Note> notesStore,
        DataStore<BoardItem> boardStore)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("EveryDayHub")
                .Centered()
                .Color(Color.Aqua)
        );

        AnsiConsole.MarkupLine(
            "[grey]Familjens gemensamma anslagstavla[/]"
        );

        AnsiConsole.WriteLine();

        // Display today's work shifts and activities.
        AnsiConsole.MarkupLine("[yellow]Dagens schema[/]");

        var today = DateTime.Today;

        var todaySchedule = boardStore.GetAll()
            .Where(item =>
                (item is WorkShift shift && shift.Start.Date == today) ||
                (item is Activity activity && activity.Start.Date == today))
            .OrderBy(item => item is WorkShift shift
                ? shift.Start
                : ((Activity)item).Start)
            .ToList();

        if (todaySchedule.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[grey]Inga aktiviteter planerade idag.[/]"
            );
        }
        else
        {
            foreach (var item in todaySchedule)
            {
                AnsiConsole.MarkupLine(
                    $"[white]{Markup.Escape(item.Describe())}[/]"
                );
            }
        }

        AnsiConsole.WriteLine();

        // Display notes on the start screen.
        AnsiConsole.MarkupLine("[yellow]Lappar[/]");

        var notes = notesStore.GetAll();

        if (notes.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[grey]Inga lappar finns ännu.[/]"
            );
        }
        else
        {
            foreach (var note in notes)
            {
                AnsiConsole.MarkupLine(
                    $"[white]{Markup.Escape(note.Text)}[/] - [grey]{Markup.Escape(note.Author)}[/]"
                );
            }
        }

        AnsiConsole.WriteLine();

        // Display a dinner suggestion based on available ingredients.
        AnsiConsole.MarkupLine("[yellow]Middagsförslag[/]");

        var recipeStore = new DataStore<Recipe>("Data/recipes.json");
        var foodStore = new DataStore<FoodItem>("Data/fooditems.json");

        bool recipesLoaded = recipeStore.LoadFromJson();
        bool foodLoaded = foodStore.LoadFromJson();

        if (!recipesLoaded || !foodLoaded)
        {
            AnsiConsole.MarkupLine(
                "[grey]Middagsförslag är inte tillgängligt just nu.[/]"
            );
        }
        else
        {
            var availableItems = foodStore.GetAll()
                .Where(item => item.IsAvailable)
                .ToList();

            var dinnerSuggestion = recipeStore.GetAll()
                .Where(recipe => recipe.Ingredients.All(ingredient =>
                    availableItems.Any(item =>
                        item.Name.Equals(
                            ingredient,
                            StringComparison.OrdinalIgnoreCase))))
                .FirstOrDefault();

            if (dinnerSuggestion == null)
            {
                AnsiConsole.MarkupLine(
                    "[grey]Inga recept kan lagas med tillgängliga ingredienser.[/]"
                );
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"[green]{Markup.Escape(dinnerSuggestion.Name)}[/] - {dinnerSuggestion.CookingTimeMinutes} min"
                );
            }
        }

        AnsiConsole.WriteLine();
    }
}
