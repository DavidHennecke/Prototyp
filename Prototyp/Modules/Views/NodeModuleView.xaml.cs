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
            });
        }
    }
}