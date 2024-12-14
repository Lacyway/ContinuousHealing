using BepInEx;
using BepInEx.Logging;
using ContinuousHealing.Patches;

namespace ContinuousHealing
{
	[BepInPlugin("com.lacyway.ch", "ContinuousHealing", "1.1.1")]
	internal class CH_Plugin : BaseUnityPlugin
	{
		internal static ManualLogSource CH_Logger;

		protected void Awake()
		{
			CH_Logger = Logger;
			CH_Logger.LogInfo($"{nameof(CH_Plugin)} has been loaded.");

			new CH_EndHeal_Patch().Enable();
			new CH_CancelHeal_Patch().Enable();
			new CH_StartHeal_Patch().Enable();
		}
	}
}
