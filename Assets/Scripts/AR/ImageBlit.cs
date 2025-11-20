using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

namespace CAU4R.Tuho.AR
{
    public class ImageBlit : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        [SerializeField]
        private RenderTexture _renderTexture;
        
        [SerializeField]
        private ARCameraBackground _cameraBackground;

        public void Blit()
        {
            var commandBuffer = new CommandBuffer();
            
            var texture = _cameraBackground.material.HasProperty(MainTex) ? _cameraBackground.material.GetTexture(MainTex) : null;
            
            var colorBuffer = Graphics.activeColorBuffer;
            var depthBuffer = Graphics.activeDepthBuffer;
            
            Graphics.SetRenderTarget(_renderTexture);
            
            commandBuffer.ClearRenderTarget(true, false, Color.clear);
            
            commandBuffer.Blit(
                texture,
                BuiltinRenderTextureType.CurrentActive,
                _cameraBackground.material);
            
            Graphics.ExecuteCommandBuffer(commandBuffer);
            
            Graphics.SetRenderTarget(colorBuffer, depthBuffer);
            
        }

        private void UpdateRenderTextureSize()
        {
            var currentWidth = _renderTexture.width;
            var currentHeight = _renderTexture.height;
            
            var newWidth = Screen.width;
            var newHeight = Screen.height;
        }

        private void CopyRenderTextureTo2DTexture()
        {
            
        }
    }
}