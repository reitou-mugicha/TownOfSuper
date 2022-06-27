using System;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace TownOfSuper
{
    [BepInPlugin(Id, "TownOfSuper", Version)]
    public class TosPlugin : BasePlugin
    {
        public const String Id = "jp.reitou-mugicha.townofsuper";
        public const String Version = "1.1.1";

        public static ConfigEntry<bool>? debugTool { get; set; }
        public static ConfigEntry<string>? StereotypedText { get; set; }
        public Harmony Harmony = new Harmony(Id);

        public override void Load()
        {

            debugTool = Config.Bind("Client Options", "Debug Tool", false);
            StereotypedText = Config.Bind("Client Options", "StereotypedText", "TownOfSuper定型文");

            Harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public class ShowModStampPatch
    {
        public static void Postfix(ModManager __instance)
        {
            __instance.ShowModStamp();
        }
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class VersionShowerPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.text += " & <color=#4169e1>TownOfSuper</color> ver." + TosPlugin.Version; //<color=#ffddef>AZ</color> 
        }
    }
}
