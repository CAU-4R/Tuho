using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CAU4R.Tuho.AR
{
    public class ImageSender : NetworkBehaviour
    {
        private const int ChunkSize = 32000;
        private const string MessageName = "TextureChunk";

        private Texture2D _texture;
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.CustomMessagingManager?.RegisterNamedMessageHandler(MessageName, (_, _) => { });
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SendTextureToClient;
        }
        
        public void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        private void SendTextureToClient(ulong clientId)
        {
            if (!IsHost) return;
            if (clientId == NetworkManager.Singleton.LocalClientId) return;

            var bytes = _texture.EncodeToPNG();
            var totalChunks = Mathf.CeilToInt(bytes.Length / (float)ChunkSize);

            for (var i = 0; i < totalChunks; i++)
            {
                var start = i * ChunkSize;
                var length = Mathf.Min(ChunkSize, bytes.Length - start);

                var chunk = new byte[length];
                Buffer.BlockCopy(bytes, start, chunk, 0, length);

                using var writer = new FastBufferWriter(ChunkSize + 100, Allocator.Temp);
                writer.WriteValueSafe(totalChunks);
                writer.WriteValueSafe(length);
                writer.WriteBytesSafe(chunk);

                NetworkManager.CustomMessagingManager.SendNamedMessage(
                    MessageName,
                    clientId,
                    writer,
                    NetworkDelivery.Reliable);
            }
        }
    }
}