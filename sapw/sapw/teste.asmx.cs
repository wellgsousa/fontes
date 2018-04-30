using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;

namespace sapw
{
    /// <summary>
    /// Summary description for teste
    /// </summary>
    [WebService(Namespace = "http://brasalrefrigerantes.teste/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class teste : System.Web.Services.WebService
    {
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string RecebeMostraParamentros(string nome, string texto)
        {
            return nome + " (" + texto + ")";
        }

        [WebMethod]
        public DataTable SolicitacaoFechadas(string area)
        {
            Vales objVales = new Vales();
            return objVales.SolicitaFechadas(area);
        }

        [WebMethod]
        public String CriaOrdem(string ordem, string material, string dti, string dtf, string hri, string hrf, string qtd, string versao)
        {
            Vales objSAP = new Vales();
            return objSAP.CriaOrdem(ordem, material, dti, dtf, hri, hrf, qtd, versao);
        }

        [WebMethod]
        public string VALES_CriarVale(string CODFUNCIONARIO, string CCUSTO, string VALOR, string CATEGORIA, string SUBCAT, string DESC)
        {
            Vales ObjVales = new Vales();
            return ObjVales.CriarVale(CODFUNCIONARIO, CCUSTO, VALOR, CATEGORIA, SUBCAT, DESC);
        }
        [WebMethod]
        public String testeGuilhermePontual(string USERNAME)
        {
            Vales ObjVales = new Vales();
            return ObjVales.TesteGuilherme(USERNAME);
        }

        [WebMethod]
        public string testeSamuel()
        {
            Vales ObjVales = new Vales();
            return ObjVales.TesteSamuel();
        }

        [WebMethod]
        public DataTable testeSamuel2(string BUKRS, string GJAHR, string MONAT, string HKONT)
        {
            Vales ObjVales = new Vales();
            return ObjVales.TesteSamuel_2(BUKRS, GJAHR, MONAT, HKONT);
        }

        [WebMethod]
        public void testeSamuel_teste_altera_clientes()
        {
            Vales ObjVales = new Vales();
            ObjVales.teste_altera_clientes();
        }

        [WebMethod]
        public void testeCalcMiniSap()
        {
            Vales ObjVales = new Vales();
            ObjVales.testeRFCminiSap();
        }

        

       
    }
}
