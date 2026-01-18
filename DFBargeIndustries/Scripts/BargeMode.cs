using Eco.Core.Utils.Logging;
using Eco.Gameplay.Components;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Modules;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using System.Collections.Generic;

namespace DF.BargeIndustries
{
    public abstract class BargeMode
    {
        public abstract string Name { get; }
        protected virtual bool Debug => false;

        /// <summary>
        /// What upgrade plugin modules activate this barge mode? Can be more than one.
        /// </summary>
        public virtual IReadOnlyList<PluginModule> SupportedModules
        {
            get
            {
                return new List<PluginModule>();
            }
        }

        /// <summary>
        /// What work orders are compatible with this barge mode? Can be more than one.
        /// </summary>
        public virtual IReadOnlyList<RecipeFamily> SupportedWorkOrders
        {
            get
            {
                return new List<RecipeFamily>();
            }
        }

        /// <summary>
        /// Does this barge mode only function if the barge is stationary and undriven?
        /// </summary>
        public virtual bool RequiresStationary => true;

        /// <summary>
        /// Does this barge mode only function if inside the Deep Ocean biome?
        /// </summary>
        public virtual bool RequiresDeepOceanBiome => true;
        public virtual float OperatingRadius => 0.0f;
        public virtual float OperatingThreshold => 0.0f;

        protected IndustrialBargeObject Barge { get; private set; }

        public virtual void InitializeBargeMode(IndustrialBargeObject barge)
        {
            Barge = barge;
        }

        public virtual void TickModeBehavior()
        {
        }

        protected void LogMessage(string message)
        {
            if (Debug)
            {
                ConsoleLogWriter.Instance.Write(message);
            }
        }

        public class BargeModeStatus
        {
            public bool bEnabled = false;
            public LocString EnabledMessage = LocString.Empty;
            public LocString DisabledMessage = LocString.Empty;

            public BargeModeStatus() { }
            public BargeModeStatus(bool bEnabled, LocString Message)
            {
                this.bEnabled = bEnabled;

                if (bEnabled)
                {
                    EnabledMessage = Message;
                }
                else
                {
                    DisabledMessage = Message;
                }
            }
            public BargeModeStatus(bool bEnabled, LocString EnabledMessage, LocString DisabledMessage)
            {
                this.bEnabled = bEnabled;
                this.EnabledMessage = EnabledMessage;
                this.DisabledMessage = DisabledMessage;
            }
        }

        public virtual IReadOnlyCollection<BargeModeStatus> GetAdditionalStatuses() { return new List<BargeModeStatus>(); }
    }

    public static class BargeModeExtensions
    {
        public static bool Is<BargeModeType>(this BargeMode bargeMode) where BargeModeType : BargeMode
        {
            if (bargeMode == null) return false;
            return bargeMode.GetType() == typeof(BargeModeType);
        }
    }
}
