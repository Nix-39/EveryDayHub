using System.Globalization;
using Spectre.Console;
using EveryDayHub.Models;
using EveryDayHub.Services;

namespace EveryDayHub.UI;

// Schemamenyn (kort 8 och 13): arbetspass och aktiviteter för hela familjen
public static class ScheduleMenu
{
    // Visar schemamenyn tills användaren väljer Tillbaka.
    // Skåpen skapas i MainMenu och skickas hit, så att alla menyer delar samma data.
    public static void Run(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        bool running = true;

        while (running)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Schema - vad vill du göra?")
                    .AddChoices(
                        "Visa veckan",
                        "Filtrera schemat",
                        "Vem är ledig?",
                        "Lägg till arbetspass",
                        "Lägg till aktivitet",
                        "Ändra",
                        "Ta bort",
                        "Tillbaka"
                    ));

            switch (choice)
            {
                case "Visa veckan":
                    ShowWeek(boardStore, memberStore);
                    break;
                case "Filtrera schemat":
                    Filter(boardStore, memberStore);
                    break;
                case "Vem är ledig?":
                    ShowFree(boardStore, memberStore);
                    break;
                case "Lägg till arbetspass":
                    AddWorkShift(boardStore, memberStore);
                    break;
                case "Lägg till aktivitet":
                    AddActivity(boardStore, memberStore);
                    break;
                case "Ändra":
                    Edit(boardStore, memberStore);
                    break;
                case "Ta bort":
                    Remove(boardStore);
                    break;
                case "Tillbaka":
                    running = false;
                    break;
            }
        }
    }

    // Visar en vecka som tabell med en rad per person och en kolumn per dag
    private static void ShowWeek(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
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
        List<BoardItem> items = boardStore.GetAll();
        CultureInfo swedish = new CultureInfo("sv-SE");

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Person");

        for (int i = 0; i < 7; i++)
        {
            DateTime day = monday.AddDays(i);
            table.AddColumn(day.ToString("ddd dd/MM", swedish));
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

        AnsiConsole.MarkupLine($"[bold]Vecka från {monday:yyyy-MM-dd}[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[yellow]Arbetspass[/]   [aqua]Aktivitet[/]");
        Pause();
    }

    // Räknar ut måndagen i den vecka som datumet tillhör
    private static DateTime StartOfWeek(DateTime date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    // Bygger texten i en ruta: alla arbetspass och aktiviteter för en person en viss dag
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

    // Filtrerar schemat på person och/eller dag och visar resultatet som tabell
    private static void Filter(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            Pause();
            return;
        }

        FamilyMember? person = AskPersonFilter(members);
        int? personId = person?.Id;

        DateTime? dayFilter = null;

        if (AskYesNo("Vill du filtrera på en viss dag?"))
        {
            dayFilter = AskDate("Dag (åååå-mm-dd):");
        }

        // Varje Where är en sil: bara det som matchar släpps igenom
        List<BoardItem> result = boardStore.GetAll()
            .Where(item => item is WorkShift || item is Activity)
            .Where(item => personId == null || InvolvesPerson(item, personId.Value))
            .Where(item => dayFilter == null || StartOf(item).Date == dayFilter.Value.Date)
            .OrderBy(item => StartOf(item))
            .ToList();

        string personText = person == null ? "alla" : person.Name;
        string dayText = dayFilter == null ? "alla dagar" : dayFilter.Value.ToString("yyyy-MM-dd");
        AnsiConsole.MarkupLine($"[bold]Filter:[/] {Markup.Escape(personText)}, {dayText}");

        if (result.Count == 0)
        {
            Messages.Info("Inget matchar filtret.");
            Pause();
            return;
        }

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Datum");
        table.AddColumn("Tid");
        table.AddColumn("Typ");
        table.AddColumn("Vad");
        table.AddColumn("Personer");

        foreach (BoardItem item in result)
        {
            string type = item is WorkShift ? "[yellow]Arbetspass[/]" : "[aqua]Aktivitet[/]";

            table.AddRow(
                StartOf(item).ToString("yyyy-MM-dd"),
                $"{StartOf(item):HH:mm}-{EndOf(item):HH:mm}",
                type,
                Markup.Escape(WhatText(item)),
                Markup.Escape(PeopleText(item, members)));
        }

        AnsiConsole.Write(table);
        Pause();
    }

    // Visar vem som är ledig och vem som är upptagen en viss dag
    private static void ShowFree(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            Pause();
            return;
        }

        DateTime day = AskDate("Dag (åååå-mm-dd):");

        // Silar en gång: bara arbetspass och aktiviteter som sker den valda dagen
        List<BoardItem> dayItems = boardStore.GetAll()
            .Where(item => item is WorkShift || item is Activity)
            .Where(item => StartOf(item).Date == day.Date)
            .OrderBy(item => StartOf(item))
            .ToList();

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Person");
        table.AddColumn("Status");
        table.AddColumn("Bokat");

        List<string> freeNames = new List<string>();

        foreach (FamilyMember member in members)
        {
            // En person är ledig om inget den dagen gäller personen
            List<BoardItem> memberItems = dayItems
                .Where(item => InvolvesPerson(item, member.Id))
                .ToList();

            if (memberItems.Count == 0)
            {
                freeNames.Add(member.Name);
                table.AddRow(Markup.Escape(member.Name), "[green]Ledig[/]", "");
            }
            else
            {
                string booked = string.Join("\n", memberItems.Select(item =>
                    Markup.Escape($"{StartOf(item):HH:mm}-{EndOf(item):HH:mm} {WhatText(item)}")));

                table.AddRow(Markup.Escape(member.Name), "[red]Upptagen[/]", booked);
            }
        }

        AnsiConsole.MarkupLine($"[bold]Vem är ledig {day:yyyy-MM-dd}?[/]");
        AnsiConsole.Write(table);

        if (freeNames.Count == 0)
        {
            Messages.Info("Ingen är ledig den dagen.");
        }
        else
        {
            Messages.Info($"Lediga: {string.Join(", ", freeNames)}");
        }

        Pause();
    }

    // Låter användaren välja en person eller "Alla". Ger null tillbaka för "Alla".
    private static FamilyMember? AskPersonFilter(List<FamilyMember> members)
    {
        List<int> options = Enumerable.Range(0, members.Count + 1).ToList();

        int selected = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("Vilken person?")
                .MoreChoicesText("[grey](Flytta upp och ner för fler val)[/]")
                .UseConverter(option => option == 0 ? "Alla" : Markup.Escape(members[option - 1].Name))
                .AddChoices(options));

        return selected == 0 ? null : members[selected - 1];
    }

    // Sant om personen jobbar passet eller deltar i aktiviteten
    private static bool InvolvesPerson(BoardItem item, int personId)
    {
        if (item is WorkShift shift)
        {
            return shift.FamilyMemberId == personId;
        }

        if (item is Activity activity)
        {
            return activity.ParticipantIds.Contains(personId);
        }

        return false;
    }

    // Frågar efter uppgifterna och lägger till ett nytt arbetspass i skåpet
    private static void AddWorkShift(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            Pause();
            return;
        }

        FamilyMember person = AskPerson("Vem ska jobba?", members);

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Rubrik:")
                .DefaultValue("Arbetspass"));

        DateTime date = AskDate("Datum (åååå-mm-dd):");
        TimeSpan start = AskTime("Starttid (tt:mm):");
        TimeSpan end = AskTime("Sluttid (tt:mm):");

        while (end <= start)
        {
            Messages.Error("Sluttiden måste vara efter starttiden.");
            end = AskTime("Sluttid (tt:mm):");
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
        Messages.Success($"Arbetspasset sparades: {shift.Describe()}");
        Pause();
    }

    // Frågar efter uppgifterna och lägger till en ny aktivitet i skåpet
    private static void AddActivity(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count == 0)
        {
            Messages.Error("Det finns inga familjemedlemmar än. Lägg till en familjemedlem först.");
            Pause();
            return;
        }

        string title = AnsiConsole.Prompt(new TextPrompt<string>("Rubrik:"));
        string location = AnsiConsole.Prompt(new TextPrompt<string>("Plats:"));

        DateTime date = AskDate("Datum (åååå-mm-dd):");
        TimeSpan start = AskTime("Starttid (tt:mm):");
        TimeSpan end = AskTime("Sluttid (tt:mm):");

        while (end <= start)
        {
            Messages.Error("Sluttiden måste vara efter starttiden.");
            end = AskTime("Sluttid (tt:mm):");
        }

        List<FamilyMember> participants = AskParticipants(members);

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
        Messages.Success($"Aktiviteten sparades: {activity.Describe()}");
        Pause();
    }

    // Låter användaren välja ett pass eller en aktivitet och ändra det
    private static void Edit(DataStore<BoardItem> boardStore, DataStore<FamilyMember> memberStore)
    {
        BoardItem? chosen = ChooseItem(boardStore, "Vad vill du ändra?");

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

    // Låter användaren välja ett pass eller en aktivitet och ta bort det efter en kontrollfråga
    private static void Remove(DataStore<BoardItem> boardStore)
    {
        BoardItem? chosen = ChooseItem(boardStore, "Vad vill du ta bort?");

        if (chosen == null)
        {
            return;
        }

        if (!AskYesNo($"Ta bort {Markup.Escape(chosen.Describe())}?"))
        {
            Messages.Info("Inget togs bort.");
            Pause();
            return;
        }

        if (boardStore.RemoveItem(chosen))
        {
            Messages.Success("Borttaget.");
        }
        else
        {
            Messages.Error("Det gick inte att ta bort.");
        }

        Pause();
    }

    // Låter användaren välja ett arbetspass eller en aktivitet i en lista.
    // Ger null tillbaka om listan är tom eller om användaren väljer Avbryt.
    private static BoardItem? ChooseItem(DataStore<BoardItem> boardStore, string title)
    {
        // Bara arbetspass och aktiviteter visas (lappar hör till lappmenyn)
        List<BoardItem> choices = boardStore.GetAll()
            .Where(item => item is WorkShift || item is Activity)
            .OrderBy(item => StartOf(item))
            .ToList();

        if (choices.Count == 0)
        {
            Messages.Info("Det finns inga arbetspass eller aktiviteter än.");
            Pause();
            return null;
        }

        List<string> labels = choices
            .Select(item => $"{item.Id}: {StartOf(item):yyyy-MM-dd} {item.Describe()}")
            .ToList();
        labels.Add("Avbryt");

        string selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](Flytta upp och ner för fler val)[/]")
                .UseConverter(label => Markup.Escape(label))
                .AddChoices(labels));

        if (selected == "Avbryt")
        {
            return null;
        }

        return choices[labels.IndexOf(selected)];
    }

    // Ändrar ett arbetspass. Enter behåller det nuvarande värdet.
    private static void EditWorkShift(WorkShift shift, DataStore<FamilyMember> memberStore)
    {
        AnsiConsole.MarkupLine("[grey]Tryck Enter för att behålla nuvarande värde.[/]");

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Rubrik:").DefaultValue(shift.Title));

        DateTime date = AskDate("Datum (åååå-mm-dd):", shift.Start);
        TimeSpan start = AskTime("Starttid (tt:mm):", shift.Start.TimeOfDay);
        TimeSpan end = AskTime("Sluttid (tt:mm):", shift.End.TimeOfDay);

        while (end <= start)
        {
            Messages.Error("Sluttiden måste vara efter starttiden.");
            end = AskTime("Sluttid (tt:mm):", shift.End.TimeOfDay);
        }

        int personId = shift.FamilyMemberId;
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count > 0 && AskYesNo("Vill du byta vem som jobbar?"))
        {
            personId = AskPerson("Vem ska jobba?", members).Id;
        }

        // Svaren skrivs in i objektet först här, så att det aldrig blir halvt ändrat
        shift.Title = title;
        shift.Date = date.Date;
        shift.Start = date.Date + start;
        shift.End = date.Date + end;
        shift.FamilyMemberId = personId;

        Messages.Success($"Arbetspasset uppdaterades: {shift.Describe()}");
        Pause();
    }

    // Ändrar en aktivitet. Enter behåller det nuvarande värdet.
    private static void EditActivity(Activity activity, DataStore<FamilyMember> memberStore)
    {
        AnsiConsole.MarkupLine("[grey]Tryck Enter för att behålla nuvarande värde.[/]");

        string title = AnsiConsole.Prompt(
            new TextPrompt<string>("Rubrik:").DefaultValue(activity.Title));

        string location = AnsiConsole.Prompt(
            new TextPrompt<string>("Plats:").DefaultValue(activity.Location));

        DateTime date = AskDate("Datum (åååå-mm-dd):", activity.Start);
        TimeSpan start = AskTime("Starttid (tt:mm):", activity.Start.TimeOfDay);
        TimeSpan end = AskTime("Sluttid (tt:mm):", activity.End.TimeOfDay);

        while (end <= start)
        {
            Messages.Error("Sluttiden måste vara efter starttiden.");
            end = AskTime("Sluttid (tt:mm):", activity.End.TimeOfDay);
        }

        List<int> participantIds = activity.ParticipantIds;
        List<FamilyMember> members = memberStore.GetAll();

        if (members.Count > 0 && AskYesNo("Vill du byta vilka som deltar?"))
        {
            participantIds = AskParticipants(members).Select(person => person.Id).ToList();
        }

        // Svaren skrivs in i objektet först här, så att det aldrig blir halvt ändrat
        activity.Title = title;
        activity.Location = location;
        activity.Date = date.Date;
        activity.Start = date.Date + start;
        activity.End = date.Date + end;
        activity.ParticipantIds = participantIds;

        Messages.Success($"Aktiviteten uppdaterades: {activity.Describe()}");
        Pause();
    }

    // Starttiden för ett arbetspass eller en aktivitet
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

    // Sluttiden för ett arbetspass eller en aktivitet
    private static DateTime EndOf(BoardItem item)
    {
        if (item is WorkShift shift)
        {
            return shift.End;
        }

        if (item is Activity activity)
        {
            return activity.End;
        }

        return item.Date;
    }

    // Kort text om vad det är, till exempel "Fotboll på Heden"
    private static string WhatText(BoardItem item)
    {
        if (item is Activity activity)
        {
            return $"{activity.Title} på {activity.Location}";
        }

        return item.Title;
    }

    // Namnen på personerna som hör till passet eller aktiviteten, till exempel "Arian, Alper"
    private static string PeopleText(BoardItem item, List<FamilyMember> members)
    {
        List<int> ids = new List<int>();

        if (item is WorkShift shift)
        {
            ids.Add(shift.FamilyMemberId);
        }
        else if (item is Activity activity)
        {
            ids = activity.ParticipantIds;
        }

        List<string> names = members
            .Where(member => ids.Contains(member.Id))
            .Select(member => member.Name)
            .ToList();

        return string.Join(", ", names);
    }

    // Låter användaren välja en person i en lista
    private static FamilyMember AskPerson(string title, List<FamilyMember> members)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<FamilyMember>()
                .Title(title)
                .MoreChoicesText("[grey](Flytta upp och ner för fler val)[/]")
                .UseConverter(member => Markup.Escape(member.Name))
                .AddChoices(members));
    }

    // Låter användaren kryssa i en eller flera deltagare
    private static List<FamilyMember> AskParticipants(List<FamilyMember> members)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<FamilyMember>()
                .Title("Vilka deltar?")
                .InstructionsText("[grey](Tryck [blue]<mellanslag>[/] för att välja, [green]<enter>[/] för att bekräfta)[/]")
                .MoreChoicesText("[grey](Flytta upp och ner för fler val)[/]")
                .UseConverter(member => Markup.Escape(member.Name))
                .AddChoices(members));
    }

    // Ja/nej-fråga som meny. Nej står först, så att Enter är det säkra valet.
    private static bool AskYesNo(string question)
    {
        string answer = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(question)
                .AddChoices("Nej", "Ja"));

        return answer == "Ja";
    }

    // Frågar efter ett datum och godtar bara formatet åååå-mm-dd
    private static DateTime AskDate(string question, DateTime? defaultDate = null)
    {
        DateTime defaultValue = defaultDate ?? DateTime.Today;

        string text = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValue(defaultValue.ToString("yyyy-MM-dd"))
                .Validate(input =>
                    DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Använd formatet åååå-mm-dd, till exempel 2026-11-05[/]")));

        return DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    // Frågar efter ett klockslag och godtar bara formatet tt:mm
    private static TimeSpan AskTime(string question, TimeSpan? defaultTime = null)
    {
        TextPrompt<string> prompt = new TextPrompt<string>(question)
            .Validate(input =>
                DateTime.TryParseExact(input, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Använd formatet tt:mm, till exempel 16:00[/]"));

        if (defaultTime.HasValue)
        {
            prompt = prompt.DefaultValue(defaultTime.Value.ToString(@"hh\:mm"));
        }

        string text = AnsiConsole.Prompt(prompt);

        return DateTime.ParseExact(text, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;
    }

    // Nästa lediga Id: det högsta Id:t i skåpet plus ett
    private static int NextId(DataStore<BoardItem> boardStore)
    {
        List<BoardItem> all = boardStore.GetAll();
        return all.Count == 0 ? 1 : all.Max(item => item.Id) + 1;
    }

    // Väntar på en tangent så att användaren hinner läsa meddelandet
    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att fortsätta...[/]");
        Console.ReadKey(true);
    }
}