using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using ChaletScripts.Threshold;

namespace ChaletScripts.ModelConstructor
{
    public class ModelConstructor
    {
        private double[] data;
        private long dataLength;
        private double _max, _min;
        private Threshold<double>[] _thresholds;
        private Dictionary<long, Threshold<double>> _enumSet;

        /// <summary>
        /// Maximum of data
        /// </summary>
        public double Max
        {
            get { return _max; }
        }

        /// <summary>
        /// Minimum of data
        /// </summary>
        public double Min
        {
            get { return _min; }
        }

        /// <summary>
        /// Group of thresholds(it is available after Method PlaceThresholds carried out)
        /// </summary>
        public Threshold<double>[] Thresholds
        {
            get { return _thresholds; }
        }


        /// <summary>
        /// EnumSet matching the group of thresholds.
        /// </summary>
        public Dictionary<long, Threshold<double>> EnumSet
        {
            get { return this._enumSet; }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="data">source data</param>
        public ModelConstructor(double[] data)
        {
            this.dataLength = data.Length;
            Array.Copy((double[])data.Clone(), this.data, this.dataLength);
            _max = GetMaximum();
            _min = GetMinimun();
            this._thresholds = null;
            this._enumSet = new Dictionary<long, Threshold<double>>();
        }

        /// <summary>
        /// Get the Maximum in data
        /// </summary>
        /// <returns>maximum in data</returns>
        private double GetMaximum()
        {
            double max = Double.MinValue;
            foreach(var element in data)
            {
                if (element > max)
                    max = element;
            }

            return max;
        }


        /// <summary>
        /// Get the minimum in data
        /// </summary>
        /// <returns>minimum in data</returns>
        private double GetMinimun()
        {
            double min = Double.MaxValue;
            foreach(var element in data)
            {
                if (element < min)
                    min = element;
            }

            return min;
        }

        /// <summary>
        /// determin intervals and thresholds
        /// </summary>
        /// <param name="numOfIntervals">the number of intervals</param>
        /// <returns>the length of every interval</returns>
        public double PlaceThresholds(long numOfIntervals)
        {
            double intervalLength = (this._max - this._min) / (double)numOfIntervals;
            this._thresholds = new Threshold<double>[numOfIntervals];

            _thresholds[0]._min = this._min;
            _thresholds[0]._max = this._min + intervalLength;
            this._enumSet.Add(0, _thresholds[0]);

            for(int i = 1; i < numOfIntervals; i ++)
            {
                _thresholds[i]._min = _thresholds[i - 1]._max;
                _thresholds[i]._max = _thresholds[i]._min + intervalLength;
                this._enumSet.Add(i, _thresholds[i]);
            }

            return intervalLength;
        }
    }
}
