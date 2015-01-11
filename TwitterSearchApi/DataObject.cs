using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WZWVAPI
{
    public abstract class DataObject
    {
        public int ID { get; protected set; }

        public DataObject()
        {

        }

        public void setID(int ID)
        {
            this.ID = ID;
        }

        public abstract override string ToString();
    }
}
