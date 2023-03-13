using System;
using System.Collections.Generic;
using UnityEngine;
using WuLin;
using WuLin.GameFrameworks;
using BepInEx.Logging;
using UniverseLib;
using TMPro;


namespace DaXiaTrainer.UI
{
    public class UIRoleEditorPanel : MonoBehaviour
    {
        public GameObject uiLeftContent;
        public GameObject uiRightContent;
        public List<UITeamEntry> teamRoles = new();

        public static GameCharacterInstance _character;

        #region[role properties]
        private TMP_InputField _gongji;
        private TMP_InputField _qinggong;
        private TMP_InputField _quanzhang;
        private TMP_InputField _shuadao;
        private TMP_InputField _duanbing;
        private TMP_InputField _mingzhong;
        private TMP_InputField _baoji;
        private TMP_InputField _yishu;
        private TMP_InputField _anqi;
        private TMP_InputField _wuxuechangshi;

        private TMP_InputField _fangyu;
        private TMP_InputField _jiqi;
        private TMP_InputField _yujian;
        private TMP_InputField _changbing;
        private TMP_InputField _yinlv;
        private TMP_InputField _shanbi;
        private TMP_InputField _gedang;
        private TMP_InputField _dushu;
        private TMP_InputField _hubo;
        private TMP_InputField _shizhannengli;

        private TMP_InputField _bili;
        private TMP_InputField _tizhi;
        private TMP_InputField _minjie;
        private TMP_InputField _wuxing;
        private TMP_InputField _fuyuan;

        private TMP_InputField _hp;
        private TMP_InputField _mp;
        private TMP_InputField _point;
        private TMP_InputField _exp;
        private TMP_InputField _lv;

        private TMP_InputField _rende;
        private TMP_InputField _yiqi;
        private TMP_InputField _lijie;
        private TMP_InputField _xinyong;
        private TMP_InputField _zhihui;
        private TMP_InputField _yongqi;

        private TMP_InputField _coin;
        private TMP_InputField _replv;
        private TMP_InputField _repexp;

        private Dictionary<TMP_InputField, string> stringMap = new();

        #endregion

        public UIRoleEditorPanel(IntPtr ptr) : base(ptr) { }
        public ManualLogSource log => BepInExLoader.log;

        private void Awake()
        {
            log.LogMessage("UIRoleEditorPanel Awake Fired");

            foreach (var inputObj in stringMap.Keys)
            {
                inputObj.onEndEdit.AddListener(delegate (string str) {
                    OnInputFeildEdit(inputObj, str);
                });
            }

            _exp.onEndEdit.AddListener(delegate (string str)
            {
                if (!int.TryParse(str, out int value))
                    _exp.text = _exp.m_OriginalText;
                else if (_character != null) {
                    _character.m_exp = value;
                }
            });
            _lv.onEndEdit.AddListener(delegate (string str)
            {
                if (!int.TryParse(str, out int value))
                    _lv.text = _lv.m_OriginalText;
                else if (_character != null)
                {
                    _character.m_level = value;
                }
            });
            _coin.onEndEdit.AddListener(delegate (string str)
            {
                if (!long.TryParse(str, out long value))
                    _coin.text = _coin.m_OriginalText;
                else
                {
                    var inventory = MonoSingleton<PlayerTeamManager>.Instance.TeamInventory;
                    inventory.SetCurrency(CurrencyType.Coin, value * 1000);
                }
            });
        }

        public void OnInputFeildEdit(TMP_InputField inputObj, string input)
        {
            if (!Il2CppSystem.Decimal.TryParse(input, out Il2CppSystem.Decimal value)) {
                inputObj.text = inputObj.m_OriginalText;
            }
            else if (_character != null) {
                var propStr = stringMap[inputObj];
                if (!_character.m_originProps.ContainsKey(propStr))
                    _character.m_originProps.Add(propStr, 0);

                var diff = value - _character.m_originProps[propStr];
                _character.ChangeOriginProp(propStr, diff);
            }
        }

        private void OnEnable()
        {
            log.LogMessage("UIRoleEditorPanel OnEnable Fired");
            #region[Init Team Panel]

            var teamManager = MonoSingleton<PlayerTeamManager>.Instance;
            if (teamManager.GetTeamMemberByIndex(0) == null) return;

            for (int i = 0; i < 6; i++)
            {
                var teamMate = teamManager.GetTeamMemberByIndex(i);
                teamRoles[i].SetEntry(teamMate);

                teamRoles[i].button.onClick.RemoveAllListeners();

                var tmp = i;
                teamRoles[i].button.onClick.AddListener(delegate
                {
                    OnClickTeamRole(tmp);
                });
            }

            #endregion
            OnClickTeamRole(0);
        }

        public void OnClickTeamRole(int index)
        {
            for (int i = 0; i < 6; i++)
            {
                teamRoles[i].selected.SetActive(i==index);
            }

            _character = teamRoles[index].character;

            var propSource = GameCharacterInstance.FinalPropSource.Origin;
            foreach (var inputObj in stringMap.Keys)
            {
                inputObj.text= _character.GetFinalPropAsDecimal(stringMap[inputObj], propSource).ToString();
            }

            _mingzhong.text = _character.GetFinalPropAsDecimal("命中", propSource).ToString("F3");
            _baoji.text = _character.GetFinalPropAsDecimal("暴击", propSource).ToString("F3");
            _shanbi.text = _character.GetFinalPropAsDecimal("闪避", propSource).ToString("F3");
            _gedang.text = _character.GetFinalPropAsDecimal("格挡", propSource).ToString("F3");
            _hubo.text = _character.GetFinalPropAsDecimal("互搏", propSource).ToString("F3");

            _exp.text = _character.Exp.ToString();
            _lv.text = _character.Level.ToString();

            var inventory = MonoSingleton<PlayerTeamManager>.Instance.TeamInventory;
            _coin.text = (inventory.GetCurrency(CurrencyType.Coin) / 1000).ToString();
        }


        public void Init()
        {
            var uiRoleInfo = gameObject.GetComponent<UIRoleInfoPanel>();

            uiLeftContent = uiRoleInfo.uIRoleInfoByRoleInfoPanel.gameObject;
            uiRightContent = uiRoleInfo.uIRolePanelRightPanel.gameObject;

            var uiTeamPanel = gameObject.GetComponentInChildren<UITeamPanel>().gameObject;
            var roleContents = uiTeamPanel.transform.Find("RoleContent").gameObject.Children();
            for (int i = 0; i < roleContents.Count; i++)
            {
                DestroyImmediate(roleContents[i].GetComponent<UIRolePanel>());

                log.LogMessage($"TeamEntry {i} Init Done");
                var uiTeamEntry = roleContents[i].AddComponent<UITeamEntry>();
                uiTeamEntry.Init();
                teamRoles.Add(uiTeamEntry);
            }


            DestroyImmediate(uiTeamPanel.GetComponent<UITeamPanel>());
            DestroyImmediate(gameObject.GetComponentInChildren<UILeftContentByRoleInfoPanel>());
            DestroyImmediate(gameObject.GetComponentInChildren<UIRolePanelRightPanel>());
            DestroyImmediate(gameObject.GetComponent<UIRoleInfoPanel>());


            #region[Bind Role Properties]
            try
            {
                var transform = uiLeftContent.transform;

                _gongji = transform.Find("RoleInfo/gongji").GetComponentInChildren<TMP_InputField>();
                _qinggong = transform.Find("RoleInfo/qinggong").GetComponentInChildren<TMP_InputField>();
                _quanzhang = transform.Find("RoleInfo/quanzhang").GetComponentInChildren<TMP_InputField>();
                _shuadao = transform.Find("RoleInfo/shuadao").GetComponentInChildren<TMP_InputField>();
                _duanbing = transform.Find("RoleInfo/duanbing").GetComponentInChildren<TMP_InputField>();
                _mingzhong = transform.Find("RoleInfo/mingzhong").GetComponentInChildren<TMP_InputField>();
                _baoji = transform.Find("RoleInfo/baoji").GetComponentInChildren<TMP_InputField>();
                _yishu = transform.Find("RoleInfo/yishu").GetComponentInChildren<TMP_InputField>();
                _anqi = transform.Find("RoleInfo/anqi").GetComponentInChildren<TMP_InputField>();
                _wuxuechangshi = transform.Find("RoleInfo/wuxuechangshi").GetComponentInChildren<TMP_InputField>();

                _fangyu = transform.Find("RoleInfo1/fangyu").GetComponentInChildren<TMP_InputField>();
                _jiqi = transform.Find("RoleInfo1/jiqi").GetComponentInChildren<TMP_InputField>();
                _yujian = transform.Find("RoleInfo1/yujian").GetComponentInChildren<TMP_InputField>();
                _changbing = transform.Find("RoleInfo1/changbing").GetComponentInChildren<TMP_InputField>();
                _yinlv = transform.Find("RoleInfo1/yinlv").GetComponentInChildren<TMP_InputField>();
                _shanbi = transform.Find("RoleInfo1/shanbi").GetComponentInChildren<TMP_InputField>();
                _gedang = transform.Find("RoleInfo1/gedang").GetComponentInChildren<TMP_InputField>();
                _dushu = transform.Find("RoleInfo1/dushu").GetComponentInChildren<TMP_InputField>();
                _hubo = transform.Find("RoleInfo1/hubo").GetComponentInChildren<TMP_InputField>();
                _shizhannengli = transform.Find("RoleInfo1/shizhannengli").GetComponentInChildren<TMP_InputField>();

                _bili = transform.Find("RoleInfo1/bili").GetComponentInChildren<TMP_InputField>();
                _tizhi = transform.Find("RoleInfo1/tizhi").GetComponentInChildren<TMP_InputField>();
                _minjie = transform.Find("RoleInfo1/minjie").GetComponentInChildren<TMP_InputField>();
                _wuxing = transform.Find("RoleInfo1/wuxing").GetComponentInChildren<TMP_InputField>();
                _fuyuan = transform.Find("RoleInfo1/fuyuan").GetComponentInChildren<TMP_InputField>();

                _hp = transform.Find("RoleInfo/hp").GetComponentInChildren<TMP_InputField>();
                _mp = transform.Find("RoleInfo/mp").GetComponentInChildren<TMP_InputField>();
                _point = transform.Find("RoleInfo/point").GetComponentInChildren<TMP_InputField>();
                _exp = transform.Find("RoleInfo/exp").GetComponentInChildren<TMP_InputField>();
                _lv = transform.Find("RoleInfo/lv").GetComponentInChildren<TMP_InputField>();

                _rende = transform.Find("RoleInfo2/rende").GetComponentInChildren<TMP_InputField>();
                _yiqi = transform.Find("RoleInfo2/yiqi").GetComponentInChildren<TMP_InputField>();
                _lijie = transform.Find("RoleInfo2/lijie").GetComponentInChildren<TMP_InputField>();
                _xinyong = transform.Find("RoleInfo2/xinyong").GetComponentInChildren<TMP_InputField>();
                _zhihui = transform.Find("RoleInfo2/zhihui").GetComponentInChildren<TMP_InputField>();
                _yongqi = transform.Find("RoleInfo2/yongqi").GetComponentInChildren<TMP_InputField>();

                _replv = transform.Find("RoleInfo2/replv").GetComponentInChildren<TMP_InputField>();
                _repexp = transform.Find("RoleInfo2/repexp").GetComponentInChildren<TMP_InputField>();
                _coin = transform.Find("RoleInfo2/coin").GetComponentInChildren<TMP_InputField>();


                stringMap[_gongji] = "攻击";
                stringMap[_qinggong] = "轻功";
                stringMap[_quanzhang] = "拳掌";
                stringMap[_shuadao] = "耍刀";
                stringMap[_duanbing] = "短兵";
                stringMap[_anqi] = "暗器";
                stringMap[_yishu] = "医术";
                stringMap[_fangyu] = "防御";
                stringMap[_jiqi] = "集气速度";
                stringMap[_yujian] = "御剑";
                stringMap[_changbing] = "长兵";
                stringMap[_yinlv] = "乐器";
                stringMap[_dushu] = "毒术";
                stringMap[_wuxuechangshi] = "武学常识";
                stringMap[_shizhannengli] = "实战能力";
                stringMap[_mingzhong] = "命中";
                stringMap[_baoji] = "暴击";
                stringMap[_shanbi] = "闪避";
                stringMap[_gedang] = "格挡";
                stringMap[_hubo] = "互搏";
                stringMap[_hp] = "生命";
                stringMap[_mp] = "内力";
                stringMap[_point] = "冲穴点数";
                stringMap[_rende] = "仁德";
                stringMap[_yiqi] = "义气";
                stringMap[_lijie] = "礼节";
                stringMap[_xinyong] = "信用";
                stringMap[_zhihui] = "智慧";
                stringMap[_yongqi] = "勇气";
                stringMap[_bili] = "臂力";
                stringMap[_tizhi] = "体质";
                stringMap[_minjie] = "敏捷";
                stringMap[_wuxing] = "悟性";
                stringMap[_fuyuan] = "福缘";
                stringMap[_repexp] = "名声经验";
                stringMap[_replv] = "名声级别";

            }
            catch
            {
                log.LogMessage("Failed to Bind Role Properties");
            }


            DestroyImmediate(gameObject.GetComponentInChildren<UIRoleInfoByRoleInfoPanel>());
            #endregion
        }
    }
}
