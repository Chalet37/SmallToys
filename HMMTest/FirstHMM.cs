using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.initialVector = pi;
            this.transmissionMatrix = new double[nh, nh];
            Array.Copy((double[ , ])a.Clone(), transmissionMatrix, this.numberOfHiddenStatus * this.numberOfHiddenStatus);
            this.confusionMatrix = new double[nh, no];
            Array.Copy((double[ , ])b.Clone(), confusionMatrix, this.numberOfObservationStatus * this.numberOfObservationStatus);
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
            this.beta = new double[this.numberOfHiddenStatus, T];
            this.rho = new double[this.numberOfHiddenStatus, this.numberOfHiddenStatus, T];
            this.gamma = new double[this.numberOfHiddenStatus, T];
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
            object intj = j;
            object intObservableVector = observableVector;
            if (t == 0)
                alpha[(int)intj, 0] = initialVector[(int)intj] * confusionMatrix[(int)intj, ((int[])intObservableVector)[0]];
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
        private void AlphaDP(long T, ObservableT[] observableVector)
        {
            object HiddenTypej;
            for(long t = 0; t < T; t ++)
            {
                for(int j = 0; j < numberOfHiddenStatus; j++)
                {
                    HiddenTypej = j;
                    Forward((HiddenT)HiddenTypej, observableVector, t);
                }
            }
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
               
                for (int i = 0; i <= this.numberOfHiddenStatus; i++)
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

            for(int j = 0; j < numberOfHiddenStatus; j ++)
            {
                pr += alpha[j, T];
            }
            return pr;
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

            for (long t = 0; t < T; t++)
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
                        rho[i, j, t] = alpha[i, t] *
                                                 transmissionMatrix[i, j] *
                                                 beta[j, t + 1] *
                                                 confusionMatrix[j, ((int[])intObservableVector)[t + 1]];
                    }
                }
            }
        }

        /// <summary>
        /// initialVector-Learning method
        /// </summary>
        private void UpdateInitialVector()
        {
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
                    transmissionMatrix[i, j] = numerator / denominator;
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
                    confusionMatrix[j, k] = numerator / denominator;
                }
            }
        }

        /// <summary>
        /// do parametres estimation for one time
        /// </summary>
        /// <param name="oq"></param>
        /// <param name="T"></param>
        public void HMMLearningForOneTime(ObservableT[] oq, long T)
        {
            RefreshHMM(T);
            AlphaDP(T, oq);
            BetaDP(T, oq);
            GammaDP(T);
            RhoDP(T, oq);
            UpdateInitialVector();
            UpdateTransmissionMatrix(T);
            UpdateConfusionMatrix(T, oq);
        }

        /// <summary>
        /// carry out parametres learning for specific times
        /// </summary>
        /// <param name="oqs">some groups of ObservableStatus</param>
        /// <param name="numOfObservableStatusGroups">number of groups of ObservableStatus</param>
        public void HMMLearning(ObservableT[][] oqs, long numOfObservableStatusGroups)
        {
            for(int i = 0; i < numOfObservableStatusGroups; i ++)
            {
                HMMLearningForOneTime(oqs[i], oqs[i].Length);
            }
        }

        /// <summary>
        /// continue parametres learning till specific condition is converged
        /// </summary>
        /// <param name="oqs">some groups of ObservableStatus</param>
        /// <param name="isConvergence">condition function judge whether the iteration is converged</param>
        public void HMMLearning(ObservableT[][] oqs, Func<FirstHMM<HiddenT, ObservableT>, bool> isConvergence)
        {
            int i = 0;
            while(i < oqs.Length && !isConvergence(this))
            {
                HMMLearningForOneTime(oqs[i], oqs[i].Length);
            }
        }
    }
}
