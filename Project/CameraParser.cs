using System;
using UnityEngine;
using UnityEngine.Rendering;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using VRChatUtilityKit.Utilities;

namespace VRCBhapticsIntegration
{
	internal class CameraParser : MonoBehaviour
	{
		private bHaptics.PositionType Position = ModConfig.PositionArr[0];
		private int Intensity = ModConfig.DefaultIntensity;
		internal int[] OldLayers;
		internal Camera _camera = null;
		private byte[] Value = new byte[20];
		private Color[] OldColors;

		public CameraParser(IntPtr ptr) : base(ptr) { }

		[HideFromIl2Cpp]
		private bool Enabled
		{
			get => enabled;
			set => _camera.enabled = enabled = value;
		}

		[HideFromIl2Cpp]
		internal void SetupFromConfig(int index)
        {
			Enabled = ModConfig.Allow_bHapticsPlayer_Communication.Value && ModConfig.Entries_Enable[index].Value;
			Position = ModConfig.PositionArr[index];
			Intensity = ModConfig.Entries_Intensity[index].Value;
			OldColors = null;
			VRCBhapticsIntegration.OnCameraParserSetup(this);
		}

		[HideFromIl2Cpp]
		private bool ShouldRun()
        {
			if (bHaptics.WasError
				|| !Enabled
				|| VRCBhapticsIntegration.IsInFBTCalibration())
				return false;
			return true;
		}

		[HideFromIl2Cpp]
		private void RearrangeValueBuffer()
		{
			Array.Reverse(Value, 0, Value.Length);
			switch (Position)
			{
				case bHaptics.PositionType.VestFront:
					Array.Reverse(Value, 0, 4);
					Array.Reverse(Value, 4, 4);
					Array.Reverse(Value, 8, 4);
					Array.Reverse(Value, 12, 4);
					Array.Reverse(Value, 16, 4);
					break;

				case bHaptics.PositionType.Head:
					Array.Reverse(Value, 0, 6);
					break;

				case bHaptics.PositionType.FootR:
					Array.Reverse(Value, 0, 3);
					break;
			}
		}

		private void OnPreCull()
        {
			if (!ShouldRun())
				return;

			if ((VRCBhapticsIntegration.ObjectsToCull == null) || (VRCBhapticsIntegration.ObjectsToCull.Length <= 0))
				return;

			for (int i = 0; i < VRCBhapticsIntegration.ObjectsToCull.Length; i++)
			{
				GameObject object_to_cull = VRCBhapticsIntegration.ObjectsToCull[i];
				if (object_to_cull == null)
					continue;

				OldLayers[i] = gameObject.layer;
				object_to_cull.SetLayerRecursive(VRCBhapticsIntegration.LayerForCulling);
			}
        }

        private void OnPostCull()
        {
            if (!ShouldRun())
                return;

            if ((VRCBhapticsIntegration.ObjectsToCull == null) || (VRCBhapticsIntegration.ObjectsToCull.Length <= 0))
                return;

            for (int i = 0; i < VRCBhapticsIntegration.ObjectsToCull.Length; i++)
            {
                GameObject object_to_cull = VRCBhapticsIntegration.ObjectsToCull[i];
                if (object_to_cull == null)
                    continue;

				object_to_cull.SetLayerRecursive(OldLayers[i]);
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (!ShouldRun())
				return;

			// Credit to ImTiara for this tip on using AsyncGPUReadback.Request
			AsyncGPUReadback.Request(src, callback: new Action<AsyncGPUReadbackRequest>(ParseRequest));
		}

		[HideFromIl2Cpp]
		private void ParseRequest(AsyncGPUReadbackRequest req)
        {
			IntPtr rawdata = req.GetDataRaw(0);
			if (rawdata == IntPtr.Zero)
				return;

			Color[] pixelcolors = VRCBhapticsIntegration.RawDataToColorArray(rawdata, req.GetLayerDataSize());
			if (pixelcolors.Length <= 0)
				return;

			if (OldColors == null)
				OldColors = pixelcolors;
			else
			{
				for (int col = 0; col < req.height; col++)
					for (int row = 0; row < req.width; row++)
					{
						int bytepos = row * req.height + col;
						int colorpos = bytepos - 1;

						if (colorpos < 0)
							colorpos = 0;
						else if (colorpos >= 0)
							colorpos += 1;

						Color pixel = pixelcolors[colorpos];
						Color oldpixel = OldColors[colorpos];
						if (pixel != oldpixel)
							Value[colorpos] = (byte)Intensity;
						else
							Value[colorpos] = 0;
					}

				RearrangeValueBuffer();
				bHaptics.Submit($"vrchat_{Position}", Position, Value, 100);
			}
		}
	}
}