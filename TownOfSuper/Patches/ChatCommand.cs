/*Source Code by TheOtherRoles GM KiyoMugi Edition & Town Of Host & Town Of Plus. Thank you*/

using HarmonyLib;
using System.Linq;
using Hazel;

namespace TownOfSuper.Patches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public class ChatCommand
    {
        public static bool Prefix(ChatController __instance)
        {
            string text = __instance.TextArea.text;
            bool handled = false;
            string[] args = text.Split(' ');

            switch(args[0])
            {
                case ".kick":
                    string kickPlayerName = args[1];
                    PlayerControl kickTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(kickPlayerName));
                    if(AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                    {
                        if(kickTarget is null)
                        {
                            Chat.SendPrivateChat("プレイヤーが存在しない、または引数が間違っています");
                            Chat.SendPrivateChat("使用方法: \n.kick <PlayerName>");
                            handled = true;
                            return true;
                        }

                        var client = AmongUsClient.Instance.GetClient(kickTarget.OwnerId);
                        if(client != null)
                        {
                            AmongUsClient.Instance.KickPlayer(client.Id, false);
                            Chat.SendAllChat($"{client.PlayerName}をキックしました");
                            handled = false;
                        }
                    }
                    break;
                case ".ban":
                    string banPlayerName = args[1];
                    PlayerControl banTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(banPlayerName));
                    if(AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                    {
                        if(banTarget is null)
                        {
                            Chat.SendPrivateChat("プレイヤーが存在しない、または引数が間違っています");
                            Chat.SendPrivateChat("使用方法: \n.ban <PlayerName>");
                            handled = true;
                            return false;
                        }

                        var client = AmongUsClient.Instance.GetClient(banTarget.OwnerId);
                        if(client != null)
                        {
                            AmongUsClient.Instance.KickPlayer(client.Id, false);
                            Chat.SendAllChat($"{client.PlayerName}をバンしました");
                            handled = true;
                        }
                    }
                    break;
                case ".help":
                case ".h":
                    switch(args[1].ToLower()) //.helpの第一引数
                    {
                        case "commands":
                        case "cm":
                            Chat.SendPrivateChat("コマンドヘルプ: \n.help [commands, functions] : ヘルプを表示する\n" +
                                                 ".kick <PlayerName> : プレイヤーをキックする\n" +
                                                 ".ban <PlayerName> : プレイヤーをバンする\n" +
                                                 ".chat send : 定型文を表示\n" +
                                                 ".chat set <text> : 定型文を設定\n" +
                                                 ".kill <PlayerName> : プレイヤーをキルする(フリープレイのみ)\n" +
                                                 ".revive <PlayerName> : プレイヤーを復活させる(フリープレイのみ)\n" +
                                                 ".tp <PlayerName> : プレイヤーをテレポートする(フリープレイのみ)\n" +
                                                 ".tpme <PlayerName> : プレイヤーを自分にテレポートする(フリープレイのみ)");
                            handled = true;
                            break;

                        case "functions":
                        case "fc":
                            Chat.SendPrivateChat("機能ヘルプ: \nF10 : 廃村\n" +
                                                 "F1 : チャットバグ修正\n" +
                                                 "CTRL + K : 部屋コードコピー\n" +
                                                 "CTRL + A : 全選択\n" +
                                                 "CTRL + Z : 元に戻す\n" +
                                                 "CTRL + Y : やり直し\n" +
                                                 "CTRL + X : 切り取り\n" +
                                                 "SHIFT + BackSpace : 全削除");
                            handled = true;
                        break;
                    }
                    Chat.SendPrivateChat(".help [commands, functions] : ヘルプを表示");
                    handled = true;
                    break;
                case ".chat":
                    switch(args[1])
                    {
                        case "set":
                            if(TosPlugin.StereotypedText == null) return true;
                            if(args[2] != null)
                            {
                                TosPlugin.StereotypedText.SetSerializedValue(args[2]);
                                Chat.SendPrivateChat($"定型文を\"{args[2]}\"に設定しました");
                                handled = true;
                                break;
                            } else if(args[2] == null) {
                                Chat.SendPrivateChat("引数が不足しています");
                                Chat.SendPrivateChat("使用方法:\n.chat set <text>");
                                handled = true;
                                break;
                            }
                            break;
                        case "send":
                            if(TosPlugin.StereotypedText == null) return true;
                            Chat.SendAllChat(TosPlugin.StereotypedText.Value);
                            handled = true;
                            break;
                        case "":
                            Chat.SendPrivateChat("使用方法が違います");
                            Chat.SendPrivateChat("使用方法:\n.chat set <text> : 定型文を設定\n" +
                                                 ".chat send : 定型文を表示");
                            handled = true;
                            break;
                    }
                    break;
                case ".kill":
                    if(TosPlugin.debugTool is not null && TosPlugin.debugTool.Value || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        string killPlayerName = args[1];
                        PlayerControl killTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(killPlayerName));
                        if(killTarget != null)
                        {
                            killTarget.MurderPlayer(killTarget);
                            Chat.SendPrivateChat($"{killTarget.Data.PlayerName}をキルしました");
                            handled = true;
                        } else {
                            Chat.SendPrivateChat($"{killPlayerName}は存在しません");
                            Chat.SendPrivateChat("使用方法: \n.kill <PlayerName>");
                            handled = true;
                        }
                    }
                    break;
                case ".revive":
                    if(TosPlugin.debugTool is not null && TosPlugin.debugTool.Value || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        string revivePlayerName = args[1];
                        PlayerControl reviveTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(revivePlayerName));
                        if(reviveTarget != null)
                        {
                            reviveTarget.Revive();
                            Chat.SendPrivateChat($"{reviveTarget.Data.PlayerName}を復活しました");
                            handled = true;
                        } else {
                            Chat.SendPrivateChat($"{revivePlayerName}は存在しません");
                            Chat.SendPrivateChat("使用方法: \n.revive <PlayerName>");
                            handled = true;
                        }
                    }
                    break;
                case ".tp":
                    if(TosPlugin.debugTool is not null && TosPlugin.debugTool.Value || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        string tpPlayerName = args[1];
                        PlayerControl tpTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(tpPlayerName));
                        if(tpTarget != null)
                        {
                            PlayerControl.LocalPlayer.transform.position = tpTarget.transform.position;
                            Chat.SendPrivateChat($"{PlayerControl.LocalPlayer.Data.PlayerName}を{tpTarget.Data.PlayerName}をテレポートしました");
                            handled = true;
                        } else {
                            Chat.SendPrivateChat($"{tpPlayerName}は存在しません");
                            Chat.SendPrivateChat("使用方法: \n.tp <PlayerName>");
                            handled = true;
                        }
                    }
                    break;
                case ".tpme":
                    if(TosPlugin.debugTool is not null && TosPlugin.debugTool.Value || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        string tpmePlayerName = args[1];
                        PlayerControl tpmeTarget = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(tpmePlayerName));
                        if(tpmeTarget != null)
                        {
                            tpmeTarget.transform.position = PlayerControl.LocalPlayer.transform.position;
                            Chat.SendPrivateChat($"{tpmeTarget.Data.PlayerName}を自分にテレポートしました");
                            handled = true;
                        } else {
                            Chat.SendPrivateChat($"{tpmePlayerName}は存在しません");
                            Chat.SendPrivateChat("使用方法: \n.tpme <PlayerName>");
                            handled = true;
                        }
                    }
                    break;
            }
            
            if (handled)
            {
                __instance.TextArea.Clear();
                __instance.quickChatMenu.ResetGlyphs();
            }
            return !handled;
        }
    }

    /*[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public class ChatUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            writer.Write("msg");
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }*/

    public class Chat
    {
        public static void SendPrivateChat(string text)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "[Private] \n" + text);
        }

        public static void SendAllChat(string text)
        {
            PlayerControl.LocalPlayer.RpcSendChat(text);
        }
    }
}