using HarmonyLib;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public class PingTrackerPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.text += "\n<color=#4169e1>Town Of Super</color> ver." + TosPlugin.Version;
        }
    }
}