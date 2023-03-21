using Prototyp.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Prototyp.Elements
{
    public class BinaryLauncher
    {
        public static Node_Module Launch(string XMLPath, string Url, string ModuleName = "", VorteXML constructXML = null, bool DoLaunch = true, string ModulePath = "")
        {
            XMLPath = XMLPath.Replace("\\", "/");
            ModulePath = ModulePath.Replace("\\", "/");

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;

            Node_Module nodeModule;

            System.Diagnostics.Process moduleProcess = new System.Diagnostics.Process();

            //Assume XML path is the same as binary path - will be checked later, after XML has been read.
            System.Diagnostics.ProcessStartInfo moduleProcessInfo = new System.Diagnostics.ProcessStartInfo(ModulePath, Url.Substring(Url.LastIndexOf(":") + 1 ));
            moduleProcessInfo.UseShellExecute = false; // 'UseShellExecute = true' would be available only on the Windows platform.
            moduleProcessInfo.LoadUserProfile = true;
            moduleProcessInfo.WorkingDirectory = XMLPath.Substring(0, XMLPath.LastIndexOf("/"));
            moduleProcess.StartInfo = moduleProcessInfo;
            try
            {
                // Establish GRPC connection

                // This is only nessesary if you don't trust your tool's custom certificate.
                /***************************************************************************
                System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(url, new Grpc.Net.Client.GrpcChannelOptions
                {
                    HttpHandler = handler
                }
                );
                *//////////////////////////////////////////////////////////////////////////

                // Otherwise, use this:
                // /***************************************************************************
                Grpc.Net.Client.GrpcChannel channel = Grpc.Net.Client.GrpcChannel.ForAddress(Url);
                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);
                // *//////////////////////////////////////////////////////////////////////////

                grpcConnection = new GrpcClient.ControlConnector.ControlConnectorClient(channel);

                if (ModuleName == "")
                {
                    nodeModule = new Node_Module(XMLPath + ".xml", grpcConnection, Url, moduleProcess);
                }
                else
                {
                    nodeModule = new Node_Module(constructXML, ModuleName, grpcConnection, Url, moduleProcess);
                    nodeModule.PathXML = XMLPath;
                }

                if (nodeModule.StdLib)
                {
                    //Find module folder
                    moduleProcessInfo.FileName =  MainWindow.ModulesPath() + "/stdLib.exe";
                    moduleProcessInfo.Arguments = moduleProcessInfo.Arguments + " name=" + nodeModule.Name;
                    System.Diagnostics.Trace.WriteLine("Standard library module found. Launching \"" + nodeModule.Name + "\" from " + moduleProcessInfo.FileName);
                } else
                {
                    moduleProcessInfo.FileName = XMLPath + ".exe";
                }

                moduleProcessInfo.CreateNoWindow = !nodeModule.ShowGUI;
                if (DoLaunch) moduleProcess.Start();
            }
            catch
            {
                if (!System.IO.File.Exists(moduleProcessInfo.FileName))
                {
                    throw new System.Exception("Could not start binary: No executable file present.");
                }
                else
                {
                    throw new System.Exception("Could not start binary: Reason unknown.");
                }
            }

            return (nodeModule);
        }
    }
}
