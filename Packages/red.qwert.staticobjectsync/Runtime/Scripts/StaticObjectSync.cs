﻿using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Static Object Sync/Static Object Sync")]
    public class StaticObjectSync : UdonSharpBehaviour
    {
        [SerializeField] private StaticObjectContainerManager containerManager;
        [SerializeField] private bool syncContainer;

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
        private bool _disableNextSerialization;
        private int _requestSerializationCount = 0;

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

        private void UpdateTransformInfo()
        {
            UpdateCurrentContainerInfo();
            _globalPosition = transform.position;
            _globalRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
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
            UpdateTransformInfo();
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            if (syncContainer)
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
            else
            {
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

        private void LocallyRespawnToGlobal()
        {
            transform.position = _originalGlobalPosition;
            transform.rotation = _originalGlobalRotation;
            _hasBeenMoved = false;
        }

        public void GloballyRespawnToGlobal()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallyRespawnToGlobal();
            RequestSerialization();
        }

        private void LocallyRespawnToLocal()
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPosition;
            transform.localRotation = _originalLocalRotation;
            _hasBeenMoved = false;
        }

        public void GloballyRespawnToLocal()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallyRespawnToLocal();
            RequestSerialization();
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

        public void GloballySetParentContainer(StaticObjectContainer container)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            LocallySetParentContainer(container);
            RequestSerialization();
        }

        public void LocallySetParentContainer(StaticObjectContainer container)
        {
            transform.SetParent(
                Utilities.IsValid(container)
                    ? container.transform
                    : null
            );
        }

        public void DisableNextSerialization() => _disableNextSerialization = true;

        public new void RequestSerialization()
        {
            _requestSerializationCount++;
            SendCustomEventDelayedFrames(nameof(DelayedRequestSerialization), 0);
        }

        public void DelayedRequestSerialization()
        {
            if (_requestSerializationCount == 1)
            {
                if (!_disableNextSerialization)
                {
                    base.RequestSerialization();
                }

                _disableNextSerialization = false;
            }

            _requestSerializationCount = Math.Max(0, _requestSerializationCount - 1);
        }
    }
}