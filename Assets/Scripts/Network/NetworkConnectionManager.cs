using Unity.Netcode;
using UnityEngine;

namespace CAU4R.Tuho.Network
{
    public class NetworkConnectionManager : MonoBehaviour
    {
        private bool _isHost;
        
        public void PrepareAsHost()
        {
            _isHost = true;
        }

        public void PrepareAsClient()
        {
            _isHost = false;
        }

        public void Connect()
        {
            if (_isHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }
}
