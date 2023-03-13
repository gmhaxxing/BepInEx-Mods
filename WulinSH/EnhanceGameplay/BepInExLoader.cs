using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using EnhanceGameplay.UI;
using Il2CppInterop.Runtime.Injection;


namespace EnhanceGameplay
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BepInExLoader : BasePlugin
    {
        #region[Declarations]
        public const string 
            GUID = "com.haxx.EnhanceGameplay",
            NAME = "EnhanceGameplay",
            AUTHOR = "Haxx",
            VERSION = "1.0.0";

        public static ManualLogSource log;

        public static ConfigEntry<KeyCode> batchHotKey;
        public static ConfigEntry<bool> easyQTE;
        public static ConfigEntry<bool> martialNum;
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
                ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();
                ClassInjector.RegisterTypeInIl2Cpp<ModComponent>();
                ClassInjector.RegisterTypeInIl2Cpp<WindowDragHandler>();
            }
            catch
            {
                log.LogError("FAILED to Register Il2Cpp Type!");
            }

            #endregion

            #region[Bootstrap The Main Trainer GameObject]

            Bootstrapper.Create("BootStrapperGO");

            #endregion

            #region[Get Configuration]
            batchHotKey = Config.Bind(new ConfigDefinition("Settings", "Batch Toggle"),
                KeyCode.LeftControl,
                new ConfigDescription("批量出售/购买快捷键"));

            easyQTE = Config.Bind(new ConfigDefinition("Settings", "EasyQTE"),
                true,
                new ConfigDescription("简单的QTE（偷窃/下毒/刺杀"));

            martialNum = Config.Bind(new ConfigDefinition("Settings", "MartialNum"),
                true,
                new ConfigDescription("武学数量无上限"));
            #endregion

            #region[Harmony Patching]
            try
            {
                var harmony = new Harmony(GUID);

                var originalUpdate = AccessTools.Method(typeof(UnityEngine.UI.CanvasScaler), "Handle");
                var postUpdate = AccessTools.Method(typeof(EnhanceGameplay.Bootstrapper), "Update");
                harmony.Patch(originalUpdate, postfix: new HarmonyMethod(postUpdate));

                #region[IBeginDragHandler, IDragHandler, IEndDragHandler Hooks]

                //// These are required since UnHollower doesn't support Interfaces yet

                //// IBeginDragHandler
                //var originalOnBeginDrag = AccessTools.Method(typeof(UnityEngine.EventSystems.EventTrigger), "OnBeginDrag");
                //log.LogMessage("   Original Method: " + originalOnBeginDrag.DeclaringType.Name + "." + originalOnBeginDrag.Name);
                //var postOnBeginDrag = AccessTools.Method(typeof(WindowDragHandler), "OnBeginDrag");
                //log.LogMessage("   Postfix Method: " + postOnBeginDrag.DeclaringType.Name + "." + postOnBeginDrag.Name);
                //harmony.Patch(originalOnBeginDrag, postfix: new HarmonyMethod(postOnBeginDrag));

                //// IDragHandler
                //var originalOnDrag = AccessTools.Method(typeof(UnityEngine.EventSystems.EventTrigger), "OnDrag");
                //log.LogMessage("   Original Method: " + originalOnDrag.DeclaringType.Name + "." + originalOnDrag.Name);
                //var postOnDrag = AccessTools.Method(typeof(WindowDragHandler), "OnDrag");
                //log.LogMessage("   Postfix Method: " + postOnDrag.DeclaringType.Name + "." + postOnDrag.Name);
                //harmony.Patch(originalOnDrag, postfix: new HarmonyMethod(postOnDrag));

                //// IEndDragHandler
                //var originalOnEndDrag = AccessTools.Method(typeof(UnityEngine.EventSystems.EventTrigger), "OnEndDrag");
                //log.LogMessage("   Original Method: " + originalOnEndDrag.DeclaringType.Name + "." + originalOnEndDrag.Name);
                //var postOnEndDrag = AccessTools.Method(typeof(WindowDragHandler), "OnEndDrag");
                //log.LogMessage("   Postfix Method: " + postOnEndDrag.DeclaringType.Name + "." + postOnEndDrag.Name);
                //harmony.Patch(originalOnEndDrag, postfix: new HarmonyMethod(postOnEndDrag));

                #endregion

                harmony.PatchAll(typeof(MiscPatch));
                if (martialNum.Value) harmony.PatchAll(typeof(MartialNumPatch));
                if (easyQTE.Value) harmony.PatchAll(typeof(EasyQTEPatch));
            }
            catch { log.LogError("FAILED to Apply Hooks's!"); }

            #endregion



            log.LogMessage("Initializing Il2CppTypeSupport...");
            Il2CppTypeSupport.Initialize();
            Bootstrapper.Create("BootStrapperGO");
        }

    }
}
