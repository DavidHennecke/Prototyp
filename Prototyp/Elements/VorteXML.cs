namespace Prototyp.Elements
{
    public class VorteXML
    {
        /***********************************************************************************
        
        Class VorteXML
        Contains properties and methods for Vortex module XML config file handling.
                
        (c) 2022 by Carsten Croonenbroeck, Markus Berger, and David Hennecke. Contact us at
        carsten.croonenbroeck@uni-rostock.de.

            Add license information here.

        Dependencies (NuGet packages):
        - None.

        *///////////////////////////////////////////////////////////////////////////////////

        private string IntNodeStyle;
        private string IntEditorVersion;
        private string IntNodeTitle;

        public ToolRow[] ToolRows;

        public enum RowType
        {
            Input,
            Output,
            Control

            //...?
        }

        public enum ControlType
        {
            Slider,
            Checkbox,
            Dropdown,
            Textbox

            //...
        }

        public enum ConnectorType
        {
            // Vector types
            VectorPoint,
            VectorLine,
            VectorPolygon,

            // Raster types
            Raster

            // Add primitive types such as int, float, string, ...? E.g. for controls.
        }

        public struct Slider
        {
            public string Style;
            public float Start;
            public float End;
            public float Default;
            public float TickFrequency;
            public string Unit;
        }

        public struct Checkbox
        {
            public string Style;
            public int Reference;
        }

        public struct Dropdown
        {
            public string Style;
            public string[] Values;
        }

        public struct Textbox
        {
            public string Style;
            public string Name;
            public string Value;
        }

        public struct AltControls
        {
            public ConnectorType inputType;
            public Textbox[] textboxes;
        }

        public struct InputRow
        {
            public ConnectorType[] inputTypes;
            public AltControls[] altControls;
        }

        public struct OutputRow
        {
            public ConnectorType[] outputTypes;
        }

        public struct ControlRow
        {
            public ControlType controlType;
            public Slider slider;
            public Checkbox checkbox;
            public Dropdown dropdown;
            public Textbox textbox;
        }

        public struct ToolRow
        {
            public int Index;
            public string Name;
            public RowType rowType;
            public InputRow inputRow;
            public OutputRow outputRow;
            public ControlRow controlRow;
        }

        //-----------------------------------------------------------------------------------------------------
        // Getters and setters

        public string NodeStyle
        {
            get { return (IntNodeStyle); }
            set { IntNodeStyle = value; }
        }

        public string EditorVersion
        {
            get { return (IntEditorVersion); }
            set { IntEditorVersion = value; }
        }

        public string NodeTitle
        {
            get { return (IntNodeTitle); }
            set { IntNodeTitle = value; }
        }

        //-----------------------------------------------------------------------------------------------------
        // Constructors

        public VorteXML()
        {
            // Nothing useful to do here...
        }

        public VorteXML(string MyString)
        {
            System.Xml.Linq.XDocument MyXML;

            if (System.IO.File.Exists(MyString)) // Assume that the given string is a file name.
            {
                MyXML = System.Xml.Linq.XDocument.Load(MyString);
            }
            else // Assume that the given string is an actual XML string.
            {
                 MyXML = System.Xml.Linq.XDocument.Parse(MyString);
            }
                        
            ImportXML(MyXML);
        }

        public VorteXML(System.Xml.Linq.XDocument MyXML)
        {
            ImportXML(MyXML);
        }

        //-----------------------------------------------------------------------------------------------------
        // Private methods

        private void ImportXML(System.Xml.Linq.XDocument MyXML)
        {
            int Counter = 0;
            int NumRows = -1;
            System.Xml.Linq.XElement FirstNode = null;
            System.Xml.Linq.XNode CurrentNode = null;

            if (!MyXML.Root.FirstNode.ToString().ToLower().Contains("editorversion"))
            {
                throw new System.Exception("No VorteXML format detected.");
            }

            var RootDescendants = MyXML.Root.Descendants();

            foreach (var nodes in RootDescendants)
            {
                if (nodes.Name == "Node")
                {
                    foreach (var attribs in nodes.Attributes())
                    {
                        if (attribs.Name == "style") IntNodeStyle = attribs.Value;
                        if (attribs.Name == "editorVersion") IntEditorVersion = attribs.Value;
                    }
                }

                if (nodes.Name == "NodeTitle") IntNodeTitle = nodes.Value;

                if (nodes.Name.NamespaceName == "Element")
                {
                    foreach (var attribs in nodes.Attributes())
                    {
                        if (attribs.Name == "rowNr")
                        {
                            NumRows = System.Convert.ToInt32(attribs.Value);

                            if (NumRows == 1)
                            {
                                FirstNode = nodes;
                                break;
                            }
                        }
                    }
                }
            }

            if (NumRows == -1) throw new System.Exception("No rows present.");

            ToolRows = new ToolRow[NumRows];

            foreach (var Children in FirstNode.Nodes())
            {
                // Iterate through the first row's contents.

                HandleContent(Children, Counter);
            }
            CurrentNode = FirstNode.NextNode;
            Counter++;

            while (CurrentNode != null)
            {
                // Iterate through the following rows.
                foreach (var Children in ((System.Xml.Linq.XElement)CurrentNode).Nodes())
                {
                    // Iterate through the following rows' contents.

                    HandleContent(Children, Counter);
                }
                CurrentNode = CurrentNode.NextNode;
                Counter++;
            }
        }

        private void HandleContent(System.Xml.Linq.XNode ThisNode, int Counter)
        {
            System.Xml.Linq.XElement ThisElement = (System.Xml.Linq.XElement)ThisNode;
            System.Xml.Linq.XElement SubElement;
            System.Xml.Linq.XElement SubElement2;
            int SubCounter;
            int SubCounter2;

            if (ThisElement.Parent.Name.NamespaceName == "Element" & ThisElement.Parent.Name.LocalName == "Input")
            {
                ToolRows[Counter].rowType = RowType.Input;
                ToolRows[Counter].Index = Counter + 1;

                if (ThisElement.Name == "InputTypes")
                {
                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubCounter++;
                    }

                    ToolRows[Counter].inputRow.inputTypes = new ConnectorType[SubCounter];
                    foreach (var attribs in ThisElement.Parent.Attributes())
                    {
                        if (attribs.Name == "name")
                        {
                            ToolRows[Counter].Name = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        if (SubElement.Name == "{Vector}Point") ToolRows[Counter].inputRow.inputTypes[SubCounter] = ConnectorType.VectorPoint;
                        if (SubElement.Name == "{Vector}Line") ToolRows[Counter].inputRow.inputTypes[SubCounter] = ConnectorType.VectorLine;
                        if (SubElement.Name == "{Vector}Polygon") ToolRows[Counter].inputRow.inputTypes[SubCounter] = ConnectorType.VectorPolygon;

                        if (SubElement.Name == "{Raster}Raster") ToolRows[Counter].inputRow.inputTypes[SubCounter] = ConnectorType.Raster;

                        SubCounter++;
                    }
                }

                if (ThisElement.Name == "AlternateControl")
                {
                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubCounter++;
                    }

                    ToolRows[Counter].inputRow.altControls = new AltControls[SubCounter];
                    foreach (var attribs in ThisElement.Parent.Attributes())
                    {
                        if (attribs.Name == "name")
                        {
                            ToolRows[Counter].Name = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        if (SubElement.Name == "{Vector}Point") ToolRows[Counter].inputRow.altControls[SubCounter].inputType = ConnectorType.VectorPoint;
                        if (SubElement.Name == "{Vector}Line") ToolRows[Counter].inputRow.altControls[SubCounter].inputType = ConnectorType.VectorLine;
                        if (SubElement.Name == "{Vector}Polygon") ToolRows[Counter].inputRow.altControls[SubCounter].inputType = ConnectorType.VectorPolygon;

                        if (SubElement.Name == "{Raster}Raster") ToolRows[Counter].inputRow.altControls[SubCounter].inputType = ConnectorType.Raster;

                        SubCounter2 = 0;
                        foreach (var nodes2 in SubElement.Nodes())
                        {
                            SubCounter2++;
                        }
                        ToolRows[Counter].inputRow.altControls[SubCounter].textboxes = new Textbox[SubCounter2];

                        SubCounter2 = 0;
                        foreach (var nodes2 in SubElement.Nodes())
                        {
                            SubElement2 = (System.Xml.Linq.XElement)nodes2;

                            ToolRows[Counter].inputRow.altControls[SubCounter].textboxes[SubCounter2].Name = SubElement2.Value;

                            foreach (var attribs in SubElement2.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].inputRow.altControls[SubCounter].textboxes[SubCounter2].Value = attribs.Value;
                                    break;
                                }
                            }

                            SubCounter2++;
                        }

                        SubCounter++;
                    }
                }
            }

            if (ThisElement.Parent.Name.NamespaceName == "Element" & ThisElement.Parent.Name.LocalName == "Output")
            {
                ToolRows[Counter].rowType = RowType.Output;
                ToolRows[Counter].Index = Counter + 1;

                if (ThisElement.Name == "OutputTypes")
                {
                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubCounter++;
                    }

                    ToolRows[Counter].outputRow.outputTypes = new ConnectorType[SubCounter];
                    foreach (var attribs in ThisElement.Parent.Attributes())
                    {
                        if (attribs.Name == "name")
                        {
                            ToolRows[Counter].Name = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        if (SubElement.Name == "{Vector}Point") ToolRows[Counter].outputRow.outputTypes[SubCounter] = ConnectorType.VectorPoint;
                        if (SubElement.Name == "{Vector}Line") ToolRows[Counter].outputRow.outputTypes[SubCounter] = ConnectorType.VectorLine;
                        if (SubElement.Name == "{Vector}Polygon") ToolRows[Counter].outputRow.outputTypes[SubCounter] = ConnectorType.VectorPolygon;

                        if (SubElement.Name == "{Raster}Raster") ToolRows[Counter].outputRow.outputTypes[SubCounter] = ConnectorType.Raster;

                        SubCounter++;
                    }
                }
            }

            if (ThisElement.Parent.Name.NamespaceName == "Element" & ThisElement.Parent.Name.LocalName == "Control")
            {
                ToolRows[Counter].rowType = RowType.Control;
                ToolRows[Counter].Index = Counter + 1;

                foreach (var attribs in ThisElement.Parent.Attributes())
                {
                    if (attribs.Name == "name")
                    {
                        ToolRows[Counter].Name = attribs.Value;
                        break;
                    }
                }

                if (ThisElement.Name == "{Control}Slider") ToolRows[Counter].controlRow.controlType = ControlType.Slider;
                if (ThisElement.Name == "{Control}Checkbox") ToolRows[Counter].controlRow.controlType = ControlType.Checkbox;
                if (ThisElement.Name == "{Control}Dropdown") ToolRows[Counter].controlRow.controlType = ControlType.Dropdown;
                if (ThisElement.Name == "{Control}Textbox") ToolRows[Counter].controlRow.controlType = ControlType.Textbox;
                //...

                if (ThisElement.Name == "{Control}Slider")
                {
                    foreach (var attribs in ThisElement.Attributes())
                    {
                        if (attribs.Name == "style")
                        {
                            ToolRows[Counter].controlRow.slider.Style = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        if (SubElement.Value == "Start")
                        {
                            foreach (var attribs in SubElement.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].controlRow.slider.Start = System.Convert.ToSingle(attribs.Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
                        }

                        if (SubElement.Value == "End")
                        {
                            foreach (var attribs in SubElement.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].controlRow.slider.End = System.Convert.ToSingle(attribs.Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
                        }

                        if (SubElement.Value == "Default")
                        {
                            foreach (var attribs in SubElement.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].controlRow.slider.Default = System.Convert.ToSingle(attribs.Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
                        }

                        if (SubElement.Value == "TickFrequency")
                        {
                            foreach (var attribs in SubElement.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].controlRow.slider.TickFrequency = System.Convert.ToSingle(attribs.Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
                        }

                        if (SubElement.Value == "Unit")
                        {
                            foreach (var attribs in SubElement.Attributes())
                            {
                                if (attribs.Name == "default")
                                {
                                    ToolRows[Counter].controlRow.slider.Unit = attribs.Value;
                                    break;
                                }
                            }
                        }

                        SubCounter++;
                    }
                }

                if (ThisElement.Name == "{Control}Checkbox")
                {
                    foreach (var attribs in ThisElement.Attributes())
                    {
                        if (attribs.Name == "style")
                        {
                            ToolRows[Counter].controlRow.checkbox.Style = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;
                        if (SubElement.Name == "EnableElements")
                        {
                            SubCounter2 = 0;
                            foreach (var nodes2 in SubElement.Nodes())
                            {
                                SubElement2 = (System.Xml.Linq.XElement)nodes2;

                                foreach (var attribs in SubElement2.Attributes())
                                {
                                    if (attribs.Name == "row")
                                    {
                                        ToolRows[Counter].controlRow.checkbox.Reference = System.Convert.ToInt32(attribs.Value);
                                        break;
                                    }
                                }

                                SubCounter2++;
                            }
                        }

                        SubCounter++;
                    }
                }

                if (ThisElement.Name == "{Control}Dropdown")
                {
                    foreach (var attribs in ThisElement.Attributes())
                    {
                        if (attribs.Name == "style")
                        {
                            ToolRows[Counter].controlRow.dropdown.Style = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubCounter++;
                    }
                    ToolRows[Counter].controlRow.dropdown.Values = new string[SubCounter];

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        foreach (var attribs in SubElement.Attributes())
                        {
                            if (attribs.Name == "value")
                            {
                                ToolRows[Counter].controlRow.dropdown.Values[SubCounter] = attribs.Value;
                                break;
                            }
                        }

                        SubCounter++;
                    }
                }

                if (ThisElement.Name == "{Control}Textbox")
                {
                    foreach (var attribs in ThisElement.Attributes())
                    {
                        if (attribs.Name == "style")
                        {
                            ToolRows[Counter].controlRow.textbox.Style = attribs.Value;
                            break;
                        }
                    }

                    SubCounter = 0;
                    foreach (var nodes in ThisElement.Nodes())
                    {
                        SubElement = (System.Xml.Linq.XElement)nodes;

                        foreach (var attribs in SubElement.Attributes())
                        {
                            if (attribs.Name == "value")
                            {
                                ToolRows[Counter].controlRow.textbox.Value = attribs.Value;
                                break;
                            }
                        }

                        foreach (var attribs in SubElement.Attributes())
                        {
                            if (attribs.Name == "name")
                            {
                                ToolRows[Counter].controlRow.textbox.Name = attribs.Value;
                                break;
                            }
                        }

                        SubCounter++;
                    }
                }
            }
        }

        private string ExportInput(int Row)
        {
            string InpString = "";
            string Line;

            Line = "\t\t" + "<Element:Input rowNr='" + ToolRows[Row].Index + "' name='" + ToolRows[Row].Name + "'>" + "\n";
            Line = Line.Replace("'", "\"");
            InpString = InpString + Line;

            if (ToolRows[Row].inputRow.inputTypes.Length > 0)
            {
                Line = "\t\t\t" + "<InputTypes>" + "\n";
                InpString = InpString + Line;

                for (int i = 0; i < ToolRows[Row].inputRow.inputTypes.Length; i++)
                {
                    if (ToolRows[Row].inputRow.inputTypes[i] == ConnectorType.VectorPoint) Line = "\t\t\t\t" + "<Vector:Point />" + "\n";
                    if (ToolRows[Row].inputRow.inputTypes[i] == ConnectorType.VectorLine) Line = "\t\t\t\t" + "<Vector:Line />" + "\n";
                    if (ToolRows[Row].inputRow.inputTypes[i] == ConnectorType.VectorPolygon) Line = "\t\t\t\t" + "<Vector:Polygon />" + "\n";

                    if (ToolRows[Row].inputRow.inputTypes[i] == ConnectorType.Raster) Line = "\t\t\t\t" + "<Raster:Raster />" + "\n";

                    InpString = InpString + Line;
                }

                Line = "\t\t\t" + "</InputTypes>" + "\n";
                InpString = InpString + Line;
            }

            if (ToolRows[Row].inputRow.altControls != null)
            {
                Line = "\t\t\t" + "<AlternateControl>" + "\n";
                InpString = InpString + Line;

                for (int i = 0; i < ToolRows[Row].inputRow.altControls.Length; i++)
                {
                    string AltTypeStr = "";
                    if (ToolRows[Row].inputRow.altControls[i].inputType == ConnectorType.VectorPoint) AltTypeStr = "Vector:Point";
                    if (ToolRows[Row].inputRow.altControls[i].inputType == ConnectorType.VectorLine) AltTypeStr = "Vector:Line";
                    if (ToolRows[Row].inputRow.altControls[i].inputType == ConnectorType.VectorPolygon) AltTypeStr = "Vector:Polygon";

                    if (ToolRows[Row].inputRow.altControls[i].inputType == ConnectorType.Raster) AltTypeStr = "Raster:Raster";

                    Line = "\t\t\t\t" + "<" + AltTypeStr + ">" + "\n";
                    InpString = InpString + Line;

                    for (int j = 0; j < ToolRows[Row].inputRow.altControls[i].textboxes.Length; j++)
                    {
                        Line = "\t\t\t\t\t" + "<Value:Float default='" + ToolRows[Row].inputRow.altControls[i].textboxes[j].Value
                                                                       + "'>"
                                                                       + ToolRows[Row].inputRow.altControls[i].textboxes[j].Name
                                                                       + "</Value:Float>"
                                                                       + "\n";
                        Line = Line.Replace("'", "\"");
                        InpString = InpString + Line;
                    }

                    Line = "\t\t\t\t" + "</" + AltTypeStr + ">" + "\n";
                    InpString = InpString + Line;
                }

                Line = "\t\t\t" + "</AlternateControl>" + "\n";
                InpString = InpString + Line;
            }

            Line = "\t\t" + "</Element:Input>" + "\n";
            InpString = InpString + Line;


            return (InpString);
        }

        private string ExportControl(int Row)
        {
            string ContString = "";
            string Line;

            Line = "\t\t" + "<Element:Control rowNr='" + ToolRows[Row].Index + "' name='" + ToolRows[Row].Name + "'>" + "\n";
            Line = Line.Replace("'", "\"");
            ContString = ContString + Line;

            if (ToolRows[Row].controlRow.slider.Style != null)
            {
                Line = "\t\t\t" + "<Control:Slider style='" + ToolRows[Row].controlRow.slider.Style + "'>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<Value:Float default='" + ToolRows[Row].controlRow.slider.Start.ToString("0.0########", System.Globalization.CultureInfo.InvariantCulture) + "'>Start</Value:Float>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<Value:Float default='" + ToolRows[Row].controlRow.slider.End.ToString("0.0########", System.Globalization.CultureInfo.InvariantCulture) + "'>End</Value:Float>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<Value:Float default='" + ToolRows[Row].controlRow.slider.Default.ToString("0.0########", System.Globalization.CultureInfo.InvariantCulture) + "'>Default</Value:Float>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<Value:Float default='" + ToolRows[Row].controlRow.slider.TickFrequency.ToString("0.0########", System.Globalization.CultureInfo.InvariantCulture) + "'>TickFrequency</Value:Float>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;


                Line = "\t\t\t\t" + "<Value:String default='" + ToolRows[Row].controlRow.slider.Unit + "'>Unit</Value:String>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t" + "</Control:Slider>" + "\n";
                ContString = ContString + Line;
            }

            if (ToolRows[Row].controlRow.checkbox.Style != null)
            {
                Line = "\t\t\t" + "<Control:Checkbox style='" + ToolRows[Row].controlRow.checkbox.Style + "'>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<EnableElements>" + "\n";
                ContString = ContString + Line;

                Line = "\t\t\t\t\t" + "<Reference row='" + ToolRows[Row].controlRow.checkbox.Reference + "' />" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "</EnableElements>" + "\n";
                ContString = ContString + Line;

                Line = "\t\t\t" + "</Control:Checkbox>" + "\n";
                ContString = ContString + Line;
            }

            if (ToolRows[Row].controlRow.dropdown.Style != null)
            {
                Line = "\t\t\t" + "<Control:Dropdown style='" + ToolRows[Row].controlRow.dropdown.Style + "'>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                for (int i = 0; i < ToolRows[Row].controlRow.dropdown.Values.Length; i++)
                {
                    Line = "\t\t\t\t" + "<Value:String value='" + ToolRows[Row].controlRow.dropdown.Values[i] + "' />" + "\n";
                    Line = Line.Replace("'", "\"");
                    ContString = ContString + Line;
                }

                Line = "\t\t\t" + "</Control:Dropdown>" + "\n";
                ContString = ContString + Line;
            }

            if (ToolRows[Row].controlRow.textbox.Style != null)
            {
                Line = "\t\t\t" + "<Control:Textbox style='" + ToolRows[Row].controlRow.textbox.Style + "'>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t\t" + "<Value:String default='" + ToolRows[Row].controlRow.textbox.Value + "'>" + ToolRows[Row].controlRow.textbox.Name + "</Value:String>" + "\n";
                Line = Line.Replace("'", "\"");
                ContString = ContString + Line;

                Line = "\t\t\t" + "</Control:Dropdown>" + "\n";
                ContString = ContString + Line;
            }

            Line = "\t\t" + "</Element:Control>" + "\n";
            ContString = ContString + Line;

            return (ContString);
        }

        private string ExportOutput(int Row)
        {
            string OutString = "";
            string Line;

            Line = "\t\t" + "<Element:Output rowNr='" + ToolRows[Row].Index + "' name='" + ToolRows[Row].Name + "'>" + "\n";
            Line = Line.Replace("'", "\"");
            OutString = OutString + Line;

            Line = "\t\t\t" + "<OutputTypes>" + "\n";
            OutString = OutString + Line;

            for (int i = 0; i < ToolRows[Row].outputRow.outputTypes.Length; i++)
            {
                string OutTypeStr = "";

                if (ToolRows[Row].outputRow.outputTypes[i] == ConnectorType.VectorPoint) OutTypeStr = "Vector:Point";
                if (ToolRows[Row].outputRow.outputTypes[i] == ConnectorType.VectorLine) OutTypeStr = "Vector:Line";
                if (ToolRows[Row].outputRow.outputTypes[i] == ConnectorType.VectorPolygon) OutTypeStr = "Vector:Polygon";

                if (ToolRows[Row].outputRow.outputTypes[i] == ConnectorType.Raster) OutTypeStr = "Raster:Raster";

                Line = "\t\t\t\t" + "<" + OutTypeStr + " />" + "\n";
                OutString = OutString + Line;
            }

            Line = "\t\t\t" + "</OutputTypes>" + "\n";
            OutString = OutString + Line;

            Line = "\t\t" + "</Element:Output>" + "\n";
            Line = Line.Replace("'", "\"");
            OutString = OutString + Line;

            return (OutString);
        }

        //-----------------------------------------------------------------------------------------------------
        // Public methods

        public string ExportXML()
        {
            string XMLFile = "";
            string Line;

            Line = "<?xml version='1.0' encoding='UTF-8'?>" + "\n";
            Line = Line.Replace("'", "\"");
            XMLFile = XMLFile + Line;

            Line = "<root xmlns:Element='Element' xmlns:Raster='Raster' xmlns:Vector='Vector' xmlns:Control='Control' xmlns:Value='Value'>" + "\n";
            Line = Line.Replace("'", "\"");
            XMLFile = XMLFile + Line;

            Line = "<Node style='" + IntNodeStyle + "' editorVersion='" + IntEditorVersion + "'>" + "\n";
            Line = Line.Replace("'", "\"");
            XMLFile = XMLFile + Line;

            Line = "\t" + "<NodeTitle>" + IntNodeTitle + "</NodeTitle>" + "\n";
            XMLFile = XMLFile + Line;

            Line = "\t" + "<NodeElements>" + "\n";
            XMLFile = XMLFile + Line;

            for (int i = 0; i < ToolRows.Length; i++)
            {
                if (ToolRows[i].rowType == RowType.Input) Line = ExportInput(i);
                if (ToolRows[i].rowType == RowType.Control) Line = ExportControl(i);
                if (ToolRows[i].rowType == RowType.Output) Line = ExportOutput(i);

                XMLFile = XMLFile + Line;
            }

            Line = "\t" + "</NodeElements>" + "\n";
            XMLFile = XMLFile + Line;

            Line = "</Node>" + "\n";
            XMLFile = XMLFile + Line;

            Line = "</root>";
            XMLFile = XMLFile + Line;

            return (XMLFile);
        }
    }
}
