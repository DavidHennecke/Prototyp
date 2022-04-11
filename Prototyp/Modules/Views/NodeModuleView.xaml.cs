﻿using System;
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
            if (this.ViewModel.Status == 0)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            }
            else if (this.ViewModel.Status == 1)
            {
                this.NodeView.Background = System.Windows.Media.Brushes.Green;
            }
            else if (this.ViewModel.Status == 2)
            {
                this.NodeView.Background = System.Windows.Media.Brushes.Red;
            }
            else if (this.ViewModel.Status == 3)
            {
                this.NodeView.Background = System.Windows.Media.Brushes.Green;
            }
            else if (this.ViewModel.Status == 4)
            {
                this.NodeView.Background = System.Windows.Media.Brushes.Green;
            }
            else if (this.ViewModel.Status == 5)
            {
                this.NodeView.Background = System.Windows.Media.Brushes.Green;
            }
        }
    }
}
