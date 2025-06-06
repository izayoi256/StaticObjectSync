﻿using UdonSharp;
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
                staticObjectSyncs[i].DisableNextSerialization();
                staticObjectSyncs[i].GloballyTeleportToLocal(
                    staticObjectSyncs[i].transform.localPosition + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.localRotation
                );
                staticObjectSyncs[i].GloballyTeleportToLocal(
                    staticObjectSyncs[i].transform.localPosition + new Vector3(-0.1f, 0, 0),
                    staticObjectSyncs[i].transform.localRotation
                );
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
                    staticObjectSyncs[i].DisableNextSerialization();
                    staticObjectSyncs[i].GloballyRespawnToLocal();
                }
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

        public void LocallyRespawnToLocal()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                if (staticObjectSyncs[i].HasBeenMoved)
                {
                    staticObjectSyncs[i].LocallyRespawnToLocal();
                }
            }
        }

        public void Enable()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            for (var i = 0; i < staticObjectSyncs.Length; i++)
            {
                staticObjectSyncs[i].gameObject.SetActive(false);
            }
        }
    }
}