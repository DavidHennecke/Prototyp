using System.Linq;

namespace Prototyp.Elements
{
    public class VectorData
    {
        /***********************************************************************************

        Class VectorData
        Contains properties and methods for vector data handling.
        
        Internally, vector data is stored as a serialized FlatGeoBuf object,
        i.e. as a byte array. It can easily be deserialized and then is represented as
        as collection of geometries and attributes.

        The class also handles ESRI shapefiles and all other kinds of vector layers
        supported by GDAL. Loading a layer utilizes GDAL to actually import it. The data
        is then converted to FlatGeoBuf and serialized for internal storage. All
        supported vector data types can be imported and exported, loaded and saved.

        The class also contains methods to sending and receiving vector data via gRPC.
            ???

        (c) 2022 by Carsten Croonenbroeck, Markus Berger and David Hennecke. Contact us at
        carsten.croonenbroeck@uni-rostock.de.

            Add license information here.

        Dependencies (NuGet packages):
        - MaxRev.Gdal.Core
        - MaxRev.Gdal.WindowsRuntime.Minimal
        - FlatGeobuf
        - NetTopologySuite (comes with FlatGeobuf, update if necessary)

        *///////////////////////////////////////////////////////////////////////////////////

        // Internal variables --------------------------------------------------------------

        private byte[] IntVecData;
        private bool IntBusy;
        private OSGeo.OSR.SpatialReference IntSRS;
        private string IntName;
        private string IntDescription;
        private double IntID = 0.0;

        // Getters and setters -------------------------------------------------------------

        public byte[] VecData
        {
            get { return (SerializeVec()); }
            set
            {
                if (ByteArrValid(value))
                {
                    IntVecData = value;
                    HandleNameAndCRS();
                }
                else
                {
                    throw new System.Exception("Array does not represent a valid FlatGeobuf format.");
                }
            }
        }

        public bool Busy
        {
            get { return (IntBusy); }
        }

        public string Name
        {
            get { return (IntName); }
            set { IntName = value; }
        }

        public string Description
        {
            get { return (IntDescription); }
            set { IntDescription = value; }
        }

        public OSGeo.OSR.SpatialReference SpatialReference
        {
            get { return (IntSRS); }
            set { IntSRS = value; }
        }

        public OSGeo.OGR.Layer Layer
        {
            get { return (GetAsLayer()); }
            set { if (value != null) ImportLayer(value); }
        }

        public string GeoJSON
        {
            get { return (GetAsGeoJSON()); }
            set
            {
                if (value != null) IntVecData = ImportFromGeoJSON(value);
            }
        }

        public NetTopologySuite.Features.FeatureCollection FeatureCollection
        {
            get
            {
                if (IntVecData != null)
                {
                    NetTopologySuite.Features.FeatureCollection MyFC = FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(IntVecData);
                    return (MyFC);
                }
                else
                {
                    return (null);
                }
            }

            set
            {
                if (value != null)
                {
                    IntVecData = FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(value, FlatGeobuf.GeometryType.Unknown);
                    IntSRS = null;
                    IntName = null;
                    IntDescription = null;
                }
            }
        }
        public double ID
        {
            get { return (IntID); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public VectorData()
        {
            MakeID();
        }

        // Constructor that opens an entire FlatGeobuf file.
        // Example:
        // VectorData vectorData = new VectorData("C:/Temp/UScounties.fgb");
        public VectorData(string FlatGeobufFileName)
        {
            if (System.IO.File.Exists(FlatGeobufFileName))
            {
                IntBusy = true;
                using (System.IO.Stream SourceFile = System.IO.File.OpenRead(FlatGeobufFileName))
                {
                    byte[] FileBuffer = new byte[8];
                    SourceFile.Read(FileBuffer, 0, FileBuffer.Length);
                    if (!ByteArrValid(FileBuffer))
                    {
                        throw new System.Exception("File is not in valid FlatGeobuf format.");
                    }
                }

                IntVecData = System.IO.File.ReadAllBytes(FlatGeobufFileName);
                HandleNameAndCRS();
                MakeID();
            }
            else
            {
                throw new System.Exception("File does not exist.");
            }
            IntBusy = false;
        }

        // Constructor that opens a rectangle out of a FlatGeobuf file.
        // Example:
        // var FilterRect = new NetTopologySuite.Geometries.Envelope(-100, -90, 40, 30);
        // VectorData vectorData = new VectorData("C:/Temp/UScounties.fgb", FilterRect);
        public VectorData(string FlatGeobufFileName, NetTopologySuite.Geometries.Envelope FlatGeobufRect)
        {
            if (System.IO.File.Exists(FlatGeobufFileName))
            {
                IntBusy = true;
                using (System.IO.Stream SourceFile = System.IO.File.OpenRead(FlatGeobufFileName))
                {
                    byte[] FileBuffer = new byte[8];
                    SourceFile.Read(FileBuffer, 0, FileBuffer.Length);
                    if (FileBuffer[0] != 0x66 | FileBuffer[1] != 0x67 | FileBuffer[2] != 0x62 | FileBuffer[4] != 0x66 | FileBuffer[5] != 0x67 | FileBuffer[6] != 0x62)
                    {
                        throw new System.Exception("File is not in valid FlatGeobuf format.");
                    }
                }

                using (System.IO.Stream SourceFile = System.IO.File.OpenRead(FlatGeobufFileName))
                {
                    var FeaturesList = FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(SourceFile, FlatGeobufRect);
                    NetTopologySuite.Features.FeatureCollection NewCollection = new NetTopologySuite.Features.FeatureCollection();
                    foreach (var ListElement in FeaturesList) { NewCollection.Add(ListElement); }

                    SourceFile.Seek(0, 0);
                    FlatGeobuf.Header MyHeader = FlatGeobuf.Helpers.ReadHeader(SourceFile);
                    HandleHeader(MyHeader);

                    IntVecData = FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(NewCollection, FlatGeobuf.GeometryType.Unknown);
                    MakeID();
                }
            }
            else
            {
                throw new System.Exception("File is not in valid FlatGeobuf format.");
            }
            IntBusy = false;
        }

        // Constructor that is provided a GDAL layer as a data source (e.g. obtained from a shape file).
        // Example:
        // VectorData vectorData = new VectorData(LayerData);
        public VectorData(OSGeo.OGR.Layer LayerData)
        {
            if (LayerData != null)
            {
                IntBusy = true;
                IntVecData = ImportLayer(LayerData);
                IntBusy = false;
                MakeID();
            }
        }

        // Constructor that is provided a byte array containing serialized FGB VectorData.
        // Example:
        // VectorData vectorData = new VectorData(VecArray);
        public VectorData(byte[] VecArray)
        {
            if (ByteArrValid(VecArray))
            {
                IntVecData = VecArray;
                HandleNameAndCRS();
                MakeID();
            }
        }

        // Private methods -----------------------------------------------------------------

        // Make ID.
        private void MakeID()
        {
            System.Random rnd = new System.Random();
            IntID = rnd.NextDouble();
        }

        // Checks the FGB-validity of a provided byte array.
        private bool ByteArrValid(byte[] TestData)
        {
            if (TestData.Length >= 8)
            {
                if (TestData[0] == 0x66 & TestData[1] == 0x67 & TestData[2] == 0x62 & TestData[4] == 0x66 & TestData[5] == 0x67 & TestData[6] == 0x62)
                {
                    return (true);
                }
            }
            return (false);
        }

        // Imports a GDAL layer.
        private byte[] ImportLayer(OSGeo.OGR.Layer LayerData)
        {
            if (LayerData == null) return (null);

            OSGeo.OGR.Geometry OGRGeom;
            OSGeo.OGR.FieldType ThisType;
            NetTopologySuite.Geometries.Geometry NTSGeom;
            NetTopologySuite.Features.AttributesTable NTSAttribTable;
            NetTopologySuite.Features.Feature NTSFeature;
            byte[] WkbBuffer;
            NetTopologySuite.Features.FeatureCollection NTSFC = new NetTopologySuite.Features.FeatureCollection();

            for (int i = 0; i < LayerData.GetFeatureCount(0); i++)
            {
                OGRGeom = LayerData.GetFeature(i).GetGeometryRef();
                WkbBuffer = new byte[OGRGeom.WkbSize()];
                OGRGeom.ExportToWkb(WkbBuffer);

                NetTopologySuite.IO.WKBReader MyReader = new NetTopologySuite.IO.WKBReader();
                NTSGeom = MyReader.Read(WkbBuffer);

                NTSFeature = new NetTopologySuite.Features.Feature();
                NTSFeature.Geometry = NTSGeom;

                if (LayerData.GetFeature(i).GetFieldCount() > 0)
                {
                    NTSAttribTable = new NetTopologySuite.Features.AttributesTable();
                    for (int j = 0; j < LayerData.GetFeature(i).GetFieldCount(); j++)
                    {
                        ThisType = LayerData.GetFeature(i).GetFieldType(j);
                        if (ThisType == OSGeo.OGR.FieldType.OFTInteger)
                        {
                            NTSAttribTable.Add(LayerData.GetFeature(i).GetFieldDefnRef(j).GetName(), LayerData.GetFeature(i).GetFieldAsInteger(j));
                        }
                        else if (ThisType == OSGeo.OGR.FieldType.OFTInteger64)
                        {
                            NTSAttribTable.Add(LayerData.GetFeature(i).GetFieldDefnRef(j).GetName(), LayerData.GetFeature(i).GetFieldAsInteger64(j));
                        }
                        else if (ThisType == OSGeo.OGR.FieldType.OFTReal)
                        {
                            NTSAttribTable.Add(LayerData.GetFeature(i).GetFieldDefnRef(j).GetName(), LayerData.GetFeature(i).GetFieldAsDouble(j));
                        }
                        else
                        {
                            NTSAttribTable.Add(LayerData.GetFeature(i).GetFieldDefnRef(j).GetName(), LayerData.GetFeature(i).GetFieldAsString(j));
                        }
                    }
                    NTSFeature.Attributes = NTSAttribTable;
                }

                NTSFC.Add(NTSFeature);
            }

            IntSRS = LayerData.GetSpatialRef();
            IntName = LayerData.GetName();
            IntDescription = null;

            return (FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(NTSFC, FlatGeobuf.GeometryType.Unknown));
        }

        // Returns the internal data representation as a GDAL layer.
        private OSGeo.OGR.Layer GetAsLayer()
        {
            if (IntVecData == null) return (null);

            IntBusy = true;
            InitGDAL();

            OSGeo.OGR.Driver MyDriver = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile");
            OSGeo.OGR.DataSource MyDS = MyDriver.CreateDataSource("/vsimem/Temporary", new string[] { });
            OSGeo.OGR.Layer MyLayer = MyDS.CreateLayer(IntName + MyDS.GetLayerCount().ToString(), IntSRS, OSGeo.OGR.wkbGeometryType.wkbUnknown, new string[] { });
            OSGeo.OGR.FeatureDefn MyFeatureDefn = MyLayer.GetLayerDefn();
            OSGeo.OGR.Feature MyFeature;
            OSGeo.OGR.FieldDefn MyFieldDefn = null;
            NetTopologySuite.Features.FeatureCollection MyFC = FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(IntVecData);
            NetTopologySuite.IO.WKBWriter MyWriter;
            byte[] WKB;
            string[] AttribNames = MyFC[0].Attributes.GetNames();
            string[] FieldTypes = new string[AttribNames.Length];

            if (MyFC[0].Attributes.Count > 0)
            {
                // Prepare fields.
                for (int i = 0; i < MyFC[0].Attributes.Count; i++)
                {
                    FieldTypes[i] = MyFC[0].Attributes.GetType(AttribNames[i]).ToString();
                    if (FieldTypes[i] == "System.Int32")
                    {
                        MyFieldDefn = new OSGeo.OGR.FieldDefn(AttribNames[i], OSGeo.OGR.FieldType.OFTInteger);
                    }
                    else if (FieldTypes[i] == "System.Int64")
                    {
                        MyFieldDefn = new OSGeo.OGR.FieldDefn(AttribNames[i], OSGeo.OGR.FieldType.OFTInteger64);
                    }
                    else if (FieldTypes[i] == "System.Double")
                    {
                        MyFieldDefn = new OSGeo.OGR.FieldDefn(AttribNames[i], OSGeo.OGR.FieldType.OFTReal);
                    }
                    else
                    {
                        MyFieldDefn = new OSGeo.OGR.FieldDefn(AttribNames[i], OSGeo.OGR.FieldType.OFTString);
                    }

                    MyLayer.CreateField(MyFieldDefn, 0);
                    MyFieldDefn.Dispose();
                }

                for (int i = 0; i < MyFC.Count; i++)
                {
                    // Handle geometry.
                    MyWriter = new NetTopologySuite.IO.WKBWriter();
                    WKB = MyWriter.Write(MyFC[i].Geometry);
                    OSGeo.OGR.Geometry OGRGeom = OSGeo.OGR.Geometry.CreateFromWkb(WKB);
                    MyFeature = new OSGeo.OGR.Feature(MyFeatureDefn);
                    MyFeature.SetGeometry(OGRGeom);

                    // Handle fields.
                    if (MyFC[i].Attributes.Count > 0)
                    {
                        object[] AttribValuesObj = MyFC[i].Attributes.GetValues();
                        for (int j = 0; j < MyFC[i].Attributes.Count; j++)
                        {
                            if (FieldTypes[j] == "System.Int32")
                            {
                                MyFeature.SetField(j, System.Convert.ToInt32(AttribValuesObj[j]));
                            }
                            else if (FieldTypes[j] == "System.Int64")
                            {
                                MyFeature.SetField(j, System.Convert.ToInt64(AttribValuesObj[j]));
                            }
                            else if (FieldTypes[j] == "System.Double")
                            {
                                MyFeature.SetField(j, System.Convert.ToDouble(AttribValuesObj[j]));
                            }
                            else
                            {
                                MyFeature.SetField(j, AttribValuesObj[j].ToString());
                            }
                        }
                    }

                    MyLayer.CreateFeature(MyFeature);
                    MyFeature.Dispose();
                }
                MyFeatureDefn.Dispose();
                MyDS.SyncToDisk();
                MyDriver.Dispose();
            }

            IntBusy = false;
            return (MyLayer);
        }

        // Returns the internal data representation as a GeoJSON string.
        private string GetAsGeoJSON()
        {
            if (IntVecData == null) return (null);

            IntBusy = true;
            NetTopologySuite.IO.GeoJsonWriter MyWriter = new NetTopologySuite.IO.GeoJsonWriter();
            string GeoJSON = MyWriter.Write(FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(IntVecData));
            IntBusy = false;

            return (GeoJSON);
        }

        // Imports a GeoJSON string.
        private byte[] ImportFromGeoJSON(string JSONData)
        {
            if (JSONData == null) return (null);

            IntBusy = true;
            NetTopologySuite.IO.GeoJsonReader MyReader = new NetTopologySuite.IO.GeoJsonReader();
            NetTopologySuite.Features.FeatureCollection MyFC = MyReader.Read<NetTopologySuite.Features.FeatureCollection>(JSONData);
            byte[] MyData = null;
            if (MyFC != null) MyData = FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(MyFC, FlatGeobuf.GeometryType.Unknown);
            InitGDAL();
            IntSRS = new OSGeo.OSR.SpatialReference(null);
            IntSRS.ImportFromEPSG(4326);
            IntName = null;
            IntDescription = null;
            IntBusy = false;

            return (MyData);
        }

        private void HandleHeader(FlatGeobuf.Header MyHeader)
        {
            InitGDAL();
            IntSRS = new OSGeo.OSR.SpatialReference(null);

            IntName = MyHeader.Name;
            IntDescription = MyHeader.Description;
            FlatGeobuf.Crs? MyCRS = MyHeader.Crs;
            string WKTString = MyCRS?.Wkt;
            int Code = (int)MyCRS?.Code;
            string Organization = MyCRS?.Org;
            IntSRS.ImportFromWkt(ref WKTString);

            string Proj4;            
            IntSRS.ExportToProj4(out Proj4);
            if (Proj4 == "")
            {
                if (Organization == "EPSG") IntSRS.ImportFromEPSG(Code);
            }
        }

        private void HandleNameAndCRS()
        {
            using (System.IO.MemoryStream MemStream = new System.IO.MemoryStream(IntVecData))
            {
                FlatGeobuf.Header MyHeader = FlatGeobuf.Helpers.ReadHeader(MemStream);
                HandleHeader(MyHeader);
            }
        }

        // Subsequent stuff here stolen and modified from https://github.com/flatgeobuf/flatgeobuf/blob/master/src/net/FlatGeobuf/NTS/FeatureCollectionConversions.cs#L116.
        private static FlatGeobuf.ColumnType ToColumnType(System.Type type)
        {
            if (System.Type.GetTypeCode(type) == System.TypeCode.Byte) return (FlatGeobuf.ColumnType.UByte);
            if (System.Type.GetTypeCode(type) == System.TypeCode.SByte) return (FlatGeobuf.ColumnType.Byte);
            if (System.Type.GetTypeCode(type) == System.TypeCode.Boolean) return (FlatGeobuf.ColumnType.Bool);
            if (System.Type.GetTypeCode(type) == System.TypeCode.Int32) return (FlatGeobuf.ColumnType.Int);
            if (System.Type.GetTypeCode(type) == System.TypeCode.Int64) return (FlatGeobuf.ColumnType.Long);
            if (System.Type.GetTypeCode(type) == System.TypeCode.Double) return (FlatGeobuf.ColumnType.Double);
            if (System.Type.GetTypeCode(type) == System.TypeCode.String) return (FlatGeobuf.ColumnType.String);

            throw new System.ApplicationException("Unknown type");
        }

        private byte[] SerializeVec()
        {
            if (IntVecData == null) return (null);

            NetTopologySuite.Features.FeatureCollection MyFC = FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(IntVecData);
            byte[] retVal = IntSerialize(MyFC, FlatGeobuf.GeometryType.Unknown);

            return (retVal);
        }

        private byte[] IntSerialize(NetTopologySuite.Features.FeatureCollection fc, FlatGeobuf.GeometryType geometryType, byte dimensions = 2, System.Collections.Generic.IList<FlatGeobuf.NTS.ColumnMeta> columns = null)
        {
            var featureFirst = fc.First();
            if (columns == null && featureFirst.Attributes != null)
            {
                columns = featureFirst.Attributes.GetNames()
                          .Select(n => new FlatGeobuf.NTS.ColumnMeta() { Name = n, Type = ToColumnType(featureFirst.Attributes.GetType(n)) })
                          .ToList();
            }
            var memoryStream = new System.IO.MemoryStream();
            IntSerialize(memoryStream, fc, geometryType, dimensions, columns);
            return (memoryStream.ToArray());
        }

        private void IntSerialize(System.IO.Stream output, System.Collections.Generic.IEnumerable<NetTopologySuite.Features.IFeature> features, FlatGeobuf.GeometryType geometryType, byte dimensions = 2, System.Collections.Generic.IList<FlatGeobuf.NTS.ColumnMeta> columns = null)
        {
            Nito.AsyncEx.AsyncContext.Run(async () => await IntSerializeAsync(output, features, geometryType, dimensions, columns));
        }

        private async System.Threading.Tasks.Task IntSerializeAsync(System.IO.Stream output, System.Collections.Generic.IEnumerable<NetTopologySuite.Features.IFeature> features, FlatGeobuf.GeometryType geometryType, byte dimensions = 2, System.Collections.Generic.IList<FlatGeobuf.NTS.ColumnMeta> columns = null)
        {
            await output.WriteAsync(FlatGeobuf.Constants.MagicBytes, 0, FlatGeobuf.Constants.MagicBytes.Length);
            var headerBuffer = IntBuildHeader(0, geometryType, dimensions, columns, null);
            var bytes = headerBuffer.ToSizedArray();
            await output.WriteAsync(bytes, 0, bytes.Length);
            headerBuffer.Position = headerBuffer.Position + 4;
            var header = FlatGeobuf.Header.GetRootAsHeader(headerBuffer).UnPack();
            foreach (var feature in features)
            {
                var buffer = FlatGeobuf.NTS.FeatureConversions.ToByteBuffer(feature, header);
                bytes = buffer.ToSizedArray();
                await output.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private FlatBuffers.ByteBuffer IntBuildHeader(ulong count, FlatGeobuf.GeometryType geometryType, byte dimensions, System.Collections.Generic.IList<FlatGeobuf.NTS.ColumnMeta> columns, FlatGeobuf.Index.PackedRTree index)
        {
            string WKTString;
            IntSRS.ExportToWkt(out WKTString, null);
            string WKTStart = null;
            string CRSName = "";
            if (WKTString.Contains("["))
            {
                WKTStart = WKTString.Substring(0, WKTString.IndexOf("["));
                CRSName = IntSRS.GetAttrValue(WKTStart, 0);
            }
            string Organization = IntSRS.GetAuthorityName(null);
            int Code = System.Convert.ToInt32(IntSRS.GetAuthorityCode(null));

            var builder = new FlatBuffers.FlatBufferBuilder(1024);

            FlatBuffers.Offset<FlatGeobuf.Crs> MyCRS = FlatGeobuf.Crs.CreateCrs(
                                                       builder,
                                                       builder.CreateString(Organization),
                                                       Code,
                                                       builder.CreateString(CRSName),
                                                       builder.CreateString(""),
                                                       builder.CreateString(WKTString),
                                                       builder.CreateString(Code.ToString()));

            FlatBuffers.StringOffset NameO = builder.CreateString("");
            if (IntName != null) NameO = builder.CreateString(IntName);
            FlatBuffers.StringOffset DescO = builder.CreateString("");
            if (IntDescription != null) DescO = builder.CreateString(IntDescription);

            FlatBuffers.VectorOffset? columnsOffset = null;
            if (columns != null)
            {
                var columnsArray = columns
                                   .Select(c => FlatGeobuf.Column.CreateColumn(builder, builder.CreateString(c.Name), c.Type))
                                   .ToArray();
                columnsOffset = FlatGeobuf.Header.CreateColumnsVector(builder, columnsArray);
            }

            FlatGeobuf.Header.StartHeader(builder);

            FlatGeobuf.Header.AddCrs(builder, MyCRS);
            FlatGeobuf.Header.AddGeometryType(builder, geometryType);
            if (IntName != null) FlatGeobuf.Header.AddName(builder, NameO);
            if (IntDescription != null) FlatGeobuf.Header.AddDescription(builder, DescO);
            if (dimensions == 3) FlatGeobuf.Header.AddHasZ(builder, true);
            if (dimensions == 4) FlatGeobuf.Header.AddHasM(builder, true);
            if (columnsOffset.HasValue) FlatGeobuf.Header.AddColumns(builder, columnsOffset.Value);
            if (index != null)
            {
                FlatGeobuf.Header.AddIndexNodeSize(builder, 16);
            }
            else
            {
                FlatGeobuf.Header.AddIndexNodeSize(builder, 0);
            }

            FlatGeobuf.Header.AddFeaturesCount(builder, count);
            var offset = FlatGeobuf.Header.EndHeader(builder);

            builder.FinishSizePrefixed(offset.Value);

            return (builder.DataBuffer);
        }

        // Static methods ------------------------------------------------------------------

        // Static helper that initializes GDAL.
        public static void InitGDAL()
        {
            MaxRev.Gdal.Core.GdalBase.ConfigureAll();
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            OSGeo.GDAL.Gdal.SetConfigOption("PROJ_DEBUG", "5");
        }

        // Methods -------------------------------------------------------------------------

        public string ToString(bool GeoJSON = false)
        {
            string MyString = null;

            if (IntVecData != null)
            {
                if (GeoJSON)
                {
                    MyString = GetAsGeoJSON();
                }
                else
                {
                    NetTopologySuite.Features.FeatureCollection MyFC = FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(IntVecData);
                    MyString = "Number of features: " + MyFC.Count + ". To obtain the data as a GeoJSON string, use \".ToString(true)\" or method \".GetAsGeoJSON()\" directly.";
                }
            }

            return (MyString);
        }

        // Saves the data natively as a FlatGeobuf file.
        public void SaveAsFGB(string FileName)
        {
            IntBusy = true;
            byte[] FGB = SerializeVec();
            System.IO.File.WriteAllBytes(FileName, FGB);
            IntBusy = false;
        }
    }
}