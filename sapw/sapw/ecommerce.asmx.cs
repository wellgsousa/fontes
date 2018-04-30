using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;

namespace sapw
{
    /// <summary>
    /// Summary description for ecommerce
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class ecommerce : System.Web.Services.WebService
    {

        [WebMethod]
        public DataTable CRIAR_PEDIDO_VENDA(string EMPRESA, string COD_CLIENTE, string CONDICAO_PAGMTO, string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO, string TIPO_DOC, string TABELA_PRECO,  string NOME_CLIENTE, string CPF, string PRODUTOS, string ENDERECO)
        {
            Rfc objEcommerce = new Rfc();
            return objEcommerce.criarPedidosVendasEcommerce(EMPRESA, COD_CLIENTE, CONDICAO_PAGMTO, ORG_VENDAS, CANAL_DIST, SETOR_ATV, CENTRO, TIPO_DOC, TABELA_PRECO, NOME_CLIENTE, CPF, PRODUTOS, ENDERECO);
        }

        [WebMethod]
        public DataTable CONSULTA_QUANTIDADE_ESTOQUE(string CENTRO, string COD_PRODUTO)
        {
            Rfc objEcommerce = new Rfc();
            return objEcommerce.verifica_quantidade_estoque_ecommerce(COD_PRODUTO, CENTRO);
        }

        [WebMethod]
        public DataTable CONSULTA_ECOMMERCE(string P_KAPPL, string P_KSCHL, string P_WERKS, string P_REGIO, string P_ZZKVGR1, string P_KOPOS, string P_LGORT, string P_DATAB, string P_PRECO, string MATNR)
        {
            Rfc objEcommerce = new Rfc();
            return objEcommerce.consultasEcommerce( P_KAPPL,  P_KSCHL,  P_WERKS,  P_REGIO,  P_ZZKVGR1,  P_KOPOS,  P_LGORT,  P_DATAB,  P_PRECO,  MATNR);
        }


        


    }
}
