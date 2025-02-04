using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    public enum PickupHand
    {
        None,
        Right,
        Left,
    }

    public enum PickupInterpolationMode
    {
        None,
        SmoothDamp,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(StaticObjectSync))]
    [RequireComponent(typeof(VRCPickup))]
    [RequireComponent(typeof(ParentConstraint))]
    [AddComponentMenu("Static Object Sync/Pickup")]
    public class Pickup : UdonSharpBehaviour
    {
        [SerializeField] private PickupManager pickupManager;

        [SerializeField] private PickupInterpolationMode interpolationMode = PickupInterpolationMode.None;
        [SerializeField] private float positionSmoothTime = 0.1f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        private VRCPlayerApi _owner;
        private PickupGhost _pickupGhost;
        [UdonSynced] private PickupHand _pickupHand;

        [UdonSynced, FieldChangeCallback(nameof(OwnerId))]
        private int _ownerId;

        private int OwnerId
        {
            get => _ownerId;
            set
            {
                if (value == _ownerId)
                {
                    return;
                }

                if (Utilities.IsValid(_owner))
                {
                    _pickup.Drop(_owner);
                }

                _ownerId = value;
                _owner = VRCPlayerApi.GetPlayerById(_ownerId);
            }
        }

        public PickupInterpolationMode InterpolationMode => interpolationMode;
        public PickupHand PickupHand => _pickupHand;
        public float PositionSmoothTime => positionSmoothTime;
        public float RotationSmoothTime => rotationSmoothTime;
        private bool IsHeldGlobally => PickupHand != PickupHand.None;

        private VRCPickup _pickup;
        private ParentConstraint _constraint;

        private void Start()
        {
            _pickup = GetComponent<VRCPickup>();
            _constraint = GetComponent<ParentConstraint>();
            _constraint.enabled = false;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && IsHeldGlobally)
            {
                RequestSerialization();
            }
        }

        public override void OnDeserialization()
        {
            if (_pickupHand == PickupHand.None)
            {
                if (Utilities.IsValid(_pickupGhost))
                {
                    _pickupGhost.UnregisterFollower();
                    _pickupGhost = null;
                }

                Unfollow();
                _pickup.pickupable = true;
            }
            else
            {
                if (Utilities.IsValid(pickupManager))
                {
                    _pickupGhost = pickupManager.GetPickupGhostOf(Networking.GetOwner(gameObject), _pickupHand);

                    if (Utilities.IsValid(_pickupGhost))
                    {
                        _pickupGhost.RegisterFollower(this);
                    }
                }

                _pickup.pickupable = !_pickup.DisallowTheft;
            }
        }

        public override void OnPickup()
        {
            Unfollow();
            OwnerId = Networking.LocalPlayer.playerId;
            _pickupHand = CurrentPickupHand();
            RequestSerialization();

            if (Utilities.IsValid(pickupManager))
            {
                // _pickupHandの代入より後に処理
                pickupManager.OnLocalPickup(this);
            }
        }

        public override void OnDrop()
        {
            if (Utilities.IsValid(pickupManager))
            {
                // _pickupHandの代入より前に処理
                pickupManager.OnLocalDrop(this);
            }

            _pickupHand = PickupHand.None;
            RequestSerialization();
        }

        private PickupHand CurrentPickupHand() => ConvertPickupHand(
            Utilities.IsValid(_pickup) ? _pickup.currentHand : VRC_Pickup.PickupHand.None
        );

        private static PickupHand ConvertPickupHand(VRC_Pickup.PickupHand pickupHand)
        {
            if (pickupHand == VRC_Pickup.PickupHand.Left)
            {
                return PickupHand.Left;
            }

            if (pickupHand == VRC_Pickup.PickupHand.Right)
            {
                return PickupHand.Right;
            }

            return PickupHand.None;
        }

        public bool IsFollowing()
        {
            return _constraint.sourceCount > 0;
        }

        public void Follow(Transform target)
        {
            if (!Utilities.IsValid(target))
            {
                return;
            }

            var source = new ConstraintSource();
            source.sourceTransform = target;
            source.weight = 1.0f;
            _constraint.AddSource(source);
            _constraint.constraintActive = true;
            _constraint.enabled = true;
        }

        public void Unfollow()
        {
            for (var i = 0; i < _constraint.sourceCount; i++)
            {
                _constraint.RemoveSource(0);
            }

            _constraint.constraintActive = false;
            _constraint.enabled = false;
        }
    }
}