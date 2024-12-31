using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Static Object Sync/Static Object Container")]
    public class StaticObjectContainer : UdonSharpBehaviour
    {
        [SerializeField] private StaticObjectContainerManager containerManager;

        public string Id { get; private set; }

        void Start()
        {
            Id = Networking.GetUniqueName(gameObject);
            containerManager.Register(this);
        }
    }
}