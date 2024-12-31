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
        [SerializeField] private StaticObjectContainerManager containerManager;

        [UdonSynced] private string _containerId;
        [UdonSynced] private bool _hasBeenMoved;
        [UdonSynced] private Vector3 _globalPosition;
        [UdonSynced] private Quaternion _globalRotation;
        [UdonSynced] private Vector3 _localPosition;
        [UdonSynced] private Quaternion _localRotation;

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
                    SendCustomEvent(nameof(OnSync));
                }

                _sync = false;
            }
        }

        private void Start()
        {
            _containerId = GetCurrentContainerId();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        private string GetCurrentContainerId()
        {
            if (!Utilities.IsValid(transform.parent))
            {
                return null;
            }

            var container = transform.parent.GetComponent<StaticObjectContainer>();
            return Utilities.IsValid(container)
                ? container.Id
                : null;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && _hasBeenMoved)
            {
                RequestSerialization();
            }
        }

        public override void OnPreSerialization()
        {
            _containerId = GetCurrentContainerId();
            _globalPosition = transform.position;
            _globalRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
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
            GloballyTeleportToGlobal(transform);
        }

        public void LocallyRespawn()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _hasBeenMoved = false;
        }

        public void GloballyRespawn()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(LocallyRespawn));
        }

        public void GloballyTeleportToGlobal(Transform location) => GloballyTeleportToGlobal(
            location.position,
            location.rotation
        );


        public void GloballyTeleportToGlobal(Vector3 position, Quaternion rotation)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallyTeleportToGlobal(position, rotation);
            _sync = true;
            RequestSerialization();
        }

        public void LocallyTeleportToGlobal(Transform location) => LocallyTeleportToGlobal(
            location.position,
            location.rotation
        );

        public void LocallyTeleportToGlobal(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            _hasBeenMoved = true;
        }

        public void GloballyTeleportToLocal(Transform location) => GloballyTeleportToLocal(
            location.position,
            location.rotation
        );

        public void GloballyTeleportToLocal(Vector3 position, Quaternion rotation)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallyTeleportToLocal(position, rotation);
            _sync = true;
            RequestSerialization();
        }

        public void LocallyTeleportToLocal(Transform location) => LocallyTeleportToLocal(
            location.position,
            location.rotation
        );

        public void LocallyTeleportToLocal(Vector3 position, Quaternion rotation)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            _hasBeenMoved = true;
        }

        public void OnSync()
        {
            if (!Utilities.IsValid(_containerId) || !Utilities.IsValid(containerManager))
            {
                LocallyTeleportToGlobal(_globalPosition, _globalRotation);
                return;
            }

            var container = containerManager.Find(_containerId);
            if (!Utilities.IsValid(container))
            {
                LocallyTeleportToGlobal(_globalPosition, _globalRotation);
                return;
            }

            transform.SetParent(container.transform);
            LocallyTeleportToLocal(_localPosition, _localRotation);
        }
    }
}