using HarmonyLib;
using UnityEngine;
using Hazel;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    public class KeyBoardPatch
    {
        public static void Postfix()
        {
            //左コントロールとF10同時押しで廃村
            if (Input.GetKeyDown(KeyCode.F10) && AmongUsClient.Instance.AmHost &&  AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCEvents.ForceEnd();
            }
        }
    }

    //チャットバグ対策
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ChatControllerAwakePatch
    {
        public static void Prefix()
        {
            SaveManager.chatModeType = 1;
            SaveManager.isGuest = false;
        }
        public static void Postfix(ChatController __instance)
        {
            SaveManager.chatModeType = 1;
            SaveManager.isGuest = false;
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!__instance.isActiveAndEnabled) return;
                __instance.SetVisible(false);
                new LateTask(() =>
                {
                    __instance.SetVisible(true);
                }, 0f, "AntiChatBag");
            }
        }
    }
}