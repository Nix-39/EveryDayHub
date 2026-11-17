namespace EveryDayHub.Models
{
    public class LogEntry
    {
        // Date and time when the event occurred.
        public DateTime Timestamp { get; set; }

        // Event type, such as Info or Error.
        public string Type { get; set; } = "";

        // Description of the event.
        public string Message { get; set; } = "";
    }
}