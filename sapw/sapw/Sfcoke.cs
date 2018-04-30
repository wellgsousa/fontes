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
        //public string strConexao = ConfigurationManager.ConnectionStrings["ConexaoSQLSFCoke_Homolog"].ConnectionString;
        public string strConexaoDW = ConfigurationManager.ConnectionStrings["ConexaoDWBRASAL"].ConnectionString;
        
        public DataSet consultar_pedidos_sfcoke(string centro, string codvend, string codcli,string n_pedido, string data_i, string data_f)
        {
            String sql = "";
            String where = "";

            SqlConnection con = new SqlConnection(strConexao);

            if (!n_pedido.Trim().Equals(""))
            {
                where += " C.NUPED = '" + n_pedido + "'";
            }
            else
            {

                if (!codvend.Trim().Equals(""))
                {
                    where += " c.CDEQP = '" + codvend + "' AND ";
                }

                if (!codcli.Trim().Equals(""))
                {
                    where += " CONVERT(INT,c.CDCL) = '" + codcli + "' AND ";
                }

                if (!centro.Trim().Equals(""))
                {
                    where += " C.CDUNID = '" + centro + "' AND ";
                }

                if(!data_i.Trim().Equals("") && !data_f.Trim().Equals(""))
                {
                    where += " CONVERT(VARCHAR, C.DTPROC, 112) >= '" + data_i + "' AND CONVERT(VARCHAR, C.DTPROC, 112) <= '" + data_f + "' ";
                }

                

            }

                
           
            sql = "SELECT                                                           "+
                           "C.CDUNID,                                               "+
                          " C.CDCL,                                                 "+
                          " CL.DSRAZ,                                              "+
                          " C.CDEQP,                                                "+
                          " C.CDCFO,                                                "+
                          " I.CDPROD,                                               "+
                          " P.DSNOME,                                               "+
                          " I.QTDNEG,                                               "+
                          " I.VLVEND,                                               "+   
                          " CONVERT(CHAR(10), C.DTPROC, 103) as DATA,               "+
                          " CONVERT(VARCHAR(11),C.DTPROC,114) AS HORA_PROCESSADO,   "+
                          "       CASE C.IDTXT                                      "+
                          "              WHEN '1' THEN 'PROCESSADO SAP'             "+
                          "              WHEN '0' THEN 'NÃO PROCESSADO SAP'         "+
                          "       END AS EXPORTADO  ,                               "+  
                          " C.NUPED                                                 "+
                "FROM TAFCAB C                                                      "+
                "JOIN TAFITE I ON I.NUPED = C.NUPED                                 "+
                "JOIN TAFCLI CL ON CL.CDCL = C.CDCL                                 "+
                "JOIN TAFPRODUTOS P ON P.CDPROD=I.CDPROD                            "+
                "Where                                                              "+
                "C.CDUNID <> '99999' AND                                            "+
                where +
                "Order BY C.DTPROC, CL.DSFANT";
           
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



        public DataSet consultar_pedidos_dia_sfcoke(string data, string gerencia, string coordenador, string vendedor)
        {
            String sql = "";
            String where = " C.CDUNID <> '99999' ";

            SqlConnection con = new SqlConnection(strConexao);

            if (!gerencia.Trim().Equals(""))
            {
                where += " AND left(C.CDROTA,1) = '" + gerencia + "'";
            }


            if (!coordenador.Trim().Equals(""))
            {
                where += " AND left(C.CDROTA,3) = '" + coordenador + "'";
            }


            if (!vendedor.Trim().Equals(""))
            {
                where += " AND C.CDROTA = '" + vendedor + "'";
            }
           

            if (data.Trim().Equals("") )
            {
                where += " AND  CONVERT(VARCHAR, C.DTPROC, 112) =CONVERT(VARCHAR, getdate(), 112)";
            }
            else
            {
                where += " AND  CONVERT(VARCHAR, C.DTPROC, 112) ='" + data + "'";
            }
            

            sql = "SELECT                                                   " +
                  "      C.NUPED,                                           " +
                  "      C.CDCL,                                            " +
                  "      CL.DSFANT,                                         " +
                  "      C.CDEQP,                                           " +
                  "      C.DTFECH,                                          " +
                  "      C.VLTPED,                                          " +
                  "      C.CDCFO,                                           " +
                  "      C.DTPROC,                                          " +
	              "      CASE IDTXT                                         " +                                     
                  "                WHEN '1' THEN 'PROCESSADO SAP'           " +          
                  "                WHEN '0' THEN 'NÃO PROCESSADO SAP'       " +      
                  "       END AS EXPORTADO                                  " +
                  "  FROM TAFCAB  C                                         " +
                  "  JOIN  TAFCLI CL ON CL.CDCL = C.CDCL                    " +
                  "  WHERE  " + where +
                  "  ORDER BY CL.DSFANT";

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


        public DataSet descargas_sfcoke(string data)
        {
            String sql = "";
            String where = "";

            SqlConnection con = new SqlConnection(strConexao);

            if (!data.Trim().Equals(""))
            {
                where += " C.NUPED = '" + data + "'";
            }


            sql = "SELECT '00:06 - 09:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS," +
                  "         (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '00:00:00' AND CONVERT(VARCHAR, DTPROC,114) <= '08:59:59' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE   CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '00:00:00' AND CONVERT(VARCHAR(11), DTPROC,114) <= '08:59:59'" +
                  " UNION   " +
                  "SELECT '09:00 - 10:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "         (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '09:00:00' AND CONVERT(VARCHAR(11), DTPROC,114) <= '10:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE   CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '09:00:00' AND CONVERT(VARCHAR(11), DTPROC,114) <= '10:00:00'" +
                  " UNION   "+
                  "SELECT '10:00 - 11:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS, "+
                  "         (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '10:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '11:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '10:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '11:00:00'" +
                  " UNION   "+
                  "SELECT '11:00 - 12:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "        (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '11:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '12:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '11:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '12:00:00'" +
                  " UNION   "+
                  "SELECT '12:00 - 13:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "        (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '12:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '13:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '12:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '13:00:00'" +
                  " UNION   "+
                  "SELECT '13:00 - 14:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "       (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '13:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '14:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '13:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '14:00:00'" +
                  " UNION   "+
                  "SELECT '14:00 - 15:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "      (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '14:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '15:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '14:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '15:00:00'" +
                  "  UNION  "+
                  "SELECT '15:00 - 15:30' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "      (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '15:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '15:30:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '15:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '15:30:00'" +
                  "  UNION  "+
                  "SELECT '15:30 - 16:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "     (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '15:30:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '16:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '15:30:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '16:00:00'" +
                  " UNION   " +
                  "SELECT '16:00 - 16:15' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "     (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '16:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '16:15:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '16:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '16:15:00'" +
                  " UNION   "+
                  "SELECT '16:15 - 17:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "     (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '16:15:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '17:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '16:15:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '17:00:00'" +
                  " UNION   "+
                  "SELECT '17:00 - 18:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "     (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '17:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '18:00:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '17:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '18:00:00' " +
                  " UNION   "+
                  "SELECT '18:00 - 22:00' AS PERIODO , COUNT(NUPED) AS TOTAL_PEDIDOS,"+
                  "     (SELECT  COUNT(NUPED)  FROM TAFCAB (NOLOCK) WHERE CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '18:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '23:59:00' AND IDTXT = '1') AS PROCESSADOS " +
                  "FROM TAFCAB (NOLOCK) WHERE  CONVERT(VARCHAR, DTPROC, 112) = '" + data + "' AND CONVERT(VARCHAR(11), DTPROC,114) >= '18:00:01' AND CONVERT(VARCHAR(11), DTPROC,114) <= '23:59:00' ";
                  

                   

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




        public String verificaSeTemErro()
        {
            String result = null;

            SqlConnection con = new SqlConnection(strConexao);

            //string sql = "select count(*) as erros from TMCTR  where  CONVERT(nvarchar,dtproc,103) =   CONVERT(nvarchar,getdate(),103) and cdestr in(5,6)";
            string sql = "select count(*) as erros from TMCTR  where  CONVERT(nvarchar,DTINSR,103) =    CONVERT(nvarchar,getdate(),103) and cdestr in(5,6)";
            //string sql = "select * from TAFMCTR  where CONVERT(nvarchar,dtproc,103) = CONVERT(nvarchar,getdate(),103) and CAST(dtproc AS TIME) BETWEEN '03:00' and '05:59'  and cdestr in(5,6)";        
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            
            while (dr.Read())
            {
                result = dr["erros"].ToString();
            }

            return result;


        }


        public DataSet lista_cargas()
        {
            String sql = "";
            
            SqlConnection con = new SqlConnection(strConexao);

          //  sql = "select M.CDTRF,M.DTINSR,M.DTPROC,M.DSRES,M.DSTRF,S.DSESTR from TAFMCTR  M "+
          //        "JOIN TAFMCESTR S ON M.CDESTR=S.CDESTR  where  CONVERT(nvarchar,dtproc,103) =  convert(nvarchar, getdate(),103) order by M.DTPROC desc";

            sql = "select M.CDTRF,M.DTINSR,M.DTFIPR AS DTPROC,M.DSRES,M.DSTRF,S.DSESTR from TMCTR  M " +
                 " JOIN tmcestrlg S ON M.CDESTR=S.CDESTR  where  CONVERT(nvarchar,DTINSR,103) =   CONVERT(nvarchar,getdate(),103) order by M.DTINSR desc";
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

        public DataSet lista_erros_cargas()
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);

            //sql = "select * from TAFMCTR  where  CONVERT(nvarchar,dtproc,103) =  '08/03/2016' and cdestr in(5,6)";

            sql = "select *,DTFIPR as DTPROC from TMCTR  where  CONVERT(nvarchar,DTINSR,103) =  CONVERT(nvarchar,getdate(),103) and cdestr in(5,6)";

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



        public DataSet verificaNivelIsolamentoSQLServer()
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);

            //  sql = "select M.CDTRF,M.DTINSR,M.DTPROC,M.DSRES,M.DSTRF,S.DSESTR from TAFMCTR  M "+
            //        "JOIN TAFMCESTR S ON M.CDESTR=S.CDESTR  where  CONVERT(nvarchar,dtproc,103) =  convert(nvarchar, getdate(),103) order by M.DTPROC desc";

            sql = "dbcc useroptions";

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


        public DataSet verificaProcessosAtivosSQLSever()
        {
            string hostname = ConfigurationManager.AppSettings["servidorParaVerProcessosAtivos"];
            
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);            

            sql = "SELECT * FROM master..sysprocesses  where loginame = 'sfcokeservice' and   hostname = '"+hostname+"'";

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


        public DataSet verificaUltimoPedidoProcessadoSAP()
        {  
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);

            sql = "SELECT top 1 'PROCESSADO' AS TIPO, DTPROC," +
		                  "RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(HOUR, DTPROC,  getdate()) % 24), 2) + ':'"+
	                      " + RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(MINUTE, DTPROC,  getdate()) % 60), 2) + ':'" +
	                      " + RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(SECOND, DTPROC,  getdate()) % 60), 2) AS tempo " +
	                      " FROM tafcab (nolock) " +
	                      " WHERE idtxt=1 AND CONVERT(VARCHAR,DTPROC,112) = CONVERT(VARCHAR, getdate(),112) " + 
	                      " ORDER BY DTPROC DESC";

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

        public DataSet verificaUltimoPedidoRecebido()
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);

            sql = "SELECT top 1 'RECEBIDO' AS TIPO, DTPROC," +
                          "RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(HOUR, DTPROC,  getdate()) % 24), 2) + ':'" +
                          " + RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(MINUTE, DTPROC,  getdate()) % 60), 2) + ':'" +
                          " + RIGHT('00' + CONVERT(VARCHAR, DATEDIFF(SECOND, DTPROC,  getdate()) % 60), 2) AS tempo " +
                          " FROM tafcab (nolock) " +
                          " WHERE CONVERT(VARCHAR,DTPROC,112) = CONVERT(VARCHAR, getdate(),112) " +
                          " ORDER BY DTPROC DESC";

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

        public String verificaOciosidadeCargaSFCoke(Int32 limite_max_gerar_carga)
        {
            String result = "F";

            SqlConnection con = new SqlConnection(strConexao);

            
            string sql = "SELECT CDMODL,DSCRON,CONVERT(VARCHAR(10),GETDATE(),120) + ' ' + DSHORA + ':' + DSMIN + ':00' AS DT_AGENDAMENTO," +
                         "       DATEDIFF(hour,(CONVERT(VARCHAR(10),GETDATE(),120) + ' ' + DSHORA + ':' + DSMIN + ':00'),GETDATE()) AS TEMPO " +
                         "FROM " +
                                "TMCTRAG (nolock)" +
                         "WHERE " +
                         "     IDATIV=1 AND" +
                         "       CDMODL IN " +
                         "       (         " +
                         "        'AC3460D2-D64C-456A-8642-B5877A2F2BA2'," +
                         "        'A9D66DC0-2AAB-4510-8054-836294A6E3BF'," +
                         "        'B59A06A8-124F-4E5F-85AD-6A737EA6672A'," +
                         "        '1C12B863-3A1C-42AB-B9B9-F7BD0D9026C6'," +
                         "        '4FE3E1D7-2776-4C5C-A227-B511FFA813E2'," +
                         "        'E3EB95C3-0E6A-4AAD-8485-0FFCAD19EBE0'," +
                         "        '231AD93B-B044-4B4D-A947-91CCF509D416'" +
                         "       );";
                         
           con.Open();

           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                string sql2 = "";

                int cont = 0;
                //Roda cada agendamento encontrado
                foreach (Object ob in dr)
                {
                    cont++;

                   
                    //Verifica se o tempo já ultrapassou ao limite médio de uma carga do SFCoke para ser gerada
                    if (Convert.ToInt16(dr["TEMPO"].ToString()) > limite_max_gerar_carga)
                    {
                        if (cont <= 1) { result = "T"; }

                        SqlConnection con2 = new SqlConnection(strConexao);

                        con2.Open();
                        //Verifica se a carga do agendamento acima rodou
                        sql2 = "select * from TMCTR  (nolock) WHERE CDMODL = 'a" + dr["CDMODL"].ToString() + "' AND CONVERT(CHAR(10),DTINSR,102) = CONVERT(CHAR(10),GETDATE(),102)";

                        SqlCommand cmd2 = new SqlCommand(sql2, con2);
                        SqlDataReader dr2 = cmd2.ExecuteReader();

                        //Caso não encontre a carga na tabela
                        if (!dr2.HasRows)
                        {
                            //Guarda o agendamento que não rodou para retornar
                            result += dr["CDMODL"].ToString() + ';' + dr["DSCRON"].ToString() + ';' + dr["DT_AGENDAMENTO"].ToString() + ';' + dr["TEMPO"].ToString() + '|';
                        }

                        con2.Close();
                    }
                    
                }
            }

            return result;


        }

        public DataSet verificaVendedoresSemDescarga()
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);

            sql = "SELECT E.CDUNID, E.CDEQP, " +
                  "(SELECT COUNT (DISTINCT(CDCL)) FROM TAFCAB C WHERE  CONVERT(CHAR(10), C.DTPROC, 103) = '03/08/2016' AND C.CDEQP = E.CDEQP) AS CLIENTES" +
                  " FROM TAFEQP E  (nolock) " +
                  " WHERE  " +
                  "(SELECT COUNT (DISTINCT(CDCL)) FROM TAFCAB C WHERE  CONVERT(CHAR(10), C.DTPROC, 103) = '03/08/2016' AND C.CDEQP = E.CDEQP) = 0 and  " +
                  "E.CDEQP NOT IN ('0736', '2010', '2016', '2017', '2027', '2028', '3011', '3012', '3013', '3021', '3022', '3023', '3031',  " +
                  "'3032', '3033', '3041', '3042', '3043', '3044', '3045', '3046', '4501', '7001', '8011', '8012', '8013',  " +
                  "'8014', '8015', '8016', '8017', '8021', '8022', '8023', '8024', '8025', '8026', '8031', '8032', '8033',  " +
                  "'8041', '8042', '8043', '8044', '8045', '8046', '9001', '9999', '0189');  ";

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


//Verificar Divergências de cargas de clientes SAP
        //TAFCLI
        public DataSet VerificaDivergenciaCargaSAPClientesTAFCLI()
        {
            string sql;

            SqlConnection con = new SqlConnection(strConexaoDW);

            sql = "select A.CDCL,B.CDCL,A.DT_ATUALIZACAO,B.DT_ATUALIZACAO" +
                  "  from BI_SFCOKE_TAFCLI_MANHA A                       " +
                  "  left join BI_SFCOKE_TAFCLI B                        " +
                  "  ON  B.CDCL = A.CDCL                                 " +
                  "  WHERE B.CDCL IS NULL OR A.CDCL IS NULL              ";

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

        //TAFVIS
        public DataSet VerificaDivergenciaCargaSAPClientesTAFVIS()
        {
            string sql;

            SqlConnection con = new SqlConnection(strConexaoDW);

            sql = "SELECT A.CDCL,B.CDCL,A.DT_ATUALIZACAO,B.DT_ATUALIZACAO, A.CDROTA,B.CDROTA " +
                  "  FROM BI_SFCOKE_TAFVIS_MANHA A                                           " +
                  " LEFT JOIN BI_SFCOKE_TAFVIS  B ON B.CDCL = A.CDCL                         " +
                  "  WHERE B.CDCL IS NULL";

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

        //TAFVIS
        public DataSet VerificaDivergenciaCargaSAPClientesTAFCLIROTA()
        {
            string sql;

            SqlConnection con = new SqlConnection(strConexaoDW);

            sql = "SELECT A.CDCL,B.CDCL,A.DT_ATUALIZACAO,B.DT_ATUALIZACAO, A.CDROTA,B.CDROTA" +
                  " FROM BI_SFCOKE_TAFCLIROTA_MANHA A                                       " +
                  " LEFT JOIN BI_SFCOKE_TAFCLIROTA  B ON B.CDCL = A.CDCL                    " +
                  " WHERE B.CDCL IS NULL";

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


        public DataSet lista_pedidos_Nprocess_cliente(string codcli, string dias_pedido)
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);
          
            sql = "SELECT NUPED,DTPROC, DATEDIFF (dd , DTPROC , GETDATE() ) as DIAS, CDCFO"+
                  "  FROM TAFCAB WHERE IDTXT = 0 AND DTPROC > (GETDATE() - " + dias_pedido + ") AND CONVERT(INT,CDCL) = '"+codcli+"'";
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


        public DataSet lista_itens_pedidos_Nprocess_cliente(string nuped, string dias_pedido)
        {
            String sql = "";

            SqlConnection con = new SqlConnection(strConexao);
          
            sql = "SELECT I.NUPED, I.CDCFO,I.CDPROD,I.CDUNPR, I.CDTAB, I.CDPROM, I.CDJUST, J.DSJUST, I.VLVEND,I.VLVDUN, "+
                  "I.VLTOT,I.VLPRZO, I.VLCUST, I.VLDIG, I.CDCATG, C.CDCL, C.CDEQP, C.DTFECH, C.DTPROC, C.VLTPED,P.DSNOME, " +
                  "I.QTDNEG, getdate() as 'DT_ATUALIZACAO' "+
                  " FROM TAFITE I (NOLOCK) "+
                  " JOIN TAFCAB C (NOLOCK)ON C.NUPED = I.NUPED " +
                  " LEFT JOIN TAFJUST J (NOLOCK) " +
                  " ON J.CDJUST = I.CDJUST " +
                  " JOIN TAFPRODUTOS P (NOLOCK)  " +
                  " ON P.CDPROD = I.CDPROD " +
                  " WHERE C.IDTXT = 0 AND C.NUPED ='" + nuped + "' AND DTPROC > (GETDATE() - " + dias_pedido + ") "; 
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