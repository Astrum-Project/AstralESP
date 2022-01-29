using MelonLoader;

namespace Astrum
{
    public static class Config
    {
        public static MelonPreferences_Entry<bool> pickupsEnabled;
        public static MelonPreferences_Entry<bool> pickupsOwner;
        public static MelonPreferences_Entry<bool> pickupsDistance;
        public static MelonPreferences_Entry<float> pickupsMaxDistance;
        public static MelonPreferences_Entry<bool> outlines;

        public static void Initialize()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("Astrum-AstralESP", "Astral ESP");

            pickupsEnabled = category.CreateEntry("Pickups-Enabled", true, "Pickups - Enabled");
            pickupsOwner = category.CreateEntry("Pickups-Owner", true, "Pickups - Show Owner");
            pickupsDistance = category.CreateEntry("Pickups-Distance", true, "Pickups - Show Distance");
            pickupsMaxDistance = category.CreateEntry("Pickups-MaxDistance", 10000f, "Pickups - Max Distance");

            (outlines = category.CreateEntry("outlines", true, "Outlines")).OnValueChanged += (prev, curr) => AstralESP.Outlines.Enabled = curr;
            AstralESP.Outlines.Enabled = outlines.Value;
        }
    }
}
