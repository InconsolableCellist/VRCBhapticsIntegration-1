using UnityEngine;

namespace VRCBhapticsIntegration
{
    internal static class OculusRiftTracking
    {
		internal static VRCTrackingRift GetTrackingManagerRift()
			=> VRCBhapticsIntegration.FindVRCTracking<VRCTrackingRift>();
	}
}
