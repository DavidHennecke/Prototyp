using System;
using System.Collections.Generic;
using System.Text;

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

        private List<PSetting> _settings = new List<PSetting>();

        // Getters and setters --------------------------------------

        public List<PSetting> PSettings
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
            string JSON = System.IO.File.ReadAllText(FileName);

            _settings = System.Text.Json.JsonSerializer.Deserialize<List<PSetting>>(JSON);
        }
    }
}
