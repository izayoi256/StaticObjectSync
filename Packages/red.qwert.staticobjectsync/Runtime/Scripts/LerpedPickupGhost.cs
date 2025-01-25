using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRCPlayerObject))]
    [AddComponentMenu("Static Object Sync/Lerped Pickup Ghost")]
    public class LerpedPickupGhost : UdonSharpBehaviour
    {
        [SerializeField] private PickupHand pickupHand;

        public bool IsRightHand => pickupHand == PickupHand.Right;
        public bool IsLeftHand => pickupHand == PickupHand.Left;

        private Transform _target;
        private float _stabilizationReduceAngle;
        private float _stabilizationEndAngle;
        private float _stabilizationReducePosition;
        private float _stabilizationEndPosition;

        private void Start()
        {
            var owner = Networking.GetOwner(gameObject);
            var pickupGhost = FindPickupGhostOf(owner);
            if (Utilities.IsValid(pickupGhost))
            {
                pickupGhost.SetLerpedPickupGhost(this);
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

        public void SetStabilizationValues(
            float stabilizationReduceAngle,
            float stabilizationEndAngle,
            float stabilizationReducePosition,
            float stabilizationEndPosition
        )
        {
            _stabilizationReduceAngle = stabilizationReduceAngle;
            _stabilizationEndAngle = stabilizationEndAngle;
            _stabilizationReducePosition = stabilizationReducePosition;
            _stabilizationEndPosition = stabilizationEndPosition;
        }

        private void Update()
        {
            if (Utilities.IsValid(_target))
            {
                if (_stabilizationEndPosition != 0f)
                {
                    transform.position = Vector3.Lerp(
                        transform.position,
                        _target.position,
                        Mathf.Clamp(
                            Vector3.Distance(transform.position, _target.position),
                            _stabilizationReducePosition,
                            _stabilizationEndPosition
                        ) / _stabilizationEndPosition
                    );
                }

                if (_stabilizationEndAngle != 0f)
                {
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        _target.rotation,
                        Mathf.Clamp(
                            Quaternion.Angle(transform.rotation, _target.rotation),
                            _stabilizationReduceAngle,
                            _stabilizationEndAngle
                        ) / _stabilizationEndAngle
                    );
                }
            }
        }
    }
}