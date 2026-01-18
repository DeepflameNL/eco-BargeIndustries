namespace DF.BargeIndustries
{
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Skills;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using Eco.Simulation.WorldLayers;
    using System.Collections.Generic;

    [RequiresSkill(typeof(OilDrillingSkill), 6)]
    public partial class OffshoreDrillingRecipe : RecipeFamily
    {
        public OffshoreDrillingRecipe()
        {
            int requestedProduct = 40;

            var recipe = new Recipe();
            recipe.Init(
                name: "OffshoreDrilling",
                displayName: Localizer.DoStr("Offshore Drilling"),

                ingredients: new List<IngredientElement>
                {
                    new(typeof(SteelDrillBitItem), 1, staticIngredient: true),
                    new(typeof(SteelPipeItem), 10, staticIngredient: true),
                    new(typeof(BarrelItem), requestedProduct, true)
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<PetroleumItem>(requestedProduct),
                    new CraftingElement<SteelDrillBitItem>(typeof(OilDrillingSkill), 1),
                    new CraftingElement<SteelPipeItem>(typeof(OilDrillingSkill), 10)
                }
            );
            this.Recipes = new List<Recipe> { recipe };

            this.ExperienceOnCraft = 0.5f;
            this.LaborInCalories = CreateLaborInCaloriesValue(35 * requestedProduct, typeof(OilDrillingSkill));
            this.CraftMinutes = new MultiDynamicValue(MultiDynamicOps.Multiply,
                CreateCraftTimeValue(beneficiary: typeof(PetroleumRecipe), start: 17.5f * requestedProduct, skillType: typeof(OilDrillingSkill), typeof(OilDrillingFocusedSpeedTalent), typeof(OilDrillingParallelSpeedTalent)),
                new LayerModifiedValue(LayerNames.Oilfield, 3)
            );

            this.ModsPreInitialize();
            this.Initialize(displayText: recipe.DisplayName, recipeType: this.GetType());
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(IndustrialBargeObject), recipeFamily: this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
