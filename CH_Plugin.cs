using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ContinuousHealing.Patches;
using EFT.UI;

namespace ContinuousHealing
{
	[BepInPlugin("com.lacyway.ch", "ContinuousHealing", PluginVersion)]
	internal class CH_Plugin : BaseUnityPlugin
	{
		public const string PluginVersion = "1.3.0";

		public static ConfigEntry<bool> HealLimbs { get; set; }
		public static ConfigEntry<int> HealDelay { get; set; }
        public static ConfigEntry<bool> ResetAnimation { get; set; }

		internal static ManualLogSource CH_Logger;

		protected void Awake()
		{
			CH_Logger = Logger;
			CH_Logger.LogInfo($"{nameof(CH_Plugin)} has been loaded.");

			HealLimbs = Config.Bind("1. Settings", "Heal Limbs",
				true, new ConfigDescription("If using surgery kits should also be continuous.\nNOTE: Animation does not loop."));
			HealDelay = Config.Bind("1. Settings", "Heal Delay", 0,
				new ConfigDescription("The delay between every heal on each limb. Game default is 2, set to 0 to use intended Continuous Healing behavior.",
				new AcceptableValueRange<int>(0, 5)));
            ResetAnimation = Config.Bind("1. Settings", "Reset Animations", true,
                new ConfigDescription("If a new animaton should be played between every limb being healed. This does not affect the speed at which you heal."));

			new CH_EndHeal_Patch().Enable();
			new CH_CancelHeal_Patch().Enable();
			new CH_StartHeal_Patch().Enable();

#if DEBUG
            ConsoleScreen.Processor.RegisterCommandGroup<DebugCommands>(); 
#endif
        }
	}
}
