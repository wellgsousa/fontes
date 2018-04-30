using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;

namespace sapw
{
    /// <summary>
    /// Summary description for FrotaService
    /// </summary>
    [WebService(Namespace = "http://brasalrefrigerantes.frotaservice/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class FrotaService : System.Web.Services.WebService
    {

        [WebMethod(Description = "Lista funcionários")]
        public DataSet FrotaWEB_Funcionarios(string unidade, string matricula)
        {
            Frotaweb ObjFrota = new Frotaweb();

            return ObjFrota.consultar_funcionarios(unidade, matricula);
        }

        [WebMethod(Description = "Lista notas de combustível do SAP")]
        public DataSet FrotaWEB_EntradaCombustivel(string data_entrada)
        {
            Frotaweb ObjFrota = new Frotaweb();
            return ObjFrota.consultar_estoqueCombustivel(data_entrada);
        }

        [WebMethod(Description = "Envia abastecimento do veículo para o SAP")]
        public string FrotaWEB_Abastece(string Equipamento, string Posto, string Data, string Hora, string Trajeto_consumo, string Consumo, string Posicao_contador)
        {
            Rfc_Frota objSAP = new Rfc_Frota();
            return objSAP.RegistraAbastecimento(Equipamento, Posto, Data, Hora, Trajeto_consumo, Consumo, Posicao_contador);
        }
         

        [WebMethod]
        public string FrotaWEB_CriaOrdens(string Tela1_Tipo_ordem, string Tela1_Equipamento, string Tela1_Centro_planej, string CabCentral_Descricao_ordem, string CabCentral_Inicio_base, string CabCentral_Fim_base, string CabCentral_Descricao_breve_ordem, string Operacoes_PM01, string Complementos_PM03,string Componente_L_Estoque, string Componente_N_Compra)
        {
            Rfc_Frota objFrotaweb = new Rfc_Frota();
            return objFrotaweb.CriaOrdem(Tela1_Tipo_ordem, Tela1_Equipamento, Tela1_Centro_planej, CabCentral_Descricao_ordem, CabCentral_Inicio_base, CabCentral_Fim_base, CabCentral_Descricao_breve_ordem, Operacoes_PM01, Complementos_PM03, Componente_L_Estoque, Componente_N_Compra);
        }

        [WebMethod]
        public string FrotaWEB_FechaOrdens(string NUM_ORDEM, string DESC_ORDEM, string DT_INICIO, string DT_FIM, string DT_REFER, string DESC_BREVE_ORDEM, string FECHA_ORDEM, string MODIFICA_ORDEM,string Excluir_Operacao,string Excluir_Componente, string Operacao_PM01, string Complemento_ChavePM03, string Componente_L, string Componente_N)
        {
            Rfc_Frota objFrotaweb = new Rfc_Frota();
            return objFrotaweb.FechaOrdem(NUM_ORDEM, DESC_ORDEM, DT_INICIO, DT_FIM, DT_REFER, DESC_BREVE_ORDEM, FECHA_ORDEM, MODIFICA_ORDEM,Excluir_Operacao,Excluir_Componente, Operacao_PM01, Complemento_ChavePM03, Componente_L, Componente_N);         
        }

        

        
    }
}
