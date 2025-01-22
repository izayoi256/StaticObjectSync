using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.Sandbox
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SerializationOrderExaminePrimary : UdonSharpBehaviour
    {
        [SerializeField] private SerializationOrderExamineSecondary secondary;
        [SerializeField] private float serializationIntervalInSeconds = 0.5f;
        [SerializeField] private int serializationDelayInFrames = 1;

        [UdonSynced] private int _x;
        public int x => _x;

        private void Start()
        {
            SendCustomEventDelayedFrames(nameof(RequestSerializationPrimary), 1);
        }

        public override void OnDeserialization()
        {
            if (Utilities.IsValid(secondary))
            {
                if (_x - secondary._y == 1)
                {
                    Debug.Log($"Primary: OK (x = {_x}, y = {secondary._y}");
                }
                else
                {
                    Debug.LogError($"Primary: Something went wrong! (x = {_x}, y = {secondary._y}");
                }
            }
        }

        public void RequestSerializationPrimary()
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }

            _x++;
            RequestSerialization();
            SendCustomEventDelayedSeconds(nameof(RequestSerializationPrimary), serializationIntervalInSeconds);

            if (Utilities.IsValid(secondary))
            {
                // ↓同期オブジェクト数と回数が多いとSomething went wrongが発生することがある
                secondary.SendCustomEventDelayedFrames(nameof(SerializationOrderExamineSecondary.RequestSerializationSecondary), serializationDelayInFrames);

                // ↓Something went wrongが発生する確率が明らかに上がる
                // secondary.RequestSerializationSecondary();
            }
        }
    }
}