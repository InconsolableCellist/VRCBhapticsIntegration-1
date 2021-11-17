using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace VRCBhapticsIntegration
{
	internal static class ModConfig
	{
		internal static int DefaultIntensity = 50;
		private static MelonPreferences_Category Category;
		internal static MelonPreferences_Entry<bool> Allow_bHapticsPlayer_Communication;
		internal static MelonPreferences_Entry<bool> Use_AsyncGPUReadback;
		internal static Dictionary<bHaptics.PositionType, MelonPreferences_Entry<bool>> Entries_Enable;
		internal static Dictionary<bHaptics.PositionType, MelonPreferences_Entry<int>> Entries_Intensity;

		internal static void Initialize()
		{
			Category = MelonPreferences.CreateCategory(BuildInfo.Name, BuildInfo.Name);
			string filepath = Path.Combine(MelonUtils.UserDataDirectory, $"{BuildInfo.Name}.cfg");
			Category.SetFilePath(filepath);

			Allow_bHapticsPlayer_Communication = Category.CreateEntry("Allow_bHapticsPlayer_Communication", true, "Allow bHapticsPlayer Communication");
			Allow_bHapticsPlayer_Communication.OnValueChanged += (oldval, newval) =>
			{
				MelonLogger.Msg($"bHaptics Player Communication is now {(newval ? "Enabled" : "Disabled")}");
				foreach (KeyValuePair<bHaptics.PositionType, string> keyValuePair in PosToName)
					VRCBhapticsIntegration.ResetCameraParser(keyValuePair.Key);
			};

			Use_AsyncGPUReadback = Category.CreateEntry("Use_AsyncGPUReadback", true, "Use AsyncGPUReadback if Supported");
			Use_AsyncGPUReadback.OnValueChanged += (oldval, newval) => { MelonLogger.Msg($"AsyncGPUReadback Usage is now {(newval ? "Enabled" : "Disabled")}"); };

			Entries_Enable = new Dictionary<bHaptics.PositionType, MelonPreferences_Entry<bool>>();
			Entries_Intensity = new Dictionary<bHaptics.PositionType, MelonPreferences_Entry<int>>();
			foreach (KeyValuePair<bHaptics.PositionType, string> keyValuePair in PosToName)
            {
				string name = keyValuePair.Value;
				string nameUnderscore = name.Replace(" ", "_");

				Entries_Enable[keyValuePair.Key] = Category.CreateEntry($"Enable_{nameUnderscore}", true, $"Enable {name}");
				Entries_Enable[keyValuePair.Key].OnValueChanged += (oldval, newval) =>
				{
					MelonLogger.Msg($"{name} is now {(newval ? "Enabled" : "Disabled")}");
					VRCBhapticsIntegration.ResetCameraParser(keyValuePair.Key);
				};

				Entries_Intensity[keyValuePair.Key] = Category.CreateEntry($"{nameUnderscore}_Intensity", DefaultIntensity, $"{name} Intensity");
				Entries_Intensity[keyValuePair.Key].OnValueChanged += (oldval, newval) => { MelonLogger.Msg($"{name} Intensity is now set to {newval}"); };
			}

			if (!File.Exists(filepath))
				Category.SaveToFile(false);
		}

		internal static Dictionary<bHaptics.PositionType, string> PosToName = new Dictionary<bHaptics.PositionType, string>()
		{
			{ bHaptics.PositionType.Head, "Head" },
			{ bHaptics.PositionType.VestFront, "Vest Front" },
			{ bHaptics.PositionType.VestBack, "Vest Back" },
			{ bHaptics.PositionType.ForearmR, "Right Arm" },
			{ bHaptics.PositionType.ForearmL, "Left Arm" },
			{ bHaptics.PositionType.HandR, "Right Hand" },
			{ bHaptics.PositionType.HandL, "Left Hand" },
			{ bHaptics.PositionType.FootR, "Right Foot" },
			{ bHaptics.PositionType.FootL, "Left Foot" },
		};

		internal static Dictionary<string, bHaptics.PositionType> RenderTextureToPos = new Dictionary<string, bHaptics.PositionType>()
		{
			{ "tactal_head", bHaptics.PositionType.Head },
			{ "tactot_front", bHaptics.PositionType.VestFront },
			{ "tactot_back", bHaptics.PositionType.VestBack },
			{ "tactosy_arm_right", bHaptics.PositionType.ForearmR },
			{ "tactosy_arm_left", bHaptics.PositionType.ForearmL },
			{ "tactosy_hand_right", bHaptics.PositionType.HandR },
			{ "tactosy_hand_left", bHaptics.PositionType.HandL },
			{ "tactosy_foot_right", bHaptics.PositionType.FootR },
			{ "tactosy_foot_left", bHaptics.PositionType.FootL }
		};
	}
}