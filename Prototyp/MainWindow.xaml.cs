using DynamicData;
using Microsoft.Win32;
using NodeNetwork.ViewModels;
using Prototyp.Elements;
using Prototyp.Modules;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

/* -------------------------------

Infos:

https://github.com/Wouterdek/NodeNetwork/blob/master/NodeNetworkTests/NetworkViewModelTests.cs
^ Nützliche Beispielimplementierungen zum Node-Network.

------------------------------- */

/* -------------------------------

TODO:

o Low priority: Add multi-select in toolbar modules selection. (CHECK)
o Ein Modul erzeugen, das keinen Input, sondern nur einen Output hat, Typ Float (gesteuert über einen Slider). Erforderlich für MinDist bei WFLO.
o dynamic path specification for button creation
------------------------------- */

namespace Prototyp
{
    public class ComboItem
    {
        public string IconPath { get; set; }
        public string ToolName { get; set; }
        public VorteXML VorteXMLStruct { get; set; }
        public string BinaryPath { get; set; }
        public string Folder { get; set; }
    }

    public class NodeConnection
    {
        public int InputChannel { get; set; }
        public int OutputChannel { get; set; }
        public double ImportNodeOutput { get; set; }
        public Node_Module InputNode { get; set; }
    }

    public class NodeProgressReport
    {
        public Node_Module node { get; set; }
        public int progress { get; set; }
        public NodeProgress stage { get; set; }
    }

    public enum NodeProgress
    {
        Waiting,        //Not all inputs ready, node is waiting for inputs
        Processing,     //Currently running the process
        Finished,       //Process finished successfully
        Interrupted     //Process ended unsuccessfully
    }

    public partial class MainWindow : Window
    {
        public const int MAX_UNSIGNED_SHORT = 65535;
        public const int BASEPORT = 5000;
        public int DevBASEPORT = 0;
        public int DevBASEPORTCurrent = 0;

        public int IgnoreStarts = 0;
        public int ElapsedStarts = 0;

        private const string COMBOMSG = "Select your tool here...";
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private bool Typing = false;

        public List<VectorData> vectorData = new List<VectorData>();
        public List<RasterData> rasterData = new List<RasterData>();
        public List<TableData> tableData = new List<TableData>();
        public int importDataUID = 0;
        private List<ComboItem> ComboItems = new List<ComboItem>();
        private List<ComboItem> ComboSearchItems = new List<ComboItem>();

        public string ModulesPath;
        public static System.IO.DirectoryInfo ParentDir;
        public NetworkViewModel network = new NetworkViewModel();

        public static MainWindow AppWindow;

        // Getters and setters -------------------------------------------------------------

        public List<ComboItem> ComboBoxItems
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
            LoadSettings(ParentDir);
        }

        // Private methods --------------------------------------------------------------------
        private void ParseModules(string Path)
        {
            string[] SubDirs = Directory.GetDirectories(Path);
            string[] FileNames;
            string XMLName;
            VorteXML ThisXML;
            List<ComboItem> LocalList = new List<ComboItem>();

            foreach (string Dir in SubDirs)
            {
                System.Windows.Controls.TreeViewItem newNode = new System.Windows.Controls.TreeViewItem();
                newNode.Header = new DirectoryInfo(Dir).Name;
                newNode.FontSize = 13;
                newNode.Focusable = false;
                
                string[] Dirs = Directory.GetDirectories(Dir);

                foreach (string ModuleDir in Dirs)
                {
                    FileNames = System.IO.Directory.GetFiles(ModuleDir);

                    System.Windows.Controls.TreeViewItem newItem = new System.Windows.Controls.TreeViewItem();
                    System.Windows.Controls.DockPanel tempDock = new System.Windows.Controls.DockPanel();
                    System.Windows.Controls.Label tempLabel = new System.Windows.Controls.Label();
                    System.Windows.Controls.Image tempImage = new System.Windows.Controls.Image();

                    tempLabel.Content = "  " + new DirectoryInfo(ModuleDir).Name;
                    tempLabel.FontSize = 12;
                    System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(ModuleDir + "/Icon.png"));

                    tempImage.Source = bitmapImage;
                    tempImage.Width = 12;
                    tempImage.Height = 12;

                    tempDock.Children.Add(tempImage);
                    tempDock.Children.Add(tempLabel);

                    newItem.Header = tempDock;
                    newItem.Focusable = false;
                    newItem.MouseDoubleClick += (sender, e) => doubleClickTreeViewItem(sender, e, ModuleDir);
                    newNode.Items.Add(newItem);

                    foreach (string FileName in FileNames)
                    {
                        if (FileName.ToLower().EndsWith(".xml"))
                        {
                            XMLName = FileName;
                            ThisXML = new VorteXML(XMLName);

                            ComboItem NextItem = new ComboItem();

                            NextItem.IconPath = ModuleDir + "/Icon.png";
                            NextItem.VorteXMLStruct = ThisXML;
                            NextItem.ToolName = ThisXML.NodeTitle;
                            NextItem.BinaryPath = ModuleDir + "/" + ThisXML.NodeTitle;
                            NextItem.Folder = new DirectoryInfo(Dir).Name;

                            LocalList.Add(NextItem);

                            break;
                        }
                    }
                }
                Toolbox_Treeview.Items.Add(newNode);
            }

            // Order the list alphabetically
            LocalList.Sort((x, y) => x.ToolName.CompareTo(y.ToolName));
            // Order the list alphabetically in descending order
            //LocalList.Sort((x, y) => y.ToolName.CompareTo(x.ToolName));

            ComboItems.Clear();
            ComboItem CaptionItem = new ComboItem();
            CaptionItem.ToolName = COMBOMSG;
            ComboItems.Add(CaptionItem);
            for (int i = 0; i < LocalList.Count; i++) {
                ComboItems.Add(LocalList[i]);
            };

            ToolsComboBox.ItemsSource = null;
            ToolsComboBox.ItemsSource = ComboItems;
            ToolsComboBox.SelectedIndex = 0;
        }

        private void doubleClickTreeViewItem(object sender, System.Windows.Input.MouseButtonEventArgs e, string ModuleDir)
        {
            string moduleName = new DirectoryInfo(ModuleDir).Name;
            importModule(ModuleDir + "/" + moduleName);
        }
        private void LoadSettings(System.IO.DirectoryInfo LocalDir)
        {
            ProgSettings progSettings = new ProgSettings(LocalDir.FullName + "/appsettings.json");

            // Process toolbars
            foreach (PSetting s in progSettings.PSettings)
            {
                if (s.tButton != null)
                {
                    CreateButton(s.tButton.ToolName, s.tButton.TargetControl, s.tButton.Folder);
                }
                else if (s.wfButton != null)
                {
                    CreateButtonWFF(s.wfButton.WFPath, s.wfButton.IconPath, s.wfButton.TargetControl);
                }
                else if (s.devSettings != null)
                {
                    DevBASEPORT = int.Parse(s.devSettings.BasePort);
                    DevBASEPORTCurrent = DevBASEPORT;
                    IgnoreStarts = int.Parse(s.devSettings.IgnoreNumber);
                }
                // else if ... Any other settings here.
            }

            // Process anything else at will.
        }

        private void SaveSettings()
        {
            ProgSettings ps = new ProgSettings();
            ps.PrepareSaveButtons();
            ps.AddDevSettings(DevBASEPORT, IgnoreStarts);
            ps.SaveProgSettings(ParentDir.FullName + "/appsettings.json");
        }

        //Overload that creates tool buttons.
        private void CreateButton(string ToolName, string DockPanelName, string Folder)
        {
            System.Windows.Controls.Button ModuleBtn = new System.Windows.Controls.Button();
            ModuleBtn.Content = new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(ModulesPath + "/" + Folder  + "/" + ToolName + "/" + "Icon.png")),
                ToolTip = ToolName,
                VerticalAlignment = VerticalAlignment.Center
            };
            ModuleBtn.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            ModuleBtn.Click += new RoutedEventHandler((sender, e) => importModule(ModulesPath + "/" + Folder  + "/" + ToolName+"/"+ToolName));

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
        private void CreateButtonWFF(string WFFile, string IconPath, string DockPanelName)
        {
            System.Windows.Controls.Button ModuleBtn = new System.Windows.Controls.Button();

            string FileName;
            if (System.IO.File.Exists(IconPath)) FileName = IconPath; else FileName = ParentDir.FullName + "/Images/VortexIcon.png";
            ModuleBtn.Content = new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(FileName)),
                ToolTip = WFFile,
                VerticalAlignment = VerticalAlignment.Center
            };

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
            if (!VectorData.FileAccessable(FileName)) { throw new System.Exception("File does not exist or is not accessible, maybe opened in some other software?"); }

            // Here we go. First, stop all active servers.
            TerminateAllServers();

            Prototyp.Elements.NetworkLoadAndSave open = new Prototyp.Elements.NetworkLoadAndSave(FileName);

            network = open.ImportWorkflow(vectorData, rasterData, tableData, network, ModulesPath);
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

        private void TerminateServerEvent()
        {
            foreach (NodeViewModel node in network.Nodes.Items)
            {
                if (node.IsSelected)
                {
                    if (node is Node_Module module)
                    {
                        TerminateServer(module);
                    }

                    network.Nodes.Remove(node);
                }
            }
        }

        private void importModule(string BinaryPath)
        {
            //Find lowest available port
            int port = Node_Module.GetNextPort(BASEPORT);
            bool DoLaunch = true;
            if (DevBASEPORT != 0)
            {
                if (ElapsedStarts < IgnoreStarts)
                {
                    port = DevBASEPORTCurrent;
                    DevBASEPORTCurrent++;
                    DoLaunch = false;
                }
            }

            string Url = "https://localhost:" + port.ToString();

            Node_Module nodeModule = Prototyp.Elements.BinaryLauncher.Launch(BinaryPath, Url, DoLaunch: DoLaunch);
            ElapsedStarts++;

            //Node Position
            Point TempPoint;
            TempPoint.X = 20 / networkView.ViewModel.ZoomFactor;
            TempPoint.Y = 20 / networkView.ViewModel.ZoomFactor;
            nodeModule.Position = TempPoint;

            //This is necessary to get to the delete event of the node. Otherwise the node will be deleted before we recognize it as selected in TerminateServerEvent().
            nodeModule.CanBeRemovedByUser = false;

            network.Nodes.Add(nodeModule);

            ToolsComboBox.SelectedIndex = 0;
        }

        private void AppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            TerminateAllServers();
        }

        // Public methods ---------------------------------------------------------------------------

        // Nothing here so far...

        // User control handlers --------------------------------------------------------------------
        public static VectorData CheckVectorDataCRS(VectorData tempVectorData)
        {
            HandlerCRS handlerCRS = new HandlerCRS();

            string srs = "undefined";
            string epsg = "undefined";

            OSGeo.OSR.SpatialReference OSGeoSRS = tempVectorData.SpatialReference;
            if (OSGeoSRS == null)
            {
                tempVectorData.SpatialReference = new OSGeo.OSR.SpatialReference("");
                OSGeoSRS = tempVectorData.SpatialReference;
            }

            if (OSGeoSRS.GetAttrValue("AUTHORITY", 1) != null) if (Int16.Parse(OSGeoSRS.GetAttrValue("AUTHORITY", 1)) == 4326) return tempVectorData;

            string oldWKT;
            tempVectorData.SpatialReference.ExportToWkt(out oldWKT, null);
            if (oldWKT != "")
            {
                srs = OSGeoSRS.GetName();
                epsg = OSGeoSRS.GetAttrValue("AUTHORITY", 1);
            }

            handlerCRS.CrsName.Content = srs;
            handlerCRS.EPSG.Content = epsg;
            handlerCRS.ShowDialog();

            if (!handlerCRS.OkayClicked) return tempVectorData;
            if (handlerCRS.EPSG.Content.ToString() == "undefined") return tempVectorData;

            if (OSGeoSRS.GetAttrValue("AUTHORITY", 1) != null) if (Int16.Parse(OSGeoSRS.GetAttrValue("AUTHORITY", 1)) == 4326) return tempVectorData;

            tempVectorData.SpatialReference.ImportFromEPSG(Int32.Parse(handlerCRS.EPSG.Content.ToString()));
            tempVectorData.TransformToWGS84();
            tempVectorData.ProjectToETRS89UTM();
            MessageBox.Show(tempVectorData.SpatialReference.GetAttrValue("AUTHORITY", 1));

            return tempVectorData;
        }
        public static RasterData CheckRasterDataCRS(RasterData tempRasterData)
        {
            HandlerCRS handlerCRS = new HandlerCRS();

            string srs = "undefined";
            string epsg = "undefined";

            OSGeo.OSR.SpatialReference OSGeoSRS = tempRasterData.SpatialReference;
            if (OSGeoSRS == null)
            {
                tempRasterData.SpatialReference = new OSGeo.OSR.SpatialReference("");
                OSGeoSRS = tempRasterData.SpatialReference;
            }

            if (OSGeoSRS.GetAttrValue("AUTHORITY", 1) != null) if (Int16.Parse(OSGeoSRS.GetAttrValue("AUTHORITY", 1)) == 4326) return tempRasterData;

            if (tempRasterData.SpatialReference_WKT != null)
            {
                if (tempRasterData.SpatialReference_WKT != "")
                {
                    srs = OSGeoSRS.GetName();
                    epsg = OSGeoSRS.GetAttrValue("AUTHORITY", 1);
                }
            }

            handlerCRS.CrsName.Content = srs;
            handlerCRS.EPSG.Content = epsg;
            handlerCRS.ShowDialog();

            if (!handlerCRS.OkayClicked) return tempRasterData;
            if (handlerCRS.EPSG.Content.ToString() == "undefined") return tempRasterData;

            if (OSGeoSRS.GetAttrValue("AUTHORITY", 1) != null) if (Int16.Parse(OSGeoSRS.GetAttrValue("AUTHORITY", 1)) == 4326) return tempRasterData;

            tempRasterData.SpatialReference.ImportFromEPSG(Int32.Parse(handlerCRS.EPSG.Content.ToString()));
            tempRasterData.TransformToWGS84();

            return tempRasterData;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FlatGeobuf files (*.fgb)|*.fgb|" +
                                    "Shapefiles (*.shp)|*.shp|" +
                                    "GeoTiff (*.tif)|*.tif|" +
                                    "GeoASCII (*.asc)|*.asc|" +
                                    "Raster files (*.sdat)|*.sdat|" +
                                    "Generic list data files (*.csv)|*.csv|" +
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

                    if (!VectorData.FileAccessable(openFileDialog.FileName))
                    {
                        MessageBox.Show("File is not accessible, maybe opened in some other software? Please close and try again.", "File not accessible", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VectorData peek = (new VectorData(-1, openFileDialog.FileName));//das muss doch iwie anders gehen? so wird die Datei doch zwei mal geladen... Bei großen Datenmengen kostet das Zeit
                    string geometryType = peek.FeatureCollection[0].Geometry.GeometryType;
                    peek = null;
                    VectorData tempVectorData;
                    string AuthorityString;
                    switch (geometryType)
                    {
                        case "Point":
                            tempVectorData = new VectorPointData(importDataUID, openFileDialog.FileName);
                            tempVectorData = CheckVectorDataCRS(tempVectorData);

                            AuthorityString = tempVectorData.SpatialReference.GetAttrValue("AUTHORITY", 1);
                            if (AuthorityString == null) break;
                            if (Int16.Parse(AuthorityString) != 4326) break;

                            vectorData.Add(tempVectorData);
                            break;

                        case "Line":
                            tempVectorData = new VectorLineData(importDataUID, openFileDialog.FileName);
                            tempVectorData = CheckVectorDataCRS(tempVectorData);

                            AuthorityString = tempVectorData.SpatialReference.GetAttrValue("AUTHORITY", 1);
                            if (AuthorityString == null) break;
                            if (Int16.Parse(AuthorityString) != 4326) break;

                            vectorData.Add(tempVectorData);
                            break;

                        case "Polygon":
                            tempVectorData = new VectorPolygonData(importDataUID, openFileDialog.FileName);
                            tempVectorData = CheckVectorDataCRS(tempVectorData);

                            AuthorityString = tempVectorData.SpatialReference.GetAttrValue("AUTHORITY", 1);
                            if (AuthorityString == null) break;
                            if (Int16.Parse(AuthorityString) != 4326) break;

                            vectorData.Add(tempVectorData);
                            break;

                        case "MultiPolygon":
                            tempVectorData = new VectorMultiPolygonData(importDataUID, openFileDialog.FileName);
                            tempVectorData = CheckVectorDataCRS(tempVectorData);

                            AuthorityString = tempVectorData.SpatialReference.GetAttrValue("AUTHORITY", 1);
                            if (AuthorityString == null) break;
                            if (Int16.Parse(AuthorityString) != 4326) break;

                            vectorData.Add(tempVectorData);
                            break;

                        // TODO: More cases, e.g. 'triangle' data... :-/
                        default:
                            // There should be nothing here.
                            break;
                    }

                    //Add vector data to node editor
                    for (int i = 0; i < vectorData.Count; i++)
                    {
                        if (vectorData[i].ID == vectorData[vectorData.Count - 1].ID)
                        {
                            VectorImport_Module importNode = null;

                            Type vecType = vectorData[i].GetType();
                            if (vecType.Name == "VectorPointData")
                            {
                                //importNode = new VectorImport_ModulePoint(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                                importNode = new VectorImport_ModulePoint(vectorData[i].Name, "Point", vectorData[i].ID);
                            }
                            else if (vecType.Name == "VectorLineData")
                            {
                                //importNode = new VectorImport_ModuleLine(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                                importNode = new VectorImport_ModuleLine(vectorData[i].Name, "Line", vectorData[i].ID);
                            }
                            else if (vecType.Name == "VectorPolygonData")
                            {
                                //importNode = new VectorImport_ModulePolygon(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                                importNode = new VectorImport_ModulePolygon(vectorData[i].Name, "Polygon", vectorData[i].ID);
                            }
                            else if (vecType.Name == "VectorMultiPolygonData")
                            {
                                //importNode = new VectorImport_ModuleMultiPolygon(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                                importNode = new VectorImport_ModuleMultiPolygon(vectorData[i].Name, "MultiPolygon", vectorData[i].ID);
                            }
                            // TODO: More cases, e.g. 'triangle' data... :-/
                            else
                            {
                                // There should be nothing here.
                            }

                            //Node Position
                            Point TempPoint;
                            TempPoint.X = 20 / networkView.ViewModel.ZoomFactor;
                            TempPoint.Y = 20 / networkView.ViewModel.ZoomFactor;
                            importNode.Position = TempPoint;

                            //is needed to get to the delete event of the node. Otherwise the node will be deleted before we recognize it as selected in TerminateServerEvent()
                            importNode.CanBeRemovedByUser = false;

                            network.Nodes.Add(importNode);                            
                        }
                    }
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
                    RasterData tempRasterData = (new RasterData(importDataUID, openFileDialog.FileName));
                    tempRasterData = CheckRasterDataCRS(tempRasterData);

                    string AuthorityString = tempRasterData.SpatialReference.GetAttrValue("AUTHORITY", 1);
                    if (AuthorityString == null) return;
                    if (Int16.Parse(AuthorityString) != 4326) return;
                    
                    rasterData.Add(tempRasterData);

                    //Add raster data to node editor
                    for (int i = 0; i < rasterData.Count; i++)
                    {
                        if (rasterData[i].ID == rasterData[rasterData.Count - 1].ID)
                        {
                            RasterImport_Module importNode = new RasterImport_Module(rasterData[i].Name, rasterData[i].FileType, rasterData[i].ID);

                            //Node Position
                            Point TempPoint;
                            //TempPoint = e.GetPosition(networkView);
                            //TempPoint.X = (TempPoint.X - networkView.ViewModel.DragOffset.X) / networkView.ViewModel.ZoomFactor;
                            //TempPoint.Y = (TempPoint.Y - networkView.ViewModel.DragOffset.Y) / networkView.ViewModel.ZoomFactor;
                            TempPoint.X = 20 / networkView.ViewModel.ZoomFactor;
                            TempPoint.Y = 20 / networkView.ViewModel.ZoomFactor;
                            importNode.Position = TempPoint;

                            //is needed to get to the delete event of the node. Otherwise the node will be deleted before we recognize it as selected in TerminateServerEvent()
                            importNode.CanBeRemovedByUser = false;
                            network.Nodes.Add(importNode);

                            break;
                        }
                    }
                }
                else if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".csv")
                {
                    if (tableData.Count > 0)
                    {
                        if (tableData[tableData.Count - 1] != null)
                        {
                            while (tableData[tableData.Count - 1].Busy)
                            {
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                    }
                    tableData.Add(new TableData(importDataUID, openFileDialog.FileName));

                    //Add table data to node editor.
                    for (int i = 0; i < tableData.Count; i++)
                    {
                        if (tableData[i].ID == tableData[tableData.Count - 1].ID)
                        {
                            TableImport_Module importNode = new TableImport_Module(tableData[i].Name.Substring(tableData[i].Name.LastIndexOf("\\") + 1), "Table", tableData[i].ID);

                            //Node Position
                            Point TempPoint;
                            TempPoint.X = 20 / networkView.ViewModel.ZoomFactor;
                            TempPoint.Y = 20 / networkView.ViewModel.ZoomFactor;
                            importNode.Position = TempPoint;

                            //is needed to get to the delete event of the node. Otherwise the node will be deleted before we recognize it as selected in TerminateServerEvent()
                            importNode.CanBeRemovedByUser = false;
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
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    return;
                }
                importDataUID++;

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }

        private void ComboSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int Index = ToolsComboBox.SelectedIndex;
            if (Index <= 0) return;
            if (Typing==true) return;

            Prototyp.ComboItem SelectedItem = (Prototyp.ComboItem)ToolsComboBox.SelectedItem;
           importModule(SelectedItem.BinaryPath);

            ToolsComboBox.SelectedIndex = 0;
        }

        private void ComboLostFocus(object sender, RoutedEventArgs e)
        {
            if (ToolsComboBox.IsDropDownOpen == false)
            {
                ComboSearchItems.Clear();
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
            }
        }

        private void Combo_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Typing = false;

            //Hack to get the ChangeSelectionEvent
            int tempSelectIndex = ToolsComboBox.SelectedIndex;
            ToolsComboBox.SelectedIndex = 0;
            ToolsComboBox.SelectedIndex = ToolsComboBox.SelectedIndex;
        }

        private void ComboKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Search_Toolbar.IsFocused)
            {
                return;
            }
            if (e.Key == System.Windows.Input.Key.Down |
                e.Key == System.Windows.Input.Key.Up |
                e.Key == System.Windows.Input.Key.LeftCtrl |
                e.Key == System.Windows.Input.Key.RightCtrl) return;

            string KeyPress = "";
            if (e.Key == System.Windows.Input.Key.Back) KeyPress = "Back";
            if (e.Key == System.Windows.Input.Key.Escape) KeyPress = "Esc";
            if (e.Key == System.Windows.Input.Key.Delete) KeyPress = "Del";
            if (e.Key == System.Windows.Input.Key.Enter) KeyPress = "Enter";
            if (e.Key == System.Windows.Input.Key.Return) KeyPress = "Ret";
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
                //if (ComboSearchItems.Count > 1) ToolsComboBox.SelectedIndex = 1;
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

            if (KeyPress == "Del")
            {
                ComboSearchItems.Clear();
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
                if (ToolsComboBox.IsDropDownOpen==false)
                {
                    TerminateServerEvent();
                }
                ToolsComboBox.IsDropDownOpen = false;
                return;
            }

            if (KeyPress == "Enter")
            {
                ComboSearchItems.Clear();
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
                ToolsComboBox.IsDropDownOpen = false;
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
                ToolsComboBox.IsDropDownOpen = true;
                Typing = true;
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
                Typing = false;
                return;
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //Check graph validity
            if (NodeNetwork.Toolkit.GraphAlgorithms.FindLoops(network).Any())
            {
                MessageBox.Show("Network contains loop(s). Please revert and try again.", "Loop detected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            //Keeps a list of all modules that need to have data imported
            List<NodeConnection> importConnections = new List<NodeConnection>();
            //Keeps a list of outgoing connections for each module
            Dictionary<Node_Module, List<NodeConnection>> moduleConnections = new Dictionary<Node_Module, List<NodeConnection>>();
            // Collect info for both lists
            foreach (ConnectionViewModel conn in network.Connections.Items)
            {
                NodeConnection nc = new NodeConnection();
                if (conn.Output.Parent is Node_Module)
                { 
                    nc.OutputChannel = conn.Output.GetID();
                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();

                    //Add output module to the dictionary in case they aren't already
                    var outputModule = (Node_Module)conn.Output.Parent;
                    if (!moduleConnections.ContainsKey(outputModule))
                    {
                        moduleConnections[outputModule] = new List<NodeConnection>();
                    }
                    //Add input module too, in case it is an end point in the graph
                    if (!moduleConnections.ContainsKey(nc.InputNode))
                    {
                        moduleConnections[nc.InputNode] = new List<NodeConnection>();
                    }

                    //Record outgoing connection for current output node
                    moduleConnections[outputModule].Add(nc);
                }
                else
                {
                    // Data ID in vectorData[i].ID mit foreach durch Liste iterieren.
                    nc.ImportNodeOutput = conn.Output.GetDataID();
                    nc.OutputChannel = 0;

                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();

                    importConnections.Add(nc);

                    //Add input module to node modules, to catch node modules that are connected only to import modules
                    if (!moduleConnections.ContainsKey(nc.InputNode))
                    {
                        moduleConnections[nc.InputNode] = new List<NodeConnection>();
                    }

                    //Nodes that receive inputs are the first nodes who start waiting
                    NodeProgressReport report = new NodeProgressReport();
                    report.node = nc.InputNode;
                    report.stage = NodeProgress.Waiting;
                    ReportProgress(report);
                }
            }

            //STEP 2: Traverse graph, check for possible changes in outputs (TODO)

            //STEP 3: Load inputs into the correct modules
            //
            //Initialize Progress object to asynchronously report module progress throughout the whole upload and graph traversal
            var progress = new Progress<NodeProgressReport>(ReportProgress);
            //Prepare a list of grpc streams and chunk-lists
            List<Task> uploadTasks = new List<Task>();
            foreach (NodeConnection nc in importConnections)
            {
                //Get data
                System.Diagnostics.Trace.WriteLine("Searching for import node " + nc.ImportNodeOutput + " in all data import lists.");
                string layer = null;
                foreach (VectorData v in vectorData)
                {
                    if (v.ID == nc.ImportNodeOutput)
                    {
                        layer = v.ToString(ToStringParams.ByteString);
                        break;
                    }
                }
                foreach (RasterData r in rasterData)
                {
                    if (r.ID == nc.ImportNodeOutput)
                    {
                        layer = r.ToString();
                    }
                }
                foreach (TableData t in tableData)
                {
                    if (t.ID == nc.ImportNodeOutput)
                    {
                        layer = t.ToString();
                    }
                }
                //Split into chunks of 65536 bytes (64 KiB)
                List<string> chunks = new List<string>();
                int maxChunkSize = MAX_UNSIGNED_SHORT / sizeof(Char);
                if (maxChunkSize % 2 != 0) maxChunkSize++;
                for (int i = 0; i < layer.Length; i += maxChunkSize)
                {
                    chunks.Add(layer.Substring(i, Math.Min(maxChunkSize, layer.Length - i)));
                }
                //Upload data to module through GRPC call
                uploadTasks.Add(UploadChunks(nc.InputNode, nc.InputChannel, chunks, progress));
            }
            //Run all uploads asynchronously
            try
            {
                await Task.WhenAll(uploadTasks);
            } catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Uploading data failed");
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                //MessageBox.Show("Error while uploading data!\n" + ex.ToString());
            }
             
            //STEP 4: Run modules
            //
            //Start module handling process
            try {
                await Task.Run(() => RunGraph(moduleConnections, progress));
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine("Node processing interrupted");
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                MessageBox.Show("Error while processing nodes!\n" + ex.ToString());
            }
        }

        async Task UploadChunks(Node_Module targetNode, int targetChannel, List<string> chunks, IProgress<NodeProgressReport> progress)
        {
            try
            {
                var call = targetNode.grpcConnection.SetLayer();
                await call.RequestStream.WriteAsync(new GrpcClient.ByteStream { TargetChannel = targetChannel });
                foreach (string chunk in chunks)
                {
                    //TODO: Add targeted channel once new grpc protobuf definition is available
                    await call.RequestStream.WriteAsync(new GrpcClient.ByteStream { Chunk = Google.Protobuf.ByteString.FromBase64(chunk) });
                }
                await call.RequestStream.CompleteAsync();
                await call.ResponseAsync;
            }
            catch (Exception ex)
            {
                NodeProgressReport report = new NodeProgressReport();
                report.node = targetNode;
                report.stage = NodeProgress.Interrupted;
                progress.Report(report);
                throw (ex);
            }
        }

        async Task RunGraph(Dictionary<Node_Module, List<NodeConnection>> sendList, IProgress<NodeProgressReport> progress)
        {
            //Concept: Receive a dictionary which for each node has the outgoing connections (sendList)
            //For each node, also count the incoming connections (incomingCount) 
            //Begin processing nodes that have no incoming connections. Once they are done, send to all outgoing connections
            //Once data is sent over a connection, subtract from the count of incoming connections
            //Once the count hits zero, node is ready to send to all its outgoing connections, etc...

            //Start by marking all node modules as waiting (for progress indicators in UI)
            System.Diagnostics.Trace.WriteLine("Collecting info for " + sendList.Keys.Count + " nodes...");
            foreach (var node in sendList.Keys)
            {
                NodeProgressReport report = new NodeProgressReport();
                report.node = node;
                report.stage = NodeProgress.Waiting;
                progress.Report(report);
            }

            //Prepare input occurence Dictionary and set all nodes to 0 (no occurrences as input)
            Dictionary<Node_Module, int> incomingCount = new Dictionary<Node_Module, int>();
            foreach (var node in sendList.Keys)
            {
                incomingCount.Add(node, 0);
            }

            //Get lists of connections from dictionary and flatten into one list of all connections
            //Traverse that list and count each occurence of node as input
            foreach (NodeConnection nc in sendList.Values.SelectMany(x => x))
            {
                incomingCount[nc.InputNode]++;
            }

            System.Diagnostics.Trace.WriteLine("Node info collected, generating starting tasks...");
            //Generate tasks for all starting nodes (input count == 0) and put them into a collection
            var nodeTasks = incomingCount.Where(pair => pair.Value == 0).Select(pair => runGRPCNode(pair.Key, sendList[pair.Key], progress)).ToList();

            //Start tasks, wait for whichever task completes first (whenAny)
            //Once a task is done, remove it from the list, check for any newly activated nodes and add them to the task list
            //Keeps going until the list is empty
            System.Diagnostics.Trace.WriteLine("Starting task loop with " + nodeTasks.Count + " starting tasks.");
            while (nodeTasks.Any())
            {
                Task<Node_Module> finishedNode = await Task.WhenAny(nodeTasks);
                nodeTasks.Remove(finishedNode);
                //Iterate through all nodes that received data from this node. Subtract 1 from their input count.
                //If the input count hits zero, that means all inuts are resolved, and the node can be started.
                foreach(var conn in sendList[finishedNode.Result])
                {
                    incomingCount[conn.InputNode]--;
                    if (incomingCount[conn.InputNode] == 0)
                    {
                        nodeTasks.Add(runGRPCNode(conn.InputNode, sendList[conn.InputNode], progress));
                    }
                }
            }
        }

        async private Task<Node_Module> runGRPCNode(Node_Module node, List<NodeConnection> sendList, IProgress<NodeProgressReport> progress)
        {
            //try-catch so that the offending node can be marked with the interrupted status if it fails at any point
            try {
                //  STEP 1:
                //  Uploading current node configuration
                System.Diagnostics.Trace.WriteLine("Uploading settings for node " + node.Url);
                var settings = new GrpcClient.Settings
                {
                    Mapping = node.ParamsToProtobufStruct()
                };
                node.grpcConnection.SendSettings(settings);

                //  STEP 2:
                //  RUN NODE
                System.Diagnostics.Trace.WriteLine("Running node " + node.Url);
                var request = new GrpcClient.RunRequest { };
                NodeProgressReport report = new NodeProgressReport();
                report.node = node;
                using (var call = node.grpcConnection.RunProcess(request))
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
                //  STEP 3:
                //  IMMEDIATELY SEND DATA TO ALL DOWNSTREAM NODES
                var sendingTasks = new List<Grpc.Core.AsyncUnaryCall<GrpcClient.TransferResponse>>();
                foreach (var send in sendList)
                {
                    Uri fromUrl = new Uri(node.Url, UriKind.Absolute);
                    Uri toUrl = new Uri(send.InputNode.Url, UriKind.Absolute);
                    //Check if it this is a remote -> local send (which can't be a normal send, but has to be turned around to be a request from local to remote)
                    if ( !fromUrl.IsLoopback && toUrl.IsLoopback )
                    {
                        System.Diagnostics.Trace.WriteLine(send.InputNode.Url + "[" + send.InputChannel + "]" + "requesting data from " + node.Url + "[" + send.OutputChannel + "]");
                        var receiveRequest = new GrpcClient.ChannelInfo
                        {
                            TargetNodeUrl = node.Url,
                            SourceChannelID = send.OutputChannel,
                            TargetChannelID = send.InputChannel
                        };
                        sendingTasks.Add(send.InputNode.grpcConnection.ReceiveDataAsync(receiveRequest));
                    } 
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Sending data from " + node.Url + "[" + send.OutputChannel + "] to " + send.InputNode.Url + "[" + send.InputChannel + "]");
                        var sendRequest = new GrpcClient.ChannelInfo
                        {
                            TargetNodeUrl = send.InputNode.Url,
                            SourceChannelID = send.OutputChannel,
                            TargetChannelID = send.InputChannel
                        };
                        sendingTasks.Add(node.grpcConnection.SendDataAsync(sendRequest));
                    }
                }
                await Task.WhenAll(sendingTasks.Select(c => c.ResponseAsync));
                return node;
            } 
            catch (Exception ex)
            {
                NodeProgressReport report = new NodeProgressReport();
                report.node = node;
                report.stage = NodeProgress.Interrupted;
                progress.Report(report);
                throw (ex);
            }
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

        private void ModuleDesigner_Click(object sender, RoutedEventArgs e)
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

        private void DeveloperMode_Click(object sender, RoutedEventArgs e)
        {
            Developer_mode developer_mode = new Developer_mode();
            developer_mode.Owner = this;

            if (DevBASEPORT != 0) developer_mode.TxtStartPort.Text = DevBASEPORT.ToString(); else developer_mode.TxtStartPort.Text = BASEPORT.ToString();
            developer_mode.TxtNumberIgnore.Text = IgnoreStarts.ToString();

            developer_mode.ShowDialog();

            if (!developer_mode.OkayClicked) return;

            DevBASEPORT = int.Parse(developer_mode.TxtStartPort.Text);
            IgnoreStarts = int.Parse(developer_mode.TxtNumberIgnore.Text);            
        }

        private void FormatLayout_Click(object sender, RoutedEventArgs e)
        {
            // Startet ein automatisches Network-Arragement, mehr oder weniger gut. Vielleicht irgendwann mal nützlich.
            NodeNetwork.Toolkit.Layout.ForceDirected.ForceDirectedLayouter layout = new NodeNetwork.Toolkit.Layout.ForceDirected.ForceDirectedLayouter();
            layout.Layout(new NodeNetwork.Toolkit.Layout.ForceDirected.Configuration { Network = network }, 10000);
        }

        private void RemoveModule_Click(object sender, RoutedEventArgs e)
        {
            TerminateServerEvent();
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
                bool includeData = false;
                if (MessageBox.Show("Include data sets? This will increase data size and processing time but will make the user of the workflow independent from the data files.", "Include data?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) includeData = true; else includeData = false;

                Cursor = System.Windows.Input.Cursors.Wait;

                if (System.IO.File.Exists(saveFileDialog.FileName)) System.IO.File.Delete(saveFileDialog.FileName);

                Prototyp.Elements.NetworkLoadAndSave save = new Prototyp.Elements.NetworkLoadAndSave(network, vectorData, rasterData, tableData, saveFileDialog.FileName, includeData);

                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
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
            if (chooseModuleWindow.selectedModuleList.Count > 0)
            {
                foreach(Prototyp.ComboItem Item in chooseModuleWindow.selectedModuleList)
                {
                    System.IO.FileInfo DirInfo = new System.IO.FileInfo(Item.IconPath);
                    string parentDir = DirInfo.Directory.Name;
                    CreateButton(Item.ToolName, dockPanel.Name, parentDir);
                }
                
                SaveSettings();
            }
        }

        private void removeBtn_Click(System.Windows.Controls.Button ModuleBtn, System.Windows.Controls.DockPanel dockPanel)
        {
            dockPanel.Children.Remove(ModuleBtn);
            SaveSettings();
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

                CreateButton(WFFile, IconPath, dockPanel.Name);
                SaveSettings();
            }
        }

        private void Search_Toolbar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search_Toolbar.Text == "Search...")
            {
                Search_Toolbar.Text = "";
            }
        }

        private void Search_Toolbar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Search_Toolbar.Text == "")
            {
                Search_Toolbar.Text = "Search...";
            }
        }

        private void Search_Toolbar_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && Search_Toolbar.Text!="")
            {
                ToolsTabControl.SelectedIndex= 1;
            }
        }
    }
}