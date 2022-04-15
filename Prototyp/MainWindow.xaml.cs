using DynamicData;
using Microsoft.Win32;
using NodeNetwork.ViewModels;
using Prototyp.Elements;
using Prototyp.Modules;
using System;
using System.IO;
using System.Linq;
using System.Windows;

/* -------------------------------

Infos:

https://github.com/Wouterdek/NodeNetwork/blob/master/NodeNetworkTests/NetworkViewModelTests.cs
^ Nützliche Beispielimplementierungen zum Node-Network.

------------------------------- */


/* -------------------------------

TODO:

o Mouse select in ComboBox after typing does not work.
o Please make an event for node deletion.
o Low priority: Add multi-select in toolbar modules selection.

Notiz: Multi-Subtyp-Vektor-Anschlüsse sind nicht mehr erlaubt. Falls mehrere Vektortypen
erlaubt sein sollen, muss man für jeden einen eigenen Anschluss machen. Siehe neues Buffer-Modul.

------------------------------- */

namespace Prototyp
{
    //public class Button
    //{
    //    public string Binary { get; set; }
    //    public string Icon { get; set; }
    //}
    public class ComboItem
    {
        public string IconPath { get; set; }
        public string ToolName { get; set; }
        public VorteXML VorteXMLStruct { get; set; }
        public string BinaryPath { get; set; }
    }

    public class NodeConnection
    {
        public int InputChannel { get; set; }
        public int OutputChannel { get; set; }
        public double ImportNodeOutput { get; set; }
        public Node_Module InputNode { get; set; }
        public Node_Module OutputNode { get; set; }
    }

    public class NodeProgressReport
    {
        public Node_Module node { get; set; }
        public int progress { get; set; }
        public NodeProgress stage { get; set; }
    }

    public enum NodeProgress
    {
        Waiting,        // Not all inputs ready, node is waiting for inputs
        Marked,         // All required inputs are ready
        Processing,     // Currently running the process
        Finished,       // Process finished successfully
        Interrupted     // Process ended unsuccessfully
    }

    public partial class MainWindow : Window
    {
        public const int BASEPORT = 5000;
        public const int MAX_UNSIGNED_SHORT = 65536;

        private const string COMBOMSG = "Select your tool here...";
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private bool Typing = false;

        public System.Collections.Generic.List<VectorData> vectorData = new System.Collections.Generic.List<VectorData>();
        public System.Collections.Generic.List<RasterData> rasterData = new System.Collections.Generic.List<RasterData>();
        private System.Collections.Generic.List<ComboItem> ComboItems = new System.Collections.Generic.List<ComboItem>();
        private System.Collections.Generic.List<ComboItem> ComboSearchItems = new System.Collections.Generic.List<ComboItem>();

        public string ModulesPath;
        System.IO.DirectoryInfo ParentDir;
        public NetworkViewModel network = new NetworkViewModel();

        public static MainWindow AppWindow;

        // Getters and setters -------------------------------------------------------------

        public System.Collections.Generic.List<VectorData> VecList
        {
            get { return (vectorData); }
            set { vectorData = value; }
        }

        public System.Collections.Generic.List<RasterData> RasList
        {
            get { return (rasterData); }
            set { rasterData = value; }
        }

        public System.Collections.Generic.List<ComboItem> ComboBoxItems
        {
            get { return (ComboItems); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor: Initialize.
        public MainWindow()
        {
            // Init WPF
            InitializeComponent();

            // Init modules path and start parsing.
            //TODO: Besseren Weg finden, um das parent directory zu bestimmen.
            ModulesPath = System.IO.Directory.GetCurrentDirectory();
            ParentDir = System.IO.Directory.GetParent(ModulesPath);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            if (ParentDir.ToString().EndsWith("bin")) ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ModulesPath = ParentDir.FullName + "\\Custom modules";

            // Startup NetworkView.
            AppWindow = this;
            networkView.ViewModel = network;

            ParseModules(ModulesPath);
            LoadButtons(ParentDir);
        }

        // Private methods --------------------------------------------------------------------

        private void ParseModules(string Path)
        {
            string[] SubDirs = Directory.GetDirectories(Path);
            string[] FileNames;
            string XMLName;
            VorteXML ThisXML;
            System.Collections.Generic.List<ComboItem> LocalList = new System.Collections.Generic.List<ComboItem>();

            foreach (string Dir in SubDirs)
            {
                FileNames = System.IO.Directory.GetFiles(Dir);
                foreach (string FileName in FileNames)
                {
                    if (FileName.ToLower().EndsWith(".xml"))
                    {
                        XMLName = FileName;
                        ThisXML = new VorteXML(XMLName);

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

            ComboItems.Clear();
            ComboItem CaptionItem = new ComboItem();
            CaptionItem.ToolName = COMBOMSG;
            ComboItems.Add(CaptionItem);
            for (int i = 0; i < LocalList.Count; i++) ComboItems.Add(LocalList[i]);

            ToolsComboBox.ItemsSource = null;
            ToolsComboBox.ItemsSource = ComboItems;
            ToolsComboBox.SelectedIndex = 0;
        }

        private void LoadButtons(System.IO.DirectoryInfo LocalDir)
        {
            ProgSettings progSettings = new ProgSettings(LocalDir.FullName + "/appsettings.json");

            // Process toolbars
            foreach (PSetting s in progSettings.PSettings)
            {
                if (s.tButton != null)
                {
                    createButton(s.tButton.ToolName, s.tButton.TargetControl);
                }
                else if (s.wfButton != null)
                {
                    createButton(s.wfButton.WFPath, s.wfButton.IconPath, s.wfButton.TargetControl);
                }
                // else if ... Any other settings here.
            }

            // Process anything else at will
        }

        private void SaveButtons()
        {
            ProgSettings ps = new ProgSettings();

            string Child = null;

            for (int i = 1; ; i++)
            {
                Child = "ToolBar" + i;
                System.Windows.Controls.DockPanel toolbar = FindName(Child) as System.Windows.Controls.DockPanel;

                if (toolbar == null) break;
                if (toolbar.Children.Count == 0) break;
                if (toolbar.Children[0].GetType().FullName != "System.Windows.Controls.Button") break;

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
                            ps.PSettings.Add(ProgSetting);
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
                        ps.PSettings.Add(ProgSetting);
                    }
                }
            }

            ps.SaveProgSettings(ParentDir.FullName + "/appsettings.json");
        }

        //Overload that creates tool buttons.
        private void createButton(string ToolName, string DockPanelName)
        {
            System.Windows.Controls.Button ModuleBtn = new System.Windows.Controls.Button();
            ModuleBtn.Content = new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(ModulesPath + "/" + ToolName + "/" + "Icon.png")),
                ToolTip = ToolName,
                VerticalAlignment = VerticalAlignment.Center
            };
            ModuleBtn.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            ModuleBtn.Click += new RoutedEventHandler((sender, e) => importModule(ModulesPath + "/" + ToolName + "/" + ToolName));

            System.Windows.Controls.ContextMenu buttonContextmenu = new System.Windows.Controls.ContextMenu();

            System.Windows.Controls.MenuItem removeBtn = new System.Windows.Controls.MenuItem();
            removeBtn.Header = "Remove";

            System.Windows.Controls.DockPanel dockPanel = this.FindName(DockPanelName) as System.Windows.Controls.DockPanel;
            removeBtn.Click += new RoutedEventHandler((sender, e) => removeBtn_Click(ModuleBtn, dockPanel));
            buttonContextmenu.Items.Add(removeBtn);
            ModuleBtn.ContextMenu = buttonContextmenu;

            dockPanel.Children.Add(ModuleBtn);
        }

        // Overload that creates workflow buttons.
        private void createButton(string WFFile, string IconPath, string DockPanelName)
        {
            System.Windows.Controls.Button ModuleBtn = new System.Windows.Controls.Button();
            if (System.IO.File.Exists(IconPath))
            {
                ModuleBtn.Content = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(IconPath)),
                    ToolTip = WFFile,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                ModuleBtn.Content = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(ParentDir.FullName + "/Images/VortexIcon.png")),
                    ToolTip = WFFile,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            ModuleBtn.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            ModuleBtn.Click += new RoutedEventHandler((sender, e) => LoadWorkflow(WFFile));

            System.Windows.Controls.ContextMenu buttonContextmenu = new System.Windows.Controls.ContextMenu();

            System.Windows.Controls.MenuItem removeBtn = new System.Windows.Controls.MenuItem();
            removeBtn.Header = "Remove";

            System.Windows.Controls.DockPanel dockPanel = this.FindName(DockPanelName) as System.Windows.Controls.DockPanel;
            removeBtn.Click += new RoutedEventHandler((sender, e) => removeBtn_Click(ModuleBtn, dockPanel));
            buttonContextmenu.Items.Add(removeBtn);
            ModuleBtn.ContextMenu = buttonContextmenu;

            dockPanel.Children.Add(ModuleBtn);
        }

        private void LoadWorkflow(string FileName)
        {
            // Note: Make sure to stop ongoing computations first.

            if (network.Nodes.Count > 0)
            {
                if (MessageBox.Show("Are you sure? Current progress will be lost.", "Open a workflow?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            }

            LoadWorkflowFinally(FileName);
        }

        private void LoadWorkflowFinally(string FileName)
        {
            Cursor = System.Windows.Input.Cursors.Wait;

            // Here we go. First, stop all active servers.
            TerminateAllServers();

            Prototyp.Elements.NetworkLoadAndSave open = new Prototyp.Elements.NetworkLoadAndSave(FileName);

            network = open.ImportWorkflow(vectorData, rasterData, network, ModulesPath);

            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void TerminateServer(Node_Module module)
        {
            try
            {
                if (module.Process != null)
                {
                    module.Process.Kill();
                    module.grpcConnection = null;
                }
            }
            catch
            {
                // No harm done.
            }
        }

        private void TerminateAllServers()
        {
            foreach (NodeViewModel node in network.Nodes.Items)
            {
                if (node is Node_Module module)
                {
                    TerminateServer(module);
                }
            }
        }

        private void importModule(string BinaryPath)
        {
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true); // Wozu ist das? Habe unten url von http auf https geändert.

            //Find lowest available port
            int port = Node_Module.GetNextPort(BASEPORT);

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;

            System.Diagnostics.Process moduleProcess = new System.Diagnostics.Process();

            System.Diagnostics.ProcessStartInfo moduleProcessInfo = new System.Diagnostics.ProcessStartInfo(BinaryPath + ".exe", port.ToString());
            //moduleProcessInfo.CreateNoWindow = true; //Ja, dies macht das Server-Window wirklich unsichtbar. Sichtbarkeit nur für Debugging-Zwecke.
            moduleProcessInfo.UseShellExecute = false; //'UseShellExecute = true' would be available only on the Windows platform.
            moduleProcess.StartInfo = moduleProcessInfo;
            try
            {
                moduleProcess.Start();

                // Establish GRPC connection
                // TODO: nicht nur localhost
                string url = "https://localhost:" + port.ToString();
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);

                Node_Module nodeModule = new Node_Module(BinaryPath + ".xml", grpcConnection, url, moduleProcess);
                network.Nodes.Add(nodeModule);
            }
            catch
            {
                //if (!System.IO.File.Exists(ComboItems[Index].BinaryPath + ".exe"))
                //{
                //    throw new System.Exception("Could not start binary: No executable file present.");
                //}
                //else
                //{
                //    throw new System.Exception("Could not start binary: Reason unknown.");
                //}
            }

            ToolsComboBox.SelectedIndex = 0;
        }

        private void AppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TerminateAllServers();
        }

        // Public methods ---------------------------------------------------------------------------

        public void RemoveVectorData(string Uid)
        {
            for (int i = 0; i < vectorData.Count; i++)
            {
                if (vectorData[i].ID.ToString() == Uid)
                {
                    vectorData.RemoveAt(i);
                    break;
                }
            }
        }

        public void RemoveRasterData(string Uid)
        {
            for (int i = 0; i < rasterData.Count; i++)
            {
                if (rasterData[i].ID.ToString() == Uid)
                {
                    rasterData.RemoveAt(i);
                    break;
                }
            }
        }

        // User control handlers --------------------------------------------------------------------

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FlatGeobuf files (*.fgb)|*.fgb|" +
                                    "Shapefiles (*.shp)|*.shp|" +
                                    "GeoTiff (*.tif)|*.tif|" +
                                    "GeoASCII (*.asc)|*.asc|" +
                                    "Raster files (*.sdat)|*.sdat|" +
                                    "All files (*.*)|*.*";
            openFileDialog.FilterIndex = openFileDialog.Filter.Length;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open a data file...";

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".shp" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".fgb" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".geojson") //TODO: Auch andere GDAL-Layer-Dateitypen möglich.
                {
                    if (vectorData.Count > 0)
                    {
                        if (vectorData[vectorData.Count - 1] != null)
                        {
                            while (vectorData[vectorData.Count - 1].Busy)
                            {
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                    }

                    VectorData peek = (new VectorData(openFileDialog.FileName));
                    string geometryType = peek.FeatureCollection[0].Geometry.GeometryType;
                    peek = null;
                    switch (geometryType)
                    {
                        case "Point":
                            vectorData.Add(new VectorPointData(openFileDialog.FileName));
                            break;
                        case "Line":
                            vectorData.Add(new VectorLineData(openFileDialog.FileName));
                            break;
                        case "Polygon":
                            vectorData.Add(new VectorPolygonData(openFileDialog.FileName));
                            break;
                        case "MultiPolygon":
                            vectorData.Add(new VectorMultiPolygonData(openFileDialog.FileName));
                            break;
                        default:
                            // There should be nothing here.
                            break;
                    }
                    //// Testcode start
                    //// Bitte noch nicht löschen!
                    //string Test1 = vectorData[vectorData.Count - 1].ToString(ToStringParams.ByteString);
                    //VectorData Test2 = new VectorData(Test1);

                    //byte[] ByteArr1 = vectorData[vectorData.Count - 1].VecData;
                    //byte[] ByteArr2 = Test2.VecData;
                    //for (int i = 0; i < ByteArr1.Length; i++)
                    //{
                    //    if (ByteArr1[i] != ByteArr2[i]) MessageBox.Show("Index: " + i.ToString() + ", ByteArr1: " + ByteArr1[i].ToString("X") + ", ByteArr2: " + ByteArr2[i].ToString("X"));
                    //}
                    //// Testcode end
                    MainWindowHelpers mainWindowHelpers = new MainWindowHelpers();
                    mainWindowHelpers.AddTreeViewChild(vectorData[vectorData.Count - 1]);
                }
                else if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".tif" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".asc" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".sdat") //TODO: Auch andere GDAL-Raster-Dateitypen möglich.
                {
                    if (rasterData.Count > 0)
                    {
                        if (rasterData[rasterData.Count - 1] != null)
                        {
                            while (rasterData[rasterData.Count - 1].Busy)
                            {
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                    }

                    rasterData.Add(new RasterData(openFileDialog.FileName));
                    //// Testcode start
                    //// Bitte noch nicht löschen!
                    //string Test1 = rasterData[rasterData.Count - 1].ToString();
                    //RasterData Test2 = new RasterData(Test1);

                    //byte[] ByteArr1 = rasterData[rasterData.Count - 1].Serialize();
                    //byte[] ByteArr2 = Test2.Serialize();
                    //for (int i = 0; i < ByteArr1.Length; i++)
                    //{
                    //    if (ByteArr1[i] != ByteArr2[i]) MessageBox.Show("Index: " + i.ToString() + ", ByteArr1: " + ByteArr1[i].ToString("X") + ", ByteArr2: " + ByteArr2[i].ToString("X"));
                    //}
                    //// Testcode end
                    MainWindowHelpers mainWindowHelpers = new MainWindowHelpers();
                    mainWindowHelpers.AddTreeViewChild(rasterData[rasterData.Count - 1]);
                }
                //else if (...) //TODO: Ggf. andere Datentypen...
                //{

                //}
                else
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    return;
                }

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }

        //Objekt wird in den NodeEditor gezogen
        public void DropTargetEventNodeEditor(object sender, DragEventArgs e)
        {
            if (((string)e.Data.GetData("Type")).ToLower() == "vector")
            {
                for (int i = 0; i < vectorData.Count; i++)
                {
                    if (vectorData[i].ID.ToString() == (string)e.Data.GetData("ID"))
                    {
                        VectorImport_Module importNode = null;

                        Type vecType = vectorData[i].GetType();
                        if (vecType.Name == "VectorPointData")
                        {
                            importNode = new VectorImport_ModulePoint(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                        }
                        else if (vecType.Name == "VectorLineData")
                        {
                            importNode = new VectorImport_ModuleLine(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                        }
                        else if (vecType.Name == "VectorPolygonData")
                        {
                            importNode = new VectorImport_ModulePolygon(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                        }
                        else if (vecType.Name == "VectorMultiPolygonData")
                        {
                            importNode = new VectorImport_ModuleMultiPolygon(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                        }
                        else
                        {
                            // There should be nothing here.
                        }

                        Point TempPoint;
                        TempPoint = e.GetPosition(networkView);
                        TempPoint.X = (TempPoint.X - networkView.ViewModel.DragOffset.X) / networkView.ViewModel.ZoomFactor;
                        TempPoint.Y = (TempPoint.Y - networkView.ViewModel.DragOffset.Y) / networkView.ViewModel.ZoomFactor;
                        importNode.Position = TempPoint;

                        network.Nodes.Add(importNode);
                        break;
                    }
                }
            }
            else if (((string)e.Data.GetData("Type")).ToLower() == "raster")
            {
                for (int i = 0; i < rasterData.Count; i++)
                {
                    if (rasterData[i].ID.ToString() == (string)e.Data.GetData("ID"))
                    {
                        RasterImport_Module importNode = new RasterImport_Module(rasterData[i].Name, rasterData[i].FileType, rasterData[i].ID);

                        Point TempPoint;
                        TempPoint = e.GetPosition(networkView);
                        TempPoint.X = (TempPoint.X - networkView.ViewModel.DragOffset.X) / networkView.ViewModel.ZoomFactor;
                        TempPoint.Y = (TempPoint.Y - networkView.ViewModel.DragOffset.Y) / networkView.ViewModel.ZoomFactor;
                        importNode.Position = TempPoint;

                        network.Nodes.Add(importNode);
                        break;
                    }
                }
            }
            //else if (...) //TODO: Ggf. andere Datentypen...
            //{

            //}
            else
            {

            }
        }

        private void ComboSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int Index = ToolsComboBox.SelectedIndex;
            if (Index <= 0) return;
            if (Typing) return;

            importModule(ComboItems[Index].BinaryPath);

            ToolsComboBox.SelectedIndex = 0;
        }

        private void ComboLostFocus(object sender, RoutedEventArgs e)
        {
            if (ToolsComboBox.IsDropDownOpen == false)
            {
                System.Diagnostics.Debug.WriteLine("Hallo");
                ComboSearchItems.Clear();
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
            }
        }

        private void ComboKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down |
                e.Key == System.Windows.Input.Key.Up |
                e.Key == System.Windows.Input.Key.LeftCtrl |
                e.Key == System.Windows.Input.Key.RightCtrl) return;

            ToolsComboBox.IsDropDownOpen = true;
            Typing = true;

            string KeyPress = "";
            if (e.Key == System.Windows.Input.Key.Back) KeyPress = "Back";
            if (e.Key == System.Windows.Input.Key.Escape) KeyPress = "Esc";
            if (e.Key == System.Windows.Input.Key.Return | e.Key == System.Windows.Input.Key.Enter) KeyPress = "Ret";
            if (e.Key == System.Windows.Input.Key.D1) KeyPress = "1";
            if (e.Key == System.Windows.Input.Key.D2) KeyPress = "2";
            if (e.Key == System.Windows.Input.Key.D3) KeyPress = "3";
            if (e.Key == System.Windows.Input.Key.D4) KeyPress = "4";
            if (e.Key == System.Windows.Input.Key.D5) KeyPress = "5";
            if (e.Key == System.Windows.Input.Key.D6) KeyPress = "6";
            if (e.Key == System.Windows.Input.Key.D7) KeyPress = "7";
            if (e.Key == System.Windows.Input.Key.D8) KeyPress = "8";
            if (e.Key == System.Windows.Input.Key.D9) KeyPress = "9";
            if (e.Key == System.Windows.Input.Key.D0) KeyPress = "0";
            if (ALPHABET.Contains(e.Key.ToString())) KeyPress = e.Key.ToString();

            if (KeyPress == "Back")
            {
                if (ComboSearchItems.Count == 0) return;

                if (ComboSearchItems[0].ToolName.Length == 1)
                {
                    ToolsComboBox.IsDropDownOpen = false;
                    ComboSearchItems.Clear();
                    ComboItems[0].ToolName = COMBOMSG;
                    ToolsComboBox.ItemsSource = null;
                    ToolsComboBox.ItemsSource = ComboItems;
                    ToolsComboBox.SelectedIndex = 0;
                    Typing = false;
                    return;
                }

                string TempString = ComboSearchItems[0].ToolName.Substring(0, ComboSearchItems[0].ToolName.Length - 1);
                ComboSearchItems.Clear();
                ComboSearchItems.Add(ComboItems[0]);
                ComboSearchItems[0].ToolName = TempString;

                for (int i = 1; i < ComboItems.Count; i++)
                {
                    if (ComboItems[i].ToolName.ToLower().Contains(ComboSearchItems[0].ToolName))
                    {
                        ComboSearchItems.Add(ComboItems[i]);
                    }
                }
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboSearchItems;
                if (ComboSearchItems.Count > 1) ToolsComboBox.SelectedIndex = 1;
                return;
            }

            if (KeyPress == "Esc")
            {
                ToolsComboBox.IsDropDownOpen = false;
                ComboSearchItems.Clear();
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
                return;
            }

            if (KeyPress == "Ret")
            {
                if (ComboSearchItems.Count == 0) return;

                for (int i = 1; i < ComboItems.Count; i++)
                {
                    if (ComboItems[i].ToolName.ToLower().Contains(ComboSearchItems[0].ToolName))
                    {
                        ComboSearchItems.Clear();
                        ComboItems[0].ToolName = COMBOMSG;
                        ToolsComboBox.ItemsSource = null;
                        ToolsComboBox.ItemsSource = ComboItems;

                        Typing = false;
                        ToolsComboBox.SelectedIndex = i;

                        break;
                    }
                }
                return;
            }

            if (ALPHABET.Contains(KeyPress))
            {
                string TempString;
                if (ComboSearchItems.Count > 0)
                {
                    TempString = ComboSearchItems[0].ToolName + KeyPress.ToLower();
                }
                else
                {
                    TempString = KeyPress.ToLower();
                }
                ComboSearchItems.Clear();
                ComboSearchItems.Add(ComboItems[0]);
                ComboSearchItems[0].ToolName = TempString;

                for (int i = 1; i < ComboItems.Count; i++)
                {
                    if (ComboItems[i].ToolName.ToLower().Contains(ComboSearchItems[0].ToolName))
                    {
                        ComboSearchItems.Add(ComboItems[i]);
                    }
                }

                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboSearchItems;
                if (ComboSearchItems.Count > 1) ToolsComboBox.SelectedIndex = 1;
                return;
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<NodeConnection> modules = new System.Collections.Generic.List<NodeConnection>();
            System.Collections.Generic.List<NodeConnection> imports = new System.Collections.Generic.List<NodeConnection>();
            System.Collections.Generic.List<Node_Module> marked = new System.Collections.Generic.List<Node_Module>();

            if (NodeNetwork.Toolkit.GraphAlgorithms.FindLoops(network).Any())
            {
                MessageBox.Show("Network contains loop(s). Please revert and try again.", "Loop detected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Collect all current node-channel-connections
            foreach (ConnectionViewModel conn in network.Connections.Items)
            {
                //MessageBox.Show(conn.Output.Parent.GetType().ToString());
                NodeConnection nc = new NodeConnection();
                if (conn.Output.Parent is Node_Module)
                {
                    // URL im ((Node_Module)conn.Output.Parent).url
                    nc.OutputNode = (Node_Module)conn.Output.Parent;
                    nc.OutputChannel = conn.Output.GetID();

                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();


                    modules.Add(nc);
                    //MessageBox.Show(nc.OutputNode + "_" + nc.OutputChannel + " -> " + nc.InputNode + "_" + nc.InputChannel);
                }
                else
                {
                    // Data ID in vectorData[i].ID mit foreach durch Liste iterieren.
                    nc.ImportNodeOutput = conn.Output.GetDataID();
                    nc.OutputChannel = 0;

                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();

                    imports.Add(nc);
                    //MessageBox.Show(nc.ImportNodeOutput +"_" + nc.OutputChannel + " -> " + nc.InputNode + "_" + nc.InputChannel);
                }
            }

            //STEP 2: Traverse graph, check for possible changes in outputs (TODO)
            //STEP 3: Update module configs (TODO)
            //STEP 4: Load inputs into the correct modules and mark starting modules of graph
            //


            foreach (NodeConnection nc in imports)
            {
                //Get data
                //TODO: vectorData as <int> dictionary
                //string layer = vectorData[(int)nc.ImportNodeOutput].ToString(ToStringParams.ByteString);
                string layer = null;
                foreach (VectorData v in vectorData)
                {
                    if (v.ID == nc.ImportNodeOutput)
                    {
                        layer = v.ToString(ToStringParams.ByteString);
                        break;
                    }
                }
                //Split into chunks of 65536 bytes (64 KiB)
                System.Collections.Generic.List<string> chunks = new System.Collections.Generic.List<string>();
                int maxChunkSize = MAX_UNSIGNED_SHORT / sizeof(Char);
                for (int i = 0; i < layer.Length; i += maxChunkSize)
                {
                    chunks.Add(layer.Substring(i, Math.Min(maxChunkSize, layer.Length - i)));
                }
                //Upload data to module through GRPC call (TODO: multitasking?)
                using (var call = nc.InputNode.grpcConnection.SetLayer())
                {
                    foreach (string chunk in chunks)
                    {
                        await call.RequestStream.WriteAsync(new GrpcClient.ByteStream { Chunk = Google.Protobuf.ByteString.FromBase64(chunk) });
                    }
                    await call.RequestStream.CompleteAsync();
                    GrpcClient.LayerResponse response = await call.ResponseAsync;
                    if (response.Finished == 1)
                    {
                        System.Diagnostics.Trace.WriteLine("Data loaded from " + nc.ImportNodeOutput + " to " + nc.InputNode + "_" + nc.InputChannel);
                        marked.Add(nc.OutputNode);
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Data load failed from " + nc.ImportNodeOutput + " to " + nc.InputNode + "_" + nc.InputChannel);
                    }
                }
            }
            //STEP 5: Run modules (TODO - WIP)
            //
            //Initialize Progress object to report module progress
            var progressIndicator = new Progress<NodeProgressReport>(ReportProgress);
            //Start module handling process
            if (await RunGraphAsync(modules, marked, progressIndicator))
            {
                System.Diagnostics.Trace.WriteLine("All nodes processed");
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Node processing interrupted");
            }
        }

        async System.Threading.Tasks.Task<bool> RunGraphAsync(System.Collections.Generic.List<NodeConnection> connections, System.Collections.Generic.List<Node_Module> marked, IProgress<NodeProgressReport> progress)
        {
            bool successful = await System.Threading.Tasks.Task.Run<bool>(async () =>
            {
                //Async node graph traversal
                //Prepare and mark nodes
                foreach (NodeConnection nc in connections)
                {
                    NodeProgressReport report = new NodeProgressReport();
                    report.node = nc.InputNode;
                    report.stage = NodeProgress.Waiting;
                    progress.Report(report);
                    //TODO: find marked nodes
                }
                //TODO: Run activated nodes, mark new nodes, keep running
                foreach (NodeConnection nc in connections)
                {
                    //Make grpc call and report progress throughout
                    var request = new GrpcClient.RunRequest { };
                    NodeProgressReport report = new NodeProgressReport();
                    report.node = nc.InputNode;
                    using (var call = nc.InputNode.grpcConnection.RunProcess(request))
                    {
                        report.stage = NodeProgress.Processing;
                        report.progress = 0;
                        progress.Report(report);
                        while (await call.ResponseStream.MoveNext(System.Threading.CancellationToken.None))
                        {
                            GrpcClient.RunUpdate update = call.ResponseStream.Current;
                            //report.progress = update.Progress;
                            //progress.Report(report);
                        }
                        report.progress = 100;
                        report.stage = NodeProgress.Finished;
                        progress.Report(report);
                    }
                }
                return true;
            });
            return successful;
        }

        //Method to report node state from async tasks
        private void ReportProgress(NodeProgressReport report)
        {
            //TODO: Update the UI to reflect all the progress values that are passed back.
            switch (report.stage)
            {
                case NodeProgress.Waiting:
                    System.Diagnostics.Trace.WriteLine("Node " + report.node.Url + " waiting for input.");
                    report.node.ChangeStatus(NodeProgress.Waiting);
                    break;
                case NodeProgress.Marked:
                    report.node.ChangeStatus(NodeProgress.Marked);
                    break;
                case NodeProgress.Processing:
                    System.Diagnostics.Trace.WriteLine("Node " + report.node.Url + " progress: " + report.progress);
                    report.node.ChangeStatus(NodeProgress.Processing);
                    break;
                case NodeProgress.Finished:
                    System.Diagnostics.Trace.WriteLine("Node " + report.node.Url + " finished!");
                    report.node.ChangeStatus(NodeProgress.Finished);
                    break;
                case NodeProgress.Interrupted:
                    report.node.ChangeStatus(NodeProgress.Interrupted);
                    break;
                default:
                    break;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ModuleDesigner moduleDesigner = new ModuleDesigner();
            moduleDesigner.Owner = this;
            moduleDesigner.ShowDialog();

            if (!moduleDesigner.OkayClicked) return;
            if (moduleDesigner.ListViewEntries.Count == 0) return;

            VorteXML vorteXML = moduleDesigner.MakeXML();

            string XMLStr = vorteXML.ExportXML();
            DirectoryInfo di = Directory.CreateDirectory(ModulesPath + "\\" + vorteXML.NodeTitle);
            File.WriteAllText(di.FullName + "\\" + vorteXML.NodeTitle + ".xml", XMLStr);
            ComboItems.Clear();
            ParseModules(ModulesPath);
        }

        private void ConnectionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Startet ein automatisches Network-Arragement, mehr oder weniger gut. Vielleicht irgendwann mal nützlich.
            NodeNetwork.Toolkit.Layout.ForceDirected.ForceDirectedLayouter layout = new NodeNetwork.Toolkit.Layout.ForceDirected.ForceDirectedLayouter();
            layout.Layout(new NodeNetwork.Toolkit.Layout.ForceDirected.Configuration { Network = network }, 10000);
        }

        private void ToolboxButton_Click(object sender, RoutedEventArgs e)
        {
            bool DeleteData;

            foreach (NodeViewModel node in network.Nodes.Items)
            {
                DeleteData = true;

                if (node.IsSelected)
                {
                    if (node is Node_Module module)
                    {
                        TerminateServer(module);
                    }
                    else if (node is VectorImport_Module v)
                    {
                        //// First check whether there is another importer that also employs the same data source. If yes, data must not be deleted.
                        //foreach (NodeViewModel node2 in network.Nodes.Items)
                        //{
                        //    if (node2 is VectorImport_Module v2)
                        //    {
                        //        if (v2 != v)
                        //        {
                        //            if (v2.IntID == v.IntID)
                        //            {
                        //                // Found one. Don't delete data.
                        //                DeleteData = false;
                        //            }
                        //        }
                        //    }
                        //}

                        //if (DeleteData)
                        //{
                        //    // Okay, now it's save to delete the data. Find entries and off you go.
                        //    foreach (object t in TableOfContentsVector.Items)
                        //    {
                        //        if (((Prototyp.Custom_Controls.VectorListViewItem)t).Uid == v.IntID.ToString())
                        //        {
                        //            RemoveVectorData(((Prototyp.Custom_Controls.VectorListViewItem)t).Uid);
                        //            TableOfContentsVector.Items.Remove(t);
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else if (node is RasterImport_Module r)
                    {
                        //// First check whether there is another importer that also employs the same data source. If yes, data must not be deleted.
                        //foreach (NodeViewModel node2 in network.Nodes.Items)
                        //{
                        //    if (node2 is RasterImport_Module r2)
                        //    {
                        //        if (r2 != r)
                        //        {
                        //            if (r2.IntID == r.IntID)
                        //            {
                        //                // Found one. Don't delete data.
                        //                DeleteData = false;
                        //            }
                        //        }
                        //    }
                        //}

                        //if (DeleteData)
                        //{
                        //    // Okay, now it's save to delete the data. Find entries and off you go.
                        //    foreach (object t in TableOfContentsRaster.Items)
                        //    {
                        //        if (((Prototyp.Custom_Controls.RasterListViewItem)t).Uid == r.IntID.ToString())
                        //        {
                        //            RemoveRasterData(((Prototyp.Custom_Controls.RasterListViewItem)t).Uid);
                        //            TableOfContentsRaster.Items.Remove(t);
                        //            break;
                        //        }
                        //    }
                        //}
                    }

                    network.Nodes.Remove(node);
                }
            }
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            // Note: Make sure to stop ongoing computations first.

            if (network.Nodes.Count > 0)
            {
                if (MessageBox.Show("Are you sure? Current progress will be lost.", "Open a workflow?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Workflow files (*.wff)|*.wff|" +
                                    "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open a workflow file...";

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                LoadWorkflowFinally(openFileDialog.FileName);
            }
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            // Note: Make sure to stop ongoing computations first.

            if (network.Nodes.Count > 0)
            {
                if (MessageBox.Show("Are you sure? Current progress will be lost.", "Open a workflow?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            }

            TerminateAllServers();

            network = null;
            network = new NodeNetwork.ViewModels.NetworkViewModel();
            AppWindow.networkView.ViewModel = network;

            Point TempPoint;
            TempPoint.X = 0;
            TempPoint.Y = 0;

            networkView.ViewModel.DragOffset = TempPoint;
            networkView.ViewModel.ZoomFactor = 1;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Workflow files (*.wff)|*.wff|" +
                                    "All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "Save a workflow file...";

            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                if (System.IO.File.Exists(saveFileDialog.FileName))
                {
                    if (MessageBox.Show("File exists. Overwrite?", "Overwrite file?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                }

                bool includeData = false;
                if (MessageBox.Show("Include data sets? This will increase data size and processing time but will make the user of the workflow independent from the data files.", "Include data?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) includeData = true; else includeData = false;

                Cursor = System.Windows.Input.Cursors.Wait;

                if (System.IO.File.Exists(saveFileDialog.FileName)) System.IO.File.Delete(saveFileDialog.FileName);

                Prototyp.Elements.NetworkLoadAndSave save = new Prototyp.Elements.NetworkLoadAndSave(network, vectorData, rasterData, saveFileDialog.FileName, includeData);

                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (NodeViewModel nodeTest in network.Nodes.Items)
            {
                if (nodeTest is Node_Module m) m.ChangeStatus(NodeProgress.Finished);
            }
        }

        private void OpenModuleList(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = sender as System.Windows.Controls.MenuItem;
            System.Windows.Controls.ContextMenu contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
            System.Windows.Controls.DockPanel dockPanel = contextMenu.PlacementTarget as System.Windows.Controls.DockPanel;

            ModuleListButtonSelection chooseModuleWindow = new ModuleListButtonSelection();
            chooseModuleWindow.Owner = this;
            chooseModuleWindow.ShowDialog();
            if (chooseModuleWindow.selectedModule.ToolName != null)
            {
                createButton(chooseModuleWindow.selectedModule.ToolName, dockPanel.Name);
                SaveButtons();
            }
        }

        private void removeBtn_Click(System.Windows.Controls.Button ModuleBtn, System.Windows.Controls.DockPanel dockPanel)
        {
            dockPanel.Children.Remove(ModuleBtn);
            SaveButtons();
        }

        private void AddWorkflowClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = sender as System.Windows.Controls.MenuItem;
            System.Windows.Controls.ContextMenu contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
            System.Windows.Controls.DockPanel dockPanel = contextMenu.PlacementTarget as System.Windows.Controls.DockPanel;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Workflow files (*.wff)|*.wff|" +
                                    "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open a workflow file...";

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string WFFile = openFileDialog.FileName;
                string IconPath;

                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png)|*.png|" +
                                        "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Open an image file...";

                result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    IconPath = openFileDialog.FileName;
                }
                else
                {
                    IconPath = ParentDir.FullName + "/Images/VortexIcon.png";
                }

                createButton(WFFile, IconPath, dockPanel.Name);
                SaveButtons();
            }
        }
    }
}


