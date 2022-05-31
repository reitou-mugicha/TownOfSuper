/*Source Code by TheOtherRoles GM KiyoMugi Edition & Town Of Host*/

using HarmonyLib;
using System.Linq;

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
                                                 ".chat set <text> : 定型文を設定");
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
                            }
                            Chat.SendPrivateChat("引数が不足しています");
                            Chat.SendPrivateChat("使用方法:\n.chat set <text>");
                            handled = true;
                            break;
                        case "send":
                            if(TosPlugin.StereotypedText == null) return true;
                            Chat.SendAllChat(TosPlugin.StereotypedText.Value);
                            handled = true;
                            break;
                    }
                    Chat.SendPrivateChat("使用方法が違います");
                    Chat.SendPrivateChat("使用方法:\n.chat set <text> : 定型文を設定\n" +
                                         ".chat send : 定型文を表示");
                    handled = true;
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