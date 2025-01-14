using Distance.NitronicHUD.Data;
using HarmonyLib;
using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Distance.NitronicHUD.Scripts
{
	public class VisualDisplay : MonoBehaviour
	{
		#region Properties and Fields
		public const string AssetName = "assets/nr hud/nr_hud.prefab";

		public static bool ForceDisplay { get; set; } = false;

		public Assets Assets { get; internal set; }

		public AssetBundle Bundle => Assets.Bundle as AssetBundle;

		private GameObject Prefab { get; set; }

		private VisualDisplayContent[] huds_;

		private Text timer_;
		#endregion

		#region Prefab Setup
		private void CreatePrefab(bool loadBundle = true)
		{
			if (loadBundle)
			{
				Assets = new Assets("hud.assets", true);
			}

			if (!Bundle)
			{
				Mod.Log.LogInfo("The following assets file could not be loaded: hud.assets");

				DestroyImmediate(this);
				return;
			}

			Prefab = Instantiate(Bundle.LoadAsset<GameObject>(AssetName), transform);

			if (!Prefab)
			{
				Mod.Log.LogInfo($"The following asset from the hud.assets could not be loaded: \"{AssetName}\"");

				DestroyImmediate(this);
				return;
			}

			Prefab.name = "Visual Display";

			GameObject hud_left = Prefab.transform.Find("Hud_Left").gameObject;
			GameObject hud_right = Prefab.transform.Find("Hud_Right").gameObject;

			huds_ = new VisualDisplayContent[2]
			{
				new VisualDisplayContent(hud_left),
				new VisualDisplayContent(hud_right)
			};

			timer_ = Prefab?.transform.Find("Time")?.GetComponent<Text>();
		}
		#endregion

		#region Unity Calls
		public void Awake()
		{
			CreatePrefab();
		}

		public void Update()
		{
			if (huds_.Length == 0)
			{
				return;
			}

			UpdateVisibility();
			UpdateTransforms();
			UpdateTimerText();
			UpdateHeatIndicators();
			UpdateScoreLabel();
			UpdateSpeedLabel();
		}
		#endregion

		#region Update Logic
		#region Object Active States
		private void UpdateVisibility()
		{
			huds_.Do(x => x.rectTransform.gameObject.SetActive((Flags.CanDisplayHudElements && Mod.DisplayHeatMeter.Value) || ForceDisplay));
			timer_?.gameObject?.SetActive((Flags.CanDisplayHudElements && Mod.DisplayTimer.Value) || ForceDisplay);
		}
		#endregion

		#region Size / Position Logic
		public void UpdateTransforms()
		{
			if (huds_.Length >= 2)
			{
				for (int x = 0; x <= 1; x++)
				{
					float direction = x == 0 ? 1 : -1;

					const float defaultScale = 1.7f;
					float newScale = defaultScale * (Mod.HeatMeterScale.Value / 30f);

					huds_[x].rectTransform.localScale = new Vector3(newScale * direction, newScale, newScale);
					huds_[x].rectTransform.anchoredPosition = new Vector2(Mod.HeatMetersHorizontalOffset.Value * direction, Mod.HeatMetersVerticalOffset.Value);
				}
			}

			if (timer_)
			{
				const float defaultScale = 0.5f;

				RectTransform rect = timer_.GetComponent<RectTransform>();

				if (rect)
				{
					rect.localScale = Vector2.one * defaultScale * (Mod.TimerScale.Value / 30f);
					rect.anchoredPosition = new Vector2(0, Mod.TimerVerticalOffset.Value + 45);
				}
			}
		}
		#endregion

		#region Overheat Meter
		private void UpdateHeatIndicators()
		{
			try
			{
				float heat = Mathf.Clamp(Vehicle.HeatLevel, 0, 1);

				if (huds_.Length >= 2)
				{
					for (int x = 0; x <= 1; x++)
					{
						VisualDisplayContent instance = huds_[x];

						if (!instance.main)
						{
							continue;
						}

						instance.heatHigh.fillAmount = heat;
						instance.heatLow.fillAmount = heat;

						float blink = 0;

						if (heat > Mod.HeatBlinkStartAmount.Value)
						{
							blink = (heat - Mod.HeatBlinkStartAmount.Value) / (1 - Mod.HeatBlinkStartAmount.Value);
						}

						blink *= (0.5f * Mathf.Sin((float)Timex.ModeTime_ * (Mod.HeatBlinkFrequency.Value - ((1 - heat) * heat * Mod.HeatBlinkFrequencyBoost.Value)) * 3 * Mathf.PI)) + 0.5f;
						instance.main.color = new Color(1, 1 - (blink * Mod.HeatBlinkAmount.Value), 1 - (blink * Mod.HeatBlinkAmount.Value));

						float flame = 0;

						if (heat > Mod.HeatFlameAmount.Value)
						{
							flame = (heat - Mod.HeatFlameAmount.Value) / (1 - Mod.HeatFlameAmount.Value);
						}

						instance.flame.color = new Color(1, 1, 1, flame);
					}
				}
			}
			catch (Exception ex)
			{
				Mod.Log.LogError(ex);
			}
		}
		#endregion

		#region Timer Logic
		private void UpdateTimerText()
		{
			GameManager gameManager = G.Sys.GameManager_;
			GameMode gamemode = gameManager.Mode_;

			if (!gamemode || !timer_)
			{
				return;
			}

			float time = Mathf.Max(0, (float)gamemode.GetDisplayTime(0));

			StringBuilder result = new StringBuilder();

			GUtils.GetFormattedTime(result, time, time >= 3600 ? 0 : 2, time > 3600);

			timer_.text = result.ToString();
		}
		#endregion

		#region Score Logic
		private void UpdateScoreLabel()
		{
			if (huds_.Length >= 2)
			{
				for (int x = 0; x <= 1; x++)
				{
					VisualDisplayContent hud = huds_[x];

					if (hud.main && hud.score)
					{
						hud.score.text = GetScore().ToString(CultureInfo.GetCultureInfo("en-GB") ?? CultureInfo.InvariantCulture);
					}
				}
			}
		}

		private long GetScore()
		{
			if (G.Sys.GameManager_.IsModeStarted_)
			{
				return G.Sys.StatsManager_?.GetMatchStats(G.Sys.PlayerManager_?.current_.playerData_).totalPoints_ ?? 0;
			}
			else
			{
				return 0;
			}
		}
		#endregion

		#region Speed Logic
		private void UpdateSpeedLabel()
		{
			if (huds_.Length >= 2)
			{
				for (int x = 0; x <= 1; x++)
				{
					VisualDisplayContent hud = huds_[x];

					if (hud.speed)
					{
						hud.speed.text = Mathf.RoundToInt(GetSpeedValue()).ToString();
					}

					if (hud.speedLabel)
					{
						hud.speedLabel.text = GetSpeedUnit();
					}
				}
			}
		}

		private float GetSpeedValue()
		{
			if (G.Sys.GameManager_.IsModeStarted_)
			{
				switch (G.Sys.OptionsManager_.General_.Units_)
				{
					case Units.Metric:
						return Vehicle.VelocityKPH;
					case Units.Imperial:
						return Vehicle.VelocityMPH;
					default:
						return 0;
				}
			}
			else
			{
				return 0;
			}
		}

		private string GetSpeedUnit()
		{
			switch (G.Sys.OptionsManager_.General_.Units_)
			{
				case Units.Metric:
					return "KM/H";
				case Units.Imperial:
					return "MPH";
				default:
					return "ERR";
			}
		}
		#endregion
		#endregion
	}
}