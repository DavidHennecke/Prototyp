using System.Linq;

namespace Prototyp.Elements
{
    public class TableData
    {
        private byte[] _csvData;
        private bool _busy;
        private string _filename;
        private string _fileType;
        private string _name;
        private int _ID = 0;

        // Getters and setters -------------------------------------------------------------

        public byte[] csvData
        {
            get { return (_csvData); }
            set
            {
                _csvData = value;
            }
        }

        public bool Busy
        {
            get { return (_busy); }
        }

        public string FileName
        {
            get { return (_filename); }
            set { _filename = value; }
        }

        public string Name
        {
            get { return (_name); }
            set { _name = value; }
        }

        public string FileType
        {
            get { return (_fileType); }
            set { _fileType = value; }
        }

        public int ID
        {
            get { return (_ID); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public TableData(int uid)
        {
            SetID(uid);
        }

        // Constructor that accepts a string that is a filename of a csv file.
        public TableData(int uid, string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                _busy = true;
                using (System.IO.Stream SourceFile = System.IO.File.OpenRead(fileName))
                {
                    _filename = fileName;

                    byte[] FileBuffer = new byte[8];
                    SourceFile.Read(FileBuffer, 0, FileBuffer.Length);
                    _csvData = System.IO.File.ReadAllBytes(fileName);
                    SetID(uid);

                }
                _busy = false;
            }
            else
            {
                throw new System.Exception("File does not exist.");
            }
        }

        // Constructor that is provided a byte array containing serialized FGB VectorData.
        // Example:
        // VectorData vectorData = new VectorData(VecArray);
        public TableData(int uid, byte[] csvArray)
        {
            _busy = true;
            _csvData = csvArray;
            SetID(uid);
            _busy = false;
        }

        // Private methods -----------------------------------------------------------------

        // Set ID.
        private void SetID(int uid)
        {
            _ID = uid;
        }


        // Static methods ------------------------------------------------------------------

        public static string ByteArrToString(byte[] ByteArr)
        {
            return (System.Convert.ToBase64String(ByteArr));
        }

        public static byte[] StringToByteArr(string ByteStr)
        {
            return (System.Convert.FromBase64String(ByteStr));
        }


        // Methods -------------------------------------------------------------------------

        public string ToString()
        {
            string MyString = null;

            if (_csvData != null)
            {
                MyString = ByteArrToString(_csvData);
            }

            return (MyString);
        }
    }
}