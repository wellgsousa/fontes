using System;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace sapw
{
    public class Frotaweb
    {
        public string strConexao = ConfigurationManager.ConnectionStrings["ConexaoSQLFrotaweb"].ConnectionString;
       
        /*Lista funcionários*/
        public DataSet consultar_funcionarios(string unidade,string matricula)
        {
            String sql = "";
            SqlConnection con = new SqlConnection(strConexao);

            if (string.IsNullOrEmpty(matricula))
            {
                sql = "select * from funcionarios";
            }
            else
            {
                sql = "select *,1 AS TrataAcao from funcionarios WHERE matricula=" + matricula + " AND est=" + unidade;
            }
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

        /*Lista notas de combustível*/
        public DataSet consultar_estoqueCombustivel(string data_entrada)
        {
            String sql = "";
            SqlConnection con = new SqlConnection(strConexao);

            if (string.IsNullOrEmpty(data_entrada))
            {
                sql = "select * from Entrada_combustivel";
            }
            else
            {
                sql = "select * from Entrada_combustivel WHERE DATA_ENTRADA='" + data_entrada + "'";
            }
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