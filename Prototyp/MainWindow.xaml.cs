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
        public VectorData vectorData;

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
                if (vectorData != null) while (vectorData.Busy) { System.Threading.Thread.Sleep(50); }

                if (Path.GetExtension(openFileDialog.FileName) == ".shp")
                {
                    VectorData.InitGDAL();
                    OSGeo.OGR.DataSource MyDS;
                    MyDS = OSGeo.OGR.Ogr.Open(openFileDialog.FileName, 0);

                    if (MyDS != null) vectorData = new VectorData(MyDS.GetLayerByIndex(0));
                }
                else if (Path.GetExtension(openFileDialog.FileName) == ".fgb")
                {
                    vectorData = new VectorData(openFileDialog.FileName);
                }

                if (vectorData != null)
                {
                    MainWindowHelpers mainWindowHelpers = new MainWindowHelpers();
                    mainWindowHelpers.AddTreeViewChild(vectorData);
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
            var importNode = new Import_Module();

            importNode.importNodeOutput.Value = System.Reactive.Linq.Observable.Return(vectorData);
            importNode.importNodeOutput.Name = (string) e.Data.GetData("Vectorname"); //TODO: Eindeutige ID verwenden.
            network.Nodes.Add(importNode);
        }
    }
}

