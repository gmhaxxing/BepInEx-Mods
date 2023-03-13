using HarmonyLib;
using System;
using UnityEngine;
using Il2CppInterop.Runtime;

namespace DaXiaTrainer
{
    public class Bootstrapper : MonoBehaviour
    {
        private static GameObject trainer = null;

        internal static GameObject Create(string name)
        {
            var obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            var component = new Bootstrapper(obj.AddComponent(Il2CppType.Of<Bootstrapper>()).Pointer);
            return obj;
        }

        public Bootstrapper(IntPtr intPtr) : base(intPtr) { }

        public void Awake()
        {
            // BepInExLoader.log.LogMessage("Bootstrapper Awake() Fired!");
        }

        [HarmonyPostfix]
        public static void Update()
        {
            //BepInExLoader.log.LogMessage("Bootstrapper Update() Fired!");

            if (trainer == null)
            {
                BepInExLoader.log.LogMessage(" ");
                BepInExLoader.log.LogMessage("Bootstrapping Trainer...");
                try
                {
                    trainer = TrainerComponent.Create("TrainerComponentGO");
                    if (trainer != null) { BepInExLoader.log.LogMessage("Trainer Bootstrapped!"); BepInExLoader.log.LogMessage(" "); }
                }
                catch (Exception e)
                {
                    BepInExLoader.log.LogMessage("ERROR Bootstrapping Trainer: " + e.Message);
                    BepInExLoader.log.LogMessage(" ");
                }
            }
        }
    }
}
