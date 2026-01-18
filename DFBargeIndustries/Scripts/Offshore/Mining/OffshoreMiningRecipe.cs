namespace DF.BargeIndustries
{
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Skills;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using System.Collections.Generic;

    [RequiresSkill(typeof(MiningSkill), 6)]
    public partial class OffshoreMiningRecipe : RecipeFamily
    {
        public OffshoreMiningRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "OffshoreMining",
                displayName: Localizer.DoStr("Offshore Mining"),

                ingredients: new List<IngredientElement>
                {
                    new(typeof(SteelDrillBitItem), 1, staticIngredient: true),
                    new(typeof(SteelPipeItem), 10, staticIngredient: true)
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<CrushedGoldOreItem>(40),
                    new CraftingElement<SteelDrillBitItem>(typeof(MiningSkill), 1),
                    new CraftingElement<SteelPipeItem>(typeof(MiningSkill), 10)
                }
            );
            this.Recipes = new List<Recipe> { recipe };

            this.ExperienceOnCraft = 0;
            this.LaborInCalories = CreateLaborInCaloriesValue(12500, typeof(MiningSkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: this.GetType(), start: 45, skillType: typeof(MiningSkill));

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
