using DynamicData;
using System.Windows;
using NodeNetwork.ViewModels;

namespace Prototyp
{
    /// <summary>
    /// Interaction logic for ModuleDesigner.xaml
    /// </summary>
    public partial class ModuleDesigner : Window
    {
        private NetworkViewModel previewNetwork = new NetworkViewModel();

        public class ListViewEntry
        {
            public string SlotType { get; set; }
            public string RowName;

            public bool Invalid = true;

            public System.Collections.Generic.List<string> InputTypes = new System.Collections.Generic.List<string>();

            public string ControlType;

            public float SliderStart;
            public float SliderEnd;
            public float SliderDefault;
            public float SliderTickFrequency;
            public string SliderUnit;

            public System.Collections.Generic.List<string> DropDownEntries = new System.Collections.Generic.List<string>();

            public System.Collections.Generic.List<string> OutputTypes = new System.Collections.Generic.List<string>();
        }

        public System.Collections.Generic.List<ListViewEntry> ListViewEntries = new System.Collections.Generic.List<ListViewEntry>();

        public bool OkayClicked;

        public ModuleDesigner()
        {
            InitializeComponent();

            networkPreView.ViewModel = previewNetwork;

            MyListBox.ItemsSource = ListViewEntries;
        }

        // Private methods -------------------------------------------------------------------------

        private void MakePreview()
        {
            previewNetwork.Nodes.Clear();

            Prototyp.Elements.VorteXML vorteXML = MakeXML();
            if (vorteXML == null) return;

            Prototyp.Modules.Node_Module nodeModule = new Prototyp.Modules.Node_Module(vorteXML);
            previewNetwork.ZoomFactor = 2.0 / 3.0;

            System.Windows.Point TempPoint;
            TempPoint.X = 119.0 / 3.0;
            TempPoint.Y = 275.0 / 6.0;
            previewNetwork.DragOffset = TempPoint;

            previewNetwork.Nodes.Add(nodeModule);
        }

        private void CheckValidity()
        {
            int si = MyListBox.SelectedIndex;

            foreach (ListViewEntry l in ListViewEntries)
            {
                l.SlotType = l.SlotType.Replace(" (invalid)", "");
                if (l.Invalid) l.SlotType = l.SlotType + " (invalid)";
            }
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MyListBox.SelectedIndex = si;
        }

        // Event handlers -------------------------------------------------------------------------

        private void CmdAddInput_Click(object sender, RoutedEventArgs e)
        {
            ListViewEntries.Add(new ListViewEntry() { SlotType = "Input" });
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
            MyListBox.SelectedIndex = ListViewEntries.Count - 1;
            CheckValidity();
        }

        private void CmdAddControl_Click(object sender, RoutedEventArgs e)
        {
            ControlTypeSelector controlTypeSelector = new ControlTypeSelector();
            controlTypeSelector.Owner = this;
            controlTypeSelector.ShowDialog();

            if ((bool)controlTypeSelector.RdoSlider.IsChecked)
            {
                ListViewEntries.Add(new ListViewEntry() { SlotType = "Control (slider)" });
                ListViewEntries[ListViewEntries.Count - 1].ControlType = "Slider";
            }
            else if ((bool)controlTypeSelector.RdoDropdown.IsChecked)
            {
                ListViewEntries.Add(new ListViewEntry() { SlotType = "Control (dropdown)" });
                ListViewEntries[ListViewEntries.Count - 1].ControlType = "Dropdown";
            }

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
            MyListBox.SelectedIndex = ListViewEntries.Count - 1;
            CheckValidity();
        }

        private void CmdAddOutput_Click(object sender, RoutedEventArgs e)
        {
            ListViewEntries.Add(new ListViewEntry() { SlotType = "Output" });
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
            MyListBox.SelectedIndex = ListViewEntries.Count - 1;
            CheckValidity();
        }

        private void CmdEditProperties_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex == -1) return;

            ModuleProperties moduleProperties = new ModuleProperties();
            moduleProperties.Owner = this;

            if (ListViewEntries[MyListBox.SelectedIndex].RowName != null) moduleProperties.TxtRowName.Text = ListViewEntries[MyListBox.SelectedIndex].RowName;

            if (ListViewEntries[MyListBox.SelectedIndex].SlotType.Contains("Input"))
            {
                moduleProperties.TxtSliderStart.IsEnabled = false;
                moduleProperties.TxtSliderEnd.IsEnabled = false;
                moduleProperties.TxtSliderDefault.IsEnabled = false;
                moduleProperties.TxtSliderTick.IsEnabled = false;
                moduleProperties.TxtSliderUnit.IsEnabled = false;
                moduleProperties.TxtDropdownEntries.IsEnabled = false;
                moduleProperties.RdoOutVectorPoint.IsEnabled = false;
                moduleProperties.RdoOutVectorLine.IsEnabled = false;
                moduleProperties.RdoOutVectorPolygon.IsEnabled = false;
                moduleProperties.RdoOutVectorMultiPolygon.IsEnabled = false;
                moduleProperties.RdoOutRaster.IsEnabled = false;
                moduleProperties.RdoOutTable.IsEnabled = false;

                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorPoint") moduleProperties.RdoInVectorPoint.IsChecked = true; else moduleProperties.RdoInVectorPoint.IsChecked = false; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorLine") moduleProperties.RdoInVectorLine.IsChecked = true; else moduleProperties.RdoInVectorLine.IsChecked = false; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorPolygon") moduleProperties.RdoInVectorPolygon.IsChecked = true; else moduleProperties.RdoInVectorPolygon.IsChecked = false; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorMultiPolygon") moduleProperties.RdoInVectorMultiPolygon.IsChecked = true; else moduleProperties.RdoInVectorMultiPolygon.IsChecked = false; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "Raster") moduleProperties.RdoInRaster.IsChecked = true; else moduleProperties.RdoInRaster.IsChecked = false; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "Table") moduleProperties.RdoInTable.IsChecked = true; else moduleProperties.RdoInTable.IsChecked = false; }

                moduleProperties.ShowDialog();
                if (!moduleProperties.OkayClicked) return;

                ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                ListViewEntries[MyListBox.SelectedIndex].InputTypes.Clear();
                if ((bool)moduleProperties.RdoInVectorPoint.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorPoint");
                if ((bool)moduleProperties.RdoInVectorLine.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorLine");
                if ((bool)moduleProperties.RdoInVectorPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorPolygon");
                if ((bool)moduleProperties.RdoInVectorMultiPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorMultiPolygon");
                if ((bool)moduleProperties.RdoInRaster.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("Raster");
                if ((bool)moduleProperties.RdoInTable.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("Table");
            }
            else if (ListViewEntries[MyListBox.SelectedIndex].SlotType.Contains("Control"))
            {
                if (ListViewEntries[MyListBox.SelectedIndex].ControlType == "Slider")
                {
                    moduleProperties.RdoInVectorPoint.IsEnabled = false;
                    moduleProperties.RdoInVectorLine.IsEnabled = false;
                    moduleProperties.RdoInVectorPolygon.IsEnabled = false;
                    moduleProperties.RdoInVectorMultiPolygon.IsEnabled = false;
                    moduleProperties.RdoInRaster.IsEnabled = false;
                    moduleProperties.RdoInTable.IsEnabled = false;
                    moduleProperties.TxtDropdownEntries.IsEnabled = false;
                    moduleProperties.RdoOutVectorPoint.IsEnabled = false;
                    moduleProperties.RdoOutVectorLine.IsEnabled = false;
                    moduleProperties.RdoOutVectorPolygon.IsEnabled = false;
                    moduleProperties.RdoOutVectorMultiPolygon.IsEnabled = false;
                    moduleProperties.RdoOutRaster.IsEnabled = false;
                    moduleProperties.RdoOutTable.IsEnabled = false;

                    if (ListViewEntries[MyListBox.SelectedIndex].SliderUnit != null)
                    {
                        moduleProperties.TxtSliderStart.Text = ListViewEntries[MyListBox.SelectedIndex].SliderStart.ToString();
                        moduleProperties.TxtSliderEnd.Text = ListViewEntries[MyListBox.SelectedIndex].SliderEnd.ToString();
                        moduleProperties.TxtSliderDefault.Text = ListViewEntries[MyListBox.SelectedIndex].SliderDefault.ToString();
                        moduleProperties.TxtSliderTick.Text = ListViewEntries[MyListBox.SelectedIndex].SliderTickFrequency.ToString();
                        moduleProperties.TxtSliderUnit.Text = ListViewEntries[MyListBox.SelectedIndex].SliderUnit;
                    }

                    moduleProperties.ShowDialog();
                    if (!moduleProperties.OkayClicked) return;

                    ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                    ListViewEntries[MyListBox.SelectedIndex].SliderStart = float.Parse(moduleProperties.TxtSliderStart.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderEnd = float.Parse(moduleProperties.TxtSliderEnd.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderDefault = float.Parse(moduleProperties.TxtSliderDefault.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderTickFrequency = float.Parse(moduleProperties.TxtSliderTick.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderUnit = moduleProperties.TxtSliderUnit.Text;
                }
                else if (ListViewEntries[MyListBox.SelectedIndex].ControlType == "Dropdown")
                {
                    moduleProperties.RdoInVectorPoint.IsEnabled = false;
                    moduleProperties.RdoInVectorLine.IsEnabled = false;
                    moduleProperties.RdoInVectorPolygon.IsEnabled = false;
                    moduleProperties.RdoInVectorMultiPolygon.IsEnabled = false;
                    moduleProperties.RdoInRaster.IsEnabled = false;
                    moduleProperties.RdoInTable.IsEnabled = false;
                    moduleProperties.TxtSliderStart.IsEnabled = false;
                    moduleProperties.TxtSliderEnd.IsEnabled = false;
                    moduleProperties.TxtSliderDefault.IsEnabled = false;
                    moduleProperties.TxtSliderTick.IsEnabled = false;
                    moduleProperties.TxtSliderUnit.IsEnabled = false;
                    moduleProperties.RdoOutVectorPoint.IsEnabled = false;
                    moduleProperties.RdoOutVectorLine.IsEnabled = false;
                    moduleProperties.RdoOutVectorPolygon.IsEnabled = false;
                    moduleProperties.RdoOutVectorMultiPolygon.IsEnabled = false;
                    moduleProperties.RdoOutRaster.IsEnabled = false;
                    moduleProperties.RdoOutTable.IsEnabled = false;

                    moduleProperties.TxtDropdownEntries.Text = string.Join(System.Environment.NewLine, ListViewEntries[MyListBox.SelectedIndex].DropDownEntries);

                    moduleProperties.ShowDialog();
                    if (!moduleProperties.OkayClicked) return;

                    ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                    string[] DropDownList = moduleProperties.TxtDropdownEntries.Text.Split(System.Environment.NewLine);
                    ListViewEntries[MyListBox.SelectedIndex].DropDownEntries.Clear();
                    foreach (string LstStr in DropDownList) ListViewEntries[MyListBox.SelectedIndex].DropDownEntries.Add(LstStr);
                }
            }
            else if (ListViewEntries[MyListBox.SelectedIndex].SlotType.Contains("Output"))
            {
                moduleProperties.RdoInVectorPoint.IsEnabled = false;
                moduleProperties.RdoInVectorLine.IsEnabled = false;
                moduleProperties.RdoInVectorPolygon.IsEnabled = false;
                moduleProperties.RdoInVectorMultiPolygon.IsEnabled = false;
                moduleProperties.RdoInRaster.IsEnabled = false;
                moduleProperties.RdoInTable.IsEnabled = false;
                moduleProperties.TxtSliderStart.IsEnabled = false;
                moduleProperties.TxtSliderEnd.IsEnabled = false;
                moduleProperties.TxtSliderDefault.IsEnabled = false;
                moduleProperties.TxtSliderTick.IsEnabled = false;
                moduleProperties.TxtSliderUnit.IsEnabled = false;
                moduleProperties.TxtDropdownEntries.IsEnabled = false;

                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorPoint") moduleProperties.RdoOutVectorPoint.IsChecked = true; else moduleProperties.RdoOutVectorPoint.IsChecked = false; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorLine") moduleProperties.RdoOutVectorLine.IsChecked = true; else moduleProperties.RdoOutVectorLine.IsChecked = false; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorPolygon") moduleProperties.RdoOutVectorPolygon.IsChecked = true; else moduleProperties.RdoOutVectorPolygon.IsChecked = false; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorMultiPolygon") moduleProperties.RdoOutVectorMultiPolygon.IsChecked = true; else moduleProperties.RdoOutVectorMultiPolygon.IsChecked = false; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "Raster") moduleProperties.RdoOutRaster.IsChecked = true; else moduleProperties.RdoOutRaster.IsChecked = false; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "Table") moduleProperties.RdoOutTable.IsChecked = true; else moduleProperties.RdoOutTable.IsChecked = false; }

                moduleProperties.ShowDialog();
                if (!moduleProperties.OkayClicked) return;

                ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Clear();
                if ((bool)moduleProperties.RdoOutVectorPoint.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorPoint");
                if ((bool)moduleProperties.RdoOutVectorLine.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorLine");
                if ((bool)moduleProperties.RdoOutVectorPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorPolygon");
                if ((bool)moduleProperties.RdoOutVectorMultiPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorMultiPolygon");
                if ((bool)moduleProperties.RdoOutRaster.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("Raster");
                if ((bool)moduleProperties.RdoOutTable.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("Table");
            }

            MakePreview();
            CheckValidity();
        }

        private void CmdDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex == -1) return;

            ListViewEntries.RemoveAt(MyListBox.SelectedIndex);
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MakePreview();
            CheckValidity();
        }

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex <= 0) return;

            int SwapPos = MyListBox.SelectedIndex;

            ListViewEntry Temp = ListViewEntries[SwapPos - 1];
            ListViewEntries[SwapPos - 1] = ListViewEntries[SwapPos];
            ListViewEntries[SwapPos] = Temp;

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MyListBox.SelectedIndex = SwapPos - 1;

            MakePreview();
            CheckValidity();
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex >= ListViewEntries.Count - 1) return;

            int SwapPos = MyListBox.SelectedIndex;

            ListViewEntry Temp = ListViewEntries[SwapPos + 1];
            ListViewEntries[SwapPos + 1] = ListViewEntries[SwapPos];
            ListViewEntries[SwapPos] = Temp;

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MyListBox.SelectedIndex = SwapPos + 1;

            MakePreview();
            CheckValidity();
        }

        private void CmdClear_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewEntries.Count == 0) return;
            if (MessageBox.Show("Are you sure?", this.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            ListViewEntries.Clear();

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MakePreview();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayClicked = true;
            this.Close();
        }
        private void TxtName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MakePreview();
            CheckValidity();
        }

        // Public methods -------------------------------------------------------------------------

        public Prototyp.Elements.VorteXML MakeXML()
        {
            Prototyp.Elements.VorteXML vorteXML = new Prototyp.Elements.VorteXML();

            vorteXML.EditorVersion = "0.1";
            vorteXML.NodeStyle = "default";
            if (ChkGUI != null)
            {
                if (ChkGUI.IsChecked == true)
                {
                    vorteXML.ShowGUI = true;
                }
                else
                {
                    vorteXML.ShowGUI = false;
                }
            }
            else
            {
                vorteXML.ShowGUI = false;
            }
            vorteXML.NodeTitle = TxtName.Text;

            vorteXML.ToolRows = new Prototyp.Elements.VorteXML.ToolRow[ListViewEntries.Count];
            for (int i = 0; i < ListViewEntries.Count; i++)
            {
                vorteXML.ToolRows[i].Index = i + 1;
                vorteXML.ToolRows[i].Name = ListViewEntries[i].RowName;

                if (ListViewEntries[i].SlotType.Contains("Input"))
                {
                    vorteXML.ToolRows[i].rowType = Prototyp.Elements.VorteXML.RowType.Input;
                    vorteXML.ToolRows[i].inputRow.inputTypes = new Prototyp.Elements.VorteXML.ConnectorType[ListViewEntries[i].InputTypes.Count];

                    ListViewEntries[i].Invalid = true;
                    if (ListViewEntries[i].InputTypes.Count == 0) return (null); // Module description corrupted: ANY input type must be allowed.
                    ListViewEntries[i].Invalid = false;
                    int j = 0;
                    foreach (string InpTy in ListViewEntries[i].InputTypes)
                    {
                        if (InpTy == "VectorPoint") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorPoint;
                        if (InpTy == "VectorLine") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorLine;
                        if (InpTy == "VectorPolygon") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorPolygon;
                        if (InpTy == "VectorMultiPolygon") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorMultiPolygon;
                        if (InpTy == "Raster") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.Raster;
                        if (InpTy == "Table") vorteXML.ToolRows[i].inputRow.inputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.Table;
                        j++;
                    }
                }
                else if (ListViewEntries[i].SlotType.Contains("Control"))
                {
                    vorteXML.ToolRows[i].rowType = Prototyp.Elements.VorteXML.RowType.Control;

                    if (ListViewEntries[i].ControlType == "Slider")
                    {
                        vorteXML.ToolRows[i].controlRow.controlType = Prototyp.Elements.VorteXML.ControlType.Slider;

                        vorteXML.ToolRows[i].controlRow.slider.Style = "default";

                        vorteXML.ToolRows[i].controlRow.slider.Start = ListViewEntries[i].SliderStart;
                        vorteXML.ToolRows[i].controlRow.slider.End = ListViewEntries[i].SliderEnd;
                        vorteXML.ToolRows[i].controlRow.slider.Default = ListViewEntries[i].SliderDefault;
                        vorteXML.ToolRows[i].controlRow.slider.TickFrequency = ListViewEntries[i].SliderTickFrequency;
                        vorteXML.ToolRows[i].controlRow.slider.Unit = ListViewEntries[i].SliderUnit;

                        ListViewEntries[i].Invalid = false;
                    }
                    else if (ListViewEntries[i].ControlType == "Dropdown")
                    {
                        vorteXML.ToolRows[i].controlRow.controlType = Prototyp.Elements.VorteXML.ControlType.Dropdown;

                        vorteXML.ToolRows[i].controlRow.dropdown.Style = "default";

                        vorteXML.ToolRows[i].controlRow.dropdown.Values = new string[ListViewEntries[i].DropDownEntries.Count];

                        ListViewEntries[i].Invalid = true;
                        if (ListViewEntries[i].DropDownEntries.Count == 0) return (null); // Module description corrupted: ANY dropdown entry must be present.
                        ListViewEntries[i].Invalid = false;
                        int j = 0;
                        foreach (string DD in ListViewEntries[i].DropDownEntries)
                        {
                            vorteXML.ToolRows[i].controlRow.dropdown.Values[j] = DD;
                            j++;
                        }
                    }
                }
                else if (ListViewEntries[i].SlotType.Contains("Output"))
                {
                    vorteXML.ToolRows[i].rowType = Prototyp.Elements.VorteXML.RowType.Output;
                    vorteXML.ToolRows[i].outputRow.outputTypes = new Prototyp.Elements.VorteXML.ConnectorType[ListViewEntries[i].OutputTypes.Count];

                    ListViewEntries[i].Invalid = true;
                    if (ListViewEntries[i].OutputTypes.Count == 0) return (null); // Module description corrupted: ANY output type must be allowed.
                    ListViewEntries[i].Invalid = false;
                    int j = 0;
                    foreach (string OutTy in ListViewEntries[i].OutputTypes)
                    {
                        if (OutTy == "VectorPoint") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorPoint;
                        if (OutTy == "VectorLine") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorLine;
                        if (OutTy == "VectorPolygon") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorPolygon;
                        if (OutTy == "VectorMultiPolygon") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.VectorMultiPolygon;
                        if (OutTy == "Raster") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.Raster;
                        if (OutTy == "Table") vorteXML.ToolRows[i].outputRow.outputTypes[j] = Prototyp.Elements.VorteXML.ConnectorType.Table;
                        j++;
                    }
                }
            }

            return (vorteXML);
        }
    }
}
