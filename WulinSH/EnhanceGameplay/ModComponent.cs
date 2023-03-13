using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Input = BepInEx.Unity.IL2CPP.UnityEngine.Input; //For UnityEngine.Input
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using WuLin.GameFrameworks;
using UniverseLib;
using TMPro;
using WuLin;
using System.Collections;
using BepInEx.Logging;
using HarmonyLib;
using WuLin.StateMachine;
using static WuLin.UISlider;
using static WuLin.GameCharacterInstance;
using EnhanceGameplay.UI;

namespace EnhanceGameplay
{
    public class ModComponent : MonoBehaviour
    {
        #region[Declarations]

        // Trainer Base
        public static GameObject obj = null;
        public static ModComponent instance;
        private static bool initialized = false;

        public static bool isBatch = false;
        public static bool playerSpeedUp = false;


        public ManualLogSource log => BepInExLoader.log;

        #endregion

        internal static GameObject Create(string name)
        {
            obj = new GameObject(name);
            DontDestroyOnLoad(obj);

            var component = new ModComponent(obj.AddComponent(Il2CppType.Of<ModComponent>()).Pointer);

            return obj;
        }

        public ModComponent(IntPtr ptr) : base(ptr)
        {
            instance = this;
        }

        private void Initialize()
        {
            
            var roamingUI = GameObject.Find("GameSingletonRoot/WuLin.UIRoot(Clone)/SafeArea/Normal/RoamingUI(Clone)/TimePanelGroup");
            if (roamingUI == null) { return; }

            var buttonTemp = UiSingletonPrefab<BattleUI>.Instance.transform.Find("BattleControlGroup/SpeedButton");
            if (buttonTemp == null) { return; }

            #region[Create Speed Up Button]
            var button = Instantiate(buttonTemp, roamingUI.transform, false).gameObject;
            DestroyImmediate(button.GetComponent<Button>());
            DestroyImmediate(button.GetComponent<EventTriggerDelegate>());
            DestroyImmediate(button.GetComponent<HotKeyBinder>());
            button.GetComponent<RectTransform>().localPosition = new Vector3(-205, 85, 0);

            var buttonComp = button.AddComponent<Button>();
            buttonComp.onClick.AddListener(delegate
            {
                playerSpeedUp = !playerSpeedUp;
                button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerSpeedUp ? "x2" : "x1";

            });
            #endregion

            #region[Create Martial ScrollRect]

            var martialPanel = UiSingletonPrefab<UIMenuPanel>.Instance.panel[2].transform;
            var kungfuContent = martialPanel.Find("MartialArts/KongFu/KongFu");
            var scrollViewTemp = martialPanel.Find("MartialArts/UniqueSkill/ScrollView");
            var scrollView = Instantiate(scrollViewTemp, kungfuContent.parent, false).GetComponent<ScrollRect>();
            kungfuContent.SetParent(scrollView.content.parent);
            DestroyImmediate(scrollView.content.gameObject);
            kungfuContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollView.content = kungfuContent.GetComponent<RectTransform>();
            scrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(865.7728f, 540.16f);
            scrollView.transform.localPosition = new Vector3(0, -275, 0);

            if (BepInExLoader.martialNum.Value)
            {
                foreach (var kongfuEntry in kungfuContent.gameObject.Children())
                {
                    var removeButton = kongfuEntry.transform.Find("Remove");
                    var upButton = Instantiate(removeButton, kongfuEntry.transform, false);
                    upButton.name = "MoveForward";
                    removeButton.localPosition = new Vector3(140, -18, 0);
                    upButton.localPosition = new Vector3(45, -18, 0);
                    upButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "前置";
                }
            }
            #endregion

            initialized = true;
            log.LogMessage("EnhanceGameplay Initialized!");
        }


        public void Update()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (UnityEngine.Input.GetKeyDown(BepInExLoader.batchHotKey.Value))
            {
                log.LogMessage("Batch key down");
                isBatch = true;
            }
            if (UnityEngine.Input.GetKeyUp(BepInExLoader.batchHotKey.Value))
            {
                log.LogMessage("Batch key up");
                isBatch = false;
            }
        }

    }

    public class MiscPatch
    {
        public static ManualLogSource log => BepInExLoader.log;
        public static int battleSpeed = 1;
        public static bool isMinning = false;


        #region[加速]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BattleUI), "OnSpeedButtonClickHandler")]
        public static void BattleUISwitchSpeed_PrePatch()
        {
            battleSpeed = (battleSpeed * 2) % 7;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUI), "SwitchSpeed")]
        public static void BattleUISwitchSpeed_PostPatch(BattleUI __instance)
        {
            __instance.speedText.text = $"x{battleSpeed}";
            Time.timeScale = battleSpeed;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Role), "UpdateSpeed")]
        public static void RoleUpdateSpeed_PostPatch(Role __instance)
        {
            if (ModComponent.playerSpeedUp && __instance == MonoSingleton<RoamingManager>.Instance.player)
            {
                __instance.speed *= 2;
            }
        }


        #endregion

        #region[批量出售/购买]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactionSellItem), "OnSellButtonClicked")]
        public static bool OnSellButtonClicked_PrePatch(FactionSellItem __instance)
        {
            var item = __instance.cachedData;
            if (item.IsStackable && !ModComponent.isBatch) return true;

            log.LogMessage($"SellItem {item.Stack}");
            var num = item.Stack;
            for (int i = 0; i < num; i++)
            {
                TradingWithFactionManager.SellItem(item);
            }

            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactionBuyItem), "OnBuyButtonClicked")]
        public static bool OnBuyButtonClicked_PrePatch(FactionBuyItem __instance)
        {
            var item = __instance.cachedData;
            if (!ModComponent.isBatch) return true;

            log.LogMessage($"BuyItem {item.Stock}");
            var num = item.Stock;
            for (int i = 0; i < num; i++)
            {
                TradingWithFactionManager.BuyItem(item);
            }

            return false;
        }
        #endregion


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartSpInteractiveActionNode), "InvokeAction")]
        public static void StartSpInteractiveActionNode_PrePatch(StartSpInteractiveActionNode __instance)
        {
            if (__instance.interactiveType == StartSpInteractiveActionNode.InteractiveType.TradeWithNpc)
            {
                __instance.useKey = false;
                __instance.minimalRalation = -100;
            }
        }

         
        #region[工具]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MiningBatchUI), "OnPlayEnd")]
        public static void MiningBatchUIOnPlayEnd_PrePatch(MiningBatchUI __instance)
        {
            __instance.tableData.Level = 0;
            isMinning = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MiningBatchUI), "OnPlayEnd")]
        public static void MiningBatchUIOnPlayEnd_PostPatch(MiningBatchUI __instance)
        {
            isMinning = false;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnityEngine.Random), "Range", new Type[] { typeof(float), typeof(float)})]
        public static void RandomRange_PostPatch(ref float __result)
        {
            if (isMinning && __result < 0.1f) __result = 0.1f; 
        }
        #endregion
    }

    public class MartialNumPatch
    {
        #region[武学上限]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameCharacterInstance), "CouldLearnKungfu")]
        public static void CouldLearnKungfu_PostPatch(ref EquipingCheckResult __result)
        {
            if (__result == EquipingCheckResult.CharacterMaxKungfuCount)
                __result = EquipingCheckResult.Ok;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIKongfuPanel), "InitLeftPanel")]
        public static void InitLeftPanel_PostPatch(UIKongfuPanel __instance)
        {
            __instance.LearnedSkillPanel.GetChild(9).gameObject.SetActive(true);

            var kungfuContent = __instance.LearnedSkillPanel;
            foreach (var kongfuEntry in kungfuContent.gameObject.Children())
            {
                var upButton = kongfuEntry.transform.Find("MoveForward");

                upButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "前置";
                upButton.GetComponent<Button>().onClick.RemoveAllListeners();
                upButton.GetComponent<Button>().onClick.AddListener(delegate
                {
                    var entry = upButton.parent;
                    var curObjIndex = entry.GetSiblingIndex();
                    if (curObjIndex == 0) return;

                    var currKungfu = entry.GetComponent<UILearnedSkillPanel>().data;
                    //var prevKungfu = entry.parent.GetChild(0).GetComponent<UILearnedSkillPanel>().data;
                    var kungfuInstances = currKungfu.GameCharacterInstance.KungfuInstances;
                    int curIndex = 0;
                    //int prevIndex = 0;
                    for (int i = 0; i < kungfuInstances.Count; i++)
                    {
                        if (kungfuInstances[i] == currKungfu)
                        {
                            curIndex = i;
                        }
                        //if (kungfuInstances[i] == prevKungfu)
                        //{
                        //    prevIndex = i;
                        //}
                    }
                    //var tmpKungfu = currKungfu;
                    kungfuInstances.Remove(currKungfu);
                    kungfuInstances.Insert(0, currKungfu);
                    //kungfuInstances[curIndex] = kungfuInstances[prevIndex];
                    //kungfuInstances[prevIndex] = tmpKungfu;

                    __instance.ClearKongfu();
                    __instance.InitLeftPanel();
                });
                upButton.gameObject.SetActive(upButton.parent.GetComponent<UILearnedSkillPanel>().data != null);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameCharacterInstance), "ComputeKungfuProp")]
        public static bool InitLeftPanel_PrePatch(GameCharacterInstance __instance)
        {
            //TODO: only add first 9 martial's prop   
            var kungfuModifiedProps = __instance.KungfuModifiedProps;
            var kungfuInstances = __instance.kungfuInstances;
            if (kungfuModifiedProps == null || kungfuInstances == null) return false;
            kungfuModifiedProps.Clear();

            int kungfuActiveNum = 0;
            foreach (var kungfu in kungfuInstances)
            {
                if (kungfu.IsPropMatched && kungfu.IsValid) { continue; }

                var propAddByLevel = kungfu.levelTemplete.CharacterPropAdd;
                for(int i=0; i < propAddByLevel.Length; i++)
                {
                    if (!propAddByLevel[i].isValid) { continue; }

                    GameUtil.AddVaule(kungfuModifiedProps, propAddByLevel[i].key, propAddByLevel[i].dValues[0]);
                }
                if (++kungfuActiveNum == 9) break;
            }
            return false;
        }
        #endregion
    }

    public class EasyQTEPatch
    {
        #region[QTE]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UISlider), "InitPanel")]
        public static void UISliderInitPanel_PrePatch(UISlider __instance, SliderParameter par)
        {
            par.Range = 1;
        }
        #endregion
    }
}
