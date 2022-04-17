using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    /// <summary>
    /// Interaction logic for NodeModuleView.xaml
    /// </summary>
    public partial class NodeModuleView : IViewFor<Node_Module>
    {

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(Node_Module), typeof(NodeModuleView), new PropertyMetadata(null));

        public Node_Module ViewModel
        {
            get => (Node_Module)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (Node_Module)value;
        }
        

        #endregion
        public NodeModuleView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.NodeView.ViewModel).DisposeWith(d);
                this.ViewModel.ProcessStatusChanged += ViewModel_ProcessStatusChanged;
            });  
        }

        private void ViewModel_ProcessStatusChanged(object sender, EventArgs e)
        {
            // Dieser Fall hier ist gar nicht definiert.
            //if (this.ViewModel.Status == 0)
            //{
            //    this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            //}
            if (this.ViewModel.Status == NodeProgress.Finished)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#3b794e");
            }
            else if (this.ViewModel.Status == NodeProgress.Interrupted)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#793b3b");
            }
            else if (this.ViewModel.Status == NodeProgress.Waiting)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#e6f0ef");
            }
            else if (this.ViewModel.Status == NodeProgress.Ready)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#e5a31f");
            }
            else if (this.ViewModel.Status == NodeProgress.Processing)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#345282");
            }
        }
    }
}
