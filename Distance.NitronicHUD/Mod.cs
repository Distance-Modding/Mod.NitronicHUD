using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Distance.NitronicHUD.Scripts;
using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.NitronicHUD
{
	[BepInPlugin(modGUID, modName, modVersion)]
	public sealed class Mod : BaseUnityPlugin
	{
		//Mod Details
		private const string modGUID = "Distance.NitronicHUD";
		private const string modName = "Nitronic HUD";
		private const string modVersion = "1.0.0";

		//Config Entry Strings
		public static string DisplayHeatMeterKey = "Display Heat Meter";
		public static string DisplayCountdownKey = "Display Countdown";
		public static string DisplayTimerKey = "Display Timer";
		public static string AnnouncerCountdownKey = "Display Countdown";
		public static string HeatMeterScaleKey = "Overheat Scale";
		public static string HeatMeterHorizontalKey = "Overheat HOffset";
		public static string HeatMeterVerticalKey = "Overheat VOffset";
		public static string TimerScaleKey = "Timer Scale";
		public static string TimerVeritcalKey = "Timer VOffset";
		public static string HeatBlinkStartAmountKey = "Blink Start";
		public static string HeatBlinkFrequencyKey = "Blink Frequency";
		public static string HeatBlinkFrequencyBoostKey = "Blink Freq Boost";
		public static string HeatBlinkAmountKey = "Heat Blink Amount";
		public static string HeatFlameAmountKey = "Heat Flame Amount";

		//Config Entries
		public static ConfigEntry<bool> DisplayHeatMeter { get; set; }
		public static ConfigEntry<bool> DisplayCountdown { get; set; }
		public static ConfigEntry<bool> DisplayTimer { get; set; }
		//public static ConfigEntry<bool> AnnouncerCountdown { get; set; }
		public static ConfigEntry<float> HeatMeterScale { get; set; }
		public static ConfigEntry<int> HeatMetersHorizontalOffset { get; set; }
		public static ConfigEntry<int> HeatMetersVerticalOffset { get; set; }
		public static ConfigEntry<float> TimerScale { get; set; }
		public static ConfigEntry<int> TimerVerticalOffset { get; set; }
		public static ConfigEntry<float> HeatBlinkStartAmount { get; set; }
		public static ConfigEntry<float> HeatBlinkFrequency { get; set; }
		public static ConfigEntry<float> HeatBlinkFrequencyBoost { get; set; }
		public static ConfigEntry<float> HeatBlinkAmount { get; set; }
		public static ConfigEntry<float> HeatFlameAmount { get; set; }

		//Public Variables
		public MonoBehaviour[] Scripts { get; set; }

		//Other
		private static readonly Harmony harmony = new Harmony(modGUID);
		public static ManualLogSource Log = new ManualLogSource(modName);
		public static Mod Instance;

		public void Awake()
		{
			DontDestroyOnLoad(this);

			if (Instance == null)
			{
				Instance = this;
			}

			Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
			Logger.LogInfo("Thanks for using Nitronic HUD!");

			//Config Setup
			DisplayCountdown = Config.Bind<bool>("Interface Options",
				DisplayCountdownKey,
				true,
				new ConfigDescription("Displays the 3... 2... 1... RUSH countdown when playing a level."));

			DisplayHeatMeter = Config.Bind<bool>("Interface Options",
				DisplayHeatMeterKey,
				true,
				new ConfigDescription("Displays overheat indicator bars in the lower screen corners."));

			DisplayTimer = Config.Bind<bool>("Interface Options",
				DisplayTimerKey,
				true,
				new ConfigDescription("Displays the timer at the bottom of the screen."));

			HeatMeterScale = Config.Bind<float>("Interface Options",
				HeatMeterScaleKey,
				20f,
				new ConfigDescription("Set the size of the overheat bars.",
					new AcceptableValueRange<float>(1f, 50f)));

			HeatMetersHorizontalOffset = Config.Bind<int>("Interface Options",
				HeatMeterHorizontalKey,
				0,
				new ConfigDescription("Set the horizontal position offset of the overheat bars.",
					new AcceptableValueRange<int>(-200, 200)));

			HeatMetersVerticalOffset = Config.Bind<int>("Interface Options",
				HeatMeterVerticalKey,
				0,
				new ConfigDescription("Set the vertical position offset of the overheat bars.",
					new AcceptableValueRange<int>(-100, 100)));

			TimerScale = Config.Bind<float>("Interface Options",
				TimerScaleKey,
				20f,
				new ConfigDescription("Set the size of the timer.",
					new AcceptableValueRange<float>(1f, 50f)));

			TimerVerticalOffset = Config.Bind<int>("Interface Options",
				TimerVeritcalKey,
				0,
				new ConfigDescription("Set the vertical position of the timer.",
					new AcceptableValueRange<int>(-100, 100)));

			HeatBlinkStartAmount = Config.Bind<float>("Advanced Interface Options",
				HeatBlinkStartAmountKey,
				0.7f,
				new ConfigDescription("Set the heat threshold after which the hud starts to blink.",
					new AcceptableValueRange<float>(0.0f, 1.0f)));

			HeatBlinkFrequency = Config.Bind<float>("Advanced Interface Options",
				HeatBlinkFrequencyKey,
				2.0f,
				new ConfigDescription("Set the hud blink rate (per second).",
					new AcceptableValueRange<float>(0.0f, 10.0f)));

			HeatBlinkFrequencyBoost = Config.Bind<float>("Advanced Interface Options",
				HeatBlinkFrequencyBoostKey,
				1.15f,
				new ConfigDescription("Sets the blink rate boost.\nThe blink rate at 100% heat is the blink rate times this value (set this to 1 to keep the rate constant).",
					new AcceptableValueRange<float>(0.0f, 10.0f)));

			HeatBlinkAmount = Config.Bind<float>("Advanced Interface Options",
				HeatBlinkAmountKey,
				0.7f,
				new ConfigDescription("Sets the color intensity of the overheat blink animation (lower values means smaller color changes).",
					new AcceptableValueRange<float>(0.0f, 1.0f)));

			HeatFlameAmount = Config.Bind<float>("Advanced Interface Options",
				HeatFlameAmountKey,
				0.5f,
				new ConfigDescription("Sets the color intensity of the overheat flame animation (lower values means smaller color changes).",
					new AcceptableValueRange<float>(0.0f, 1.0f)));


			//Apply Patches
			Logger.LogInfo("Loading...");
			harmony.PatchAll();
			Logger.LogInfo("Loaded!");
		}

		public void OnEnable()
        {
			Flags.SubscribeEvents();
			//MenuOpened.Subscribe(OnGUIMenuOpened);
		}

		public void OnDisable()
        {
			Flags.UnsubscribeEvents();
        }

		//This event is dependent on the event from Centrifuge. It turns off the nitronic HUD when the Centrifuge menu opens.
		//DistanceModConfiguration has this event, but it would never trigger it
		/*private void OnGUIMenuOpened(MenuOpened.Data data)
		{
			if (data.menu.Id != "menu.mod.nitronichud#interface")
			{
				VisualDisplay.ForceDisplay = false;
			}
		}*/

		public void LateInitialize()
		{
			Scripts = new MonoBehaviour[2]
			{
				gameObject.AddComponent<VisualCountdown>(),
				gameObject.AddComponent<VisualDisplay>()
			};
		}
	}
}