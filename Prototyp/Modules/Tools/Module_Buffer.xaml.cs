using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prototyp.Modules.Tools
{
    /// <summary>
    /// Interaction logic for Module_Buffer.xaml
    /// </summary>
    public partial class Module_Buffer : UserControl
    {
        public Module_Buffer()
        {
            InitializeComponent();
        }

        public static void DeleteModul(object sender, RoutedEventArgs e, Module_Buffer newBufferModule)
        {
            Prototyp.MainWindow.AppWindow.NodeEditor.Children.Remove(newBufferModule);
        }
    }
}
