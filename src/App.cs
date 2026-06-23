using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

[assembly: System.Reflection.AssemblyTitle("HTML Slide Editor")]
[assembly: System.Reflection.AssemblyProduct("HTML Slide Editor")]
[assembly: System.Reflection.AssemblyCompany("everside76")]
[assembly: System.Reflection.AssemblyCopyright("MIT License")]
[assembly: System.Reflection.AssemblyVersion("1.5.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.5.0.0")]

namespace HtmlSlideEditor
{
    static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        public const string Version = "1.5.0";
        const string AppName = "HTML 보고서 에디터";
        static string AppDir;

        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbedded;

            AppDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HtmlSlideEditor");
            Directory.CreateDirectory(AppDir);

            ExtractResource("WebView2Loader.dll", Path.Combine(AppDir, "WebView2Loader.dll"));
            SetDllDirectory(AppDir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RunApp();
        }

        static void RunApp()
        {
            try
            {
                Application.Run(new MainForm(AppDir, AppName + "  v" + Version));
            }
            catch (Exception ex)
            {
                MessageBox.Show("실행 중 오류가 발생했습니다.\n\n" + ex.Message,
                    AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static Assembly ResolveEmbedded(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            string res = null;
            if (name == "Microsoft.Web.WebView2.Core") res = "Microsoft.Web.WebView2.Core.dll";
            else if (name == "Microsoft.Web.WebView2.WinForms") res = "Microsoft.Web.WebView2.WinForms.dll";
            if (res == null) return null;

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(res))
            {
                if (s == null) return null;
                using (MemoryStream ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    return Assembly.Load(ms.ToArray());
                }
            }
        }

        static void ExtractResource(string resName, string outPath)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
            {
                if (s == null) return;
                try
                {
                    using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                        s.CopyTo(fs);
                }
                catch { }
            }
        }

        public static string LoadEditorHtml()
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("editor.html"))
            using (StreamReader r = new StreamReader(s, Encoding.UTF8))
                return r.ReadToEnd();
        }
    }

    public class MainForm : Form
    {
        // 업데이트 소스 (GitHub Releases)
        const string RepoOwner = "everside76";
        const string RepoName  = "html-slide-editor";

        WebView2 web;
        string appDir;

        // 설정
        string saveMode  = "ask";   // "ask" = 저장 시 위치 선택, "folder" = 지정 폴더 자동 저장
        string saveFolder = "";

        public MainForm(string dir, string title)
        {
            appDir = dir;
            LoadCfg();

            Text = title;
            Width = 1440;
            Height = 900;
            MinimumSize = new System.Drawing.Size(1000, 640);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.FromArgb(31, 35, 48);

            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            web = new WebView2();
            web.Dock = DockStyle.Fill;
            Controls.Add(web);

            this.Load += FormLoaded;
        }

        async void FormLoaded(object sender, EventArgs e)
        {
            try
            {
                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(
                    null, Path.Combine(appDir, "WebView2Data"), null);
                await web.EnsureCoreWebView2Async(env);

                CoreWebView2Settings st = web.CoreWebView2.Settings;
                st.IsStatusBarEnabled = false;
                st.AreDefaultContextMenusEnabled = true;
                st.IsZoomControlEnabled = true;

                web.CoreWebView2.WebMessageReceived += OnWebMessage;
                web.CoreWebView2.DownloadStarting += OnDownloadStarting;

                web.CoreWebView2.NavigateToString(Program.LoadEditorHtml());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "WebView2 초기화에 실패했습니다.\n\nWebView2 런타임이 필요합니다.\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== 설정 저장/로드 =====
        string CfgFile() { return Path.Combine(appDir, "settings.json"); }

        void LoadCfg()
        {
            try
            {
                string p = CfgFile();
                if (File.Exists(p))
                {
                    var d = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(p, Encoding.UTF8)) as Dictionary<string, object>;
                    if (d != null)
                    {
                        if (d.ContainsKey("saveMode"))   saveMode   = Convert.ToString(d["saveMode"]);
                        if (d.ContainsKey("saveFolder")) saveFolder = Convert.ToString(d["saveFolder"]);
                    }
                }
            }
            catch { }
            if (saveMode != "folder") saveMode = "ask";
        }

        void SaveCfg()
        {
            try
            {
                var d = new Dictionary<string, object>();
                d["saveMode"] = saveMode;
                d["saveFolder"] = saveFolder;
                File.WriteAllText(CfgFile(), new JavaScriptSerializer().Serialize(d), Encoding.UTF8);
            }
            catch { }
        }

        // ===== JS <-> 호스트 메시지 =====
        void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg;
            try { msg = e.TryGetWebMessageAsString(); }
            catch { return; }
            if (string.IsNullOrEmpty(msg)) return;

            Dictionary<string, object> m;
            try { m = new JavaScriptSerializer().DeserializeObject(msg) as Dictionary<string, object>; }
            catch { return; }
            if (m == null || !m.ContainsKey("t")) return;

            string t = Convert.ToString(m["t"]);
            if (t == "getCfg") PostCfg();
            else if (t == "setSaveMode") { saveMode = (Convert.ToString(m["mode"]) == "folder") ? "folder" : "ask"; SaveCfg(); PostCfg(); }
            else if (t == "pickFolder") PickFolder();
            else if (t == "checkUpdate") CheckUpdate();
            else if (t == "doUpdate") DoUpdate(m.ContainsKey("url") ? Convert.ToString(m["url"]) : "");
        }

        void PostJson(string json)
        {
            try
            {
                if (web == null || web.IsDisposed) return;
                if (web.InvokeRequired)
                    web.BeginInvoke((Action)(delegate { try { web.CoreWebView2.PostWebMessageAsString(json); } catch { } }));
                else
                    web.CoreWebView2.PostWebMessageAsString(json);
            }
            catch { }
        }

        void PostCfg()
        {
            var d = new Dictionary<string, object>();
            d["t"] = "cfg";
            d["saveMode"] = saveMode;
            d["saveFolder"] = saveFolder;
            d["version"] = Program.Version;
            PostJson(new JavaScriptSerializer().Serialize(d));
        }

        void PostStatus(string msg)
        {
            var d = new Dictionary<string, object>();
            d["t"] = "updStatus"; d["msg"] = msg;
            PostJson(new JavaScriptSerializer().Serialize(d));
        }

        void PostSaved(string path)
        {
            var d = new Dictionary<string, object>();
            d["t"] = "saved"; d["path"] = path;
            PostJson(new JavaScriptSerializer().Serialize(d));
        }

        void PickFolder()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "편집본을 저장할 기본 폴더를 선택하세요";
                if (!string.IsNullOrEmpty(saveFolder) && Directory.Exists(saveFolder)) dlg.SelectedPath = saveFolder;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    saveFolder = dlg.SelectedPath;
                    saveMode = "folder";
                    SaveCfg();
                }
            }
            PostCfg();
        }

        // ===== 다운로드(편집본 저장) 가로채기 =====
        void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                string fname = Path.GetFileName(e.ResultFilePath);
                if (string.IsNullOrEmpty(fname)) fname = "report_편집본.html";

                if (saveMode == "folder" && !string.IsNullOrEmpty(saveFolder) && Directory.Exists(saveFolder))
                {
                    e.ResultFilePath = Path.Combine(saveFolder, fname);
                    e.Handled = true; // 기본 다운로드 UI 숨김
                    PostSaved(e.ResultFilePath);
                }
                else
                {
                    using (var dlg = new SaveFileDialog())
                    {
                        dlg.Title = "편집본 저장";
                        dlg.FileName = fname;
                        dlg.Filter = "HTML 파일 (*.html)|*.html|모든 파일 (*.*)|*.*";
                        dlg.OverwritePrompt = true;
                        if (!string.IsNullOrEmpty(saveFolder) && Directory.Exists(saveFolder)) dlg.InitialDirectory = saveFolder;
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            e.ResultFilePath = dlg.FileName;
                            e.Handled = true;
                            PostSaved(dlg.FileName);
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch { }
        }

        // ===== 업데이트 =====
        void CheckUpdate()
        {
            Task.Run((Action)delegate
            {
                var res = new Dictionary<string, object>();
                res["t"] = "upd";
                res["current"] = Program.Version;
                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    using (var wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Add("User-Agent", "HtmlSlideEditor-Updater");
                        wc.Headers.Add("Accept", "application/vnd.github+json");
                        string json = wc.DownloadString("https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest");
                        var d = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;

                        string tag = (d != null && d.ContainsKey("tag_name")) ? Convert.ToString(d["tag_name"]) : "";
                        string latest = tag.TrimStart('v', 'V');
                        string notes = (d != null && d.ContainsKey("body")) ? Convert.ToString(d["body"]) : "";
                        string url = "";
                        if (d != null && d.ContainsKey("assets") && d["assets"] is object[])
                        {
                            foreach (object ao in (object[])d["assets"])
                            {
                                var a = ao as Dictionary<string, object>;
                                if (a != null && a.ContainsKey("name") && a.ContainsKey("browser_download_url"))
                                {
                                    string nm = Convert.ToString(a["name"]);
                                    if (nm.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        url = Convert.ToString(a["browser_download_url"]);
                                        break;
                                    }
                                }
                            }
                        }
                        res["latest"] = latest;
                        res["notes"] = notes;
                        res["url"] = url;
                        res["hasUpdate"] = (latest.Length > 0 && IsNewer(latest, Program.Version) && url.Length > 0);
                    }
                }
                catch (Exception ex)
                {
                    res["hasUpdate"] = false;
                    res["error"] = ex.Message;
                }
                PostJson(new JavaScriptSerializer().Serialize(res));
            });
        }

        void DoUpdate(string url)
        {
            if (string.IsNullOrEmpty(url)) { PostStatus("다운로드 주소를 찾을 수 없습니다."); return; }
            PostStatus("downloading");
            Task.Run((Action)delegate
            {
                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    string tmp = Path.Combine(Path.GetTempPath(), "HtmlSlideEditor-Setup-latest.exe");
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "HtmlSlideEditor-Updater");
                        wc.DownloadFile(url, tmp);
                    }
                    this.BeginInvoke((Action)delegate
                    {
                        try { Process.Start(tmp); } catch { }
                        Application.Exit();
                    });
                }
                catch (Exception ex)
                {
                    PostStatus("업데이트 다운로드 실패: " + ex.Message);
                }
            });
        }

        static bool IsNewer(string a, string b)
        {
            try
            {
                string[] pa = a.Split('.');
                string[] pb = b.Split('.');
                int n = Math.Max(pa.Length, pb.Length);
                for (int i = 0; i < n; i++)
                {
                    int x = (i < pa.Length) ? Num(pa[i]) : 0;
                    int y = (i < pb.Length) ? Num(pb[i]) : 0;
                    if (x != y) return x > y;
                }
            }
            catch { }
            return false;
        }

        static int Num(string s)
        {
            int v = 0;
            foreach (char c in s)
            {
                if (c < '0' || c > '9') break;
                v = v * 10 + (c - '0');
            }
            return v;
        }
    }
}
