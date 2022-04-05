using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void addListViewItem(Template t) {
            var item = new TemplateItem(t);
            item.SubItems.Add(t.Language);
            item.SubItems.Add(t.Tags);
            templateSelector.Items.Add(item);
        }

        private List<Template> templates;

        private async Task pickLanguage() {
            templateSelector.Items.Clear();
            var templates = await DotNetWrapper.GetTemplates();
            var tags = templates.Select(n => n.Tags).Distinct().ToList();

            tags.Sort();
            tags.ForEach(tag => {
                var items = templates.Where(n => n.Tags == tag && n.Language.Contains(this.language.Text)).ToList();
                items.ForEach(t => this.addListViewItem(t));
            });
        }

        private async void Form1_Load(object sender, EventArgs e) {
            await pickLanguage();
        }

        private async void toolStripLabel1_Click(object sender, EventArgs e) {
           var item = (TemplateItem)templateSelector.SelectedItems[0];
           var template = item.Template;

          var result = await DotNetWrapper.Create(template.ShortName, language.Text, projectName.Text, directoryLocation.Directory);
            MessageBox.Show(result);
        }

        private async void language_SelectedValueChanged(object sender, EventArgs e) {
          await  pickLanguage();
        }
    }

    public class TemplateItem : ListViewItem {
        public TemplateItem() { }
        public TemplateItem(Template template) {
            this.Template = template;
            this.Text = template.TemplateName;
        }

        public Template Template { get; set; }
        
    }

   

    public class Template {
        public string TemplateName { get; set; }
        public string ShortName { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }

}

public class DotNetWrapper {

        public static async Task<List<Template>> GetTemplates() {
            {
                List<Template> result = new List<Template>();
                using (var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "dotnet",
                        Arguments = "new -l",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                }) {
                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream) {
                        string text = await proc.StandardOutput.ReadToEndAsync();

                        string[] separators = new string[] { Environment.NewLine };
                        var lines = text.Split(separators, StringSplitOptions.None).ToList();

                        lines.RemoveAt(0);
                        lines.RemoveAt(0);
                        lines.RemoveAt(1);
                        lines.RemoveAt(1);


                        Regex rex = new Regex(@"\s\s\s*");

                        lines = lines.Select(n => {
                            var txt = rex.Replace(n, "|");
                            if (txt.Length > 0) {
                                txt = txt.Substring(0, txt.Length - 1);
                            }
                            return txt;
                        }).ToList();



                        result = lines.Select(line => {
                            Template template = null;
                            if (line.Contains("|")) {
                           
                                var values = line.Split('|');
                                if (values.Length == 4) {
                                    template = new Template();
                                    template.TemplateName = values[0];
                                    template.ShortName = values[1];
                                    template.Language = values[2];
                                    template.Tags = values[3];
                                }
                            }

                            return template;

                        }).Where(n => n != null).ToList();




                    }
                }
                return result;
            }
        }



        public static async Task<string> Create(string shortName, string language, string name, string dir, bool dry = false) {
            string result = null;
            string arg;

            if (dry) {
                arg = $@"new ""{shortName}"" --dry-run -lang ""{language}"" --name ""{name}"" --output ""{dir}""";
            } else {
                arg = $@"new ""{shortName}"" -lang ""{language}"" --name ""{name}"" --output ""{dir}""";
            }

            using (var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "dotnet",
                    Arguments = arg,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            }) {

                proc.Start();
                while (!proc.StandardOutput.EndOfStream) {
                    result = await proc.StandardOutput.ReadToEndAsync();
                }
                return result;
            }
        }

        public static async Task<List<string>> GetSDKs() {
            {
                List<string> result = new List<string>();
                using (var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "dotnet",
                        Arguments = "--list-sdks",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                }) {
                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream) {
                        string text = await proc.StandardOutput.ReadToEndAsync();

                        Regex rex = new Regex(@"(\d*\.\d*\.\d*)\s\[[^\]]*]");
                        var matches = rex.Matches(text);

                        foreach (Match m in matches) {
                            result.Add(m.Groups[1].Value);
                        }

                    }
                }
                return result;
            }
        }

    }


}
