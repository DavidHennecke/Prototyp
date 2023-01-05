namespace Prototyp.Elements
{
    public class RasterData
    {
        /***********************************************************************************

        Class RasterData
        Contains properties and methods for raster data handling.

        The class handles GDAL raster data. The class also contains methods to sending
        and receiving vector data via gRPC.
            ???

        (c) 2022 by Carsten Croonenbroeck, Markus Berger, and David Hennecke. Contact us at
        carsten.croonenbroeck@uni-rostock.de.

            Add license information here.

        Dependencies (NuGet packages):
        - MaxRev.Gdal.Core
        - MaxRev.Gdal.WindowsRuntime.Minimal

        *///////////////////////////////////////////////////////////////////////////////////

        // Internal variables --------------------------------------------------------------

        public struct sBand
        {
            public double[] RasterValues;
        }
        private sBand[] _bands;
        private bool _busy;
        private string _fileType;
        private OSGeo.OSR.SpatialReference _SRS;
        private string _WKT_SRS;
        private int _rasterXSize;
        private int _rasterYSize;
        private int _rasterCount;
        private double _NA;
        private double[] _geoTransform;
        private int _ID = 0;
        private string _name;
        private string _filename;

        // Getters and setters -------------------------------------------------------------

        public OSGeo.GDAL.Dataset GDALDataSet
        {
            get { return (ExportDataset()); }
            set { ImportDataset(value); } // Warning: Setter will always import the entire raster data. To import only a rectangle, use 'ImportDataset' method directly.
        }

        public sBand[] RasterBands
        {
            get { return (_bands); }
            set { _bands = value; }
        }

        public bool Busy
        {
            get { return (_busy); }
        }

        public string FileType
        {
            get { return (_fileType); }
            set { _fileType = value; }
        }

        public string SpatialReference_WKT
        {
            get { return (_WKT_SRS); }
            set { _WKT_SRS = value; }
        }
        public OSGeo.OSR.SpatialReference SpatialReference
        {
            get { return (_SRS); }
            set { _SRS = value; }
        }

        public int RasterXSize
        {
            get { return (_rasterXSize); }
            set { _rasterXSize = value; }
        }

        public int RasterYSize
        {
            get { return (_rasterYSize); }
            set { _rasterYSize = value; }
        }

        public int RasterCount
        {
            get { return (_rasterCount); }
            set { _rasterCount = value; }
        }

        public double NACode
        {
            get { return (_NA); }
            set { _NA = value; }
        }

        public double[] GeoTransform
        {
            get { return (_geoTransform); }
            set { if (value.Length == 6) _geoTransform = value; }
        }

        public int ID
        {
            get { return (_ID); }
        }

        public string Name
        {
            get { return (_name); }
            set { _name = value; }
        }

        public string FileName
        {
            get { return (_filename); }
            set { _filename = value; }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public RasterData()
        {

        }

        // Constructor utilizing only the mere ID.
        public RasterData(int uid)
        {
            SetID(uid);
        }

        // Constructor that accepts a string and decides what to do internally.
        // First, it is assumed that the string contains a file name.
        // If that's not it, a Base64 coded byte array string is assumed.
        // Example:
        // RasterData rasterData = new RasterData(1, "C:/Temp/WindData.asc");
        // RasterData rasterData = new RasterData(1, MyByteArrayString);
        public RasterData(int uid, string MyString)
        {
            if (System.IO.File.Exists(MyString))
            {
                if (!VectorData.FileAccessable(MyString)) { throw new System.Exception("File is not accessible, maybe opened in some other software?"); }

                _busy = true;
                InitGDAL();
                OSGeo.GDAL.Dataset DS = OSGeo.GDAL.Gdal.Open(MyString, OSGeo.GDAL.Access.GA_ReadOnly);
                ImportDataset(DS, 0, 0, DS.RasterXSize, DS.RasterYSize);
                SetID(uid);
                _name = GetName(MyString);
                _filename = MyString;
                _busy = false;
            }
            else
            {
                _busy = true;
                Deserialize(System.Convert.FromBase64String(MyString));
                SetID(uid);
                _busy = false;
            }
        }

        // Constructor that opens a rectangle from a file.
        // Example:
        // RasterData rasterData = new RasterData(1, "C:/Temp/WindData.asc", 100, 100, 200, 200);
        public RasterData(int uid, string RasterFileName, int xOff, int yOff, int xSize, int ySize)
        {
            if (System.IO.File.Exists(RasterFileName))
            {
                if (!VectorData.FileAccessable(RasterFileName)) { throw new System.Exception("File is not accessible, maybe opened in some other software?"); }

                _busy = true;
                InitGDAL();
                OSGeo.GDAL.Dataset DS = OSGeo.GDAL.Gdal.Open(RasterFileName, OSGeo.GDAL.Access.GA_ReadOnly);
                ImportDataset(DS, xOff, yOff, xSize, ySize);
                SetID(uid);
                _name = GetName(RasterFileName);
                _filename = RasterFileName;
            }
            else
            {
                _busy = false;
                throw new System.Exception("File does not exist.");
            }
            _busy = false;
        }

        // Constructor that accepts a pre-loaded GDAL dataset object.
        public RasterData(int uid, OSGeo.GDAL.Dataset DS)
        {
            _busy = true;
            ImportDataset(DS, 0, 0, DS.RasterXSize, DS.RasterYSize);
            SetID(uid);
            _busy = false;
        }

        // Constructor that deserializes a byte array representation of raster data.
        // Example:
        // RasterData rasterData = new RasterData(1, SomeByteArray);
        public RasterData(int uid, byte[] SerializedData)
        {
            Deserialize(SerializedData);
            SetID(uid);
        }

        // Private methods -----------------------------------------------------------------

        // Make ID.
        private void SetID(int uid)
        {
            _ID = uid;
        }

        private string GetName(string FileName)
        {
            string[] TempNameArr = FileName.Split("\\");
            string TempName = TempNameArr[TempNameArr.Length - 1];
            TempNameArr = TempName.Split(".");
            TempName = TempNameArr[0];

            return (TempName);
        }

        private OSGeo.GDAL.Dataset ExportDataset()
        {
            _busy = true;

            string RandomFilename = System.IO.Path.GetRandomFileName();

            OSGeo.GDAL.Driver MyDriver;
            if (CreateCapByFileType()) MyDriver = OSGeo.GDAL.Gdal.GetDriverByName(_fileType); else MyDriver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff");

            OSGeo.GDAL.Dataset MyDataSet;
            RandomFilename = System.IO.Path.GetRandomFileName();
            MyDataSet = MyDriver.Create("/vsimem/" + RandomFilename, _rasterXSize, _rasterYSize, _rasterCount, OSGeo.GDAL.DataType.GDT_Float64, null);
            MyDataSet = PutData(MyDataSet);

            _busy = false;
            return (MyDataSet);
        }

        private OSGeo.GDAL.Dataset PutData(OSGeo.GDAL.Dataset MyDataSet)
        {
            MyDataSet.SetProjection(_WKT_SRS);
            for (int i = 0; i < _rasterCount; i++)
            {
                MyDataSet.GetRasterBand(i + 1).WriteRaster(0, 0, _rasterXSize, _rasterYSize, _bands[i].RasterValues, _rasterXSize, _rasterYSize, 0, 0);
                MyDataSet.GetRasterBand(i + 1).SetNoDataValue(_NA);
            }
            MyDataSet.GetDriver().SetDescription(_fileType);
            MyDataSet.SetGeoTransform(_geoTransform);

            return (MyDataSet);
        }

        private double[] ByteArr2DoubleArr(byte[] ByteData)
        {
            int n = ByteData.Length / sizeof(double);
            double[] RetVal = new double[n];
            int DoubleCount;

            for (DoubleCount = 0; DoubleCount < n; DoubleCount++)
            {
                RetVal[DoubleCount] = System.BitConverter.ToDouble(ByteData, DoubleCount * sizeof(double));
            }

            return (RetVal);
        }

        private byte[] DoubleArr2ByteArr(double[] DoubleData)
        {
            byte[] TempByteArr;
            int n = DoubleData.Length;
            byte[] RetVal = new byte[n * sizeof(double)]; ;
            int DoubleCount;

            for (DoubleCount = 0; DoubleCount < n; DoubleCount++)
            {
                TempByteArr = System.BitConverter.GetBytes(DoubleData[DoubleCount]);
                for (int i = 0; i < sizeof(double); i++) RetVal[(DoubleCount * sizeof(double)) + i] = TempByteArr[i];
            }

            return (RetVal);
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

        public static string FileType2FileExtension(string FileType)
        {
            if (FileType == null | FileType == "") return (null);

            InitGDAL();

            OSGeo.GDAL.Driver MyDriver;

            for (int i = 0; i < OSGeo.GDAL.Gdal.GetDriverCount(); i++)
            {
                MyDriver = OSGeo.GDAL.Gdal.GetDriver(i);
                if (MyDriver.GetDescription() == FileType)
                {
                    return (MyDriver.GetMetadataItem(OSGeo.GDAL.GdalConst.GDAL_DMD_EXTENSION, null));
                }
            }


            return (null);
        }

        public static bool CreateCapByFileType(string FileType)
        {
            InitGDAL();

            OSGeo.GDAL.Driver MyDrv = OSGeo.GDAL.Gdal.GetDriverByName(FileType);
            string[] Meta = MyDrv.GetMetadata(null);
            for (int i = 0; i < Meta.Length; i++) if (Meta[i] == "DCAP_CREATE=YES") return (true);

            return (false);
        }

        // Methods -------------------------------------------------------------------------

        public void Deserialize(byte[] SerializedData)
        {
            _busy = true;
            int HeaderSize = (6 * sizeof(int)) + sizeof(double);
            int BandSize;

            int NumBands = System.BitConverter.ToInt32(SerializedData, 0 * sizeof(int));
            int NumBytesFileType = System.BitConverter.ToInt32(SerializedData, 1 * sizeof(int));
            int NumBytesWKT = System.BitConverter.ToInt32(SerializedData, 2 * sizeof(int));
            int NumBytesName = System.BitConverter.ToInt32(SerializedData, 3 * sizeof(int));
            int XSize = System.BitConverter.ToInt32(SerializedData, 4 * sizeof(int));
            int YSize = System.BitConverter.ToInt32(SerializedData, 5 * sizeof(int));
            double NA = System.BitConverter.ToDouble(SerializedData, 6 * sizeof(int));
            string FileType = System.Text.Encoding.UTF8.GetString(SerializedData, HeaderSize, NumBytesFileType);
            string WKT = System.Text.Encoding.UTF8.GetString(SerializedData, HeaderSize + NumBytesFileType, NumBytesWKT);
            string Name = System.Text.Encoding.UTF8.GetString(SerializedData, HeaderSize + NumBytesFileType + NumBytesWKT, NumBytesName);

            if (NumBands <= 0) throw new System.ArgumentException("Data seem to be inconsistent.");
            if (NumBytesFileType < 0) throw new System.ArgumentException("Data seem to be inconsistent.");
            if (NumBytesWKT < 0) throw new System.ArgumentException("Data seem to be inconsistent.");
            if (NumBytesName < 0) throw new System.ArgumentException("Data seem to be inconsistent.");
            if (XSize <= 0) throw new System.ArgumentException("Data seem to be inconsistent.");
            if (YSize <= 0) throw new System.ArgumentException("Data seem to be inconsistent.");

            _fileType = FileType;
            _WKT_SRS = WKT;
            _name = Name;
            _rasterXSize = XSize;
            _rasterYSize = YSize;
            _rasterCount = NumBands;
            _NA = NA;
            _bands = new sBand[NumBands];

            BandSize = XSize * YSize;
            HeaderSize = HeaderSize + NumBytesFileType + NumBytesWKT + NumBytesName;
            byte[] TempArr;
            TempArr = new byte[6 * sizeof(double)];
            for (int i = 0; i < TempArr.Length; i++) TempArr[i] = SerializedData[HeaderSize + i];
            _geoTransform = ByteArr2DoubleArr(TempArr);

            HeaderSize = HeaderSize + 6 * (sizeof(double));
            for (int Band = 0; Band < NumBands; Band++)
            {
                TempArr = new byte[BandSize * sizeof(double)];
                for (int i = 0; i < TempArr.Length; i++) TempArr[i] = SerializedData[HeaderSize + (Band * TempArr.Length) + i];
                _bands[Band].RasterValues = ByteArr2DoubleArr(TempArr);
            }

            _busy = false;
        }

        public byte[] Serialize()
        {
            _busy = true;

            int Offset = 0;
            byte[] TempArr;
            int j = 0;
            int BandSize = _rasterXSize * _rasterYSize;

            byte[] FileTypeBytes = System.Text.Encoding.UTF8.GetBytes(_fileType);
            byte[] WKTStringBytes = System.Text.Encoding.UTF8.GetBytes(_WKT_SRS);
            byte[] NameBytes = System.Text.Encoding.UTF8.GetBytes(_name);
            byte[] GeoTransformBytes;
            byte[] Result = new byte[(6 * sizeof(int)) + (7 * sizeof(double)) + FileTypeBytes.Length + WKTStringBytes.Length + NameBytes.Length + (sizeof(double) * BandSize * _rasterCount)];

            TempArr = System.BitConverter.GetBytes(_rasterCount);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(FileTypeBytes.Length);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(WKTStringBytes.Length);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(NameBytes.Length);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(_rasterXSize);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(_rasterYSize);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(_NA);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];

            Offset = (6 * sizeof(int)) + sizeof(double);
            for (int i = Offset; i < Offset + FileTypeBytes.Length; i++)
            {
                Result[i] = FileTypeBytes[j];
                j++;
            }
            Offset = Offset + FileTypeBytes.Length;
            j = 0;
            for (int i = Offset; i < Offset + WKTStringBytes.Length; i++)
            {
                Result[i] = WKTStringBytes[j];
                j++;
            }
            Offset = Offset + WKTStringBytes.Length;
            j = 0;
            for (int i = Offset; i < Offset + NameBytes.Length; i++)
            {
                Result[i] = NameBytes[j];
                j++;
            }
            Offset = Offset + NameBytes.Length;

            GeoTransformBytes = DoubleArr2ByteArr(_geoTransform);
            j = 0;
            for (int i = Offset; i < Offset + (6 * sizeof(double)); i++)
            {
                Result[i] = GeoTransformBytes[j];
                j++;
            }

            Offset = Offset + (6 * sizeof(double));
            for (int i = 0; i < _rasterCount; i++)
            {
                TempArr = DoubleArr2ByteArr(_bands[i].RasterValues);
                for (j = 0; j < TempArr.Length; j++) Result[Offset + (i * TempArr.Length) + j] = TempArr[j];
            }

            _busy = false;
            return (Result);
        }

        public bool isNorth()
        {
            _busy = true;
            InitGDAL();

            double centerY = (_geoTransform[3]);
            _busy = false;
            if (centerY < 0)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        public int getUTMZone()
        {
            _busy = true;
            InitGDAL();

            int Zone = 0;

            double centerX = (_geoTransform[0]);

            if (centerX >= 0)
            {
                int coordPrev = 0;
                Zone = 31;
                for (int coord = 6; coord >= 0 && coord <= 180; coord = coord + 6)
                {
                    if (coordPrev <= centerX && centerX <= coord)
                    {
                        break;
                    }
                    Zone++;
                }
            }
            else
            {
                int coordPrev = -180;
                Zone = 1;
                for (int coord = -174; coord >= -180 && coord < 0; coord = coord + 6)
                {
                    if (coordPrev <= centerX && centerX <= coord)
                    {
                        break;
                    }
                    Zone++;
                }
            }

            _busy = false;
            return (Zone);
        }

        public void TransformToWGS84() 
        {
            ProjectTo(4326);
        }

        public void ProjectToWGS84UTM()
        {
            int EPSG;

            if (isNorth())
            {
                EPSG = 32600;
                EPSG = EPSG + getUTMZone();
            }
            else
            {
                EPSG = 32700;
                EPSG = EPSG + getUTMZone();
            }

            ProjectTo(EPSG);
        }

        public static int ProgressFunc(double Complete, System.IntPtr Message, System.IntPtr Data)
        {
            System.Console.Write("Processing ... " + Complete * 100 + "% Completed.");
            if (Message != System.IntPtr.Zero)
                System.Console.Write(" Message:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Message));
            if (Data != System.IntPtr.Zero)
                System.Console.Write(" Data:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Data));

            System.Console.WriteLine("");
            return (1);
        }

        public void ProjectTo(int EPSG)
        {
            OSGeo.OSR.SpatialReference FromSRS = new OSGeo.OSR.SpatialReference(null);
            FromSRS.ImportFromEPSG(System.Int32.Parse(SpatialReference.GetAttrValue("AUTHORITY", 1)));
            string FromSRS_wkt;
            FromSRS.ExportToWkt(out FromSRS_wkt, null);

            OSGeo.OSR.SpatialReference ToSRS= new OSGeo.OSR.SpatialReference(null);
            ToSRS.ImportFromEPSG(EPSG);
            string ToSRS_wkt;
            ToSRS.ExportToWkt(out ToSRS_wkt, null);

            try
            {
                OSGeo.GDAL.Driver MyDriver = OSGeo.GDAL.Gdal.GetDriverByName(_fileType);
                string RandomFilename = System.IO.Path.GetRandomFileName();
                OSGeo.GDAL.Dataset vrtDataset;
                string[] options = new string[] {"-of", "GTiff"};

                vrtDataset = MyDriver.Create("/vsimem/" + RandomFilename, _rasterXSize, _rasterYSize, _rasterCount, OSGeo.GDAL.DataType.GDT_Float64, null);
                vrtDataset = OSGeo.GDAL.Gdal.AutoCreateWarpedVRT(GDALDataSet, FromSRS_wkt, ToSRS_wkt, OSGeo.GDAL.ResampleAlg.GRA_NearestNeighbour, 0);
                RandomFilename = System.IO.Path.GetRandomFileName();
                GDALDataSet = OSGeo.GDAL.Gdal.wrapper_GDALTranslate("/vsimem/" + RandomFilename, vrtDataset, new OSGeo.GDAL.GDALTranslateOptions(options), new OSGeo.GDAL.Gdal.GDALProgressFuncDelegate(ProgressFunc), "Sample Data");
                vrtDataset.Dispose();
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }
        }
        // Imports a GDAL raster dataset.
        public void ImportDataset(OSGeo.GDAL.Dataset DataSet)
        {
            _busy = true;
            
            ImportDataset(DataSet, 0, 0, DataSet.RasterXSize, DataSet.RasterYSize);
            _busy = false;
        }

        // Imports a rectangle of data from a GDAL raster dataset.
        public void ImportDataset(OSGeo.GDAL.Dataset DataSet, int xOff, int yOff, int xSize, int ySize)
        {
            int HasVal;

            _busy = true;

            _fileType = DataSet.GetDriver().GetDescription();
            _bands = new sBand[DataSet.RasterCount];
            _SRS = DataSet.GetSpatialRef();
            _WKT_SRS = DataSet.GetProjection();
            _rasterXSize = xSize;
            _rasterYSize = ySize;
            _rasterCount = DataSet.RasterCount;
            _geoTransform = new double[6];
            DataSet.GetGeoTransform(_geoTransform);

            for (int i = 1; i <= _bands.Length; i++)
            {
                DataSet.GetRasterBand(i).GetNoDataValue(out _NA, out HasVal);
                _bands[i - 1].RasterValues = new double[_rasterXSize * _rasterYSize];
                DataSet.GetRasterBand(i).ReadRaster(xOff,
                                                    yOff,
                                                    _rasterXSize,
                                                    _rasterYSize,
                                                    _bands[i - 1].RasterValues,
                                                    _rasterXSize,
                                                    _rasterYSize,
                                                    0,
                                                    0);
            }

            _busy = false;
        }

        public bool CreateCapByFileType()
        {
            return (CreateCapByFileType(_fileType));
        }

        public bool SaveToDisk(string FileName)
        {
            return (SaveToDisk(FileName, _fileType));
        }

        public bool SaveToDisk(string FileName, string Driver)
        {
            if (!CreateCapByFileType(Driver)) return (false);

            _busy = true;

            InitGDAL();

            OSGeo.GDAL.Driver MyDriver = OSGeo.GDAL.Gdal.GetDriverByName(Driver);
            OSGeo.GDAL.Dataset MyDataSet;

            MyDataSet = MyDriver.Create(FileName, _rasterXSize, _rasterYSize, _rasterCount, OSGeo.GDAL.DataType.GDT_Float64, null);
            MyDataSet = PutData(MyDataSet);

            MyDataSet.Dispose();

            _busy = false;
            return (true);
        }

        // Computes the minimum of values for a given band.
        public double Min(int BandIndex)
        {
            if (_bands.Length == 0) return (0);

            double ThisMin = 0;
            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    ThisMin = _bands[BandIndex].RasterValues[i];
                    break;
                }
            }

            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    if (_bands[BandIndex].RasterValues[i] <= ThisMin) ThisMin = _bands[BandIndex].RasterValues[i];
                }
            }

            return (ThisMin);
        }

        // Computes the maximum of values for a given band.
        public double Max(int BandIndex)
        {
            if (_bands.Length == 0) return (0);

            double ThisMax = 0;
            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    ThisMax = _bands[BandIndex].RasterValues[i];
                    break;
                }
            }

            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    if (_bands[BandIndex].RasterValues[i] >= ThisMax) ThisMax = _bands[BandIndex].RasterValues[i];
                }
            }

            return (ThisMax);
        }

        // Computes the mean of values for a given band.
        public double Mean(int BandIndex)
        {
            double Sum = 0;
            int NumVals = 0;

            if (_bands.Length == 0) return (0);

            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    Sum = Sum + _bands[BandIndex].RasterValues[i];
                    NumVals++;
                }
            }

            return (Sum / NumVals);
        }

        // Computes the standard deviation of values for a given band.
        public double StdDev(int BandIndex)
        {
            double Sum = 0;
            int NumVals = 0;
            double MyMean = Mean(BandIndex);

            if (_bands.Length == 0) return (0);

            for (int i = 0; i < _bands[BandIndex].RasterValues.Length; i++)
            {
                if (_bands[BandIndex].RasterValues[i] != _NA)
                {
                    Sum = Sum + System.Math.Pow(_bands[BandIndex].RasterValues[i] - MyMean, 2);
                    NumVals++;
                }
            }

            return (System.Math.Sqrt(Sum / (NumVals - 1)));
        }

        public override string ToString()
        {
            return (System.Convert.ToBase64String(this.Serialize()));
        }
    }
}
