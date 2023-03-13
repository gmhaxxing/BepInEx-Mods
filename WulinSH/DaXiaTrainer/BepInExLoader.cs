using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using DaXiaTrainer.UI;
using HarmonyLib;
using UnityEngine;


using Il2CppInterop.Runtime.Injection;


namespace DaXiaTrainer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BepInExLoader : BasePlugin
    {
        #region[Declarations]
        public const string 
            GUID = "com.haxx.DaXiaTrainer",
            NAME = "DaXiaTrainer",
            AUTHOR = "Haxx",
            VERSION = "1.0.0";

        public static ManualLogSource log;

        public static ConfigEntry<KeyCode> editorToggle;
        #endregion

        public BepInExLoader()
        {
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
            Application.runInBackground = true;
            log = Log;
        }
        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e) => log.LogError("\r\n\r\nUnhandled Exception:" + (e.ExceptionObject as Exception).ToString());


        public override void Load()
        {
            #region[Register Types in Il2Cpp]

            log.LogMessage("Registering C# Type's in Il2Cpp");

            try
            {
                // Trainer
                ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();
                ClassInjector.RegisterTypeInIl2Cpp<TrainerComponent>();

                // UI
                ClassInjector.RegisterTypeInIl2Cpp<UITrainerPanel>();
                ClassInjector.RegisterTypeInIl2Cpp<UIRoleEditorPanel>();
                ClassInjector.RegisterTypeInIl2Cpp<UITeamEntry>();
                ClassInjector.RegisterTypeInIl2Cpp<UIItemPanel>();
                ClassInjector.RegisterTypeInIl2Cpp<UIMiscPanel>();
                ClassInjector.RegisterTypeInIl2Cpp<UIMartialPanel>();
                ClassInjector.RegisterTypeInIl2Cpp<UIAddTrait>();
            }
            catch
            {
                log.LogError("FAILED to Register Il2Cpp Type!");
            }

            #endregion

            #region[Bootstrap The Main Trainer GameObject]

            Bootstrapper.Create("BootStrapperGO");

            #endregion

            #region[Harmony Patching]

            try {
                var harmony = new Harmony(GUID);

                var originalUpdate = AccessTools.Method(typeof(UnityEngine.UI.CanvasScaler), "Handle");
                var postUpdate = AccessTools.Method(typeof(DaXiaTrainer.Bootstrapper), "Update");
                harmony.Patch(originalUpdate, postfix: new HarmonyMethod(postUpdate));

                //harmony.PatchAll(typeof(Patches));
                harmony.PatchAll(typeof(ItemTipPatch));
                harmony.PatchAll(typeof(MiscPatch));
            }
            catch { log.LogError("FAILED to Apply Hooks's!"); }

            #endregion

            editorToggle = Config.Bind(new ConfigDefinition("Settings", "Editor Toggle"),
                KeyCode.F5,
                new ConfigDescription("The toggle for the Editor"));

            log.LogMessage("Initializing Il2CppTypeSupport...");
            Il2CppTypeSupport.Initialize();
            Bootstrapper.Create("BootStrapperGO");
        }

    }
}
