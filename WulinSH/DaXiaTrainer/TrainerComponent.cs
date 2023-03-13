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
using DaXiaTrainer.UI;
using WuLin.GameFrameworks;
using UniverseLib;
using TMPro;
using WuLin;
using System.Collections;

namespace DaXiaTrainer
{
    public class TrainerComponent : MonoBehaviour
    {
        #region[Declarations]

        // Trainer Base
        public static GameObject obj = null;
        public static TrainerComponent instance;
        private static bool initialized = false;
        private static bool initializedMisc = false;
        private static BepInEx.Logging.ManualLogSource log;
        public static bool optionToggle = false;

        // UI
        private static GameObject uiPanel = null;

        #endregion

        internal static GameObject Create(string name)
        {
            obj = new GameObject(name);
            DontDestroyOnLoad(obj);

            var component = new TrainerComponent(obj.AddComponent(Il2CppType.Of<TrainerComponent>()).Pointer);

            return obj;
        }

        public TrainerComponent(IntPtr ptr) : base(ptr)
        {
            log = BepInExLoader.log;

            instance = this;
        }

        private void Initialize()
        {
            var normal = GameObject.Find("GameSingletonRoot/WuLin.UIRoot(Clone)/SafeArea/Window");
            if (normal == null) { return; }
            var uiMenu = UiSingletonPrefab<UIMenuPanel>.Instance;
            if (uiMenu == null || normal == null) { return; }

            #region[Destroy Redundant Object]
            if (uiPanel == null) uiPanel = Instantiate(uiMenu, normal.transform, false).gameObject;
            uiPanel.name = "UIEditorPanle";
            var uiMenuComp = uiPanel.GetComponent<WuLin.UIMenuPanel>();
            GameObject uiRole = null;
            foreach (var panel in uiMenuComp.panel)
            {
                if (panel.name.Contains("UIPack"))
                {
                    panel.AddComponent<UIItemPanel>().Init();
                    panel.name = "ItemPanel";
                    continue;
                }
                if (panel.name.Contains("UIRoleMenuPanel"))
                {
                    uiRole = panel;
                    panel.name = "RolePanel";
                    continue;
                }
                if (panel.name.Contains("UIMartialArts"))
                {
                    panel.AddComponent<UIMartialPanel>().Init();
                    panel.name = "MartialPanel";
                    continue;
                }
                DestroyImmediate(panel);
            }

            #region[Misc Panel]

            var uiMisc = new GameObject().AddComponent<RectTransform>();
            uiMisc.SetParent(uiPanel.transform, false);
            uiMisc.name = "MiscPanel";
            uiMisc.sizeDelta = new Vector2(1460, 800);
            uiMisc.localPosition = new Vector3(0, -50, 0);
            var verGroup = uiMisc.gameObject.AddComponent<VerticalLayoutGroup>();
            verGroup.childControlHeight = false;
            verGroup.childControlWidth = false;
            verGroup.childForceExpandHeight = false;
            verGroup.childForceExpandWidth = false;
            verGroup.spacing = 30;

            #endregion

            #region[Martial Panel]

            //var uiMartial = new GameObject().AddComponent<RectTransform>();
            //uiMartial.SetParent(uiPanel.transform, false);
            //uiMartial.name = "MartialPanel";

            #endregion

            #region [Tittle Panel]
            var mainToggleGroup = uiMenuComp.Tittle.transform;
            for (int i = 0; i < mainToggleGroup.childCount; i++)
            {
                DestroyImmediate(mainToggleGroup.GetChild(i).GetComponent<RedDotMonoView>());
                mainToggleGroup.GetChild(i).GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                mainToggleGroup.GetChild(i).gameObject.SetActive(false);
            }

            var roleToggle = mainToggleGroup.GetChild(0);
            var martialToggle = mainToggleGroup.GetChild(1);
            var itemToggle = mainToggleGroup.GetChild(2);
            var miscToggle = mainToggleGroup.GetChild(3);
            roleToggle.name = "Role";
            martialToggle.name = "Martial";
            itemToggle.name = "Item";
            miscToggle.name = "Misc";
            roleToggle.GetComponentInChildren<TextMeshProUGUI>().text = "角色";
            martialToggle.GetComponentInChildren<TextMeshProUGUI>().text = "武学";
            itemToggle.GetComponentInChildren<TextMeshProUGUI>().text = "物品";
            miscToggle.GetComponentInChildren<TextMeshProUGUI>().text = "杂项";
            roleToggle.gameObject.SetActive(true);
            martialToggle.gameObject.SetActive(true);
            itemToggle.gameObject.SetActive(true);
            miscToggle.gameObject.SetActive(true);


            //roleToggle.GetComponent<Toggle>().onValueChanged.AddListener(UnityAction<bool>(OnRoleToggleClick));
            uiPanel.AddComponent<UITrainerPanel>().Init();

            #endregion

            #region[Team Panel]
            WuLin.UITeamPanel uiTeam = uiMenuComp.TeamPanel;
            MonoSingleton<WuLin.GameEventManager>.Instance.UnregisterAllListenerByReceiver(uiTeam);


            #endregion

            #region[Role Panel]

            var roleInfo = uiRole.GetComponentInChildren<UILeftContentByRoleInfoPanel>();
            foreach (var obj in roleInfo.gameObject.Children())
            {
                if (obj.name == "RoleInfor1")
                {
                    InitRoleInfoPanel(obj);
                    continue;
                }
                DestroyImmediate(obj);
            }

            var traitPanel = uiRole.GetComponentInChildren<WuLin.UIRolePanelRightPanel>();
            traitPanel.transform.Find("titleTxt").GetComponent<TextMeshProUGUI>().text = "添加天赋";

            traitPanel.gameObject.AddComponent<UIAddTrait>().Init();

            DestroyImmediate(roleInfo.Pill.gameObject);
            DestroyImmediate(uiRole.transform.GetChild(uiRole.transform.GetChildCount() - 1).gameObject);

            uiTeam.transform.SetParent(uiRole.transform, false);
            uiRole.AddComponent<UIRoleEditorPanel>().Init();
            #endregion


            DestroyImmediate(uiMenuComp.Setting.gameObject);
            DestroyImmediate(uiMenuComp.Esc.gameObject);
            DestroyImmediate(uiMenuComp.Red);
            DestroyImmediate(uiMenuComp.AbilityRed);
            //DestroyImmediate(uiPanel.GetComponent<CanvasGroup>());
            #endregion


            initialized = true;
            log.LogMessage("TrainerComponent Initialized!");
        }

        private void InitRoleInfoPanel(GameObject obj)
        {
            obj.transform.localPosition = new Vector3(-120, 120, 0);
            var roleInfo = obj.transform.Find("RoleInfo");
            var roleInfo1 = obj.transform.Find("RoleInfo1");
            var tagBG = obj.transform.Find("tagBG");
            var tag = obj.transform.Find("tag");
            var tag1 = obj.transform.Find("tag1");
            for (int i = 0; i < 25; i++)
            {
                Instantiate(tagBG.GetChild(0), tagBG, false);
            }
            tagBG.GetComponent<GridLayoutGroup>().constraintCount = 3;
            tagBG.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            log.LogMessage("add tagBg done");
            
            foreach (var entry in roleInfo.gameObject.Children())
            {
                var textInputTemp = WuLin.UiSingletonPrefab<WuLin.ConsoleUI>.Instance.textInput;
                var textInput = Instantiate(textInputTemp.gameObject, entry.transform, false);
                textInput.GetComponent<TMP_InputField>().pointSize = 25;
                textInput.GetComponent<TMP_InputField>().characterValidation = TMP_InputField.CharacterValidation.Decimal;
                textInput.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 30);

                #region[try]
                //var tmpInput = textInput.AddComponent<TMP_InputField>();
                //tmpInput.caretColor = new Color(0.67f, 0.404f, 0.106f, 1);
                //tmpInput.caretPositionInternal = textInputTemp.caretPositionInternal;
                //tmpInput.fontAsset = textInputTemp.fontAsset;
                //tmpInput.textComponent = tmpInput.transform.Find("Text Area/Text").GetComponent<TextMeshProUGUI>();
                //tmpInput.textViewport = tmpInput.transform.Find("Text Area").GetComponent<RectTransform>();
                //tmpInput.selectionFocusPosition = textInputTemp.selectionFocusPosition;
                //tmpInput.selectionStringFocusPosition = textInputTemp.selectionStringFocusPosition;
                //tmpInput.stringPosition = textInputTemp.selectionFocusPosition;
                //tmpInput.stringSelectPositionInternal = textInputTemp.selectionFocusPosition;
                //tmpInput.pointSize = 25;
                //tmpInput.characterValidation = TMP_InputField.CharacterValidation.Decimal;
                #endregion

                DestroyImmediate(textInput.transform.Find("Text Area/Placeholder").gameObject);
                DestroyImmediate(entry.GetComponent<TextMeshProUGUI>());
                DestroyImmediate(entry.GetComponent<WuLin.FontAdaptor>());
                DestroyImmediate(entry.GetComponent<WuLin.LocalizationComponent>());
            }

            foreach (var entry in roleInfo1.gameObject.Children())
            {
                var textInputTemp = WuLin.UiSingletonPrefab<WuLin.ConsoleUI>.Instance.textInput;
                var textInput = Instantiate(textInputTemp.gameObject, entry.transform, false);
                textInput.GetComponent<TMP_InputField>().pointSize = 25;
                textInput.GetComponent<TMP_InputField>().characterValidation = TMP_InputField.CharacterValidation.Decimal;
                textInput.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 30);
                DestroyImmediate(textInput.transform.Find("Text Area/Placeholder").gameObject);
                DestroyImmediate(entry.GetComponent<TextMeshProUGUI>());
                DestroyImmediate(entry.GetComponent<WuLin.FontAdaptor>());
                DestroyImmediate(entry.GetComponent<WuLin.LocalizationComponent>());
            }

            log.LogMessage("create inputfield done");

            var tmpTrans = Instantiate(tag1.GetChild(0), tag1, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "臂力";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_臂力";
            tmpTrans = Instantiate(roleInfo1.GetChild(0), roleInfo1, false);
            tmpTrans.name = "bili";

            tmpTrans = Instantiate(tag1.GetChild(0), tag1, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "体质";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_体质";
            tmpTrans = Instantiate(roleInfo1.GetChild(0), roleInfo1, false);
            tmpTrans.name = "tizhi";

            tmpTrans = Instantiate(tag1.GetChild(0), tag1, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "敏捷";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_敏捷";
            tmpTrans = Instantiate(roleInfo1.GetChild(0), roleInfo1, false);
            tmpTrans.name = "minjie";

            tmpTrans = Instantiate(tag1.GetChild(0), tag1, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "悟性";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_悟性";
            tmpTrans = Instantiate(roleInfo1.GetChild(0), roleInfo1, false);
            tmpTrans.name = "wuxing";

            tmpTrans = Instantiate(tag1.GetChild(0), tag1, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "福缘";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_福缘";
            tmpTrans = Instantiate(roleInfo1.GetChild(0), roleInfo1, false);
            tmpTrans.name = "fuyuan";

            tmpTrans = Instantiate(tag.GetChild(0), tag, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "生命上限";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_生命";
            tmpTrans = Instantiate(roleInfo.GetChild(0), roleInfo, false);
            tmpTrans.name = "hp";

            tmpTrans = Instantiate(tag.GetChild(0), tag, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "内力上限";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_内力";
            tmpTrans = Instantiate(roleInfo.GetChild(0), roleInfo, false);
            tmpTrans.name = "mp";

            tmpTrans = Instantiate(tag.GetChild(0), tag, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "实战经验";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_实战经验";
            tmpTrans = Instantiate(roleInfo.GetChild(0), roleInfo, false);
            tmpTrans.name = "exp";

            tmpTrans = Instantiate(tag.GetChild(0), tag, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "领悟点";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "领悟点";
            tmpTrans = Instantiate(roleInfo.GetChild(0), roleInfo, false);
            tmpTrans.name = "point";

            tmpTrans = Instantiate(tag.GetChild(0), tag, false);
            tmpTrans.name = $"Text({tmpTrans.GetSiblingIndex()})";
            tmpTrans.GetComponent<TextMeshProUGUI>().text = "等级";
            tmpTrans.GetComponent<WuLin.SimpleTextTip>().key = "等级";
            tmpTrans = Instantiate(roleInfo.GetChild(0), roleInfo, false);
            tmpTrans.name = "lv";

            tmpTrans = Instantiate(tag, tag.parent, false);
            tmpTrans.name = "tag2";
            tmpTrans.localPosition = new Vector3(295, 90, 0);
            tmpTrans.GetChild(0).GetComponent<TextMeshProUGUI>().text = "仁德";
            tmpTrans.GetChild(1).GetComponent<TextMeshProUGUI>().text = "义气";
            tmpTrans.GetChild(2).GetComponent<TextMeshProUGUI>().text = "礼节";
            tmpTrans.GetChild(3).GetComponent<TextMeshProUGUI>().text = "信用";
            tmpTrans.GetChild(4).GetComponent<TextMeshProUGUI>().text = "智慧";
            tmpTrans.GetChild(5).GetComponent<TextMeshProUGUI>().text = "勇气";
            tmpTrans.GetChild(6).GetComponent<TextMeshProUGUI>().text = "名声级别";
            tmpTrans.GetChild(7).GetComponent<TextMeshProUGUI>().text = "名声经验";
            tmpTrans.GetChild(8).GetComponent<TextMeshProUGUI>().text = "铜钱（贯）";
            tmpTrans.GetChild(0).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_仁德";
            tmpTrans.GetChild(1).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_义气";
            tmpTrans.GetChild(2).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_礼节";
            tmpTrans.GetChild(3).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_信用";
            tmpTrans.GetChild(4).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_智慧";
            tmpTrans.GetChild(5).GetComponent<WuLin.SimpleTextTip>().key = "CharPropInfo_勇气";
            tmpTrans.GetChild(6).GetComponent<WuLin.SimpleTextTip>().key = "名声级别";
            tmpTrans.GetChild(7).GetComponent<WuLin.SimpleTextTip>().key = "名声经验";
            tmpTrans.GetChild(8).GetComponent<WuLin.SimpleTextTip>().key = "铜钱";
            for (int i = 9; i < tmpTrans.childCount; i++)
            {
                tmpTrans.GetChild(i).gameObject.SetActive(false);
            }


            tmpTrans = Instantiate(roleInfo, roleInfo.parent, false);
            tmpTrans.name = "RoleInfo2";
            tmpTrans.localPosition = new Vector3(410, 285, 0);
            tmpTrans.GetChild(0).name = "rende";
            tmpTrans.GetChild(1).name = "yiqi";
            tmpTrans.GetChild(2).name = "lijie";
            tmpTrans.GetChild(3).name = "xinyong";
            tmpTrans.GetChild(4).name = "zhihui";
            tmpTrans.GetChild(5).name = "yongqi";
            tmpTrans.GetChild(6).name = "replv";
            tmpTrans.GetChild(7).name = "repexp";
            tmpTrans.GetChild(8).name = "coin";
            tmpTrans.GetComponent<VerticalLayoutGroup>().enabled = false;
            for (int i = 9; i < tmpTrans.childCount; i++)
            {
                tmpTrans.GetChild(i).gameObject.SetActive(false);
            }

            roleInfo.GetComponent<VerticalLayoutGroup>().enabled = true;
            roleInfo1.GetComponent<VerticalLayoutGroup>().enabled = true;
        }

        private void InitMiscPanel()
        {
            var uiMisc = uiPanel.transform.Find("MiscPanel");

            UiSingletonPrefab<UISetting>.Instance.Show();
            var uiSettingEntry = UiSingletonPrefab<UISetting>.Instance.GameBoolSettingLoopRect
                .transform.Find("Content").GetChild(0);
            Instantiate(uiSettingEntry, uiMisc, false);
            UiSingletonPrefab<UISetting>.Instance.Hide();

            uiMisc.gameObject.AddComponent<UIMiscPanel>().Init();

            initializedMisc = true;
        }


        private void Awake()
        {
            log.LogMessage("TrainerComponent Awake() Fired!");
        }

        public void Start()
        {
            log.LogMessage("TrainerComponent Start() Fired!");
        }

        public void OnEnable()
        {
            log.LogMessage("TrainerComponent OnEnable() Fired!");
        }


        public void Update()
        {
            if (!initialized) {
                Initialize();
            }

            if (initialized && !initializedMisc &&
                UiSingletonPrefab<WuLin.MainMenuUI>.Instance.gameObject.active)
            {
                InitMiscPanel();
            }

            if (UnityEngine.Input.GetKeyDown(BepInExLoader.editorToggle.Value))
            {
                log.LogMessage("key down");
                uiPanel.active = !uiPanel.active;
            }
        }

        //private bool enter = false;
        //IEnumerator timer()
        //{
        //    enter = true;
        //    //log.LogMessage("Your enter Coroutine at" + Time.time);
        //    yield return new WaitForSeconds(1.0f);

        //    if (!initialized)  {
        //        Initialize(); 
        //    }
        //    enter = false;
        //}
        /*
using DaXiaTrainer;
using DaXiaTrainer.UI;
using UnityEngine;

var log = DaXiaTrainer.BepInExLoader.log;
var chara = DaXiaTrainer.UI.UIRoleEditorPanel._character;
var buffs = chara.buffLib.buffInstances;
var traitDB = BaseDataClass.GetGameData<GameData.TraitDataScriptObject>().data;

foreach (var key in chara.Traits)
{
    log.LogMessage($"{traitDB[key].GetName(false)}");
}

var kungfus = chara.kungfuInstances;
    foreach (var kungfu in kungfus)
{
    log.LogMessage($"{kunfu.Templete.GetName()}");
}

foreach (var buff in buffs)
{
    log.LogMessage($"{buff.Templete.GetName()}");
}


         */


    }
}
