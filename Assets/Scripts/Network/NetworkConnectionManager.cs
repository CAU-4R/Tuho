using Unity.Netcode;
using UnityEngine;

namespace CAU4R.Tuho.Network
{
    public class NetworkConnectionManager : MonoBehaviour
    {
        public void ConnectAsHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void ConnectAsClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
