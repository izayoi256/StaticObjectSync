using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.Sandbox
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UniqueNameInspector : UdonSharpBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;

        void Start()
        {
            textMesh.text = Networking.GetUniqueName(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }

            var self = other.GetComponent<UniqueNameInspector>();
            var staticObjectSync = other.GetComponent<StaticObjectSync.StaticObjectSync>();
            if (Utilities.IsValid(self) || !Utilities.IsValid(staticObjectSync))
            {
                return;
            }

            staticObjectSync.transform.SetParent(transform);
            staticObjectSync.GloballyTeleportToLocal(
                new Vector3(
                    (transform.childCount - 2) * 0.5f,
                    1f,
                    0
                ),
                Quaternion.identity
            );
        }
    }
}