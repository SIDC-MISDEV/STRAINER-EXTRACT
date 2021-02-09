﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using STRAINER_EXTRACT.Model;

namespace STRAINER_EXTRACT.Service
{
    public class MySQLHelper 
    {

        private MySqlConnection conn;
        private MySqlCommand cmd;
       
        private string connectionString = Properties.Settings.Default.DB;
        private string branchName = Properties.Settings.Default.BRANCH_CODE;
        private string warehouseCode = Properties.Settings.Default.WAREHOUSE;
        private string query = string.Empty;

        public void GetExtract(string _query)
        {
            try
            {
            

                using (conn = new MySqlConnection(connectionString))
                {

                    conn.Open();

                    query = _query;

                    using (cmd = new MySqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }

                }

            }
            catch
            {

                throw;
            }
        }

        public List<string> GetReferenceNumbers(string dateFrom, string dateTo)
        {
            try
            {
                List<string> val = new List<string>();

                using (conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    query = @"SELECT DISTINCT reference FROM ledger where DATE(date) BETWEEN @dateFrom AND @dateTo
                        AND LEFT(reference, 2) NOT IN (SELECT prefix FROM serial where objectType IN (13, 24, 14)) ORDER BY reference ASC;";

                    using (cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@dateFrom", dateFrom);
                        cmd.Parameters.AddWithValue("@dateTo", dateTo);

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                val.Add(dr["reference"].ToString());
                            }
                        }
                    }

                }

                return val;
            }
            catch
            {

                throw;
            }
        }

        public Branch GetBranchName()
        {
            try
            {
                var branch = new Branch();

                using (conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    query = "SELECT branchName, whse FROM business_segments WHERE branchCode = @branch AND whse = @whc";

                    using (cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@branch", branchName);
                        cmd.Parameters.AddWithValue("@whc", warehouseCode);

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                branch.BranchName = dr["branchName"].ToString();
                                branch.WarehouseCode = dr["whse"].ToString();
                            }
                        }
                    }
                }

                return branch;
            }
            catch
            {

                throw;
            }
        }

        public List<Prefix> GetPrefixData()
        {
            try
            {
                var result = new List<Prefix>();

                using (conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    query = @"SELECT objectType, prefix FROM serial ORDER BY idserial ASC;";

                    using (cmd = new MySqlCommand(query, conn))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                result.Add(new Prefix
                                {
                                    ObjectType = dr["objectType"].ToString(),
                                    ObjectPrefix = dr["prefix"].ToString()
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch
            {

                throw;
            }
        }
    
    }

   
}
