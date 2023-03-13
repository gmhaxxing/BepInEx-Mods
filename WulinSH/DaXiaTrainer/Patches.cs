using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using WuLin;
using GameData;


namespace DaXiaTrainer
{
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCreateManager), "GenerateSelectabelTraits")]
        public static bool GenerateSelectabelTraits_PrePatch(PlayerCreateManager __instance)
        {
            if (__instance.selectTraitIndexesFromSelectable == null)
                return true;

            __instance.selectableTraitDatas = new List<TraitData>(10);
            var backgroundRate = GameConfig.Instance.GetConfigValue("MainCharCreate_BackgroundRate");
            
            var traitDatas = BaseDataClass.GetGameData<TraitDataScriptObject>().TraitData;
            foreach (var traitData in traitDatas)
            {
                if (traitData.Rarity == 0 && traitData.GenRate != 0) traitData.GenRate = 2;
                else if (traitData.Rarity > 1 && traitData.GenRate > 0 && traitData.GenRate < 10) traitData.GenRate *= 15;
            }

            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameItemInstance), "RelationIncreaseForGift", MethodType.Getter)]
        public static void RelationIncreaseForGift_PostPatch(ref int __result)
        {
            __result *= 5;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradingWithNpcManager), "StartTrading")]
        public static void StartTrading_PostPatch(GameCharacterInstance other)
        {
            var playerItemValue = TradingWithNpcManager.playerItemValue;
            var npcItemValue = TradingWithNpcManager.npcItemValue;

            if (playerItemValue <= npcItemValue)
            {
                TradingWithNpcManager.playerItemValue = npcItemValue * 5;
                TradingWithNpcManager.playerFondItemValue = TradingWithNpcManager.npcFondItemValue * 5;
            }
        }


        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(PlayerCreateManager), "GenerateSelectabelTraits")]
        //public static void ConfirmOrder_Patch(UIBuildShipCtrl __instance)
        //{
        //    try
        //    {
        //        Ship ship = TemplateManager.GetShip(__instance._Model_k__BackingField.shipTid);
        //        StringBuilder stringBuilder = new StringBuilder(128);
        //        List<int> shipEntryList = __instance._Model_k__BackingField.ShipEntryList;
        //        if (shipEntryList != null && shipEntryList._size > 0)
        //        {
        //            List<int>.Enumerator enumerator = shipEntryList.GetEnumerator();
        //            while (enumerator.MoveNext())
        //            {
        //                ShipBuff shipBuff = TemplateManager.GetShipBuff(enumerator.Current);
        //                stringBuilder.AppendFormat("{0}: {1}\n", TextLibUtils.ColorText(TextLibUtils.Text(shipBuff.name), "#E47833"), TextLibUtils.Text(shipBuff.desc));
        //            }
        //        }
        //        else
        //        {
        //            stringBuilder.AppendLine("无词条");
        //        }
        //        Singleton<UIPopupsCtrl>.Instance?.Show(stringBuilder.ToString(), (Il2CppSystem.Action)null, (string)null, TextLibUtils.Text(ship.name));
        //    }
        //    catch (System.ArgumentException ex)
        //    {
        //        System.Console.WriteLine("Patch error!>>UIBuildShipCtrl.ConfirmOrder(): " + ex.Message);
        //    }
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ResourceManager), "GetAssetAsync")]
        //public static bool GetAssetAsync_PrePatch(string path)
        //{
        //    //
        //    //var instance = WuLin.GameFrameworks.GTSingleton<WuLin.GameFrameworks.ResourceManager>.Instance;
        //    //var obj = instance.GetAsset<Texture2D>("UIPrefab/UISetting", true);
        //    Debug.Log($"{path}");
        //    Plugin.Log.LogInfo($"{path}");
        //    return true;
        //}


    }

    //[HarmonyPatch] // at least one Harmony annotation makes Harmony not skip this patch class when calling PatchAll()
    //class MyPatch
    //{
    //    // here, inside the patch class, you can place the auxilary patch methods
    //    // for example TargetMethod:

    //    static MethodBase TargetMethod()
    //    {
    //        return AccessTools.Method(typeof(ResourceManager), "GetAsset").MakeGenericMethod(typeof(TextAsset));
    //    }

    //    // your patches
    //    static void Prefix()
    //    {
    //        Debug.Log($"1");
    //        Plugin.Log.LogInfo($"1");
    //    }
    //}
}
