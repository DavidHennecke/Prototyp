﻿using Prototyp.Modules;
using System;

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

        (c) 2022 by Carsten Croonenbroeck, Markus Berger and David Hennecke. Contact us at
        carsten.croonenbroeck@uni-rostock.de.

            Add license information here.

        Dependencies (NuGet packages):
        - None

        *///////////////////////////////////////////////////////////////////////////////////

        // Internal variables --------------------------------------------------------------

        private double IntZoomFactor;
        private System.Windows.Point IntDragOffset;
        private System.Collections.Generic.List<ModuleNodeProperties> IntModuleNodeProperties;
        private System.Collections.Generic.List<VecImportNodeProperties> IntVecImportNodeProperties;
        private System.Collections.Generic.List<RasImportNodeProperties> IntRasImportNodeProperties;
        private System.Collections.Generic.List<ConnectionProperties> IntConnectionProperties;

        // Getters and setters -------------------------------------------------------------

        public double ZoomFactor
        {
            get { return (IntZoomFactor); }
        }

        public System.Windows.Point DragOffset
        {
            get { return (IntDragOffset); }
        }

        public System.Collections.Generic.List<ModuleNodeProperties> ModNodeProps
        {
            get { return (IntModuleNodeProperties); }
        }

        public System.Collections.Generic.List<VecImportNodeProperties> VecImportNodeProps
        {
            get { return (IntVecImportNodeProperties); }
        }

        public System.Collections.Generic.List<RasImportNodeProperties> RasImportNodeProps
        {
            get { return (IntRasImportNodeProperties); }
        }

        public System.Collections.Generic.List<ConnectionProperties> ConnectionProps
        {
            get { return (IntConnectionProperties); }
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

                IntZoomFactor = Read.ZoomFactor;
                IntDragOffset = Read.DragOffset;

                IntModuleNodeProperties = Read.ModNodeProps;
                IntVecImportNodeProperties = Read.VecImportNodeProps;
                IntRasImportNodeProperties = Read.RasImportNodeProps;
                IntConnectionProperties = Read.ConnectionProps;
            }
        }

        // Private methods --------------------------------------------------------------------

        private void MakeInternalLists(NodeNetwork.ViewModels.NetworkViewModel network,
                                       System.Collections.Generic.List<VectorData> vec,
                                       System.Collections.Generic.List<RasterData> ras,
                                       bool IncludeDataSets)
        {
            // Init lists.
            IntVecImportNodeProperties = new System.Collections.Generic.List<VecImportNodeProperties>();
            IntRasImportNodeProperties = new System.Collections.Generic.List<RasImportNodeProperties>();
            IntModuleNodeProperties = new System.Collections.Generic.List<ModuleNodeProperties>();
            IntConnectionProperties = new System.Collections.Generic.List<ConnectionProperties>();

            // First, save some basic infos.
            IntZoomFactor = network.ZoomFactor;
            IntDragOffset = network.DragOffset;

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

                    IntModuleNodeProperties.Add(modProp);
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

                    IntVecImportNodeProperties.Add(impProp);
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

                    IntRasImportNodeProperties.Add(impProp);
                }
                // else if (other data types...)
                {

                }
            }

            // Third, check the connections.
            foreach (NodeNetwork.ViewModels.ConnectionViewModel conn in network.Connections.Items)
            {
                ConnectionProperties connProp = new ConnectionProperties();

                connProp.InputID = conn.Input.GetID();
                connProp.OutputID = conn.Output.GetID();

                // Find the attached input node (*TO* which this connection feeds).
                //foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                {
                    if (conn.Input.Parent is Node_Module module)
                    {
                        int i = 0;

                        // Find the corresponding entry in the modules list.
                        for (i = 0; i < IntModuleNodeProperties.Count; i++)
                        {
                            //if (IntModuleNodeProperties[i].Position == node.Position & IntModuleNodeProperties[i].Size == node.Size)
                            if (IntModuleNodeProperties[i].Position == conn.Input.Parent.Position & IntModuleNodeProperties[i].Size == conn.Input.Parent.Size)
                            {
                                connProp.InputType = ConnectionType.Module;
                                connProp.InputIndex = i;
                                break;
                            }
                        }

                        // Find the attached port.
                        i = 0;
                        foreach (NodeNetwork.ViewModels.NodeInputViewModel p in module.VisibleInputs.Items)
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
                }

                // Find the attached output node (*FROM* which this connection feeds).
                //foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
                {
                    if (conn.Output.Parent is Node_Module module)
                    {
                        int i = 0;

                        // Find the corresponding entry in the modules list.
                        for (i = 0; i < IntModuleNodeProperties.Count; i++)
                        {
                            //if (IntModuleNodeProperties[i].Position == node.Position & IntModuleNodeProperties[i].Size == node.Size)
                            if (IntModuleNodeProperties[i].Position == conn.Output.Parent.Position & IntModuleNodeProperties[i].Size == conn.Output.Parent.Size)
                            {
                                connProp.OutputType = ConnectionType.Module;
                                connProp.OutputIndex = i;
                                break;
                            }
                        }

                        // Find the attached port.
                        i = 0;
                        foreach (NodeNetwork.ViewModels.NodeOutputViewModel p in module.VisibleOutputs.Items)
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
                        int i = 0;

                        // Find the corresponding entry in the vector imports list.
                        for (i = 0; i < IntVecImportNodeProperties.Count; i++)
                        {
                            //if (IntVecImportNodeProperties[i].Position == node.Position & IntVecImportNodeProperties[i].Size == node.Size)
                            if (IntVecImportNodeProperties[i].Position == conn.Output.Parent.Position & IntVecImportNodeProperties[i].Size == conn.Output.Parent.Size)
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
                        int i = 0;

                        // Find the corresponding entry in the vector imports list.
                        for (i = 0; i < IntRasImportNodeProperties.Count; i++)
                        {
                            //if (IntRasImportNodeProperties[i].Position == node.Position & IntRasImportNodeProperties[i].Size == node.Size)
                            if (IntRasImportNodeProperties[i].Position == conn.Output.Parent.Position & IntRasImportNodeProperties[i].Size == conn.Output.Parent.Size)
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
                }

                IntConnectionProperties.Add(connProp);
            }
        }
    }
}
