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

        [UdonSynced] private Vector3 _globalPosition;
        [UdonSynced] private Quaternion _globalRotation;
        [UdonSynced] private Vector3 _localPosition;
        [UdonSynced] private Quaternion _localRotation;

        [UdonSynced, FieldChangeCallback(nameof(ContainerId))] private string _containerId;
        private string ContainerId
        {
            get => _containerId;
            set
            {
                _containerId = value;
                _container = Utilities.IsValid(containerManager)
                    ? containerManager.Find(_containerId)
                    : null;
            }
        }

        private StaticObjectContainer _container;

        [UdonSynced] private bool _hasBeenMoved;
        public bool HasBeenMoved => _hasBeenMoved;

        private Transform _originalParent;
        private Vector3 _originalGlobalPosition;
        private Quaternion _originalGlobalRotation;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;
        private bool _requestSerialization;
        private bool _disableSerializationForCurrentFrame;

        private void Start()
        {
            _originalParent = transform.parent;
            _originalGlobalPosition = transform.position;
            _originalGlobalRotation = transform.rotation;
            _originalLocalPosition = transform.localPosition;
            _originalLocalRotation = transform.localRotation;
        }

        private void UpdateCurrentContainerInfo()
        {
            if (!Utilities.IsValid(transform.parent))
            {
                _container = null;
                _containerId = null;
                return;
            }

            _container = transform.parent.GetComponent<StaticObjectContainer>();
            _containerId = Utilities.IsValid(_container)
                ? _container.Id
                : null;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && _hasBeenMoved)
            {
                _requestSerialization = true;
            }
        }

        public override void OnPreSerialization()
        {
            UpdateCurrentContainerInfo();
            _globalPosition = transform.position;
            _globalRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            if (Utilities.IsValid(_container))
            {
                transform.SetParent(_container.transform);
                LocallyTeleportToLocal(_localPosition, _localRotation);
            }
            else
            {
                transform.SetParent(null);
                LocallyTeleportToGlobal(_globalPosition, _globalRotation);
            }
        }

        public override void OnPickup()
        {
            _hasBeenMoved = true;
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
            _requestSerialization = true;
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
            _requestSerialization = true;
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

        public void GloballySetParentContainer(StaticObjectContainer container)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallySetParentContainer(container);
            _requestSerialization = true;
        }

        public void LocallySetParentContainer(StaticObjectContainer container)
        {
            transform.SetParent(
                Utilities.IsValid(container)
                    ? container.transform
                    : null
            );
        }

        public void DisableSerializationForCurrentFrame() => _disableSerializationForCurrentFrame = true;

        public void LateUpdate()
        {
            if (_requestSerialization && !_disableSerializationForCurrentFrame)
            {
                RequestSerialization();
            }

            _requestSerialization = false;
            _disableSerializationForCurrentFrame = false;
        }
    }
}