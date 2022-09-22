﻿using NodeNetwork.Toolkit.ValueNode;
using Prototyp.Modules.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules.ViewModels
{
    public class DropDownMenuViewModel : ValueEditorViewModel<string>
    {
        public DropDownMenuViewModel(string controlName, string[] items)
        {
            Splat.Locator.CurrentMutable.Register(() => new DropDownMenuView(controlName, items), typeof(IViewFor<DropDownMenuViewModel>));
        }

        #region StringItems
        private string _stringItem;
        public string StringItem
        {
            get => _stringItem;
            set => this.RaiseAndSetIfChanged(ref _stringItem, value);
        }
        #endregion


        public DropDownMenuViewModel()
        {
            this.WhenAnyValue(vm => vm.StringItem)
                .BindTo(this, vm => vm.Value);
        }
    }
}
