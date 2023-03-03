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
        public ClipNCollectApp()
        {
            ToolStripMenuItem configMenuItem = new ToolStripMenuItem("Configuration", null, new EventHandler(ShowConfig));
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", null, new EventHandler(Exit));

            notifyIcon.Click += new EventHandler(Icon_LeftClick);
            notifyIcon.Icon = IconResources.ClipNCollect;

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add(configMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add("-");
            notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);
            notifyIcon.Visible = true;
        }

        Form1 infoWindow = new Form1();
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
