/*Source Code by Town Of Plus*/

using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TownOfSuper.Patches
{
    public class ImprovedChat
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
        public static class Delete
        {
            public static void Prefix(ChatController __instance)
            {
                if (!HudManager.Instance.Chat.IsOpen) return;
                if (SaveManager.chatModeType != 1) return;
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Backspace))
                {
                    __instance.TextArea.Clear();
                    __instance.quickChatMenu.ResetGlyphs();
                }
            }
        }
        //Control+Zで一個戻す Control+Yで一個取り消し
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
        public static class UndoAndRedo
        {
            public static List<string> List = new List<string>();
            public static int count = 1;
            public static bool UndoRedo = false;
            public static string Text = "text";
            public static void Prefix(ChatController __instance)
            {
                if (!HudManager.Instance.Chat.IsOpen) return;
                if (SaveManager.chatModeType != 1) return;
                if (__instance.TextArea.text != Text)
                {
                    Text = __instance.TextArea.text;
                    if (UndoRedo)
                    {
                        List.RemoveRange(count + 1, List.Count - (count + 1));
                        UndoRedo = false;
                    }
                    List.Add(Text);
                    count = List.Count - 1;
                }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                {
                    if (count != 0)
                    {
                        UndoRedo = true;
                        count -= 1;
                        __instance.TextArea.SetText(List[count]);
                        __instance.quickChatMenu.ResetGlyphs();
                        Text = __instance.TextArea.text;
                    }
                }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
                {
                    if (count != List.Count - 1)
                    {
                        UndoRedo = true;
                        count += 1;
                        __instance.TextArea.SetText(List[count]);
                        __instance.quickChatMenu.ResetGlyphs();
                        Text = __instance.TextArea.text;
                    }
                }
            }
        }
        //Control+Cでコピー　Control+Xでカット
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
        public static class CopyAndCut
        {
            public static void Prefix(ChatController __instance)
            {
                if (!HudManager.Instance.Chat.IsOpen) return;
                if (SaveManager.chatModeType != 1) return;
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
                {
                    GUIUtility.systemCopyBuffer = __instance.TextArea.text;
                    __instance.TextArea.SetText("");
                    __instance.quickChatMenu.ResetGlyphs();
                }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                {
                    GUIUtility.systemCopyBuffer = __instance.TextArea.text;
                }
            }
        }
        private static string text = "";
        //Control+Vでペースト
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
        public static class Paste
        {
            public static void Prefix(ChatController __instance)
            {
                if (!HudManager.Instance.Chat.IsOpen) return;
                if (SaveManager.chatModeType != 1) return;
                if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
                {
                    bool Shift = false;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) Shift = true;
                    //コピーしてあるのを調べる
                    var CopyWord = GUIUtility.systemCopyBuffer;
                    //Banされる文字の削除
                    CopyWord = CopyWord.Replace("<", "").Replace(">", "").Replace("\r", "");
                    //改行の削除
                    if (!Shift) CopyWord = CopyWord.Replace("\n", "");
                    //コピーしてある文字数
                    int CopyWordCount = CopyWord.Length;
                    //クールダウン確認
                    float num = 3f - __instance.TimeSinceLastMessage;

                    if (Shift)
                    {
                        if (num <= 0.0f)
                        {
                            //文字数判定
                            if (CopyWordCount <= 120)
                            {
                                //コマンドを対応させる
                                text = (CopyWord);
                            }
                            else
                            {
                                //文字数多いときに送る
                                __instance.AddChat(PlayerControl.LocalPlayer, "ERROR:文字数は120文字までです");
                            }
                        }
                    }
                    else
                    {
                        if (CopyWordCount + __instance.TextArea.text.Length >= 100)
                        {
                            CopyWord = CopyWord.Substring(0, 100 - __instance.TextArea.text.Length);
                        }
                        __instance.TextArea.SetText(__instance.TextArea.text + CopyWord);
                        __instance.quickChatMenu.ResetGlyphs();
                    }
                }
            }
        }

    }
}