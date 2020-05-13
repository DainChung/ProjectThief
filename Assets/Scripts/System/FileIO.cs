using System.Text;
using System.Data;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using Mono.Data.Sqlite;

using Com.MyCompany.MyGame;
using Com.MyCompany.MyGame.GameSystem;
using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame.FileIO
{
    public static class DBIO
    {
        //private const string fileName = "UnitStatDB.db";

        private static List<UnitStat> CreateInitData()
        {
            List<UnitStat> initData = new List<UnitStat>();
            for (int i = 0; i < 5; i++)
                initData.Add(new UnitStat());

            initData[0] = new UnitStat("DummyFast", 50, 0.41f, 0.31f, 5, 400);
            initData[1] = new UnitStat("Enemy", 20, 0.41f, 0.31f, 5, 400);
            initData[2] = new UnitStat("DummySlow", 10, 0.41f, 0.31f, 5, 400);
            initData[3] = new UnitStat("Player", 200, 0.41f, 0.31f, 10, 300);
            initData[4] = new UnitStat("DummyTank", 20, 0.41f, 0.31f, 10, 400);

            return initData;
        }

        private static void Write(string file, List<UnitStat> data)
        {
            bool CreateTable = false;
            string filePath = @"Data Source=" + FilePaths.DataPath + "/" + file;

            if (data.Count == 0)
            {
                CreateTable = true;
                data = CreateInitData();
            }

            using (var dbConnection = new SqliteConnection(filePath))
            {
                dbConnection.Open();

                using (SqliteTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string sqlQuery;

                        #region DB파일이 없으면 생성
                        if (CreateTable)
                        {
                            sqlQuery = "CREATE TABLE UNITSTAT (Code varchar(20), Speed float, WalkVar float, CoverVar float, MaxHealth float, JumpPower float)";

                            dbCommand.CommandText = sqlQuery;
                            dbCommand.ExecuteNonQuery();
                        }
                        #endregion

                        #region 데이터 입력
                        for (int i = 0; i < data.Count; i++)
                        {
                            sqlQuery = "INSERT INTO UNITSTAT (Code, Speed, WalkVar, CoverVar, MaxHealth, JumpPower) VALUES (@code, @speed, @walkVar, @coverVar, @maxHealth, @jumpPower);";
                            dbCommand.CommandText = sqlQuery;

                            dbCommand.Parameters.Add(new SqliteParameter("@code", data[i].unitCode));
                            dbCommand.Parameters.Add(new SqliteParameter("@speed", data[i].speed));
                            dbCommand.Parameters.Add(new SqliteParameter("@walkVar", (data[i].walkSpeed / data[i].speed)));
                            dbCommand.Parameters.Add(new SqliteParameter("@coverVar", (data[i].coverSpeed / data[i].speed)));
                            dbCommand.Parameters.Add(new SqliteParameter("@maxHealth", data[i].MaxHealth));
                            dbCommand.Parameters.Add(new SqliteParameter("@jumpPower", data[i].jumpPower));

                            dbCommand.ExecuteNonQuery();
                        }
                        #endregion
                        dbCommand.Dispose();
                    }
                    dbTransaction.Commit();
                }
                dbConnection.Close();
            }
        }

        /// <summary>
        /// unitCode로 데이터 하나만 읽어서 반환
        /// </summary>
        /// <param name="unitCode">DB 파일에서 읽을 유닛의 데이터</param>
        /// <returns></returns>
        public static UnitStat Read(string file, string unitCode)
        {
            string filePath = @"" + FilePaths.DataPath + "/" + file;
            //파일이 없다면 만든다.
            if (!File.Exists(filePath))
            {
                List<UnitStat> data = new List<UnitStat>();
                Write(file, data);
            }
            UnitStat result = new UnitStat();
            filePath = @"Data Source=" + filePath;

            using (var dbConnection = new SqliteConnection(filePath))
            {
                dbConnection.Open();

                using (SqliteTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string sqlQuery = "SELECT * FROM UNITSTAT WHERE code='" + unitCode + "';";
                        dbCommand.CommandText = sqlQuery;

                        IDataReader reader = dbCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            string code = reader.GetString(0);
                            float speed = reader.GetFloat(1);
                            float walkVar = reader.GetFloat(2);
                            float coverVar = reader.GetFloat(3);
                            float maxHealth = reader.GetFloat(4);
                            float jumpPower = reader.GetFloat(5);

                            result = new UnitStat(speed, walkVar, coverVar, maxHealth, jumpPower);
                            result.SetCode(code);
                        }
                        reader.Close();
                        reader = null;
                        dbCommand.Dispose();
                    }
                    dbTransaction.Commit();
                }
                dbConnection.Close();
            }

            if (result.unitCode == "NULL") result = null;

            return result;
        }

        public static List<UnitStat> ReadAll(string file)
        {
            string filePath = @"" + FilePaths.DataPath + "/" + file;
            //파일이 없다면 만든다.
            if (!File.Exists(filePath))
            {
                List<UnitStat> data = new List<UnitStat>();
                Write(file, data);
            }
            List<UnitStat> result = new List<UnitStat>();
            filePath = @"Data Source=" + filePath;

            int index = 0;
            using (var dbConnection = new SqliteConnection(filePath))
            {
                dbConnection.Open();

                using (SqliteTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string sqlQuery = "SELECT * FROM UNITSTAT;";
                        dbCommand.CommandText = sqlQuery;

                        IDataReader reader = dbCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            string code = reader.GetString(0);
                            float speed = reader.GetFloat(1);
                            float walkVar = reader.GetFloat(2);
                            float coverVar = reader.GetFloat(3);
                            float maxHealth = reader.GetFloat(4);
                            float jumpPower = reader.GetFloat(5);

                            result.Add(new UnitStat(speed, walkVar, coverVar, maxHealth, jumpPower));
                            result[index].SetCode(code);
                            index++;
                        }
                        reader.Close();
                        reader = null;
                        dbCommand.Dispose();
                    }
                    dbTransaction.Commit();
                }
                dbConnection.Close();
            }

            return result;
        }
    }

    public static class DataIO
    {
        private static void Write(string file)
        {
            string filePath = FilePaths.DataPath + "/" + file;
            //BestRecords.data 생성
            if (file.Contains("Best"))
            {
                StringBuilder data = new StringBuilder();

                data.AppendLine("Stage1,9999.99,0.000");
                data.AppendLine("Stage2,9999.99,0.000");

                File.WriteAllText(filePath, data.ToString());
            }
            //SoundSetting.data 생성
            else
            {
                StringBuilder data = new StringBuilder();

                data.AppendLine("20");

                File.WriteAllText(filePath, data.ToString());
            }
        }

        public static void Write(string file, string stageName, float gameTime, float gameScore)
        {
            string filePath = FilePaths.DataPath + "/" + file;
            StringBuilder writeData = new StringBuilder();
            string[] allData = File.ReadAllLines(filePath);

            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i].Contains(stageName)) writeData.AppendLine(string.Format("{0},{1},{2}", stageName, gameTime, gameScore));
                else writeData.AppendLine(allData[i]);
            }
            File.WriteAllText(filePath, writeData.ToString());
        }

        /// <summary>
        /// 파일의 특정 줄만 수정함
        /// </summary>
        /// <param name="file">파일이름</param>
        /// <param name="data">저장할 데이터</param>
        /// <param name="index">수정할 줄</param>
        public static void Write(string file, string data, int index)
        {
            string filePath = FilePaths.DataPath + "/" + file;
            StringBuilder writeData = new StringBuilder();
            string[] allData = File.ReadAllLines(filePath);

            for (int i = 0; i < allData.Length; i++)
            {
                if (index == i) allData[index] = data;

                writeData.AppendLine(allData[i]);
            }
            File.WriteAllText(filePath, writeData.ToString());
        }

        public static GameResult Read(string file, string stageName)
        {
            string filePath = FilePaths.DataPath + "/" + file;
            //파일이 없다면 만든다
            if (!File.Exists(filePath)) Write(file);

            string[] allData = File.ReadAllLines(filePath);
            GameResult result;
            result.score = -1;
            result.gameTime = new GameTime("9999");

            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i].Contains(stageName))
                {
                    string[] d = allData[i].Split(',');
                    result.gameTime = new GameTime(d[1]);
                    float.TryParse(d[2], out result.score);
                    break;
                }
            }

            return result;
        }
        /// <summary>
        /// 파일의 특정 줄만 읽음
        /// </summary>
        /// <param name="file">파일이름</param>
        /// <param name="index">읽을 줄</param>
        /// <returns></returns>
        public static string Read(string file, int index)
        {
            string filePath = FilePaths.DataPath + "/" + file;
            //파일이 없다면 만든다
            if (!File.Exists(filePath)) Write(file);

            string[] allData = File.ReadAllLines(filePath);
            string result;

            result = allData[index];

            return result;
        }
    }
}
