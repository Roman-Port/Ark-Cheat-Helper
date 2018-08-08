using CefSharp;
using HtmlAgilityPack;
using RomanPortTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ArkCommander
{
    public class NativeJs
    {
        [DllImport("user32.dll")]
        static extern ushort MapVirtualKey(int wCode, int wMapType);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        public void setKey(string keyName, string statusName)
        {
            //Parse key name and status
            bool down = statusName == "down";
            Keys key = (Keys)Enum.Parse(typeof(Keys), keyName);
            if(down)
            {
                keybd_event((byte)key, 0, 0x0001 | 0, 0);
            } else
            {
                keybd_event((byte)key, 0, 0x0001 | 0x0002, 0);
            }
        }

        public void pressKey(string keyName )
        {
            setKey(keyName, "down");
            //Wait 50 ms.
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //Wait
                Thread.Sleep(100);
                //Key up
                setKey(keyName, "up");
            }).Start();
        }

        public void pressKey(string keyName, int timeMs)
        {
            setKey(keyName, "down");
            //Wait 50 ms.
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //Wait
                Thread.Sleep(timeMs);
                //Key up
                setKey(keyName, "up");
            }).Start();
        }

        public void typeMessage(string message)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //Press each
                foreach (char c in message)
                {
                    string keyName = c.ToString().ToUpper();
                    pressKey(keyName);
                    Thread.Sleep(140);
                    
                }
            }).Start();
        }

        public void setArkActive()
        {
            //Make Ark active.
            var handle = StaticVars.arkWindow.MainWindowHandle;
            SetForegroundWindow(handle);
        }

        [STAThread]
        public void sendArkCommand(string cmd)
        {
            //This assumes Ark is already open.
            //First, convert the base 64 encoded string
            cmd = Encoding.UTF8.GetString(Convert.FromBase64String(WebUtility.UrlDecode(cmd)));
            //Copy the text to the clipboard using an STA thread
            Thread thread = new Thread(() => System.Windows.Clipboard.SetText(cmd));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
            //Save current active window
            var window = GetForegroundWindow();
            //Switch to Ark
            this.setArkActive();
            //Wait a moment
            Thread.Sleep(30);
            //Do paste key combo to paste
            SendKeys.SendWait("^V");
            //Wait
            Thread.Sleep(30);
            //Press enter.
            this.pressKey("Enter",50);
            //Wait
            Thread.Sleep(100);
            //Show console
            this.pressKey("Tab", 50);
            //Wait
            Thread.Sleep(30);
            //Return the window earlier to foreground
            SetForegroundWindow(window);
            //Send Snackbar to confirm
            StaticVars.browser.ExecuteScriptAsync("SendSnackbar('Done.',1000);");
        }

        public void showDebugMessage(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }

        public void setAllowedWindowToggle(string allowed)
        {
            StaticVars.allowShowHideNow = allowed=="true";
        }

        public void requestUpdatedDinoTable(string search)
        {
            //Create the HTML table for the DINOSAURS
            var doc = new HtmlAgilityPack.HtmlDocument();
            var node = HtmlNode.CreateNode("<html><head></head><body></body></html>");
            doc.DocumentNode.AppendChild(node);
            HtmlNode table = doc.CreateElement("table");
            table.Attributes.Add("class", "mdl-data-table mdl-js-data-table mdl-data-table--selectable mdl-shadow--2dp dino_table_inner");
            foreach (ArkDino dino in StaticVars.dinos)
            {
                if(search=="" || dino.name.ToLower().Contains(search.ToLower()))
                {
                    HtmlNode entry = doc.CreateElement("tr");
                    entry.AddClass("dino_list_element");

                    var icon = doc.CreateElement("td");
                    icon.AddClass("mdl-data-table__cell--non-numeric");
                    var iconImg = doc.CreateElement("img");
                    iconImg.Attributes.Add("src", dino.img);
                    icon.AppendChild(iconImg);
                    entry.AppendChild(icon);

                    //name
                    var name = doc.CreateElement("td");
                    name.AddClass("mdl-data-table__cell--non-numeric");
                    name.InnerHtml = dino.name;
                    entry.AppendChild(name);

                    //Buttons
                    var btns = doc.CreateElement("td");
                    btns.AddClass("mdl-data-table__cell--non-numeric");
                    btns.AddClass("dino_list_element_btns");
                    string dinoString = Convert.ToBase64String(Encoding.ASCII.GetBytes(RpTools.SerializeObject(dino)));
                    btns.InnerHtml = "<button class=\"mdl-button mdl-js-button mdl-button--raised\" onclick=\"SpawnCharacter('"+dinoString+"');\"> Spawn </button>";
                    entry.AppendChild(btns);

                    //Add this to the table.
                    table.AppendChild(entry);
                }
            }
            //Convert this to the html.
            string html = table.WriteTo();
            //Send this back in base64 because it's a lot of effort to escape it.
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(html);
            StaticVars.browser.ExecuteScriptAsync("FinishRefreshTable('" + System.Convert.ToBase64String(plainTextBytes) + "');");
        }
    }
}
