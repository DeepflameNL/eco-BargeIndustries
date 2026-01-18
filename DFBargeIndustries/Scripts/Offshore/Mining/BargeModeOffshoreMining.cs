using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Modules;
using Eco.Mods.TechTree;
using System.Collections.Generic;

namespace DF.BargeIndustries
{
    public class BargeModeOffshoreMining : BargeMode
    {
        public override string Name => "Offshore Mining";

        public override IReadOnlyList<PluginModule> SupportedModules => new List<PluginModule>
        {
            Item.Get<MiningModernUpgradeItem>()
        };

        public override IReadOnlyList<RecipeFamily> SupportedWorkOrders => new List<RecipeFamily>
        {
            RecipeManager.GetRecipeFamily<OffshoreMiningRecipe>()
        };

        public override float OperatingRadius => 15.0f;
        public override float OperatingThreshold => 30 * 0.4f;
    }
}
