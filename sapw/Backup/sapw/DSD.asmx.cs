using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Data;
using SAP.Middleware.Connector;

namespace sapw
{
    /// <summary>
    /// Summary description for DSD
    /// </summary>
    [WebService(Namespace = "http://brasalrefrigerantes.dsd/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class DSD : System.Web.Services.WebService
    {

   /*     [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }*/

     /*   [WebMethod]
        public String testeGuilhermePontual(string USERNAME)
        {
            Vales ObjVales = new Vales();
            return ObjVales.TesteGuilherme(USERNAME);
        }*/

     /*   [WebMethod(Description = "Chama o cliente pela BAPI")]
        public string pega_clientes(string codcli )
        {
            Vales ObjVales = new Vales();

            return ObjVales.chamaBAPI(codcli); 
        }*/

        [WebMethod(Description = "Consulta Clientes CPF/CNPJ")]
        public DataTable CONSULTA_CLIENTES(string codigo_cliente)
        {
            Rfc ObjRfc_RFC_GET_TABLE = new Rfc();
            return ObjRfc_RFC_GET_TABLE.consultaClienteSAP(codigo_cliente);
        }

        [WebMethod(Description = "Consulta tabelas do SAP:")]
        public DataTable RFC_GET_TABLE(string tabelaSap)
        {
            Rfc ObjRfc_RFC_GET_TABLE = new Rfc();
            return ObjRfc_RFC_GET_TABLE.consultaTabelaGenerica(tabelaSap);
        }

        [WebMethod(Description = "Bloqueia/Desbloqueia cliente conforme consulta cadastro SEFAZ")]
        public string SEFAZ_CLIENTE_SAP(string CNPJ, string CENTRO, string BLOQUEIO)
        {
            Rfc ObjSAP = new Rfc();

            return ObjSAP.SEFAZ_CLIENTE_SAP(CNPJ, CENTRO, BLOQUEIO);
        }

        [WebMethod(Description = "Lista Clientes SAP por parametros multiplos")]
        public DataTable LISTA_CLIENTES(string COD_CLI, string NOME, string ENDERECO, string BAIRRO, string CNPJ, string TELEFONE, string ROTA)
        {
            Rfc ObjSAP = new Rfc();

            return ObjSAP.listaClientesSAP(COD_CLI, NOME, ENDERECO, BAIRRO, CNPJ, TELEFONE, ROTA);
        }

        [WebMethod(Description = "Pega a forma de Pagamento do Cliente")]
        public string PEGA_FORMA_PAGTO_CLIENTE(string COD_CLIENTE)
        {
            Rfc ObjSAP = new Rfc();

            return ObjSAP.consultaFormaPagamentoCliente(COD_CLIENTE);
        }

        [WebMethod(Description = "Pega o vendedor pelo código de vendedor SAP")]
        public DataTable CONSULTA_VENDEDOR(string VENDEDOR_CODSAP)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.consultaVendedor(VENDEDOR_CODSAP);
        }

        [WebMethod(Description = "Consulta os dias de visitas no cliente pela rota")]
        public DataTable CONSULTA_DIAS_VISITA_NO_CLIENTE(string ROTA)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.consultaDiasDeVisitaNoCliente(ROTA);
        }

        [WebMethod(Description = "Consulta faturas em aberto do cliente")]
        public DataTable CONSULTA_FATURAS_EMABERTO_CLIENTE(string COD_CLIENTE)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.consultaFaturasEmAbertoCliente(COD_CLIENTE);
        }

        [WebMethod(Description = "Lista Pedidos do Vendedor")]
        public DataTable CONSULTA_PEDIDOS_VENDEDOR(string COD_VENDEDOR,string DT_INICIO,string DT_FINAL)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.listaPedidosVendedor(COD_VENDEDOR, DT_INICIO, DT_FINAL);
        }

        [WebMethod(Description = "Lista Compras do Cliente")]
        public DataTable CONSULTA_COMPRAS_CLIENTE(string COD_CLIENTE)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.listaComprasCliente(COD_CLIENTE);
        }

        [WebMethod(Description = "Lista Itens Cancelados/Retornados do Cliente")]
        public DataTable CONSULTA_ITENS_CANCELADOS_RETORNADOS(string COD_CLIENTE,string TRAZER_CANCELADOS, string TRAZER_RETORNADOS)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.listaItensCanceladosRetornadosClientes(COD_CLIENTE, TRAZER_CANCELADOS, TRAZER_RETORNADOS);
        }

        [WebMethod(Description = "Incluir tripulantes no documento de transportes. Passando os ajudantes vazio: APAGA; Motorista é um campo obrigatório")]
        public DataTable SCD_ATUALIZAR_TRIPULANTES(string DATA_ENTREGA, string CARGA, string MOTORISTA, string AJUDANTE1, string AJUDANTE2, string AJUDANTE3, string AJUDANTE4)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_ATUALIZAR_TRIPULANTES(DATA_ENTREGA, CARGA, MOTORISTA, AJUDANTE1, AJUDANTE2, AJUDANTE3, AJUDANTE4);
        }

        [WebMethod(Description = "Pegar os clientes da carga e suas cubagens")]
        public DataTable SCD_CUBAGEM_CLIENTES_DACARGA(string DATA_ENTREGA, string CARGA)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_CUBAGEM_CLIENTES_DACARGA(DATA_ENTREGA, CARGA);
        }

        [WebMethod(Description = "Verifica se o veículo existe no SAP")]
        public DataTable SCD_VERIFICA_VEICULO_EXISTE_SAP(string NUM_FROTA)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_VERIFICA_VEICULO_EXISTE_SAP(NUM_FROTA);
        }

        [WebMethod(Description = "Verifica o Status do Documento de transporte")]
        public DataTable SCD_VERIFICA_STATUS_TRANSPORTE(string DOC_TRANSPORTE)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_VERIFICA_STATUS_TRANSPORTE(DOC_TRANSPORTE);
        }

        [WebMethod(Description = "Verifica o Status do Documento de transporte")]
        public DataTable SCD_VERIFICA_CAMPO_DE_AUXILIAR_DISPONIVEL_TRANSP(string DOC_TRANSPORTE)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_VERIFICA_CAMPO_DE_AUXILIAR_DISPONIVEL_TRANSP(DOC_TRANSPORTE);
        }

        [WebMethod(Description = "Localiza um funcionário pela matrícula")]
        public DataTable SCD_BUSCA_FUNCIONARIO(string CENTRO,string MATRICULA)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.SCD_PEGA_FUNCIONARIO(CENTRO,MATRICULA);
        }

        [WebMethod(Description = "Pega as cargas que ainda não sairam, conforme a data informada")]
        public DataTable SCD_PEGA_CARGAS_DISPONIVEIS_RECARGA_RAPIDA(string data_inicial, string Data_final)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.PegarCargasDisponiveisParaRecargaRapida(data_inicial,Data_final);
        }

        [WebMethod(Description = "Listar as cargas da data informada")]
        public DataTable SCD_PEGA_CARGAS_COMCUBAGEM_NA_DATA(string DATA, string CENTRO)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.listaCargasNaData(DATA, CENTRO);
        }

        [WebMethod(Description = "Listar os produtos dos pedidos do cliente que cairam por falta de estoque")]
        public DataTable LISTA_PEDIDOS_CAIRAM_FALTA_ESTOQUE(string COD_CLIENTE)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.listaProdutosClienteFaltaEstoque(COD_CLIENTE);
        }

        [WebMethod(Description = "Listar Gerencias")]
        public DataTable LISTA_GERENCIAS(string COD_GERENCIA)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.ListarGerencias(COD_GERENCIA);
        }

        [WebMethod(Description = "Listar Coordenadores")]
        public DataTable LISTA_COORDENADOR(string COD_COORDENADOR)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.ListaCoordenadores(COD_COORDENADOR);
        }

        [WebMethod(Description = "Listar as Bonificações de acordo com os parametros:Agrupar[G=Gerencia/C=Coordenador/V=Vendedor/T=Todos]")]
        public DataTable LISTA_BONIFICACOES(string AGRUPAR_POR, string VALOR, string DATA_INI, string DATA_FIM)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.bonificacoes(AGRUPAR_POR, VALOR, DATA_INI, DATA_FIM);
        }

        [WebMethod(Description = "Grava os produtos críticos do estoque (coloque códigos separados por vírgula)")]
        public DataTable INFORMAR_ITENS_CRITICOS(string CENTRO,string PRODUTOS)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.itensCriticos(CENTRO,PRODUTOS);
        }

        [WebMethod(Description = "Mostra os ítens do pedido)")]
        public DataTable ITENS_PEDIDO(string DOC_NUMERO)
        {
            Rfc ObjSAP = new Rfc();
            return ObjSAP.itensPedido(DOC_NUMERO);
        }

        [WebMethod(Description = "Visualiza pedidos do SFCoke")]
        public DataSet Pedidos_SFCoke(string CENTRO, string COD_VEND, string COD_CLI, string NUM_PEDIDO, string DATA_I, string DATA_F)
        {
            Sfcoke ObjSfcoke = new Sfcoke();
            return ObjSfcoke.consultar_pedidos_sfcoke(CENTRO, COD_VEND, COD_CLI,NUM_PEDIDO, DATA_I, DATA_F);
        }
        
       
               
    }
}
