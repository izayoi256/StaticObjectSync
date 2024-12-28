using System;
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

        public void TeleportTo()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].TeleportTo(
                    staticObjectSyncs[i].transform.position + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.rotation
                );
            }
        }

        public void TeleportToGlobally()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].TeleportToGlobally(
                    staticObjectSyncs[i].transform.position + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.rotation
                );
            }
        }

        public void Respawn()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].Respawn();
            }
        }

        public void RespawnGlobally()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].RespawnGlobally();
            }
        }
    }
}