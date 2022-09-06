using HarmonyLib;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.InitializeOptions))]
    public static class GameSettingMenuPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            // Unlocks map/impostor amount changing in online (for testing on your custom servers)
            // オンラインモードで部屋を立て直さなくてもマップを変更できるように変更 SourceCode by TownOfHost
            __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
        }
    }
}