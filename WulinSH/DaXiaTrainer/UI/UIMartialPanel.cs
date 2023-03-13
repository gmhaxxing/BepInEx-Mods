using BepInEx.Logging;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuLin;
using WuLin.GameFrameworks;
using UniverseLib;

namespace DaXiaTrainer.UI
{
    public class  UIMartialPanel: MonoBehaviour
    {
        public UIMartialPanel(IntPtr ptr) : base(ptr) { }
        public ManualLogSource log => BepInExLoader.log;

        private Transform learnedPanel;
        private Transform addMatrialPanel;

        private GameCharacterInstance _character => UIRoleEditorPanel._character;

        private void Awake()
        {

        }

        private void OnEnable()
        {
            if (_character == null) return;
            var learned = _character.kungfuInstances;

            for (int i = 0; i < learnedPanel.childCount; i++)
            {
                var entry = learnedPanel.GetChild(i).GetComponent<UILearnedSkillPanel>();
                var button = entry.transform.Find("Delete").GetComponent<Button>();
                if (i < learned.Count) {
                    var martial = learned[i];
                    entry.icon.GetComponent<UISkill>().data = martial;
                    entry.Name.GetComponent<TextMeshProUGUI>().text = martial.KungfuNameWithColor;
                    entry.Bg.sprite = martial.GetRarityBg();
                    entry.icon.GetComponent<Image>().sprite = martial.GetIcon();
                    button.onClick.RemoveAllListeners();
                    var tmp = i;
                    button.onClick.AddListener(delegate
                    {
                        learned.RemoveAt(tmp);
                        if (_character.ActivedInternalKungfu == martial)
                        {
                            foreach (var buffstr in martial.Templete.ActiveInternalBuffs)
                            {
                                var buff = _character.buffLib.TryGetBuff(buffstr);
                                if (buff != null)
                                {
                                    log.LogMessage($"Remove Buff {buffstr}");
                                    _character.buffLib.TryRemoveBuff(buff);
                                }
                            }
                            _character.SetActiveInternalKungfu(null);
                        }
                        if (!string.IsNullOrEmpty(martial.Templete.LearntBuff))
                        {
                            var buff = _character.buffLib.TryGetBuff(martial.Templete.LearntBuff);
                            log.LogMessage($"Get Buff {martial.Templete.LearntBuff}");
                            if (buff != null)
                            {
                                log.LogMessage("Remove Buff");
                                _character.buffLib.TryRemoveBuff(buff);
                            }
                        }
                        
                        _character.ComputeKungfuProp();
                        OnEnable();
                    });
                }

                entry.Bg.gameObject.SetActive(i < learned.Count);
                entry.icon.gameObject.SetActive(i < learned.Count);
                entry.Name.gameObject.SetActive(i < learned.Count);
                button.gameObject.SetActive(i < learned.Count);
            }
        }


        public void Init()
        {
            DestroyImmediate(transform.Find("MartialArts/UniqueSkill").gameObject);
            DestroyImmediate(transform.Find("SelectPanel").gameObject);
            DestroyImmediate(transform.Find("GoodsPanel").gameObject);
            transform.localPosition = new Vector3(-170.0f, 0.0f, 0.0f);
            addMatrialPanel = transform.Find("SetUp").RemoveAllChilren();
            learnedPanel = gameObject.GetComponent<UIKongfuPanel>().LearnedSkillPanel;
            DestroyImmediate(gameObject.GetComponent<UIKongfuPanel>());
            Instantiate(learnedPanel.GetChild(0), learnedPanel, false);
            Instantiate(learnedPanel.GetChild(0), learnedPanel, false);
            Instantiate(learnedPanel.GetChild(0), learnedPanel, false);
            Instantiate(learnedPanel.GetChild(0), learnedPanel, false);

            var deleteButton = UiSingletonPrefab<UIMenuPanel>.Instance.Esc.transform;

            foreach (var entry in learnedPanel.gameObject.Children())
            {
                var button = Instantiate(deleteButton, entry.transform, false);
                DestroyImmediate(button.GetComponent<EscRelateButton>());
                button.name = "Delete";
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
                button.localPosition = new Vector3(160, 0, 0);
            }
        }
    }
}
