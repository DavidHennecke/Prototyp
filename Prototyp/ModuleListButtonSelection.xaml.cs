using System.Windows;

namespace Prototyp
{

    public partial class ModuleListButtonSelection : Window
    {
        public System.Collections.IList selectedModuleList = new Prototyp.ComboItem[] { };
        System.Collections.Generic.List<ComboItem> LocalList = new System.Collections.Generic.List<ComboItem>();

        public ModuleListButtonSelection()
        {
            InitializeComponent();
            
            //TODO: Besseren Weg finden, um das parent directory zu bestimmen.
            string ModulesPath = System.IO.Directory.GetCurrentDirectory();
            System.IO.DirectoryInfo ParentDir = System.IO.Directory.GetParent(ModulesPath);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            if (ParentDir.ToString().EndsWith("bin")) ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ModulesPath = ParentDir.FullName + "\\Custom modules";

            ParseModules(ModulesPath);
        }

        private void ParseModules(string Path)
        {
            string[] SubDirs = System.IO.Directory.GetDirectories(Path);
            string[] FileNames;
            string XMLName;
            Elements.VorteXML ThisXML;

            foreach (string Dir in SubDirs)
            {
                FileNames = System.IO.Directory.GetFiles(Dir);
                foreach (string FileName in FileNames)
                {
                    if (FileName.ToLower().EndsWith(".xml"))
                    {
                        XMLName = FileName;
                        ThisXML = new Elements.VorteXML(XMLName);

                        ComboItem NextItem = new ComboItem();

                        NextItem.IconPath = Dir + "/Icon.png";
                        NextItem.VorteXMLStruct = ThisXML;
                        NextItem.ToolName = ThisXML.NodeTitle;
                        NextItem.BinaryPath = Dir + "/" + ThisXML.NodeTitle;

                        LocalList.Add(NextItem);

                        break;
                    }
                }
            }

            // Order the list alphabetically
            LocalList.Sort((x, y) => x.ToolName.CompareTo(y.ToolName));
            // Order the list alphabetically in descending order
            //LocalList.Sort((x, y) => y.ToolName.CompareTo(x.ToolName));

            ModuleListButtonSlectionList.ItemsSource = null;
            ModuleListButtonSlectionList.ItemsSource = LocalList;
            ModuleListButtonSlectionList.SelectedIndex = 0;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            selectedModuleList = ModuleListButtonSlectionList.SelectedItems;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
