using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace STRAINER_EXTRACT.Service
{
    public class MySQLHelper 
    {

        private MySqlConnection conn;
        private MySqlCommand cmd;
        
        


        private string connectionString = Properties.Settings.Default.DB;
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
                        //if your quer/script is stored procedure use the code below
                        //cmd.CommandType = CommandType.StoredProcedure;

                        //parameters
                        //cmd.Parameters.AddWithValue("@SpName", view.EmployeeId);

                        cmd.ExecuteNonQuery();
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
