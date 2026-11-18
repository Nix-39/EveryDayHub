using System.Globalization;
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

// Statistikmenyn (kort 18): sammanfattningar av det som finns i skåpen
public static class StatisticsMenu
{
    // Samma filer som kylskåpsmenyn sparar i, så att statistiken visar samma data
    private const string FoodFile = "Data/fooditems.json";
    private const string RecipeFile = "Data/recipes.json";

    // Visar statistikmenyn tills användaren väljer Tillbaka
    public static void Run(DataStore<BoardItem> boardStore, DataStore<FamilyMember> familyStore)
    {
        bool running = true;

        while (running)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Statistik - vad vill du se?")
                    .AddChoices(
                        "Timmar per person och vecka",
                        "Rätter som går att laga i dag",
                        "Varor som snart går ut",
                        "Tillbaka"
                    ));

            switch (choice)
            {
                case "Timmar per person och vecka":
                    ShowHours(boardStore, familyStore);
                    Pause.Wait();
                    break;
                case "Rätter som går att laga i dag":
                    ShowCookableRecipes();
                    Pause.Wait();
                    break;
                case "Varor som snart går ut":
                    ShowExpiringItems();
                    Pause.Wait();
                    break;
                case "Tillbaka":
                    running = false;
                    break;
            }
        }
    }

    // Visar inbokade timmar per person en vald vecka, som tabell och stapeldiagram
    private static void ShowHours(DataStore<BoardItem> boardStore, DataStore<FamilyMember> familyStore)
    {
        List<FamilyMember> members = familyStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            return;
        }

        DateTime anyDay = AskDate("Visa veckan för (åååå-mm-dd):");
        DateTime monday = StartOfWeek(anyDay);
        DateTime nextMonday = monday.AddDays(7);
        int weekNumber = ISOWeek.GetWeekOfYear(monday);

        List<BoardItem> items = boardStore.GetAll();

        // Silar fram bara veckans arbetspass och aktiviteter
        List<WorkShift> shifts = items.OfType<WorkShift>()
            .Where(s => s.Start >= monday && s.Start < nextMonday)
            .ToList();

        List<Activity> activities = items.OfType<Activity>()
            .Where(a => a.Start >= monday && a.Start < nextMonday)
            .ToList();

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Person");
        table.AddColumn(new TableColumn("Arbetspass (h)").RightAligned());
        table.AddColumn(new TableColumn("Aktiviteter (h)").RightAligned());
        table.AddColumn(new TableColumn("Totalt (h)").RightAligned());

        BarChart chart = new BarChart()
            .Width(60)
            .Label($"[bold]Inbokade timmar vecka {weekNumber}[/]")
            .CenterLabel();

        Color[] colors = { Color.Yellow, Color.Aqua, Color.Green, Color.Fuchsia, Color.Orange1, Color.Blue };
        double allHours = 0;

        for (int i = 0; i < members.Count; i++)
        {
            FamilyMember member = members[i];

            // Summerar personens arbetspass med Hours från WorkShift
            double workHours = shifts
                .Where(s => s.FamilyMemberId == member.Id)
                .Sum(s => s.Hours);

            // Summerar tiden för aktiviteter som personen deltar i
            double activityHours = activities
                .Where(a => a.ParticipantIds.Contains(member.Id))
                .Sum(a => (a.End - a.Start).TotalHours);

            double total = workHours + activityHours;
            allHours += total;

            table.AddRow(
                Markup.Escape(member.Name),
                workHours.ToString("0.#"),
                activityHours.ToString("0.#"),
                $"[bold]{total:0.#}[/]");

            chart.AddItem(Markup.Escape(member.Name), Math.Round(total, 1), colors[i % colors.Length]);
        }

        AnsiConsole.MarkupLine($"[bold]Vecka {weekNumber} ({monday:yyyy-MM-dd} – {nextMonday.AddDays(-1):yyyy-MM-dd})[/]");
        AnsiConsole.Write(table);

        if (allHours == 0)
        {
            Messages.Info("Inga arbetspass eller aktiviteter den veckan.");
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(chart);
    }

    // Visar hur många rätter som går att laga med det som finns hemma, och vilka
    private static void ShowCookableRecipes()
    {
        List<FoodItem>? foodItems = LoadList<FoodItem>(FoodFile, "matvarorna");
        List<Recipe>? recipes = LoadList<Recipe>(RecipeFile, "recepten");

        if (foodItems == null || recipes == null)
        {
            return;
        }

        if (recipes.Count == 0)
        {
            Messages.Info("Det finns inga recept än.");
            return;
        }

        // Bara varor som finns hemma räknas, samma regel som i kylskåpsmenyn
        List<FoodItem> available = foodItems
            .Where(item => item.IsAvailable)
            .ToList();

        // Ett recept går att laga om alla ingredienser finns bland de tillgängliga varorna
        List<Recipe> cookable = recipes
            .Where(recipe => recipe.Ingredients.All(ingredient =>
                available.Any(item => item.Name.Equals(ingredient, StringComparison.OrdinalIgnoreCase))))
            .OrderBy(recipe => recipe.CookingTimeMinutes)
            .ToList();

        int notCookable = recipes.Count - cookable.Count;

        BarChart chart = new BarChart()
            .Width(60)
            .Label($"[bold]Rätter i dag ({recipes.Count} recept totalt)[/]")
            .CenterLabel()
            .AddItem("Kan lagas", cookable.Count, Color.Green)
            .AddItem("Saknar ingredienser", notCookable, Color.Red);

        AnsiConsole.Write(chart);
        AnsiConsole.WriteLine();

        if (cookable.Count == 0)
        {
            Messages.Info("Inga rätter går att laga med det som finns hemma just nu.");
            return;
        }

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Rätt");
        table.AddColumn(new TableColumn("Tillagningstid").RightAligned());

        foreach (Recipe recipe in cookable)
        {
            table.AddRow(Markup.Escape(recipe.Name), $"{recipe.CookingTimeMinutes} min");
        }

        AnsiConsole.Write(table);
    }

    // Visar varor vars bäst före-datum har passerat eller snart passerar
    private static void ShowExpiringItems()
    {
        List<FoodItem>? foodItems = LoadList<FoodItem>(FoodFile, "matvarorna");

        if (foodItems == null)
        {
            return;
        }

        // Bara kylvaror har bäst före-datum (skafferivaror har inget)
        List<FridgeItem> fridgeItems = foodItems.OfType<FridgeItem>().ToList();

        if (fridgeItems.Count == 0)
        {
            Messages.Info("Det finns inga varor med bäst före-datum än.");
            return;
        }

        DateTime today = DateTime.Today;

        // Samma regel som bäst före-kollen: varan varnas inom sina WarningDays
        List<FridgeItem> expired = fridgeItems
            .Where(item => item.ExpirationDate.Date < today)
            .ToList();

        List<FridgeItem> soon = fridgeItems
            .Where(item => item.ExpirationDate.Date >= today
                && item.ExpirationDate.Date <= today.AddDays(item.WarningDays))
            .ToList();

        int okCount = fridgeItems.Count - expired.Count - soon.Count;

        BarChart chart = new BarChart()
            .Width(60)
            .Label($"[bold]Bäst före ({fridgeItems.Count} varor)[/]")
            .CenterLabel()
            .AddItem("Passerat", expired.Count, Color.Red)
            .AddItem("Går snart ut", soon.Count, Color.Yellow)
            .AddItem("OK", okCount, Color.Green);

        AnsiConsole.Write(chart);
        AnsiConsole.WriteLine();

        List<FridgeItem> warnings = expired
            .Concat(soon)
            .OrderBy(item => item.ExpirationDate)
            .ToList();

        if (warnings.Count == 0)
        {
            Messages.Success("Inga varor behöver kontrolleras just nu.");
            return;
        }

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Vara");
        table.AddColumn("Bäst före");
        table.AddColumn(new TableColumn("Dagar kvar").RightAligned());
        table.AddColumn("Status");

        foreach (FridgeItem item in warnings)
        {
            int daysLeft = (item.ExpirationDate.Date - today).Days;
            string status = daysLeft < 0 ? "[red]Passerat[/]" : "[yellow]Går snart ut[/]";

            table.AddRow(
                Markup.Escape(item.Name),
                item.ExpirationDate.ToString("yyyy-MM-dd"),
                daysLeft.ToString(),
                status);
        }

        AnsiConsole.Write(table);
    }

    // Läser en lista från en JSON-fil. Finns ingen fil blir listan tom.
    // Ger null tillbaka om filen finns men inte går att läsa.
    private static List<T>? LoadList<T>(string filePath, string what) where T : class
    {
        DataStore<T> store = new DataStore<T>(filePath);

        if (!File.Exists(filePath))
        {
            return new List<T>();
        }

        if (!store.LoadFromJson())
        {
            Messages.Error($"Kunde inte läsa {what}: {store.LastError}");
            return null;
        }

        return store.GetAll();
    }

    // Räknar ut måndagen i den vecka som datumet tillhör
    private static DateTime StartOfWeek(DateTime date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    // Frågar efter ett datum och godtar bara formatet åååå-mm-dd
    private static DateTime AskDate(string question)
    {
        string text = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValue(DateTime.Today.ToString("yyyy-MM-dd"))
                .Validate(input =>
                    DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Använd formatet åååå-mm-dd, till exempel 2026-11-05[/]")));

        return DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}