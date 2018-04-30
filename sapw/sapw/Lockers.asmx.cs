using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace sapw
{
    /// <summary>
    /// Summary description for Lockers
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Lockers : System.Web.Services.WebService
    { 
        [WebMethod]
        public DataTable CRIAR_PEDIDO_VENDA(string COD_CLIENTE, string CONDICAO_PAGMTO, string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO, string TIPO_DOC, string TABELA_PRECO, string DEPOSITO, string NOME_CLIENTE, string CPF, string PRODUTOS, string ENDERECO)
        {
            Rfc objLocker = new Rfc();
            return objLocker.criarPedidosVendasLockers(COD_CLIENTE, CONDICAO_PAGMTO, ORG_VENDAS, CANAL_DIST, SETOR_ATV, CENTRO, TIPO_DOC, TABELA_PRECO, DEPOSITO,  NOME_CLIENTE, CPF, PRODUTOS, ENDERECO);
        }
    }
}
