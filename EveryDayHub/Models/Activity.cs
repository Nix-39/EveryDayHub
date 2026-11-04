namespace EveryDayHub.Models;

public class Activity : BoardItem
{
    public string Location { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<int> ParticipantIds { get; set; } = new();

    public override string Describe() =>
        $"{Title} at {Location}, {Start:HH:mm}-{End:HH:mm}";
}