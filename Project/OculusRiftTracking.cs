using System.Collections.Generic;
using UnityEngine;

namespace VRCBhapticsIntegration
{
	internal static class OculusRiftTracking
	{
		internal static VRCTrackingRift GetTrackingManager()
			=> VRCBhapticsIntegration.FindVRCTracking<VRCTrackingRift>();

		internal static GameObject[] GetTrackedObjects()
		{
			VRCTrackingRift tracking = GetTrackingManager();
			if (tracking == null)
				return null;

			List<GameObject> objects = new List<GameObject>();
			if (tracking.field_Public_GameObject_6 != null)
				objects.Add(tracking.field_Public_GameObject_6);
			if (tracking.field_Public_GameObject_7 != null)
				objects.Add(tracking.field_Public_GameObject_7);
			return objects.ToArray();
		}
	}
}
