using System;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;


namespace sapw
{
    public class Sfcoke
    {
        public string strConexao = ConfigurationManager.ConnectionStrings["ConexaoSQLSFCoke"].ConnectionString;

        public DataSet consultar_pedidos_sfcoke(string centro, string codvend, string codcli,string n_pedido, string data_i, string data_f)
         {
            String sql = "";
            String where = "";

            SqlConnection con = new SqlConnection(strConexao);

              if(!codvend.Trim().Equals(""))
              {
                   where+= " c.CDEQP = '" + codvend + "' AND ";
              }

              if(!codcli.Trim().Equals(""))
              {
                  where += " CONVERT(INT,c.CDCL) = '" + codcli + "' AND ";
              }

              if (!n_pedido.Trim().Equals(""))
              {
                  where += " C.NUPED = '" + n_pedido + "' AND ";
              }

                
           
            sql = "SELECT                                                           "+
                           "C.CDUNID,                                               "+
                          " C.CDCL,                                                 "+
                          " CL.DSFANT,                                              "+
                          " C.CDEQP,                                                "+
                          " C.CDCFO,                                                "+
                          " I.CDPROD,                                               "+
                          " P.DSNOME,                                               "+
                          " I.QTDNEG,                                               "+
                          " I.VLVEND,                                               "+   
                          " CONVERT(CHAR(10), C.DTPROC, 103) as DATA,               "+
                          " CONVERT(VARCHAR(11),C.DTPROC,114) AS HORA_PROCESSADO,   "+
                          "       CASE C.IDTXT                                      "+
                          "              WHEN '1' THEN 'PROCESSADO SAP'                        "+
                          "              WHEN '0' THEN 'NÃO PROCESSADO SAP'                        " +
                          "       END AS EXPORTADO  ,                               "+  
                          " C.NUPED                                                 "+
                "FROM TAFCAB C                                                      "+
                "JOIN TAFITE I ON I.NUPED = C.NUPED                                 "+
                "JOIN TAFCLI CL ON CL.CDCL = C.CDCL                                 "+
                "JOIN TAFPRODUTOS P ON P.CDPROD=I.CDPROD                            "+
                "Where                                                              "+
                "C.CDUNID = '"+centro+"' AND                                        "+
                where +
                "CONVERT(CHAR(10), C.DTPROC, 103) >= '" + data_i + "' AND CONVERT(CHAR(10), C.DTPROC, 103) <= '" + data_f + "' "+
                "Order BY CL.DSFANT";
           
            DataSet dt = new DataSet();
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                da.Fill(dt);
                con.Close();
            }
            catch (Exception ex)
            {
                throw ex;
                //  MessageBox.Show(ex.Message);
            }

            return dt;
        }


    }
    
}