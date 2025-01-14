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
        [SerializeField] private bool temp;

        private ParentConstraint _constraint;
        private VRCObjectSync _objectSync;

        public bool IsLocal { get; private set; }
        public bool IsRightHand => pickupHand == PickupHand.Right;
        public bool IsLeftHand => pickupHand == PickupHand.Left;

        private void Start()
        {
            _constraint = GetComponentInParent<ParentConstraint>();
            _objectSync = GetComponentInParent<VRCObjectSync>();

            var owner = Networking.GetOwner(gameObject);
            IsLocal = Utilities.IsValid(owner) && owner.isLocal;

            if (Utilities.IsValid(pickupManager))
            {
                pickupManager.RegisterPickupGhost(this);
            }
        }

        public void OnLocalPickup(Pickup pickup)
        {
            if (!Utilities.IsValid(pickup))
            {
                return;
            }

            _objectSync.TeleportTo(pickup.transform);

            var source = new ConstraintSource();
            source.sourceTransform = pickup.transform;
            source.weight = 1.0f;
            _constraint.AddSource(source);
            _constraint.constraintActive = true;
        }

        public void OnLocalDrop(Pickup pickup)
        {
            for (var i = 0; i < _constraint.sourceCount; i++)
            {
                _constraint.RemoveSource(0);
            }

            _constraint.constraintActive = false;
        }
    }
}
