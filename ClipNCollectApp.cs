using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoorMansTSqlFormatterLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClipNCollect
{
    public class ClipNCollectApp : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        CountDisplay currentCountDisplay = null;
        public ClipNCollectApp()
        {
            ToolStripMenuItem letterCountMenuItem = new ToolStripMenuItem("Letter Count", null, new EventHandler(DoLetterCount));
            ToolStripMenuItem wordCountMenuItem = new ToolStripMenuItem("Word Count", null, new EventHandler(DoWordCount));
            ToolStripMenuItem configMenuItem = new ToolStripMenuItem("Configuration", null, new EventHandler(ShowConfig));
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", null, new EventHandler(Exit));

            notifyIcon.Click += new EventHandler(Icon_LeftClick);
            notifyIcon.Icon = IconResources.ClipNCollect;

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add(letterCountMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add(wordCountMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add("-");
            notifyIcon.ContextMenuStrip.Items.Add(configMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add("-");
            notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);
            notifyIcon.Visible = true;
        }

        InfoForm infoWindow = new InfoForm();
        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window, merely focus it.
            if (infoWindow.Visible)
            {
                infoWindow.Activate();
            }
            else
            {
                infoWindow.ShowDialog();
            }
        }

        void RemoveExistingCountDisplay()
        {
            if (currentCountDisplay != null && !currentCountDisplay.IsDisposed)
            {
                currentCountDisplay.Close();
                currentCountDisplay.Dispose();
            }
        }

        void DoWordCount(object sender, EventArgs e)
        {
            RemoveExistingCountDisplay();

            var clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                int wordsCount = clipboardText.Split(' ').Length;
                currentCountDisplay = new CountDisplay("Word Count", wordsCount.ToString());
                currentCountDisplay.Show();
            }
        }

        void DoLetterCount(object sender, EventArgs e)
        {
            RemoveExistingCountDisplay();

            var clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                var letterCount = clipboardText.Length;
                currentCountDisplay = new CountDisplay("Letter Count", letterCount.ToString());
                currentCountDisplay.Show();
            }
        }

        void Icon_LeftClick(object sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseclick && mouseclick.Button != MouseButtons.Left)
                return;

            try
            {
                if (infoWindow.Visible)
                {
                    infoWindow.Activate();
                }

                var clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    //SQL
                    Regex sqlRgx = new Regex(@"^^(?=.*SELECT.*FROM)(?!.*(?:CREATE|DROP|UPDATE|INSERT|ALTER|DELETE|ATTACH|DETACH)).*$", RegexOptions.IgnoreCase);
                    if (sqlRgx.IsMatch(clipboardText))
                    {
                        SqlFormattingManager sqlFormatTool = new SqlFormattingManager();
                        var output = sqlFormatTool.Format(clipboardText);
                        Clipboard.SetText(output);
                    }
                    else
                    {
                        //JSON
                        var jsonValid = TryParseJson(clipboardText);
                        if (jsonValid != null)
                        {
                            Clipboard.SetText(jsonValid.ToString(Formatting.Indented));
                        }
                        else
                        {
                            //XML
                            var xmlValid = TryParseXml(clipboardText);
                            if (xmlValid != null)
                            {
                                Clipboard.SetText(xmlValid.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        XDocument TryParseXml(string xml)
        {
            try
            {
                return XDocument.Parse(xml);
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
        }

        JToken TryParseJson(string json)
        {
            try
            {
                var obj = JToken.Parse(json);
                return obj;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
