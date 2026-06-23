namespace Tungsten.Models {
    public class RsvpSummary {
        public int Attending { get; set; }
        public int Declined { get; set; }
        public Dictionary<string, int> Meals = new Dictionary<string, int>();
    }
}
