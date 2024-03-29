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

        private Dictionary<string, int> transactions = new Dictionary<string, int>()
        {
            { "AR", 13 },
            { "GI", 60 },
            { "GR", 59 },
            { "IP", 24 },
            { "PR", 22 },
            { "RC", 14 },
            { "RG", 21 },
            { "RV", 20 }
        };

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
                List<string> parameter = new List<string>();

                var excluded = Properties.Settings.Default.GENERATION_BY_BATCH;

                foreach (var item in excluded.Split(','))
                {
                    parameter.Add(transactions[item].ToString());
                }

                using (conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    var prm = string.Join(",", parameter);

                    query = @"SELECT DISTINCT reference FROM ledger where (DATE(date) BETWEEN @dateFrom AND @dateTo)
                        AND LEFT(reference, 2) NOT IN (SELECT prefix FROM serial where objectType IN (@trans)) AND extracted = 'N'
                        UNION ALL
                        SELECT DISTINCT reference FROM paiwi where (DATE(date) BETWEEN @dateFrom AND @dateTo)
                        AND LEFT(reference, 2) = 'WS' AND extracted = 'N'
                        ORDER BY reference ASC;";

                    using (cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@dateFrom", dateFrom);
                        cmd.Parameters.AddWithValue("@dateTo", dateTo);
                        cmd.Parameters.AddWithValue("@trans", prm);
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

                    query = "SELECT branchName, whse, branchCodeNumber FROM business_segments WHERE branchCode = @branch AND whse = @whc";

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
                                branch.BranchCodeNumber = dr["branchCodeNumber"].ToString();
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

        public void UpdateExtracted(string dates, List<string> trType)
        {
            StringBuilder sb = new StringBuilder();
            string transType = $"'{string.Join("','", trType)}'";

            try
            {


                using (conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    //query = @"update invoice i set i.extracted='Y' where i.extracted ='N' and left(i.reference,2) IN
                    // (@trtype) and date(i.date)=@dates and
                    // not i.cancelled and  i.quantity<>'0';
                    // update ledger l set l.extracted='Y' where l.extracted='N' and left(l.reference,2) in ('SI','OR') and  date(l.date)=@dates ;
                    // update transactionpayments a set a.extracted='Y' where a.extracted='N' and left(a.reference,2) in ('SI','OR') and  date(a.date)=@dates ;";

                    //sb.Append(@"UPDATE ledger SET extracted = 'Y' WHERE LEFT(reference, 2) IN (@trtype) AND DATE(date) = @dates;
                    //            UPDATE invoice SET extracted = 'Y' WHERE LEFT(reference, 2) IN(@trtype) AND DATE(date) = @dates; 
                    //            UPDATE paiwi SET extracted = 'Y' WHERE LEFT(reference, 2) = 'WS' AND DATE(date) = @dates;");

                    sb.Append($@"UPDATE ledger SET extracted = 'Y' WHERE LEFT(reference, 2) IN ({transType}) AND DATE(date) = '{dates}';
                                UPDATE invoice SET extracted = 'Y' WHERE LEFT(reference, 2) IN ({transType}) AND DATE(date) = '{dates}'; 
                                UPDATE paiwi SET extracted = 'Y' WHERE LEFT(reference, 2) = 'WS' AND DATE(date) = '{dates}'");

                    using (cmd = new MySqlCommand(sb.ToString(), conn))
                    {
                        cmd.CommandTimeout = 0;
                        //cmd.Parameters.AddWithValue("@trtype", transType);
                        //cmd.Parameters.AddWithValue("@dates", dates);

                        int result = cmd.ExecuteNonQuery();

                    }

                }


            }
            catch
            {
                throw;
            }
        }
    
    }

   
}
