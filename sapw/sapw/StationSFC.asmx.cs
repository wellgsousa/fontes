using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;

namespace sapw
{
    /// <summary>
    /// Summary description for StationSFC
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class StationSFC : System.Web.Services.WebService
    {

        [WebMethod(Description = "Visualiza pedidos do SFCoke")]
        public DataSet Pedidos_SFCoke(string CENTRO, string COD_VEND, string COD_CLI, string NUM_PEDIDO, string DATA_I, string DATA_F)
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.consultar_pedidos_sfcoke(CENTRO, COD_VEND, COD_CLI, NUM_PEDIDO, DATA_I, DATA_F);
        }

        [WebMethod(Description = "Visualiza pedidos do dia SFCoke")]
        public DataSet Pedidos_SFCoke_Dia(string GERENCIA, string COORDENADOR, string VENDEDOR, string DATA)
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.consultar_pedidos_dia_sfcoke( DATA, GERENCIA, COORDENADOR, VENDEDOR);
        }

        [WebMethod(Description = "Verifica cargas com erros no SFCoke")]
        public String Erro_cargas_SFCoke()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.verificaSeTemErro();
        }

        [WebMethod(Description = "Lista erros cargas do SFCoke")]
        public DataSet Lista_erros_cargas_SFCoke()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.lista_erros_cargas();
        }

        [WebMethod(Description = "Lista cargas do SFCoke do dia")]
        public DataSet Lista_cargas_SFCoke()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.lista_cargas();
        }

        [WebMethod(Description = "Verifica cargas não geradas/ociosas")]
        public String VerificaCargasSFCokeNaoGeradas(Int32 Tempo_Max_gerar_carga)
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.verificaOciosidadeCargaSFCoke(Tempo_Max_gerar_carga);
        }

        [WebMethod(Description = "Verifica Divergência de Cargas Cliente SAP TAFCLI")]
        public DataSet Verifica_Divergencia_Carga_SAP_Clientes_TAFCLI()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.VerificaDivergenciaCargaSAPClientesTAFCLI();
        }

        [WebMethod(Description = "Verifica Divergência de Cargas Cliente SAP TAFVIS")]
        public DataSet Verifica_Divergencia_Carga_SAP_Clientes_TAFVIS()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.VerificaDivergenciaCargaSAPClientesTAFVIS();
        }

        [WebMethod(Description = "Verifica Divergência de Cargas Cliente SAP TAFCLIROTA")]
        public DataSet Verifica_Divergencia_Carga_SAP_Clientes_TAFCLIROTA()
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.VerificaDivergenciaCargaSAPClientesTAFCLIROTA();
        }
    }
}
