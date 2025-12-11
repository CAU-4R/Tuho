using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace CAU4R.Tuho.AR
{
    public class ImageBlitter : MonoBehaviour
    {
        [SerializeField]
        private ARCameraManager _cameraManager;
        
        [SerializeField]
        private UnityEvent<Texture2D> _onCapturedImage;

        private Texture2D _cameraTexture;

        public void CaptureImage()
        {
            if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage)) return;

            using (cpuImage)
            {
                var isPortrait = Screen.height > Screen.width;

                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                    outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                    outputFormat = TextureFormat.RGB24,
                    transformation = XRCpuImage.Transformation.MirrorY
                };

                var size = cpuImage.GetConvertedDataSize(conversionParams);
                var buffer = new NativeArray<byte>(size, Allocator.Temp);

                cpuImage.Convert(conversionParams, buffer);

                if (_cameraTexture == null ||
                    _cameraTexture.width != conversionParams.outputDimensions.x ||
                    _cameraTexture.height != conversionParams.outputDimensions.y)
                {
                    _cameraTexture = new Texture2D(
                        conversionParams.outputDimensions.x,
                        conversionParams.outputDimensions.y,
                        conversionParams.outputFormat,
                        false
                    );
                }

                _cameraTexture.LoadRawTextureData(buffer);
                _cameraTexture.Apply();

                buffer.Dispose();

                if (isPortrait)
                {
                    _cameraTexture = RotateTexture90(_cameraTexture);
                }
                
                _cameraTexture.name = "CapturedImage";

                _onCapturedImage?.Invoke(_cameraTexture);
            }
        }

        private Texture2D RotateTexture90(Texture2D tex)
        {
            var width = tex.width;
            var height = tex.height;
            var rotated = new Texture2D(height, width, tex.format, false);
            var pixels = tex.GetPixels();
            var rotatedPixels = new Color[pixels.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rotatedPixels[x * height + (height - y - 1)] = pixels[y * width + x];
                }
            }

            rotated.SetPixels(rotatedPixels);
            rotated.Apply();
            
            return rotated;
        }
    }
}
