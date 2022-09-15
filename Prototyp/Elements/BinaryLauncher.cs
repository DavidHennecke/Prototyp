using Prototyp.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Elements
{
    public class BinaryLauncher
    {
        public static Node_Module Launch(string BinaryPath, string Url, string ModuleName = "", VorteXML constructXML = null)
        {
            BinaryPath = BinaryPath.Replace("\\", "/");

            GrpcClient.ControlConnector.ControlConnectorClient grpcConnection;

            Node_Module nodeModule;

            System.Diagnostics.Process moduleProcess = new System.Diagnostics.Process();

            System.Diagnostics.ProcessStartInfo moduleProcessInfo = new System.Diagnostics.ProcessStartInfo(BinaryPath + ".exe", Url.Substring(Url.LastIndexOf(":") + 1));
            moduleProcessInfo.UseShellExecute = false; // 'UseShellExecute = true' would be available only on the Windows platform.
            moduleProcessInfo.LoadUserProfile = true;
            moduleProcessInfo.WorkingDirectory = BinaryPath.Substring(0, BinaryPath.LastIndexOf("/"));
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
                    nodeModule = new Node_Module(BinaryPath + ".xml", grpcConnection, Url, moduleProcess);
                }
                else
                {
                    nodeModule = new Node_Module(constructXML, ModuleName, grpcConnection, Url, moduleProcess);
                }

                moduleProcessInfo.CreateNoWindow = !nodeModule.ShowGUI;
                moduleProcess.Start();
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
