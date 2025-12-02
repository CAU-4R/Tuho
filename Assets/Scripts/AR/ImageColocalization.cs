using Niantic.Lightship.SharedAR.Colocalization;
using UnityEngine;
using UnityEngine.Events;

namespace CAU4R.Tuho.AR
{
    public class ImageColocalization : MonoBehaviour
    {
        public UnityEvent OnTrackingBegin;
        public UnityEvent OnTrackingEnd;
        
        [SerializeField]
        private SharedSpaceManager _sharedSpaceManager;
        
        [SerializeField]
        private Texture2D _targetImage;

        [SerializeField]
        private float _targetImageSize = 0.5f;

        [SerializeField]
        private string _roomName = "DemoRoom";

        [SerializeField]
        private int _roomCapacity = 10;
        
        [SerializeField]
        private string _roomDescription = "Demo Room Description";

        private void Start()
        {
            _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
        }

        public void SetTargetImage(Texture2D image)
        {
            _targetImage = image;
        }

        public void SetTargetImageSize(float size)
        {
            _targetImageSize = size;
        }

        public void SetRoomName(string roomName)
        {
            _roomName = roomName;
        }

        public void SetRoomCapacity(int capacity)
        {
            _roomCapacity = capacity;
        }

        public void SetRoomDescription(string description)
        {
            _roomDescription = description;
        }
        
        public void StartSharedSpace()
        {
            var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(_targetImage, _targetImageSize);
            var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(_roomName, _roomCapacity, _roomDescription);
            
            _sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomOptions);
        }

        private void OnColocalizationTrackingStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
        {
            if (args.Tracking)
            {
                OnTrackingBegin?.Invoke();
            }
            else
            {
                OnTrackingEnd?.Invoke();
            }
        }
    }
}
