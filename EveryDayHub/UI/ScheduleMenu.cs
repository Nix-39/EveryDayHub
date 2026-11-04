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
                    .Title("Schema - vad vill du göra?")
                    .AddChoices(
                        "Tillbaka"
                    ));

            switch (choice)
            {
                case "Tillbaka":
                    running = false;
                    break;
            }
        }
    }
}