using DynamicData;
using Microsoft.Win32;
using NodeNetwork.ViewModels;
using Prototyp.Elements;
using Prototyp.Modules;
using System;
using System.IO;
using System.Windows;

namespace Prototyp
{
    public partial class MainWindow : Window
    {
        private const int BASEPORT = 5000;

        private System.Collections.Generic.List<VectorData> vectorData = new System.Collections.Generic.List<VectorData>();
        private System.Collections.Generic.List<RasterData> rasterData = new System.Collections.Generic.List<RasterData>();
        private System.Collections.Generic.List<VorteXML> vorteXMLStructures = new System.Collections.Generic.List<VorteXML>();
        private System.Collections.Generic.List<string> BinaryPaths = new System.Collections.Generic.List<string>();

        private string ModulesPath;

        public static MainWindow AppWindow;
        NetworkViewModel network = new NetworkViewModel();

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor: Create a new viewmodel for the NetworkView.
        public MainWindow()
        {
            // Startup NetworkView.
            InitializeComponent();
            AppWindow = this;
            networkView.ViewModel = network;
            
            // Initialize modules path and start parsing.
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
                        vorteXMLStructures.Add(ThisXML);
                        BinaryPaths.Add(Dir + "\\" + ThisXML.NodeTitle);

                        //TODO: Grrr, Buttons sollten ein Array von Buttons sein!
                        if (ThisXML.NodeTitle == "Buffer")
                        {
                            ToolButton1.Text = ThisXML.NodeTitle;
                            Button1.ToolTip = ThisXML.NodeTitle;
                        }
                        if (ThisXML.NodeTitle == "RasterTest")
                        {
                            ToolButton2.Text = ThisXML.NodeTitle;
                            Button2.ToolTip = ThisXML.NodeTitle;
                        }

                        System.Windows.Media.Imaging.BitmapImage MyImage = new System.Windows.Media.Imaging.BitmapImage();
                        MyImage.BeginInit();
                        MyImage.UriSource = new Uri(Dir + "/Icon.png", UriKind.Absolute);
                        MyImage.EndInit();

                        if (ThisXML.NodeTitle == "Buffer")
                        {
                            B1Image.Source = MyImage;
                        }
                        if (ThisXML.NodeTitle == "RasterTest")
                        {
                            B2Image.Source = MyImage;
                        }

                        break;
                    }
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

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //Find lowest available node ID
            int port = BASEPORT;
            foreach (Node_Module node in network.Nodes.Items)
            {
                if (node.Port == port)
                {
                    port++;
                }
            }
            if (!Node_Module.PortAvailable(port)) throw new System.Exception("This port is not available."); //TODO: Besseres Handling. Nächsten Kandidaten holen?

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;
            using (System.Diagnostics.Process myProcess = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo myProcessStartInfo = new System.Diagnostics.ProcessStartInfo(BinaryPaths[0] + ".exe", port.ToString());

                //myProcessStartInfo.CreateNoWindow = true; //Ja, dies macht das Server-Window wirklich unsichtbar. Sicherstellen, dass der Krempel terminiert wird.
                myProcessStartInfo.UseShellExecute = false; //Muss für .NETCore tatsächlich false sein, weil ShellExecute wirklich nur auf der Windows-Plattform verfügbar ist.
                myProcess.StartInfo = myProcessStartInfo;
                myProcess.Start();

                //Establish GRPC connection
                //TODO: nicht nur localhost
                string url = "https://localhost:" + port;
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);
            }

            Node_Module nodeModule = new Node_Module(BinaryPaths[0] + ".xml", port, grpcConnection);

            network.Nodes.Add(nodeModule);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            //Find lowest available node ID
            int port = BASEPORT;
            foreach (Node_Module node in network.Nodes.Items)
            {
                if (node.Port == port)
                {
                    port++;
                }
            }
            if (!Node_Module.PortAvailable(port)) throw new System.Exception("This port is not available."); //TODO: Besseres Handling. Nächsten Kandidaten holen?

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;
            using (System.Diagnostics.Process myProcess = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo myProcessStartInfo = new System.Diagnostics.ProcessStartInfo(BinaryPaths[1] + ".exe", port.ToString());

                //myProcessStartInfo.CreateNoWindow = true; //Ja, dies macht das Server-Window wirklich unsichtbar. Sicherstellen, dass der Krempel terminiert wird.
                myProcessStartInfo.UseShellExecute = false; //Muss für .NETCore tatsächlich false sein, weil ShellExecute wirklich nur auf der Windows-Plattform verfügbar ist.
                myProcess.StartInfo = myProcessStartInfo;
                myProcess.Start();

                //Establish GRPC connection
                //TODO: nicht nur localhost
                string url = "https://localhost:" + port;
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);
            }

            Node_Module nodeModule = new Node_Module(BinaryPaths[1] + ".xml", port, grpcConnection);

            network.Nodes.Add(nodeModule);
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
                        importNode.Position = e.GetPosition(networkView);
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
        }
    }
}

