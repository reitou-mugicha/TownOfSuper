using HarmonyLib;
using UnityEngine;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class StartPatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1; //一人で開始できる
        }
    }
    
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class FastStartPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if(Input.GetKeyDown(KeyCode.LeftShift)) //FastStart
            {
                __instance.countDownTimer = 0;
            }

            if(Input.GetKeyDown(KeyCode.R)) //Reset CountDown
            {
                __instance.ResetStartState();
            }
        }
    }
}