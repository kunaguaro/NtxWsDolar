using Microsoft.Extensions.Configuration;
using NtxWsDolar.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtxWsDolar.Dal
{
    public class ScraperDal 
    {
        private string conStr;

        public ScraperDal(IConfiguration iconfiguration)
        {
            conStr = iconfiguration.GetConnectionString("Default");
        }

        public int AddNewScraperDate(ScraperDolar scraperDolar)
        {
            string sqlQry = "";
            sqlQry = string.Format("INSERT INTO dbo.ScraperDolar(FechaPagina,FechaProcesado,CambioDolar,ErrorDescripcion, FechaCreacion)VALUES('{0}','{1}',{2},'{3}','{4}')",
                scraperDolar.StrFechaPagina, scraperDolar.StrFechaProcesado, scraperDolar.CambioDolar, scraperDolar.ErrorDescripcion, scraperDolar.FechaCreacion);


            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd = new SqlCommand(sqlQry, con);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return 0;
                }
            }


        }
    }
}
