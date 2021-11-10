using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using UnhollowerRuntimeLib;
using VRChatUtilityKit.Utilities;

namespace VRCBhapticsIntegration
{
    internal static class BuildInfo
    {
        public const string Name = "VRCBhapticsIntegration";
        public const string Author = "Herp Derpinstine, benaclejames, BenjaminZehowlt, knah, and ImTiara";
        public const string Company = "Lava Gang";
        public const string Version = "1.0.6";
        public const string DownloadLink = "https://github.com/HerpDerpinstine/VRCBhapticsIntegration";
    }

    public class VRCBhapticsIntegration : MelonMod
	{
		private static CameraParser[] CameraParsers;
		private static bool HasSearched = false;
		internal static GameObject[] ObjectsToCull;
		internal static int LayerForCulling;
		private static string CurrentAvatarID = null;
		private static float[] CullingDistances = new float[30];

		public override void OnApplicationStart()
        {
			if (bHaptics.WasError)
				return;

			for (int i = 0; i < CullingDistances.Length; i++)
				CullingDistances[i] = float.MaxValue;

			LayerForCulling = LayerMask.NameToLayer("MirrorReflection");
			CameraParsers = new CameraParser[ModConfig.PositionArr.Length];

			ModConfig.Initialize();
			ClassInjector.RegisterTypeInIl2Cpp<CameraParser>();
			NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;
		}

		public override void OnPreferencesLoaded() => OnPreferencesSaved();
        public override void OnPreferencesSaved()
		{
			if (bHaptics.WasError)
				return;

			int arr_size = CameraParsers.Length;
			if (arr_size <= 0)
				return;

			for (int i = 0; i < arr_size; i++)
			{
				if (CameraParsers[i] == null)
					continue;
				CameraParsers[i].SetupFromConfig(i);
			}
		}

		internal static void OnCameraParserSetup(CameraParser cameraParser)
        {
			if (!HasSearched)
			{
				HasSearched = true;

				GameObject[] gameObjects = GetSteamVRTrackedObjects();
				if (gameObjects == null)
					return;

				int arr_size = gameObjects.Length;
				if (arr_size <= 0)
					return;

				List<GameObject> objects = new List<GameObject>();
				for (int i = 0; i < arr_size; i++)
				{
					GameObject gameObject = gameObjects[i];
					if (gameObject == null)
						continue;

					Transform modelTransform = gameObject.transform.FindChild("Model");
					if (modelTransform == null)
						continue;

					MelonDebug.Msg($"Adding Model of {gameObject.name} to Culling List");
					objects.Add(modelTransform.gameObject);
				}

				ObjectsToCull = objects.ToArray();
			}

			if (ObjectsToCull != null)
				cameraParser.OldLayers = new int[ObjectsToCull.Length];
		}

		private static Camera[] TempCameraArray;
		private static void OnAvatarInstantiated(VRCAvatarManager avatar_manager, VRC.Core.ApiAvatar api_avatar, GameObject gameobject)
        {
			if (bHaptics.WasError)
				return;

			if ((avatar_manager == null)
				|| (gameobject == null))
				return;

			VRCPlayer vrcPlayer = VRCPlayer.field_Internal_Static_VRCPlayer_0;
			if (vrcPlayer == null)
				return;

			VRCAvatarManager vrcAvatarManager = vrcPlayer.prop_VRCAvatarManager_0;
			if ((vrcAvatarManager == null) || (vrcAvatarManager != avatar_manager))
				return;

			CurrentAvatarID = api_avatar.id;

			for (int i = 0; i < CameraParsers.Length; i++)
				CameraParsers[i] = null;

			Camera[] foundCameras = gameobject.GetComponentsInChildren<Camera>(true);
			if (foundCameras.Length <= 0)
				return;

			if (TempCameraArray == null)
				TempCameraArray = new Camera[CameraParsers.Length];
			else
				for (int i = 0; i < TempCameraArray.Length; i++)
					TempCameraArray[i] = null;

			foreach (Camera cam in foundCameras)
			{
				RenderTexture tex = cam.targetTexture;
				if (tex == null)
					continue;

				for (int i = 0; i < ModConfig.RenderTextureNames.Length; i++)
                {
					if (!tex.name.Equals(ModConfig.RenderTextureNames[i]))
						continue;

					TempCameraArray[i] = cam;
					break;
                }
			}

			for (int i = 0; i < TempCameraArray.Length; i++)
            {
				if (TempCameraArray[i] == null)
					continue;

				CameraParsers[i] = TempCameraArray[i].gameObject.AddComponent(Il2CppType.Of<CameraParser>()).TryCast<CameraParser>();
				
				CameraParsers[i]._camera = TempCameraArray[i];
				CameraParsers[i]._camera.useOcclusionCulling = true;
				CameraParsers[i]._camera.cullingMask &= ~(1 << LayerForCulling);
				CameraParsers[i]._camera.SetLayerCullDistances(CullingDistances);

				CameraParsers[i].SetupFromConfig(i);

				MelonLogger.Msg(ModConfig.ProperNames[i] + " Linked!");
			}
		}

		// Credit to knah for this simple method to grab VRCTrackingSteam
		private static VRCTrackingSteam vrcTrackingSteam;
		internal static VRCTrackingSteam GetVRCTrackingSteam()
		{
			if (vrcTrackingSteam != null)
				return vrcTrackingSteam;

			foreach (VRCTracking vrcTracking in VRCTrackingManager.field_Private_Static_VRCTrackingManager_0.field_Private_List_1_VRCTracking_0)
			{
				vrcTrackingSteam = vrcTracking.TryCast<VRCTrackingSteam>();
				if (vrcTrackingSteam != null)
					break;
			}

			return vrcTrackingSteam;
		}

		private static SteamVR_ControllerManager steamVR_ControllerManager;
		internal static SteamVR_ControllerManager GetSteamVRControllerManager()
		{
			if (steamVR_ControllerManager != null)
				return steamVR_ControllerManager;

			VRCTrackingSteam tracking = GetVRCTrackingSteam();
			if (tracking == null)
				return null;

			return steamVR_ControllerManager = tracking.field_Private_SteamVR_ControllerManager_0;
		}

		internal static GameObject[] GetSteamVRTrackedObjects()
        {
			SteamVR_ControllerManager controllerManager = GetSteamVRControllerManager();
			if (controllerManager == null)
				return null;

			return controllerManager.field_Public_ArrayOf_GameObject_0;
		}

		// Credit to BenjaminZehowlt for helping with this FBT Calibration Detection
		internal static bool IsInFBTCalibration()
        {
			VRCTrackingSteam tracking = GetVRCTrackingSteam();
			if (tracking == null)
				return false;
			return tracking.Method_Public_Virtual_Boolean_String_0(CurrentAvatarID);
		}

		// Credit to knah for helping with this Raw Data to Color Conversion
		internal static unsafe Color[] RawDataToColorArray(IntPtr rawdata_ptr, int rawdata_length)
		{
			byte* rawdata = (byte*)rawdata_ptr;
			Color[] colors = new Color[rawdata_length / 4];
			for (int i = 0; i < rawdata_length; i += 4)
				colors[i / 4] = new Color(rawdata[i] / 255f, rawdata[i + 1] / 255f, rawdata[i + 2] / 255f, rawdata[i + 3] / 255f);
			return colors;
		}
	}
}