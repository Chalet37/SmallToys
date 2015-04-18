using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaletScripts.Threshold
{
    public class Threshold<ValueT>
        where ValueT :struct
    {
        internal ValueT _max;
        internal ValueT _min;
        private double _length;

        public ValueT Max
        {
            get { return _max; }
        }

        public ValueT Min
        {
            get { return _min; }
        }

        public double Length
        {
            get { return _length; }
        }

        public Threshold(ValueT min, ValueT max)
        {
            this._max = max;
            this._min = min;

            object objMax = max;
            object objMin = min;

            this._length = (double)objMax - (double)objMin;
        }
    }
}
