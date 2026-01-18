namespace DF.BargeIndustries
{
    using Eco.Core.Items;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Skills;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// <para>Server side item definition for the "SteelDrillBit" item.</para>
    /// </summary>
    [Serialized]
    [LocDisplayName("Steel Drill Bit")]
    [Weight(10000)]
    [MaxStackSize(10)]
    [Ecopedia("Items", "Products", createAsSubPage: true)]
    [LocDescription("An essential part in the effort of drilling for resources.")]
    public partial class SteelDrillBitItem : PartItem
    {
        public override IDynamicValue SkilledRepairCost => skilledRepairCost;
        private static IDynamicValue skilledRepairCost = new ConstantValue(1);
        public override LocString DisplayNamePlural { get { return Localizer.DoStr("Steel Drill Bits"); } }
        public float ReduceMaxDurabilityByPercent => 0.05f;
    }

    [RequiresSkill(typeof(IndustrySkill), 5)]
    public partial class SteelDrillBitRecipe : RecipeFamily
    {
        public SteelDrillBitRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                name: "SteelDrillBit",
                displayName: Localizer.DoStr("Steel Drill Bit"),

                ingredients: new List<IngredientElement>
                {
                    new IngredientElement(typeof(SteelGearItem), 5, typeof(IndustrySkill), typeof(IndustryLavishResourcesTalent)),
                    new IngredientElement(typeof(SteelBarItem), 2, typeof(IndustrySkill), typeof(IndustryLavishResourcesTalent)),
                    new IngredientElement(typeof(LubricantItem), 1, typeof(IndustrySkill), typeof(IndustryLavishResourcesTalent))
                },

                items: new List<CraftingElement>
                {
                    new CraftingElement<SteelDrillBitItem>(1)
                }
            );
            this.Recipes = new List<Recipe> { recipe };

            this.ExperienceOnCraft = 1;
            this.LaborInCalories = CreateLaborInCaloriesValue(250, typeof(IndustrySkill));
            this.CraftMinutes = CreateCraftTimeValue(beneficiary: this.GetType(), start: 1.2f, skillType: typeof(IndustrySkill), typeof(IndustryFocusedSpeedTalent), typeof(IndustryParallelSpeedTalent));

            this.ModsPreInitialize();
            this.Initialize(displayText: recipe.DisplayName, recipeType: this.GetType());
            this.ModsPostInitialize();

            CraftingComponent.AddRecipe(tableType: typeof(ElectricMachinistTableObject), recipeFamily: this);
        }

        /// <summary>Hook for mods to customize RecipeFamily before initialization. You can change recipes, xp, labor, time here.</summary>
        partial void ModsPreInitialize();

        /// <summary>Hook for mods to customize RecipeFamily after initialization, but before registration. You can change skill requirements here.</summary>
        partial void ModsPostInitialize();
    }
}
