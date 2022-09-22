using System;

namespace Prototyp.Elements
{
    [Serializable]
    public class WorkflowButton
    {
        public string WFPath { get; set; }
        public string IconPath { get; set; }
        public string TargetControl { get; set; }
    }

    [Serializable]
    public class ToolButton
    {
        public string ToolName { get; set; }
        public string TargetControl { get; set; }
    }


    [Serializable]
    public class PSetting
    {
        public WorkflowButton wfButton { get; set; }
        public ToolButton tButton { get; set; }
    }

    [Serializable]
    public class ProgSettings
    {
        // Internal fields ------------------------------------------

        private System.Collections.Generic.List<PSetting> _settings = new System.Collections.Generic.List<PSetting>();

        // Getters and setters --------------------------------------

        public System.Collections.Generic.List<PSetting> PSettings
        {
            get { return (_settings); }
            set { _settings = value; }
        }

        // Constructors ---------------------------------------------

        // Parameterless constructor
        public ProgSettings()
        {
            // Nothing much to de here...
        }

        // Constructor that accepts a string, interprets it as file name, loads and deserializes it.
        public ProgSettings(string FileName)
        {
            LoadProgSettings(FileName);
        }

        // Public methods ---------------------------------------------

        public void PrepareSaveButtons()
        {
            string Child = null;

            for (int i = 1; i<=9 ; i++)
            {
                Child = "ToolBar" + i;
                System.Windows.Controls.DockPanel toolbar = MainWindow.AppWindow.FindName(Child) as System.Windows.Controls.DockPanel;

                //not needed
                //if (toolbar == null) break;
                //if (toolbar.Children.Count == 0) break;
                //if (toolbar.Children[0].GetType().FullName != "System.Windows.Controls.Button") break;

                for (int j = 0; j < toolbar.Children.Count; j++)
                {
                    System.Windows.Controls.Button button = toolbar.Children[j] as System.Windows.Controls.Button;

                    System.Windows.Controls.Image cont = (System.Windows.Controls.Image)button.Content;
                    string TT = cont.ToolTip.ToString();

                    if (TT.EndsWith(".wff")) // Indicates a workflow.
                    {
                        if (System.IO.File.Exists(TT))
                        {
                            WorkflowButton wf = new WorkflowButton();
                            wf.WFPath = TT;
                            wf.IconPath = cont.Source.ToString().Replace("file:///", "");
                            wf.TargetControl = Child;

                            PSetting ProgSetting = new PSetting();
                            ProgSetting.wfButton = wf;
                            _settings.Add(ProgSetting);
                        }
                    }
                    // Extend with else ifs for additional purposes.
                    else
                    {
                        ToolButton tb = new ToolButton();
                        tb.ToolName = TT;
                        tb.TargetControl = Child;

                        PSetting ProgSetting = new PSetting();
                        ProgSetting.tButton = tb;
                        _settings.Add(ProgSetting);
                    }
                }
            }
        }

        public bool SaveProgSettings(string FileName)
        {
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string JSON = System.Text.Json.JsonSerializer.Serialize(_settings, options);
            if (JSON == null) return (false);

            System.IO.File.WriteAllText(FileName, JSON);

            if (System.IO.File.Exists(FileName)) return (true); else return (false);
        }

        public void LoadProgSettings(string FileName)
        {
            if (!VectorData.FileAccessable(FileName)) { throw new System.Exception("File does not exist or is not accessible, maybe opened in some other software?"); }

            string JSON = System.IO.File.ReadAllText(FileName);

            _settings = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<PSetting>>(JSON);
        }
    }
}
