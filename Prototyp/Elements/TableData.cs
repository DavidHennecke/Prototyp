using System.Linq;
using System.Text;

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
        public TableData()
        {

        }

        // Constructor utilizing only the mere ID.
        public TableData(int uid)
        {
            SetID(uid);
        }

        // Constructor that accepts a string that is a filename of a csv file OR the content of the csv file itself.
        public TableData(int uid, string InString)
        {
            if (System.IO.File.Exists(InString))
            {
                _busy = true;
                _filename = InString;
                _name = InString;

                if (!VectorData.FileAccessable(InString)) { throw new System.Exception("File is not accessible, maybe opened in some other software?"); }

                _csvData = System.IO.File.ReadAllBytes(InString);
                SetID(uid);

                _busy = false;
            }
            else
            {
                _busy = true;
                _csvData = Encoding.Default.GetBytes(InString);
                SetID(uid);
                _name = uid.ToString();
                _busy = false;
            }
        }

        // Constructor that is provided a byte array containing serialized table data.
        public TableData(int uid, byte[] csvArray)
        {
            _busy = true;
            _csvData = csvArray;
            SetID(uid);
            _name = uid.ToString();
            _busy = false;
        }

        // Private methods -----------------------------------------------------------------

        // Set ID.
        private void SetID(int uid)
        {
            _ID = uid;
        }

        // Static methods ------------------------------------------------------------------

        public static byte[] FindData(System.Collections.Generic.List<Google.Protobuf.ByteString> data)
        {
            // Reassemble csv data
            int targetPos = 0;
            byte[] result = new byte[data.Sum(a => a.Length)];
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Length; j++)
                {
                    result[targetPos] = data[i][j];
                    targetPos++;
                }
            }
            return result;
        }

        public static string ByteArrToString(byte[] ByteArr)
        {
            return (System.Convert.ToBase64String(ByteArr));
        }

        public static byte[] StringToByteArr(string ByteStr)
        {
            return (System.Convert.FromBase64String(ByteStr));
        }

        // Methods -------------------------------------------------------------------------

        public override string ToString()
        {
            string MyString = null;

            if (_csvData != null)
            {
                MyString = ByteArrToString(_csvData);
            }

            return (MyString);
        }

        public string[] GetLines()
        {
            var text = System.Text.Encoding.UTF8.GetString(_csvData);
            string[] lines = text.Split(
                new string[] { System.Environment.NewLine },
                System.StringSplitOptions.None
            );
            return lines;
        }
    }
}