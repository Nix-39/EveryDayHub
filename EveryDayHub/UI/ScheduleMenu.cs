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
                        "Show week",
                        "Add work shift",
                        "Add activity",
                        "Edit",
                        "Remove",
                        "Back"
                    ));

            switch (choice)
            {
                case "Show week":
                    ShowWeek(boardStore, memberStore);
                    break;
                case "Add work shift":
                    AddWorkShift(boardStore, memberStore);
                    break;
                case "Add activity":
                    AddActivity(boardStore, memberStore);
                    break;
                case "Edit":
                    Edit(boardStore, memberStore);
                    break;
                case "Remove":
                    Remove(boardStore);
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private static void ShowWeek(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("There are no family members yet. Add a family member first.");
            Pause();
            return;
        }

        DateTime anyDay = AskDate("Show the week of (yyyy-MM-dd):");
        DateTime monday = StartOfWeek(anyDay);
        List<BoardItem> items = boardStore.GetAll();

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Person");

        for (int i = 0; i < 7; i++)
        {
            DateTime day = monday.AddDays(i);
            table.AddColumn(day.ToString("ddd dd/MM", CultureInfo.InvariantCulture));
        }

        foreach (FamilyMember member in members)
        {
            string[] cells = new string[8];
            cells[0] = $"[bold]{Markup.Escape(member.Name)}[/]";

            for (int i = 0; i < 7; i++)
            {
                cells[i + 1] = BuildCell(items, member, monday.AddDays(i));
            }

            table.AddRow(cells);
        }

        AnsiConsole.MarkupLine($"[bold]Week of {monday:yyyy-MM-dd}[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[yellow]Work shift[/]   [aqua]Activity[/]");
        Pause();
    }

    // Monday of the week that the given date belongs to
    private static DateTime StartOfWeek(DateTime date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    // Builds the text for one cell: all work shifts and activities for one person on one day
    private static string BuildCell(List<BoardItem> items, FamilyMember person, DateTime day)
    {
        List<string> lines = new List<string>();

        foreach (WorkShift shift in items.OfType<WorkShift>()
            .Where(s => s.FamilyMemberId == person.Id && s.Start.Date == day.Date)
            .OrderBy(s => s.Start))
        {
            lines.Add($"[yellow]{shift.Start:HH:mm}-{shift.End:HH:mm}[/]");
        }

        foreach (Activity activity in items.OfType<Activity>()
            .Where(a => a.ParticipantIds.Contains(person.Id) && a.Start.Date == day.Date)
            .OrderBy(a => a.Start))
        {
            lines.Add($"[aqua]{Markup.Escape(activity.Title)} {activity.Start:HH:mm}[/]");
        }

        return string.Join("\n", lines);
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

    private static void AddActivity(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("There are no family members yet. Add a family member first.");
            Pause();
            return;
        }

        string title = AnsiConsole.Prompt(new TextPrompt<string>("Title:"));
        string location = AnsiConsole.Prompt(new TextPrompt<string>("Location:"));

        DateTime date = AskDate("Date (yyyy-MM-dd):");
        TimeSpan start = AskTime("Start time (HH:mm):");
        TimeSpan end = AskTime("End time (HH:mm):");

        while (end <= start)
        {
            Messages.Error("The end time must be after the start time.");
            end = AskTime("End time (HH:mm):");
        }

        List<FamilyMember> participants = AnsiConsole.Prompt(
            new MultiSelectionPrompt<FamilyMember>()
                .Title("Who is taking part?")
                .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to accept)[/]")
                .UseConverter(member => Markup.Escape(member.Name))
                .AddChoices(members));

        Activity activity = new Activity
        {
            Id = NextId(boardStore),
            Title = title,
            Date = date.Date,
            CreatedById = participants[0].Id,
            Location = location,
            Start = date.Date + start,
            End = date.Date + end,
            ParticipantIds = participants.Select(person => person.Id).ToList()
        };

        boardStore.AddItem(activity);
        Messages.Success($"Activity saved: {activity.Describe()}");
        Pause();
    }

    private static void Edit(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        BoardItem? chosen = ChooseItem(boardStore, "What do you want to edit?");

        if (chosen == null)
        {
            return;
        }

        if (chosen is WorkShift shift)
        {
            EditWorkShift(shift, memberStore);
        }
        else if (chosen is Activity activity)
        {
            EditActivity(activity, memberStore);
        }
    }

    private static void Remove(DataStore<BoardItem> boardStore)
    {
        BoardItem? chosen = ChooseItem(boardStore, "What do you want to remove?");

        if (chosen == null)
        {
            return;
        }

        if (!AnsiConsole.Confirm($"Remove {Markup.Escape(chosen.Describe())}?", false))
        {
            Messages.Info("Nothing was removed.");
            Pause();
            return;
        }

        if (boardStore.RemoveItem(chosen))
        {
            Messages.Success("Removed.");
        }
        else
        {
            Messages.Error("Could not remove the item.");
        }

        Pause();
    }

    // Lets the user pick a work shift or an activity from a list.
    // Returns null if there is nothing to pick or if the user cancels.
    private static BoardItem? ChooseItem(DataStore<BoardItem> boardStore, string title)
    {
        // Only work shifts and activities are listed (notes belong to the notes menu)
        List<BoardItem> choices = boardStore.GetAll()
            .Where(item => item is WorkShift || item is Activity)
            .OrderBy(item => StartOf(item))
            .ToList();

        if (choices.Count == 0)
        {
            Messages.Info("There are no work shifts or activities yet.");
            Pause();
            return null;
        }

        List<string> labels = choices
            .Select(item => $"{item.Id}: {StartOf(item):yyyy-MM-dd} {item.Describe()}")
            .ToList();
        labels.Add("Cancel");

        string selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .UseConverter(label => Markup.Escape(label))
                .AddChoices(labels));

        if (selected == "Cancel")
        {
            return null;
        }

        return choices[labels.IndexOf(selected)];
    }

    private static void EditWorkShift(WorkShift shift, DataStore<FamilyMember> memberStore)
    {
        AnsiConsole.MarkupLine("[grey]Press Enter to keep the current value.[/]");

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:").DefaultValue(shift.Title));

        DateTime date = AskDate("Date (yyyy-MM-dd):", shift.Start);
        TimeSpan start = AskTime("Start time (HH:mm):", shift.Start.TimeOfDay);
        TimeSpan end = AskTime("End time (HH:mm):", shift.End.TimeOfDay);

        while (end <= start)
        {
            Messages.Error("The end time must be after the start time.");
            end = AskTime("End time (HH:mm):", shift.End.TimeOfDay);
        }

        int personId = shift.FamilyMemberId;
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count > 0 && AnsiConsole.Confirm("Change who is working?", false))
        {
            FamilyMember person = AnsiConsole.Prompt(
                new SelectionPrompt<FamilyMember>()
                    .Title("Who is working?")
                    .UseConverter(member => Markup.Escape(member.Name))
                    .AddChoices(members));

            personId = person.Id;
        }

        shift.Title = title;
        shift.Date = date.Date;
        shift.Start = date.Date + start;
        shift.End = date.Date + end;
        shift.FamilyMemberId = personId;

        Messages.Success($"Work shift updated: {shift.Describe()}");
        Pause();
    }

    private static void EditActivity(Activity activity, DataStore<FamilyMember> memberStore)
    {
        AnsiConsole.MarkupLine("[grey]Press Enter to keep the current value.[/]");

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Title:").DefaultValue(activity.Title));

        string location = AnsiConsole.Prompt(
            new TextPrompt<string>("Location:").DefaultValue(activity.Location));

        DateTime date = AskDate("Date (yyyy-MM-dd):", activity.Start);
        TimeSpan start = AskTime("Start time (HH:mm):", activity.Start.TimeOfDay);
        TimeSpan end = AskTime("End time (HH:mm):", activity.End.TimeOfDay);

        while (end <= start)
        {
            Messages.Error("The end time must be after the start time.");
            end = AskTime("End time (HH:mm):", activity.End.TimeOfDay);
        }

        List<int> participantIds = activity.ParticipantIds;
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count > 0 && AnsiConsole.Confirm("Change who is taking part?", false))
        {
            List<FamilyMember> participants = AnsiConsole.Prompt(
                new MultiSelectionPrompt<FamilyMember>()
                    .Title("Who is taking part?")
                    .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to accept)[/]")
                    .UseConverter(member => Markup.Escape(member.Name))
                    .AddChoices(members));

            participantIds = participants.Select(person => person.Id).ToList();
        }

        activity.Title = title;
        activity.Location = location;
        activity.Date = date.Date;
        activity.Start = date.Date + start;
        activity.End = date.Date + end;
        activity.ParticipantIds = participantIds;

        Messages.Success($"Activity updated: {activity.Describe()}");
        Pause();
    }

    // Start time of a work shift or an activity
    private static DateTime StartOf(BoardItem item)
    {
        if (item is WorkShift shift)
        {
            return shift.Start;
        }

        if (item is Activity activity)
        {
            return activity.Start;
        }

        return item.Date;
    }

    // Asks for a date and does not accept anything else than yyyy-MM-dd
    private static DateTime AskDate(string question, DateTime? defaultDate = null)
    {
        DateTime defaultValue = defaultDate ?? DateTime.Today;

        string text = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValue(defaultValue.ToString("yyyy-MM-dd"))
                .Validate(input =>
                    DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Use the format yyyy-MM-dd, for example 2026-11-05[/]")));

        return DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    // Asks for a time of day and does not accept anything else than HH:mm
    private static TimeSpan AskTime(string question, TimeSpan? defaultTime = null)
    {
        TextPrompt<string> prompt = new TextPrompt<string>(question)
            .Validate(input =>
                DateTime.TryParseExact(input, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Use the format HH:mm, for example 16:00[/]"));

        if (defaultTime.HasValue)
        {
            prompt = prompt.DefaultValue(defaultTime.Value.ToString(@"hh\:mm"));
        }

        string text = AnsiConsole.Prompt(prompt);

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