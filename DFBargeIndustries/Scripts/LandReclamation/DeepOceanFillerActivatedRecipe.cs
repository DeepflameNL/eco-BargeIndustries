namespace DF.BargeIndustries
{
    using Eco.Core.Items;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Skills;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// <para>Server side item definition for the "ActivatedDeepOceanFiller" item.</para>
    /// </summary>
    [Serialized]
    [LocDisplayName("Activated Deep Ocean Filler")]
    [Weight(22000)]
    [MaxStackSize(10)]
    [Ecopedia("Items", "Products", createAsSubPage: true)]
    [LocDescription("Created in the ocean and immediately used in the deep ocean filling process.")]
    public partial class ActivatedDeepOceanFillerItem : Item { }

    [RequiresSkill(typeof(SelfImprovementSkill), 6)]
    public partial class ActivatedDeepOceanFillerRecipe : RecipeFamily
    {
        public ActivatedDeepOceanFillerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "ActivatedDeepOceanFiller",
                displayName: Localizer.DoStr("Activated Deep Ocean Filler"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(DeepOceanFillerItem), 1, true)
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<ActivatedDeepOceanFillerItem>(1)
                }
            );
            this.Recipes = new List<Recipe> { recipe };

            this.ExperienceOnCraft = 1;
            this.LaborInCalories = CreateLaborInCaloriesValue(25, typeof(SelfImprovementSkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: this.GetType(), start: 0.64f, skillType: typeof(SelfImprovementSkill));

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
