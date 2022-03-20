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
            openFileDialog.Filter = "FlatGeobuf files (*.fgb)|*.fgb|Shapefiles (*.shp)|*.shp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = openFileDialog.Filter.Length;

            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
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

                if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".shp" | Path.GetExtension(openFileDialog.FileName).ToLower() == ".geojson") //TODO: Auch andere GDAL-Layer-Dateitypen möglich.
                {
                    VectorData.InitGDAL();
                    OSGeo.OGR.DataSource MyDS;
                    MyDS = OSGeo.OGR.Ogr.Open(openFileDialog.FileName, 0);

                    if (MyDS != null) vectorData.Add(new VectorData(MyDS.GetLayerByIndex(0)));
                }
                else if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".fgb")
                {
                    vectorData.Add(new VectorData(openFileDialog.FileName));
                }
                else
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    return;
                }

                if (vectorData[vectorData.Count - 1] != null)
                {
                    MainWindowHelpers mainWindowHelpers = new MainWindowHelpers();
                    mainWindowHelpers.AddTreeViewChild(vectorData[vectorData.Count - 1]);
                }

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }

        private void WithInButton_Click(object sender, RoutedEventArgs e)
        {
            var nodeModule = new Node_Module("..\\..\\..\\..\\Modules\\Buffer\\Buffer.xml");
            network.Nodes.Add(nodeModule);
        }

        //Objekt wird in den NodeEditor gezogen
        public void DropTargetEventNodeEditor(object sender, DragEventArgs e)
        {
            Import_Module importNode = new Import_Module();

            for (int i = 0; i < vectorData.Count; i++)
            {
                if (vectorData[i].Name == (string) e.Data.GetData("Vectorname")) //!!!!!!!!!! TODO: Eindeutige ID verwenden.
                {
                    importNode.importNodeOutput.Name = vectorData[i].Name;
                    importNode.importNodeOutput.Value = System.Reactive.Linq.Observable.Return(vectorData[i]);
                    network.Nodes.Add(importNode);
                    break;
                }
            }
        }
    }
}

