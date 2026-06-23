using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace HtmlSlideEditor
{
    static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        const string AppName = "HTML 보고서 에디터";
        static string AppDir;

        [STAThread]
        static void Main()
        {
            // 내장 관리 DLL(Core/WinForms)을 메모리에서 로드
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbedded;

            AppDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HtmlSlideEditor");
            Directory.CreateDirectory(AppDir);

            // 네이티브 로더(WebView2Loader.dll)는 디스크에 있어야 하므로 추출
            ExtractResource("WebView2Loader.dll", Path.Combine(AppDir, "WebView2Loader.dll"));
            SetDllDirectory(AppDir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RunApp();
        }

        // WebView2 타입을 참조하는 코드는 별도 메서드에 두어, 위 리졸버 등록 이후에 JIT 되게 함
        static void RunApp()
        {
            try
            {
                Application.Run(new MainForm(AppDir, AppName));
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
                catch
                {
                    // 이미 사용 중이면(앱 2개 실행 등) 기존 파일을 그대로 사용
                }
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
        WebView2 web;
        string appDir;

        public MainForm(string dir, string title)
        {
            appDir = dir;
            Text = title;
            Width = 1440;
            Height = 900;
            MinimumSize = new System.Drawing.Size(1000, 640);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.FromArgb(31, 35, 48);

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

                web.CoreWebView2.NavigateToString(Program.LoadEditorHtml());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "WebView2 초기화에 실패했습니다.\n\nWebView2 런타임이 필요합니다.\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
