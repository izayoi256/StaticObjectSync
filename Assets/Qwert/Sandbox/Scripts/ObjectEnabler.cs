using UdonSharp;
using UnityEngine;

namespace Qwert.Sandbox
{
    public class ObjectEnabler : UdonSharpBehaviour
    {
        [SerializeField] private GameObject[] objectsToEnable;
        [SerializeField] private float intervalSeconds;
        private int _j;

        void Start()
        {
            for (var i = 0; i < objectsToEnable.Length; i++)
            {
                SendCustomEventDelayedSeconds(nameof(Enable), (i + 1) * intervalSeconds);
            }
        }

        public void Enable()
        {
            objectsToEnable[_j++].SetActive(true);
        }
    }
}