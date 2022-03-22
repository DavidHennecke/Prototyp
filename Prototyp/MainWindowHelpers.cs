using System;
using System.Windows;

namespace Prototyp
{
    public class MainWindowHelpers
    {
        private System.Random rnd = new System.Random();

        private Prototyp.Custom_Controls.VectorListViewItem newVectorChild = new Prototyp.Custom_Controls.VectorListViewItem();
        private Prototyp.Custom_Controls.RasterListViewItem newRasterChild = new Prototyp.Custom_Controls.RasterListViewItem();

        private Windows.UI.Color vectorColor = new Windows.UI.Color();
        private Windows.UI.Color rasterColor = new Windows.UI.Color();

        private int vectorOrderNr;
        private int vectorOrderCount;
        private int vectorOrderZValue;

        private int rasterOrderNr;
        private int rasterOrderCount;
        private int rasterOrderZValue;

        public void AddTreeViewChild(Prototyp.Elements.VectorData vectorData)
        {
            //Name auf Listeneintrag übergeben
            newVectorChild.VectorListViewItemText.Text = vectorData.Name;
            newVectorChild.VectorListViewItemText.Uid = vectorData.ID.ToString();

            //Vektordaten an- und ausschalten auf Karte
            newVectorChild.VectorListViewItemCheckBox.Unchecked += new RoutedEventHandler(DisableVector);
            newVectorChild.VectorListViewItemCheckBox.Checked += new RoutedEventHandler(EnableVector);

            // Context Menu bauen
            System.Windows.Controls.ContextMenu vectorContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem ZoomToVector = new System.Windows.Controls.MenuItem();
            ZoomToVector.Header = "Zoom to vector data";
            ZoomToVector.Click += new RoutedEventHandler(ZoomToVector_Click);
            vectorContextMenu.Items.Add(ZoomToVector);
            System.Windows.Controls.MenuItem Remove = new System.Windows.Controls.MenuItem();
            Remove.Header = "Remove";
            Remove.Click += new RoutedEventHandler(RemoveVector);
            vectorContextMenu.Items.Add(Remove);
            System.Windows.Controls.MenuItem Properties = new System.Windows.Controls.MenuItem();
            Properties.Header = "Properties";
            vectorContextMenu.Items.Add(Properties);
            newVectorChild.ContextMenu = vectorContextMenu;

            //Vektorfarbe bestimmen
            vectorColor = Windows.UI.Color.FromArgb(255, (byte) rnd.Next(256), (byte) rnd.Next(256), (byte) rnd.Next(256));
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (System.Windows.Media.Brush) converter.ConvertFromString(vectorColor.ToString());
            newVectorChild.VectorListViewItemColorPicker.Background = brush;
            newVectorChild.VectorListViewItemColorPicker.Click += new RoutedEventHandler(PickColor);

            //Drag-Event starten, um Vektordaten in NodeEditor zu ziehen
            newVectorChild.VectorListViewItemText.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => StartDragEvent(sender, e, newVectorChild));

            //new Child wird dem Table of Contents hinzugefügt
            Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Add(newVectorChild);
            ((System.Collections.Specialized.INotifyCollectionChanged)Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items).CollectionChanged += TableOfContentsVector_CollectionChanged;
        }

        public void AddTreeViewChild(Prototyp.Elements.RasterData rasterData)
        {
            //Name auf Listeneintrag übergeben
            newRasterChild.RasterListViewItemText.Text = rasterData.Name;
            newRasterChild.RasterListViewItemText.Uid = rasterData.ID.ToString();

            //Rasterdaten an- und ausschalten auf Karte
            newRasterChild.RasterListViewItemCheckBox.Unchecked += new RoutedEventHandler(DisableRaster);
            newRasterChild.RasterListViewItemCheckBox.Checked += new RoutedEventHandler(EnableRaster);

            // Context Menu bauen
            System.Windows.Controls.ContextMenu rasterContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem ZoomToRaster = new System.Windows.Controls.MenuItem();
            ZoomToRaster.Header = "Zoom to raster data";
            ZoomToRaster.Click += new RoutedEventHandler(ZoomToRaster_Click);
            rasterContextMenu.Items.Add(ZoomToRaster);
            System.Windows.Controls.MenuItem Remove = new System.Windows.Controls.MenuItem();
            Remove.Header = "Remove";
            Remove.Click += new RoutedEventHandler(RemoveRaster);
            rasterContextMenu.Items.Add(Remove);
            System.Windows.Controls.MenuItem Properties = new System.Windows.Controls.MenuItem();
            Properties.Header = "Properties";
            rasterContextMenu.Items.Add(Properties);
            newRasterChild.ContextMenu = rasterContextMenu;

            //Rasterfarbe bestimmen
            rasterColor = Windows.UI.Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (System.Windows.Media.Brush)converter.ConvertFromString(rasterColor.ToString());
            newRasterChild.RasterListViewItemColorPicker.Background = brush;
            newRasterChild.RasterListViewItemColorPicker.Click += new RoutedEventHandler(PickColor);

            //Drag-Event starten, um Rasterdaten in NodeEditor zu ziehen
            newRasterChild.RasterListViewItemText.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => StartDragEvent(sender, e, newRasterChild));

            //new Child wird dem Table of Contents hinzugefügt
            Prototyp.MainWindow.AppWindow.TableOfContentsRaster.Items.Add(newRasterChild);
            ((System.Collections.Specialized.INotifyCollectionChanged)Prototyp.MainWindow.AppWindow.TableOfContentsRaster.Items).CollectionChanged += TableOfContentsRaster_CollectionChanged;
        }

        public void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, Prototyp.Custom_Controls.VectorListViewItem newVectorChild)
        {
            DataObject dataObj = new DataObject();
            dataObj.SetData("Vectorname", newVectorChild.VectorListViewItemText.Text);
            dataObj.SetData("ID", newVectorChild.VectorListViewItemText.Uid);
            dataObj.SetData("Type", "Vector");

            DragDrop.DoDragDrop(newVectorChild, dataObj, DragDropEffects.Move);
        }

        public void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, Prototyp.Custom_Controls.RasterListViewItem newRasterChild)
        {
            DataObject dataObj = new DataObject();
            dataObj.SetData("Rastername", newRasterChild.RasterListViewItemText.Text);
            dataObj.SetData("ID", newRasterChild.RasterListViewItemText.Uid);
            dataObj.SetData("Type", "Raster");

            DragDrop.DoDragDrop(newRasterChild, dataObj, DragDropEffects.Move);
        }

        public void TableOfContentsVector_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                vectorOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Count;
                vectorOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.IndexOf(newVectorChild);
                vectorOrderZValue = vectorOrderCount - vectorOrderNr + 1;
            }
        }

        public void TableOfContentsRaster_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                rasterOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsRaster.Items.Count;
                rasterOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsRaster.Items.IndexOf(newRasterChild);
                rasterOrderZValue = rasterOrderCount - rasterOrderNr + 1;
            }
        }

        public void PickColor(Object sender, RoutedEventArgs e)
        {

        }

        public void RemoveVector(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Remove(newVectorChild);
        }

        public void RemoveRaster(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.TableOfContentsRaster.Items.Remove(newRasterChild);
        }

        public void DisableVector(Object sender, RoutedEventArgs e)
        {

        }

        public void DisableRaster(Object sender, RoutedEventArgs e)
        {

        }

        public void EnableVector(Object sender, RoutedEventArgs e)
        {

        }

        public void EnableRaster(Object sender, RoutedEventArgs e)
        {

        }

        public void ZoomToVector_Click(Object sender, RoutedEventArgs e)
        {

        }

        public void ZoomToRaster_Click(Object sender, RoutedEventArgs e)
        {

        }
    }
}