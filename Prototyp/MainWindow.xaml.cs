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
Bug in RxApp.cs, der auftritt, wenn z.B. der Ausgang eines Buffers an den Eingang eines anderen
    Buffers angeschlossen wird. Wodurch kommt das, wie beheben?

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

    public partial class MainWindow : Window
    {
        private const int BASEPORT = 5000;

        private System.Collections.Generic.List<VectorData> vectorData = new System.Collections.Generic.List<VectorData>();
        private System.Collections.Generic.List<RasterData> rasterData = new System.Collections.Generic.List<RasterData>();
        private System.Collections.Generic.List<int> UsedPorts = new System.Collections.Generic.List<int>();
        private System.Collections.Generic.List<ComboItem> ComboItems = new System.Collections.Generic.List<ComboItem>();

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

        public void ParseModules(string Path)
        {
            string[] SubDirs = Directory.GetDirectories(Path);
            string[] FileNames;
            string XMLName;
            VorteXML ThisXML;

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
                        NextItem.BinaryPath = Dir + "\\" + ThisXML.NodeTitle;

                        ComboItems.Add(NextItem);

                        break;
                    }
                }
            }

            // Order the list alphabetically
            ComboItems.Sort((x, y) => x.ToolName.CompareTo(y.ToolName));

            // Order the list alphabetically in descending order
            //ComboItems.Sort((x, y) => y.ToolName.CompareTo(x.ToolName));

            ToolsComboBox.ItemsSource = ComboItems;
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

                if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".shp" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".geojson") //TODO: Auch andere GDAL-Layer-Dateitypen möglich.
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

                    VectorData.InitGDAL();
                    OSGeo.OGR.DataSource MyDS;
                    MyDS = OSGeo.OGR.Ogr.Open(openFileDialog.FileName, 0);

                    if (MyDS != null)
                    {
                        vectorData.Add(new VectorData(MyDS.GetLayerByIndex(0)));
                        MainWindowHelpers mainWindowHelpers = new MainWindowHelpers();
                        mainWindowHelpers.AddTreeViewChild(vectorData[vectorData.Count - 1]);
                    }
                }
                else if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".fgb")
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
            if ((string)e.Data.GetData("Type") == "Vector")
            {
                VectorImport_Module importNode = new VectorImport_Module();

                for (int i = 0; i < vectorData.Count; i++)
                {
                    if (vectorData[i].ID.ToString() == (string)e.Data.GetData("ID"))
                    {
                        importNode.importNodeOutput.Name = vectorData[i].Name;
                        importNode.importNodeOutput.Value = System.Reactive.Linq.Observable.Return(vectorData[i]);

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
            else if ((string)e.Data.GetData("Type") == "Raster")
            {
                RasterImport_Module importNode = new RasterImport_Module();

                for (int i = 0; i < rasterData.Count; i++)
                {
                    if (rasterData[i].ID.ToString() == (string)e.Data.GetData("ID"))
                    {
                        importNode.importNodeOutput.Name = rasterData[i].Name;
                        importNode.importNodeOutput.Value = System.Reactive.Linq.Observable.Return(rasterData[i]);
                        importNode.Position = e.GetPosition(networkView);
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
            if (Index == -1) return;

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
                myProcess.Start();

                //Establish GRPC connection
                //TODO: nicht nur localhost
                string url = "https://localhost:" + port.ToString();
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);
            }

            Node_Module nodeModule = new Node_Module(ComboItems[Index].BinaryPath + ".xml", grpcConnection);

            UsedPorts.Add(port);
            network.Nodes.Add(nodeModule);

            ToolsComboBox.SelectedIndex = -1;
        }
    }
}

