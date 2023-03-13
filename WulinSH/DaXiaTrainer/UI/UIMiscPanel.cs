using BepInEx.Logging;
using HarmonyLib;
using System;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using TMPro;
using WuLin;
using GameData;


namespace DaXiaTrainer.UI
{
    public class UIMiscPanel : MonoBehaviour
    {
        public UIMiscPanel(IntPtr ptr) : base(ptr) { }
        public ManualLogSource log => BepInExLoader.log;

        public static bool timeFreezed = false;
        public static bool recover = false;
        public static bool noEncounter = false;
        public static bool abilityMulti = false;
        public static bool incRelation = false;
        public static int battleSpeed = 1;

        private void Awake()
        {

        }

        public void Init()
        {
            var miscEntry = transform.GetChild(0).GetComponent<RectTransform>();
            DestroyImmediate(miscEntry.GetComponent<GameBoolSettingReference>());
            DestroyImmediate(miscEntry.GetComponent<PoolObject>());
            DestroyImmediate(miscEntry.Find("Desc").GetComponent<LocalizationComponent>());
            DestroyImmediate(miscEntry.Find("Desc").GetComponent<FontAdaptor>());
            miscEntry.sizeDelta = new Vector2(1460, 66);
            miscEntry.Find("Background/CheckmarkBg").localPosition = new Vector3(690, 0, 0);
            miscEntry.Find("Background/Checkmark").localPosition = new Vector3(690, 2, 0);


            var tmp = Instantiate(miscEntry, transform, true);
            tmp.Find("Background/CheckmarkBg").localPosition = new Vector3(690, 0, 0);
            tmp.Find("Background/Checkmark").localPosition = new Vector3(690, 2, 0);



            log.LogMessage("Create Entry Done");

            transform.GetChild(0).Find("Desc").GetComponent<TextMeshProUGUI>().text = "时间暂停";
            var toggle = transform.GetChild(0).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                timeFreezed = newValue;
            });
            toggle.Set(false);
            //log.LogMessage("Freeze Time Done");

            transform.GetChild(1).Find("Desc").GetComponent<TextMeshProUGUI>().text = "战斗后恢复状态";
            toggle = transform.GetChild(1).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                recover = newValue;
            });
            toggle.Set(false);
            //log.LogMessage("Recover Done");

            Instantiate(miscEntry, transform, true);
            transform.GetChild(2).Find("Desc").GetComponent<TextMeshProUGUI>().text = "不遇敌";
            toggle = transform.GetChild(2).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                noEncounter = newValue;
            });
            toggle.Set(false);

            Instantiate(miscEntry, transform, true);
            transform.GetChild(3).Find("Desc").GetComponent<TextMeshProUGUI>().text = "5倍能力经验获取";
            toggle = transform.GetChild(3).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                abilityMulti = newValue;
            });
            toggle.Set(false);
            // log.LogMessage("No Encounter Done");

            Instantiate(miscEntry, transform, true);
            transform.GetChild(4).Find("Desc").GetComponent<TextMeshProUGUI>().text = "添加满好感按钮（送礼页面）";
            toggle = transform.GetChild(4).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate (bool newValue)
            {
                incRelation = newValue;
            });
            toggle.Set(false);


            #region[Button Grid]
            var buttonGrid = new GameObject().AddComponent<RectTransform>();
            buttonGrid.SetParent(transform, false);
            buttonGrid.name = "ButtonGrid";
            buttonGrid.sizeDelta = new Vector2(1460, 300);
            var grid = buttonGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300, 80);
            grid.spacing = new Vector2(85, 20);

            var buttonTemp = UiSingletonPrefab<EscUI>.Instance.main_Resume.gameObject;
            var button = Instantiate(buttonTemp, buttonGrid, false);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "恢复体力";
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                var teamManager = MonoSingleton<PlayerTeamManager>.Instance;
                teamManager.ModifyProp("队伍体力", 100);
            });


            button = Instantiate(buttonTemp, buttonGrid, false);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "恢复心情";
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                var teamManager = MonoSingleton<PlayerTeamManager>.Instance;
                teamManager.ModifyProp("队伍心情", 100);
            });

            button = Instantiate(buttonTemp, buttonGrid, false);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "解锁全部成就";
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                var achievementDB = BaseDataClass.GetGameData<AchievementDataScriptObject>().data;
                foreach(var id in achievementDB.Keys)
                {
                    MonoSingleton<AchievementManager>.Instance.Complate(id);
                }
            });

            #endregion


            //var sliderTemp = UiSingletonPrefab<UISetting>.Instance.MusicVolumeSlider.transform.parent;
            //var sliderGroup = Instantiate(sliderTemp, transform, false);
            //sliderGroup.SetSiblingIndex(0);
            //sliderGroup.Find("NameBg/Name").GetComponent<TextMeshProUGUI>().text = "战斗速度";
            //var valueText = sliderGroup.Find("Value/MusicVolumeValue").GetComponent<TextMeshProUGUI>();
            //valueText.text = (battleSpeed * 100f).ToString("F2");
            //var slider = sliderGroup.GetComponentInChildren<Slider>();
            //slider.minValue = 100;
            //slider.maxValue = 500;
            //slider.onValueChanged.RemoveAllListeners();
            //slider.onValueChanged.AddListener(delegate (float value)
            //{
            //    valueText.text = (value / 100f).ToString("F2");
            //    battleSpeed = value / 100f;
            //});

        }

    }

    public class MiscPatch
    {
        public static ManualLogSource log => BepInExLoader.log;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveObjectGameTime), "AddDeltaTime")]
        public static bool AddDeltaTime_PrePatch()
        {
            return !UIMiscPanel.timeFreezed;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleManager), "LeaveBattle")]
        public static void LeaveBattle_PrePatch()
        {
            if (UIMiscPanel.recover) {
                PlayerTeamManager.Instance.PlayerDataInstance.FullyRecover();
            }
        }

        #region[遇敌]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoamingManager), "GetNpcBySightPoint")]
        public static void GetNpcBySightPoint_PostPatch(ref List<Npc> __result)
        {
            if (UIMiscPanel.noEncounter)
            {
                __result.Clear();
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StealthManager), "GetPerceptionSpeed")]
        public static void GetPerceptionSpeed_PostPatch(ref float __result)
        {
            if (UIMiscPanel.noEncounter)
            {
                __result = 0;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StealthPerceptComponent), "OnFound")]
        public static bool OnFound_PrePatch()
        {
            return !UIMiscPanel.noEncounter;
        }
        #endregion


        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameCharacterInstance), "ChangeAdditionProp")]
        public static bool ChangeAdditionProp_PrePatch(string key, ref Il2CppSystem.Decimal value)
        {
            if (UIMiscPanel.abilityMulti && key.Contains("能力经验_"))
            {
                value *= 5;
            }
            return true;
        }

        #region[好感]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GiftingWithNpcUI), "Awake")]
        public static void GiftingWithNpcUIAwake_PostPatch(GiftingWithNpcUI __instance)
        {
            var buttonTemp = UiSingletonPrefab<EscUI>.Instance.main_Resume.gameObject;

            var button = UnityEngine.Object.Instantiate(buttonTemp, __instance.transform, false);
            button.name = "IncRelation";
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
            button.GetComponent<RectTransform>().localPosition = new Vector3(-10, 120, 0);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "满好感";
            button.GetComponentInChildren<TextMeshProUGUI>().fontSize = 20;
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                var source = GameCharacterInstance.RelationModifySource.Gift;
                GiftingWithNpcManager.npc?.ModifyRelationWithPlayer(100, source);
            }) ;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GiftingWithNpcUI), "Update")]
        public static bool GiftingWithNpcUIUpdate_PrePatch(GiftingWithNpcUI __instance)
        {
            var button = __instance.transform.Find("IncRelation");
            button?.gameObject.SetActive(UIMiscPanel.incRelation);
            return true;
        }

        #endregion

        #region[加速]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BattleUI), "OnSpeedButtonClickHandler")]
        public static void BattleUISwitchSpeed_PrePatch()
        {
            UIMiscPanel.battleSpeed = (UIMiscPanel.battleSpeed * 2) % 7;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleUI), "SwitchSpeed")]
        public static void BattleUISwitchSpeed_PostPatch(BattleUI __instance)
        {
            __instance.speedText.text = $"x{UIMiscPanel.battleSpeed}";
            Time.timeScale = UIMiscPanel.battleSpeed;

        }

        #endregion
    }

}
