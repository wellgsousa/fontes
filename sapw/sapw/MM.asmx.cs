using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;

namespace sapw
{
    /// <summary>
    /// Summary description for MM
    /// </summary>
    [WebService(Namespace = "http://brasalrefrigerantes.mm/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class MM : System.Web.Services.WebService
    {

        [WebMethod(Description = "Mostra as Reservas do Almoxarifado (Tabela: 1 - RETORNO | 2 - TOTAL)")]
        public DataTable MM_RESERVAS_ALMOX(string CENTRO, string DATA_RESERVA, string TABELA)
        {
            Rfc ObjSRfc = new Rfc();
            return ObjSRfc.MM_MONITOR_ALMOX(CENTRO, DATA_RESERVA, TABELA);
        }
    }
}
