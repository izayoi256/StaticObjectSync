using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRCPlayerObject))]
    [AddComponentMenu("Static Object Sync/Interpolated Pickup Ghost")]
    public class InterpolatedPickupGhost : UdonSharpBehaviour
    {
        [SerializeField] private PickupHand pickupHand;

        public bool IsRightHand => pickupHand == PickupHand.Right;
        public bool IsLeftHand => pickupHand == PickupHand.Left;

        private Transform _target;
        private Vector3 _velocity;
        private float _smoothTime;

        private void Start()
        {
            var owner = Networking.GetOwner(gameObject);
            var pickupGhost = FindPickupGhostOf(owner);
            if (Utilities.IsValid(pickupGhost))
            {
                pickupGhost.SetInterpolatedPickupGhost(this);
                _target = pickupGhost.transform;
            }
        }

        private PickupGhost FindPickupGhostOf(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player))
            {
                return null;
            }

            var playerObjects = Networking.GetPlayerObjects(player);
            for (var i = 0; i < playerObjects.Length; i++)
            {
                var playerObject = playerObjects[i];
                if (!Utilities.IsValid(playerObject))
                {
                    continue;
                }

                var pickupGhost = playerObject.GetComponent<PickupGhost>();
                if (!Utilities.IsValid(pickupGhost))
                {
                    continue;
                }

                if (IsLeftHand && pickupGhost.IsLeftHand || IsRightHand && pickupGhost.IsRightHand)
                {
                    return pickupGhost;
                }
            }

            return null;
        }

        public void FollowPickupGhost(PickupGhost pickupGhost, Pickup pickup)
        {
            transform.position = pickup.transform.position;
            transform.rotation = pickup.transform.rotation;
            _smoothTime = pickup.SmoothTime;
            _target = pickupGhost.transform;
        }

        private void Update()
        {
            if (Utilities.IsValid(_target))
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    _target.position,
                    ref _velocity,
                    _smoothTime
                );

                transform.rotation = _target.rotation;
            }
        }
    }
}