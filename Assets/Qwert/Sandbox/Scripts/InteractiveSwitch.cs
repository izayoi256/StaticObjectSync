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

        public void GloballyTeleportToLocal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].GloballyTeleportToLocal(
                    staticObjectSyncs[i].transform.localPosition + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.localRotation
                );
            }
        }

        public void GloballyTeleportToLocalDisableSerialization()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].DisableSerializationForCurrentFrame();
                staticObjectSyncs[i].GloballyTeleportToLocal(
                    staticObjectSyncs[i].transform.localPosition + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.localRotation
                );
            }
        }

        public void LocallyRespawnToGlobal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                if (staticObjectSyncs[i].HasBeenMoved)
                {
                    staticObjectSyncs[i].LocallyRespawnToGlobal();
                }
            }
        }

        public void GloballyRespawnToGlobal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                if (staticObjectSyncs[i].HasBeenMoved)
                {
                    staticObjectSyncs[i].GloballyRespawnToGlobal();
                }
            }
        }

        public void GloballyRespawnToLocal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                if (staticObjectSyncs[i].HasBeenMoved)
                {
                    staticObjectSyncs[i].GloballyRespawnToLocal();
                }
            }
        }

        public void GloballyRespawnToLocalDisableSerialization()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                if (staticObjectSyncs[i].HasBeenMoved)
                {
                    staticObjectSyncs[i].DisableSerializationForCurrentFrame();
                    staticObjectSyncs[i].GloballyRespawnToLocal();
                }
            }
        }
    }
}