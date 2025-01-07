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
        [UdonSynced] private Vector3 _globalPosition;
        [UdonSynced] private Quaternion _globalRotation;
        [UdonSynced] private Vector3 _localPosition;
        [UdonSynced] private Quaternion _localRotation;

        [UdonSynced] private bool _hasBeenMoved;

        public bool HasBeenMoved => _hasBeenMoved;

        private Transform _originalParent;
        private Vector3 _originalGlobalPosition;
        private Quaternion _originalGlobalRotation;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;

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
            _originalParent = transform.parent;
            _originalGlobalPosition = transform.position;
            _originalGlobalRotation = transform.rotation;
            _originalLocalPosition = transform.localPosition;
            _originalLocalRotation = transform.localRotation;
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

        public void LocallyRespawnToGlobal()
        {
            transform.position = _originalGlobalPosition;
            transform.rotation = _originalGlobalRotation;
            _hasBeenMoved = false;
        }

        public void GloballyRespawnToGlobal()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(LocallyRespawnToGlobal));
        }

        public void LocallyRespawnToLocal()
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPosition;
            transform.localRotation = _originalLocalRotation;
            _hasBeenMoved = false;
        }

        public void GloballyRespawnToLocal()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(LocallyRespawnToLocal));
        }

        public void GloballyTeleportToGlobal(Transform location) => GloballyTeleportToGlobal(
            location.position,
            location.rotation
        );

        public void GloballyTeleportToGlobal(Vector3 position) => GloballyTeleportToGlobal(
            position,
            transform.rotation
        );

        public void GloballyRotateToGlobal(Quaternion rotation) => GloballyTeleportToGlobal(
            transform.position,
            rotation
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

        public void LocallyTeleportToGlobal(Vector3 position) => LocallyTeleportToGlobal(
            position,
            transform.rotation
        );

        public void LocallyRotateToGlobal(Quaternion rotation) => LocallyTeleportToGlobal(
            transform.position,
            rotation
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

        public void GloballyTeleportToLocal(Vector3 position) => GloballyTeleportToLocal(
            position,
            transform.localRotation
        );

        public void GloballyRotateToLocal(Quaternion rotation) => GloballyTeleportToLocal(
            transform.localPosition,
            rotation
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

        public void LocallyTeleportToLocal(Vector3 position) => LocallyTeleportToLocal(
            position,
            transform.localRotation
        );

        public void LocallyRotateToLocal(Quaternion rotation) => LocallyTeleportToLocal(
            transform.localPosition,
            rotation
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