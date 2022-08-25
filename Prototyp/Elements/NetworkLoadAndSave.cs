using DynamicData;
using Prototyp.Modules;
using System;
using System.Linq;

namespace Prototyp.Elements
{
    public enum ConnectionType
    {
        Module,
        Vector,
        Raster
    }

    [Serializable]
    public class ModuleNodeProperties
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public System.Windows.Point Position { get; set; }
        public System.Windows.Size Size { get; set; }
        public string XML { get; set; }
    }

    [Serializable]
    public class VecImportNodeProperties
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public System.Windows.Point Position { get; set; }
        public System.Windows.Size Size { get; set; }
        public byte[] RawData { get; set; }
    }

    [Serializable]
    public class RasImportNodeProperties
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public System.Windows.Point Position { get; set; }
        public System.Windows.Size Size { get; set; }
        public byte[] RawData { get; set; }

    }

    [Serializable]
    public class ConnectionProperties
    {
        public int InputIndex { get; set; }
        public int InputPort { get; set; }
        public int InputID { get; set; }
        public ConnectionType InputType { get; set; }
        public int OutputIndex { get; set; }
        public int OutputPort { get; set; }
        public int OutputID { get; set; }
        public ConnectionType OutputType { get; set; }
    }

    [Serializable]
    public class NetworkLoadAndSave
    {
        /***********************************************************************************

        Class NetworkLoadAndSave
        Contains properties and methods for network loading and saving.

        (c) 2022 by Carsten Croonenbroeck, Markus Berger, and David Hennecke. Contact us at
        carsten.croonenbroeck@uni-rostock.de.

            Add license information here.

        Dependencies (NuGet packages):
        - None

        *///////////////////////////////////////////////////////////////////////////////////

        // Internal variables --------------------------------------------------------------

        private double _ZoomFactor;
        private System.Windows.Point _DragOffset;
        private System.Collections.Generic.List<ModuleNodeProperties> _ModuleNodeProperties;
        private System.Collections.Generic.List<VecImportNodeProperties> _VecImportNodeProperties;
        private System.Collections.Generic.List<RasImportNodeProperties> _RasImportNodeProperties;
        private System.Collections.Generic.List<ConnectionProperties> _ConnectionProperties;

        // Getters and setters -------------------------------------------------------------

        public double ZoomFactor
        {
            get { return (_ZoomFactor); }
        }

        public System.Windows.Point DragOffset
        {
            get { return (_DragOffset); }
        }

        public System.Collections.Generic.List<ModuleNodeProperties> ModNodeProps
        {
            get { return (_ModuleNodeProperties); }
        }

        public System.Collections.Generic.List<VecImportNodeProperties> VecImportNodeProps
        {
            get { return (_VecImportNodeProperties); }
        }

        public System.Collections.Generic.List<RasImportNodeProperties> RasImportNodeProps
        {
            get { return (_RasImportNodeProperties); }
        }

        public System.Collections.Generic.List<ConnectionProperties> ConnectionProps
        {
            get { return (_ConnectionProperties); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public NetworkLoadAndSave()
        {
            // Nothing much to do here...
        }

        // Constructor that accepts pointers to the required data structures and extracts the necessary
        // information.
        public NetworkLoadAndSave(NodeNetwork.ViewModels.NetworkViewModel network,
                                  System.Collections.Generic.List<VectorData> vec,
                                  System.Collections.Generic.List<RasterData> ras,
                                  bool IncludeDataSets = true)
        {
            MakeInternalLists(network, vec, ras, IncludeDataSets);
        }

        // Constructor that accepts pointers to the required data structures, extracts the necessary
        // information, and saves the serialized result as a file.
        public NetworkLoadAndSave(NodeNetwork.ViewModels.NetworkViewModel network,
                                  System.Collections.Generic.List<VectorData> vec,
                                  System.Collections.Generic.List<RasterData> ras,
                                  string FileName,
                                  bool IncludeDataSets = true)
        {
            MakeInternalLists(network, vec, ras, IncludeDataSets);

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binForm = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (System.IO.FileStream fs = System.IO.File.Create(FileName)) { binForm.Serialize(fs, this); }
        }

        // Constructor that accepts a filename and deserializes its content.
        public NetworkLoadAndSave(string FileName)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binForm = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Prototyp.Elements.NetworkLoadAndSave Read = new NetworkLoadAndSave();
            using (System.IO.FileStream fs = System.IO.File.Open(FileName, System.IO.FileMode.Open))
            {
                Read = (Prototyp.Elements.NetworkLoadAndSave)binForm.Deserialize(fs);

                _ZoomFactor = Read.ZoomFactor;
                _DragOffset = Read.DragOffset;

                _ModuleNodeProperties = Read.ModNodeProps;
                _VecImportNodeProperties = Read.VecImportNodeProps;
                _RasImportNodeProperties = Read.RasImportNodeProps;
                _ConnectionProperties = Read.ConnectionProps;
            }
        }

        // Private methods --------------------------------------------------------------------

        private void MakeInternalLists(NodeNetwork.ViewModels.NetworkViewModel network,
                                       System.Collections.Generic.List<VectorData> vec,
                                       System.Collections.Generic.List<RasterData> ras,
                                       bool IncludeDataSets)
        {
            // Init lists.
            _VecImportNodeProperties = new System.Collections.Generic.List<VecImportNodeProperties>();
            _RasImportNodeProperties = new System.Collections.Generic.List<RasImportNodeProperties>();
            _ModuleNodeProperties = new System.Collections.Generic.List<ModuleNodeProperties>();
            _ConnectionProperties = new System.Collections.Generic.List<ConnectionProperties>();

            // First, save some basic infos.
            _ZoomFactor = network.ZoomFactor;
            _DragOffset = network.DragOffset;

            // Second, look into the nodes.
            foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
            {
                if (node is Node_Module module)
                {
                    ModuleNodeProperties modProp = new ModuleNodeProperties();
                    modProp.Name = module.Name;
                    modProp.Path = module.PathXML;
                    modProp.Position = module.Position;
                    modProp.Size = module.Size;
                    modProp.XML = System.IO.File.ReadAllText(module.PathXML);

                    // Eigentlich müssten auch noch die Settings aller Controls in dem Modul gespeichert werden... :-(

                    _ModuleNodeProperties.Add(modProp);
                }
                else if (node is VectorImport_Module vecImp)
                {
                    VecImportNodeProperties impProp = new VecImportNodeProperties();
                    impProp.Name = vecImp.Name;
                    impProp.Position = vecImp.Position;
                    impProp.Size = vecImp.Size;

                    foreach (VectorData v in vec)
                    {
                        if (v.ID == vecImp.IntID)
                        {
                            impProp.FileName = v.FileName;
                            if (IncludeDataSets) impProp.RawData = v.VecData;
                        }
                    }

                    _VecImportNodeProperties.Add(impProp);
                }
                else if (node is RasterImport_Module rasImp)
                {
                    RasImportNodeProperties impProp = new RasImportNodeProperties();
                    impProp.Name = rasImp.Name;
                    impProp.Position = rasImp.Position;
                    impProp.Size = rasImp.Size;

                    foreach (RasterData r in ras)
                    {
                        if (r.ID == rasImp.IntID)
                        {
                            impProp.FileName = r.FileName;
                            if (IncludeDataSets) impProp.RawData = r.Serialize();
                        }
                    }

                    _RasImportNodeProperties.Add(impProp);
                }
                // else if (other data types...)
                {

                }
            }

            // Third, check the connections.
            foreach (NodeNetwork.ViewModels.ConnectionViewModel conn in network.Connections.Items)
            {
                ConnectionProperties connProp = new ConnectionProperties();

                int i = 0;

                connProp.InputID = conn.Input.GetID();
                connProp.OutputID = conn.Output.GetID();

                // Find the attached input node (*TO* which this connection feeds).
                if (conn.Input.Parent is Node_Module moduleIn)
                {
                    // Find the corresponding entry in the modules list.
                    for (i = 0; i < _ModuleNodeProperties.Count; i++)
                    {
                        if (_ModuleNodeProperties[i].Position == conn.Input.Parent.Position && _ModuleNodeProperties[i].Size == conn.Input.Parent.Size)
                        {
                            connProp.InputType = ConnectionType.Module;
                            connProp.InputIndex = i;
                            break;
                        }
                    }

                    // Find the attached port.
                    i = 0;
                    foreach (NodeNetwork.ViewModels.NodeInputViewModel p in moduleIn.VisibleInputs.Items)
                    {
                        if (p.Port == conn.Input.Port)
                        {
                            connProp.InputPort = i;
                            break;
                        }
                        i++;
                    }
                }
                // else if (...?) Input can't be an importer, as these only have outputs. Maybe something for the future.

                // Find the attached output node (*FROM* which this connection feeds).
                if (conn.Output.Parent is Node_Module moduleOut)
                {
                    // Find the corresponding entry in the modules list.
                    for (i = 0; i < _ModuleNodeProperties.Count; i++)
                    {
                        if (_ModuleNodeProperties[i].Position == conn.Output.Parent.Position && _ModuleNodeProperties[i].Size == conn.Output.Parent.Size)
                        {
                            connProp.OutputType = ConnectionType.Module;
                            connProp.OutputIndex = i;
                            break;
                        }
                    }

                    // Find the attached port.
                    i = 0;
                    foreach (NodeNetwork.ViewModels.NodeOutputViewModel p in moduleOut.VisibleOutputs.Items)
                    {
                        if (p.Port == conn.Output.Port)
                        {
                            connProp.OutputPort = i;
                            break;
                        }
                        i++;
                    }
                }
                else if (conn.Output.Parent is VectorImport_Module vecImp)
                {
                    // Find the corresponding entry in the vector imports list.
                    for (i = 0; i < _VecImportNodeProperties.Count; i++)
                    {
                        if (_VecImportNodeProperties[i].Position == conn.Output.Parent.Position && _VecImportNodeProperties[i].Size == conn.Output.Parent.Size)
                        {
                            connProp.OutputType = ConnectionType.Vector;
                            connProp.OutputIndex = i;
                            break;
                        }
                    }

                    // Find the attached port.
                    i = 0;
                    foreach (NodeNetwork.ViewModels.NodeOutputViewModel p in vecImp.VisibleOutputs.Items)
                    {
                        if (p.Port == conn.Output.Port)
                        {
                            connProp.OutputPort = i;
                            break;
                        }
                        i++;
                    }
                }
                else if (conn.Output.Parent is RasterImport_Module rasImp)
                {
                    // Find the corresponding entry in the raster imports list.
                    for (i = 0; i < _RasImportNodeProperties.Count; i++)
                    {
                        if (_RasImportNodeProperties[i].Position == conn.Output.Parent.Position && _RasImportNodeProperties[i].Size == conn.Output.Parent.Size)
                        {
                            connProp.OutputType = ConnectionType.Raster;
                            connProp.OutputIndex = i;
                            break;
                        }
                    }

                    // Find the attached port.
                    i = 0;
                    foreach (NodeNetwork.ViewModels.NodeOutputViewModel p in rasImp.VisibleOutputs.Items)
                    {
                        if (p.Port == conn.Output.Port)
                        {
                            connProp.OutputPort = i;
                            break;
                        }
                        i++;
                    }
                }

                _ConnectionProperties.Add(connProp);
            }
        }

        // Public methods --------------------------------------------------------------------

        public NodeNetwork.ViewModels.NetworkViewModel ImportWorkflow(System.Collections.Generic.List<VectorData> vectorData,
                                                                      System.Collections.Generic.List<RasterData> rasterData,
                                                                      NodeNetwork.ViewModels.NetworkViewModel network,
                                                                      string ModulesPath)
        {
            // Tidy up all lists.
            //vectorData.Clear();                                           // Do
            //rasterData.Clear();                                           // we
            //MainWindow.AppWindow.TableOfContentsVector.Items.Clear();     // really
            //MainWindow.AppWindow.TableOfContentsRaster.Items.Clear();     // want this?
            network = null;
            network = new NodeNetwork.ViewModels.NetworkViewModel();
            MainWindow.AppWindow.networkView.ViewModel = network;

            // Set up elementary stuff.
            network.ZoomFactor = ZoomFactor;
            network.DragOffset = DragOffset;

            // Pre-populate the ports to be used.
            int i = 0;
            int[] PrepPorts = null;
            if (ModNodeProps.Count > 0)
            {
                PrepPorts = new int[ModNodeProps.Count];
                PrepPorts[0] = Node_Module.GetNextPort(MainWindow.BASEPORT);
                for (i = 1; i < PrepPorts.Count(); i++) PrepPorts[i] = Node_Module.GetNextPort(PrepPorts[i - 1] + 1);
            }

            // Okay, add the module nodes, start the corresponding servers.
            i = 0;
            foreach (ModuleNodeProperties m in ModNodeProps)
            {
                VorteXML constructXML = new VorteXML(m.XML);

                GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;

                System.Diagnostics.Process moduleProcess = new System.Diagnostics.Process();

                System.Diagnostics.ProcessStartInfo moduleProcessInfo = new System.Diagnostics.ProcessStartInfo(ModulesPath + "\\" + m.Name + "\\" + m.Name + ".exe", PrepPorts[i].ToString());
                //moduleProcessInfo.CreateNoWindow = true;
                moduleProcessInfo.UseShellExecute = false;
                moduleProcessInfo.LoadUserProfile = true;
                moduleProcessInfo.WorkingDirectory = ModulesPath + "\\" + m.Name;
                moduleProcess.StartInfo = moduleProcessInfo;
                try
                {
                    moduleProcess.Start();
                }
                catch
                {
                    if (!System.IO.File.Exists(ModulesPath + "\\" + m.Name + "\\" + m.Name + ".exe"))
                    {
                        // Maybe this user doesn't have that module installed? Go ahead and grab it, pal!
                        throw new System.Exception("Could not start binary: No executable file present.");
                    }
                    else
                    {
                        throw new System.Exception("Could not start binary: Reason unknown.");
                    }
                }

                // Establish GRPC connection
                // TODO: nicht nur localhost
                string url = "https://localhost:" + PrepPorts[i].ToString();
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);

                Node_Module nodeModule = new Node_Module(constructXML, m.Name, grpcConnection, url, moduleProcess);

                nodeModule.Position = m.Position;
                //newModule.Size = m.Size; // Damn, write protected.
                nodeModule.PathXML = ModulesPath + "\\" + m.Name + "\\" + m.Name + ".xml";
                network.Nodes.Add(nodeModule);

                i++;
            }

            // Next, add the vector data/list entry/node.
            foreach (VecImportNodeProperties v in VecImportNodeProps)
            {
                // Is data already present? If so, do not load again.
                bool AlreadyPresent = false;
                foreach (VectorData vec in vectorData)
                {
                    if (vec.Name == v.Name) // Maybe 'Name' is not the best criterion for uniqueness, but it's better than nothing. TODO.
                    {
                        AlreadyPresent = true;
                        break;
                    }
                }
                if (!AlreadyPresent)
                {
                    if (v.RawData == null)
                    {
                        // If data is not embedded and the file cannot be found at the specified location, we have a problem.
                        // TODO: Ask the user to specify location?
                        if (!System.IO.File.Exists(v.FileName)) throw new Exception("File not found. Invalid path?");
                        vectorData.Add(new VectorData(v.FileName));
                    }
                    else
                    {
                        vectorData.Add(new VectorData(v.RawData));
                    }

                }

                VectorImport_Module importNode = null;

                string geometryType = vectorData.Last().FeatureCollection[0].Geometry.GeometryType;
                if (geometryType == "Point")
                {
                    importNode = new VectorImport_ModulePoint(vectorData.Last().Name, vectorData.Last().FeatureCollection[0].Geometry.GeometryType, vectorData.Last().ID);
                }
                else if (geometryType == "Line")
                {
                    importNode = new VectorImport_ModuleLine(vectorData.Last().Name, vectorData.Last().FeatureCollection[0].Geometry.GeometryType, vectorData.Last().ID);
                }
                else if (geometryType == "Polygon")
                {
                    importNode = new VectorImport_ModulePolygon(vectorData.Last().Name, vectorData.Last().FeatureCollection[0].Geometry.GeometryType, vectorData.Last().ID);
                }
                else if (geometryType == "MultiPolygon")
                {
                    importNode = new VectorImport_ModuleMultiPolygon(vectorData.Last().Name, vectorData.Last().FeatureCollection[0].Geometry.GeometryType, vectorData.Last().ID);
                }

                importNode.Position = v.Position;
                network.Nodes.Add(importNode);
            }

            // Then, add the raster data/list entry/node.
            foreach (RasImportNodeProperties r in RasImportNodeProps)
            {
                // Is data already present? If so, do not load again.
                bool AlreadyPresent = false;
                foreach (RasterData ras in rasterData)
                {
                    if (ras.Name == r.Name) // Maybe 'Name' is not the best criterion for uniqueness, but it's better than nothing. TODO.
                    {
                        AlreadyPresent = true;
                        break;
                    }
                }
                if (!AlreadyPresent)
                {
                    if (r.RawData == null)
                    {
                        // If data is not embedded and the file cannot be found at the specified location, we have a problem.
                        // TODO: Ask the user to specify location?
                        if (!System.IO.File.Exists(r.FileName)) throw new Exception("File not found. Invalid path?");
                        rasterData.Add(new RasterData(r.FileName));
                    }
                    else
                    {
                        rasterData.Add(new RasterData(r.RawData));
                    }
                }

                RasterImport_Module importNode = new RasterImport_Module(r.Name, rasterData.Last().FileType, rasterData.Last().ID);
                importNode.Position = r.Position;
                network.Nodes.Add(importNode);
            }

            // Finally, add the connections.
            NodeNetwork.ViewModels.NodeViewModel inpNode = null;
            NodeNetwork.ViewModels.NodeViewModel outpNode = null;
            i = 0;

            foreach (ConnectionProperties c in ConnectionProps)
            {
                // First, find the input node.
                i = 0;
                if (c.InputType == ConnectionType.Module)
                {
                    foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                    {
                        if (node is Node_Module m)
                        {
                            if (i == c.InputIndex)
                            {
                                inpNode = node;
                                break;
                            }
                            i++;
                        }
                    }
                }
                // ... Can't be anything other that Module right now. Maybe in the future?

                // Then, find the output node.
                i = 0;
                if (c.OutputType == ConnectionType.Module)
                {
                    foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                    {
                        if (node is Node_Module m)
                        {
                            if (i == c.OutputIndex)
                            {
                                outpNode = node;
                                break;
                            }
                            i++;
                        }
                    }
                }
                else if (c.OutputType == ConnectionType.Vector)
                {
                    foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                    {
                        if (node is VectorImport_Module v)
                        {
                            if (i == c.OutputIndex)
                            {
                                outpNode = node;
                                break;
                            }
                            i++;
                        }
                    }
                }
                else if (c.OutputType == ConnectionType.Raster)
                {
                    foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                    {
                        if (node is RasterImport_Module r)
                        {
                            if (i == c.OutputIndex)
                            {
                                outpNode = node;
                                break;
                            }
                            i++;
                        }
                    }
                }

                // Create the connection (input and output).
                NodeNetwork.ViewModels.NodeInputViewModel nInp = null;
                NodeNetwork.ViewModels.NodeOutputViewModel nOutp = null;

                // Find the ports, input and output.
                i = 0;
                foreach (NodeNetwork.ViewModels.NodeInputViewModel ni in inpNode.Inputs.Items)
                {
                    if (i == c.InputPort)
                    {
                        nInp = ni;
                        break;
                    }
                    i++;
                }
                i = 0;
                foreach (NodeNetwork.ViewModels.NodeOutputViewModel no in outpNode.Outputs.Items)
                {
                    if (i == c.OutputPort)
                    {
                        nOutp = no;
                        break;
                    }
                    i++;
                }

                nInp.SetID(c.InputID);
                nOutp.SetID(c.OutputID);

                network.Connections.Add(network.ConnectionFactory(nInp, nOutp));
            }

            return (network);
        }
    }
}
