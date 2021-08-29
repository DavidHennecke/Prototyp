using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using MaxRev.Gdal.Core;
using System.Globalization;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
using Prototyp.Elements;
using Geometry = OSGeo.OGR.Geometry;
using NodeNetwork.ViewModels;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Reactive.Linq;
using NodeNetwork.Views;
using Prototyp.Modules;

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
            //Assign the viewmodel to the view.
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
                        AddShapefile(sFilename);
                    }
                }
            }
        }


        private void BufferButton_Click(object sender, RoutedEventArgs e)
        {
            var bufferNode = new Buffer_Module();
            network.Nodes.Add(bufferNode);
        }

        private void WithInButton_Click(object sender, RoutedEventArgs e)
        {
            var withInNode = new WithIn_Module();
            network.Nodes.Add(withInNode);
        }

        private void MappingButton_Click(object sender, RoutedEventArgs e)
        {
            var mappingNode = new Mapping_Module();
            network.Nodes.Add(mappingNode);
        }


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

        public void AddShapefile(string sFilename)
        {
            Shapefile shapefile = new Shapefile();
            shapefile.sFilename = sFilename;
            shapefile.InitLayer(shapefile.sFilename);
            if (firstLayer == 0)
            {
                Geopoint newCenter = shapefile.ZoomToExtent();
                map.TrySetViewAsync(newCenter, 10);
                firstLayer += 1;
            }
        }



    }
}

