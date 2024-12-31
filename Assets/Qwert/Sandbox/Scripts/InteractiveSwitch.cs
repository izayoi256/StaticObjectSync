using UdonSharp;
using UnityEngine;

namespace Qwert.Sandbox
{
    public class InteractiveSwitch : UdonSharpBehaviour
    {
        [SerializeField] private string callback;
        [SerializeField] private StaticObjectSync.StaticObjectSync[] staticObjectSyncs;

        private void Start()
        {
            InteractionText = callback;
        }

        public override void Interact()
        {
            SendCustomEvent(callback);
        }

        public void LocallyTeleportToGlobal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].LocallyTeleportToGlobal(
                    staticObjectSyncs[i].transform.position + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.rotation
                );
            }
        }

        public void GloballyTeleportToGlobal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].GloballyTeleportToGlobal(
                    staticObjectSyncs[i].transform.position + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.rotation
                );
            }
        }

        public void LocallyRespawn()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].LocallyRespawn();
            }
        }

        public void GloballyRespawn()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].GloballyRespawn();
            }
        }
    }
}