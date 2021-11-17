using System;
using UnityEngine;
using UnityEngine.Rendering;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

namespace VRCBhapticsIntegration
{
	internal class CameraParser : MonoBehaviour
	{
		private MelonPreferences_Entry<bool> Enabled_Entry;
		private MelonPreferences_Entry<int> Intensity;
		private Texture2D TempTexture;
		private Rect TempTextureRect;

		internal bHaptics.PositionType Position;
		internal Camera _camera;
		internal Color[] OldColors;

		internal byte[] Value = new byte[20];

		public CameraParser(IntPtr ptr) : base(ptr) { }

		[HideFromIl2Cpp]
		private bool IsEnabled
		{
			get => _camera.enabled;
			set => _camera.enabled = value;
		}

		[HideFromIl2Cpp]
		internal void SetupFromConfig()
		{
			Enabled_Entry = ModConfig.Entries_Enable[Position];
			Intensity = ModConfig.Entries_Intensity[Position];
			LateUpdate();
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

		[HideFromIl2Cpp]
		internal void ParsePixels(Color[] pixelcolors, int width, int height)
		{
			if ((pixelcolors == null)
				|| (pixelcolors.Length <= 0))
				return;

			if (OldColors == null)
				OldColors = pixelcolors;
			else
			{
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
						Value[colorpos] = (byte)((pixel != oldpixel) ? Intensity.Value : 0);
					}

				RearrangeValueBuffer();
				bHaptics.Submit($"vrchat_{Position}", Position, Value, 100);
			}
		}

		private void OnDisable()
			=> enabled = true;

		private void LateUpdate()
			=> IsEnabled =
				!bHaptics.WasError
				&& ModConfig.Allow_bHapticsPlayer_Communication.Value
				&& Enabled_Entry.Value;

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (!IsEnabled)
				return;

			Graphics.Blit(src, null as RenderTexture);

			int width = src.width;
			int height = src.height;

			// Credit to ImTiara and knah for this tip on using AsyncGPUReadback.Request
			if (ModConfig.Use_AsyncGPUReadback.Value && SystemInfo.supportsAsyncGPUReadback)
			{
				AsyncGPUReadback.Request(src, 0, src.graphicsFormat, new Action<AsyncGPUReadbackRequest>(req =>
				{
					IntPtr rawdata = req.GetDataRaw(0);
					if (rawdata == IntPtr.Zero)
						return;

					ParsePixels(RawDataToColorArray(rawdata, req.GetLayerDataSize()), width, height);
				}));
			}
			else
			{
				if (TempTexture == null)
					TempTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
				if (TempTextureRect == Rect.zero)
					TempTextureRect = new Rect(0, 0, width, height);

				RenderTexture oldRenderTexture = RenderTexture.active;
				RenderTexture.active = src;
				TempTexture.ReadPixels(TempTextureRect, 0, 0);
				TempTexture.Apply();
				RenderTexture.active = oldRenderTexture;

				ParsePixels(TempTexture.GetPixels(0, 0, width, height), width, height);
			}
		}

		// Credit to knah for helping with this Raw Data to Color Conversion
		private static unsafe Color[] RawDataToColorArray(IntPtr rawdata_ptr, int rawdata_length)
		{
			byte* rawdata = (byte*)rawdata_ptr;
			Color[] colors = new Color[rawdata_length / 4];
			for (int i = 0; i < rawdata_length; i += 4)
				colors[i / 4] = new Color(rawdata[i] / 255f, rawdata[i + 1] / 255f, rawdata[i + 2] / 255f, rawdata[i + 3] / 255f);
			return colors;
		}
	}
}