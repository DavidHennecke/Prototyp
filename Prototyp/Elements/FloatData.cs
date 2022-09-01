using System.Linq;
using System.Text;

namespace Prototyp.Elements
{
    public class FloatData
    {
        private float _floatData;
        private bool _busy;
        private string _name;
        private int _ID = 0;

        // Getters and setters -------------------------------------------------------------

        public float floatData
        {
            get { return (_floatData); }
            set
            {
                _floatData = value;
            }
        }

        public bool Busy
        {
            get { return (_busy); }
        }

        public string Name
        {
            get { return (_name); }
            set { _name = value; }
        }

        public int ID
        {
            get { return (_ID); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public FloatData()
        {

        }

        // Constructor utilizing only the mere ID.
        public FloatData(int uid)
        {
            SetID(uid);
        }

        // Constructor accepting the actual data.
        public FloatData(int uid, float NewData)
        {
            SetID(uid);
            floatData = NewData;
        }

        // Private methods -----------------------------------------------------------------

        // Set ID.
        private void SetID(int uid)
        {
            _ID = uid;
        }
    }
}