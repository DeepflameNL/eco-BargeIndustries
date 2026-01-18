namespace DF.BargeIndustries
{
    using Eco.Core.Controller;
    using Eco.Core.Utils.Logging;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Items.Recipes;
    using Eco.Gameplay.Modules;
    using Eco.Gameplay.Objects;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using Eco.Shared.Math;
    using Eco.Shared.Serialization;
    using Eco.Shared.States;
    using Eco.Shared.Utils;
    using Eco.Simulation.WorldLayers;
    using Eco.Simulation.WorldLayers.Layers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(StatusComponent), null)]
    [RequireComponent(typeof(PluginModulesComponent), null)]
    public class BargeIndustryStatusComponent : WorldObjectComponent
    {
        private StringBuilder statusMessageBuilder = new StringBuilder(128);
        private bool bIsEnabled = false;

        private StatusComponent statusComponent;
        private PluginModulesComponent pluginModules;
        private VehicleComponent vehicleComponent;
        private CraftingComponent craftingComponent;
        private OnOffComponent onOffComponent;

        private List<BargeMode> BargeModes = new List<BargeMode>
        {
            new BargeModeLandReclamation(),
            new BargeModeOffshoreDrilling(),
            new BargeModeOffshoreMining()
        };

        private BargeMode CurrentBargeMode;

        private bool Debug => false;

        public override bool Enabled => bIsEnabled;

        public override void Initialize()
        {
            base.Initialize();

            statusComponent = Parent.GetComponent<StatusComponent>();
            pluginModules = Parent.GetComponent<PluginModulesComponent>();
            vehicleComponent = Parent.GetComponent<VehicleComponent>();
            craftingComponent = Parent.GetComponent<CraftingComponent>();
            onOffComponent = Parent.GetComponent<OnOffComponent>();

            foreach (BargeMode bargeMode in BargeModes)
            {
                bargeMode.InitializeBargeMode(Parent as IndustrialBargeObject);
            }
        }

        public override void Tick()
        {
            base.Tick();

            Vector2i parentPosition = Parent.GetOccupancyRangeWorldPos().CenterExc.XZi();

            bIsEnabled = true;
            statusComponent.Statuses.Clear();
            if (Debug)
            {
                statusMessageBuilder.Clear();
            }

            CurrentBargeMode = GetCurrentBargeMode();
            if (CurrentBargeMode == null)
            {
                AddStatusMessage(false, Localizer.DoStr($"{Parent.Name} is in Transportation mode. Insert a module to activate industrial features."));
            }
            else
            {
                LogMessage(Localizer.DoStr($"Barge mode: {GetCurrentBargeMode()?.Name ?? "Transportation Mode"}\n"));
                
                AddStatusMessage(true, Localizer.DoStr($"{Parent.Name} is in {CurrentBargeMode.Name} mode."));

                if (CurrentBargeMode.RequiresDeepOceanBiome)
                {
                    float operatingRadius = CurrentBargeMode.OperatingRadius;
                    float deepOceanThreshold = CurrentBargeMode.OperatingThreshold;
                    float deepOceanCurrentAmount = 0.0f;

                    WorldLayer deepOceanLayer = WorldLayerManager.Obj.GetLayer(BiomeType.DeepOceanBiome.GetName());
                    deepOceanLayer.SumAndCountOverBoundaryAlignedWorldArea(
                        new WorldArea(Parent.WorldPosXZ(), operatingRadius),
                        false,
                        out deepOceanCurrentAmount,
                        out int numCells
                    );

                    LogMessage(
                        Localizer.DoStr(
                            $"Deep Ocean biome [x: {parentPosition.X}, y: {parentPosition.Y}] (Deep Ocean Amount: {deepOceanCurrentAmount} >= {deepOceanThreshold}), over {numCells} cells\n"
                        )
                    );

                    AddStatusMessage(
                        deepOceanCurrentAmount >= deepOceanThreshold,
                        Localizer.DoStr($"There is enough Deep Ocean nearby."),
                        Localizer.DoStr($"There is insufficient Deep Ocean nearby.")
                    );
                }

                if (CurrentBargeMode.RequiresStationary)
                {
                    AddStatusMessage(
                        vehicleComponent == null || vehicleComponent.Driver == null,
                        Localizer.DoStr($"It is free of drivers."),
                        Localizer.DoStr($"It cannot have a driver.")
                    );
                }

                if (CurrentBargeMode.SupportedWorkOrders.Count > 0)
                {
                    AddStatusMessage(
                        HasWorkOrderOtherThan(CurrentBargeMode.SupportedWorkOrders),
                        "",
                        Localizer.DoStr($"Incompatible work orders have been queued up.")
                    );
                }

                foreach (var additionalStatusMessage in CurrentBargeMode.GetAdditionalStatuses())
                {
                    AddStatusMessage(
                        additionalStatusMessage.bEnabled,
                        additionalStatusMessage.EnabledMessage,
                        additionalStatusMessage.DisabledMessage
                    );
                }
            }

            if (onOffComponent.On && !bIsEnabled)
            {
                onOffComponent.On = false;
            }

            if (bIsEnabled && onOffComponent.On)
            {
                CurrentBargeMode.TickModeBehavior();
            }

            LogMessage(
                Localizer.DoStr(
                    $"Enabled: {Enabled}, status message: {statusMessageBuilder.ToString()}\n"
                )
            );
        }

        public BargeMode GetCurrentBargeMode()
        {
            if (pluginModules != null)
            {
                PluginModule PluggedInModule = pluginModules.GetModule(ModuleTypes.SpeedEfficiency);
                if (PluggedInModule != null)
                {
                    foreach (var bargeMode in BargeModes)
                    {
                        if (bargeMode.SupportedModules.Contains(PluggedInModule))
                        {
                            return bargeMode;
                        }
                    }
                }
            }
            return null;
        }

        private void AddStatusMessage(bool bEnabled, string statusMessage)
        {
            bIsEnabled = bIsEnabled && bEnabled;
            statusComponent.CreateStatusElement().SetStatusMessage(bEnabled, Localizer.DoStr(statusMessage));

            if (Debug)
            {
                statusMessageBuilder.AppendLine(statusMessage);
            }
        }

        private void AddStatusMessage(bool bEnabled, string enabledMessage, string disabledMessage)
        {
            bIsEnabled = bIsEnabled && bEnabled;
            string messageToAppend = bEnabled ? enabledMessage : disabledMessage;
            if (!string.IsNullOrWhiteSpace(messageToAppend))
            {
                statusComponent.CreateStatusElement().SetStatusMessage(bEnabled, Localizer.DoStr(messageToAppend));

                if (Debug)
                {
                    statusMessageBuilder.AppendLine(messageToAppend);
                }
            }
        }

        private bool HasWorkOrderOtherThan(IReadOnlyCollection<RecipeFamily> recipeFamilies)
        {
            if (craftingComponent == null) return false;

            foreach (var workOrder in craftingComponent.WorkOrders)
            {
                if (!recipeFamilies.Contains(workOrder.Recipe))
                {
                    return false;
                }
            }
            return true;
        }

        private void LogMessage(string message)
        {
            if (Debug)
            {
                ConsoleLogWriter.Instance.Write(message);
            }
        }
    }
}
