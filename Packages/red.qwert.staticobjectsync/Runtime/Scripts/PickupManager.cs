using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Static Object Sync/Pickup Manager")]
    public class PickupManager : UdonSharpBehaviour
    {
        private PickupGhost _localRightPickupGhost;
        private PickupGhost _localLeftPickupGhost;

        public void RegisterPickupGhost(PickupGhost pickupGhost)
        {
            if (!Utilities.IsValid(pickupGhost))
            {
                return;
            }

            if (pickupGhost.IsLocal)
            {
                if (pickupGhost.IsRightHand)
                {
                    _localRightPickupGhost = pickupGhost;
                }
                else if (pickupGhost.IsLeftHand)
                {
                    _localLeftPickupGhost = pickupGhost;
                }
            }
        }

        public void OnLocalPickup(Pickup pickup)
        {
            if (!Utilities.IsValid(pickup))
            {
                return;
            }

            var pickupGhost = LocalPickupGhostOf(pickup.PickupHand);

            if (!Utilities.IsValid(pickupGhost))
            {
                return;
            }

            pickupGhost.OnLocalPickup(pickup);
        }

        public void OnLocalDrop(Pickup pickup)
        {
            if (!Utilities.IsValid(pickup))
            {
                return;
            }

            var pickupGhost = LocalPickupGhostOf(pickup.PickupHand);

            if (!Utilities.IsValid(pickupGhost))
            {
                return;
            }

            pickupGhost.OnLocalDrop(pickup);
        }

        private PickupGhost LocalPickupGhostOf(PickupHand pickupHand)
        {
            if (pickupHand == PickupHand.Right)
            {
                return _localRightPickupGhost;
            }

            if (pickupHand == PickupHand.Left)
            {
                return _localLeftPickupGhost;
            }

            return null;
        }

        public PickupGhost GetPickupGhostOf(VRCPlayerApi player, PickupHand pickupHand)
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

                var pickupFollower = playerObject.GetComponent<PickupGhost>();
                if (!Utilities.IsValid(pickupFollower))
                {
                    continue;
                }

                if (
                    (pickupHand == PickupHand.Left && pickupFollower.IsLeftHand) ||
                    (pickupHand == PickupHand.Right && pickupFollower.IsRightHand)
                )
                {
                    return pickupFollower;
                }
            }

            return null;
        }
    }
}
