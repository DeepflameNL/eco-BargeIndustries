using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Modules;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using System.Collections.Generic;

namespace DF.BargeIndustries
{
    public class BargeModeOffshoreDrilling : BargeMode
    {
        public override string Name => "Offshore Drilling";

        public override IReadOnlyList<PluginModule> SupportedModules => new List<PluginModule>
        {
            Item.Get<OilDrillingUpgradeItem>()
        };

        public override IReadOnlyList<RecipeFamily> SupportedWorkOrders => new List<RecipeFamily>
        {
            RecipeManager.GetRecipeFamily<OffshoreDrillingRecipe>()
        };

        public override float OperatingRadius => 15.0f;
        public override float OperatingThreshold => 30 * 0.4f;

        public override IReadOnlyCollection<BargeModeStatus> GetAdditionalStatuses()
        {
            Vector2i parentPosition = Barge.GetOccupancyRangeWorldPos().CenterExc.XZi();
            WorldLayer oilfieldLayer = WorldLayerManager.Obj.GetLayer(LayerNames.Oilfield);
            float currentState = oilfieldLayer.EntryWorldPos(parentPosition);

            return new List<BargeModeStatus>
            {
                new BargeModeStatus(true, Localizer.DoStr($"Oil field efficacy is {(int)(currentState * 100.0f)}%."))
            };
        }
    }
}
