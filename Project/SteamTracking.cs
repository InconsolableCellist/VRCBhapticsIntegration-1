using UnityEngine;

namespace VRCBhapticsIntegration
{
    internal static class SteamTracking
	{
		internal static VRCTrackingSteam GetTrackingManager()
			=> VRCBhapticsIntegration.FindVRCTracking<VRCTrackingSteam>();

		internal static SteamVR_ControllerManager GetControllerManager()
		{
			VRCTrackingSteam tracking = GetTrackingManager();
			if (tracking == null)
				return null;

			return tracking.field_Private_SteamVR_ControllerManager_0;
		}

		internal static GameObject[] GetTrackedObjects()
		{
			SteamVR_ControllerManager controllerManager = GetControllerManager();
			if (controllerManager == null)
				return null;

			return controllerManager.field_Public_ArrayOf_GameObject_0;
		}
	}
}
