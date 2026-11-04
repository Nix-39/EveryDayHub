using System.Globalization;
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

public static class ScheduleMenu
{
    public static void Run(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        bool running = true;

        while (running)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Schedule - what do you want to do?")
                    .AddChoices(
                        "Add work shift",
                        "Back"
                    ));

            switch (choice)
            {
                case "Add work shift":
                    AddWorkShift(boardStore, memberStore);
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private static void AddWorkShift(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("There are no family members yet. Add a family member first.");
            Pause();
            return;
        }

        FamilyMember person = AnsiConsole.Prompt(
            new SelectionPrompt<FamilyMember>()
                .Title("Who is working?")
                .UseConverter(member => Markup.Escape(member.Name))
                .AddChoices(members));

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:")
                .DefaultValue("Work shift"));

        DateTime date = AskDate("Date (yyyy-MM-dd):");
        TimeSpan start = AskTime("Start time (HH:mm):");
        TimeSpan end = AskTime("End time (HH:mm):");

        while (end <= start)
        {
            Messages.Error("The end time must be after the start time.");
            end = AskTime("End time (HH:mm):");
        }

        WorkShift shift = new WorkShift
        {
            Id = NextId(boardStore),
            Title = title,
            Date = date.Date,
            CreatedById = person.Id,
            FamilyMemberId = person.Id,
            Start = date.Date + start,
            End = date.Date + end
        };

        boardStore.AddItem(shift);
        Messages.Success($"Work shift saved: {shift.Describe()}");
        Pause();
    }

    // Asks for a date and does not accept anything else than yyyy-MM-dd
    private static DateTime AskDate(string question)
    {
        string text = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValue(DateTime.Today.ToString("yyyy-MM-dd"))
                .Validate(input =>
                    DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Use the format yyyy-MM-dd, for example 2026-11-05[/]")));

        return DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    // Asks for a time of day and does not accept anything else than HH:mm
    private static TimeSpan AskTime(string question)
    {
        string text = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .Validate(input =>
                    DateTime.TryParseExact(input, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Use the format HH:mm, for example 16:00[/]")));

        return DateTime.ParseExact(text, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;
    }

    // Next free Id: the highest Id in the store plus one
    private static int NextId(DataStore<BoardItem> boardStore)
    {
        List<BoardItem> all = boardStore.GetAll();
        return all.Count == 0 ? 1 : all.Max(item => item.Id) + 1;
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}