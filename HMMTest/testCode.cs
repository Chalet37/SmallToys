using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Assets.MyScripts;

namespace FirstHMM
{
    public static class Program
    {
        public static void Main()
        {
            int walkNormnh = 15, runNormnh = 15;
            using(SqlConnection conn = new SqlConnection("server=local;database=HumanAction;user id=sa;password=8265529.clsm"))
            {
                conn.Open();

                Dictionary<int, double> walkNorms = ReadNormData(conn, 0);
                Dictionary<int, double> runNorms = ReadNormData(conn, 1);

                double[] initialTrans = new double[15];
                for(int i = 0; i < 15; i ++)
                {
                    initialTrans[i] = 1 / 15f;
                }

                double[] initialConfus = new double[15];

                FirstHMM<int, double> walkHMM = new FirstHMM<int,double>(walkNormnh, 15,
                    new double[15]{1.0f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, )
            }

        }

        public static Dictionary<int, double> ReadNormData(SqlConnection conn, int actionID)
        {
            SqlCommand cmd = new SqlCommand(string.Format("SELECT spine_leftWrist" +
                                            "FROM VectorsNormTable" + 
                                            "WHERE ActionID = {0} AND TimeNum > 19", actionID));
            SqlDataReader reader = cmd.ExecuteReader();

            Dictionary<int, double> norms = new Dictionary<int,double>();

            int t = 0;
            while(reader.Read())
            {
                norms.Add(t, (double)reader[t]);
            }

            return norms;
        }
    }
}