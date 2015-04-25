using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Assets.MyScripts;
using ChaletScripts.ModelConstructor;

namespace FirstHMM
{
    public static class Program
    {
        
        public static void Main()
        {
            using(SqlConnection connection = new SqlConnection("server=(local);database=HumanAction;user id=sa;password=8265529.clsm"))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select spine_leftWrist from VectorsNormTable", connection);
                List<double> data = new List<double>();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        data.Add((double)reader[0]);
                    }
                }

                double[] dataArray = data.ToArray();
                ModelConstructor constructor = new ModelConstructor(dataArray);
                Console.WriteLine("Min = {0}, Max = {1}", constructor.Min, constructor.Max);
                double[] pi = new double[13];
                for(int i = 0; i < 13; i ++)
                {
                    pi[i] = 1.0f / 13;
                }

                double[,] transmission = new double[13, 13];
                for(int i = 0; i < 13; i ++)
                {
                    for(int j = 0; j < 13; j ++)
                    {
                        transmission[i, j] = 1.0f /13;
                    }
                }

                
                double[,] confusion = new double[13, ((int)Math.Ceiling(constructor.Max) - (int)Math.Floor(constructor.Min) + 1)];
                for(int i = 0; i < 13; i ++)
                {
                    for(int j = 0; j < ((int)Math.Ceiling(constructor.Max) - (int)Math.Floor(constructor.Min) + 1); j ++)
                    {
                        confusion[i, j] = 1.0f / ((int)Math.Ceiling(constructor.Max) - (int)Math.Floor(constructor.Min) + 1);
                    }
                }

                FirstHMM<int, int> Walk = new FirstHMM<int, int>(13, (int)Math.Ceiling(constructor.Max) - (int)Math.Floor(constructor.Min) + 1, pi, transmission, confusion);

                List<int[]> observableStatusGroups = new List<int[]>();
                for(int userID = 1; userID <= 8; userID ++)
                {
                    for(int groupID = 0; groupID <= 5; groupID ++)
                    {
                        cmd = new SqlCommand(string.Format("select spine_leftWrist from VectorsNormPatternsTable where UserID = {0} and GroupID = {1}", userID, groupID), connection);

                        List<int> tmp = new List<int>();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                tmp.Add((int)Math.Round((double)reader[0] - constructor.Min));
                            }
                        }

                        if(tmp.Count == 0)
                        {
                            //Console.WriteLine("UserID = {0}, GroupID = {1}: Data no found!", userID, groupID);
                            continue;
                        }

                        observableStatusGroups.Add(tmp.ToArray());
                    }
                }

                Walk.HMMLearning(observableStatusGroups);
                Console.WriteLine("Learning finished!");

                #region TestIdentification
                /*
                for (int userID = 1; userID <= 8; userID ++)
                {
                    for(int groupID = 0; groupID <= 5; groupID ++ )
                    {
                        cmd = new SqlCommand("select spine_leftWrist " +
                                             "from VectorsNormPatternsTable " +
                                             String.Format("where UserID = {0} and GroupID = {1}", userID, groupID), connection);

                        List<int> testData = new List<int>();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                testData.Add((int)Math.Round((double)reader[0] - constructor.Min));
                            }
                        }

                        if(testData.Count == 0)
                        {
                            Console.WriteLine("UserID = {0}, GroupID = {1}: Data no found!", userID, groupID);
                            continue;
                        }

                        int[] testDataArray = testData.ToArray();

                        Console.WriteLine("UserID = {0}, GroupID = {1}: The probability of being a walking action is {2}", 
                                           userID, groupID, Walk.ProbabilityOfObservableQueue(testDataArray, testDataArray.Length));
                    }
                }
                 * */
                #endregion
            }

        }
    }
}