using Tungsten.Models;

namespace Tungsten.Helpers {
    public static partial class ExtensionMethods {


        public static bool NameMatch(this Guest guest, Guest? other) {
            if (other == null) {
                return false;
            }

            string name = guest.FirstName + " " + guest.LastName;
            return name.Equals(other.FirstName + " " + other.LastName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
