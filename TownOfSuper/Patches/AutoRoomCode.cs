using HarmonyLib;
using UnityEngine;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class AutoRoomCode
    {
        public static void Postfix(GameStartManager __instance)
        {
            Chat.SendPrivateChat($"部屋コード{InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId)}をコピーしました");
            GUIUtility.systemCopyBuffer = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        }
    }
}