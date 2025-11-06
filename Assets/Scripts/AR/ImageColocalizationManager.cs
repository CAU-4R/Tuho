using Niantic.Lightship.SharedAR.Colocalization;
using UnityEngine;

namespace CAU4R.Tuho.AR
{
    public class ImageColocalizationManager : MonoBehaviour
    {
        [SerializeField]
        private SharedSpaceManager _sharedSpaceManager;
        
        [SerializeField]
        private Texture2D _targetImage;

        [SerializeField]
        private float _targetImageSize = 0.1f;

        [SerializeField]
        private string _roomName = "DemoRoom";

        [SerializeField]
        private int _roomCapacity = 10;
        
        [SerializeField]
        private string _roomDescription = "Demo Room Description";

        public void StartSharedSpace()
        {
            var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(_targetImage, _targetImageSize);
            var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(_roomName, _roomCapacity, _roomDescription);
            
            _sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomOptions);
        }
    }
}
