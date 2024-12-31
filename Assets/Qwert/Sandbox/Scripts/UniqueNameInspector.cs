using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UniqueNameInspector : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh.text = Networking.GetUniqueName(gameObject);
    }
}
