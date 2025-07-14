using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    [RequireComponent(typeof(VRCPlayerObject))]
    [RequireComponent(typeof(VRCObjectSync))]
    [RequireComponent(typeof(ParentConstraint))]
    [RequireComponent(typeof(MeshRenderer))] // 空でもいいからMeshRendererを付けないと同期がカクつく
    [AddComponentMenu("Static Object Sync/Pickup Ghost")]
    public class PickupGhost : UdonSharpBehaviour
    {
        [SerializeField] private PickupManager pickupManager;
        [SerializeField] private PickupHand pickupHand;

        [UdonSynced] private bool _enabled;

        private ParentConstraint _constraint;
        private VRCObjectSync _objectSync;
        private Pickup _followerPickup;
        private InterpolatedPickupGhost _interpolatedPickupGhost;

        public bool IsLocal { get; private set; }
        public bool IsRightHand => pickupHand == PickupHand.Right;
        public bool IsLeftHand => pickupHand == PickupHand.Left;

        private void Start()
        {
            _constraint = GetComponentInParent<ParentConstraint>();
            _constraint.enabled = false;

            _objectSync = GetComponentInParent<VRCObjectSync>();

            var owner = Networking.GetOwner(gameObject);
            IsLocal = Utilities.IsValid(owner) && owner.isLocal;

            if (Utilities.IsValid(pickupManager))
            {
                pickupManager.RegisterPickupGhost(this);
            }
        }

        public override void OnDeserialization()
        {
            if (_enabled && Utilities.IsValid(_followerPickup))
            {
                if (_followerPickup.InterpolationMode == PickupInterpolationMode.None)
                {
                    _followerPickup.Follow(transform);
                }
                else if (_followerPickup.InterpolationMode == PickupInterpolationMode.SmoothDamp)
                {
                    if (Utilities.IsValid(_interpolatedPickupGhost))
                    {
                        _followerPickup.Follow(_interpolatedPickupGhost.transform);
                        _interpolatedPickupGhost.FollowPickupGhost(this, _followerPickup);
                    }
                }
            }

            if (!_enabled)
            {
                if (Utilities.IsValid(_interpolatedPickupGhost))
                {
                    _interpolatedPickupGhost.Unfollow();
                }
            }
        }

        public void SetInterpolatedPickupGhost(InterpolatedPickupGhost interpolatedPickupGhost) => _interpolatedPickupGhost = interpolatedPickupGhost;

        public void RegisterFollower(Pickup pickup)
        {
            _followerPickup = pickup;
        }

        public void UnregisterFollower()
        {
            _followerPickup = null;
        }

        public void OnLocalPickup(Pickup pickup)
        {
            if (!Utilities.IsValid(pickup))
            {
                return;
            }

            _enabled = true;
            _objectSync.TeleportTo(pickup.transform);

            var source = new ConstraintSource();
            source.sourceTransform = pickup.transform;
            source.weight = 1.0f;
            _constraint.AddSource(source);
            _constraint.constraintActive = true;
            _constraint.enabled = true;
        }

        public void OnLocalDrop(Pickup pickup)
        {
            _enabled = false;

            for (var i = 0; i < _constraint.sourceCount; i++)
            {
                _constraint.RemoveSource(0);
            }

            _constraint.constraintActive = false;
            _constraint.enabled = false;
        }
    }
}