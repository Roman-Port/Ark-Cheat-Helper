using CefSharp;
using CefSharp.Wpf;
using mrousavy;
using RomanPortTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArkCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        public MainWindow()
        {
            InitializeComponent();
            StaticVars.browser = BrowserYo;

            //Set screen to fill
            MainWebWindow.WindowState = WindowState.Maximized;

            //Register the hotkeys
            var key = new HotKey(
                (ModifierKeys.None),
                Key.OemTilde,
                this,
                delegate {
                    if(StaticVars.allowShowHideNow)
                    {
                        StaticVars.allowShowHideNow = false;
                        if (isShown)
                        {
                            BeginHide();
                        }
                        else
                        {
                            BeginShow();
                        }
                        isShown = !isShown;
                    }
                    
                }
            );

            //Load dinosaurs
            StaticVars.dinos = RpTools.DeserializeObject<ArkDino[]>(File.ReadAllText("dinodata.json"));

            //Init browser
            BrowserYo.Address = AppDomain.CurrentDomain.BaseDirectory + @"browser\index.html";
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            BrowserYo.RegisterAsyncJsObject("boundAsync", new NativeJs());

            //Allow
            StaticVars.allowShowHideNow = true;
        }

        public bool isShown = false;
        

        public byte[] GetScreenshotOfWindow()
        {
            //Get a screenshot of where the window should be.
            //THIS ISN'T PERFECT! FIX LATER!
            byte[] buf;
            System.Drawing.Rectangle screenBounds = Screen.GetBounds(System.Drawing.Point.Empty);
            //Modify bounds to capture just the new area.
            int taskBarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(0, 0, screenBounds.Width / 2, screenBounds.Height / 2);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new System.Drawing.Point(screenBounds.Width / 4, (screenBounds.Height / 4) - taskBarHeight + 5), new System.Drawing.Point(0, 0), bounds.Size);
                    //g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    buf = new byte[(int)ms.Length];
                    ms.Position = 0;
                    ms.Read(buf, 0, buf.Length);
                }
            }
            return buf;
        }

        public Process GetActiveWindow()
        {
            try
            {
                var activatedHandle = GetForegroundWindow();

                Process[] processes = Process.GetProcesses();
                foreach (Process clsProcess in processes)
                {

                    if (activatedHandle == clsProcess.MainWindowHandle)
                    {
                        return clsProcess;
                    }
                }
            }
            catch { }
            return null;
        }


        public void BeginShow()
        {
            //First, capture and save a screenshot for blur effects
            byte[] screenshot = GetScreenshotOfWindow();
            NativeJs nsjs = new NativeJs();
            //Convert this into a string we can pass into an image
            string base64Screenshot = "data:image/png;base64, " + Convert.ToBase64String(screenshot);
            //Start animation on JS side
            var activeWindow = GetActiveWindow();
            bool isArkActive = false;
            if(activeWindow!=null)
            {
                isArkActive = activeWindow.ProcessName == "ShooterGame";
            }
            //If Ark is active, show the window. If not, tell the user to start it.
            if(isArkActive)
            {
                BrowserYo.ExecuteScriptAsync("BeginShow('" + base64Screenshot + "');");
                //Also, press the tab key in Ark to open the console.
                nsjs.pressKey("Tab");
                //Set the process
                StaticVars.arkWindow = activeWindow;
            } else
            {
                isShown = true;
                StaticVars.allowShowHideNow = true;
                BrowserYo.ExecuteScriptAsync("ShowArkNotInForegroundWarning();");
            }
            
            
        }

        public void BeginHide()
        {
            //Start animation on JS side
            BrowserYo.ExecuteScriptAsync("BeginHide();");
            //Get rid of the console in Ark.
            NativeJs nsjs = new NativeJs();
            nsjs.setArkActive();
            Thread.Sleep(100);
            nsjs.pressKey("Escape");
        }

    }
}
