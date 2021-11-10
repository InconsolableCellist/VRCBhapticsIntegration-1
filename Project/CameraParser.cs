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
		private void RearrangeValueBuffer(ref byte[] Value)
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

		[HideFromIl2Cpp]
		private void ParsePixels(Color[] pixelcolors, int width, int height)
		{
			if ((pixelcolors == null)
				|| (pixelcolors.Length <= 0))
				return;

			if (OldColors == null)
				OldColors = pixelcolors;
			else
			{
				byte[] Value = new byte[20];

				for (int col = 0; col < height; col++)
					for (int row = 0; row < width; row++)
					{
						int bytepos = row * height + col;
						int colorpos = bytepos - 1;

						if (colorpos < 0)
							colorpos = 0;
						else if (colorpos >= 0)
							colorpos += 1;

						Color pixel = pixelcolors[colorpos];
						Color oldpixel = OldColors[colorpos];
						Value[colorpos] = (byte)((pixel != oldpixel) ? Intensity : 0);
					}

				RearrangeValueBuffer(ref Value);
				bHaptics.Submit($"vrchat_{Position}", Position, Value, 100);
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

		private Texture2D TempTexture;
		private Rect TempTextureRect = Rect.zero;
		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (!ShouldRun())
				return;
			
			Graphics.Blit(src, dest);

			if (SystemInfo.supportsAsyncGPUReadback)
			{
				// Credit to ImTiara for this tip on using AsyncGPUReadback.Request
				AsyncGPUReadback.Request(src, callback: new Action<AsyncGPUReadbackRequest>((AsyncGPUReadbackRequest req) =>
				{
					IntPtr rawdata = req.GetDataRaw(0);
					if (rawdata == IntPtr.Zero)
						return;

					ParsePixels(VRCBhapticsIntegration.RawDataToColorArray(rawdata, req.GetLayerDataSize()), req.width, req.height);
				}));
			}
			else
			{
				int width = dest.width;
				int height = dest.height;

				if (TempTexture == null)
					TempTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
				if (TempTextureRect == Rect.zero)
					TempTextureRect = new Rect(0, 0, width, height);

				TempTexture.ReadPixels(TempTextureRect, 0, 0);
				TempTexture.Apply();

				ParsePixels(TempTexture.GetPixels(0, 0, width, height), width, height);
			}
		}
	}
}