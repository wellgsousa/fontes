using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;
using SAP.Middleware.Connector;
using System.Data;

namespace sapw
{
    public class Vales
    {
        RfcRepository repo;
        RfcDestination dest;

        public Vales()
        {
            try
            {
                SAPConnect objcon = new SAPConnect();
                RfcDestinationManager.RegisterDestinationConfiguration(objcon);
                dest = RfcDestinationManager.GetDestination("PRD");
                repo = dest.Repository;
                RfcDestinationManager.UnregisterDestinationConfiguration(objcon);
            }
            catch (RfcCommunicationException e)
            {
                throw e;
            }
            catch (RfcLogonException e)
            {
                throw e;
            }
            catch (RfcAbapRuntimeException e)
            {
                throw e;
            }
            catch (RfcAbapBaseException e)
            {
                throw e;
            }


        }

        /*Exemplo de Envio para o SAP*/
        public string CriaOrdem(string ordem, string material, string dti, string dtf, string hri, string hrf, string qtd, string versao)
        {
            string detail;

            IRfcFunction companyBapi = repo.CreateFunction("Z_RFC_CRIA_OP");

            companyBapi.SetValue("ORDEM", ordem);
            companyBapi.SetValue("MATERIAL", material.PadRight(18, ' '));
            companyBapi.SetValue("START_DATE", Convert.ToDateTime(dti));
            companyBapi.SetValue("START_TIME", hri);
            companyBapi.SetValue("END_DATE", Convert.ToDateTime(dtf));
            companyBapi.SetValue("END_TIME", hrf);
            companyBapi.SetValue("QUANTITY", qtd);
            companyBapi.SetValue("PROD_VERSION", versao);

            companyBapi.Invoke(dest);
            detail = new string(companyBapi.GetCharArray("MENSAGEM"));

            RfcDestinationManager.UnregisterDestinationConfiguration(new SAPConnect());
            return detail;
        }

        /*Exemplo de Envio para o SAP*/
        public string CriarVale(string CODFUNCIONARIO, string CCUSTO, string VALOR, string CATEGORIA, string SUBCAT, string DESC)
        {
            string detail;           

            IRfcFunction companyBapi = repo.CreateFunction("Z_IMPORT_VALE");
          
          /*  companyBapi.SetValue("DTDOC", "2013-01-01");
            companyBapi.SetValue("TPDOC","KP");
            companyBapi.SetValue("EMPRESA","B001");
            companyBapi.SetValue("DTLANC","2013-01-01");
            companyBapi.SetValue("MOEDA","BRL");
            companyBapi.SetValue("REFER","TESTE");
            companyBapi.SetValue("CHLANC","30");
            companyBapi.SetValue("FORNEC","407");
            companyBapi.SetValue("MONTANT","1");
            companyBapi.SetValue("FILIAL","0001");
            companyBapi.SetValue("CONDPAGTO","F000");
            companyBapi.SetValue("DTBASE","2013-01-01");
            companyBapi.SetValue("CHLANC2","40");
            companyBapi.SetValue("CONTA","113103");
            companyBapi.SetValue("DTEFET","2013-01-01");
            companyBapi.SetValue("MENSAGEM", "SamuketePerobao");*/


            companyBapi.SetValue("CODFUNCIONARIO", CODFUNCIONARIO);
            companyBapi.SetValue("CCUSTO", CCUSTO);
            companyBapi.SetValue("VALOR", VALOR);
            companyBapi.SetValue("CATEGORIA", CATEGORIA);
            companyBapi.SetValue("SUBCAT", SUBCAT);
            companyBapi.SetValue("DESC", DESC);

            companyBapi.Invoke(dest);
            detail = new string(companyBapi.GetCharArray("MENSAGEM"));

            return detail;
        }

        /*Exemplo de recebimento do SAP*/
        public DataTable SolicitaFechadas(string area)
        {
            DataTable table = new DataTable("SolicitacoesFechadas");
            table.Columns.Add("NOTA");
            table.Columns.Add("DATA");
            table.Columns.Add("HORA");
            table.Columns.Add("USER");
            table.Columns.Add("DESC");


            IRfcTable tabela = executarTabela("ZRFC_PM_SAC_SAR_ENVIO", "NOTAS", area);
            DataRow row;
            foreach (IRfcStructure detail in tabela)
            {
                row = table.NewRow();
                row["NOTA"] = detail.GetString("NOTA");
                row["DATA"] = detail.GetString("DATA");
                row["HORA"] = detail.GetString("HORA");
                row["USER"] = detail.GetString("USER");
                row["DESC"] = detail.GetString("DESCR");
                table.Rows.Add(row);
            }
            return table;

        }

        public IRfcTable executarTabela(String nomeMetodo, String nomeTabela, string ParamValor)
        {          
            IRfcFunction metodo = repo.CreateFunction(nomeMetodo);
            metodo.SetValue("AREA", ParamValor);
            metodo.Invoke(dest);
            IRfcTable tabela = metodo.GetTable(nomeTabela);

            return tabela;
        }

        public string TesteGuilherme(string USERNAME)
        {
            string detail;

            IRfcFunction companyBapi = repo.CreateFunction("Z_RFC_TESTE_GAP");

            /*  companyBapi.SetValue("DTDOC", "2013-01-01");
              companyBapi.SetValue("TPDOC","KP");
              companyBapi.SetValue("EMPRESA","B001");
              companyBapi.SetValue("DTLANC","2013-01-01");
              companyBapi.SetValue("MOEDA","BRL");
              companyBapi.SetValue("REFER","TESTE");
              companyBapi.SetValue("CHLANC","30");
              companyBapi.SetValue("FORNEC","407");
              companyBapi.SetValue("MONTANT","1");
              companyBapi.SetValue("FILIAL","0001");
              companyBapi.SetValue("CONDPAGTO","F000");
              companyBapi.SetValue("DTBASE","2013-01-01");
              companyBapi.SetValue("CHLANC2","40");
              companyBapi.SetValue("CONTA","113103");
              companyBapi.SetValue("DTEFET","2013-01-01");
              companyBapi.SetValue("MENSAGEM", "SamuketePerobao");*/


            companyBapi.SetValue("USERNAME", USERNAME);                     

            companyBapi.Invoke(dest);
            detail = new string(companyBapi.GetCharArray("FIRSTNAME"));

            return detail;
        }


        public string TesteSamuel()
        {
            string detail;

            IRfcFunction companyBapi = repo.CreateFunction("Z_RFC_TIPO_DOC");

            /*  companyBapi.SetValue("DTDOC", "2013-01-01");
              companyBapi.SetValue("TPDOC","KP");
              companyBapi.SetValue("EMPRESA","B001");
              companyBapi.SetValue("DTLANC","2013-01-01");
              companyBapi.SetValue("MOEDA","BRL");
              companyBapi.SetValue("REFER","TESTE");
              companyBapi.SetValue("CHLANC","30");
              companyBapi.SetValue("FORNEC","407");
              companyBapi.SetValue("MONTANT","1");
              companyBapi.SetValue("FILIAL","0001");
              companyBapi.SetValue("CONDPAGTO","F000");
              companyBapi.SetValue("DTBASE","2013-01-01");
              companyBapi.SetValue("CHLANC2","40");
              companyBapi.SetValue("CONTA","113103");
              companyBapi.SetValue("DTEFET","2013-01-01");
              companyBapi.SetValue("MENSAGEM", "SamuketePerobao");*/


           // companyBapi.SetValue("USERNAME", USERNAME);                     

            companyBapi.Invoke(dest);
            IRfcTable tabela = companyBapi.GetTable("ZTIPDOC");
            //detail = new string(companyBapi.GetCharArray("ZTIPDOC"));
            detail = "";
            return detail;
        }

        public DataTable TesteSamuel_2(string BUKRS, string GJAHR, string MONAT, string HKONT)
        {          
            DataTable table = new DataTable("T_AUDITGFT");
            table.Columns.Add("BUKRS");
            table.Columns.Add("MONAT");
            table.Columns.Add("HKONT");
            table.Columns.Add("GJAHR");
            table.Columns.Add("CHLANC");
            table.Columns.Add("WRBTR");
            table.Columns.Add("STCD1");
            table.Columns.Add("BRANCH");
            
          

            IRfcFunction companyBapi = repo.CreateFunction("Z_RFC_AUDITGFT");


            companyBapi.SetValue("I_BUKRS", BUKRS);
            companyBapi.SetValue("I_GJAHR", GJAHR);
            companyBapi.SetValue("I_MONAT", MONAT);
            companyBapi.SetValue("I_HKONT", HKONT.PadLeft(10, '0'));
            
            companyBapi.Invoke(dest);
            IRfcTable tabela = companyBapi.GetTable("T_UDITGFTT");


            DataRow row;
            foreach (IRfcStructure detail in tabela)
            {
                row = table.NewRow();
                row["BUKRS"] = detail.GetString("BUKRS");
                row["MONAT"] = detail.GetString("MONAT");
                row["HKONT"] = detail.GetString("HKONT");
                row["GJAHR"] = detail.GetString("GJAHR");
                row["CHLANC"] = detail.GetString("CHLANC");
                row["WRBTR"] = detail.GetString("WRBTR");
                row["STCD1"] = detail.GetString("STCD1");
                row["BRANCH"] = detail.GetString("BRANCH");

               
                table.Rows.Add(row);
            }

            return table;
           
        }


         

        public string chamaBAPI(string CODCLI)
        {           
            IRfcFunction funcBAPI = repo.CreateFunction("BAPI_CUSTOMER_GETDETAIL");
            funcBAPI.SetValue("CUSTOMERNO", CODCLI);
            funcBAPI.SetValue("PI_SALESORG", "1000");
            funcBAPI.Invoke(dest);           
            IRfcStructure structCode = funcBAPI.GetStructure("RETURN");
            string detail = new string(funcBAPI.GetCharArray("RETURN"));
            return structCode.GetString("NAME").ToString();      
       
        }

        public string SEFAZ(string CNPJ,string CENTRO,string BLOQUEIO)
        {
            IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF019");
            funcBAPI.SetValue("I_CENTRO", CENTRO);
            funcBAPI.SetValue("I_CNPJ", CNPJ);
            funcBAPI.SetValue("I_BLOQUEIO", BLOQUEIO);
            
            funcBAPI.Invoke(dest);
           
            string detail = new string(funcBAPI.GetCharArray("E_RETURN"));

            return detail;
           // string detail = new string(funcBAPI.GetCharArray("RETURN"));
           // return structCode.GetString("NAME").ToString();      
        }


        public void teste_altera_clientes()
        {
            /*
             * ESTE MÉTODO GRAVA A TRIPULAÇÃO NO DOCUMENTO DE TRANSPORTE (UTTK)
             * CAMPOS AJUDANTES SE PASSAR VAZIO APAGA
             * MOTORISTA É UM CAMPO OBRIGATÓRIO
             * SE NÃO ENCONTRAR ALGUM DOS PARÂMETROS MOTORISTAS/AJUDANTES, NÃO GRAVA NENHUM
             * OS DADOS FICAM REGISTRADOS NA TRANSAÇÃO VT02N, Campo motorista 1 e aba Dados Adic. os ajudantes
             * ELE RETORNA O NÚMERO DO DOCUMENTO DE TRANSPORTE
             */
            IRfcFunction funcBAPI = repo.CreateFunction("Z_CARGA_CLIENTE");

           // IRfcStructure objEstrutura = funcBAPI.GetStructure("ZRFC_CLIENTES");
            IRfcTable objEstrutura = funcBAPI.GetTable("T_CLIENTES");
            objEstrutura.Append();

            objEstrutura.SetValue("ALTERACAO", "A");//Requerido
            objEstrutura.SetValue("CLIENTE", "12");//Requerido
            objEstrutura.SetValue("EMPRESA", "B001");//Requerido
            objEstrutura.SetValue("ORG_VENDAS", "1000");
            objEstrutura.SetValue("CANAL_DISTRI", "01");
            objEstrutura.SetValue("SETOR_ATIVIDADE", "01");
            objEstrutura.SetValue("GRUPO_CONTAS", "Y009");
            objEstrutura.SetValue("FORMA_TRATAMENTO", "PESSOA PJ");
            objEstrutura.SetValue("RAZAO_SOCIAL", "PEDRO BERN");
            objEstrutura.SetValue("FANTASIA", "VICTORIA PASTELARIA M");
            objEstrutura.SetValue("SETOR", "");
            objEstrutura.SetValue("SUBCANAL", "");
            objEstrutura.SetValue("RUA", "");
            objEstrutura.SetValue("SALA", "");

            objEstrutura.SetValue("ANDAR", "");
            objEstrutura.SetValue("ENDERECO", "Q 8 BL 15 LT 5 LJ 1 M");
            objEstrutura.SetValue("NUMERO", "S/N");
            objEstrutura.SetValue("SUPLEMENTO", "");
            objEstrutura.SetValue("BAIRRO", "SOBRADINHO");
            objEstrutura.SetValue("CEP", "73005-515");
            objEstrutura.SetValue("CIDADE", "SOBREADINHO");
            objEstrutura.SetValue("PAIS", "BR");
            objEstrutura.SetValue("ESTADO", "DF");
            objEstrutura.SetValue("FUSO_HORARIO", "");
            objEstrutura.SetValue("ZONA_TRANSPORTE", "");
            objEstrutura.SetValue("TELEFONE", "0061-34873091");
            objEstrutura.SetValue("RAMAL_TELEFONE", "");
            objEstrutura.SetValue("CELULAR", "");
            objEstrutura.SetValue("FAX", "");
            objEstrutura.SetValue("RAMAL_FAX", "");
            objEstrutura.SetValue("EMAIL", "");

            objEstrutura.SetValue("SITUACAO_CLIENTE", "");
            objEstrutura.SetValue("CNPJ", "36770758000156");
            objEstrutura.SetValue("CPF", "");
            objEstrutura.SetValue("PESSOA_FISICA", "");
            objEstrutura.SetValue("INSCRICAO_EST", "731083500167");
            objEstrutura.SetValue("INSCRICAO_MUN", "");
            objEstrutura.SetValue("ROTA_CLIENTE", "L2D");

            objEstrutura.SetValue("PAGADOR_DIVERG", "");
            objEstrutura.SetValue("CHAVE_INSTRUCAO", "2");
            objEstrutura.SetValue("NOME_CONT_1", "PEDRO");
            objEstrutura.SetValue("NOME_CONT_2", "");
            objEstrutura.SetValue("NOME_CONT_3", "");
            objEstrutura.SetValue("NOME_CONT_4", "");
            objEstrutura.SetValue("NOME_CONT_5", "");
            objEstrutura.SetValue("NOME_CONT_6", "");
            objEstrutura.SetValue("SOBRENOME_1", "NAO DEFINIDO");
            objEstrutura.SetValue("SOBRENOME_2", "");
            objEstrutura.SetValue("SOBRENOME_3", "");
            objEstrutura.SetValue("SOBRENOME_4", "");
            objEstrutura.SetValue("SOBRENOME_5", "");
            objEstrutura.SetValue("SOBRENOME_6", "");

            objEstrutura.SetValue("DEPARTAMENTO_1", "");
            objEstrutura.SetValue("DEPARTAMENTO_2", "");
            objEstrutura.SetValue("DEPARTAMENTO_3", "");
            objEstrutura.SetValue("DEPARTAMENTO_4", "");
            objEstrutura.SetValue("DEPARTAMENTO_5", "");
            objEstrutura.SetValue("DEPARTAMENTO_6", "");
            objEstrutura.SetValue("DEPARTAMENTO_6", "");

            objEstrutura.SetValue("FUNCAO_1", "");
            objEstrutura.SetValue("FUNCAO_2", "");
            objEstrutura.SetValue("FUNCAO_3", "");
            objEstrutura.SetValue("FUNCAO_4", "");
            objEstrutura.SetValue("FUNCAO_5", "");
            objEstrutura.SetValue("FUNCAO_6", "");
            objEstrutura.SetValue("CONT_CONCILIACAO", "112000");
            objEstrutura.SetValue("CHAVE_ORDENACAO", "3");
            objEstrutura.SetValue("GRUPO_TESOURARIA", "CL02");
            objEstrutura.SetValue("JUROS", "");
            objEstrutura.SetValue("NUMERO_ANTIGO", "");
            objEstrutura.SetValue("COND_PAGTO", "C015");

            objEstrutura.SetValue("GRUPO_TOLERANCIA", "DEB1");
            objEstrutura.SetValue("FORMA_PAGTO", "D");
            objEstrutura.SetValue("BANCO_EMPRESA", "ITAU");
            objEstrutura.SetValue("BLOQUEIO", "");
            objEstrutura.SetValue("REGIAO_VENDAS", "");
            objEstrutura.SetValue("ESCRIT_VENDAS", "BRSL");
            objEstrutura.SetValue("EQUIPE_VENDAS", "BRS");
            objEstrutura.SetValue("GRUPO_CLIENTES", "2");
            objEstrutura.SetValue("GRUPO_PRECO", "45");
            objEstrutura.SetValue("ESQUEMA_CLIENTE", "1");

            objEstrutura.SetValue("LISTA_PRECOS", "C");
            objEstrutura.SetValue("GR_ESTAT_CLIENTE", "1");
            objEstrutura.SetValue("PRIORI_REMESSA", "2");
            objEstrutura.SetValue("AGRUP_ORDENS", "X");

            objEstrutura.SetValue("COND_EXPEDICAO", "1");
            objEstrutura.SetValue("CENTRO_FORNEC", "1");
            objEstrutura.SetValue("FORNEC_PARC_MAX", "9");
            objEstrutura.SetValue("ZONA_TRANSPORTE_SD", "");
            objEstrutura.SetValue("INCOTERMS_1", "CIF");
            objEstrutura.SetValue("INCOTERMS_2", "CIF");
            objEstrutura.SetValue("COND_PAGTO_SD", "C015");
            objEstrutura.SetValue("AREA_CONTR_CRED", "");
            objEstrutura.SetValue("GRUPO_CLASS_CONT", "1");
            objEstrutura.SetValue("CLASSIF_FISCAL", "1");
            objEstrutura.SetValue("LIMITE_CRED", "11850");
            objEstrutura.SetValue("CLASSE_RISCO", "CR8");
            objEstrutura.SetValue("BLOQUEIO_CRED", "");
            objEstrutura.SetValue("TAB_PRECO", "101");
            objEstrutura.SetValue("CANAL_COMERCIAL", "20");
            objEstrutura.SetValue("TIPO_NEGOCIO", "1");
            objEstrutura.SetValue("BSB_CANAL", "R");
            objEstrutura.SetValue("ROTAE_CLIENTE", "D0F");
            objEstrutura.SetValue("MATR_FUNC", "2333");
            objEstrutura.SetValue("CONTA_CHAVE", "99999");
            objEstrutura.SetValue("SEG_VIA", "S");
            objEstrutura.SetValue("CCRM", "0");
            objEstrutura.SetValue("BOLETO", "S");
            objEstrutura.SetValue("UNIFICA_BOLETO", "S");
            objEstrutura.SetValue("CLDISTANCIA", "A");
            objEstrutura.SetValue("SABADO", "S");
            objEstrutura.SetValue("ENTREGA", "S");
            objEstrutura.SetValue("ALTER_PROD", "S");
            objEstrutura.SetValue("ITM_CRITIC", "S");
            objEstrutura.SetValue("FORA_ROTA", "S");
            objEstrutura.SetValue("AD_FINAN", "S");
            objEstrutura.SetValue("VENDA_MIN", "999");



            funcBAPI.Invoke(dest);

            IRfcTable table = funcBAPI.GetTable("T_RETURN");
            //Pega resposta e joga numa tabela
           // DataTable table=null;

           // return table;


        }


        public void testeRFCminiSap()
        {
            IRfcFunction companyBapi = repo.CreateFunction("Z_RFC_WELL_CALC");

            companyBapi.SetValue("P_VALOR1", 1);
            companyBapi.SetValue("P_SINAL", '+');
            companyBapi.SetValue("P_VALOR2", 3);

            companyBapi.Invoke(dest);

            string detail = new string(companyBapi.GetCharArray("P_RESULT"));

           // return detail;
        }



    }
}