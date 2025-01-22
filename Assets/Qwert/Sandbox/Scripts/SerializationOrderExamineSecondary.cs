using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.Sandbox
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SerializationOrderExamineSecondary : UdonSharpBehaviour
    {
        [SerializeField] private SerializationOrderExaminePrimary primary;

        [UdonSynced] public int _y;
        public int y => _y;

        public override void OnDeserialization()
        {
            if (Utilities.IsValid(primary))
            {
                if (primary.x == _y)
                {
                    Debug.Log($"Secondary: OK (x = {primary.x}, y = {_y}");
                }
                else
                {
                    Debug.LogError($"Secondary: Something went wrong! (x = {primary.x}, y = {_y}");
                }
            }
        }

        public void RequestSerializationSecondary()
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }

            _y++;
            RequestSerialization();
        }
    }
}