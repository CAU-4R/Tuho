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

        private Texture2D _cameraTexture;
        private bool _textureReady = false;

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

            if (currentWidth != newWidth || currentHeight != newHeight)
            {
                if (_renderTexture != null)
                {
                    _renderTexture.Release();
                }
                _renderTexture.width = newWidth;
                _renderTexture.height = newHeight;
                _renderTexture.depth = 24;

                _renderTexture.Create();
            }
        }

        private void CopyRenderTextureTo2DTexture()
        {
            if (_cameraTexture == null || _cameraTexture.width != _renderTexture.width ||
                _cameraTexture.height != _renderTexture.height)
            {
                _cameraTexture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false);
            }
            
            RenderTexture.active = _renderTexture;
            
            _cameraTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
            _cameraTexture.name = "SCAN_IMG";
            _cameraTexture.Apply();
            
            RenderTexture.active = null;
        }
    }
}