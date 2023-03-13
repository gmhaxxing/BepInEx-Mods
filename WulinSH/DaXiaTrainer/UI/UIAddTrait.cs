using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using TMPro;
using WuLin;
using GameData;
using WuLin.GameFrameworks;


namespace DaXiaTrainer.UI
{
    public class UIAddTrait : MonoBehaviour
    {
        public ManualLogSource log => BepInExLoader.log;
        public UIAddTrait(IntPtr ptr) : base(ptr) { }

        private ScrollRect scrollView;
        private GameObject _selected = null;

        private GameCharacterInstance _character => UIRoleEditorPanel._character;

        private void Awake()
        {
            var children = scrollView.content.gameObject.Children();
            foreach (var entry in children)
            {
                var trait = entry.GetComponent<UIFeatures>().data;
                try
                {
                    //log.LogMessage(trait.GetName());
                    entry.GetComponentInChildren<TextMeshProUGUI>().text = trait.GetName();
                }
                catch { }
            }
        }

        private void OnEnable()
        {
            scrollView.verticalNormalizedPosition = 1;
            if (_selected != null) _selected.transform.Find("Selected").gameObject.SetActive(false);
            _selected = scrollView.content.GetChild(0).gameObject;
            _selected.transform.Find("Selected").gameObject.SetActive(true);

            var children = scrollView.content.gameObject.Children();
            if (children[0].GetComponentInChildren<TextMeshProUGUI>().text == "猫猫")
            {
                foreach (var entry in children)
                {
                    var trait = entry.GetComponent<UIFeatures>().data;
                    try
                    {
                        entry.GetComponentInChildren<TextMeshProUGUI>().text = trait.GetName();
                    }
                    catch { }
                }
            }
        }

        public void Init()
        {
            var panel = gameObject.GetComponent<UIRolePanelRightPanel>();

            var traitDB = BaseDataClass.GetGameData<GameData.TraitDataScriptObject>().data;

            var entryTemp = transform.Find("Item").gameObject;
            scrollView = transform.Find("Scroll View").GetComponent<ScrollRect>();
            scrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(530, 650);
            scrollView.transform.localPosition = new Vector3(0, 0, 0);
            scrollView.inertia = true;
            scrollView.movementType = ScrollRect.MovementType.Elastic;
            scrollView.horizontal = false;
            var content = scrollView.content;

            var playerCreate = UiSingletonPrefab<PlayerCreateUI>.Instance.transform;
            var selectTemp = playerCreate.Find("CharSheet/SelectableTraits/SelectableTrait/Selected");

            var traits = new List<TraitData>();
            foreach(var trait in traitDB.Values){
                traits.Add(trait);
            }
            traits.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));

            foreach (var trait in traits)
            {
                var entry = Instantiate(entryTemp, content, false);
                entry.GetComponent<UIFeatures>().data = trait;
                DestroyImmediate(entry.GetComponent<UIPackBuff>());
                var img = entry.transform.Find("img").GetComponent<Image>();
                switch (trait.Rarity)
                {
                    case 0:
                    case 1:
                        img.sprite = panel.green;
                        break;
                    case 2:
                        img.sprite = panel.purple;
                        break;
                    case 3:
                        img.sprite = panel.orange;
                        break;
                }

                var selectedmark = Instantiate(selectTemp, entry.transform, false);
                selectedmark.name = "Selected";
                selectedmark.gameObject.SetActive(false);
                var entryButton = entry.GetComponent<Button>();
                entryButton.onClick.AddListener(delegate
                {
                    _selected.transform.Find("Selected").gameObject.SetActive(false);
                    selectedmark.gameObject.SetActive(true);
                    _selected = entry;
                });
            }

            var buttonTemp = UiSingletonPrefab<EscUI>.Instance.main_Resume.gameObject;
            var button = Instantiate(buttonTemp, transform, false);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);
            button.name = "AddTrait";
            button.transform.localPosition = new Vector3(-125, -370, 0);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "添加";
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (_character == null) return;

                Il2CppSystem.Collections.Generic.List<TraitData> traits = new();
                var traitDB = BaseDataClass.GetGameData<GameData.TraitDataScriptObject>().data;
                foreach (var key in _character.Traits)
                {
                    if (!traitDB[key].GetName(false).Contains("DefaultBgTrait"))
                    {
                        traits.Add(traitDB[key]);
                    }
                }

                var newTrait = _selected.GetComponent<UIFeatures>().data;
                traits.Add(newTrait);

                _character.SetTraits(traits);
            });

            var buttonRm = Instantiate(buttonTemp, transform, false);
            buttonRm.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);
            buttonRm.transform.localPosition = new Vector3(125, -370, 0);
            buttonRm.name = "RemoveTrait";
            buttonRm.GetComponentInChildren<TextMeshProUGUI>().text = "删除";
            buttonRm.GetComponent<Button>().onClick.RemoveAllListeners();
            buttonRm.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (_character == null) return;

                Il2CppSystem.Collections.Generic.List<TraitData> traits = new();
                var traitDB = BaseDataClass.GetGameData<TraitDataScriptObject>().data;

                foreach (var key in _character.Traits)
                {
                    if (!traitDB[key].GetName(false).Contains("DefaultBgTrait"))
                    {
                        traits.Add(traitDB[key]);
                    }
                }
                var traitSelected = _selected.GetComponent<UIFeatures>().data;
                traits.Remove(traitSelected);
                _character.SetTraits(traits);
            });


        }

    }
}
