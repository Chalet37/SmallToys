using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLibrary.triSpline;

namespace Chalet.DataPreprocess
{
    public class DataPreprocess
    {
        private double[] data;
        private int dataLength;
        public class Threshold<ValueType>
        {
            private ValueType lowwerBound;
            private ValueType upperBound;

            public ValueType LowwerBound
            {
                get{return this.lowwerBound;}
            }

            public ValueType UpperBound
            {
                get{return this.upperBound;}
            }

            public Threshold(ValueType low, ValueType up)
            {
                this.upperBound = up;
                this.lowwerBound = low;
            }
        }
        public class ModelToBe:IComparable
        {
            public Dictionary<int, double> similarGroup;
            private int length;
            private int beginIndex;
            private double averageSimilarity;

            public double AverageSimilarity
            {
                get { return this.averageSimilarity; }
            }

            public int BeginIndex
            {
                get { return this.beginIndex; }
            }

            public int Length
            {
                get { return this.length; }
            }
            public ModelToBe(int length, int beginIndex)
            {
                this.similarGroup = new Dictionary<int, double>();
                this.length = length;
                this.beginIndex = beginIndex;
                this.averageSimilarity = 0;
            }

            public void Average()
            {
                foreach(var element in similarGroup)
                {
                    averageSimilarity += element.Value;
                }

                averageSimilarity /= similarGroup.Count;
            }

            int IComparable.CompareTo(object other)
            {
                return this.CompareTo((other as ModelToBe));
            }

            public int CompareTo(ModelToBe other)
            {
                        if (this.similarGroup.Count > (other as ModelToBe).similarGroup.Count)
                            return 1;
                        else
                            if (this.similarGroup.Count < (other as ModelToBe).similarGroup.Count)
                                return -1;
                            else
                                if (this.averageSimilarity < (other as ModelToBe).averageSimilarity)
                                    return 1;
                                else
                                    if (this.averageSimilarity > (other as ModelToBe).averageSimilarity)
                                        return -1;
                                    else
                                        return 0;
            }

            public static bool operator <(ModelToBe left, ModelToBe right)
            {
                if (left.CompareTo(right) == -1)
                    return true;
                else
                    return false;
            }

            public static bool operator >(ModelToBe left, ModelToBe right)
            {
                if (left.CompareTo(right) == 1)
                    return true;
                else
                    return false;
            }

            public static bool operator ==(ModelToBe left, ModelToBe right)
            {
                if (left.CompareTo(right) == 0)
                    return true;
                else
                    return false;
            }

            public static bool operator !=(ModelToBe left, ModelToBe right)
            {
                if (left.CompareTo(right) != 0)
                    return true;
                else
                    return false;
            }

            public override bool Equals(object obj)
            {
                return (this as ModelToBe) == (obj as ModelToBe);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public class Model
        {
            private int beginIndex;
            private int length;
            private double[] value;
            public Dictionary<int, double> similarGroup;

            public int BeginIndex
            {
                get { return this.beginIndex; }
            }

            public int Length
            {
                get { return this.length; }
            }

            public double[] Value
            {
                get { return this.value; }
            }


            public Model(ModelToBe model, double[] sourceData)
            {
                this.beginIndex = model.BeginIndex;
                this.length = model.Length;
                this.value = new double[length];
                this.similarGroup = new Dictionary<int, double>(model.similarGroup);
                Array.Copy(sourceData, beginIndex, value, 0, length);
            }
        }
        private Threshold<int> windowWidthThres;
        private Threshold<double> similarityThres;
        private Dictionary<string, ModelToBe> modelsToBe;
        private Model mod;

        public Model Mod
        {
            get { return this.mod; }
        }

        public double[] Data
        {
            get { return this.data; }
        }
        /// <summary>
        /// constructor of DataPreprocess(NOTES: DataPreprocess will generate a shallow copy of sourceData to ensure any operation won't change the original data)
        /// </summary>
        /// <param name="dataSetLength">length of data</param>
        /// <param name="windowInitialWidth">initial width of window</param>
        /// <param name="windowsWidthTolerance">threshold of window width</param>
        /// <param name="similarityTolerance">tolerance of similarity</param>
        /// <param name="sourceData">original data</param>
        public DataPreprocess(long dataSetLength, int windowInitialWidth, int windowsWidthTolerance, double similarityTolerance, double[]sourceData)
        {
            this.data = (double[])sourceData.Clone();
            this.windowWidthThres = new Threshold<int>(windowInitialWidth, windowInitialWidth + windowsWidthTolerance);
            this.similarityThres = new Threshold<double>(1, similarityTolerance);
            this.dataLength = data.Length;
            modelsToBe = new Dictionary<string, ModelToBe>();
            mod = null;
        }

        /// <summary>
        /// F-Check function
        /// </summary>
        /// <param name="beginIndex1">beginning index of data segment1</param>
        /// <param name="beginIndex2">beginning index of data segment2</param>
        /// <param name="length">length of data segment</param>
        /// <returns>value of F-Check</returns>
        private double FCheck(int beginIndex1, int beginIndex2, int length)
        {
            double average1 = 0, average2 = 0;
            double s1 = 0, s2 = 0;
            for (int i = 0; i < length; i ++)
            {
                average1 += data[beginIndex1 + i];
                average2 += data[beginIndex2 + i];
            }

            average1 /= (length);
            average2 /= (length);

            for ( int i = 0; i < length; i ++ )
            {
                s1 += Math.Pow(data[beginIndex1 + i] - average1, 2);
                s2 += Math.Pow(data[beginIndex2 + i] - average2, 2);
            }

            s1 /= (double)(length - 1);
            s2 /= (double)(length - 1);

            return s1 >= s2 ? (s1 / s2) : (s2 / s1);
        }

        /// <summary>
        /// Chi-Square-Check funcion 
        /// </summary>
        /// <param name="beginIndex1">beginning index of data segment1</param>
        /// <param name="beginIndex2">beginning index of data segment2</param>
        /// <param name="length">length of data segment</param>
        /// <returns>value of Chi-Square-Check</returns>
        private double ChiSquareCheck(int beginIndex1, int beginIndex2, int length)
        {
            double d = 0;
            for(int i = 0; i < length; i ++)
            {
                d += Math.Pow((data[beginIndex1 + i] - data[beginIndex2 + i]), 2) / (data[beginIndex1 + i] + data[beginIndex2 + i]);
            }

            return d;
        }

        /// <summary>
        /// check whether the two data segments are similar 
        /// </summary>
        /// <param name="beginIndex1">beginning index of data segment1</param>
        /// <param name="beginIndex2">beginning index of data segment2</param>
        /// <param name="length">length of data segment</param>
        /// <param name="similarity"></param>
        /// <returns></returns>
        private bool IsSimilar(int beginIndex1, int beginIndex2, int length, out double similarity)
        {
            similarity = ChiSquareCheck(beginIndex1, beginIndex2, length);
            return similarity <= this.similarityThres.UpperBound;
        }

        /// <summary>
        /// find repeat construction from data;
        /// </summary>
        public void GetModel()
        {
            modelsToBe.Clear();
            for(int w = this.windowWidthThres.LowwerBound; w < this.windowWidthThres.UpperBound; w ++)
            {
                for(int modelBegin = 0; modelBegin < dataLength - w; modelBegin ++)
                {
                    ModelToBe newModel = new ModelToBe(w, modelBegin);
                    for(int t = modelBegin - modelBegin / w  * w; t < dataLength - w; t += w)
                    {
                        if (t == modelBegin)
                            continue;
                        double similarity;
                        if(IsSimilar(modelBegin, t, w, out similarity))
                        {
                            //if(ChiSquareCheck(modelBegin, t, w) <= ChiSquareThres)
                                newModel.similarGroup.Add(t, similarity);
                        }
                    }

                    if (newModel.similarGroup.Count != 0)
                    {
                        newModel.Average();
                        modelsToBe.Add(modelBegin.ToString() + "_" + w.ToString(), newModel);
                    }
                }
            }

            //cannot find a model
            if (modelsToBe.Count == 0)
                return;

            ModelToBe model = modelsToBe.Values.First();
            foreach(var element in modelsToBe.Values)
            {
                if (element > model)
                    model = element;
            }


            //
            mod = new Model(model, data);
        }

        /// <summary>
        /// sorpt every period of data to the current model
        /// </summary>
        public void SorptToModel(double thres)
        {
                for (int t = mod.BeginIndex - mod.BeginIndex / mod.Length * mod.Length; t < dataLength - mod.Length; t += mod.Length)
                {
                    if (t == mod.BeginIndex || !mod.similarGroup.Keys.Contains(t))
                        continue;
                    for (int i = 0; i < mod.Length; i++)
                    {
                        data[t + i] = ((1 - thres) * data[t + i] + thres * mod.Value[i]);
                    }
                }
        }


        /// <summary>
        /// get repeated model after specific times of Iteration of sorption
        /// NOTE:if the sorption rate is samll, this method will be converge soon.
        /// </summary>
        public void IterativeGetModel()
        {
            int prevCount = 0;
            while(Mod == null || prevCount != Mod.similarGroup.Count)
            {
                this.GetModel();
                if (this.Mod != null)
                    this.SorptToModel(0.1f);
                else
                    return;
            }
        }


        /// <summary>
        /// get repeated model after specific times of Iteration of sorption.
        /// </summary>
        /// <param name="times">times of iteration</param>
        public void IterativeGetModel(long times)
        {
            for (int i = 0; i < times; i++)
            {
                this.GetModel();
                if (this.Mod != null)
                    this.SorptToModel(0.1f);
                else
                    return;
            }
        }

        public int GetPeriod()
        {
            return mod.Length;
        }

        public int GetNumOfPeriods()
        {
            return mod.similarGroup.Count + 1;
        }

        /// <summary>
        /// compress or strech the data to the particular length
        /// </summary>
        /// <param name="L">model length</param>
        /// <param name="data">original data</param>
        /// <param name="T">period of the data if it has</param>
        public static double[] LengthAdaption(long L, double[]data)
        {
            long l = data.Length;
            if (l == L)
                return data;

            double[] newData = new double[L];
            if (l < L)
            {
                #region Spline
                double[] t_x = new double[Math.Abs(l - L)];
                double[] x = new double[l];
                bool[] flags = new bool[L];
                long t = 0;
                for (t = 0; t < l; t++)
                {
                    x[t] = t * L / l;
                    flags[t * L / l] = true;
                }
                long t_i = 0;
                for(t = 0; t < L; t ++)
                {
                    if(flags[t] == false)
                    {
                        t_x[t_i] = t;
                        t_i++;
                    }
                }

                #region Observe x
                /*
                Console.WriteLine("array x:");
                foreach (var element in x)
                    Console.Write(element + "\t");
                Console.WriteLine();
                */
                #endregion

                #region Observe t_x
                /*
                Console.WriteLine("array t_x:");
                foreach (var element in t_x)
                    Console.Write(element + "\t");
                Console.WriteLine();
                */
                #endregion

                double[] t_y = triSpline.GettriSpline_y(x, data, (int)l, t_x, (int)(L - l));

                t = 0;
                foreach(var element in x)
                {
                    newData[(long)element] = data[t++];
                }

                t = 0;
                foreach(var element in t_x)
                {
                    newData[(long)element] = t_y[t++];
                }
                #endregion
                return newData;
            }
            else
            {
                #region compression
                double[] t_x = new double[L];
                for (long t = 0, prevIndex = -1; t < l; t++)
                {
                    if (t * L / l != prevIndex)
                    {
                        prevIndex = t * L / l;
                        t_x[prevIndex] = t;
                    }
                }

                #region Observe t_x
                /*
                Console.WriteLine("array t_x:");
                foreach (var element in t_x)
                    Console.Write(element + "\t");
                Console.WriteLine();
                */
                #endregion

                for (int i = 0; i < L; i++)
                {
                    newData[i] = data[(long)t_x[i]];
                }
                #endregion
                    return newData;
            }
        }
    }
}
