using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Static Object Sync/Static Object Sync")]
    public class StaticObjectSync : UdonSharpBehaviour
    {
        [UdonSynced] private bool _hasBeenMoved;
        [UdonSynced] private Vector3 _position;
        [UdonSynced] private Quaternion _rotation;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        [UdonSynced, FieldChangeCallback(nameof(Sync))]
        private bool _sync;

        private bool Sync
        {
            get => _sync;
            set
            {
                if (value)
                {
                    SendCustomEvent(nameof(TeleportToSyncedTransform));
                }

                _sync = false;
            }
        }

        private void Start()
        {
            _position = _initialPosition = transform.position;
            _rotation = _initialRotation = transform.rotation;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && _hasBeenMoved)
            {
                RequestSerialization();
            }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            _sync = false;
        }

        public override void OnPickup()
        {
            _hasBeenMoved = true;
            RequestSerialization();
        }

        public override void OnDrop()
        {
            TeleportToGlobally(transform);
        }

        public void Respawn()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _hasBeenMoved = false;
        }

        public void RespawnGlobally()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Respawn));
        }

        public void TeleportToGlobally(Transform location) => TeleportToGlobally(location.position, location.rotation);


        public void TeleportToGlobally(Vector3 position, Quaternion rotation)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            transform.position = _position = position;
            transform.rotation = _rotation = rotation;
            _hasBeenMoved = true;
            _sync = true;
            RequestSerialization();
        }

        public void TeleportTo(Transform location) => TeleportTo(location.position, location.rotation);

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            _hasBeenMoved = true;
        }

        public void TeleportToSyncedTransform() => TeleportTo(_position, _rotation);
    }
}