namespace DF.BargeIndustries
{
    using Eco.Core.Plugins.Interfaces;

    public class DeepflameModInitializer : IModInit
    {
        public static ModRegistration Register()
        {
            return new ModRegistration()
            {
                ModName = "DF_BargeIndustries",
                ModDescription =
                    "A mod that extends the functionality of the Industrial Barge.",
                ModDisplayName = "[DF] Barge Industries",
            };
        }
    }
}
