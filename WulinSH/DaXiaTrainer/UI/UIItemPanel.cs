using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using TMPro;

namespace DaXiaTrainer.UI
{
    public class UIItemPanel : MonoBehaviour
    {
        public GameObject item;
        public GameObject lastMenu;
        public TextMeshProUGUI pageDisplay;

        public Il2CppReferenceArray<Button> menuPanel;

        public List<UIItem> curPageItems = new();

        public static Dictionary<int, List<GameData.ItemData>> itemDataByType = new();

        //public LoopVerticalScrollRect itemsLoopVerticalScrollRect;

        public ManualLogSource log;

        public static bool isOpen = false;
        public int seletctTypeId;
        public WuLin.ItemType bagItemtype;
        public int currPage;

        private const int numPerPage = 45;

        public UIItemPanel(IntPtr ptr) : base(ptr) { }

        private void Awake()
        {
        }

        private void OnEnable()
        {
            isOpen = true;
            Refresh(0);
        }

        private void OnDisable()
        {
            isOpen = false;
        }

        private void InitMenuPanel()
        {
            for (int i = 0; i < menuPanel.Count; i++)
            {
                var type = i;
                menuPanel[i].m_OnClick.AddListener(delegate
                {
                    currPage = 1;
                    Refresh(type);
                });
            }
        }

        public void Init()
        {
            log = BepInExLoader.log;
            log.LogMessage($"UIItemPanel Init Fired");

            var uiPack = gameObject.GetComponent<WuLin.UIPack>();
            item = uiPack.Item;
            lastMenu = uiPack.LastMenu;

            menuPanel = new Il2CppReferenceArray<Button>(uiPack.menuPanel);
            InitMenuPanel();

            #region[Init Grid of Items]

            var items = uiPack.ItemsLoopVerticalScrollRect.gameObject;
            DestroyImmediate(transform.Find("Package/ShortcutItemsSetting").gameObject);
            DestroyImmediate(items.GetComponent<LoopVerticalScrollRect>());
            DestroyImmediate(items.GetComponent<LoopVerticalScrollRect>());
            DestroyImmediate(items.GetComponent<Image>());
            DestroyImmediate(items.GetComponent<Mask>());

            //var nameObj = FindObjectOfType<WuLin.IllustratedHandbookItemItem>().transform.Find("Name");
            for (int i = 0; i < numPerPage; i++)
            {
                var tmpItem = Instantiate(item, items.transform.Find("Content"), false).GetComponent<UIItem>();
                tmpItem.Num.transform.localPosition = new Vector3(0, -65, 0);
                tmpItem.Num.alignment = TextAlignmentOptions.Center;
                tmpItem.Num.gameObject.SetActive(true);
                //tmpItem.MarkTxt = Instantiate(nameObj, tmpItem.transform, false).gameObject.GetComponent<TextMeshProUGUI>();
                DestroyImmediate(tmpItem.gameObject.GetComponent<Button>());
                var btn = tmpItem.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(delegate{
                    var itemData = GetGameData(tmpItem.ID);
                    var pack = new WuLin.GameItemPack();
                    pack.AddItem(itemData);
                    MonoSingleton<WuLin.PlayerTeamManager>.Instance?.PickupPack(pack);
                });
                curPageItems.Add(tmpItem);
            }

            items.GetComponentInChildren<GridLayoutGroup>().constraintCount = 9;
            items.GetComponentInChildren<GridLayoutGroup>().spacing = new Vector2(50, 30);

            #endregion

            #region[Init Item Data]

            for (int i = 0; i < 6; i++)
            {
                itemDataByType.Add(i, new());
            }

            var itemsConfig = MonoSingleton<WuLin.GameConfig>.Instance.ItemDataScriptObject.ItemData;
            foreach (var itemData in itemsConfig)
            {
                var typeEquip = WuLin.ItemType.Equip;
                var typeBook = WuLin.ItemType.KungfuBook;
                var typeConsumable = WuLin.ItemType.Consumeable_Recipe | WuLin.ItemType.Consumeable_Edible;
                var typeMaterial = WuLin.ItemType.Consumeable_Material;
                var typeMap = WuLin.ItemType.Misc_Map;
                var typeOther = WuLin.ItemType.Misc ^ WuLin.ItemType.Misc_Map;

                if ((itemData.Type & typeEquip) == itemData.Type)
                {
                    itemDataByType[0].Add(itemData);
                }
                else if ((itemData.Type & typeBook) == itemData.Type)
                {
                    itemDataByType[1].Add(itemData);
                }
                else if ((itemData.Type & typeConsumable) == itemData.Type)
                {
                    itemDataByType[2].Add(itemData);
                }
                else if ((itemData.Type & typeMaterial) == itemData.Type)
                {
                    itemDataByType[3].Add(itemData);
                }
                else if ((itemData.Type & typeMap) == itemData.Type)
                {
                    itemDataByType[4].Add(itemData);
                }
                else if ((itemData.Type & typeOther) == itemData.Type)
                {
                    itemDataByType[5].Add(itemData);
                }
            }

            foreach (var list in itemDataByType.Values)
            {
                list.Sort((a, b) => a.Piror.CompareTo(b.Piror));
                log.LogMessage($"Item list size is {list.Count}");
            }

            #endregion

            #region[Add Buttons]

            var uiCharSheet = WuLin.UiSingletonPrefab<WuLin.PlayerCreateUI>.Instance.charSheetGroup.transform;
            var curr = Instantiate(uiCharSheet.Find("Choice"), item.transform.parent, false);
            var prev = Instantiate(uiCharSheet.Find("Return"), item.transform.parent, false);
            var next = Instantiate(uiCharSheet.Find("Continue"), item.transform.parent, false);
            curr.localPosition = new Vector3(0, -400, 0);
            prev.localPosition = new Vector3(-200, -400, 0);
            next.localPosition = new Vector3(200, -400, 0);
            prev.GetComponentInChildren<TextMeshProUGUI>().text = "上一页";
            next.GetComponentInChildren<TextMeshProUGUI>().text = "下一页";
            curr.GetComponentInChildren<TextMeshProUGUI>().text = "第 1 页";

            pageDisplay = curr.GetComponentInChildren<TextMeshProUGUI>();
            DestroyImmediate(curr.GetComponent<Button>());
            DestroyImmediate(prev.GetComponent<Button>());
            DestroyImmediate(next.GetComponent<Button>());

            next.gameObject.AddComponent<Button>().onClick.AddListener(delegate
            {
                int maxPage = (itemDataByType[seletctTypeId].Count + numPerPage - 1) / numPerPage;
                if (currPage == maxPage) return;

                currPage += 1;
                Refresh(seletctTypeId);
            });
            prev.gameObject.AddComponent<Button>().onClick.AddListener(delegate
            {
                if (currPage == 1) return;

                currPage -= 1;
                Refresh(seletctTypeId);
            });

            #endregion

            isOpen = false;
            seletctTypeId = 0;
            currPage = 1;

            gameObject.transform.localPosition = new Vector3(-150, 0, 0);
            DestroyImmediate(uiPack);
        }


        public void Refresh(int type)
        {
            log.LogMessage($"Refresh Type {type}");
            if (type >= menuPanel.Count)
            {
                log.LogError("Refresh Type Out of Range");
                return;
            }

            #region[Set Type Menu Panel]

            var target = menuPanel[type].transform;
            target.Find("select").gameObject.SetActive(true);
            target.Find("line").gameObject.SetActive(true);

            if (lastMenu != null && lastMenu != target.gameObject)
            {
                lastMenu.transform.Find("select").gameObject.SetActive(false);
                lastMenu.transform.Find("line").gameObject.SetActive(false);
            }
            lastMenu = target.gameObject;

            #endregion

            seletctTypeId = type;
            bagItemtype = MonoSingleton<WuLin.PlayerTeamManager>.Instance.GetItemType(type);

            #region[Set Item Grid Entry]

            int itemNum = Math.Min(numPerPage, itemDataByType[type].Count - (currPage - 1) * 50);
            for (int i = 0; i < numPerPage; i++)
            {
                var uiItem = curPageItems[i];
                if (uiItem == null) { return; }

                if (currPage - 1 == itemDataByType[type].Count / numPerPage &&
                    i > itemDataByType[type].Count % numPerPage - 1)
                {
                    uiItem.gameObject.SetActive(false);
                }
                else
                {
                    var item = itemDataByType[type][(currPage - 1) * numPerPage + i];
                    uiItem.gameObject.SetActive(true);
                    uiItem.ID = type*10000 + (currPage - 1)*numPerPage + i;
                    uiItem.Icon.sprite = item.GetIcon();
                    uiItem.Icon.gameObject.SetActive(true);
                    uiItem.Bg.gameObject.SetActive(false);
                    uiItem.SetQuality(item.GetIconRarity());
                    uiItem.Num.text = item.GetName();
                    //log.LogMessage($"{item.GetName(false)}");
                }
            }

            #endregion

            pageDisplay.text = $"第 {currPage} 页";
        }

        public static GameData.ItemData GetGameData(int id)
        {
            try  { return itemDataByType[id / 10000][id % 10000]; }
            catch { return null; }
        }
    }

    public class ItemTipPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIItem), "OnPointerEnter")]
        public static bool OnPointerEnter_PrePatch(UIItem __instance)
        {
            if (__instance.Data == null && UIItemPanel.isOpen)
            {
                try
                {
                    var tipManager = WuLin.GameFrameworks.GTSingleton<WuLin.UITipsManager>.Instance;

                    tipManager.ShowTips(UIItemPanel.GetGameData(__instance.ID), __instance.transform);
                }
                catch { return true; }
                return false;
            }

            return true;
        }
    }
}
