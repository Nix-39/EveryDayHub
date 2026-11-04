
namespace EveryDayHub.Models
{
    // Represents dairy products stored in the fridge
    public class Dairy : FridgeItem
    {
        // Warn three days before expiration
        public override int WarningDays => 3;
    }
}
