using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;
using SAP.Middleware.Connector;

namespace sapw
{
    /// <summary>
    /// Summary description for FI
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class FI : System.Web.Services.WebService
    {

        [WebMethod(Description = "Grava FBCJ")]
        public DataTable VALES_LANCA_FBCJ(string DATA_DOC, string EMITENTE, string NUM_VALE, string TRANS_CONTA, string VALOR, string FORNEC_NF, string CCUSTO, string EMPRESA, string CENTRO, string ORIGEM)
        {
            Rfc ObjValesFBCJ = new Rfc();
            return ObjValesFBCJ.VALES_LANCA_FBCJ( DATA_DOC,   EMITENTE,  NUM_VALE,  TRANS_CONTA,  VALOR,  FORNEC_NF,  CCUSTO,  CENTRO, ORIGEM,EMPRESA);
        }

        [WebMethod(Description = "Grava F-43")]
        public DataTable VALES_LANCA_F43(string DATA_DOC, string MATRICULA, string NUM_VALE, string TRANS_CONTAB, string FORNEC_NF, string ORIGEM, string EMPRESA, string CENTRO, string CCUSTO, string VALOR, string DESC_VALE)
        {
            Rfc ObjValesFBCJ = new Rfc();
            return ObjValesFBCJ.VALES_LANCA_F43(DATA_DOC, MATRICULA, NUM_VALE, TRANS_CONTAB, FORNEC_NF, ORIGEM, CENTRO, CCUSTO, VALOR, DESC_VALE, EMPRESA);
        }
    }
}
