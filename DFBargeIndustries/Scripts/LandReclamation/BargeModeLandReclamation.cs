using Eco.Gameplay.Components;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Modules;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.States;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using System;
using System.Collections.Generic;

namespace DF.BargeIndustries
{
    public class BargeModeLandReclamation : BargeMode
    {
        public override string Name => "Land Reclamation";
        protected override bool Debug => base.Debug;

        public override IReadOnlyList<PluginModule> SupportedModules => new List<PluginModule>
        {
            Item.Get<ModernUpgradeLvl1Item>(),
            Item.Get<ModernUpgradeLvl2Item>(),
            Item.Get<ModernUpgradeLvl3Item>(),
            Item.Get<ModernUpgradeLvl4Item>()
        };

        public override IReadOnlyList<RecipeFamily> SupportedWorkOrders => new List<RecipeFamily>
        {
            RecipeManager.GetRecipeFamily<ActivatedDeepOceanFillerRecipe>()
        };

        public override bool RequiresDeepOceanBiome => false;
        public override bool RequiresStationary
        {
            get
            {
                // Driver support is available for MU2 and MU4.
                int currentPluginLevel = GetPluginLevel();
                if (currentPluginLevel == 2 || currentPluginLevel == 4)
                    return false;
                return false;
            }
        }

        private Type FillerItemType = typeof(ActivatedDeepOceanFillerItem);

        private PublicStorageComponent storageComponent;
        private PluginModulesComponent pluginModules;

        private Dictionary<int, List<Vector2i>> OffsetsPerLevel;

        public override void InitializeBargeMode(IndustrialBargeObject barge)
        {
            base.InitializeBargeMode(barge);

            storageComponent = barge.GetComponent<PublicStorageComponent>();
            pluginModules = barge.GetComponent<PluginModulesComponent>();

            OffsetsPerLevel = new Dictionary<int, List<Vector2i>>();
            OffsetsPerLevel.Add(0, new List<Vector2i>());
            OffsetsPerLevel.Add(1, new List<Vector2i>() {
                Vector2i.Zero
            });
            OffsetsPerLevel.Add(2, new List<Vector2i>() {
                new Vector2i(-3, -3), new Vector2i(2, -3),
                new Vector2i(-3,  2), new Vector2i(2,  2)
            });
            OffsetsPerLevel.Add(3, new List<Vector2i>() {
                // Start with center tile
                new Vector2i(0,  0),

                // Then cirle outwards
                new Vector2i(-5, -5), new Vector2i(0, -5), new Vector2i(5, -5),
                new Vector2i(-5,  0),                      new Vector2i(5,  0),
                new Vector2i(-5,  5), new Vector2i(0,  5), new Vector2i(5,  5)
            });
            OffsetsPerLevel.Add(4, new List<Vector2i>() {
                // Start with center 4 tiles
                new Vector2i(-3, -3), new Vector2i(2, -3),
                new Vector2i(-3,  2), new Vector2i(2,  2),

                // Then circle outwards
                new Vector2i(-8, -8), new Vector2i(-3, -8), new Vector2i(2, -8), new Vector2i(7, -8),
                new Vector2i(-8, -3),                                            new Vector2i(7, -3),
                new Vector2i(-8,  2),                                            new Vector2i(7,  2),
                new Vector2i(-8,  7), new Vector2i(-3,  7), new Vector2i(2,  7), new Vector2i(7,  7)
            });
        }

        public override void TickModeBehavior()
        {
            base.TickModeBehavior();

            if (HasFillerMaterial())
            {
                TryFillDeepOcean();
            }
        }

        public int GetPluginLevel()
        {
            if (pluginModules != null)
            {
                PluginModule PluggedInModule = pluginModules.GetModule(ModuleTypes.SpeedEfficiency);
                if (PluggedInModule != null)
                {
                    if (PluggedInModule == Item.Get<ModernUpgradeLvl1Item>())
                    {
                        return 1;
                    }
                    else if (PluggedInModule == Item.Get<ModernUpgradeLvl2Item>())
                    {
                        return 2;
                    }
                    else if (PluggedInModule == Item.Get<ModernUpgradeLvl3Item>())
                    {
                        return 3;
                    }
                    else if (PluggedInModule == Item.Get<ModernUpgradeLvl4Item>())
                    {
                        return 4;
                    }
                }
            }
            return 0;
        }

        private List<Vector2i> GetNearbyFillableTiles()
        {
            List<Vector2i> fillableTiles = new List<Vector2i>();

            // Fill range depends on MU, MU 1 and 2 have a range of 2, MU 3 and 4 have a range of 4.
            int fillRange = 0;
            switch (GetPluginLevel())
            {
                case 1:
                case 2:
                    fillRange = 2;
                    break;

                case 3:
                case 4:
                    fillRange = 4;
                    break;

                default:
                    fillRange = 0;
                    break;
            }

            Vector2i parentPosition = Barge.GetOccupancyRangeWorldPos().CenterExc.XZi();
            WorldLayer saltLayer = WorldLayerManager.Obj.GetLayer(LayerNames.SaltWater);

            foreach (Vector2i offset in OffsetsPerLevel[fillRange])
            {
                Vector2i fillPosition = parentPosition + offset;
                float currentState = saltLayer.EntryWorldPos(fillPosition);
                if (currentState > 0)
                {
                    fillableTiles.Add(fillPosition);
                }
            }

            return fillableTiles;
        }

        private bool HasFillerMaterial()
        {
            return storageComponent.Storage.ContainsItem(Item.Get(FillerItemType));
        }

        private void DeleteFillerMaterial()
        {
            storageComponent.Storage.RemoveItem(FillerItemType);
        }

        private void TryFillDeepOcean()
        {
            WorldLayer deepOceanLayer = WorldLayerManager.Obj.GetLayer(BiomeType.DeepOceanBiome.GetName());
            WorldLayer saltLayer = WorldLayerManager.Obj.GetLayer(LayerNames.SaltWater);
            int tilesFilled = 0;

            var fillableTiles = GetNearbyFillableTiles();
            foreach (Vector2i fillPosition in fillableTiles)
            {
                if (!HasFillerMaterial())
                {
                    LogMessage("No filler remaining, stopping fill.\n");
                    break;
                }

                LogMessage($"Filling {fillPosition.ToString()}\n");

                deepOceanLayer.SetAtWorldPos(fillPosition, 0f);
                saltLayer.SetAtWorldPos(fillPosition, 0f);
                tilesFilled++;

                DeleteFillerMaterial();
            }

            if (tilesFilled > 0)
            {
                WorldLayerManager.Obj.SaveAll();
                WorldLayerManager.Obj.ForceTick();
                WorldLayerManager.Obj.SaveAll();

                LogMessage($"Filled {tilesFilled} tiles\n");
            }
        }

        public override IReadOnlyCollection<BargeModeStatus> GetAdditionalStatuses()
        {
            var fillableTiles = GetNearbyFillableTiles();
            return new List<BargeModeStatus>
            {
                new BargeModeStatus(
                    fillableTiles.Count > 0,
                    Localizer.DoStr($"There are tiles nearby that can be filled."),
                    Localizer.DoStr($"There are no tiles nearby that can be filled.")
                )
            };
        }
    }
}
