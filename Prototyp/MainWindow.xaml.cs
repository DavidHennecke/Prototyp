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
using Prototyp.Modules.Tools;

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
        public Point exNode;
        Point inNode;
        


        public void DropTargetEventNodeEditor(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(NodeEditor);

            ShapefileImport importModul = new ShapefileImport();
            string fileName = (string)e.Data.GetData("Filename");
            string LayerName = (string)e.Data.GetData("Layername");
            importModul.ShapeImportFileName.Text = LayerName;
            importModul.exportFile.FilePath.Text = fileName;
            Canvas.SetTop(importModul, dropPoint.Y);
            Canvas.SetLeft(importModul, dropPoint.X);
            importModul.ShapeImportFileName.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler ((sender, e) => ShapefileImpot_PreviewMouseDown(sender, e, importModul));
            NodeEditor.Children.Add(importModul);
            importModul.ModulDeleteButton.Click += new RoutedEventHandler((sender, e) => ShapefileImport.DeleteModul(sender, e, importModul));
            //importModul.exportFile.exportNode.Click += new RoutedEventHandler((sender, e) => exNode = Export_File_ModuleItem.StartDrawConnectionLine(sender, e, exNode));
        }

        private void DrawConnectionLine(object sender, RoutedEventArgs e)
        {
            System.Windows.Shapes.Line nodeConnection = new System.Windows.Shapes.Line();
            inNode = System.Windows.Input.Mouse.GetPosition(NodeEditor);
            nodeConnection.Stroke = Brushes.Black;
            nodeConnection.X1 = exNode.X;
            nodeConnection.Y1 = exNode.Y;
            nodeConnection.X2 = inNode.X;
            nodeConnection.Y2 = inNode.Y;

            NodeEditor.Children.Add(nodeConnection);
        }

        private void ShapefileImpot_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e, ShapefileImport importModul)
        {
            this.dragObject = importModul as UIElement;
            this.offset = e.GetPosition(this.NodeEditor);
            this.offset.X -= Canvas.GetLeft(this.dragObject);
            this.offset.Y -= Canvas.GetTop(this.dragObject);
            this.NodeEditor.CaptureMouse();
        }

        private void BufferModule_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e, Module_Buffer newBufferModule)
        {
            this.dragObject = newBufferModule as UIElement;
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

        private void BufferButton_Click(object sender, RoutedEventArgs e)
        {
            Module_Buffer newBufferModule = new Module_Buffer();
            Canvas.SetTop(newBufferModule, 20);
            Canvas.SetLeft(newBufferModule, 20);
            newBufferModule.BufferHeader.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => BufferModule_PreviewMouseDown(sender, e, newBufferModule));
            NodeEditor.Children.Add(newBufferModule);
            newBufferModule.ModulDeleteButton.Click += new RoutedEventHandler((sender, e) => Module_Buffer.DeleteModul(sender, e, newBufferModule));
            //newBufferModule.exportFile.exportNode.Click += new RoutedEventHandler((sender, e) => exNode = Export_File_ModuleItem.StartDrawConnectionLine(sender, e, exNode));
            newBufferModule.importNode.Click += new RoutedEventHandler(DrawConnectionLine);
        }


    }
}

