global using UnhollowerBaseLib;
global using UnhollowerBaseLib.Attributes;
global using UnhollowerRuntimeLib;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace TownOfSuper
{
    [BepInPlugin(Id, "TownOfSuper", Version)]
    public class TosPlugin : BasePlugin
    {
        #nullable enable
        public const String Id = "jp.reitou-mugicha.townofsuper";
        public const String Version = "1.2.2";

        public static ConfigEntry<bool>? debugTool { get; set; }
        public static ConfigEntry<string>? StereotypedText { get; set; }
        public static ConfigEntry<string>? ShowPopUpVersion { get; set; }
        public static ConfigEntry<string>? Ip { get; set; }
        public static ConfigEntry<ushort>? Port { get; set; }
        public Harmony Harmony = new Harmony(Id);

        public static IRegionInfo[]? defaultRegions;
        public static void UpdateRegions()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = defaultRegions!;

            var CustomRegion = new DnsRegionInfo(Ip!.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port!.Value, false);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public override void Load()
        {

            debugTool = Config.Bind("Client Options", "Debug Tool", false);
            StereotypedText = Config.Bind("Client Options", "StereotypedText", "TownOfSuper定型文");
            ShowPopUpVersion = Config.Bind("Update", "Show PopUp", "0");
            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);

            defaultRegions = ServerManager.DefaultRegions;

            UpdateRegions();

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
