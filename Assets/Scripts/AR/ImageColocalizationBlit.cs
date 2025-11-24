using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace CAU4R.Tuho.AR
{
    public class ImageColocalizationBlit : MonoBehaviour
    {
        public UnityEvent<Texture2D> OnImageBlit;
        
        [SerializeField]
        private ARCameraManager _cameraManager;

        public void BlitImage()
        {
            if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage)) return;

            var conversionParams = new XRCpuImage.ConversionParams()
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            
            var size = cpuImage.GetConvertedDataSize(conversionParams);

            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            
            cpuImage.Convert(conversionParams, buffer);
            cpuImage.Dispose();
            
            Texture2D texture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false
                );
            
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            
            buffer.Dispose();
            
            OnImageBlit?.Invoke(texture);
        }
    }
}
