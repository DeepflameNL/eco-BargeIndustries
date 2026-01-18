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
    /// <para>Server side item definition for the "DeepOceanFiller" item.</para>
    /// </summary>
    [Serialized]
    [LocDisplayName("Deep Ocean Filler")]
    [Weight(22000)]
    [MaxStackSize(10)]
    [Ecopedia("Items", "Products", createAsSubPage: true)]
    [LocDescription("A mixture of sand, clay, rocks, and cement to fill up the deep ocean.")]
    public partial class DeepOceanFillerItem : Item { }

    [RequiresSkill(typeof(AdvancedMasonrySkill), 6)]
    public partial class DeepOceanFillerRecipe : RecipeFamily
    {
        public DeepOceanFillerRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "DeepOceanFiller",
                displayName: Localizer.DoStr("Deep Ocean Filler"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SandItem), 2, typeof(AdvancedMasonrySkill), typeof(AdvancedMasonryLavishResourcesTalent)),
                    new IngredientElement(typeof(ClayItem), 2, typeof(AdvancedMasonrySkill), typeof(AdvancedMasonryLavishResourcesTalent)),
                    new IngredientElement("CrushedRock", 2, typeof(AdvancedMasonrySkill), typeof(AdvancedMasonryLavishResourcesTalent)),
                    new IngredientElement(typeof(CementItem), 2, typeof(AdvancedMasonrySkill), typeof(AdvancedMasonryLavishResourcesTalent))
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<DeepOceanFillerItem>(1)
                }
            );
            this.Recipes = new List<Recipe> { recipe };

            this.ExperienceOnCraft = 1;
            this.LaborInCalories = CreateLaborInCaloriesValue(25, typeof(AdvancedMasonrySkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: this.GetType(), start: 0.64f, skillType: typeof(AdvancedMasonrySkill), typeof(AdvancedMasonryFocusedSpeedTalent), typeof(AdvancedMasonryParallelSpeedTalent));

            this.ModsPreInitialize();
            this.Initialize(displayText: recipe.DisplayName, recipeType: this.GetType());
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(CementKilnObject), recipeFamily: this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
