using System;
using System.Windows;

namespace Prototyp
{
    public class MainWindowHelpers
    {
        private Prototyp.Custom_Controls.VectorListViewItem newChild = new Prototyp.Custom_Controls.VectorListViewItem();

        private Windows.UI.Color vectorColor = new Windows.UI.Color();
        private System.Random rnd = new System.Random();

        private int vectorOrderNr;
        private int vectorOrderCount;
        private int vectorOrderZValue;

        public void AddTreeViewChild(Prototyp.Elements.VectorData vectorData)
        {            
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

        public void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, Prototyp.Custom_Controls.VectorListViewItem newChild)
        {
            DataObject dataObj = new DataObject();
            dataObj.SetData("Vectorname", newChild.VectorListViewItemText.Text); //TODO: Eindeutige ID vergeben.

            DragDrop.DoDragDrop(newChild, dataObj, DragDropEffects.Move);
        }

        public void TableOfContentsVector_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                vectorOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Count;
                vectorOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.IndexOf(newChild);
                vectorOrderZValue = vectorOrderCount - vectorOrderNr + 1;
            }
        }
        public void PickColor(Object sender, RoutedEventArgs e)
        {

        }

        public void RemoveVector(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.TableOfContentsVector.Items.Remove(newChild);
        }

        public void DisableVector(Object sender, RoutedEventArgs e)
        {

        }

        public void EnableVector(Object sender, RoutedEventArgs e)
        {

        }

        public void ZoomToVector_Click(Object sender, RoutedEventArgs e)
        {

        }
    }
}