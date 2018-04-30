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
    }
}
