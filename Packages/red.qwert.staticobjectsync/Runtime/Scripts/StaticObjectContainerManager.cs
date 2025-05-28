using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Qwert.StaticObjectSync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Static Object Sync/Static Object Container Manager")]
    public class StaticObjectContainerManager : UdonSharpBehaviour
    {
        private StaticObjectContainer[] _staticObjectContainers;

        public void Register(StaticObjectContainer container)
        {
            if (container == null)
            {
                return;
            }

            if (!Utilities.IsValid(_staticObjectContainers))
            {
                _staticObjectContainers = new StaticObjectContainer[] { };
            }

            var newArray = new StaticObjectContainer[_staticObjectContainers.Length + 1];
            Array.Copy(_staticObjectContainers, newArray, _staticObjectContainers.Length);
            newArray[newArray.Length - 1] = container;
            _staticObjectContainers = newArray;
        }

        public StaticObjectContainer Find(string containerId)
        {
            if (!Utilities.IsValid(containerId) || !Utilities.IsValid(_staticObjectContainers))
            {
                return null;
            }

            for (var i = 0; i < _staticObjectContainers.Length; i++)
            {
                var container = _staticObjectContainers[i];
                if (container.Id == containerId)
                {
                    return container;
                }
            }

            return null;
        }
    }
}