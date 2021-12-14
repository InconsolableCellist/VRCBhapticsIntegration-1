using System.Collections.Generic;
using UnityEngine;

namespace VRCBhapticsIntegration
{
    internal static class OculusTouchTracking
    {
		internal static VRCTrackingTouch GetTrackingManager()
			=> VRCBhapticsIntegration.FindVRCTracking<VRCTrackingTouch>();

		internal static GameObject[] GetTrackedObjects()
		{
			VRCTrackingTouch tracking = GetTrackingManager();
			if (tracking == null)
				return null;

			List<GameObject> objects = new List<GameObject>();
			if (tracking.field_Public_GameObject_2 != null)
				objects.Add(tracking.field_Public_GameObject_2);
			if (tracking.field_Public_GameObject_3 != null)
				objects.Add(tracking.field_Public_GameObject_3);
			return objects.ToArray();
		}
	}
}
