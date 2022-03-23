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

        (c) 2022 by Carsten Croonenbroeck, Markus Berger and David Hennecke. Contact us at
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
        private sBand[] IntBands;
        private bool IntBusy;
        private string IntFileType;
        private string IntWKT_SRS;
        private int IntRasterXSize;
        private int IntRasterYSize;
        private int IntRasterCount;
        private double IntNA;
        private double[] IntGeoTransform;
        private double IntID = 0.0;
        private string IntName;

        // Getters and setters -------------------------------------------------------------

        public OSGeo.GDAL.Dataset GDALDataSet
        {
            get { return (ExportDataset()); }
            set { ImportDataset(value); } // Warning: Setter will always import the entire raster data. To import only a rectangle, use 'ImportDataset' method directly.
        }

        public sBand[] RasterBands
        {
            get { return (IntBands); }
            set { IntBands = value; }
        }

        public bool Busy
        {
            get { return (IntBusy); }
        }

        public string FileType
        {
            get { return (IntFileType); }
            set { IntFileType = value; }
        }

        public string SpatialReference_WKT
        {
            get { return (IntWKT_SRS); }
            set { IntWKT_SRS = value; }
        }

        public int RasterXSize
        {
            get { return (IntRasterXSize); }
            set { IntRasterXSize = value; }
        }

        public int RasterYSize
        {
            get { return (IntRasterYSize); }
            set { IntRasterYSize = value; }
        }

        public int RasterCount
        {
            get { return (IntRasterCount); }
            set { IntRasterCount = value; }
        }

        public double NACode
        {
            get { return (IntNA); }
            set { IntNA = value; }
        }

        public double[] GeoTransform
        {
            get { return (IntGeoTransform); }
            set { if (value.Length == 6) IntGeoTransform = value; }
        }

        public double ID
        {
            get { return (IntID); }
        }

        public string Name
        {
            get { return (IntName); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public RasterData()
        {
            MakeID();
        }

        // Constructor that opens an entire file.
        // Example:
        // RasterData rasterData = new RasterData("C:/Temp/WindData.asc");
        public RasterData(string RasterFileName)
        {
            if (System.IO.File.Exists(RasterFileName))
            {
                IntBusy = true;
                InitGDAL();
                OSGeo.GDAL.Dataset DS = OSGeo.GDAL.Gdal.Open(RasterFileName, OSGeo.GDAL.Access.GA_ReadOnly);
                ImportDataset(DS, 0, 0, DS.RasterXSize, DS.RasterYSize);
                MakeID();
                IntName = GetName(RasterFileName);
            }
            else
            {
                IntBusy = false;
                throw new System.Exception("File does not exist.");
            }
            IntBusy = false;
        }

        // Constructor that opens a rectangle from a file.
        // Example:
        // RasterData rasterData = new RasterData("C:/Temp/WindData.asc", 100, 100, 200, 200);
        public RasterData(string RasterFileName, int xOff, int yOff, int xSize, int ySize)
        {
            if (System.IO.File.Exists(RasterFileName))
            {
                IntBusy = true;
                InitGDAL();
                OSGeo.GDAL.Dataset DS = OSGeo.GDAL.Gdal.Open(RasterFileName, OSGeo.GDAL.Access.GA_ReadOnly);
                ImportDataset(DS, xOff, yOff, xSize, ySize);
                MakeID();
                IntName = GetName(RasterFileName);
            }
            else
            {
                IntBusy = false;
                throw new System.Exception("File does not exist.");
            }
            IntBusy = false;
        }

        // Constructor that accepts a pre-loaded GDAL dataset object.
        public RasterData(OSGeo.GDAL.Dataset DS)
        {
            IntBusy = true;
            ImportDataset(DS, 0, 0, DS.RasterXSize, DS.RasterYSize);
            MakeID();
            IntBusy = false;
        }

        // Constructor that deserializes a byte array representation of raster data.
        // Example:
        // RasterData rasterData = new RasterData(SomeByteArray);
        public RasterData(byte[] SerializedData)
        {
            Deserialize(SerializedData);
            MakeID();
        }

        // Private methods -----------------------------------------------------------------

        // Make ID.
        private void MakeID()
        {
            System.Random rnd = new System.Random();
            IntID = rnd.NextDouble();
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
            IntBusy = true;

            OSGeo.GDAL.Driver MyDriver;
            if (CreateCapByFileType()) MyDriver = OSGeo.GDAL.Gdal.GetDriverByName(IntFileType); else MyDriver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff");

            OSGeo.GDAL.Dataset MyDataSet;
            MyDataSet = MyDriver.Create("/vsimem/Temporary", IntRasterXSize, IntRasterYSize, IntRasterCount, OSGeo.GDAL.DataType.GDT_Float64, null);
            MyDataSet = PutData(MyDataSet);

            IntBusy = false;
            return (MyDataSet);
        }

        private OSGeo.GDAL.Dataset PutData(OSGeo.GDAL.Dataset MyDataSet)
        {
            MyDataSet.SetProjection(IntWKT_SRS);
            for (int i = 0; i < IntRasterCount; i++)
            {
                MyDataSet.GetRasterBand(i + 1).WriteRaster(0, 0, IntRasterXSize, IntRasterYSize, IntBands[i].RasterValues, IntRasterXSize, IntRasterYSize, 0, 0);
                MyDataSet.GetRasterBand(i + 1).SetNoDataValue(IntNA);
            }
            MyDataSet.GetDriver().SetDescription(IntFileType);
            MyDataSet.SetGeoTransform(IntGeoTransform);

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
            IntBusy = true;
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

            IntFileType = FileType;
            IntWKT_SRS = WKT;
            IntName = Name;
            IntRasterXSize = XSize;
            IntRasterYSize = YSize;
            IntRasterCount = NumBands;
            IntNA = NA;
            IntBands = new sBand[NumBands];

            BandSize = XSize * YSize;
            HeaderSize = HeaderSize + NumBytesFileType + NumBytesWKT + NumBytesName;
            byte[] TempArr;
            TempArr = new byte[6 * sizeof(double)];
            for (int i = 0; i < TempArr.Length; i++) TempArr[i] = SerializedData[HeaderSize + i];
            IntGeoTransform = ByteArr2DoubleArr(TempArr);

            HeaderSize = HeaderSize + 6 * (sizeof(double));
            for (int Band = 0; Band < NumBands; Band++)
            {
                TempArr = new byte[BandSize * sizeof(double)];
                for (int i = 0; i < TempArr.Length; i++) TempArr[i] = SerializedData[HeaderSize + (Band * TempArr.Length) + i];
                IntBands[Band].RasterValues = ByteArr2DoubleArr(TempArr);
            }

            IntBusy = false;
        }

        public byte[] Serialize()
        {
            IntBusy = true;

            int Offset = 0;
            byte[] TempArr;
            int j = 0;
            int BandSize = IntRasterXSize * IntRasterYSize;

            byte[] FileTypeBytes = System.Text.Encoding.UTF8.GetBytes(IntFileType);
            byte[] WKTStringBytes = System.Text.Encoding.UTF8.GetBytes(IntWKT_SRS);
            byte[] NameBytes = System.Text.Encoding.UTF8.GetBytes(IntName);
            byte[] GeoTransformBytes;
            byte[] Result = new byte[(6 * sizeof(int)) + (7 * sizeof(double)) + FileTypeBytes.Length + WKTStringBytes.Length + NameBytes.Length + (sizeof(double) * BandSize * IntRasterCount)];

            TempArr = System.BitConverter.GetBytes(IntRasterCount);
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

            TempArr = System.BitConverter.GetBytes(IntRasterXSize);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(IntRasterYSize);
            for (int i = 0; i < TempArr.Length; i++) Result[(Offset * sizeof(int)) + i] = TempArr[i];
            Offset++;

            TempArr = System.BitConverter.GetBytes(IntNA);
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

            GeoTransformBytes = DoubleArr2ByteArr(IntGeoTransform);
            j = 0;
            for (int i = Offset; i < Offset + (6 * sizeof(double)); i++)
            {
                Result[i] = GeoTransformBytes[j];
                j++;
            }

            Offset = Offset + (6 * sizeof(double));
            for (int i = 0; i < IntRasterCount; i++)
            {
                TempArr = DoubleArr2ByteArr(IntBands[i].RasterValues);
                for (j = 0; j < TempArr.Length; j++) Result[Offset + (i * TempArr.Length) + j] = TempArr[j];
            }

            IntBusy = false;
            return (Result);
        }

        // Imports a GDAL raster dataset.
        public void ImportDataset(OSGeo.GDAL.Dataset DataSet)
        {
            IntBusy = true;
            ImportDataset(DataSet, 0, 0, DataSet.RasterXSize, DataSet.RasterYSize);
            IntBusy = false;
        }

        // Imports a rectangle of data from a GDAL raster dataset.
        public void ImportDataset(OSGeo.GDAL.Dataset DataSet, int xOff, int yOff, int xSize, int ySize)
        {
            int HasVal;

            IntBusy = true;

            IntFileType = DataSet.GetDriver().GetDescription();
            IntBands = new sBand[DataSet.RasterCount];
            IntWKT_SRS = DataSet.GetProjection();
            IntRasterXSize = xSize;
            IntRasterYSize = ySize;
            IntRasterCount = DataSet.RasterCount;
            IntGeoTransform = new double[6];
            DataSet.GetGeoTransform(IntGeoTransform);

            for (int i = 1; i <= IntBands.Length; i++)
            {
                DataSet.GetRasterBand(i).GetNoDataValue(out IntNA, out HasVal);
                IntBands[i - 1].RasterValues = new double[IntRasterXSize * IntRasterYSize];
                DataSet.GetRasterBand(i).ReadRaster(xOff,
                                                    yOff,
                                                    IntRasterXSize,
                                                    IntRasterYSize,
                                                    IntBands[i - 1].RasterValues,
                                                    IntRasterXSize,
                                                    IntRasterYSize,
                                                    0,
                                                    0);
            }

            IntBusy = false;
        }

        public bool CreateCapByFileType()
        {
            return (CreateCapByFileType(IntFileType));
        }

        public bool SaveToDisk(string FileName)
        {
            return (SaveToDisk(FileName, IntFileType));
        }

        public bool SaveToDisk(string FileName, string Driver)
        {
            if (!CreateCapByFileType(Driver)) return (false);

            IntBusy = true;

            InitGDAL();

            OSGeo.GDAL.Driver MyDriver = OSGeo.GDAL.Gdal.GetDriverByName(Driver);
            OSGeo.GDAL.Dataset MyDataSet;

            MyDataSet = MyDriver.Create(FileName, IntRasterXSize, IntRasterYSize, IntRasterCount, OSGeo.GDAL.DataType.GDT_Float64, null);
            MyDataSet = PutData(MyDataSet);

            MyDataSet.Dispose();

            IntBusy = false;
            return (true);
        }

        // Computes the minimum of values for a given band.
        public double Min(int BandIndex)
        {
            if (IntBands.Length == 0) return (0);

            double ThisMin = 0;
            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    ThisMin = IntBands[BandIndex].RasterValues[i];
                    break;
                }
            }

            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    if (IntBands[BandIndex].RasterValues[i] <= ThisMin) ThisMin = IntBands[BandIndex].RasterValues[i];
                }
            }

            return (ThisMin);
        }

        // Computes the maximum of values for a given band.
        public double Max(int BandIndex)
        {
            if (IntBands.Length == 0) return (0);

            double ThisMax = 0;
            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    ThisMax = IntBands[BandIndex].RasterValues[i];
                    break;
                }
            }

            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    if (IntBands[BandIndex].RasterValues[i] >= ThisMax) ThisMax = IntBands[BandIndex].RasterValues[i];
                }
            }

            return (ThisMax);
        }

        // Computes the mean of values for a given band.
        public double Mean(int BandIndex)
        {
            double Sum = 0;
            int NumVals = 0;

            if (IntBands.Length == 0) return (0);

            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    Sum = Sum + IntBands[BandIndex].RasterValues[i];
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

            if (IntBands.Length == 0) return (0);

            for (int i = 0; i < IntBands[BandIndex].RasterValues.Length; i++)
            {
                if (IntBands[BandIndex].RasterValues[i] != IntNA)
                {
                    Sum = Sum + System.Math.Pow(IntBands[BandIndex].RasterValues[i] - MyMean, 2);
                    NumVals++;
                }
            }

            return (System.Math.Sqrt(Sum / (NumVals - 1)));
        }

        public override string ToString()
        {
            return (IntWKT_SRS);
        }
    }
}
