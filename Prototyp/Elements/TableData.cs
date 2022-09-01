﻿using System.Linq;
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

        // Constructor that accepts a string that is a filename of a csv file.
        public TableData(int uid, string InString)
        {
            if (System.IO.File.Exists(InString))
            {
                _busy = true;
                _filename = InString;

                _csvData = System.IO.File.ReadAllBytes(InString);
                SetID(uid);

                _busy = false;
            }
            else
            {
                //throw new System.Exception("File does not exist.");

                _busy = true;
                _csvData = Encoding.Default.GetBytes(InString);
                SetID(uid);
                _busy = false;
            }
        }

        // Constructor that is provided a byte array containing serialized table data.
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

        public override string ToString()
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