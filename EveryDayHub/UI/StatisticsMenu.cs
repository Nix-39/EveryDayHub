using System.Globalization;
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

// Statistikmenyn (kort 18): sammanfattningar av det som finns i skåpen
public static class StatisticsMenu
{
    // Visar statistikmenyn tills användaren väljer Tillbaka
    public static void Run(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        bool running = true;

        while (running)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Statistik - vad vill du se?")
                    .AddChoices(
                        "Timmar per person och vecka",
                        "Tillbaka"
                    ));

            switch (choice)
            {
                case "Timmar per person och vecka":
                    ShowHours(boardStore, memberStore);
                    break;
                case "Tillbaka":
                    running = false;
                    break;
            }
        }
    }

    // Visar inbokade timmar per person en vald vecka, som tabell och stapeldiagram
    private static void ShowHours(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            Pause();
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
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
        }

        Pause();
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

    // Väntar på en tangent så att användaren hinner läsa
    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
        Console.ReadKey(true);
    }
}