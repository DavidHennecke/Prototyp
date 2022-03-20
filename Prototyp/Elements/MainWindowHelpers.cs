using Prototyp.Custom_Controls;
using System;
using System.Windows;

namespace Prototyp.Elements
{
    public class MainWindowHelpers
    {
        //private int layerOrderNr;
        //private int layerOrderCount;
        //private int layerOrderZValue;

        public static void AddTreeViewChild(Prototyp.Elements.VectorData vectorData)
        {
            VectorListViewItem newChild = new VectorListViewItem();
            Windows.UI.Color vectorColor = new Windows.UI.Color();
            System.Random rnd = new System.Random();

            //Name auf Listeneintrag übergeben
            newChild.VectorListViewItemText.Text = vectorData.Name;

            //Vektordaten an- und ausschalten auf Karte
            newChild.VectorListViewItemCheckBox.Unchecked += new RoutedEventHandler(DisableVector);
            newChild.VectorListViewItemCheckBox.Checked += new RoutedEventHandler(EnableVector);

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
            newChild.ContextMenu = vectorContextMenu;

            //Vektorfarbe bestimmen
            vectorColor = Windows.UI.Color.FromArgb(255, (byte) rnd.Next(256), (byte) rnd.Next(256), (byte) rnd.Next(256));
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (System.Windows.Media.Brush) converter.ConvertFromString(vectorColor.ToString());
            newChild.VectorListViewItemColorPicker.Background = brush;
            newChild.VectorListViewItemColorPicker.Click += new RoutedEventHandler(PickColor);

            //Drag-Event starten, um Vektordaten in NodeEditor zu ziehen
            newChild.VectorListViewItemText.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => StartDragEvent(sender, e, newChild));

            //new Child wird dem Table of Contents hinzugefügt
            Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Add(newChild);
            ((System.Collections.Specialized.INotifyCollectionChanged)Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items).CollectionChanged += TableOfContentsVector_CollectionChanged;
        }

        public static void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, VectorListViewItem newChild)
        {
            DataObject dataObj = new DataObject();
            dataObj.SetData("Filename", newChild.VectorListViewItemText.Text);
            dataObj.SetData("Vectorname", newChild.VectorListViewItemText.Text);

            DragDrop.DoDragDrop(newChild, dataObj, DragDropEffects.Move);
        }

        public static void TableOfContentsVector_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                //TODO
                //layerOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Count;
                //layerOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.IndexOf(newChild);
                //layerOrderZValue = layerOrderCount + 1 - layerOrderNr;
            }
        }
        public static void PickColor(Object sender, RoutedEventArgs e)
        {

        }

        public static void RemoveVector(Object sender, RoutedEventArgs e)
        {
            //TODO
            //Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Remove(newChild);
        }

        public static void DisableVector(Object sender, RoutedEventArgs e)
        {

        }

        public static void EnableVector(Object sender, RoutedEventArgs e)
        {

        }

        public static void ZoomToVector_Click(Object sender, RoutedEventArgs e)
        {

        }
    }
}