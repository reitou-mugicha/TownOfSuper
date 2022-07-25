using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Twitch;

namespace TownOfSuper
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class ModUpdaterButton 
    {
        private static void Prefix(MainMenuManager __instance) {
            ModUpdater.LaunchUpdater();
            if (ModUpdater.hasUpdate)
            {
                ModUpdater.LaunchUpdater();
                if (!ModUpdater.hasUpdate) return;
                var template = GameObject.Find("ExitGameButton");
                if (template == null) return;

                var button = UnityEngine.Object.Instantiate(template, null);
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 1.8f, button.transform.localPosition.z);

                PassiveButton passiveButton = button.GetComponent<PassiveButton>();
                SpriteRenderer buttonSprite = button.GetComponent<SpriteRenderer>();
                passiveButton.OnClick = new Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

                var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    text.SetText("TownOfSuperを\nアップデート");
                })));

                buttonSprite.color = text.color = Color.red;
                passiveButton.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    buttonSprite.color = text.color = Color.red;
                });

                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                ModUpdater.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
                ModUpdater.InfoPopup.TextAreaTMP.fontSize *= 0.7f;
                ModUpdater.InfoPopup.TextAreaTMP.enableAutoSizing = false;

                void onClick()
                {
                    ModUpdater.ExecuteUpdate();
                    button.SetActive(false);
                }
            }
        }
    }

    public class ModUpdater
    { 
        public static bool running = false;
        public static bool hasUpdate = false;
        public static string? updateURI = null;
        private static Task? updateTask = null;
        public static string announcement = "";
        public static GenericPopup? InfoPopup;

        public static void LaunchUpdater()
        {
            if (running) return;
            running = true;
            checkForUpdate().GetAwaiter().GetResult();
            clearOldVersions();
            if (hasUpdate || TosPlugin.ShowPopUpVersion!.Value != TosPlugin.Version)
            {
                DestroyableSingleton<MainMenuManager>.Instance.Announcement.gameObject.SetActive(true);
                TosPlugin.ShowPopUpVersion!.Value = TosPlugin.Version;
            }
        }

        public static void ExecuteUpdate()
        {
            string info = $"TownOfSuperを\nアップデートをしています...";
            InfoPopup!.Show(info); // Show originally
            if (updateTask == null) {
                if (updateURI != null) updateTask = downloadUpdate();
                else  info = "手動でアップデートをしてください";
            } else {
                info = "更新中です...";
            }

            InfoPopup.StartCoroutine(Effects.Lerp(0.01f, new System.Action<float>((p) => { setPopupText(info); })));
        }
        
        public static void clearOldVersions()
        {
            try {
                DirectoryInfo d = new (Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
                string[] files = d.GetFiles("*.old").Select(x => x.FullName).ToArray(); // Getting old versions
                foreach (string f in files)
                    File.Delete(f);
            } catch(System.Exception exp) {
                System.Console.WriteLine("旧バージョンのファイルを消せませんでした:\n" + exp);
            }
        }

        public static async Task<bool> checkForUpdate()
        {
            try
            {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "Town Of Super ModUpdater");
                var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/reitou-mugicha/TownOfSuper/releases/latest"), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    return false;
                }
                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);

                string tagname = data["tag_name"]?.ToString()!;
                if (tagname == null)
                {
                    return false; // Something went wrong
                }

                string changeLog = $"<size=4>TownOfSuper {TosPlugin.Version}</size>\n";
                changeLog += "======内容======\n";
                changeLog += data["body"]?.ToString();
                if (changeLog != null) announcement = changeLog;
                // check version
                System.Version ver = System.Version.Parse(tagname.TrimAll("v"));
                int diff = TosPlugin.Version.CompareTo(ver);
                if (diff < 0)
                { // Update required
                    hasUpdate = true;
                    JToken assets = data["assets"];
                    if (!assets.HasValues)
                        return false;

                    for (JToken current = assets.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString()!;
                        if (browser_download_url != null && current["content_type"] != null)
                        {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith(".dll"))
                            {
                                updateURI = browser_download_url;
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static async Task<bool> downloadUpdate()
        {
            try {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TownOfSuper ModUpdater");
                var response = await http.GetAsync(new System.Uri(updateURI), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null) {
                    return false;
                }
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                string fullname = System.Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                using (var responseStream = await response.Content.ReadAsStreamAsync()) {
                    using (var fileStream = File.Create(fullname)) { // probably want to have proper name here
                        responseStream.CopyTo(fileStream); 
                    }
                }
                showPopup($"TownOfSuperのアップデートが完了しました。\nゲームを再起動してください。");
                return true;
            } catch {
            }
            showPopup("更新に失敗しました");
            return false;
        }

        private static void showPopup(string message) {
            setPopupText(message);
            InfoPopup!.gameObject.SetActive(true);
        }

        public static void setPopupText(string message) {
            if (InfoPopup == null)
                return;
            if (InfoPopup.TextAreaTMP != null) {
                InfoPopup.TextAreaTMP.text = message;
            }
        }
    }
}