#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Qwert.StaticObjectSync.Editor
{
    [InitializeOnLoad]
    public class DependencyInjector
    {
        static DependencyInjector()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                InjectIntoPickups();
                InjectIntoStaticObjectSyncs();
                InjectIntoStaticObjectContainers();
            }
        }

        private static void InjectIntoPickups()
        {
            var objects = Object.FindObjectsByType(typeof(Pickup), FindObjectsSortMode.None);

            if (objects.Length == 0)
            {
                return;
            }

            var pickupManager = Object.FindObjectOfType(typeof(PickupManager));
            if (pickupManager == null)
            {
                Debug.LogWarning("Pickup manager not found in the scene");
                return;
            }

            foreach (var obj in objects)
            {
                var type = obj.GetType();
                var fields = type.GetFields(
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(SerializeField), true))
                    {
                        continue;
                    }

                    if (field.FieldType == typeof(PickupManager))
                    {
                        if (field.GetValue(obj) == null)
                        {
                            field.SetValue(obj, pickupManager);
                        }
                    }
                }
            }
        }

        private static void InjectIntoStaticObjectSyncs()
        {
            var objects = Object.FindObjectsByType(typeof(StaticObjectSync), FindObjectsSortMode.None);

            if (objects.Length == 0)
            {
                return;
            }

            var containerManager = Object.FindObjectOfType(typeof(StaticObjectContainerManager));
            if (containerManager == null)
            {
                Debug.LogWarning("Static pickup container manager not found in the scene");
                return;
            }

            foreach (var obj in objects)
            {
                var type = obj.GetType();
                var fields = type.GetFields(
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(SerializeField), true))
                    {
                        continue;
                    }

                    if (field.FieldType == typeof(StaticObjectContainerManager))
                    {
                        if (field.GetValue(obj) == null)
                        {
                            field.SetValue(obj, containerManager);
                        }
                    }
                }
            }
        }

        private static void InjectIntoStaticObjectContainers()
        {
            var objects = Object.FindObjectsByType(typeof(StaticObjectContainer), FindObjectsSortMode.None);

            if (objects.Length == 0)
            {
                return;
            }

            var containerManager = Object.FindObjectOfType(typeof(StaticObjectContainerManager));
            if (containerManager == null)
            {
                Debug.LogWarning("Static pickup container manager not found in the scene");
                return;
            }

            foreach (var obj in objects)
            {
                var type = obj.GetType();
                var fields = type.GetFields(
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(SerializeField), true))
                    {
                        continue;
                    }

                    if (field.FieldType == typeof(StaticObjectContainerManager))
                    {
                        if (field.GetValue(obj) == null)
                        {
                            field.SetValue(obj, containerManager);
                        }
                    }
                }
            }
        }
    }
}
#endif