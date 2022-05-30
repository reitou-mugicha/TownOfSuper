using HarmonyLib;
using Hazel;

namespace TownOfSuper
{
    public enum CustomRPC
    {
        ForceEnd,
    }
    
    public class RPCEvents
    {
        public static void ForceEnd()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId)
            {
                case (byte)CustomRPC.ForceEnd:
                    RPCEvents.ForceEnd();
                    break;
            }
        }
    }
}