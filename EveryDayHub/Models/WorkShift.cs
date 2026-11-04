namespace EveryDayHub.Models;

public class WorkShift : BoardItem
{
    public int FamilyMemberId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // Calculate from start to end, in hours (can be fractional).
    public double Hours => (End - Start).TotalHours;

    public override string Describe() =>
        $"{Title}: {Start:HH:mm}-{End:HH:mm} ({Hours:0.#} h)";
}