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
        private Vector3 _positionVelocity = Vector3.zero;
        private Vector3 _angularVelocity = Vector3.zero;
        private float _positionSmoothTime;
        private float _rotationSmoothTime;

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
            _positionSmoothTime = pickup.PositionSmoothTime;
            _rotationSmoothTime = pickup.RotationSmoothTime;
            _target = pickupGhost.transform;
        }

        public void Unfollow() => _target = null;

        private void Update()
        {
            if (Utilities.IsValid(_target))
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    _target.position,
                    ref _positionVelocity,
                    _positionSmoothTime
                );

                transform.rotation = _target.rotation;

                var targetEulerAngles = _target.rotation.eulerAngles;
                var currentEulerAngles = transform.eulerAngles;

                var smoothX = Mathf.SmoothDampAngle(
                    currentEulerAngles.x,
                    targetEulerAngles.x,
                    ref _angularVelocity.x,
                    _rotationSmoothTime
                );

                var smoothY = Mathf.SmoothDampAngle(
                    currentEulerAngles.y,
                    targetEulerAngles.y,
                    ref _angularVelocity.y,
                    _rotationSmoothTime
                );

                var smoothZ = Mathf.SmoothDampAngle(
                    currentEulerAngles.z,
                    targetEulerAngles.z,
                    ref _angularVelocity.z,
                    _rotationSmoothTime
                );

                transform.rotation = Quaternion.Euler(smoothX, smoothY, smoothZ);
            }
        }
    }
}