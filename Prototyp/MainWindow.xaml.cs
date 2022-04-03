using DynamicData;
using Microsoft.Win32;
using NodeNetwork.ViewModels;
using Prototyp.Elements;
using Prototyp.Modules;
using System;
using System.IO;
using System.Windows;

/* -------------------------------

TODO:


------------------------------- */

namespace Prototyp
{
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
        public bool isModule { get; set; }
    }

    public partial class MainWindow : Window
    {
        private const int BASEPORT = 5000;
        
        private const string COMBOMSG = "Select your tool here...";
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private bool Typing = false;

        private System.Collections.Generic.List<VectorData> vectorData = new System.Collections.Generic.List<VectorData>();
        private System.Collections.Generic.List<RasterData> rasterData = new System.Collections.Generic.List<RasterData>();
        private System.Collections.Generic.List<int> UsedPorts = new System.Collections.Generic.List<int>();
        private System.Collections.Generic.List<ComboItem> ComboItems = new System.Collections.Generic.List<ComboItem>();
        private System.Collections.Generic.List<NodeConnection> nodeConnections = new System.Collections.Generic.List<NodeConnection>();

        private string ModulesPath;
        private NetworkViewModel network = new NetworkViewModel();

        public static MainWindow AppWindow;

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor: Initialize.
        public MainWindow()
        {
            // Init WPF.
            InitializeComponent();

            // Startup NetworkView.
            AppWindow = this;
            networkView.ViewModel = network;

            // Init modules path and start parsing.
            //TODO: Besseren Weg finden, um das parent directory zu bestimmen.
            ModulesPath = System.IO.Directory.GetCurrentDirectory();
            System.IO.DirectoryInfo ParentDir = System.IO.Directory.GetParent(ModulesPath);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ParentDir = System.IO.Directory.GetParent(ParentDir.FullName);
            ModulesPath = ParentDir.FullName + "\\Custom modules";
            
            ParseModules(ModulesPath);
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

                    vectorData.Add(new VectorData(openFileDialog.FileName));
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
                        VectorImport_Module importNode = new VectorImport_Module(vectorData[i].Name, vectorData[i].FeatureCollection[0].Geometry.GeometryType, vectorData[i].ID);
                        
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

            //Find lowest available node ID
            int port = BASEPORT;
            for (int i = 0; i < UsedPorts.Count; i++)
            {
                if (port == UsedPorts[i])
                {
                    port++;
                }
            }
            if (!Node_Module.PortAvailable(port)) throw new System.Exception("This port is not available."); //TODO: Besseres Handling. Nächsten Kandidaten holen?

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;

            using (System.Diagnostics.Process myProcess = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo myProcessStartInfo = new System.Diagnostics.ProcessStartInfo(ComboItems[Index].BinaryPath + ".exe", port.ToString());

                //myProcessStartInfo.CreateNoWindow = true; //Ja, dies macht das Server-Window wirklich unsichtbar. Sicherstellen, dass der Krempel terminiert wird.
                myProcessStartInfo.UseShellExecute = false; //Muss für .NETCore tatsächlich false sein, weil ShellExecute wirklich nur auf der Windows-Plattform verfügbar ist.
                myProcess.StartInfo = myProcessStartInfo;
                try
                {
                    myProcess.Start();
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

                // Establish GRPC connection
                // TODO: nicht nur localhost
                string url = "https://localhost:" + port.ToString();
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);
                Node_Module nodeModule = new Node_Module(ComboItems[Index].BinaryPath + ".xml", grpcConnection, url);
                UsedPorts.Add(port);
                network.Nodes.Add(nodeModule);
            }
            ToolsComboBox.SelectedIndex = 0;
        }

        private void ComboKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ToolsComboBox.IsDropDownOpen = true;
            Typing = true;

            if (ComboItems[0].ToolName == COMBOMSG) ComboItems[0].ToolName = "";

            if (e.Key == System.Windows.Input.Key.Back)
            {
                if (ComboItems[0].ToolName == "") return;
                ComboItems[0].ToolName = ComboItems[0].ToolName.Substring(0, ComboItems[0].ToolName.Length - 1);
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                for (int i = 1; i < ComboItems.Count; i++)
                {
                    if (ComboItems[i].ToolName.ToLower().Contains(ComboItems[0].ToolName))
                    {
                        ToolsComboBox.SelectedIndex = i;
                        break;
                    }
                }
                return;
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                ToolsComboBox.IsDropDownOpen = false;
                ComboItems[0].ToolName = COMBOMSG;
                ToolsComboBox.ItemsSource = null;
                ToolsComboBox.ItemsSource = ComboItems;
                ToolsComboBox.SelectedIndex = 0;
                Typing = false;
                return;
            }

            if (e.Key == System.Windows.Input.Key.Return | e.Key == System.Windows.Input.Key.Enter)
            {
                for (int i = 1; i < ComboItems.Count; i++)
                {
                    if (ComboItems[i].ToolName.ToLower().Contains(ComboItems[0].ToolName))
                    {
                        ToolsComboBox.SelectedIndex = 0;
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

            if (ALPHABET.Contains(e.Key.ToString())) ComboItems[0].ToolName = ComboItems[0].ToolName + e.Key.ToString().ToLower();
            ToolsComboBox.ItemsSource = null;
            ToolsComboBox.ItemsSource = ComboItems;
            for (int i = 1; i < ComboItems.Count; i++)
            {
                if (ComboItems[i].ToolName.ToLower().Contains(ComboItems[0].ToolName))
                {
                    ToolsComboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {         
            // Collect all current node-channel-connections
            foreach (ConnectionViewModel conn in network.Connections.Items)
            {
                MessageBox.Show(conn.Output.Parent.GetType().ToString());
                NodeConnection nc = new NodeConnection();
                if (conn.Output.Parent is Node_Module)
                {
                    // URL im ((Node_Module)conn.Output.Parent).url
                    nc.OutputNode = (Node_Module)conn.Output.Parent;
                    nc.OutputChannel = conn.Output.GetID();

                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();

                    nc.isModule = true;
                    nodeConnections.Add(nc);
                    MessageBox.Show(nc.OutputNode + "_" + nc.OutputChannel + " -> " + nc.InputNode + "_" + nc.InputChannel);
                }
                else
                {
                    // Data ID in vectorData[i].ID mit foreach durch Liste iterieren.
                    nc.ImportNodeOutput = conn.Output.GetDataID();
                    nc.OutputChannel = 0;

                    nc.InputNode = (Node_Module)conn.Input.Parent;
                    nc.InputChannel = conn.Input.GetID();

                    nc.isModule = false;
                    nodeConnections.Add(nc);
                    MessageBox.Show(nc.ImportNodeOutput +"_" + nc.OutputChannel + " -> " + nc.InputNode + "_" + nc.InputChannel);
                }
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
    }
}

