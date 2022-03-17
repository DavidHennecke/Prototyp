using OSGeo.GDAL;
using OSGeo.OGR;
using Prototyp.Custom_Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using Windows.UI;


namespace Prototyp.Elements
{
    public class Shapefile
    {
        public string sFilename;
        private Layer Layer;
        private VectorListViewItem newChild = new VectorListViewItem();
        private Color layerColor = new Color();
        private Random rnd = new Random();
        private int layerOrderNr;
        private int layerOrderCount;
        private int layerOrderZValue;



        public void InitLayer(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();// Register all drivers
            DataSource ds = Ogr.Open(sFilename, 1); // 0 means read-only, 1 means modifiable
            if (ds == null)
            {
                MessageBox.Show("Failed to open file [{0}]!", sFilename);
            }

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                MessageBox.Show("Get the {0}th layer failed! n", "0");
            }
            AddTreeViewChild();
        }

        private void AddTreeViewChild()
        {
            //Name auf Listeneintrag übergeben
            string layerName = Layer.GetName();
            newChild.VectorListViewItemText.Text = layerName;

            //Layer an und aus schalten auf Karte
            newChild.VectorListViewItemCheckBox.Unchecked += new RoutedEventHandler(DisableLayer);
            newChild.VectorListViewItemCheckBox.Checked += new RoutedEventHandler(EnableLayer);

            // Context Menu bauen
            ContextMenu vectorContextMenu = new ContextMenu();
            MenuItem ZoomToLayer = new MenuItem();
            ZoomToLayer.Header = "Zoom to Layer";
            ZoomToLayer.Click += new RoutedEventHandler(ZoomToLayer_Click);
            vectorContextMenu.Items.Add(ZoomToLayer);
            MenuItem Remove = new MenuItem();
            Remove.Header = "Remove";
            Remove.Click += new RoutedEventHandler(RemoveLayer);
            vectorContextMenu.Items.Add(Remove);
            MenuItem Properties = new MenuItem();
            Properties.Header = "Properties";
            vectorContextMenu.Items.Add(Properties);
            newChild.ContextMenu = vectorContextMenu;
            
            //Layerfarbe bestimmen
            layerColor = Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (System.Windows.Media.Brush)converter.ConvertFromString(layerColor.ToString());
            newChild.VectorListViewItemColorPicker.Background = brush;
            newChild.VectorListViewItemColorPicker.Click += new RoutedEventHandler(PickColor);

            //Drag Event Starten um Layer in NodeEditor zu ziehen
            newChild.VectorListViewItemText.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => StartDragEvent(sender, e, newChild));

            //new Child wird dem Table of Contents hinzugefügt
            Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Add(newChild);
            ((System.Collections.Specialized.INotifyCollectionChanged)Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items).CollectionChanged += TableOfContentsLayer_CollectionChanged;
        }

        private void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, VectorListViewItem newChild)
        {
            string fileName = (string)sFilename;
            string LayerName = (string)Layer.GetName();

            DataObject dataObj = new DataObject();
            dataObj.SetData("Filename", fileName);
            dataObj.SetData("Layername", LayerName);

            DragDrop.DoDragDrop(newChild, dataObj, DragDropEffects.Move);

        }

        private void TableOfContentsLayer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                layerOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Count;
                layerOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.IndexOf(newChild);
                layerOrderZValue = layerOrderCount + 1 - layerOrderNr;
            }
        }
        private void PickColor(Object sender, RoutedEventArgs e)
        {

        }

        private void RemoveLayer(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Remove(newChild);
        }

        private void DisableLayer(Object sender, RoutedEventArgs e)
        {
            
        }

        private void EnableLayer(Object sender, RoutedEventArgs e)
        {
            
        }

        public void ZoomToLayer_Click(Object sender, RoutedEventArgs e)
        {
            
        }

    }
}
