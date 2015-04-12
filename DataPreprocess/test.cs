using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;


namespace Chalet.DataPreprocess
{
    public static class Program
    {
        public static void Main()
        {
            #region 
            /*
            string source = "server=(local);database=HumanAction;user id=sa;password=8265529.clsm";
            List<double> sourceData = new List<double>();

            using(SqlConnection conn = new SqlConnection(source))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT spine_leftWrist FROM VectorsNormTable WHERE ActionID = 0 AND GroupID = 1 AND UserID = 1", conn);
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        sourceData.Add((double)reader[0]);
                    }
                }

                double[] data = sourceData.ToArray();
                DataPreprocess processor = new DataPreprocess(data.Length, 10, 7, 2.0f, data);
                for (int i = 0; i < 100; i ++ )
                {
                    processor.GetModel();
                    if (processor.Mod != null)
                        processor.SorptToModel(0.1f);
                }

                int t = 0;
                foreach(var element in processor.Mod.Value)
                {
                    SqlCommand writecmd = new SqlCommand("NewNormModel", conn);
                    writecmd.CommandType = CommandType.StoredProcedure;
                    writecmd.Parameters.AddWithValue("@UserID", 1);
                    writecmd.Parameters.AddWithValue("@ActionID", 0);
                    writecmd.Parameters.AddWithValue("@GroupID", 4);
                    writecmd.Parameters.AddWithValue("@TimeNum", t++);
                    writecmd.Parameters.AddWithValue("@spine_leftWrist", element);
                    writecmd.ExecuteNonQuery();
                }

                t = 0;
                foreach (var element in processor.Data)
                {
                    SqlCommand writecmd = new SqlCommand("NewSorptionTest", conn);
                    writecmd.CommandType = CommandType.StoredProcedure;
                    writecmd.Parameters.AddWithValue("@UserID", 1);
                    writecmd.Parameters.AddWithValue("@ActionID", 0);
                    writecmd.Parameters.AddWithValue("@GroupID", 1);
                    writecmd.Parameters.AddWithValue("@TimeNum", t++);
                    writecmd.Parameters.AddWithValue("@spine_leftWrist", element);
                    writecmd.ExecuteNonQuery();
                }
            }
            */
            #endregion

            #region LengthAdaptionTest
            double[] data = new double[30];
            double[] x = new double[30];
            double[] t_x = new double[12];

            Random rand = new Random();
            for (int i = 0; i < 30; i++)
            {
                data[i] = 50.0f * rand.NextDouble();
            }

            Console.WriteLine("Original Data");
            foreach (var element in data)
                Console.Write(element + ", ");

            Console.WriteLine();
            double[] newData = DataPreprocess.LengthAdaption(50, data);

            foreach (var element in newData)
                Console.Write(element + ", ");
            #endregion
        }
    }
}