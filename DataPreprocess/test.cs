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
            string source = "server=(local);database=HumanAction;user id=sa;password=8265529.clsm";
            

            using(SqlConnection conn = new SqlConnection(source))
            {
                conn.Open();
                for (int userID = 1; userID <= 9; userID++)
                {
                    for (int groupID = 0; groupID <= 5; groupID++)
                    {
                        SqlCommand cmd = new SqlCommand(String.Format("SELECT spine_leftWrist FROM VectorsNormTable WHERE ActionID = 0 AND GroupID = {0} AND UserID = {1}", groupID, userID), conn);
                        List<double> sourceData = new List<double>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sourceData.Add((double)reader[0]);
                            }
                        }

                        Console.WriteLine("count = {0}", sourceData.Count);
                        if (sourceData.Count == 0)
                            break;

                        double[] data = sourceData.ToArray();
                        DataPreprocess processor = new DataPreprocess(data.Length, 10, 10, 2.0f, data);
                        processor.GeneratePattern(100, 0.1f);

                        if (processor.PatternPicked == null)
                        {
                            Console.WriteLine("UserID = {0}, GroupID = {1}: Pattern no found!");
                            break;
                        }

                        int t = 0;
                        foreach (var element in processor.PatternPicked.Value)
                        {
                            SqlCommand writecmd = new SqlCommand("NewNormModel", conn);
                            writecmd.CommandType = CommandType.StoredProcedure;
                            writecmd.Parameters.AddWithValue("@UserID", userID);
                            writecmd.Parameters.AddWithValue("@ActionID", 1);
                            writecmd.Parameters.AddWithValue("@GroupID", groupID);
                            writecmd.Parameters.AddWithValue("@TimeNum", t++);
                            writecmd.Parameters.AddWithValue("@spine_leftWrist", element);
                            writecmd.ExecuteNonQuery();
                        }

                        t = 0;
                        foreach (var element in processor.Data)
                        {
                            SqlCommand writecmd = new SqlCommand("NewSorptionTest", conn);
                            writecmd.CommandType = CommandType.StoredProcedure;
                            writecmd.Parameters.AddWithValue("@UserID", userID);
                            writecmd.Parameters.AddWithValue("@ActionID", 1);
                            writecmd.Parameters.AddWithValue("@GroupID", groupID);
                            writecmd.Parameters.AddWithValue("@TimeNum", t++);
                            writecmd.Parameters.AddWithValue("@spine_leftWrist", element);
                            writecmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}