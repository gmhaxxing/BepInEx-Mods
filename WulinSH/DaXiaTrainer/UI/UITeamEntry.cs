using BepInEx.Logging;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WuLin;

namespace DaXiaTrainer.UI
{
    public class UITeamEntry : MonoBehaviour
    {
        public UITeamEntry(IntPtr ptr) : base(ptr) { }
        public ManualLogSource log => BepInExLoader.log;

        public GameObject noRole;
        public GameObject isFull;
        public GameObject selected;
        public Image headIcon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI mpText;
        public Button button;

        public GameCharacterInstance character;


        private void Awake()
        {
            log.LogMessage("UITeamEntry Awake Fired");
        }

        public void Init()
        {
            noRole = transform.Find("No Role").gameObject;
            isFull = transform.Find("IsFull").gameObject;
            selected = transform.Find("IsFull/xuanzhong").gameObject;
            headIcon = transform.Find("IsFull/Head/icon").GetComponent<Image>();
            nameText = transform.Find("IsFull/Name").GetComponent<TextMeshProUGUI>();
            nameText.fontSize = 28;
            nameText.transform.localPosition = new Vector3(80, 0, 0);
            //hpText = transform.Find("IsFull/shengming/HpValue").GetComponent<TextMeshProUGUI>();
            //mpText = transform.Find("IsFull/neili/EpValue").GetComponent<TextMeshProUGUI>();
            transform.Find("IsFull/shengming").gameObject.SetActive(false);
            transform.Find("IsFull/neili").gameObject.SetActive(false);
            transform.Find("IsFull/yin").gameObject.SetActive(false);
            button = gameObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
        }

        public void SetEntry(GameCharacterInstance chara)
        {
            noRole.SetActive(chara == null);
            isFull.SetActive(chara != null);
            button.interactable = (chara != null);
            character = chara;
            if (chara == null) { return; }

            nameText.text = chara.FullName;
            headIcon.sprite = chara.GetPortrait(GameCharacterInstance.PortraitType.Small);
        }

    }
}
