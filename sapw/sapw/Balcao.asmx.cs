using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;

namespace sapw
{
    /// <summary>
    /// Summary description for Balcao
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class Balcao : System.Web.Services.WebService
    {

        [WebMethod]
        public DataTable CONSULTA_QUANTIDADE_ESTOQUE(string CENTRO,string COD_PRODUTO)
        {
            Rfc objBalcao=new Rfc();
            return objBalcao.verifica_quantidade_estoque(COD_PRODUTO, CENTRO);
        }

         [WebMethod]
        public DataTable CRIAR_PEDIDO_VENDA(string COD_CLIENTE, string CONDICAO_PAGMTO, string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO, string TIPO_DOC, string DEPOSITO, string NOME_CLIENTE, string CPF, string PRODUTOS)
        {
            Rfc objBalcao=new Rfc();
            return objBalcao.criarPedidosBalcaoVendas( COD_CLIENTE,  CONDICAO_PAGMTO, ORG_VENDAS,  CANAL_DIST,  SETOR_ATV,  CENTRO, TIPO_DOC,  DEPOSITO,  NOME_CLIENTE,  CPF,  PRODUTOS);
        }


        
    }
}
