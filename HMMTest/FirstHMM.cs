using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FirstHMM
{
    public class FirstHMM<HiddenT, ObservableT>
        where HiddenT:struct 
    {
        public int numberOfHiddenStatus;
        public int numberOfObservationStatus;
        public double[] initialVector;
        public double[ , ] transmissionMatrix;
        public double[ , ] confusionMatrix;
        public double[ , ] alpha;
        public double[ , ] beta;
        public double[ , , ] rho;
        public double[ , ] gamma;

        /// <summary>
        /// construction method of class FirstHMM
        /// </summary>
        /// <param name="nh">size of hidden status</param>
        /// <param name="no">size of observable status</param>
        /// <param name="pi">initial vector</param>
        /// <param name="a">initial copy of transmission matrix</param>
        /// <param name="b">initial copy of confussion matrix</param>
        public FirstHMM(int nh, int no, double[] pi, double[ , ]a, double[ , ]b)
        {
            this.numberOfHiddenStatus = nh;
            this.numberOfObservationStatus = no;
            this.initialVector = (double[])pi.Clone();
            this.transmissionMatrix = new double[nh, nh];
            this.transmissionMatrix = (double[,])a.Clone();
            //Array.Copy((double[ , ])a.Clone(), transmissionMatrix, this.numberOfHiddenStatus * this.numberOfHiddenStatus);
            this.confusionMatrix = new double[nh, no];
            this.confusionMatrix = (double[,])b.Clone();
            //Array.Copy((double[ , ])b.Clone(), confusionMatrix, this.numberOfObservationStatus * this.numberOfObservationStatus);
        }

        public FirstHMM(FirstHMM<HiddenT, ObservableT> hmm)
        {
            this.numberOfHiddenStatus = hmm.numberOfHiddenStatus;
            this.numberOfObservationStatus = hmm.numberOfObservationStatus;
            this.initialVector = (double[])hmm.initialVector.Clone();
            this.transmissionMatrix = ((double[,])hmm.transmissionMatrix.Clone());
            this.confusionMatrix = ((double[,])hmm.confusionMatrix.Clone());
        }


        //  method may be useless if the queue is preprocessed before input
        /// <summary>
        /// (re)Construct the array of alpha, beta, rho and gamma. 
        /// The method is supposed to be called before every time of learning for adapting input with variant length.
        /// </summary>
        /// <param name="T">length of timeline</param>
        private void RefreshHMM(long T)
        {
            this.alpha = new double[this.numberOfHiddenStatus, T];
            for (int i = 0; i < this.numberOfHiddenStatus; i ++)
            {
                for(int j = 0; j < T; j ++)
                {
                    this.alpha[i, j] = 0.0f;
                }
            }
            this.beta = new double[this.numberOfHiddenStatus, T];
            for (int i = 0; i < this.numberOfHiddenStatus; i ++)
            {
                for(int j = 0; j < T; j++)
                {
                    this.alpha[i, j] = 0.0f;
                }
            }
            this.rho = new double[this.numberOfHiddenStatus, this.numberOfHiddenStatus, T];
            for (int i = 0; i < this.numberOfHiddenStatus; i ++)
            {
                for(int j = 0; j < this.numberOfHiddenStatus; j ++)
                {
                    for(int k = 0; k < T; k ++)
                    {
                        this.rho[i, j, k] = 0.0f;
                    }
                }
            }

            this.gamma = new double[this.numberOfHiddenStatus, T];
            for(int i = 0; i < this.numberOfHiddenStatus; i ++)
            {
                for(int j = 0; j < T; j ++)
                {
                    this.gamma[i, j] = 0.0f;
                }
            }
        }
 
        /// <summary>
        /// Forward Algorithm
        /// </summary>
        /// <typeparam name="HiddenT">enum type of Hidden status</typeparam>
        /// <typeparam name="ObservableT">enum type of observable status</typeparam>
        /// <param name="j">current hidden status</param>
        /// <param name="t">specific time point</param>
        private void Forward(HiddenT j, ObservableT[] observableVector, long t)
        {
            //Console.WriteLine("Forward: j = {0}, t = {1}, observableVector[t] = {2}", j, t, observableVector[t]);
            object intj = j;
            object intObservableVector = observableVector;
            if (t == 0)
            {
                alpha[(int)intj, 0] = initialVector[(int)intj] * confusionMatrix[(int)intj, ((int[])intObservableVector)[0]];
                return;
            }
            for (int i = 0; i < numberOfHiddenStatus; i++)
            {
                alpha[(int)intj, t] += alpha[(int)i, t - 1] *
                                    transmissionMatrix[i, (int)intj] *
                                    confusionMatrix[(int)intj, ((int[])intObservableVector)[t]];
            }
        }
        /// <summary>
        /// Backward Algorithm
        /// </summary>
        /// <typeparam name="HiddenT">enum type of hidden status</typeparam>
        /// <typeparam name="ObservableT">enum type of observable status</typeparam>
        /// <param name="i">hidden status i</param>
        /// <param name="t">time point t</param>
        /// <param name="T">length of timeline</param>
        /// <param name="observableVector">specific observable queue</param>
        private void Backward(HiddenT i, long t, long T, ObservableT[] observableVector)
        {
            //Console.WriteLine("Backward: i = {0}", i);
            object inti = i;
            object intObservableVector = observableVector;
            if (t == T - 1)
                beta[(int)inti, t] = 1;
            else
            {
                for(int j = 0; j < numberOfHiddenStatus; j ++)
                {
                    beta[(int)inti, t] += beta[j, t + 1] * 
                                       transmissionMatrix[(int)inti, j] * 
                                       confusionMatrix[j, ((int[])intObservableVector)[t + 1]];
                }
            }

        }

        /// <summary>
        /// work out the whole matrix alpha by Dynamic-Programming
        /// </summary>
        /// <param name="T">length of timeline</param>
        /// <param name="observableVector">specific queue of observable queue</param>
        private double AlphaDP(long T, ObservableT[] observableVector)
        {
            object HiddenTypej;
            double[] scale = new double[T];
            for(int t = 0; t < observableVector.Length; t++)
            {
                scale[t] = 0.0f;
            }

            for(long t = 0; t < T; t ++)
            {
                for(int j = 0; j < numberOfHiddenStatus; j++)
                {
                    HiddenTypej = j;
                    Forward((HiddenT)HiddenTypej, observableVector, t);
                    scale[t] += alpha[j, t];
                }
            }

            double probability = 0.0f;
            for(int t = 0; t < observableVector.Length; t ++)
            {
                probability += Math.Log(scale[t]);
            }

            return probability;
        }

        /// <summary>
        /// work out the whole matrix beta by Dynamic-Programming
        /// </summary>
        /// <param name="T">length of timeline</param>
        /// <param name="observableVector">specific queue of observable status</param>
        private void BetaDP(long T, ObservableT[] observableVector)
        {
            object HiddenTypei;
            for (long t = T - 1; t >= 0; t--)
            {
               
                for (int i = 0; i < this.numberOfHiddenStatus; i++)
                {
                    HiddenTypei = i;
                    Backward((HiddenT)HiddenTypei, t, T, observableVector);
                }
            }
        }

        /// <summary>
        /// Calculate the probability of appearing the specific observable queue.
        /// </summary>
        /// <param name="observableQueue">a queue of observable status</param>
        /// <param name="T">length of timeline</param>
        /// <returns>the probability appearing the specific observable queue</returns>
        public double ProbabilityOfObservableQueue(ObservableT[] observableQueue, long T)
        {
            if (!(observableQueue is ObservableT[]))
                throw new Exception("observable queue is not in a right type");
            double pr = 0;

            RefreshHMM(T);
            AlphaDP(T, observableQueue);

            for(int j = 0; j < numberOfHiddenStatus; j ++)
            {
                pr += alpha[j, T - 1];
            }
            return pr;
        }

        public double LogProbabilityOfObservableQueue(ObservableT[] observableQueue, long T)
        {
            return Math.Log(this.ProbabilityOfObservableQueue(observableQueue, T));
        }

        /// <summary>
        /// update gamma[i][t]
        /// </summary>
        /// <typeparam name="T">length of timeline</typeparam>
        private void GammaDP(long T)
        {
            
            for (int i = 0; i < numberOfHiddenStatus; i++)
            {
                for(long t = 0; t < T; t ++)
                {
                    double denominator = 0;
                    for (int j = 0; j < numberOfHiddenStatus; j++)
                    {
                        denominator += alpha[(int)j, t] * beta[(int)j, t];
                    }
                    gamma[i, t] = alpha[i, t] * beta[i, t] / denominator;
                }
            }

        }
        /// <summary>
        /// update rho[i,j][t]
        /// </summary>
        /// <typeparam name="HiddenT"></typeparam>
        /// <typeparam name="ObservableT"></typeparam>
        /// <param name="observableVector">specific observable queue</param>
        /// <param name="T">length of timeline</param>
        private void RhoDP(long T, ObservableT[] observableVector)
        {
            object intObservableVector = observableVector;

            for (long t = 0; t < T - 1; t++)
            {
                double denominator = 0;
                for (int x = 0; x < numberOfHiddenStatus; x++)
                {
                    for (int y = 0; y < numberOfHiddenStatus; y++)
                    {
                        denominator += alpha[x, t] *
                                       transmissionMatrix[x, y] *
                                       beta[y, t + 1] *
                                       confusionMatrix[y, ((int[])intObservableVector)[t + 1]];
                    }
                }
                for (int i = 0; i < numberOfHiddenStatus; i++) 
                {
                    for (int j = 0; j < numberOfHiddenStatus; j++)
                    {
                        rho[i, j, t] = (alpha[i, t] *
                                       transmissionMatrix[i, j] *
                                       beta[j, t + 1] *
                                       confusionMatrix[j, ((int[])intObservableVector)[t + 1]]) / denominator;
                    }
                }
            }
        }

        /// <summary>
        /// initialVector-Learning method
        /// </summary>
        private void UpdateInitialVector()
        {
            //Console.WriteLine("length of initialVector = {0}, length of gamma = {1}, numberOfHiddenStatus = {2}", initialVector.Length, gamma.GetLength(0), numberOfHiddenStatus);
            for(int i = 0; i < numberOfHiddenStatus; i ++)
            {
                initialVector[i] = gamma[i, 0];
            }

            
        }

        /// <summary>
        /// transmissionMatrix-Learning method
        /// </summary>
        /// <param name="T">specific sample has T time points</param>
        private void UpdateTransmissionMatrix(long T)
        {
            double denominator;
            double numerator;
            for(int i = 0; i < numberOfHiddenStatus; i ++)
            {
                denominator = 0;
                for (long t = 0; t < T - 1; t ++)
                {
                    denominator += gamma[i, t];
                }

                for (int j = 0; j < numberOfHiddenStatus; j++)
                {
                    numerator = 0;
                    for (long t = 0; t < T - 1; t++)
                    {
                        numerator += rho[i, j, t];
                    }
                    transmissionMatrix[i, j] = .001 + (1 - .001 * this.numberOfHiddenStatus) * numerator / denominator;
                }
            }
        }

        /// <summary>
        /// confusionMatrix-learning method
        /// </summary>
        /// <param name="T">specific sample has T time points</param>
        /// <param name="observableVector">specific observable queue</param>
        private void UpdateConfusionMatrix(long T, ObservableT[] observableVector)
        {
            double denominator;
            double numerator;
            object intObservableVector = observableVector;

            for(int j = 0; j < numberOfHiddenStatus; j ++)
            {
                denominator = 0;
                for (long t = 0; t < T; t ++ )
                {
                    denominator += gamma[j, t];
                }
                for (int k = 0; k < numberOfObservationStatus; k++)
                {
                    
                    numerator = 0;
                    for(long t = 0; t < T; t ++)
                    {
                        if (((int[])intObservableVector)[t] == k)
                            numerator += gamma[j, t];
                    }
                    confusionMatrix[j, k] = .001 + (1 - .001 * this.numberOfObservationStatus) * numerator / denominator;
                }
            }
        }

        /// <summary>
        /// do parametres estimation for one time
        /// </summary>
        /// <param name="oq"></param>
        /// <param name="T"></param>
        public double HMMLearningForOneTime(ObservableT[] oq, long T)
        {
            #region SamplesOutput
            /*
            Console.Write("Samples: ");
            foreach(var element in oq)
            {
                Console.Write(element + "\t");
            }
            Console.WriteLine();
            */
            #endregion

            double probability = 0.0f;
            RefreshHMM(T);
            probability = AlphaDP(T, oq);
            BetaDP(T, oq);
            GammaDP(T);
            RhoDP(T, oq);
            UpdateInitialVector();
            UpdateTransmissionMatrix(T);
            UpdateConfusionMatrix(T, oq);

            #region transmissionMatrixOutput
            
            Console.WriteLine("transmission:");
            for(int i = 0; i < this.numberOfHiddenStatus; i ++)
            {
                for(int j = 0; j < this.numberOfHiddenStatus; j ++)
                {
                    Console.Write("{0}\t", this.transmissionMatrix[i, j].ToString("0.00"));
                }
                Console.WriteLine();
            }
            
            #endregion

            #region confusionMatrixOutput
            
            Console.WriteLine("confusion:");
            for (int i = 0; i < this.numberOfHiddenStatus; i++)
            {
                for (int j = 0; j < this.numberOfObservationStatus; j++)
                {
                    Console.Write("{0}\t", this.confusionMatrix[i, j].ToString("0.00"));
                }
                Console.WriteLine();
            }
            
            #endregion

            return probability;
        }

        /// <summary>
        /// carry out parametres learning for specific times
        /// </summary>
        /// <param name="oqs">some groups of ObservableStatus</param>
        /// <param name="numOfObservableStatusGroups">number of groups of ObservableStatus</param>
        public void HMMLearning(List<ObservableT[]> oqs, double exitError = 0.01)
        {
            double prevProbability = 0.0f;
            double currentProbability = 0.0f;
            foreach(var element in oqs)
            {
                while (true)
                {
                    //Console.WriteLine("HMMLearning: length of observable queue = {0}", element.Length);
                    currentProbability = HMMLearningForOneTime(element, element.Length);
                    if(currentProbability - prevProbability >= exitError)
                    {
                        prevProbability = currentProbability;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
