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
        public System.Collections.Generic.List<VectorData> vectorData = new System.Collections.Generic.List<VectorData>();
        public System.Collections.Generic.List<RasterData> rasterData = new System.Collections.Generic.List<RasterData>();

        //Create a new viewmodel for the NetworkView
        public static MainWindow AppWindow;
        NetworkViewModel network = new NetworkViewModel();
        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            networkView.ViewModel = network;
        }

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

        private void WithInButton_Click(object sender, RoutedEventArgs e)
        {
            var nodeModule = new Node_Module("..\\..\\..\\..\\Modules\\Buffer\\Buffer.xml");
            network.Nodes.Add(nodeModule);
            //TODO: Binary hier starten.
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
                        network.Nodes.Add(importNode);
                        break;
                    }
                }
            }
        }
    }
}

