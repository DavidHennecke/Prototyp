using DynamicData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Elements
{
    [Serializable]
    class ModuleNodeProperties
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public System.Windows.Point Position { get; set; }
    }

    [Serializable]
    class VecImportNodeProperties
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public System.Windows.Point Position { get; set; }
        public byte[] RawData;
    }

    [Serializable]
    class RasImportNodeProperties
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public System.Windows.Point Position { get; set; }
        public byte[] RawData;

    }

    [Serializable]
    class ConnectionProperties
    {

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
        
        double IntZoomFactor { get; set; }

        List<VecImportNodeProperties> IntVecImportNodeProperties { get; set; }
        List<RasImportNodeProperties> IntRasImportNodeProperties { get; set; }
        List<ModuleNodeProperties> IntModuleNodeProperties { get; set; }
        List<ConnectionProperties> IntConnectionProperties { get; set; }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public NetworkLoadAndSave()
        {
            // Nothing much to do here...
        }

        // Constructor that accepts a pointer to a network instance and extracts the necessary
        // information.
        public NetworkLoadAndSave(NodeNetwork.ViewModels.NetworkViewModel network, System.Collections.Generic.List<VectorData> vec, System.Collections.Generic.List<RasterData> ras)
        {
            // Init lists.
            IntVecImportNodeProperties = new List<VecImportNodeProperties>();
            IntRasImportNodeProperties = new List<RasImportNodeProperties>();
            IntModuleNodeProperties = new List<ModuleNodeProperties>();
            IntConnectionProperties = new List<ConnectionProperties>();

            // First, save some basic infos.
            IntZoomFactor = network.ZoomFactor;

            // Second, look into the nodes.
            foreach (NodeNetwork.ViewModels.NodeViewModel node in network.Nodes.Items)
            {
                if (node is Modules.Node_Module module)
                {
                    ModuleNodeProperties modProp = new ModuleNodeProperties();
                    modProp.Name = module.Name;
                    modProp.Path = module.PathXML;
                    modProp.Position = module.Position;

                    // Eigentlich müssten auch noch die Settings aller Controls in dem Modul gespeichert werden... :-(

                    IntModuleNodeProperties.Add(modProp);
                }
                else if (node is Modules.VectorImport_Module vecImp)
                {
                    VecImportNodeProperties impProp = new VecImportNodeProperties();
                    impProp.Name = vecImp.Name;
                    impProp.Position = vecImp.Position;

                    foreach (VectorData v in vec)
                    {
                        if (v.ID == vecImp.IntID)
                        {
                            impProp.FileName = v.FileName;
                            impProp.RawData = v.VecData;
                        }
                    }

                    IntVecImportNodeProperties.Add(impProp);
                }
                else if (node is Modules.RasterImport_Module rasImp)
                {
                    RasImportNodeProperties impProp = new RasImportNodeProperties();
                    impProp.Name = rasImp.Name;
                    impProp.Position = rasImp.Position;

                    foreach (RasterData r in ras)
                    {
                        if (r.ID == rasImp.IntID)
                        {
                            impProp.FileName = r.FileName;
                            impProp.RawData = r.Serialize();
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

                //...

                IntConnectionProperties.Add(connProp); 
            }
        }
    }
}
