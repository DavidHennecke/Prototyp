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
using Prototyp.Custom_Controls;
using Geometry = OSGeo.OGR.Geometry;
using Prototyp.Modules.Imports;

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();
            AppWindow = this;
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



        UIElement dragObject = null;
        Point offset;

        public void DropTargetEventNodeEditor(object sender, DragEventArgs e)
        {
            Canvas target = (Canvas)sender;
            string fileName = (string)e.Data.GetData("Filename");
            string LayerName = (string)e.Data.GetData("Layername");
            ShapefileImport importModul = new ShapefileImport();
            importModul.ShapeImportFileName.Text = LayerName;
            importModul.ShapeImportFilePath.Text = fileName;
            Canvas.SetTop(importModul, 20);
            Canvas.SetLeft(importModul, 20);
            importModul.ShapeImportFileName.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler ((sender, e) => Module_PreviewMouseDown(sender, e, importModul));
            target.Children.Add(importModul);

        }

        private void Module_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e, ShapefileImport importModul)
        {
            this.dragObject = importModul as UIElement;
            this.offset = e.GetPosition(this.NodeEditor);
            this.offset.X -= Canvas.GetLeft(this.dragObject);
            this.offset.Y -= Canvas.GetTop(this.dragObject);
            this.NodeEditor.CaptureMouse();
        }

        private void NodeEditor_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.dragObject == null)
                return;
            var position = e.GetPosition(sender as IInputElement);
            Canvas.SetLeft(this.dragObject, position.X - this.offset.X);
            Canvas.SetTop(this.dragObject, position.Y - this.offset.Y);
        }

        private void NodeEditor_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.dragObject = null;
            this.NodeEditor.ReleaseMouseCapture();
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

