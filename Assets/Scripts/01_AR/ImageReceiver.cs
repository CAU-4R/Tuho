using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CAU4R.Tuho.AR
{
    public class ImageReceiver : NetworkBehaviour
    {
        private const string MessageName = "TextureChunk";

        [SerializeField]
        private UnityEvent<Texture2D> _onChunkReceived;
        
        private readonly List<byte> _receivedBytes = new();
        private int _expectedChunks = -1;
        private int _receivedCount;

        public override void OnNetworkSpawn()
        {
            if (!IsClient) return;

            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                MessageName,
                OnChunkReceived);
        }
        
        private void OnChunkReceived(ulong serverId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int total);
            reader.ReadValueSafe(out int length);

            var chunk = new byte[length];
            reader.ReadBytesSafe(ref chunk, length);

            _receivedBytes.AddRange(chunk);
            _receivedCount++;
            _expectedChunks = total;

            if (_receivedCount != _expectedChunks) return;

            var texture = new Texture2D(2, 2);
            texture.LoadImage(_receivedBytes.ToArray());

            _receivedBytes.Clear();
            _expectedChunks = -1;
            _receivedCount = 0;

            _onChunkReceived?.Invoke(texture);
        }
    }
}