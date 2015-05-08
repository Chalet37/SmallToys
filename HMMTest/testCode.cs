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
using Chalet.DataPreprocess;

namespace FirstHMM
{
    public static class Program
    {
        
        public static void Main()
        {
            StreamWriter walkWriter = new StreamWriter("D:\\Users\\Chalet\\Workspaces\\Unity\\ActionIdentification\\Assets\\walk.hmm");
            StreamWriter runWriter = new StreamWriter("D:\\Users\\Chalet\\Workspaces\\Unity\\ActionIdentification\\Assets\\run.hmm");
            using(SqlConnection connection = new SqlConnection("server=(local);database=HumanAction;user id=sa;password=8265529.clsm"))
            {
                connection.Open();
                #region Walk
                
                SqlCommand cmd = new SqlCommand("select spine_leftWrist from VectorsNormPatternsTable where ActionID = 0", connection);
                List<double> data = new List<double>();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        data.Add((double)reader[0]);
                    }
                }

                double[] dataArray = new double[2] { 0.0f, 1.0f };
                ModelConstructor constructor = new ModelConstructor(dataArray);
                Console.WriteLine("Min = {0}, Max = {1}", constructor.Min, constructor.Max);
                constructor.PlaceThresholds(5);
                Random rand = new Random();
                double[] pi = new double[13];
                double sum = 0;
                for(int i = 0; i < 13; i ++)
                {
                    pi[i] = rand.NextDouble();
                    sum += pi[i];
                }

                for (int i = 0; i < 13; i ++ )
                {
                    pi[i] /= sum;
                }

                Console.WriteLine("Initial PI = ");
                for(int i = 0; i < 13; i ++)
                {
                    Console.Write("{0} ", pi[i].ToString("0.00"));
                }

                double[,] transmission = new double[13, 13];
                
                for(int i = 0; i < 13; i ++)
                {
                    sum = 0;
                    for(int j = 0; j < 13; j ++)
                    {
                        transmission[i, j] = rand.NextDouble();
                        sum += transmission[i, j];
                    }

                    for(int j = 0; j < 13; j ++)
                    {
                        transmission[i, j] /= sum;
                    }
                }

                Console.WriteLine("Initial transmission Matrix:");
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        Console.Write("{0} ", transmission[i, j].ToString("0.00"));
                    }
                    Console.WriteLine();
                }
                
                double[,] confusion = new double[13, 5];
                for(int i = 0; i < 13; i ++)
                {
                    sum = 0;
                    for(int j = 0; j < 5; j ++)
                    {
                        confusion[i, j] = rand.NextDouble();
                        sum += confusion[i, j];
                    }

                    for(int j = 0; j < 5; j ++)
                    {
                        confusion[i, j] /= sum;
                    }
                }

                Console.WriteLine("Initial confusion Matrix:");
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Console.Write("{0} ", confusion[i, j].ToString("0.00"));
                    }
                    Console.WriteLine();
                }

                FirstHMM<int, int> Walk = new FirstHMM<int, int>(13, 5, pi, transmission, confusion);

                List<int[]> observableStatusGroups = new List<int[]>();
                for(int userID = 1; userID <= 8; userID ++)
                {
                    for(int groupID = 0; groupID <= 5; groupID ++)
                    {

                        if ((userID == 3 && groupID == 0) || (userID == 4 && groupID == 0) || (userID == 4 && groupID == 1) || (userID == 6 && groupID == 0) || (userID == 7) || userID == 8)
                        {
                            continue;
                        }

                        

                        cmd = new SqlCommand(string.Format("select spine_leftWrist from VectorsNormPatternsTable where UserID = {0} and GroupID = {1} and ActionID = 0", userID, groupID), connection);

                        List<double> tmp = new List<double>();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                tmp.Add((double)reader[0]);
                                Console.Write("{0}\t", (double)reader[0]);
                            }
                        }

                        if(userID == 1 && groupID == 1)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                tmp.Add(tmp[0]);
                                tmp.RemoveAt(0);
                            }
                        }

                        if(userID == 1 && groupID == 2)
                        {
                            tmp.Add(tmp[0]);
                            tmp.RemoveAt(0);
                        }

                        Console.WriteLine();

                        if(tmp.Count == 0)
                        {
                            //Console.WriteLine("UserID = {0}, GroupID = {1}: Data no found!", userID, groupID);
                            continue;
                        }

                        double[] tmpArray = tmp.ToArray();
                        double[] tmpArrayRegular = tmp.ToArray();
                        Array.Sort(tmpArray);
                        double min = tmpArray[0];
                        double max = tmpArray[tmpArray.Length - 1];

                        for(int i = 0; i < tmpArrayRegular.Length; i ++)
                        {
                            tmpArrayRegular[i] = (tmpArrayRegular[i] - min) / (max - min);
                        }

                        int[] processedData = constructor.Reconstructor(DataPreprocess.LengthAdaption(50, tmpArrayRegular));
                        
                        foreach(var element in processedData)
                        {
                            Console.Write("{0}\t", element);
                        }
                        Console.WriteLine();

                       
                        Console.WriteLine();
                        Console.WriteLine("EnumSet:");
                        foreach(var element in constructor.EnumSet)
                        {
                            Console.WriteLine("{0}: min = {1}, max = {2}\t", element.Key, element.Value.Min, element.Value.Max);
                        }
                        Console.WriteLine();
                        observableStatusGroups.Add(processedData);
                    }

                }
                
                #endregion Walk

                #region Run
                cmd = new SqlCommand("select spine_leftWrist from VectorsNormPatternsTable where ActionID = 0", connection);
                List<double> dataRun = new List<double>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataRun.Add((double)reader[0]);
                    }
                }



                double[] dataArrayRun = new double[2] { 0.0f, 1.0f };
                ModelConstructor constructorRun = new ModelConstructor(dataArrayRun);
                Console.WriteLine("Min = {0}, Max = {1}", constructorRun.Min, constructorRun.Max);
                constructorRun.PlaceThresholds(5);
                rand = new Random();
                double[] piRun = new double[13];
                double sumRun = 0;
                for (int i = 0; i < 13; i++)
                {
                    piRun[i] = rand.NextDouble();
                    sumRun += piRun[i];
                }

                for (int i = 0; i < 13; i ++)
                {
                    piRun[i] /= sumRun;
                }

                Console.WriteLine("Initial PI = ");
                for (int i = 0; i < 13; i++)
                {
                    Console.Write("{0} ", piRun[i].ToString("0.00"));
                }

                double[,] transmissionRun = new double[13, 13];

                for (int i = 0; i < 13; i++)
                {
                    sumRun = 0;
                    for (int j = 0; j < 13; j++)
                    {
                        transmissionRun[i, j] = rand.NextDouble();
                        sumRun += transmissionRun[i, j];
                    }

                    for(int j = 0; j < 13; j ++)
                    {
                        transmission[i, j] /= sum;
                    }
                }

                Console.WriteLine("Initial transmission Matrix:");
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        Console.Write("{0} ", transmissionRun[i, j].ToString("0.00"));
                    }
                    Console.WriteLine();
                }

                double[,] confusionRun = new double[13, 5];
                for (int i = 0; i < 13; i++)
                {
                    sumRun = 0;
                    for (int j = 0; j < 5; j++)
                    {
                        confusionRun[i, j] = rand.NextDouble();
                        sumRun += confusionRun[i, j];
                    }

                    for(int j = 0; j < 5; j ++)
                    {
                        confusionRun[i, j] /= sum;
                    }
                }

                Console.WriteLine("Initial confusion Matrix:");
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Console.Write("{0} ", confusionRun[i, j].ToString("0.00"));
                    }
                    Console.WriteLine();
                }

                FirstHMM<int, int> Run = new FirstHMM<int, int>(13, 5, piRun, transmissionRun, confusionRun);

                List<int[]> observableStatusGroupsRun = new List<int[]>();
                for (int userID = 1; userID <= 8; userID++)
                {
                    for (int groupID = 0; groupID <= 5; groupID++)
                    {
                        cmd = new SqlCommand(string.Format("select spine_leftWrist from VectorsNormPatternsTable where UserID = {0} and GroupID = {1} and ActionID = 1", userID, groupID), connection);

                        List<double> tmpRun = new List<double>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tmpRun.Add((double)reader[0]);
                                Console.Write("{0}\t", (double)reader[0]);
                            }
                        }

                        Console.WriteLine();

                        if (tmpRun.Count == 0)
                        {
                            //Console.WriteLine("UserID = {0}, GroupID = {1}: Data no found!", userID, groupID);
                            continue;
                        }

                        double[] tmpArrayRun = tmpRun.ToArray();
                        double[] tmpArrayRegularRun = tmpRun.ToArray();
                        Array.Sort(tmpArrayRun);
                        double minRun = tmpArrayRun[0];
                        double maxRun = tmpArrayRun[tmpArrayRun.Length - 1];

                        for (int i = 0; i < tmpArrayRegularRun.Length; i++)
                        {
                            tmpArrayRegularRun[i] = (tmpArrayRegularRun[i] - minRun) / (maxRun - minRun);
                        }

                        int[] processedDataRun = constructorRun.Reconstructor(DataPreprocess.LengthAdaption(50, tmpArrayRegularRun));

                        foreach (var element in processedDataRun)
                        {
                            Console.Write("{0}\t", element);
                        }
                        Console.WriteLine();


                        Console.WriteLine();
                        Console.WriteLine("EnumSet:");
                        foreach (var element in constructorRun.EnumSet)
                        {
                            Console.WriteLine("{0}: min = {1}, max = {2}\t", element.Key, element.Value.Min, element.Value.Max);
                        }
                        Console.WriteLine();

                        observableStatusGroupsRun.Add(processedDataRun);
                    }

                }
                #endregion Run

                Walk.HMMLearning(observableStatusGroups);
                Run.HMMLearning(observableStatusGroupsRun);
                Console.WriteLine("Learning finished!");

                #region SaveWalkHMM
                walkWriter.WriteLine("{0}, {1}", Walk.numberOfHiddenStatus, Walk.numberOfObservationStatus);

                for (int i = 0; i < Walk.numberOfHiddenStatus; i ++)
                {
                    walkWriter.Write("{0}\t", Walk.initialVector[i]);
                }

                walkWriter.WriteLine();

                for (int i = 0; i < Walk.numberOfHiddenStatus; i++)
                {
                    for (int j = 0; j < Walk.numberOfHiddenStatus; j++)
                    {
                        walkWriter.Write("{0}\t", Walk.transmissionMatrix[i, j]);
                    }
                    walkWriter.WriteLine();
                }

                for(int i = 0; i < Walk.numberOfHiddenStatus; i ++)
                {
                    for(int j = 0; j < Walk.numberOfObservationStatus; j ++)
                    {
                        walkWriter.Write("{0}\t", Walk.confusionMatrix[i, j]);
                    }
                    walkWriter.WriteLine();
                }

                #endregion SaveWalkHMM

                #region SaveRunHMM
                runWriter.WriteLine("{0}, {1}", Run.numberOfHiddenStatus, Run.numberOfObservationStatus);

                for (int i = 0; i < Run.numberOfHiddenStatus; i++)
                {
                    runWriter.Write("{0}\t", Run.initialVector[i]);
                }

                runWriter.WriteLine();

                for (int i = 0; i < Run.numberOfHiddenStatus; i++)
                {
                    for (int j = 0; j < Run.numberOfHiddenStatus; j++)
                    {
                        runWriter.Write("{0}\t", Run.transmissionMatrix[i, j]);
                    }
                    runWriter.WriteLine();
                }

                for (int i = 0; i < Run.numberOfHiddenStatus; i++)
                {
                    for (int j = 0; j < Run.numberOfObservationStatus; j++)
                    {
                        runWriter.Write("{0}\t", Run.confusionMatrix[i, j]);
                    }
                    runWriter.WriteLine();
                }
                #endregion SaveRunHMM


                #region TestIdentification

                #region GenerateRandomData

                double[] randomData = new double[50];
                for (int i = 0; i < 50; i ++)
                {
                    randomData[i] = rand.NextDouble();
                }

                Console.WriteLine("Random:{0}", Math.Log(Run.ProbabilityOfObservableQueue(constructorRun.Reconstructor(randomData), 50)));
                
                #endregion GenerateRandomData
                for (int userID = 1; userID <= 8; userID++)
                {
                    for (int groupID = 0; groupID <= 5; groupID++)
                    {
                        /*
                        if ((userID == 3 && groupID == 0) || (userID == 4 && groupID == 0) || (userID == 4 && groupID == 1) || (userID == 6 && groupID == 0) || (userID == 7) || userID == 8)
                        {
                            continue;
                        }
                        */

                        cmd = new SqlCommand("select spine_leftWrist " +
                                             "from VectorsNormPatternsTable " +
                                             String.Format("where UserID = {0} and GroupID = {1} and ActionID = 0", userID, groupID), connection);

                        List<double> testData = new List<double>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                testData.Add((double)reader[0]);
                            }
                        }

                        if (testData.Count == 0)
                        {
                            Console.WriteLine("UserID = {0}, GroupID = {1}: Data no found!", userID, groupID);
                            continue;
                        }

                        if (userID == 1 && groupID == 1)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                testData.Add(testData[0]);
                                testData.RemoveAt(0);
                            }
                        }

                        if (userID == 1 && groupID == 2)
                        {
                            testData.Add(testData[0]);
                            testData.RemoveAt(0);
                        }

                        double[] tmpArray = testData.ToArray();
                        double[] tmpArrayRegular = testData.ToArray();
                        Array.Sort(tmpArray);
                        double min = tmpArray[0];
                        double max = tmpArray[tmpArray.Length - 1];

                        for (int i = 0; i < tmpArrayRegular.Length; i++)
                        {
                            tmpArrayRegular[i] = (tmpArrayRegular[i] - min) / (max - min);
                        }

                        int[] testDataArray = constructorRun.Reconstructor(DataPreprocess.LengthAdaption(50, tmpArrayRegular));

                        Console.WriteLine("UserID = {0}, GroupID = {1}: The probability of being a walking action is {2}",
                                           userID, groupID, Math.Log(Walk.ProbabilityOfObservableQueue(testDataArray, testDataArray.Length)));
                        Console.WriteLine("UserID = {0}, GroupID = {1}: The probability of being a running action is {2}", userID, groupID, Math.Log(Run.ProbabilityOfObservableQueue(testDataArray, testDataArray.Length)));
                    }
                }
                #endregion
            }

        }
    }
}