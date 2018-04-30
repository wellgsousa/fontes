using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Xml;


namespace sapw
{
    /// <summary>
    /// Summary description for canalclientes
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class canalclientes : System.Web.Services.WebService
    {

        [WebMethod]        
        public XmlDocument CONSULTA_STATUS_PEDIDOS(string CODIGO_CLIENTE, string DIAS_PEDIDO)
        {
            Rfc objCanal = new Rfc();
            return objCanal.statusPedidosCanalClientes(CODIGO_CLIENTE, DIAS_PEDIDO);
        } 

        [WebMethod]        
        public DataSet SFCOKE_PEDIDOS_N_PROCESSADO(string CODIGO_CLIENTE, string DIAS_PEDIDO)
        {
            Sfcoke objCanal = new Sfcoke();
            return objCanal.lista_pedidos_Nprocess_cliente(CODIGO_CLIENTE, DIAS_PEDIDO);
        } 

        [WebMethod]
        public DataSet SFCOKE_ITENS_PEDIDOS_N_PROCESSADO(string NUMERO_PEDIDO, string DIAS_PEDIDO)
        {
            Sfcoke objCanal = new Sfcoke();
            return objCanal.lista_itens_pedidos_Nprocess_cliente(NUMERO_PEDIDO, DIAS_PEDIDO);
        }


        
 

        
    }
}
