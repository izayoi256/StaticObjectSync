using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.Sandbox
{
    public class SerializationExample : UdonSharpBehaviour
    {
        [UdonSynced] private int _x;
        [UdonSynced] private int _y;

        public override void Interact()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _x++;
            _y++;
            RequestSerialization();
            _x++;
            _y++;
            RequestSerialization();
            _x++;
            _y++;
            RequestSerialization();
            _x++;
            _y++;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }
        }

        public override void OnPreSerialization()
        {
            Debug.Log($"_x = {_x}, _y = {_y}");
        }

        public override void OnDeserialization()
        {
            Debug.Log($"_x = {_x}, _y = {_y}");
        }

        /**
         * _x = 4, _y = 0 と
         * _x = 0, _y = 0 の
         * 両方のパターンが見られる。同期処理はUpdate内で行なわれ、Updateと同期の順は保証されていないもよう。
         */

        private void Update()
        {
            _x = 0;
        }

        private void LateUpdate()
        {
            _y = 0;
        }
    }
}