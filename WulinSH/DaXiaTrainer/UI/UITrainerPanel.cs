using BepInEx.Logging;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.Utility;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace DaXiaTrainer.UI
{
    public class UITrainerPanel : MonoBehaviour
    {
        public enum Panel_Type
        {
            Role,
            Item,
            Misc
        }

        public ToggleGroup tittleToggleGroup;
        public int tittleID;
        public Il2CppReferenceArray<GameObject> panels = new(4);

        public ManualLogSource log => BepInExLoader.log;
        public UITrainerPanel(IntPtr ptr) : base(ptr) { }

        private void Awake()
        {
            
            for (int i = 0; i < 4; i++)
            {
                var tmp = i;
                var tittleToggle = tittleToggleGroup.transform.GetChild(i).GetComponent<Toggle>();
                tittleToggle.onValueChanged.AddListener(delegate (bool isOn)
                {
                    log.LogMessage($"Toggle {tmp} is {isOn}");
                    if(isOn) { OnToggleClick(tmp); }
                });
            }
            tittleID = 0;
        }

        private void OnEnable()
        {
            foreach(var panel in panels)
            {
                if (!panel.IsNullOrDestroyed())
                    panel.SetActive(false);
            }

            Time.timeScale = 0;
            OnToggleClick(tittleID);
        }


        private void OnDisable()
        {
            Time.timeScale = 1;
        }

        private void OnToggleClick(int index)
        {
            log.LogMessage($"OnToggleClick {index}");

            if (!panels[index].IsNullOrDestroyed())
                panels[index].SetActive(true);

            if (index != tittleID)
            {
                tittleToggleGroup.transform.GetChild(tittleID).GetComponent<Toggle>().isOn = false;
                if (!panels[tittleID].IsNullOrDestroyed())
                    panels[tittleID].SetActive(false);
            }
            tittleToggleGroup.transform.GetChild(index).GetComponent<Toggle>().isOn = true;
            tittleID = index;
        }

        public void Init()
        {
            var uiMenuPanel = gameObject.GetComponent<WuLin.UIMenuPanel>();
            tittleToggleGroup = uiMenuPanel.Tittle;
            panels[0] = transform.Find("RolePanel")?.gameObject;
            panels[1] = transform.Find("MartialPanel")?.gameObject;
            panels[2] = transform.Find("ItemPanel")?.gameObject;
            panels[3] = transform.Find("MiscPanel")?.gameObject;

            DestroyImmediate(uiMenuPanel);
        }

    }
}
