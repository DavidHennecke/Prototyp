using DynamicData;
using MaxRev.Gdal.Core;
using Microsoft.Win32;
using NodeNetwork.ViewModels;
using OSGeo.OGR;
using Prototyp.Elements;
using Prototyp.Modules;
using System;
using System.IO;
using System.Windows;

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;

        //Create a new viewmodel for the NetworkView
        NetworkViewModel network = new NetworkViewModel();
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();
            AppWindow = this;
            networkView.ViewModel = network;
        }

        public int firstLayer = 0;
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".*";
            dlg.Filter = "SHP Files (*.shp)|*.shp|All files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                foreach (string sFilename in dlg.FileNames)
                {
                    string ext = Path.GetExtension(sFilename);

                    if (ext == ".shp")
                    {
                        Shapefile shapefile = new Shapefile();
                        shapefile.sFilename = sFilename;
                        shapefile.InitLayer(shapefile.sFilename);
                    }
                }
            }
        }

        private void WithInButton_Click(object sender, RoutedEventArgs e)
        {
            //var nodeModule = new Node_Module("C:\\Users\\Hennecke\\ownCloud\\WFLO\\Vortex\\Node-Beschreibungs-Theorie\\VorteXML.xml");
            var nodeModule = new Node_Module("..\\..\\..\\Modules\\Buffer\\Buffer.xml");
            network.Nodes.Add(nodeModule);
        }

        //Datei wird in NodeEditor gezogen
        public void DropTargetEventNodeEditor(object sender, DragEventArgs e)
        {
            string Filepath = (string)e.Data.GetData("Filename");
            var importNode = new Import_Module();

            DataSource ds = Ogr.Open(Filepath, 0);
            Layer Layer = ds.GetLayerByIndex(0);

            importNode.importNodeOutput.Value = System.Reactive.Linq.Observable.Return(Layer);
            importNode.importNodeOutput.Name = (string)e.Data.GetData("Layername");
            network.Nodes.Add(importNode);
        }
    }
}

