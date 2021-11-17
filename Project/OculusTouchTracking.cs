using UnityEngine;

namespace VRCBhapticsIntegration
{
    internal static class OculusTouchTracking
    {
		internal static VRCTrackingTouch GetTrackingManagerTouch()
			=> VRCBhapticsIntegration.FindVRCTracking<VRCTrackingTouch>();
	}
}
