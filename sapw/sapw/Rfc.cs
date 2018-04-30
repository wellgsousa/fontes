using System;
using System.Collections.Generic;
using System.Web;
using SAP.Middleware.Connector;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;


namespace sapw
{
    public class Rfc
    {
        RfcRepository repo;
        RfcDestination dest;
        int guarda_prox_complementoPM03=0;

        public Rfc()
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
   

/************************************************************************************************
 MÉTODOS DO PROJETO DSD
 
 *************************************************************************************************/
    //Sefaz
        public string SEFAZ_CLIENTE_SAP(string CNPJ, string CENTRO, string BLOQUEIO)
        {
            /*
             * ESTE MÉTODO FAZ O BLOQUEIO DE CLIENTES NO SAP, POR MEIO DO CAMPO AUFSD
             * BLOQUEIO SEFAZ="I" | DESBLOQUEIO=""
             */
            IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF019");
            funcBAPI.SetValue("I_CENTRO", CENTRO);
            funcBAPI.SetValue("I_CNPJ", CNPJ);
            funcBAPI.SetValue("I_BLOQUEIO", BLOQUEIO);

            funcBAPI.Invoke(dest);

            string detail = new string(funcBAPI.GetCharArray("E_RETURN"));

            return detail;                 
        }

    //SCD
        public DataTable SCD_ATUALIZAR_TRIPULANTES(string DATA_ENTREGA, string CARGA, string MOTORISTA, string AJUDANTE1, string AJUDANTE2, string AJUDANTE3, string AJUDANTE4)
        {
            /*
             * ESTE MÉTODO GRAVA A TRIPULAÇÃO NO DOCUMENTO DE TRANSPORTE (UTTK)
             * CAMPOS AJUDANTES SE PASSAR VAZIO APAGA
             * MOTORISTA É UM CAMPO OBRIGATÓRIO
             * SE NÃO ENCONTRAR ALGUM DOS PARÂMETROS MOTORISTAS/AJUDANTES, NÃO GRAVA NENHUM
             * OS DADOS FICAM REGISTRADOS NA TRANSAÇÃO VT02N, Campo motorista 1 e aba Dados Adic. os ajudantes
             * ELE RETORNA O NÚMERO DO DOCUMENTO DE TRANSPORTE
             */
            IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF029");

            funcBAPI.SetValue("I_DATA", DATA_ENTREGA);//Requerido
            funcBAPI.SetValue("I_CARGA", CARGA);//Requerido            

            string mot = "";
            string ajd1="";
            string ajd2="";
            string ajd3="";
            string ajd4="";

            if (!MOTORISTA.Trim().Equals(""))
            {
                mot = MOTORISTA.ToString().PadLeft(8, '0');
            }

            if (!AJUDANTE1.Trim().Equals(""))
            {
                ajd1= AJUDANTE1.ToString().PadLeft(8, '0');
            }
            if (!AJUDANTE2.Trim().Equals(""))
            {
                ajd2=AJUDANTE2.ToString().PadLeft(8, '0');
            }
            if (!AJUDANTE3.Trim().Equals(""))
            {
                ajd3 = AJUDANTE3.ToString().PadLeft(8, '0');
            }
            if (!AJUDANTE4.Trim().Equals(""))
            {
                ajd4 = AJUDANTE4.ToString().PadLeft(8, '0');
            }

            funcBAPI.SetValue("I_MOTORISTA", mot);//Requerido
            funcBAPI.SetValue("I_AJUDANTE01", ajd1);
            funcBAPI.SetValue("I_AJUDANTE02", ajd2);
            funcBAPI.SetValue("I_AJUDANTE03", ajd3);
            funcBAPI.SetValue("I_AJUDANTE04", ajd4);

            funcBAPI.Invoke(dest);

            //Pega resposta e joga numa tabela
            string sts = new string(funcBAPI.GetCharArray("E_STATUS"));
            string msg = new string(funcBAPI.GetCharArray("E_MESSAGE"));
            string tra = new string(funcBAPI.GetCharArray("E_TRANSPORTE"));
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("E_STATUS", typeof(string));
            table.Columns.Add("E_MESSAGE", typeof(string));
            table.Columns.Add("E_TRANSPORTE", typeof(string));
            DataRow Row = table.NewRow();
            Row["E_STATUS"]=sts;
            Row["E_MESSAGE"] = msg;
            Row["E_TRANSPORTE"] = tra;
            table.Rows.Add(Row);

            return table;

            
        }

        public DataTable SCD_CUBAGEM_CLIENTES_DACARGA(string DATA_ENTREGA, string CARGA)
        {
            /*
             * ESTE MÉTODO BUSCA DADOS DE CUBAGEM TOTAL DO CLIENTE DA CARGA
             * MOTORISTA É UM CAMPO OBRIGATÓRIO            
             */
            IRfcFunction func = repo.CreateFunction("ZDSDF030");

            func.SetValue("I_DATA", DATA_ENTREGA);//Requrido
            func.SetValue("I_CARGA", CARGA);//Requerido           

            func.Invoke(dest);

            //Pega resposta e joga numa tabela           
            string msg = new string(func.GetCharArray("E_MESSAGE"));
            string tra = new string(func.GetCharArray("E_TRANSPORTE"));
            

            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));
            table.Columns.Add("CARGA", typeof(string));
            table.Columns.Add("VEICULO", typeof(string));
            table.Columns.Add("DATA", typeof(string));
            table.Columns.Add("CLIENTE", typeof(string));
            table.Columns.Add("CUBAGEM", typeof(string));
            table.Columns.Add("DOC_TRANSPORTE", typeof(string));

            DataRow Row=null;
            try
            {
                IRfcTable returnTable = func.GetTable("T_TRANSPORTE");

                if (returnTable.Count > 0)
                {

                    foreach (IRfcStructure row in returnTable)
                    {
                        Row = table.NewRow();
                        Row["STATUS"] = "S";
                        Row["MESSAGE"] = "Carga localizada!";
                        Row["CARGA"] = row.GetValue("CARGA").ToString();
                        Row["VEICULO"] = row.GetValue("VEICULO").ToString();
                        Row["DATA"] = row.GetValue("DATA").ToString();
                        Row["CLIENTE"] = row.GetValue("CLIENTE").ToString();
                        Row["CUBAGEM"] = row.GetValue("CUBAGEM").ToString();
                        Row["DOC_TRANSPORTE"] = tra;

                        table.Rows.Add(Row);
                    }
                }
                else
                {
                    Row = table.NewRow();
                    Row["STATUS"] = "E";
                    Row["MESSAGE"] = "Carga " + CARGA + " nao localizada na data " + DATA_ENTREGA + "!";
                    table.Rows.Add(Row);
                }

               
               

            }
            catch (Exception err)
            {
                Row = table.NewRow();
                Row["STATUS"] = "E";
                Row["MESSAGE"] = err.Message;
                table.Rows.Add(Row);
            }
            

            return table;

        }

        public DataTable SCD_VERIFICA_VEICULO_EXISTE_SAP(string NUM_FROTA)
        {
            /*
             * ESTE MÉTODO VERIFICA SE O VEICULO EXISTE NO SAP NA TABELA EQUI
             * PASSA O NÚMERO DA FROTA 
             * RETORNA: 1 - Existe / 2 - Não
             */

            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MENSAGEM", typeof(string));
            table.Columns.Add("FROTA", typeof(string));
            table.Columns.Add("DENOMINACACAO", typeof(string));

            try
            {

                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "EQKT"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "EQUNR");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "EQKTX");
               

                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "EQUNR EQ '" + NUM_FROTA.ToString().PadLeft(18, '0') + "'");


                rfc_read_table.Invoke(dest);



                DataRow Row = table.NewRow();

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                if (returnTable.Count > 0)
                {
                    string[] fieldValues;
                    fieldValues = returnTable.GetValue("WA").ToString().Split(';');
                    Row["STATUS"] = "S";
                    Row["MENSAGEM"] = "Veículo Existe";
                    Row["FROTA"] = fieldValues[0];
                    Row["DENOMINACACAO"] = fieldValues[1];
                }
                else
                {
                    Row["STATUS"] = "E";
                    Row["MENSAGEM"] = "Veículo não encontrado!";
                    Row["FROTA"] = "";
                    Row["DENOMINACACAO"] = "";
                }

                table.Rows.Add(Row); 
               
            }
            catch (Exception Err)
            {
                throw Err;
            }

            return table;

        }

        public DataTable SCD_VERIFICA_STATUS_TRANSPORTE(string DOC_TRANSPORTE)
        {
            /*
             * ESTE MÉTODO VERIFICA OS STATUS DO TRANSPORTE
             * PASSA A DATA DE CRIAÇÃO E A CARGA
             */

            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MENSAGEM", typeof(string));
            table.Columns.Add("DOC_TRANSPORTE", typeof(string));
            table.Columns.Add("VEICULO", typeof(string));
            table.Columns.Add("TIP_TRANSPORTE", typeof(string));
            

            table.Columns.Add("DATA", typeof(string));
            table.Columns.Add("CARGA", typeof(string));

            table.Columns.Add("STATUS_REGISTRO", typeof(string));
            table.Columns.Add("DTHR_REGISTRO", typeof(string));

            table.Columns.Add("STATUS_INI_CARREG", typeof(string));
            table.Columns.Add("DTHR_INI_CARREG", typeof(string));

            table.Columns.Add("STATUS_FIM_CARREG", typeof(string));
            table.Columns.Add("DTHR_FIM_CARREG", typeof(string));

            table.Columns.Add("STATUS_PROCESS_TRA", typeof(string));
            table.Columns.Add("DTHR_PROCESS_TRA", typeof(string));

            table.Columns.Add("STATUS_INICIO_TRA", typeof(string));
            table.Columns.Add("DTHR_INICIO_TRA", typeof(string));

            table.Columns.Add("STATUS_ESPECIFICO_TRA", typeof(string));

            table.Columns.Add("STATUS_FIM_TRA", typeof(string));
            table.Columns.Add("DTHR_FIM_TRA", typeof(string));

            try
            {

                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "VTTK"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "TKNUM");//NUMERO DO TRANSPORTE
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "/BEV1/RPMOWA");//VEICULO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "BFART");//TIPO DE TRANSPORTE
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DPTBG");//DATA DE ENTREGA
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "TPBEZ");//NOME DA CARGA
                   
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STREG");//STATUS REGISTRO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DAREG");//DATA STATUS REGISTRO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UAREG");//HORA STATUS REGISTRO
                                        
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STLBG");//STATUS INICIO DO CARREGAMENTO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DALBG");//DATA STATUS INICIO DO CARREGAMENTO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UALBG");//HORA STATUS INICIO DO CARREGAMENTO

                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STLAD");//STATUS FIM DO CARREGAMENTO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DALEN");//DATA STATUS FIM DO CARREGAMENTO
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UALEN");//HORA STATUS FIM DO CARREGAMENTO
                

                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STABF");//STATUS PROCESSAMENTO TRA
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DTABF");//DATA STATUS PROCESSAMENTO TRA
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UZABF");//HORA STATUS PROCESSAMENTO TRA
               
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STTBG");//STATUS INICIO DO TRANSPORTE
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DATBG");//DATA STATUS INICIO DO TRANSPORTE
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UATBG");//HORA STATUS INICIO DO TRANSPORTE
                
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ADD01");//STATUS ESPECIFICOS DO TRANSPORTE 

                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STTEN");//STATUS FIM DO TRANSPORTE 
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "DATEN");//DATA FIM DO TRANSPORTE 
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "UATEN");//HORA FIM DO TRANSPORTE   
                      
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Append();

                tblOptions.SetValue("TEXT", "TKNUM EQ '" + DOC_TRANSPORTE.ToString().PadLeft(10, '0') + "'");


                rfc_read_table.Invoke(dest);


                DataRow Row = table.NewRow();

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                string dt_reg = null;
                string hr_reg = null;

                string dt_ini_carr = null;
                string hr_ini_carr = null;

                string dt_fim_carr = null;
                string hr_fim_carr = null;

                string dt_proc_carr = null;
                string hr_proc_carr = null;

                string dt_ini_tra = null;
                string hr_ini_tra = null;

                string dt_fim_tra = null;
                string hr_fim_tra = null;

                if (returnTable.Count > 0)
                {
                    string[] fieldValues;
                    fieldValues = returnTable.GetValue("WA").ToString().Split(';');
                    Row["STATUS"] = "S";
                    Row["MENSAGEM"] = "Documento Encontrado";

                    Row["DOC_TRANSPORTE"] = fieldValues[0];
                    Row["VEICULO"] = fieldValues[1];
                    Row["TIP_TRANSPORTE"] = fieldValues[2];
                    
                    Row["DATA"] = fieldValues[3];
                    Row["CARGA"] = fieldValues[4];

                    Row["STATUS_REGISTRO"] = fieldValues[5];
                   

                    if (fieldValues[5] == "X")
                    {
                        dt_reg = fieldValues[6].Substring(0, 4) + "-" + fieldValues[6].Substring(4, 2) + "-" + fieldValues[6].Substring(6, 2);
                        hr_reg = fieldValues[7].Substring(0, 2) + ":" + fieldValues[7].Substring(2, 2) + ":" + fieldValues[7].Substring(4, 2);
                    }
                    Row["DTHR_REGISTRO"] = dt_reg + " " + hr_reg;

                    Row["STATUS_INI_CARREG"] = fieldValues[8];
                    if (fieldValues[8] == "X")
                    {
                        dt_ini_carr = fieldValues[9].Substring(0, 4) + "-" + fieldValues[9].Substring(4, 2) + "-" + fieldValues[9].Substring(6, 2);
                        hr_ini_carr = fieldValues[10].Substring(0, 2) + ":" + fieldValues[10].Substring(2, 2) + ":" + fieldValues[10].Substring(4, 2);
                    }
                    Row["DTHR_INI_CARREG"] = dt_ini_carr + " " + hr_ini_carr;

                    Row["STATUS_FIM_CARREG"] = fieldValues[11];
                    if (fieldValues[11] == "X")
                    {
                        dt_fim_carr = fieldValues[12].Substring(0, 4) + "-" + fieldValues[12].Substring(4, 2) + "-" + fieldValues[12].Substring(6, 2);
                        hr_fim_carr = fieldValues[13].Substring(0, 2) + ":" + fieldValues[13].Substring(2, 2) + ":" + fieldValues[13].Substring(4, 2);
                    }
                    Row["DTHR_FIM_CARREG"] = dt_fim_carr + " " + hr_fim_carr;

                    Row["STATUS_PROCESS_TRA"] = fieldValues[14];
                    if (fieldValues[14] == "X")
                    {
                        dt_proc_carr = fieldValues[15].Substring(0, 4) + "-" + fieldValues[15].Substring(4, 2) + "-" + fieldValues[15].Substring(6, 2);
                        hr_proc_carr = fieldValues[16].Substring(0, 2) + ":" + fieldValues[16].Substring(2, 2) + ":" + fieldValues[16].Substring(4, 2);
                    }
                    Row["DTHR_PROCESS_TRA"] = dt_proc_carr + " " + hr_proc_carr;

                    Row["STATUS_INICIO_TRA"] = fieldValues[17];
                    if (fieldValues[17] == "X")
                    {
                        dt_ini_tra = fieldValues[18].Substring(0, 4) + "-" + fieldValues[18].Substring(4, 2) + "-" + fieldValues[18].Substring(6, 2);
                        hr_ini_tra = fieldValues[19].Substring(0, 2) + ":" + fieldValues[19].Substring(2, 2) + ":" + fieldValues[19].Substring(4, 2);
                    }
                    Row["DTHR_INICIO_TRA"] = dt_ini_tra + " " + hr_ini_tra;

                    Row["STATUS_ESPECIFICO_TRA"] = fieldValues[20];

                    Row["STATUS_FIM_TRA"] = fieldValues[21];
                    if (fieldValues[21] == "X")
                    {
                        dt_fim_tra = fieldValues[22].Substring(0, 4) + "-" + fieldValues[22].Substring(4, 2) + "-" + fieldValues[22].Substring(6, 2);
                        hr_fim_tra = fieldValues[23].Substring(0, 2) + ":" + fieldValues[23].Substring(2, 2) + ":" + fieldValues[23].Substring(4, 2);
                    }

                    Row["DTHR_FIM_TRA"] = dt_fim_tra + " " + hr_fim_tra;

                }
                else
                {
                    Row["STATUS"] = "E";
                    Row["MENSAGEM"] = "Documento de Transporte "+DOC_TRANSPORTE.ToString()+" não encontrado !";                  
                }

                table.Rows.Add(Row);

            }
            catch (Exception Err)
            {
                throw Err;
            }

            return table;

        }

        public DataTable SCD_VERIFICA_CAMPO_DE_AUXILIAR_DISPONIVEL_TRANSP(string DOC_TRANSPORTE)
        {
            /*
             * ESTE MÉTODO VERIFICA QUAIS CAMPOS DE AJUDANTES ESTÃO DISPONIVEIS NO DOCUMENTO DE TRANSPORTE
             * ESTE SERÁ USADO AO REGISTRAR UMA RECARGA, ONDE PRECISE ADICIONAR MAIS AJUDANTES NUMA RECARGA
             */

            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));
            table.Columns.Add("DOC_TRANSPORTE", typeof(string));
            table.Columns.Add("MOTORISTA", typeof(string));
            table.Columns.Add("AJUDANTE1", typeof(string));
            table.Columns.Add("AJUDANTE2", typeof(string));
            table.Columns.Add("AJUDANTE3", typeof(string));
            table.Columns.Add("AJUDANTE4", typeof(string));

            DataRow row;
            row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_TRIP_DOC_TRANSP");
                funcBAPI.SetValue("COD_TRANSP", DOC_TRANSPORTE.ToString().PadLeft(10, '0'));
                
                funcBAPI.Invoke(dest);

                //string retorno = funcBAPI.GetString("E_RETURN");

                IRfcTable tripulantes = funcBAPI.GetTable("ZTRIPDOCTRANSP");
               

                if (tripulantes.Count > 0)
                {
                    foreach (IRfcStructure detail in tripulantes)
                    {
                        row["STATUS"] = "S";
                        row["MESSAGE"] = "Registro encontrado";
                        row["DOC_TRANSPORTE"] = detail.GetString("DOC_TRANSPORTE");
                        row["MOTORISTA"] = detail.GetString("MOTORISTA");
                        row["AJUDANTE1"] = detail.GetString("AJUDANTE1");
                        row["AJUDANTE2"] = detail.GetString("AJUDANTE2");
                        row["AJUDANTE3"] = detail.GetString("AJUDANTE3");
                        row["AJUDANTE4"] = detail.GetString("AJUDANTE4");
                       
                    }
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Registro encontrado"; 
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = "Falha:"+e.ToString();     
            }

            table.Rows.Add(row);

            return table;
        }

    //Relatórios do Comercial
        public DataTable consultaClienteSAP(string codcli)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("CLIENTE");

            table.Columns.Add("CODCLI", typeof(string));
            table.Columns.Add("RAZAO", typeof(string));
            table.Columns.Add("FANTASIA", typeof(string));
            table.Columns.Add("CNPJ", typeof(string));
            table.Columns.Add("CPF", typeof(string));
            table.Columns.Add("TIPO", typeof(string));
            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("TELEFONE1", typeof(string));
            table.Columns.Add("CONTATO", typeof(string));

            try
            {

                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "KNA1"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "KUNNR");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "NAME1");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "NAME2");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STCD1");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STCD2");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ANRED");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "AUFSD");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "TELF1");


                /*Faz a seleção do Registro*/
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "KUNNR EQ '" + codcli.ToString().PadLeft(10, '0') + "' OR STCD1 EQ '" + codcli + "' OR ");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "STCD2 EQ '" + codcli + "'");

                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);



                DataRow Row = table.NewRow();

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                string[] fieldValues;
                string[] fieldValues2;

                foreach (IRfcStructure row in returnTable)
                {
                    fieldValues = row.GetValue("WA").ToString().Split(';');

                    Row["CODCLI"] = fieldValues[0];
                    Row["RAZAO"] = fieldValues[1];
                    Row["FANTASIA"] = fieldValues[2];
                    Row["CNPJ"] = fieldValues[3];
                    Row["CPF"] = fieldValues[4];
                    Row["TIPO"] = fieldValues[5];
                    Row["STATUS"] = fieldValues[6];
                    Row["TELEFONE1"] = fieldValues[7];

                    /*PEGAR DADOS DE CONTATO*/
                    IRfcFunction rfc_read_table3 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
                    rfc_read_table3.SetValue("QUERY_TABLE", "KNVK"); //Nome da Tabela
                    rfc_read_table3.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                    var fieldsTable3 = rfc_read_table3.GetTable("FIELDS");
                    fieldsTable3.Append();
                    fieldsTable3.SetValue("FIELDNAME", "NAMEV");//Contato

                    IRfcTable tblOptions5 = rfc_read_table3.GetTable("OPTIONS");
                    tblOptions5.Clear();
                    tblOptions5.Append();
                    tblOptions5.SetValue("TEXT", "KUNNR EQ '" + fieldValues[0] + "'");

                    /*Executa a RFC*/
                    rfc_read_table3.Invoke(dest);

                   IRfcTable returnTable2 = rfc_read_table3.GetTable("DATA");
                   if (returnTable2.Count>0)
                   {
                       fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');
                       Row["CONTATO"] = fieldValues2[0];
                   }
                   else
                   {
                       Row["CONTATO"] = "Sem informação de contato";
                   }

                  

                    /*FIM PEGAR DADOS DE CONTATO*/

                    // process field values
                }

                table.Rows.Add(Row);
            }
            catch(Exception e)
            {
                throw e;
            }

            return table;

        }


        public DataTable consultaVendedor(string vendedor_codsap)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("VENDEDOR");

            table.Columns.Add("CODIGO_VEND", typeof(string));
            table.Columns.Add("CODIGO_CLI_SAP", typeof(string));
            table.Columns.Add("MATRICULA", typeof(string));
            table.Columns.Add("NOME", typeof(string));

            try
            {
                /*Buscar o código do cliente na ZDSDT022*/
                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "ZDSDT022"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "COD_SAP");

                /*Faz a seleção do Registro*/
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Clear();
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "VENDEDOR EQ '" + vendedor_codsap + "' AND MANDT EQ '500'");

                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                string[] fieldValues;

                string cod_sap = "";

                if (returnTable.Count > 0)
                {
                    fieldValues = returnTable.GetValue("WA").ToString().Split(';');
                    cod_sap = fieldValues[0];
                }
                /*Fim de Buscar o código do cliente na ZDSDT022*/

                /*Buscar o código do cliente na KNA1*/
                IRfcFunction rfc_read_table2 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table2.SetValue("QUERY_TABLE", "KNA1"); //Nome da Tabela

                rfc_read_table2.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable2 = rfc_read_table2.GetTable("FIELDS");

                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "NAME1");

                /*Faz a seleção do Registro*/
                IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");
                tblOptions2.Clear();
                tblOptions2.Append();
                tblOptions2.SetValue("TEXT", "KUNNR EQ '" + cod_sap + "' AND MANDT EQ '500'");

                /*Executa a RFC*/
                rfc_read_table2.Invoke(dest);

                IRfcTable returnTable2 = rfc_read_table2.GetTable("DATA");
                string[] fieldValues2;


                /*Buscar o Cliente na KNA1, conforme o código localizado*/

                /*Fim de Buscar o Cliente na KNA1, conforme o código localizado*/

                /*Buscar o código do cliente na KNVV*/
                IRfcFunction rfc_read_table3 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table3.SetValue("QUERY_TABLE", "KNVV"); //Nome da Tabela

                rfc_read_table3.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable3 = rfc_read_table3.GetTable("FIELDS");

                fieldsTable3.Append();
                fieldsTable3.SetValue("FIELDNAME", "EIKTO");//MATRICULA 

                /*Faz a seleção do Registro*/
                IRfcTable tblOptions3 = rfc_read_table3.GetTable("OPTIONS");
                tblOptions3.Clear();
                tblOptions3.Append();
                tblOptions3.SetValue("TEXT", "VKORG = '1000' AND KUNNR EQ '" + cod_sap + "' AND MANDT EQ '500'");

                /*Executa a RFC*/
                rfc_read_table3.Invoke(dest);

                string[] fieldValues3;

                string matricula = "Indefinido";

                IRfcTable returnTable3 = rfc_read_table3.GetTable("DATA");

                if (returnTable3.Count > 0)
                {
                    fieldValues3 = returnTable3.GetValue("WA").ToString().Split(';');
                    matricula = fieldValues3[0];
                }
                /*Fim Buscar o código do cliente na KNVV*/

                DataRow Row = table.NewRow();

                foreach (IRfcStructure row in returnTable2)
                {
                    fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');
                    Row["CODIGO_VEND"] = vendedor_codsap;
                    Row["CODIGO_CLI_SAP"] = cod_sap;
                    Row["MATRICULA"] = matricula;
                    Row["NOME"] = fieldValues2[0];
                }

                table.Rows.Add(Row);
                /*Fim Buscar o Cliente na KNA1, conforme o código localizado*/
            }
            catch(Exception e)
            {
                throw;
            }
            return table;            

        }


        public DataTable consultaFaturasEmAbertoCliente(string codcli)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("FATURAS");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));

            table.Columns.Add("COD_CLIENTE", typeof(string));
            table.Columns.Add("EMISSAO", typeof(string));
            table.Columns.Add("VENCIMENTO", typeof(string));
            table.Columns.Add("NOTA_FISCAL", typeof(string));
            table.Columns.Add("DETALHES", typeof(string));
            table.Columns.Add("VALOR", typeof(string));
            table.Columns.Add("DOCNUM", typeof(string));
            table.Columns.Add("TIPO_DOC", typeof(string));
            table.Columns.Add("NOTAS_BOLETO", typeof(string));
            table.Columns.Add("VALOR_BOLETO", typeof(string));
            table.Columns.Add("VENC_BOLETO", typeof(string));

            DataRow Row = table.NewRow();

            try
            {

                /*Buscar o código do cliente na KNA1*/
                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "BSID"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "BLDAT");//Emissão
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ZFBDT");//Vencimento
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "XBLNR");//Ref. Nota
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "SGTXT");//Detalhes           
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "WRBTR");//Valor
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "BELNR");//Número do Documento
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "BLART");//Tipo do Documento


                /*Faz a seleção do Registro*/
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Clear();
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "KUNNR EQ '" + codcli.ToString().PadLeft(10, '0') + "' AND MANDT EQ '500' AND BUKRS EQ 'B001'");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "  AND BLART IN ('DI','DA','RV','DZ','AB')");

                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                if (returnTable.Count > 0)
                {
                    /*Buscar o código do cliente o valor da fatura com os juros de acordo com o Boleto*/
                    IRfcFunction rfc_read_table2 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                    rfc_read_table2.SetValue("QUERY_TABLE", "ZBLWEB"); //Nome da Tabela

                    rfc_read_table2.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                    var fieldsTable2 = rfc_read_table2.GetTable("FIELDS");
                    fieldsTable2.Append();
                    fieldsTable2.SetValue("FIELDNAME", "INST17");//Notas do Boleto
                    fieldsTable2.Append();
                    fieldsTable2.SetValue("FIELDNAME", "VALDOC");//Valor do Boleto
                    fieldsTable2.Append();
                    fieldsTable2.SetValue("FIELDNAME", "VENCIM");//Vencimento do Boleto
                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");

                    /*Fim de Buscar o Cliente na KNA1, conforme o código localizado*/
                    string[] fieldValues;
                    foreach (IRfcStructure row in returnTable)
                    {
                        fieldValues = row.GetValue("WA").ToString().Split(';');

                        Row = table.NewRow();

                        Row["STATUS"] = "S";
                        Row["MESSAGE"] = returnTable.Count+ " registros encontrados";

                        Row["COD_CLIENTE"] = codcli.ToString().PadLeft(10, '0');
                        Row["EMISSAO"] = fieldValues[0];
                        Row["VENCIMENTO"] = fieldValues[1];
                        Row["NOTA_FISCAL"] = fieldValues[2];
                        Row["DETALHES"] = fieldValues[3];
                        Row["VALOR"] = fieldValues[4];
                        Row["DOCNUM"] = fieldValues[5];
                        Row["TIPO_DOC"] = fieldValues[6];

                        tblOptions2.Clear();
                        tblOptions2.Append();
                        tblOptions2.SetValue("TEXT", "NUMDOC EQ '" + fieldValues[5] + "' AND MANDAT EQ '500'");
                        rfc_read_table2.Invoke(dest);

                        IRfcTable returnTable2 = rfc_read_table2.GetTable("DATA");

                        string[] fieldValues2;

                        if (returnTable2.Count > 0)
                        {
                            fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');
                            Row["NOTAS_BOLETO"] = fieldValues2[0];
                            Row["VALOR_BOLETO"] = fieldValues2[1];
                            Row["VENC_BOLETO"] = fieldValues2[2];
                        }
                        else
                        {
                            Row["NOTAS_BOLETO"] = "";
                            Row["VALOR_BOLETO"] = "";
                            Row["VENC_BOLETO"] = "";
                        }


                        table.Rows.Add(Row);


                    }

                }
                else
                {
                    Row["STATUS"] = "E";
                    Row["MESSAGE"] = "Nenhum registro encontrado para o cliente "+codcli;

                    table.Rows.Add(Row);
                }

                /*Fim Buscar o Cliente na KNA1, conforme o código localizado*/
            }
            catch(Exception e)
            {
                Row["STATUS"] = "E";
                Row["MESSAGE"] = e.Message;

                table.Rows.Add(Row);
            }

            return table;
        }


        public string consultaFormaPagamentoCliente(string codcli)
        {
            string resp_final = "Cliente não encontrado";

            /*KNB1*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "KNB1"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "ZWELS");

            /*T042Z - DESCRIÇÃO DA FORMA DE PAGAMENTO*/
            IRfcFunction rfc_read_table1 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table1.SetValue("QUERY_TABLE", "T042Z"); //Nome da Tabela

            rfc_read_table1.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable1 = rfc_read_table1.GetTable("FIELDS");
            fieldsTable1.Append();
            fieldsTable1.SetValue("FIELDNAME", "TEXT1");

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");

            tblOptions.Append();
            tblOptions.SetValue("TEXT", "BUKRS = 'B001' AND KUNNR EQ '" + codcli.ToString().PadLeft(10, '0') + "' AND MANDT EQ '500'");

            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);

           

            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                resp_final = "";
                string[] fieldValues;

                fieldValues = returnTable.GetValue("WA").ToString().Split(';');
                string resposta = fieldValues[0];
                string[] Separa;
                

                Separa=resposta.Split();

                foreach (string Row in Separa)
                {
                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions1 = rfc_read_table1.GetTable("OPTIONS");
                    tblOptions1.Clear();
                    tblOptions1.Append();
                    tblOptions1.SetValue("TEXT", "ZLSCH EQ '" + Row + "' AND  LAND1 EQ 'BR'");

                    /*Executa a RFC*/
                    rfc_read_table1.Invoke(dest);

                    /*Recebendo o retorno*/
                    IRfcTable returnTable1 = rfc_read_table1.GetTable("DATA");
                    string[] fieldValues1;
                    fieldValues1 = returnTable1.GetValue("WA").ToString().Split(';');
                    resp_final += fieldValues1[0] + "/";
                }
            }

            return resp_final;          

        }


        public DataTable consultaDiasDeVisitaNoCliente(string rota)
        {
            DataTable table= new DataTable("RETURN");

            table.Columns.Add("DIAS_VISITA", typeof(string));
            table.Columns.Add("TIPO_ROTA", typeof(string));
            
            string visita = "Indefinido";

            /*KNB1*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "/DSD/VC_VPH"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "MONDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "TUESDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "WEDNESDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "THURSDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "FRIDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "SATURDAY");
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "SUNDAY");

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
            string ultimo_digito_da_rota = rota.Substring(rota.Length-1, 1);
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "VPTYP EQ '" + ultimo_digito_da_rota + "' AND MANDT EQ 500");

            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);



            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                visita = "";

                string[] fieldValues;

                fieldValues = returnTable.GetValue("WA").ToString().Split(';');


                if (fieldValues[0] == "X") { visita = "Segunda | "; }
                if (fieldValues[1] == "X") { visita += "Terça | "; }
                if (fieldValues[2] == "X") { visita += "Quarta | "; }
                if (fieldValues[3] == "X") { visita += "Quinta | "; }
                if (fieldValues[4] == "X") { visita += "Sexta | "; }
                if (fieldValues[5] == "X") { visita += "Sábado | "; }
                
            }

            /*Pegar o Tipo de Rota*/
            /*ZDST028*/
            

            IRfcFunction rfc_read_table2 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table2.SetValue("QUERY_TABLE", "ZDSDT028"); //Nome da Tabela

            rfc_read_table2.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            var fieldsTable2 = rfc_read_table2.GetTable("FIELDS");

            fieldsTable2.Append();

            fieldsTable2.SetValue("FIELDNAME", "TIPO_ROTA");

            IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");
           
            tblOptions2.Append();

            tblOptions2.SetValue("TEXT", "ROTA EQ '" + rota.ToString() + "'");

            rfc_read_table2.Invoke(dest);

            IRfcTable returnTable2 = rfc_read_table2.GetTable("DATA");

            string[] fieldValues2;

            DataRow Row = table.NewRow();

            if (returnTable.Count > 0)
            {
                Row["DIAS_VISITA"] = visita;
            }
            else
            {
                Row["DIAS_VISITA"] = "";
            }

            if (returnTable2.Count > 0)
            {
                fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');
                switch (Convert.ToInt16((fieldValues2[0])))
                {                   
                    case 1:
                        Row["TIPO_ROTA"] = "SFCoke";
                        break;
                    case 2:
                        Row["TIPO_ROTA"] = "Cokenet";
                        break;
                    case 9:
                        Row["TIPO_ROTA"] = "Desativada";
                        break;

                }
               
            }
            else
            {
                Row["TIPO_ROTA"] = "Não encontrada em ZDSDT028";
            }

            table.Rows.Add(Row);

           
            return table;

        }




        public DataTable listaClientesSAP(string codigo, string nome, string endereco, string bairro, string cnpj, string telefone, string rota)
        {

            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("CLIENTE");

            table.Columns.Add("CODCLI", typeof(string));
            table.Columns.Add("CNPJ", typeof(string));
            table.Columns.Add("RAZAO", typeof(string));
            table.Columns.Add("FANTASIA", typeof(string));
            table.Columns.Add("COMPRADOR", typeof(string));
            table.Columns.Add("ENDERECO", typeof(string));
            table.Columns.Add("BAIRRO", typeof(string));

            table.Columns.Add("CIDADE", typeof(string));
            table.Columns.Add("UF", typeof(string));
            table.Columns.Add("CEP", typeof(string));
            table.Columns.Add("TELEFONE1", typeof(string));
            table.Columns.Add("TELEFONE2", typeof(string));
            table.Columns.Add("INSCRICAO_ESTAD", typeof(string));

            table.Columns.Add("ROTA_NORM", typeof(string));
            table.Columns.Add("ROTA_ESPE", typeof(string));

            table.Columns.Add("STATUS", typeof(string));

            table.Columns.Add("SUBCANAL", typeof(string));
            table.Columns.Add("PRAZO_PAGTO", typeof(string));
            table.Columns.Add("TABELA_PRECO", typeof(string));
            table.Columns.Add("PROMOCAO", typeof(string));
            table.Columns.Add("PRIORIDADE", typeof(string));
            table.Columns.Add("VENDEDOR", typeof(string));
            table.Columns.Add("VENDEDOR_ESP", typeof(string));
            table.Columns.Add("EMAIL", typeof(string));

            table.Columns.Add("LIMITE_CREDITO", typeof(string));
            table.Columns.Add("LIMITE_UTILIZADO", typeof(string));

            table.Columns.Add("SEQ_VISITA_NORMAL", typeof(string));
            table.Columns.Add("SEQ_VISITA_ESPECI", typeof(string));

            try
            {
                /*PEGAR DADOS DE ROTA DO CLIENTE KNA1*/
                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC mestre clientes KNA1
                rfc_read_table.SetValue("QUERY_TABLE", "KNA1"); //Nome da Tabela
                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'                      

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "KUNNR");//Código do Cliente
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STCD1");//CNPJ
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "NAME1");//Razão Social
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "NAME2");//Nome Fantasia
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STRAS");//Endereço
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ORT02");//CPF
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "AUFSD");//Status    
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "MANDT");//Mandante
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ORT01"); //Cidade
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "REGIO"); //UF
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "PSTLZ"); //CEP
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "TELF1"); //TELEFONE 1
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "TELF2"); //TELEFONE 2
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "STCD3"); //INSCRIÇÃO EST.   
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "ADRNR");//CÓDIGO DO ENDEREÇO PARA PEGAR E-MAIL

                /*FIM PEGAR DADOS DE ROTA DO CLIENTE KNA1*/

                /*PEGAR DADOS DE ROTA DO CLIENTE KNVV*/
                IRfcFunction rfc_read_table2 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
                rfc_read_table2.SetValue("QUERY_TABLE", "KNVV"); //Nome da Tabela
                rfc_read_table2.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                var fieldsTable2 = rfc_read_table2.GetTable("FIELDS");
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_ROTA");
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_ROTAE");
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "KVGR3"); //SUBCANAL
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZTERM");//PRAZO
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "KVGR1");//TABELA
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "KONDA");//PROMOÇÃO
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "LPRIO");//PRIORIDADE
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_VEND");//VENDEDOR NORMAL
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_VENDE");//VENDEDOR ESPECIAL
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_SEQU");//SEQUENCIA VISITA
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "ZZ_SEQUE");//SEQUENCIA VISITA ESP
                /*FIM PEGAR DADOS DE ROTA DO CLIENTE KNVV*/

                /*PEGAR DADOS DE CONTATO*/
                IRfcFunction rfc_read_table3 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVk
                rfc_read_table3.SetValue("QUERY_TABLE", "KNVK"); //Nome da Tabela
                rfc_read_table3.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                var fieldsTable3 = rfc_read_table3.GetTable("FIELDS");
                fieldsTable3.Append();
                fieldsTable3.SetValue("FIELDNAME", "NAMEV");//Contato                         

                
                /*FIM PEGAR DADOS DE CONTATO*/

                /*PEGAR DADOS DO E-MAIL*/
                IRfcFunction rfc_read_table4 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
                rfc_read_table4.SetValue("QUERY_TABLE", "ADR6"); //Nome da Tabela
                rfc_read_table4.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                var fieldsTable4 = rfc_read_table4.GetTable("FIELDS");
                fieldsTable4.Append();
                fieldsTable4.SetValue("FIELDNAME", "SMTP_ADDR");//E-mail
                /*FIM PEGAR DADOS DO E-MAIL*/

                /*PEGAR DADOS DO LIMITE DE CRÉDITO*/
                IRfcFunction rfc_read_table5 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
                rfc_read_table5.SetValue("QUERY_TABLE", "KNKK"); //Nome da Tabela
                rfc_read_table5.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                var fieldsTable5 = rfc_read_table5.GetTable("FIELDS");
                fieldsTable5.Append();
                fieldsTable5.SetValue("FIELDNAME", "KLIMK");//limite de Crédito
                fieldsTable5.Append();
                fieldsTable5.SetValue("FIELDNAME", "SKFOR");//Limite utilizado
                /*FIM PEGAR DADOS DO LIMITE DE CRÉDITO*/


                /*PEGAR DESCRIÇÃO DO SUBCANAL*/
                IRfcFunction rfc_read_table6 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
                rfc_read_table6.SetValue("QUERY_TABLE", "TVV3T"); //Nome da Tabela
                rfc_read_table6.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                var fieldsTable6 = rfc_read_table6.GetTable("FIELDS");
                fieldsTable6.Append();
                fieldsTable6.SetValue("FIELDNAME", "BEZEI");//Contato
                /*FIM PEGAR DESCRIÇÃO DO SUBCANAL*/

                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");

                /*Verifica quais os parâmetros*/
                int conta_campos_preenchidos = 0;

                if (!codigo.Trim().Equals(""))
                {
                    tblOptions.Append();
                    tblOptions.SetValue("TEXT", "KUNNR EQ '" + codigo.ToString().PadLeft(10, '0') + "' ");
                    conta_campos_preenchidos++;
                }

                if (!nome.Trim().Equals(""))
                {
                    if (conta_campos_preenchidos <= 0)
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "NAME2 LIKE '" + nome + "%' ");
                        conta_campos_preenchidos++;
                    }
                    else
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "  AND NAME2 LIKE '" + nome + "%' ");
                        conta_campos_preenchidos++;
                    }

                }

                if (!endereco.Trim().Equals(""))
                {
                    if (conta_campos_preenchidos <= 0)
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "STRAS LIKE '%" + endereco + "%' ");
                        conta_campos_preenchidos++;
                    }
                    else
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", " AND  STRAS LIKE '%" + endereco + "%' ");
                        conta_campos_preenchidos++;
                    }

                }

                if (!bairro.Trim().Equals(""))
                {
                    if (conta_campos_preenchidos <= 0)
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "ORT02 LIKE '" + bairro + "%' ");
                        conta_campos_preenchidos++;
                    }
                    else
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "AND ORT02 LIKE '" + bairro + "%' ");
                        conta_campos_preenchidos++;
                    }

                }

                if (!cnpj.Trim().Equals(""))
                {
                    if (conta_campos_preenchidos <= 0)
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "(STCD1 EQ '" + cnpj + "' OR STCD2 EQ '" + cnpj + "')");
                        conta_campos_preenchidos++;
                    }
                    else
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", " AND (STCD1 EQ '" + cnpj + "' OR STCD2 EQ '" + cnpj + "')");
                        conta_campos_preenchidos++;
                    }

                }

                if (!telefone.Equals(""))
                {
                    if (conta_campos_preenchidos <= 0)
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", "TELF1 EQ '" + telefone + "' ");
                        conta_campos_preenchidos++;
                    }
                    else
                    {
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", " AND TELF1 EQ '" + telefone + "' ");
                        conta_campos_preenchidos++;
                    }
                }

                /*Em Caso de usuário informar a rota*/
                if (!rota.Trim().Equals(""))
                {
                    IRfcFunction rfc_read_table_rota = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC mestre clientes KNA1
                    rfc_read_table_rota.SetValue("QUERY_TABLE", "KNVV"); //Nome da Tabela
                    rfc_read_table_rota.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                    /*Passa os Campos que retornarão*/
                    var fieldsTable_rota = rfc_read_table_rota.GetTable("FIELDS");
                    fieldsTable_rota.Clear();
                    fieldsTable_rota.Append();
                    fieldsTable_rota.SetValue("FIELDNAME", "KUNNR");
                    IRfcTable tblOptions_rota = rfc_read_table_rota.GetTable("OPTIONS");
                    tblOptions_rota.Append();

                    tblOptions_rota.SetValue("TEXT", "VKORG = '1000' AND ZZ_ROTA EQ '" + rota + "' OR ZZ_ROTAE EQ '" + rota + "'");

                    /*Executa a RFC*/
                    rfc_read_table_rota.Invoke(dest);
                    IRfcTable returnTable_rota = rfc_read_table_rota.GetTable("DATA");

                    if (returnTable_rota.Count > 0)
                    {
                        string[] fieldValues_rota;

                        int cont_cli_rota = 0;

                        foreach (IRfcStructure Row in returnTable_rota)
                        {
                            fieldValues_rota = Row.GetValue("WA").ToString().Split(';');

                            if (conta_campos_preenchidos > 0 && cont_cli_rota == 0)//Caso já tenha outros campos selecionado no parametro e seja o primeiro giro no loop de clientes da rota
                            {
                                tblOptions.Append();
                                tblOptions.SetValue("TEXT", "AND (KUNNR EQ '" + fieldValues_rota[0] + "'");
                            }
                            else
                            {

                                if (cont_cli_rota == 0)//Caso não tenha campos selicionados no parâmetro e seja o primeiro giro no loop
                                {
                                    tblOptions.Append();
                                    tblOptions.SetValue("TEXT", "(KUNNR EQ '" + fieldValues_rota[0] + "'");
                                }
                                else
                                {
                                    tblOptions.Append();
                                    tblOptions.SetValue("TEXT", " OR KUNNR EQ '" + fieldValues_rota[0] + "'");
                                }
                            }

                            cont_cli_rota++;
                        }
                        tblOptions.Append();
                        tblOptions.SetValue("TEXT", ")");
                    }
                    else
                    {
                        /*Esta linha serve para passar um parâmetro inexitente, caso o usuário informe uma rota que não exista*/
                        if (conta_campos_preenchidos > 0)
                        {
                            tblOptions.Append();
                            tblOptions.SetValue("TEXT", "AND STCD1 EQ '11111111199999'");//Busca um CNPJ que não existe
                        }
                        else
                        {
                            tblOptions.Append();
                            tblOptions.SetValue("TEXT", "STCD1 EQ '11111111199999'");//Busca um CNPJ que não existe
                        }
                    }
                }

                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);



                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                string[] fieldValues;
                foreach (IRfcStructure Row in returnTable)
                {
                    fieldValues = Row.GetValue("WA").ToString().Split(';');

                    DataRow row = table.NewRow();

                    row["CODCLI"] = fieldValues[0];
                    row["CNPJ"] = fieldValues[1];
                    row["RAZAO"] = fieldValues[2];
                    row["FANTASIA"] = fieldValues[3];

                    /*Pegar as rotas do cliente na KNVk*/

                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions3 = rfc_read_table3.GetTable("OPTIONS");
                    tblOptions3.Clear();
                    tblOptions3.Append();
                    tblOptions3.SetValue("TEXT", "KUNNR EQ '" + fieldValues[0] + "' AND MANDT EQ '" + fieldValues[7] + "'");

                    /*Executa a RFC*/
                    rfc_read_table3.Invoke(dest);

                    /*Recebendo o retorno*/
                    IRfcTable returnTable3 = rfc_read_table3.GetTable("DATA");
                    if(returnTable3.Count >0 )
                    {
                        string[] fieldValues3;
                        fieldValues3 = returnTable3.GetValue("WA").ToString().Split(';');
                        row["COMPRADOR"] = fieldValues3[0]; 
                    }
                    else
                    {
                        row["COMPRADOR"] = "Sem informação de contato";
                    }
                   

                    /*Fim Pegar as rotas cliente KNVk*/


                    
                    row["ENDERECO"] = fieldValues[4];
                    row["BAIRRO"] = fieldValues[5];

                    row["CIDADE"] = fieldValues[8];
                    row["UF"] = fieldValues[9];
                    row["CEP"] = fieldValues[10];
                    row["TELEFONE1"] = fieldValues[11];
                    row["TELEFONE2"] = fieldValues[12];
                    row["INSCRICAO_ESTAD"] = fieldValues[13];

                    row["STATUS"] = fieldValues[6];

                    /*Pegar as rotas do cliente na KNVV*/

                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");
                    tblOptions2.Clear();
                    tblOptions2.Append();
                    tblOptions2.SetValue("TEXT", "VKORG = '1000' AND KUNNR EQ '" + fieldValues[0] + "' AND MANDT EQ '" + fieldValues[7] + "'");

                    /*Executa a RFC*/
                    rfc_read_table2.Invoke(dest);

                    /*Recebendo o retorno*/

                    IRfcTable returnTable2 = rfc_read_table2.GetTable("DATA");
                    string[] fieldValues2;
                    fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');



                    /*Fim Pegar as rotas cliente KNVV*/

                    row["ROTA_NORM"] = fieldValues2[0];
                    row["ROTA_ESPE"] = fieldValues2[1];

                    /*Pegar DESCRIÇÃO SUBCANAL*/

                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions6 = rfc_read_table6.GetTable("OPTIONS");
                    tblOptions6.Clear();
                    tblOptions6.Append();
                    tblOptions6.SetValue("TEXT", "KVGR3 EQ '" + fieldValues2[2] + "'");

                    /*Executa a RFC*/
                    rfc_read_table6.Invoke(dest);

                    /*Recebendo o retorno*/

                    IRfcTable returnTable6 = rfc_read_table6.GetTable("DATA");
                    string[] fieldValues6;
                    fieldValues6 = returnTable6.GetValue("WA").ToString().Split(';');

                    /*Fim Pegar DESCRIÇÃO SUBCANAL*/

                    row["SUBCANAL"] = fieldValues6[0];
                    row["PRAZO_PAGTO"] = fieldValues2[3].Substring(1, fieldValues2[3].Length - 1);
                    row["TABELA_PRECO"] = fieldValues2[4];
                    row["PROMOCAO"] = fieldValues2[5];
                    row["PRIORIDADE"] = fieldValues2[6];
                    row["VENDEDOR"] = fieldValues2[7];
                    row["VENDEDOR_ESP"] = fieldValues2[8];
                    row["SEQ_VISITA_NORMAL"] = fieldValues2[9];
                    row["SEQ_VISITA_ESPECI"] = fieldValues2[10];
                    /*Pegar o e-mail*/
                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions4 = rfc_read_table4.GetTable("OPTIONS");
                    tblOptions4.Clear();
                    tblOptions4.Append();
                    tblOptions4.SetValue("TEXT", "ADDRNUMBER EQ '" + fieldValues[13] + "'");

                    /*Executa a RFC*/
                    rfc_read_table4.Invoke(dest);

                    string[] fieldValues4;
                    string email = "";
                    /*Recebendo o retorno*/
                    IRfcTable returnTable4 = rfc_read_table4.GetTable("DATA");
                    if (returnTable4.Count > 0)
                    {
                        fieldValues4 = returnTable4.GetValue("WA").ToString().Split(';');
                        email = fieldValues4[0];
                    }


                    /*Fim pegar o e-mail*/

                    row["EMAIL"] = email;

                    /*Pegar o LIMITE DE CRÉDITO*/
                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions5 = rfc_read_table5.GetTable("OPTIONS");
                    tblOptions5.Clear();
                    tblOptions5.Append();
                    tblOptions5.SetValue("TEXT", "KUNNR EQ '" + fieldValues[0] + "' AND MANDT EQ '" + fieldValues[7] + "'");

                    /*Executa a RFC*/
                    rfc_read_table5.Invoke(dest);

                    string[] fieldValues5;

                    /*Recebendo o retorno*/
                    IRfcTable returnTable5 = rfc_read_table5.GetTable("DATA");
                    fieldValues5 = returnTable5.GetValue("WA").ToString().Split(';');
                    /*Fim pegar o LIMITE DE CRÉDITO*/

                    row["LIMITE_CREDITO"] = fieldValues5[0];
                    row["LIMITE_UTILIZADO"] = fieldValues5[1];

                    table.Rows.Add(row);
                }
            }
            catch(Exception e)
            {
                throw;
            }

            return table;
        }


        


        public DataTable listaComprasCliente(string codcli)
        {
            DataTable table = new DataTable("COMPRAS_CLIENTE");


            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("NUMERO_DOC");
            table.Columns.Add("ITEM_NOTA");
            table.Columns.Add("CODIGO_PROD");
            table.Columns.Add("PRODUTO");
            table.Columns.Add("QUANTIDADE");
            table.Columns.Add("DATA_DOC");
            table.Columns.Add("PROMOCAO");

            DataRow row = table.NewRow(); 

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SAP_WEB");
                funcBAPI.SetValue("COD_CLI", codcli.ToString().PadLeft(10, '0'));

                funcBAPI.Invoke(dest);

                IRfcTable compras = funcBAPI.GetTable("TCOMPRAS");

                if (compras.Count > 0)
                {                   

                    foreach (IRfcStructure detail in compras)
                    {
                        row = table.NewRow();

                        row["STATUS"] = "S";
                        row["MESSAGE"] = compras.Count + " Registros encontrados.";

                        row["NUMERO_DOC"] = detail.GetString("DOCNUM");
                        row["ITEM_NOTA"] = detail.GetString("ITMNUM");
                        row["CODIGO_PROD"] = detail.GetString("MATNR");
                        row["PRODUTO"] = detail.GetString("MAKTX");
                        row["QUANTIDADE"] = detail.GetString("MENGE");
                        row["DATA_DOC"] = detail.GetString("DOCDAT");
                        row["PROMOCAO"] = detail.GetString("KONDA");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Registros não encontrado para o cliente " + codcli;

                    table.Rows.Add(row);
                }
            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;

        }

        public DataTable listaPedidosVendedor(string codvend,string dti,string dtf)
        {
            DataTable table = new DataTable("PEDIDOS_VENDEDOR");

            table.Columns.Add("DOC_SAP");
            table.Columns.Add("DATA_DOC");
            table.Columns.Add("NOTA_NUM");
            table.Columns.Add("COD_CLIENTE");
            table.Columns.Add("RAZAO");
            table.Columns.Add("FANTASIA");
            table.Columns.Add("CFOP");
            table.Columns.Add("NF_TOTAL");
            table.Columns.Add("CANCEL");
            table.Columns.Add("STATUS_NF");
            table.Columns.Add("BONIF");

            try
            {

                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SW_PEDIDO");
                funcBAPI.SetValue("COD_VEND", codvend);
                funcBAPI.SetValue("PER_PEDIDO_INI", dti);
                funcBAPI.SetValue("PER_PEDIDO_FIM", dtf);

                funcBAPI.Invoke(dest);
                // string retorno= funcBAPI.GetString("E_RETURN");

                IRfcTable pedidos = funcBAPI.GetTable("TPEDIDOVENDA");

                DataRow row;

                foreach (IRfcStructure detail in pedidos)
                {
                    row = table.NewRow();
                    row["DOC_SAP"]      = detail.GetString("DOCNUM");
                    row["DATA_DOC"]     = detail.GetString("DOCDAT");
                    row["NOTA_NUM"]     = detail.GetString("NFENUM");
                    row["COD_CLIENTE"]  = detail.GetString("PARID");
                    row["RAZAO"]        = detail.GetString("NAME1");
                    row["FANTASIA"]     = detail.GetString("NAME2");
                    row["CFOP"]         = detail.GetString("CFOP");
                    row["NF_TOTAL"]     = detail.GetString("NFTOT");
                    row["CANCEL"]       = detail.GetString("CANCEL");
                    row["STATUS_NF"]    = detail.GetString("BEZEI");
                    row["BONIF"]        = detail.GetString("BONIF");
                    table.Rows.Add(row);
                }
            }
            catch(Exception err)
            {
                throw err;
            }

            table.DefaultView.Sort = "[DATA_DOC] DESC, [FANTASIA] ASC";
            table = table.DefaultView.ToTable(true);

            return table;

        }



        public DataTable listaItensCanceladosRetornadosClientes(string COD_CLI, string STATUS_CANC, string STATUS_RETORN)
        {                           

            DataTable table = new DataTable("ITENS_CANCELADOS_RETORNADOS_CLIENTE");

            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("NOTA_NUM");
            table.Columns.Add("DATA_DOC");
            table.Columns.Add("COD_PROD");
            table.Columns.Add("PRODUTO");
            table.Columns.Add("QTD");
            table.Columns.Add("VALOR");
            table.Columns.Add("MOTIVO");
            table.Columns.Add("DATA_PC");//DATA DA PRESTAÇÃO DE CONTAS
            table.Columns.Add("STATUS_CANC");
            table.Columns.Add("STATUS_RET");

            DataRow row = table.NewRow(); 

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SW_CANC_RET");
                funcBAPI.SetValue("COD_CLI", COD_CLI.ToString().PadLeft(10, '0'));
                funcBAPI.SetValue("STATUS_CANC", STATUS_CANC);
                funcBAPI.SetValue("STATUS_RETORN", STATUS_RETORN);

                funcBAPI.Invoke(dest);
                //string retorno = funcBAPI.GetString("E_RETURN");

                IRfcTable pedidos = funcBAPI.GetTable("TCANCRETORN");

                if (pedidos.Count > 0)
                {
                    foreach (IRfcStructure detail in pedidos)
                    {
                        row = table.NewRow();

                        row["STATUS"] = "S";
                        row["MESSAGE"] = pedidos.Count + " Registros encontrados.";

                        row["NOTA_NUM"] = detail.GetString("NFENUM");
                        row["DATA_DOC"] = detail.GetString("DOCDAT");
                        row["COD_PROD"] = detail.GetString("MATNR");
                        row["PRODUTO"] = detail.GetString("MAKTX");
                        row["QTD"] = detail.GetString("MENGE");
                        row["VALOR"] = detail.GetString("NETWR");
                        row["MOTIVO"] = detail.GetString("REASON1");//MOTIVO
                        row["STATUS_CANC"] = detail.GetString("STATUS_CANC");
                        row["STATUS_RET"] = detail.GetString("STATUS_RET");
                        row["DATA_PC"] = detail.GetString("CNGDATE");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] ="Registros não encontrados.";

                    table.Rows.Add(row);
                }
               
            }catch(Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            table.DefaultView.Sort = "[DATA_DOC] DESC";
            table = table.DefaultView.ToTable(true);

            return table;

        }


        public DataTable consultaTabelaGenerica(string tabl)
        {
            string A = null;
            /*Pega a estrutura da tabela, nome dos campos*/
            IRfcFunction estrut_tbl = repo.CreateFunction("/BODS/RFC_READ_TABLE");
            estrut_tbl.SetValue("QUERY_TABLE", tabl);
            estrut_tbl.Invoke(dest);
            IRfcTable campos = estrut_tbl.GetTable("FIELDS");

            string dados = null;
            IRfcFunction companyBapi = repo.CreateFunction("/BODS/RFC_READ_TABLE");
            companyBapi.SetValue("QUERY_TABLE", tabl);
            estrut_tbl.SetValue("ROWCOUNT", "2");
            companyBapi.Invoke(dest);
            IRfcTable Employeetable = companyBapi.GetTable("DATA");

            int contador = 0;

            DataTable table = new DataTable(tabl);
           
            foreach (IRfcStructure row in Employeetable)
            {
                contador++;
                DataRow Row = table.NewRow();

                foreach (IRfcStructure rowS in campos)
                {

                    string NOME_CAMPO = rowS.GetString("FIELDNAME").ToString();
                    int POS_INICIO = Convert.ToInt16(rowS.GetString("OFFSET"));
                    int TAMANHO = Convert.ToInt16(rowS.GetString("LENGTH"));
                    
                    string DESCRICAO = rowS.GetString("FIELDTEXT").ToString();

                    if (POS_INICIO >= 150)
                    {
                        POS_INICIO = 1;
                        TAMANHO = 3;
                    }

                    if (contador <= 1)
                    {
                        table.Columns.Add(NOME_CAMPO, typeof(string));
                    }

                    Row[NOME_CAMPO] = row.GetString("WA").ToString().Substring(POS_INICIO, TAMANHO);


                }

                table.Rows.Add(Row);
            }
            /*Finaliza Chamada no SAP*/
            RfcSessionManager.EndContext(dest);
            dest = null;

            return table;           
        }


        public DataTable SCD_PEGA_FUNCIONARIO(string centro,string matricula)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("FUNCIONARIO");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));
            table.Columns.Add("MATRICULA", typeof(string));
            table.Columns.Add("NOME", typeof(string));
            table.Columns.Add("GRUPO_CONTA_FUNC", typeof(string));
            table.Columns.Add("COD_SAP", typeof(string));
            table.Columns.Add("CENTRO", typeof(string));

            DataRow Row = table.NewRow();

            /*Buscar o cliente pela matricula de funcionario na KNVV*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "KNVV"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");

            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "KUNNR");//COD SAP 
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "EIKTO");//MATRICULA 
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "VWERK"); //Centro

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
            tblOptions.Clear();
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "VWERK EQ '" + centro.ToString().PadLeft(4, '0') + "' AND EIKTO EQ '" + matricula.ToString().PadLeft(8, '0') + "'");
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "AND (MANDT EQ '110' OR MANDT EQ'500')");

            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);

            string[] fieldValues;

            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                fieldValues = returnTable.GetValue("WA").ToString().Split(';');

                /*Buscar os dados pelo código SAP do cliente na KNA1*/
                IRfcFunction rfc_read_table2 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table2.SetValue("QUERY_TABLE", "KNA1"); //Nome da Tabela

                rfc_read_table2.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable2 = rfc_read_table2.GetTable("FIELDS");

                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "KUNNR"); //COD_SAP
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "NAME1"); //NOME
                fieldsTable2.Append();
                fieldsTable2.SetValue("FIELDNAME", "KTOKD"); //Grupo de Contas:      - Motorista / Z003 - Normal funcionários
                
                /*Faz a seleção do Registro*/
                IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");
                tblOptions2.Clear();
                tblOptions2.Append();
                tblOptions2.SetValue("TEXT", "KUNNR EQ '" + fieldValues[0] + "' AND (MANDT EQ '110' OR MANDT EQ'500')");
                /*Executa a RFC*/
                rfc_read_table2.Invoke(dest);

                string[] fieldValues2;
                IRfcTable returnTable2 = rfc_read_table2.GetTable("DATA");
                if (returnTable2.Count > 0)
                {
                    fieldValues2 = returnTable2.GetValue("WA").ToString().Split(';');

                    Row["STATUS"] = "S";
                    Row["MESSAGE"] = "Funcionario Localizado";
                    Row["COD_SAP"] = fieldValues2[0];
                    Row["NOME"] = fieldValues2[1];
                    Row["GRUPO_CONTA_FUNC"] = fieldValues2[2];
                    Row["MATRICULA"] = fieldValues[1];
                    Row["CENTRO"] = fieldValues[2];

                }
                else
                {                    
                    Row["STATUS"] = "E";
                    Row["MESSAGE"] = "Funcionario Não Localizado em KNA1-KUNNR";            
                }
            }
            else
            {
                Row["STATUS"] = "E";
                Row["MESSAGE"] = "Funcionario Não Localizado em KNVV-EIKTO";
            }

            table.Rows.Add(Row);
            return table;
        }


        public DataTable PegarCargasDisponiveisParaRecargaRapida(string data_inicial,string data_final)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("RETURN");
            	       
            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));
            table.Columns.Add("DOC_TRANSPORTE", typeof(string));
            table.Columns.Add("CARGA", typeof(string));
            table.Columns.Add("VEICULO", typeof(string));
            table.Columns.Add("DATA_DOC", typeof(string));
            table.Columns.Add("UNIDADE_TRANSPORTE", typeof(string)); 

            DataRow Row = table.NewRow();

             /*Buscar o cliente pela matricula de funcionario na KNVV*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "VTTK"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");

            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "TKNUM");//NUMERO DO TRANSPORTE
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "TPBEZ");//CARGA
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "/BEV1/RPMOWA");//VEICULO*/
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "DPTBG");//DATA CRIAÇÃO 
          //  fieldsTable.SetValue("FIELDNAME", "ERDAT");//DATA CRIAÇÃO: antes era essa linha porém mudei para pegar a data de saida 
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "TPLST");//UNIDADE DE TRANSPORTE
            

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
            tblOptions.Clear();
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "DPTBG >= '" + data_inicial + "' AND DPTBG <= '" + data_final + "'");
            tblOptions.Append();
            tblOptions.SetValue("TEXT", " AND (MANDT EQ '110' OR MANDT EQ'500') ");
            tblOptions.Append();
            tblOptions.SetValue("TEXT", " AND STTBG <> 'X'");
 
            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);

            string[] fieldValues;

            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                foreach(IRfcStructure rows in returnTable)
                {
                    fieldValues = rows.GetValue("WA").ToString().Split(';');

                    Row = table.NewRow();

                    Row["STATUS"] = "S";
                    Row["MESSAGE"] = returnTable.Count.ToString()+ " Cargas Localizadas no período: "+data_inicial+ " ate "+data_final;
                    Row["DOC_TRANSPORTE"] = fieldValues[0];
                    Row["CARGA"] = fieldValues[1];
                    Row["VEICULO"] = fieldValues[2];
                    Row["DATA_DOC"] = fieldValues[3];
                    Row["UNIDADE_TRANSPORTE"] = fieldValues[4];
  
                    table.Rows.Add(Row);
                }
            }
            else
            {
                Row["STATUS"] = "E";
                Row["MESSAGE"] = "Cargas não Localizadas no periodo: " + data_final + " ate "+data_final;

                table.Rows.Add(Row);
            }

           
            return table;

        }

        public DataTable listaCargasNaData(string data, string centro)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));

            table.Columns.Add("CENTRO", typeof(string));
            table.Columns.Add("DOC_TRANSPORTE", typeof(string));
            table.Columns.Add("CARGA", typeof(string));
            table.Columns.Add("VEICULO", typeof(string));
            table.Columns.Add("DATA", typeof(string));
            table.Columns.Add("CUBAGEM", typeof(string));
            table.Columns.Add("STATUS_CARGA", typeof(string));

            DataRow row=null;

            string retorno=null;

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF031");

                funcBAPI.SetValue("I_DATA", data);

                string centr = "";

                if (!centro.Equals(""))
                {
                    centr=centro.ToString().PadLeft(4, '0');
                }

                funcBAPI.SetValue("I_CENTRO", centr);
                
                funcBAPI.Invoke(dest);

                retorno = funcBAPI.GetString("E_MESSAGE");
               
                IRfcTable cargas = funcBAPI.GetTable("T_LISTA");                             
                
               

                if (cargas.Count > 0)
                {
                    foreach (IRfcStructure detail in cargas)
                    {
                        row = table.NewRow();

                        row["STATUS"] = "S";
                        row["MESSAGE"] = "Dados encontrados com sucesso!";

                        row["CENTRO"] = detail.GetString("CENTRO");
                        row["DOC_TRANSPORTE"] = detail.GetString("TRANSPORTE");
                        row["CARGA"] = detail.GetString("CARGA");
                        row["VEICULO"] = detail.GetString("VEICULO");
                        row["DATA"] = detail.GetString("DATA");
                        row["CUBAGEM"] = detail.GetString("CUBAGEM");
                        row["STATUS_CARGA"] = detail.GetString("STATUS");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row = table.NewRow();

                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Carga(s) nao encontrada(s) na data: " + data + " e centro:" + centro + "/ ERRO:" + retorno;

                    table.Rows.Add(row);
                }
               
            }catch(Exception e)
            {
                row = table.NewRow();

                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message.ToString() + "/ ERRO:" + retorno; ;

                table.Rows.Add(row);
            }

            

            return table;
        }

        public DataTable listaProdutosClienteFaltaEstoque(string COD_CLI)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));

            table.Columns.Add("DATA_DOC", typeof(string));
            table.Columns.Add("ROTA", typeof(string));
            table.Columns.Add("VENDEDOR", typeof(string));
            table.Columns.Add("DESC_VEND", typeof(string));
            table.Columns.Add("COD_PROD", typeof(string));
            table.Columns.Add("DESC_PROD", typeof(string));
            table.Columns.Add("QUANTIDADE", typeof(string));

            DataRow row = table.NewRow();


            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SW_PEDFALTAESTOQUE");

                funcBAPI.SetValue("COD_CLI", COD_CLI.ToString().PadLeft(10, '0'));
               
                funcBAPI.Invoke(dest);

                IRfcTable itens = funcBAPI.GetTable("Z_PEDFALTESTOQ");

                if (itens.Count > 0)
                {
                    foreach (IRfcStructure detail in itens)
                    {
                        row = table.NewRow();

                        row["STATUS"] = "S";
                        row["MESSAGE"] = "Dados encontrados com sucesso!";

                        row["DATA_DOC"] = detail.GetString("DTDOC");
                        row["ROTA"] = detail.GetString("ROTA");
                        row["VENDEDOR"] = detail.GetString("VENDEDOR");
                        row["DESC_VEND"] = detail.GetString("DESC_VEND");
                        row["COD_PROD"] = detail.GetString("CODMAT");
                        row["DESC_PROD"] = detail.GetString("DESCMAT");
                        row["QUANTIDADE"] = detail.GetString("KWMENG");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Iten(s) nao encontrado(s) para o cliente  " + COD_CLI ;

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            { 
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message.ToString() ;

                table.Rows.Add(row);
            }



            return table;


        }




        public DataTable ListarGerencias(string cod_gerencia)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("GERENCIA");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));

            table.Columns.Add("CODIGO", typeof(string));
            table.Columns.Add("GERENCIA", typeof(string));           

            DataRow Row = table.NewRow();

            try
            {

                /*Buscar o cliente pela matricula de funcionario na KNVV*/
                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "T151T"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");

                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "KDGRP");//NUMERO DO TRANSPORTE
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "KTEXT");//CARGA            

                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Clear();
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "SPRAS EQ 'P'");
                /*Faz a seleção do Registro*/
                if (!cod_gerencia.Equals(""))
                {

                    tblOptions.Append();
                    tblOptions.SetValue("TEXT", "AND KDGRP EQ '" + cod_gerencia + "'");
                }

                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);

                string[] fieldValues;

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                if (returnTable.Count > 0)
                {
                    foreach (IRfcStructure rows in returnTable)
                    {
                        fieldValues = rows.GetValue("WA").ToString().Split(';');

                        Row = table.NewRow();

                        Row["STATUS"] = "S";
                        Row["MESSAGE"] = returnTable.Count.ToString() + " Registros encontrados";

                        Row["CODIGO"] = fieldValues[0];
                        Row["GERENCIA"] = fieldValues[1];

                        table.Rows.Add(Row);
                    }
                }
                else
                {
                    Row["STATUS"] = "E";
                    Row["MESSAGE"] = "Nenhum registro encontrado!";

                    table.Rows.Add(Row);
                }
            }
            catch (Exception e)
            {
                 Row["STATUS"] = "E";
                 Row["MESSAGE"] = e.Message;

                 table.Rows.Add(Row);
                
            }

            table.DefaultView.Sort = "[GERENCIA] ASC";
            table = table.DefaultView.ToTable(true);

            return table;

        }


        public DataTable ListaCoordenadores( string cod_coord)
        {
            DataTable table = new DataTable("COORDENADOR");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));

            table.Columns.Add("CODIGO", typeof(string));
            table.Columns.Add("COD_SAP", typeof(string));  
            table.Columns.Add("NOME", typeof(string));   

            DataRow Row = table.NewRow();

            /*Buscar o cliente pela matricula de funcionario na KNA1 - Cliente */
            IRfcFunction rfc_read_table_kna1 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table_kna1.SetValue("QUERY_TABLE", "KNA1"); //Nome da Tabela

            rfc_read_table_kna1.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable_kna1 = rfc_read_table_kna1.GetTable("FIELDS");

            fieldsTable_kna1.Append();
            fieldsTable_kna1.SetValue("FIELDNAME", "NAME1");//Nome           


            /*Buscar o cliente pela matricula de funcionario na ZDSDT023 - Coordenador*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "ZDSDT023"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");

            fieldsTable.Append();           
            fieldsTable.SetValue("FIELDNAME", "COORDENADOR");//Código do Coordenador 
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "COD_SAP");//Código Sap 

            if (!cod_coord.Equals(""))
            {
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Clear();
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "COORDENADOR EQ '" + cod_coord + "'");
            }


            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);

            string[] fieldValues;

            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                foreach (IRfcStructure rows in returnTable)
                {
                    fieldValues = rows.GetValue("WA").ToString().Split(';');

                    Row = table.NewRow();

                    Row["STATUS"] = "S";
                    Row["MESSAGE"] = returnTable.Count.ToString() + " Coordenadores encontrados";
                    Row["CODIGO"] = fieldValues[0];
                    Row["COD_SAP"] = fieldValues[1];

                    string NOME = "Z/ Nome indefinido! Sem código SAP cadastrado na ZDSDT023 para o coordenador " + fieldValues[0] + ".";

                    /*Pegar o nome do Coordenador na KNA1*/
                    if (!fieldValues[1].ToString().Trim().Equals(""))
                    {
                        IRfcTable tblOptions_kna1 = rfc_read_table_kna1.GetTable("OPTIONS");
                        tblOptions_kna1.Clear();
                        tblOptions_kna1.Append();
                        tblOptions_kna1.SetValue("TEXT", "KUNNR EQ '" + fieldValues[1] + "'");
                        tblOptions_kna1.Append();
                        tblOptions_kna1.SetValue("TEXT", " AND (MANDT EQ '110' OR MANDT EQ'500') ");

                        rfc_read_table_kna1.Invoke(dest);

                        IRfcTable returnTable_kna1 = rfc_read_table_kna1.GetTable("DATA");                       

                        if (returnTable_kna1.Count > 0)
                        {
                            string[] fieldValues_kna1 = returnTable_kna1.GetValue("WA").ToString().Split(';');
                            NOME = fieldValues_kna1[0];
                        }
                    }
              
                    Row["NOME"] = NOME;

                    table.Rows.Add(Row);
                }
            }
            else
            {
                Row["STATUS"] = "E";
                Row["MESSAGE"] = "Cargas não Localizadas no periodo: " ;

                table.Rows.Add(Row);
            }

            table.DefaultView.Sort = "[NOME] ASC";
            table = table.DefaultView.ToTable(true);

            return table;


        }


        public DataTable bonificacoes(string agrupamento, string valor,string dt_ini, string dt_fim)
        {
            /** BUSCA AS BONIFICAÇÕES DE ACORDO COM OS PARÂMETROS
             * Agrupamento:
             *  G=Gerencia / C=Coordenador / V=Vendedor / T=Todos
             *  Valor: referente a seleção do Agrupamento
             *  dt_ini e dt_fim: Período do pedido             
             */
            DataTable table = new DataTable("BONIFICACOES");

            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
         
            table.Columns.Add("DATA_DOC");
            table.Columns.Add("COD_PROD");
            table.Columns.Add("PRODUTO");
            table.Columns.Add("QTD");
            table.Columns.Add("VENDEDOR");
            table.Columns.Add("COD_VEND");
            table.Columns.Add("MATRICULA");
            table.Columns.Add("COD_CLI");
            table.Columns.Add("COD_MOTIVO");
            table.Columns.Add("DESC_MOTIVO");
            table.Columns.Add("GERENCIA");
            table.Columns.Add("COORDENADOR");
            table.Columns.Add("FANTASIA");  
            table.Columns.Add("EXPORTADO");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SW_BONIFICACAO");

                funcBAPI.SetValue("AGRUPAMENTO", agrupamento);
                funcBAPI.SetValue("CODIGO",valor );
                funcBAPI.SetValue("DTINI_PEDIDO", dt_ini);
                funcBAPI.SetValue("DTFIM_PEDIDO",dt_fim );

                funcBAPI.Invoke(dest);
               // string retorno = funcBAPI.GetString("E_RETURN");

                IRfcTable bonificacao = funcBAPI.GetTable("TBONIFICACAO");

                if (bonificacao.Count > 0)
                {
                    foreach (IRfcStructure detail in bonificacao)
                    {
                        row = table.NewRow();

                        row["STATUS"] = "S";
                        row["MESSAGE"] = bonificacao.Count + " Registros encontrados.";

                        row["DATA_DOC"] = detail.GetString("AUDAT");
                        row["COD_PROD"] = detail.GetString("MATNR");
                        row["PRODUTO"] = detail.GetString("ARKTX");
                        row["QTD"] = detail.GetString("KWMENG");
                        row["VENDEDOR"] = detail.GetString("DESC_VEND");
                        row["COD_VEND"] = detail.GetString("ZZ_VEND");
                        row["MATRICULA"] = detail.GetString("EIKTO");
                        row["COD_CLI"] = detail.GetString("KUNNR");
                        row["COD_MOTIVO"] = detail.GetString("AUGRU");
                        row["DESC_MOTIVO"] = detail.GetString("BEZEI");
                        row["GERENCIA"] = detail.GetString("KDGRP");
                        row["COORDENADOR"] = detail.GetString("ZZ_COOR");
                        row["FANTASIA"] = detail.GetString("NAME2");
                        row["EXPORTADO"] = detail.GetString("EXPORTA");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Registros não encontrados.";

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            table.DefaultView.Sort = "[VENDEDOR] ASC";
            table = table.DefaultView.ToTable(true);

            return table;
        }

        public DataTable verifica_quantidade_estoque(string cod_prod,string centro)
        {
            DataTable table = new DataTable("ESTOQUE");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            
            table.Columns.Add("CENTRO");
            table.Columns.Add("COD_PROD");
            table.Columns.Add("QUANTIDADE");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SALDO_ESTOQUE");

                /*Parâmetros 1*/
                funcBAPI.SetValue("DOC_DATE", "");
                
                /*Parâmetros 2 - Tabela*/
                IRfcTable objEstrutura = funcBAPI.GetTable("TG_ITENS");
                objEstrutura.Append();
                objEstrutura.SetValue("MATNR", cod_prod.ToString().PadLeft(18, '0'));//Produto
                objEstrutura.SetValue("WERKS", centro);//Centro
                objEstrutura.SetValue("LGORT", "APA");//Centro
                objEstrutura.SetValue("ENTRY_QNT", "99.999.999,999");//Centro

                funcBAPI.Invoke(dest);

                IRfcTable table_return = funcBAPI.GetTable("TG_ITENS");

                row["STATUS"] = "S";
                row["MESSAGE"] = "Registro encontrado.";
               
                row["CENTRO"] = centro;
                row["COD_PROD"] = table_return.GetString("MATNR");
                row["QUANTIDADE"] = table_return.GetString("AVAIL_QNT");

                table.Rows.Add(row);
            

            }catch(Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }


            return table;
        }


        public DataTable verifica_quantidade_estoque_ecommerce(string cod_prod, string centro)
        {
            DataTable table = new DataTable("ESTOQUE");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("CENTRO");
            table.Columns.Add("COD_PROD");
            table.Columns.Add("QUANTIDADE");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SALDO_ESTOQUE");

                /*Parâmetros 1*/
                funcBAPI.SetValue("DOC_DATE", "");

                /*Parâmetros 2 - Tabela*/
                IRfcTable objEstrutura = funcBAPI.GetTable("TG_ITENS");
                objEstrutura.Append();
                objEstrutura.SetValue("MATNR", cod_prod.ToString().PadLeft(18, '0'));//Produto
                objEstrutura.SetValue("WERKS", centro);//Centro
                objEstrutura.SetValue("LGORT", "APA");//Centro
                objEstrutura.SetValue("ENTRY_QNT", "99.999.999,999");//Centro

                funcBAPI.Invoke(dest);

                IRfcTable table_return = funcBAPI.GetTable("TG_ITENS");

                row["STATUS"] = "S";
                row["MESSAGE"] = "Registro encontrado.";

                row["CENTRO"] = centro;
                row["COD_PROD"] = table_return.GetString("MATNR");
                row["QUANTIDADE"] = table_return.GetString("AVAIL_QNT");

                table.Rows.Add(row);


            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }


            return table;
        }

        public DataTable criarPedidosBalcaoVendas(string COD_CLIENTE, string CONDICAO_PAGMTO,string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO,string TIPO_DOC, string DEPOSITO, string NOME_CLIENTE, string CPF, string PRODUTOS )
        {
            //Log
            string param;
           Stream saida = File.Open(@"c:\temp\Log_balcao.txt", FileMode.Append);
           StreamWriter escritor = new StreamWriter(saida);
           
           escritor.WriteLine(DateTime.Now+":");
         
            
            DataTable table = new DataTable("PEDIDO");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("ORDEM");
            table.Columns.Add("FORNECIMENTO");
            table.Columns.Add("FATURA");

            DataRow row = table.NewRow();
            param = "COD_CLIENTE:" + COD_CLIENTE + "/CONDICAO_PAGMTO:" + CONDICAO_PAGMTO + " /ORG_VENDAS:" + ORG_VENDAS + " /CANAL_DIST:" + CANAL_DIST +
                  " /SETOR_ATV:" + SETOR_ATV + " /CENTRO:" + CENTRO + " /TIPO_DOC:" + TIPO_DOC + " /TIPO_DOC: /DEPOSITO:" + DEPOSITO + " /NOME_CLIENTE:" + NOME_CLIENTE +
                  " /CPF:" + CPF + " / ITENS[" + PRODUTOS + "]";
                  
            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFCVENDADEBALCAO");

                /*Parâmetros 1 - Dados de Venda*/                
                funcBAPI.SetValue("P_KUNNR", COD_CLIENTE);         //Código do Cliente         - opcional
                funcBAPI.SetValue("P_ZLSCH", CONDICAO_PAGMTO);     // Condição de Pagamento    - Obrigatorio
                funcBAPI.SetValue("P_VKORG", ORG_VENDAS);          // Organização de Vendas    - Fixo 10000
                funcBAPI.SetValue("P_VTWEG", CANAL_DIST);          // Canal de Distribuição    - Fixo 01
                funcBAPI.SetValue("P_SPART", SETOR_ATV);           // Setor de atividade       - Fixo 01
                funcBAPI.SetValue("P_WERKS", CENTRO);              // Centro                   - Fixo 0008
                funcBAPI.SetValue("P_AUART", TIPO_DOC);            // Tipo de documento        - Fixo ZS44
                funcBAPI.SetValue("P_KVGR1", "");                  //Não utilizado neste momento
                funcBAPI.SetValue("P_LGORT", DEPOSITO);            // Depósito                 - Fixo APA
                funcBAPI.SetValue("P_NAME1", NOME_CLIENTE);        // Nome do Cliente          - Opcional
                funcBAPI.SetValue("P_STCD1", CPF);                 //CPF                       - Obrigatório

                /*Parâmetros 2 - Produtos Vendidos*/
                IRfcTable objEstrutura = funcBAPI.GetTable("T_MATNR");
               
               
                

                string[] linhas = Regex.Split(PRODUTOS, "/");
                string[] Colunas;
                string produto;
                string quantidade;

                for (int i = 0; i < linhas.Length;i++ )
                {
                    Colunas = Regex.Split(linhas[i], ":");

                    produto = Colunas[0];
                    quantidade = Colunas[1];
                    
                    objEstrutura.Append();
                    objEstrutura.SetValue("MANDT", "500");                                      //Mandante
                    objEstrutura.SetValue("MATERIAL", Colunas[0].ToString().PadLeft(18, '0'));  //Material                
                    objEstrutura.SetValue("QUANTIDADE", Colunas[1]);                            //Quantidade  
                }

                                 

                funcBAPI.Invoke(dest);

                string TESTE=   funcBAPI.GetString("VBELN");

                IRfcTable table_return_erro = funcBAPI.GetTable("IT_RETURN2"); //TAbela de erros
                
                //VErifica se retornou erro 
                if(table_return_erro.RowCount<=0)
                {//CAso Não teve erro
                    //Mensagem de sucesso e dados do pedido
                    row["STATUS"] = "S";
                    row["MESSAGE"] = "Pedido Criado com sucesso!";
                    row["ORDEM"] = funcBAPI.GetString("VBELN");
                    row["FORNECIMENTO"] = funcBAPI.GetString("VBELN_VL");
                    row["FATURA"] = funcBAPI.GetString("VBELN_VF"); 

                    //Log                    
                    escritor.WriteLine("Pedido Criado com sucesso: Ordem:" + row["ORDEM"] + "=>PARAMETROS:" + param);
                    escritor.Close();
                }
                else
                {//Caso tenha erros
                    //Mensgem de erro
                    row["STATUS"] = "E";
                    string monta = "";
                    foreach (IRfcStructure detail in table_return_erro)
                    {
                        monta += detail.GetString("MESSAGE")+" / ";
                    }

                    //Log                    
                    escritor.WriteLine("Erro: Ordem:" + monta + "=>PARAMETROS:" + param);
                    escritor.Close();


                    row["MESSAGE"] = monta;

                   
                }
                               
                table.Rows.Add(row);


            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);

                //Log                    
                escritor.WriteLine("Erro: Ordem:" + row["MESSAGE"] + "=>PARAMETROS:" + param);
                escritor.Close();
            }


            return table;
        }

        //e-commerce
        public DataTable criarPedidosVendasEcommerce(string EMPRESA, string COD_CLIENTE, string CONDICAO_PAGMTO, string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO, string TIPO_DOC, string TBL_PRECO, string NOME_CLIENTE, string CPF, string PRODUTOS, string ENDERECO)
        {
            //Log
            string param;
            Stream saida = File.Open(@"c:\temp\Log_ecommerce.txt", FileMode.Append);
            StreamWriter escritor = new StreamWriter(saida);

            escritor.WriteLine(DateTime.Now + ":");


            DataTable table = new DataTable("PEDIDO");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("ORDEM");
            table.Columns.Add("FORNECIMENTO");
            table.Columns.Add("FATURA");

            DataRow row = table.NewRow();
            param = "EMPRESA:" + EMPRESA + " /COD_CLIENTE:" + COD_CLIENTE + " /CONDICAO_PAGMTO:" + CONDICAO_PAGMTO + " /ORG_VENDAS:" + ORG_VENDAS + " /CANAL_DIST:" + CANAL_DIST +
                  " /SETOR_ATV:" + SETOR_ATV + " /CENTRO:" + CENTRO + " /TIPO_DOC:" + TIPO_DOC + " /TABELA PREÇO:" + TBL_PRECO +  " /NOME_CLIENTE:" + NOME_CLIENTE +
                  " /CPF:" + CPF + " / ITENS[" + PRODUTOS + "]" + " /ENDERECO[" + ENDERECO + "]";

            try
            {
                /*Chama a RFC*/
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFCVENDAECOMMERCE");

                /*Parâmetros 1 - Dados de Venda*/
                funcBAPI.SetValue("P_BUKRS", EMPRESA);             // Empresa que efetua o faturamento  - opcional
                funcBAPI.SetValue("P_KUNNR", COD_CLIENTE);         // Código do Cliente           - opcional
                funcBAPI.SetValue("P_ZLSCH", CONDICAO_PAGMTO);     // Condição de Pagamento       - Obrigatorio
                funcBAPI.SetValue("P_VKORG", ORG_VENDAS);          // Organização de Vendas       - Fixo 10000
                funcBAPI.SetValue("P_VTWEG", CANAL_DIST);          // Canal de Distribuição       - Fixo 01
                funcBAPI.SetValue("P_SPART", SETOR_ATV);           // Setor de atividade          - Fixo 01
                funcBAPI.SetValue("P_WERKS", CENTRO);              // Centro                      - Fixo 0008
                funcBAPI.SetValue("P_AUART", TIPO_DOC);            // Tipo de documento           - Fixo ZS44
                funcBAPI.SetValue("P_KVGR1", TBL_PRECO);           // Tabela de Preço             - opcional
               // funcBAPI.SetValue("P_LGORT", DEPOSITO);            // Depósito                    - Fixo APA
                funcBAPI.SetValue("P_NAME1", NOME_CLIENTE);        // Nome do Cliente             - Opcional
                funcBAPI.SetValue("P_STCD1", CPF);                 // CPF                         - Obrigatório
                
                /*Parâmetros 2 - Produtos Vendidos*/
                IRfcTable objEstrutura = funcBAPI.GetTable("T_MATNR");

                string[] linhas = Regex.Split(PRODUTOS, "/");
                string[] Colunas;

                for (int i = 0; i < linhas.Length; i++)
                {
                    Colunas = Regex.Split(linhas[i], ":");

                    objEstrutura.Append();
                                                 
                    objEstrutura.SetValue("MATERIAL", Colunas[0].ToString().PadLeft(18, '0'));  //Material                
                    objEstrutura.SetValue("QUANTIDADE", Colunas[1]);                            //Quantidade
                    objEstrutura.SetValue("VALOR", Colunas[2]);                                 //Valor
                    objEstrutura.SetValue("DESCIT", Colunas[3]);                                //DESC IT
                    objEstrutura.SetValue("DESCCP", Colunas[4]);                                //DESC CP
                    objEstrutura.SetValue("VL_FRETE", Colunas[5]);                              //VALOR FRETE
                }

                /*Parâmetros 3 - Endereço de Entrega*/
                IRfcTable objEstrutura2 = funcBAPI.GetTable("T_ADDR1");

                string[] linhas_end = Regex.Split(ENDERECO, "/"); //Linhas(registros) são separadas por / (barras)
                string[] Colunas_end;

                for (int i = 0; i < linhas_end.Length; i++)
                {
                    Colunas_end = Regex.Split(linhas_end[i], ":"); //Colunas são separadas por : (dois pontos)

                    objEstrutura2.Append();

                    objEstrutura2.SetValue("STREET", Colunas_end[0]);                               //Endereço                
                    objEstrutura2.SetValue("HOUSE_NUM1", Colunas_end[1]);                           //Número
                    objEstrutura2.SetValue("HOUSE_NUM2", Colunas_end[2]);                           //Tipo Residência
                    objEstrutura2.SetValue("CITY1", Colunas_end[3]);                                //Cidade
                    objEstrutura2.SetValue("CITY2", Colunas_end[4]);                                //Bairro
                    objEstrutura2.SetValue("POST_CODE1", Colunas_end[5]);                           //CEP
                    objEstrutura2.SetValue("REGION", Colunas_end[6]);
                    objEstrutura2.SetValue("TEL_NUMBER", Colunas_end[7]);                           //Telefone                    
                }
                

                funcBAPI.Invoke(dest);

                string fatura = funcBAPI.GetString("VBELN_VF");

                IRfcTable table_return_erro = funcBAPI.GetTable("IT_RETURN2"); //TAbela de erros

                //VErifica se retornou erro 
                if (!fatura.Equals(""))
                {//CAso Não teve erro
                    //Mensagem de sucesso e dados do pedido
                    row["STATUS"] = "S";
                    row["MESSAGE"] = "Pedido Criado com sucesso!";
                    row["ORDEM"] = funcBAPI.GetString("VBELN");
                    row["FORNECIMENTO"] = funcBAPI.GetString("VBELN_VL");
                    row["FATURA"] = funcBAPI.GetString("VBELN_VF");

                    //Log                    
                    escritor.WriteLine("Pedido Criado com sucesso: Ordem:" + row["ORDEM"] + "=>PARAMETROS:" + param);
                    escritor.Close();
                }
                else
                {//Caso tenha erros
                    //Mensgem de erro
                    row["STATUS"] = "E";
                    string monta = "";
                    foreach (IRfcStructure detail in table_return_erro)
                    {
                        monta += detail.GetString("MESSAGE") + " / ";
                    }

                    //Log                    
                    escritor.WriteLine("Erro: Ordem:" + monta + "=>PARAMETROS:" + param);
                    escritor.Close();


                    row["MESSAGE"] = monta;


                }

                table.Rows.Add(row);


            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);

                //Log                    
                escritor.WriteLine("Erro: Ordem:" + row["MESSAGE"] + "=>PARAMETROS:" + param);
                escritor.Close();
            }


            return table;
        }

  ///

        public DataTable consultasEcommerce(string P_KAPPL, string P_KSCHL, string P_WERKS, string P_REGIO, string P_ZZKVGR1, string P_KOPOS, string P_LGORT, string P_DATAB, string P_PRECO, string MATNR)
        {           

            DataTable table = new DataTable("CONSULTAPRODUTOS");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("SKU");
            table.Columns.Add("NAME");
            table.Columns.Add("ACTIVE");
            table.Columns.Add("QTY");
            table.Columns.Add("FINAL_PRICE");
            table.Columns.Add("MIN_PRICE");
            table.Columns.Add("MAX_PRICE");
            table.Columns.Add("TIER_PRICE");
            table.Columns.Add("DT_ATUALIZACAO");
            table.Columns.Add("TYPE");
            table.Columns.Add("SITUACAO");            
                                           

            DataRow row = table.NewRow();           

            try
            {
                /*Chama a RFC*/
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFCCONSULTASECOMMERCE");   

                /*Parâmetros 1 - Dados de Venda*/
                funcBAPI.SetValue("P_KAPPL", P_KAPPL);            
                funcBAPI.SetValue("P_KSCHL", P_KSCHL);         
                funcBAPI.SetValue("P_WERKS", P_WERKS);     
                funcBAPI.SetValue("P_REGIO", P_REGIO);        
                funcBAPI.SetValue("P_ZZKVGR1", P_ZZKVGR1);         
                funcBAPI.SetValue("P_KOPOS", P_KOPOS);           
                funcBAPI.SetValue("P_LGORT", P_LGORT);           
                funcBAPI.SetValue("P_DATAB", P_DATAB);           
                funcBAPI.SetValue("P_PRECO", P_PRECO);          
               
                /*Parâmetros 2 - MATERIAL*/
                if (!MATNR.ToString().Trim().Equals(""))
                {
                    IRfcTable objEstrutura = funcBAPI.GetTable("T_MATNR");

                    string[] Produto = Regex.Split(MATNR, ":");

                    for (int i = 0; i < Produto.Length; i++)
                    {
                        objEstrutura.Append();
                        objEstrutura.SetValue("MATERIAL", Produto[i].ToString().PadLeft(18, '0'));  //Material    
                    }

                }

                funcBAPI.Invoke(dest);
               

                IRfcTable objEstrutura2 = funcBAPI.GetTable("T_DADOS");

                foreach (IRfcStructure detail in objEstrutura2)
                {
                    row = table.NewRow();

                    row["STATUS"] = "S";
                    row["MESSAGE"] = "Operação realizada";

                    row["SKU"] = detail.GetString("SKU");
                    row["NAME"] = detail.GetString("NAME");
                    row["ACTIVE"] = detail.GetString("ACTIVE");
                    row["QTY"] = detail.GetString("QTY");
                    row["FINAL_PRICE"] = detail.GetString("FINAL_PRICE");
                    row["MIN_PRICE"] = detail.GetString("MIN_PRICE");
                    row["MAX_PRICE"] = detail.GetString("MAX_PRICE");
                    row["TIER_PRICE"] = detail.GetString("TIER_PRICE");
                    row["DT_ATUALIZACAO"] = detail.GetString("DT_ATUALIZACAO");
                    row["TYPE"] = detail.GetString("TYPE");
                    row["SITUACAO"] = detail.GetString("MESSAGE");   

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);               
            }


            return table;
        }
///




        //e-commerce


        public DataTable itensCriticos(string centro,string produtos)
        {

            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            DataRow row = table.NewRow();            

            try
            { 
                IRfcFunction funcBAPI   = repo.CreateFunction("ZDSDF033");

                IRfcTable objEstrutura  = funcBAPI.GetTable("T_MATERIAL");

                if (!String.IsNullOrEmpty(produtos))
                {
                    string valor = produtos;

                    string[] linhas = Regex.Split(valor, ",");

                    for (int i = 0; i <= linhas.Length - 1; i++)
                    {
                   
                        objEstrutura.Append();
                        objEstrutura.SetValue("MATNR", linhas[i]);
                        objEstrutura.SetValue("WERKS", centro);
                    }

                    funcBAPI.Invoke(dest);                  
                    row["STATUS"] = funcBAPI.GetString("E_STATUS");
                    row["MESSAGE"]= linhas.Length.ToString()+" "+ funcBAPI.GetString("E_MESSAGE");
                    table.Rows.Add(row);


                }
                else
                {
                    /*Apaga os registros para o Centro informado*/
                    objEstrutura.Append();
                    objEstrutura.SetValue("MATNR", "");
                    objEstrutura.SetValue("WERKS", centro);

                    funcBAPI.Invoke(dest); 

                    row["STATUS"]  = "E";
                    row["MESSAGE"] = "Produtos Críticos não informado!";

                    table.Rows.Add(row);
                }


            }catch(Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);        
            }

            return table;            
        }


        public DataTable itensPedido(string docnum)
        {
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("DOC_NUMERO");
            table.Columns.Add("ITEM_NUM");
            table.Columns.Add("COD_MATERIAL");
            table.Columns.Add("DESC_MATERIAL");
            table.Columns.Add("QUANTIDADE");
            table.Columns.Add("VALOR");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_SW_PEDIDO_ITEM");

                funcBAPI.SetValue("I_DOCNUM", docnum);

                funcBAPI.Invoke(dest);
              
                IRfcTable objEstrutura = funcBAPI.GetTable("T_ITENS");

                foreach (IRfcStructure detail in objEstrutura)
                {
                    row = table.NewRow();

                    row["STATUS"]           = "S";
                    row["MESSAGE"]          = funcBAPI.GetString("E_MESSAGE");
                    row["DOC_NUMERO"]       = detail.GetString("DOCNUM");
                    row["ITEM_NUM"]         = detail.GetString("ITMNUM");
                    row["COD_MATERIAL"]     = detail.GetString("MATNR");
                    row["DESC_MATERIAL"]    = detail.GetString("MAKTX");
                    row["QUANTIDADE"]       = detail.GetString("MENGE");
                    row["VALOR"]            = detail.GetString("NETWR"); 

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }



        public DataTable PEGA_HORA_PEDIDO(string n_pedido)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("PROCESSAMENTO_SAP");

            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("MESSAGE", typeof(string));
            table.Columns.Add("DATA", typeof(string));
            table.Columns.Add("HORA", typeof(string));
            table.Columns.Add("ORDEM", typeof(string));
            

            DataRow Row = table.NewRow();

            /*Buscar o cliente pela matricula de funcionario na KNVV*/
            IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

            rfc_read_table.SetValue("QUERY_TABLE", "VBAK"); //Nome da Tabela

            rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

            /*Passa os Campos que retornarão*/
            var fieldsTable = rfc_read_table.GetTable("FIELDS");

            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "ERDAT");//DATA CRIAÇÃO
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "ERZET");//HORA
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "VBELN"); //ORDEM

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
            tblOptions.Clear();
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "BSTNK EQ '" + n_pedido.ToString() + "'");           

            /*Executa a RFC*/
            rfc_read_table.Invoke(dest);

            string[] fieldValues;

            IRfcTable returnTable = rfc_read_table.GetTable("DATA");

            if (returnTable.Count > 0)
            {
                fieldValues = returnTable.GetValue("WA").ToString().Split(';');  

                    Row["STATUS"]   = "S";
                    Row["MESSAGE"]  = "Pedido Localizado";
                    Row["DATA"]     = fieldValues[0];
                    Row["HORA"]     = fieldValues[1];
                    Row["ORDEM"]    = fieldValues[2];
            }
            else
            {
                Row["STATUS"] = "E";
                Row["MESSAGE"] = "Pedido não localizado";
            }
                      

            table.Rows.Add(Row);
            return table;
        }


        public DataTable logProcessamentoPedidosSFCokeSap(string n_pedido)
        {
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("N_LOG");
            table.Columns.Add("OBJ");
            table.Columns.Add("SUBOBJ");
            table.Columns.Add("DATA");
            table.Columns.Add("HORA");
            table.Columns.Add("USUARIO");
            table.Columns.Add("PROGRAMA");
            table.Columns.Add("TRANSACAO");
            table.Columns.Add("TOT_MSG");
            table.Columns.Add("SEQ_MSG");
            table.Columns.Add("TIP_MSG");
            table.Columns.Add("MENSG1");
            table.Columns.Add("MENSG2");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF_LOGPED");

                funcBAPI.SetValue("I_NPEDIDO", n_pedido);

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_DATA");

                if (objEstrutura.Count > 0)
                {

                    foreach (IRfcStructure detail in objEstrutura)
                    {
                        row = table.NewRow();

                        row["STATUS"] = funcBAPI.GetString("E_RESULT");
                        row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE");
                        row["N_LOG"] = detail.GetString("N_LOG");
                        row["OBJ"] = detail.GetString("OBJ");
                        row["SUBOBJ"] = detail.GetString("SUBOBJ");
                        row["DATA"] = detail.GetString("DATA");
                        row["HORA"] = detail.GetString("HORA");
                        row["USUARIO"] = detail.GetString("USUARIO");
                        row["PROGRAMA"] = detail.GetString("PROGRAMA");
                        row["TRANSACAO"] = detail.GetString("TRANSACAO");
                        row["TOT_MSG"] = detail.GetString("TOT_MSG");
                        row["SEQ_MSG"] = detail.GetString("SEQ_MSG");
                        row["TIP_MSG"] = detail.GetString("TIP_MSG");
                        row["MENSG1"] = detail.GetString("MENSG1");
                        row["MENSG2"] = detail.GetString("MENSG2");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row = table.NewRow();

                    row["STATUS"] = funcBAPI.GetString("E_RESULT");
                    row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }


        public DataTable entregasCliente(string CODCLI)
        {
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("CODCLI");
            table.Columns.Add("FANTASIA");
            table.Columns.Add("DATA_PEDIDO");
            table.Columns.Add("DATA_ENTREGA");
            table.Columns.Add("COD_MOT");
            table.Columns.Add("MOTORISTA");
            table.Columns.Add("STS_VEICULO");
            table.Columns.Add("N_TRANSPORTE");
            table.Columns.Add("TELEFONE");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("ZDSDF117");

                funcBAPI.SetValue("I_CLIENTE", CODCLI.ToString().PadLeft(10, '0'));

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_RESULTADO");

                foreach (IRfcStructure detail in objEstrutura)
                {
                    row = table.NewRow();

                    row["STATUS"] = "S";
                    row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE");
                    row["CODCLI"] = detail.GetString("KUNAG");
                    row["FANTASIA"] = detail.GetString("NAME2");
                    row["DATA_PEDIDO"] = detail.GetString("ERDAT");
                    row["DATA_ENTREGA"] = detail.GetString("DATBG");
                    row["COD_MOT"] = detail.GetString("/BEV1/RPFAR1");
                    row["MOTORISTA"] = detail.GetString("NAME1");
                    row["STS_VEICULO"] = detail.GetString("ADD01");
                    row["N_TRANSPORTE"] = detail.GetString("TKNUM");
                    row["TELEFONE"] = detail.GetString("TELF1");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }



        //aui

   /*     public DataTable MM_MONITOR_ALMOX(string centro, string data)
        {
            DataTable table = new DataTable("RESERVA");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("ABERTAS");
            table.Columns.Add("ATENDIDAS");
            table.Columns.Add("ELIMINIADAS");
            table.Columns.Add("TOTAL");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_MM_MONITOR_ALMOX");

                //Parâmetros 
                funcBAPI.SetValue("I_DATA", data);
                funcBAPI.SetValue("I_WERKS", centro);
                funcBAPI.Invoke(dest);

                string r_abertas        = new string(funcBAPI.GetCharArray("RS_APV_ABERTAS"));
                string r_atendidas      = new string(funcBAPI.GetCharArray("RS_ATENDIDAS"));
                string r_eliminadas     = new string(funcBAPI.GetCharArray("RS_ELIMINADAS"));
                string r_total          = new string(funcBAPI.GetCharArray("RS_TOTAL"));

                


                row["STATUS"] = "S";
                row["MESSAGE"] = "Registro encontrado.";

                row["ABERTAS"] = r_abertas;
                row["ATENDIDAS"] = r_atendidas;
                row["ELIMINIADAS"] = r_eliminadas;
                row["TOTAL"] = r_total;

                table.Rows.Add(row);


            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }


            return table;
        }*/


        public DataTable MM_MONITOR_ALMOX(string centro, string data, string tabela)
        {
            DataTable result;

	        DataTable table = new DataTable("TG_RETORNO");
		        table.Columns.Add("STATUS");
		        table.Columns.Add("MESSAGE");
		        table.Columns.Add("RSNUM");
		        table.Columns.Add("BDTER");
		        table.Columns.Add("KOSTL");
		        table.Columns.Add("KZEAR");
                table.Columns.Add("USNAM");
                table.Columns.Add("WEMPF");
                table.Columns.Add("MCTXT");
	        DataRow row = table.NewRow();

	         DataTable table2 = new DataTable("TG_TOTAL");
		        table2.Columns.Add("STATUS");
		        table2.Columns.Add("MESSAGE");
		        table2.Columns.Add("BDTER");
		        table2.Columns.Add("TOTAB");
		        table2.Columns.Add("TOTAT");
	        DataRow row2 = table2.NewRow();	

	        try
	        {
		        IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_MM_MONITOR_ALMOX");

		        /*Parâmetros 1*/
		        funcBAPI.SetValue("I_DATA", data);
		        funcBAPI.SetValue("I_WERKS", centro);
		        funcBAPI.Invoke(dest);
		
		        if(tabela == "1")
		        {
			        /* Tabela 1 */
			        IRfcTable objEstrutura = funcBAPI.GetTable("TG_RETORNO");
                    if (objEstrutura.RowCount > 0)
                    {
                        foreach (IRfcStructure detail in objEstrutura)
                        {
                            row = table.NewRow();

                            row["STATUS"] = "S";
                            row["MESSAGE"] = "Dados encontrados";
                            row["RSNUM"] = detail.GetString("RSNUM");
                            row["BDTER"] = detail.GetString("BDTER");
                            row["KOSTL"] = detail.GetString("KOSTL");
                            row["KZEAR"] = detail.GetString("KZEAR");
                            row["USNAM"] = detail.GetString("USNAM");
                            row["WEMPF"] = detail.GetString("WEMPF");
                            row["MCTXT"] = detail.GetString("MCTXT");

                            table.Rows.Add(row);
                        }
                    }
                    else
                    {
                        row["STATUS"] = "E";
                        row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE");

                        table.Rows.Add(row);
                    }
		        }
		        else if(tabela == "2")
		        {
			        /* Tabela 2 */
			        IRfcTable objEstrutura2 = funcBAPI.GetTable("TG_TOTAL");
                    if (objEstrutura2.RowCount > 0)
                    {
                        foreach (IRfcStructure detail in objEstrutura2)
                        {
                            row2 = table2.NewRow();

                            row2["STATUS"] = "S";
                            row2["MESSAGE"] = "Dados encontrados";
                            row2["BDTER"] = detail.GetString("BDTER");
                            row2["TOTAB"] = detail.GetString("TOTAB");
                            row2["TOTAT"] = detail.GetString("TOTAT");

                            table2.Rows.Add(row2);
                        }
                    }
                    else
                    {
                        row["STATUS"] = "E";
                        row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE"); 

                        table.Rows.Add(row);
                    }
		        }
		        else
		        {
			        row["STATUS"] = "E";
			        row["MESSAGE"] = "Opção informada de Tabela é Inválida!";

			        table.Rows.Add(row);
		        }

	        }
	        catch (Exception e)
	        {
                if (tabela == "1")
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = e.Message;

                    table.Rows.Add(row);
                }
                else if (tabela == "2")
                {
                    row2["STATUS"] = "E";
                    row2["MESSAGE"] = e.Message;

                    table2.Rows.Add(row2);
                }
                else
                {
                    row["STATUS"] = "E";
                    row["MESSAGE"] = "Opção informada de Tabela é Inválida!";

                    table.Rows.Add(row);
                }
	        }

            if (tabela == "1" || tabela != "2")
            {
                result = table;
            }
            else
            {
                result = table2;
            }
            

            return result;
	
        }


        public DataTable TelgrmPedidos(string dt_pedido, string cod_vend, string cod_cli)
        {
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("VBELN");
            table.Columns.Add("KUNNR");
            table.Columns.Add("NAME1");
            table.Columns.Add("NAME2");
            table.Columns.Add("AUART");
            table.Columns.Add("NETWR");
            table.Columns.Add("ERDAT");
            table.Columns.Add("ERZET");
            table.Columns.Add("BSARK");
            table.Columns.Add("ABSTK");
            table.Columns.Add("ZZCOOR");
            table.Columns.Add("ZZVEND");
            table.Columns.Add("TKNUM");
            table.Columns.Add("STTRG");
            table.Columns.Add("FORNECIMENTO");


            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_TELGRM_PEDIDOS");

                funcBAPI.SetValue("I_DATA", dt_pedido);
                funcBAPI.SetValue("I_VENDEDOR", cod_vend);
                if(!cod_cli.Equals(""))
                {
                    funcBAPI.SetValue("I_CLIENTE", cod_cli.ToString().PadLeft(10, '0'));
                }
                
                

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_PEDIDOS");

                if (objEstrutura.Count > 0)
                {

                    foreach (IRfcStructure detail in objEstrutura)
                    {
                        row = table.NewRow();

                        row["STATUS"]       = "S";
                        row["MESSAGE"]      = funcBAPI.GetString("E_MESSAGE");
                        row["VBELN"]        = detail.GetString("VBELN");
                        row["KUNNR"]        = detail.GetString("KUNNR");
                        row["NAME1"]        = detail.GetString("NAME1");
                        row["NAME2"]        = detail.GetString("NAME2");
                        row["AUART"]        = detail.GetString("AUART");
                        row["NETWR"]        = detail.GetString("NETWR");
                        row["ERDAT"]        = detail.GetString("ERDAT");
                        row["ERZET"]        = detail.GetString("ERZET");
                        row["BSARK"]        = detail.GetString("BSARK");
                        row["ABSTK"]        = detail.GetString("ABSTK");
                        row["ZZCOOR"]       = detail.GetString("ZZCOOR");
                        row["ZZVEND"]       = detail.GetString("ZZVEND");
                        row["TKNUM"]        = detail.GetString("TKNUM");
                        row["STTRG"]        = detail.GetString("STTRG");
                        row["FORNECIMENTO"] = detail.GetString("FORNECIMENTO");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row = table.NewRow();

                    row["STATUS"]   = "E";
                    row["MESSAGE"]  = funcBAPI.GetString("E_MESSAGE");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"]   = "E";
                row["MESSAGE"]  = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }


        public DataTable TelgrmItensPedidos(string dt_pedido, string num_pedido)
        {
            DataTable table = new DataTable("RETURN");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("VBELN");
            table.Columns.Add("POSNR");
            table.Columns.Add("MATNR");
            table.Columns.Add("ARKTX");
            table.Columns.Add("KWMENG");
            table.Columns.Add("MEINS");
            table.Columns.Add("NETWR");
            table.Columns.Add("ABGRU");
            table.Columns.Add("BEZEI");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_TELGRM_PEDIDOS");

                funcBAPI.SetValue("I_DATA", dt_pedido);
                funcBAPI.SetValue("I_NUMPED", num_pedido.ToString().PadLeft(10, '0'));
                funcBAPI.SetValue("I_VENDEDOR", "0000");
                funcBAPI.SetValue("I_CLIENTE", " ");

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_ITENSPED");

                if (objEstrutura.Count > 0)
                {

                    foreach (IRfcStructure detail in objEstrutura)
                    {
                        row = table.NewRow();

                        row["STATUS"]   = "S";
                        row["MESSAGE"]  = funcBAPI.GetString("E_MESSAGE_ITENS");
                        row["VBELN"]    = detail.GetString("VBELN");
                        row["POSNR"]    = detail.GetString("POSNR");
                        row["MATNR"]    = detail.GetString("MATNR");
                        row["ARKTX"]    = detail.GetString("ARKTX");
                        row["KWMENG"]   = detail.GetString("KWMENG");
                        row["MEINS"]    = detail.GetString("MEINS");
                        row["NETWR"]    = detail.GetString("NETWR");
                        row["ABGRU"]    = detail.GetString("ABGRU");
                        row["BEZEI"]    = detail.GetString("BEZEI");

                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row = table.NewRow();

                    row["STATUS"] = "E";
                    row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE_ITENS");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }




        public DataTable StatusTransporte(string data, string org_transp)
        {
            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("DESPACHADO");
            table.Columns.Add("CONFERIDO");
            table.Columns.Add("ESTACIONADO");
            table.Columns.Add("PENDENTE");
            table.Columns.Add("FINALIZADO");
            table.Columns.Add("OUTROS");
            table.Columns.Add("PENDENTE_GERAL");

            table.Columns.Add("TKNUM");
            table.Columns.Add("DTDIS");
            table.Columns.Add("UZDIS");
            table.Columns.Add("DAREG");
            table.Columns.Add("UAREG");
            table.Columns.Add("DALBG");
            table.Columns.Add("UALBG");
            table.Columns.Add("DALEN");
            table.Columns.Add("UALEN");
            table.Columns.Add("DTABF");
            table.Columns.Add("UZABF");
            table.Columns.Add("DATBG");
            table.Columns.Add("UATBG");
            table.Columns.Add("DATEN");
            table.Columns.Add("UATEN");
            table.Columns.Add("DPTBG");
            table.Columns.Add("FROTA");
            table.Columns.Add("MOTORISTA");
            table.Columns.Add("TPLST");
            table.Columns.Add("STTRG");
            table.Columns.Add("ADD01");
            table.Columns.Add("ADD02");
            table.Columns.Add("ADD03");
            table.Columns.Add("TEXT1");
            table.Columns.Add("TEXT2");
            table.Columns.Add("TEXT3");
            table.Columns.Add("STATUS_TRA");

            table.Columns.Add("N_MOTORISTA");
            table.Columns.Add("N_AJUDANTE1");
            table.Columns.Add("N_AJUDANTE2");
            table.Columns.Add("N_AJUDANTE3");
            table.Columns.Add("EXTI1");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_TRANSP_STATUS");

                funcBAPI.SetValue("I_DATA", data);
                funcBAPI.SetValue("I_ORGTRA", org_transp);

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_TRANSPORTES");

                if (objEstrutura.Count > 0)
                {

                    foreach (IRfcStructure detail in objEstrutura)
                    {
                        row = table.NewRow();                     

                        //Mensagem
                        row["STATUS"]   = "S";
                        row["MESSAGE"]  = funcBAPI.GetString("E_MESSAGE");

                        //Totalizadores
                        row["DESPACHADO"] = funcBAPI.GetString("E_DESPACHADO");
                        row["CONFERIDO"] = funcBAPI.GetString("E_CONFERIDO");
                        row["ESTACIONADO"] = funcBAPI.GetString("E_ESTACIONADO");
                        row["PENDENTE"] = funcBAPI.GetString("E_PENDENTE");
                        row["FINALIZADO"] = funcBAPI.GetString("E_FINALIZADO");
                        row["OUTROS"] = funcBAPI.GetString("E_OUTROS");
                        row["PENDENTE_GERAL"] = funcBAPI.GetString("E_PENDENTE_GERAL");

                        //Lista do Transporte
                        row["TKNUM"] = detail.GetString("TKNUM");
                        row["DTDIS"] = detail.GetString("DTDIS");
                        row["UZDIS"] = detail.GetString("UZDIS");
                        row["DAREG"] = detail.GetString("DAREG");
                        row["UAREG"] = detail.GetString("UAREG");
                        row["DALBG"] = detail.GetString("DALBG");
                        row["UALBG"] = detail.GetString("UALBG");
                        row["DALEN"] = detail.GetString("DALEN");
                        row["UALEN"] = detail.GetString("UALEN");
                        row["DTABF"] = detail.GetString("DTABF");
                        row["UZABF"] = detail.GetString("UZABF");
                        row["DATBG"] = detail.GetString("DATBG");
                        row["UATBG"] = detail.GetString("UATBG");
                        row["DATEN"] = detail.GetString("DATEN");
                        row["UATEN"] = detail.GetString("UATEN");
                        row["DPTBG"] = detail.GetString("DPTBG");
                        row["FROTA"] = detail.GetString("FROTA");
                        row["MOTORISTA"] = detail.GetString("MOTORISTA");
                        row["TPLST"] = detail.GetString("TPLST");
                        row["STTRG"] = detail.GetString("STTRG");
                        row["ADD01"] = detail.GetString("ADD01");
                        row["ADD02"] = detail.GetString("ADD02");
                        row["ADD03"] = detail.GetString("ADD03");
                        row["TEXT1"] = detail.GetString("TEXT1");
                        row["TEXT2"] = detail.GetString("TEXT2");
                        row["TEXT3"] = detail.GetString("TEXT3");
                        row["STATUS_TRA"] = detail.GetString("STATUS");
                        row["N_MOTORISTA"] = detail.GetString("N_MOTORISTA");
                        row["N_AJUDANTE1"] = detail.GetString("N_AJUDANTE1");
                        row["N_AJUDANTE2"] = detail.GetString("N_AJUDANTE2");
                        row["N_AJUDANTE3"] = detail.GetString("N_AJUDANTE3");
                        row["EXTI1"]        = detail.GetString("EXTI1");
                       


                        table.Rows.Add(row);
                    }
                }
                else
                {
                    row = table.NewRow();

                    //Menssagem
                    row["STATUS"] = "E";
                    row["MESSAGE"] = funcBAPI.GetString("E_MESSAGE_ITENS");

                    //Totalizadores
                    row["DESPACHADO"] = funcBAPI.GetString("E_DESPACHADO");
                    row["CONFERIDO"] = funcBAPI.GetString("E_CONFERIDO");
                    row["ESTACIONADO"] = funcBAPI.GetString("E_ESTACIONADO");
                    row["PENDENTE"] = funcBAPI.GetString("E_PENDENTE");
                    row["FINALIZADO"] = funcBAPI.GetString("E_FINALIZADO");
                    row["OUTROS"] = funcBAPI.GetString("E_OUTROS");
                    row["PENDENTE_GERAL"] = funcBAPI.GetString("E_PENDENTE_GERAL");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);
            }

            return table;
        }



        public DataTable VALES_LANCA_FBCJ(string DATA_DOC, string EMITENTE, string NUM_VALE, string TRANS_CONTAB, string VALOR, string FORNEC_NF, string CCUSTO, string CENTRO, string ORIGEM, string EMPRESA)
        {
            /*
             * ESTE MÉTODO GRAVA UMA ENTRADA NO LIVRO DE CAIXA SAP, CUJA TRANSAÇÃO FBCJ           
             */
            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS");
            table.Columns.Add("ID");
            table.Columns.Add("NUM_MESSAGE");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("NUMDOC");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("ZFI_RFC_LANCA_CAIXA_DINHEIRO");

                funcBAPI.SetValue("I_DATA_DOC", DATA_DOC);//Requerido
                funcBAPI.SetValue("I_EMITENTE", EMITENTE);//Requerido
                funcBAPI.SetValue("I_NUM_VALE", NUM_VALE);//Requerido
                funcBAPI.SetValue("I_TRANS_CONTAB", TRANS_CONTAB);//Requerido
                funcBAPI.SetValue("I_VALOR", VALOR);//Requerido
                funcBAPI.SetValue("I_FORNEC_NF", FORNEC_NF);//Requerido
                funcBAPI.SetValue("I_CCUSTO", CCUSTO.ToString().PadLeft(10, '0'));//Requerido  
                funcBAPI.SetValue("I_EMPRESA", EMPRESA);//Requerido
                funcBAPI.SetValue("I_CENTRO", CENTRO);//Requerido               
                funcBAPI.SetValue("I_ORIGEM", ORIGEM);//Requerido 

                funcBAPI.Invoke(dest);
                
                IRfcTable objEstrutura = funcBAPI.GetTable("T_RETURN");

                foreach (IRfcStructure detail in objEstrutura)
                {
                    row = table.NewRow();

                    row["STATUS"]       = detail.GetString("TYPE");
                    row["ID"]           = detail.GetString("ID");
                    row["NUM_MESSAGE"]  = detail.GetString("NUMBER");
                    row["MESSAGE"]      = detail.GetString("MESSAGE");
                    row["NUMDOC"]       = funcBAPI.GetString("E_NUMDOC");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {                
                    row = table.NewRow();

                    row["STATUS"]       = "E";
                    row["ID"]           = "0";
                    row["NUM_MESSAGE"]  = "0";
                    row["MESSAGE"]      = e.Message;
                    row["NUMDOC"]       = "0";

                    table.Rows.Add(row);
            }
            

            return table;

        }



        public DataTable VALES_LANCA_F43(string DATA_DOC, string MATRICULA, string NUM_VALE, string TRANS_CONTAB, string FORNEC_NF, string ORIGEM, string CENTRO, string CCUSTO, string VALOR, string DESC_VALE, string EMPRESA)
        {
            /*
             * ESTE MÉTODO GRAVA UMA ENTRADA NO LIVRO DE CAIXA SAP, CUJA TRANSAÇÃO FBCJ           
             */
            DataTable table = new DataTable("RETURN");

            table.Columns.Add("STATUS");
            table.Columns.Add("ID");
            table.Columns.Add("NUM_MESSAGE");
            table.Columns.Add("MESSAGE");
            table.Columns.Add("NUMDOC");

            DataRow row = table.NewRow();

            try
            {
                IRfcFunction funcBAPI = repo.CreateFunction("ZFI_RFC_LANCA_DEPOSITO");

                funcBAPI.SetValue("I_DATA_DOC", DATA_DOC);//Requerido
                funcBAPI.SetValue("I_MATRICULA", MATRICULA);//Requerido
                funcBAPI.SetValue("I_NUM_VALE", NUM_VALE);//Requerido
                funcBAPI.SetValue("I_TRANS_CONTAB", TRANS_CONTAB);//Requerido
                funcBAPI.SetValue("I_FORNEC_NF", FORNEC_NF);//Requerido
                funcBAPI.SetValue("I_ORIGEM", ORIGEM);//Requerido
                funcBAPI.SetValue("I_EMPRESA", EMPRESA);//Requerido
                funcBAPI.SetValue("I_CENTRO", CENTRO);//Requerido 
                funcBAPI.SetValue("I_CCUSTO", CCUSTO);//Requerido
                //funcBAPI.SetValue("I_CCUSTO", CCUSTO.ToString().PadLeft(10, '0'));//Requerido                    
                funcBAPI.SetValue("I_VALOR", VALOR);//Requerido
                funcBAPI.SetValue("I_DESC_VALE", DESC_VALE);//

                funcBAPI.Invoke(dest);

                IRfcTable objEstrutura = funcBAPI.GetTable("T_RETURN");

                foreach (IRfcStructure detail in objEstrutura)
                {
                    row = table.NewRow();

                    row["STATUS"] = detail.GetString("TYPE");
                    row["ID"] = detail.GetString("ID");
                    row["NUM_MESSAGE"] = detail.GetString("NUMBER");
                    row["MESSAGE"] = detail.GetString("MESSAGE");
                    row["NUMDOC"] = funcBAPI.GetString("E_NUMDOC");

                    table.Rows.Add(row);
                }

            }
            catch (Exception e)
            {
                row = table.NewRow();

                row["STATUS"] = "E";
                row["ID"] = "0";
                row["NUM_MESSAGE"] = "0";
                row["MESSAGE"] = e.Message;
                row["NUMDOC"] = "0";

                table.Rows.Add(row);
            }


            return table;

        }


        public DataTable consultaSaldoEstoqueSAP(string produto, string centro, string deposito)
        {
            /*Trata o Retorno, jogando numa tabela*/
            DataTable table = new DataTable("SALDO");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("CODPROD", typeof(string));
            table.Columns.Add("CENTRO", typeof(string));
            table.Columns.Add("DEPOSITO", typeof(string));          
            table.Columns.Add("LIVRE", typeof(string));
            table.Columns.Add("TRANSITO", typeof(string));
            table.Columns.Add("CONTROLE_QUALIDADE", typeof(string));
            table.Columns.Add("RESTRITO", typeof(string));
            table.Columns.Add("BLOQUEADO", typeof(string));
            table.Columns.Add("DEVOLUCOES", typeof(string));

            try
            {

                IRfcFunction rfc_read_table = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC

                rfc_read_table.SetValue("QUERY_TABLE", "MARD"); //Nome da Tabela

                rfc_read_table.SetValue("DELIMITER", ";");// Delimitãção do retorno ';'

                /*Passa os Campos que retornarão*/
                var fieldsTable = rfc_read_table.GetTable("FIELDS");
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "MATNR");         //Material
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "WERKS");         //Centro
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "LGORT");         //Deposito
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "LABST");         //Livre
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "INSME");         //Contr. Qualidade
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "EINME");         //Restrito
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "SPEME");         //Bloqueado
                fieldsTable.Append();
                fieldsTable.SetValue("FIELDNAME", "RETME");         //Devoluções*/                      


                /*Faz a seleção do Registro*/
                IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "MATNR EQ '" + produto.ToString().PadLeft(18, '0') + "' AND WERKS EQ '" + centro + "' AND ");
                tblOptions.Append();
                tblOptions.SetValue("TEXT", "LGORT EQ '" + deposito + "'");

                
                /*Executa a RFC*/
                rfc_read_table.Invoke(dest);
                


                DataRow Row = table.NewRow();

                IRfcTable returnTable = rfc_read_table.GetTable("DATA");

                string[] fieldValues;               

                foreach (IRfcStructure row in returnTable)
                {
                    fieldValues = row.GetValue("WA").ToString().Split(';');

                    Row["STATUS"] = "S";
                    Row["MESSAGE"] = "Registro encontrado com sucesso!"; 

                    Row["CODPROD"]            = fieldValues[0];
                    Row["CENTRO"]             = fieldValues[1];
                    Row["DEPOSITO"]           = fieldValues[2];                  
                    Row["LIVRE"]              = fieldValues[3];                    
                    Row["CONTROLE_QUALIDADE"] = fieldValues[4];
                    Row["RESTRITO"]           = fieldValues[5];
                    Row["BLOQUEADO"]          = fieldValues[6];
                    Row["DEVOLUCOES"]         = fieldValues[7];                  
                }

                table.Rows.Add(Row);
            }
            catch (RfcAbapException e)
            {
                DataRow Row = table.NewRow();

                Row["STATUS"] = "E";
                Row["MESSAGE"] = e.Message + ' ' + e.Key ;

                table.Rows.Add(Row);

                string error = e.Key;
                string error_number = e.AbapMessageNumber.ToString();
                throw e;
               
            }

            return table;

        }

        public XmlDocument statusPedidosCanalClientes(string codigo_cli, string dias)
        {
            int cont_it        = 0;
            int cont_it_nota   = 0;
            XmlDocument result = null;

            try
            {
                XmlDocument xDoc = new XmlDocument();

                //Criando cabeçalho e node geral
                XmlNode xNode = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xDoc.AppendChild(xNode);

                xNode = xDoc.CreateElement("Pedidos");
                xDoc.AppendChild(xNode);


                /*Chama a RFC*/
                IRfcFunction funcBAPI = repo.CreateFunction("ZDSD_RFC_STATUS_PEDIDOS_CLIENT");

                /*Parâmetros 1 - Dados de Venda*/
                funcBAPI.SetValue("CODCLI", codigo_cli.ToString().PadLeft(10, '0'));
                funcBAPI.SetValue("DIAS_PEDIDO", dias);               

                funcBAPI.Invoke(dest);

                /*fim chama RFC*/

                XmlNode xNodeCol_resposta_status = xDoc.CreateElement("Resposta", "Status", null);

                xNodeCol_resposta_status.InnerText = funcBAPI.GetString("E_STATUS");
                xNode.AppendChild(xNodeCol_resposta_status);

                XmlNode xNodeCol_resposta_message = xDoc.CreateElement("Resposta", "Message", null);
                xNodeCol_resposta_message.InnerText = funcBAPI.GetString("E_MESSAGE"); ;
                xNode.AppendChild(xNodeCol_resposta_message);               

                IRfcTable objEstrutura1 = funcBAPI.GetTable("T_STATUS_PEDIDO");
                
                foreach (IRfcStructure detail1 in objEstrutura1)
                {
                    cont_it      = 0; //Contador de itens do pedido
                    cont_it_nota = 0; //Contador de notas do pedido

                    //ABRE REGISTO
                    XmlElement xNodeRegistro = xDoc.CreateElement("Pedido");
                    xNodeRegistro.SetAttribute("ORDEM", detail1.GetString("VBELN"));

                    //STATUS DO PEDIDO
                    XmlElement xNode_status = xDoc.CreateElement("Status");
                    xNodeRegistro.AppendChild(xNode_status);

                    //Campos do Grupo de Status do Pedido
                    XmlElement xNode_st_ped = xDoc.CreateElement("VBELN"); //N Pedido
                    xNode_st_ped.InnerText = detail1.GetString("VBELN");
                    xNode_status.AppendChild(xNode_st_ped);

                    XmlElement xNode_st_dt = xDoc.CreateElement("ERDAT"); //Data Pedido
                    xNode_st_dt.InnerText = detail1.GetString("ERDAT");
                    xNode_status.AppendChild(xNode_st_dt);

                    XmlElement xNode_st_tp = xDoc.CreateElement("AUART"); //Tipo Pedido
                    xNode_st_tp.InnerText = detail1.GetString("AUART");
                    xNode_status.AppendChild(xNode_st_tp);

                    XmlElement xNode_st_cli = xDoc.CreateElement("KUNNR"); //Cliente Pedido
                    xNode_st_cli.InnerText = detail1.GetString("KUNNR");
                    xNode_status.AppendChild(xNode_st_cli);

                    XmlElement xNode_st_just = xDoc.CreateElement("AUGRU"); //justif Pedido
                    xNode_st_just.InnerText = detail1.GetString("AUGRU"); 
                    xNode_status.AppendChild(xNode_st_just);

                    XmlElement xNode_st_ori = xDoc.CreateElement("BSARK"); //Origem Pedido
                    xNode_st_ori.InnerText = detail1.GetString("BSARK");
                    xNode_status.AppendChild(xNode_st_ori);

                    XmlElement xNode_st_nped = xDoc.CreateElement("BSTNK"); //Numero Pedido 
                    xNode_st_nped.InnerText = detail1.GetString("BSTNK");
                    xNode_status.AppendChild(xNode_st_nped);

                    XmlElement xNode_st_sts = xDoc.CreateElement("STATUS"); //Status Pedido 
                    xNode_st_sts.InnerText = detail1.GetString("STATUS");
                    xNode_status.AppendChild(xNode_st_sts);

                    XmlElement xNode_st_vcod = xDoc.CreateElement("VENDEDOR_COD"); //Status Pedido Vendedor Cod
                    xNode_st_vcod.InnerText = detail1.GetString("VENDEDOR_COD");
                    xNode_status.AppendChild(xNode_st_vcod);

                    XmlElement xNode_st_vnom = xDoc.CreateElement("VENDEDOR_NOME"); //Status Pedido Vendedor Nome
                    xNode_st_vnom.InnerText = detail1.GetString("VENDEDOR_NOME");
                    xNode_status.AppendChild(xNode_st_vnom);

                    XmlElement xNode_st_vtel = xDoc.CreateElement("VENDEDOR_TELF"); //Status Pedido Vendedor Nome
                    xNode_st_vtel.InnerText = detail1.GetString("VENDEDOR_TELF");
                    xNode_status.AppendChild(xNode_st_vtel);

                    XmlElement xNode_st_coordco = xDoc.CreateElement("COORDENADOR_COD"); //Status Pedido Coordenador cod
                    xNode_st_coordco.InnerText = detail1.GetString("COORDENADOR_COD");
                    xNode_status.AppendChild(xNode_st_coordco);

                    XmlElement xNode_st_coordnm = xDoc.CreateElement("COORDENADOR_NOME"); //Status Pedido Coordenador cod
                    xNode_st_coordnm.InnerText = detail1.GetString("COORDENADOR_NOME");
                    xNode_status.AppendChild(xNode_st_coordnm);

                    XmlElement xNode_st_coordtel = xDoc.CreateElement("COORDENADOR_TELF"); //Status Pedido Coordenador cod
                    xNode_st_coordtel.InnerText = detail1.GetString("COORDENADOR_TELF");
                    xNode_status.AppendChild(xNode_st_coordtel);

                    //MONTA OS ITENS DO PEDIDO                    
                    XmlElement xNode_itens = xDoc.CreateElement("Itens");
                    xNodeRegistro.AppendChild(xNode_itens);         

                    IRfcTable objEstrutura2 = funcBAPI.GetTable("T_ITENS_PEDIDO");
                    foreach (IRfcStructure detail2 in objEstrutura2)
                    {
                        if (detail2.GetString("VBELN").Equals(detail1.GetString("VBELN")))
                        {
                            cont_it++;
                            //SubItens do Pedido
                            XmlElement xNode_item = xDoc.CreateElement("Item");
                            xNode_item.SetAttribute("ID", cont_it.ToString());
                            xNode_itens.AppendChild(xNode_item);

                            XmlElement xNode_it_pos = xDoc.CreateElement("POSNR"); //Item do pedido
                            xNode_it_pos.InnerText = detail2.GetString("POSNR");
                            xNode_item.AppendChild(xNode_it_pos);

                            XmlElement xNode_prod_mat = xDoc.CreateElement("MATNR"); //Produto codigo
                            xNode_prod_mat.InnerText = detail2.GetString("MATNR");
                            xNode_item.AppendChild(xNode_prod_mat);

                            XmlElement xNode_prod_ark = xDoc.CreateElement("ARKTX"); //Produto Descrição
                            xNode_prod_ark.InnerText = detail2.GetString("ARKTX");
                            xNode_item.AppendChild(xNode_prod_ark);

                            XmlElement xNode_prod_mein = xDoc.CreateElement("MEINS"); //Produto medida
                            xNode_prod_mein.InnerText = detail2.GetString("MEINS");
                            xNode_item.AppendChild(xNode_prod_mein);

                            XmlElement xNode_prod_kwm = xDoc.CreateElement("KWMENG"); //Produto qtd
                            xNode_prod_kwm.InnerText = detail2.GetString("KWMENG");
                            xNode_item.AppendChild(xNode_prod_kwm);

                            XmlElement xNode_prod_kon = xDoc.CreateElement("KONDM");
                            xNode_prod_kon.InnerText = detail2.GetString("KONDM");
                            xNode_item.AppendChild(xNode_prod_kon);

                            XmlElement xNode_prod_abu = xDoc.CreateElement("ABGRU"); //Produto cod recusa
                            xNode_prod_abu.InnerText = detail2.GetString("ABGRU");
                            xNode_item.AppendChild(xNode_prod_abu);

                            XmlElement xNode_prod_bez = xDoc.CreateElement("BEZEI"); //Produto recusa
                            xNode_prod_bez.InnerText = detail2.GetString("BEZEI");
                            xNode_item.AppendChild(xNode_prod_bez);

                            XmlElement xNode_prod_cm = xDoc.CreateElement("CMPRE"); //Produto Preço
                            xNode_prod_cm.InnerText = detail2.GetString("CMPRE");
                            xNode_item.AppendChild(xNode_prod_cm);

                            XmlElement xNode_prod_rem = xDoc.CreateElement("REMESSA"); //Produto Remessa
                            xNode_prod_rem.InnerText = detail2.GetString("REMESSA");
                            xNode_item.AppendChild(xNode_prod_rem);
                        }
                    }


                    //NOTAS DO PEDIDO
                    XmlElement xNode_notas = xDoc.CreateElement("Notas");
                    xNodeRegistro.AppendChild(xNode_notas);

                    IRfcTable objEstrutura3 = funcBAPI.GetTable("T_NOTAS_PEDIDO");
                    foreach (IRfcStructure detail3 in objEstrutura3)
                    {
                                              

                        if (detail3.GetString("VBELN").Equals(detail1.GetString("VBELN")))
                        {
                            cont_it_nota++;

                            XmlElement xNode_item_nota = xDoc.CreateElement("Nota");
                            xNode_item_nota.SetAttribute("ID", cont_it_nota.ToString());
                            xNode_notas.AppendChild(xNode_item_nota);

                            XmlElement xNode_nt_num = xDoc.CreateElement("NFENUM"); //Item do pedido
                            xNode_nt_num.InnerText = detail3.GetString("NFENUM") + "-" + detail3.GetString("SERIES");
                            xNode_item_nota.AppendChild(xNode_nt_num);

                            XmlElement xNode_nt_doc = xDoc.CreateElement("DOCSTAT"); //Status nota
                            xNode_nt_doc.InnerText = detail3.GetString("DOCSTAT");
                            xNode_item_nota.AppendChild(xNode_nt_doc);

                            XmlElement xNode_nt_canc = xDoc.CreateElement("CANCEL"); //Status canc nota
                            xNode_nt_canc.InnerText = detail3.GetString("CANCEL");
                            xNode_item_nota.AppendChild(xNode_nt_canc);

                            XmlElement xNode_nt_dt = xDoc.CreateElement("DOCDAT"); //data doc
                            xNode_nt_dt.InnerText = detail3.GetString("DOCDAT");
                            xNode_item_nota.AppendChild(xNode_nt_dt);

                            XmlElement xNode_nt_docn = xDoc.CreateElement("DOCNUM"); //doc
                            xNode_nt_docn.InnerText = detail3.GetString("DOCNUM");
                            xNode_item_nota.AppendChild(xNode_nt_docn);

                            
                        }
                    }



                    //TRANSPORTES
                    XmlElement xNode_tra = xDoc.CreateElement("Transporte");
                    xNodeRegistro.AppendChild(xNode_tra);

                    IRfcTable objEstrutura4 = funcBAPI.GetTable("T_TRANSPORTES");
                    foreach (IRfcStructure detail4 in objEstrutura4)
                    {
                        if (detail4.GetString("VBELN").Equals(detail1.GetString("VBELN")))
                        {
                            XmlElement xNode_tr_num = xDoc.CreateElement("TKNUM"); //Numero doc. tra
                            xNode_tr_num.InnerText = detail4.GetString("TKNUM");
                            xNode_tra.AppendChild(xNode_tr_num);

                            XmlElement xNode_tr_frt = xDoc.CreateElement("FROTA"); //Frota
                            xNode_tr_frt.InnerText = detail4.GetString("FROTA");
                            xNode_tra.AppendChild(xNode_tr_frt);

                            XmlElement xNode_tr_plc = xDoc.CreateElement("PLACA"); //Frota
                            xNode_tr_plc.InnerText = detail4.GetString("PLACA");
                            xNode_tra.AppendChild(xNode_tr_plc);

                            XmlElement xNode_tr_crg = xDoc.CreateElement("CARGA"); //Frota
                            xNode_tr_crg.InnerText = detail4.GetString("CARGA");
                            xNode_tra.AppendChild(xNode_tr_crg);

                            XmlElement xNode_tr_ad1 = xDoc.CreateElement("ADD01"); //status veiculo
                            xNode_tr_ad1.InnerText = detail4.GetString("ADD01");
                            xNode_tra.AppendChild(xNode_tr_ad1);

                            XmlElement xNode_tr_ad3 = xDoc.CreateElement("ADD03"); //status doc. transporte
                            xNode_tr_ad3.InnerText = detail4.GetString("ADD03");
                            xNode_tra.AppendChild(xNode_tr_ad3);

                            XmlElement xNode_tr_mot_c = xDoc.CreateElement("MOTORISTA_COD"); //Motorista COD. SAP
                            xNode_tr_mot_c.InnerText = detail4.GetString("MOTORISTA_MAT");
                            xNode_tra.AppendChild(xNode_tr_mot_c);

                            XmlElement xNode_tr_mot_n = xDoc.CreateElement("MOTORISTA_NOME"); //Motorista nome
                            xNode_tr_mot_n.InnerText = detail4.GetString("MOTORISTA_NOME");
                            xNode_tra.AppendChild(xNode_tr_mot_n);

                            XmlElement xNode_tr_mot_t = xDoc.CreateElement("MOTORISTA_TEL"); //Motorista nome
                            xNode_tr_mot_t.InnerText = detail4.GetString("MOTORISTA_TEL");
                            xNode_tra.AppendChild(xNode_tr_mot_t);
                        }
                    }

                    //Adiciona o registro
                    xNode.AppendChild(xNodeRegistro);

                }

                result = xDoc;

            }
            catch (Exception e)
            {
                throw e;
                //result =  e.Message;           
            }


            return result;
        }


        //Lockers
        public DataTable criarPedidosVendasLockers(string COD_CLIENTE, string CONDICAO_PAGMTO, string ORG_VENDAS, string CANAL_DIST, string SETOR_ATV, string CENTRO, string TIPO_DOC, string TBL_PRECO, string DEPOSITO, string NOME_CLIENTE, string CPF, string PRODUTOS, string ENDERECO)
        {
            //Log
            string param;
            Stream saida = File.Open(@"c:\temp\Log_lockers.txt", FileMode.Append);
            StreamWriter escritor = new StreamWriter(saida);

            escritor.WriteLine(DateTime.Now + ":");


            DataTable table = new DataTable("PEDIDO");
            table.Columns.Add("STATUS");
            table.Columns.Add("MESSAGE");

            table.Columns.Add("ORDEM");
            table.Columns.Add("FORNECIMENTO");
            table.Columns.Add("FATURA");

            DataRow row = table.NewRow();
            param = "COD_CLIENTE:" + COD_CLIENTE + " /CONDICAO_PAGMTO:" + CONDICAO_PAGMTO + " /ORG_VENDAS:" + ORG_VENDAS + " /CANAL_DIST:" + CANAL_DIST +
                  " /SETOR_ATV:" + SETOR_ATV + " /CENTRO:" + CENTRO + " /TIPO_DOC:" + TIPO_DOC + " /TABELA PREÇO:" + TBL_PRECO + " /DEPOSITO:"+ DEPOSITO + " /NOME_CLIENTE:" + NOME_CLIENTE +
                  " /CPF:" + CPF + " / ITENS[" + PRODUTOS + "]" + " /ENDERECO[" + ENDERECO + "]";

            try
            {
                /*Chama a RFC*/
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFCVENDALOCKERS");

                /*Parâmetros 1 - Dados de Venda*/
                //funcBAPI.SetValue("P_BUKRS", EMPRESA);             // Empresa que efetua o faturamento  - opcional
                funcBAPI.SetValue("P_KUNNR", COD_CLIENTE);         // Código do Cliente           - opcional
                funcBAPI.SetValue("P_ZLSCH", CONDICAO_PAGMTO);     // Condição de Pagamento       - Obrigatorio
                funcBAPI.SetValue("P_VKORG", ORG_VENDAS);          // Organização de Vendas       - Fixo 10000
                funcBAPI.SetValue("P_VTWEG", CANAL_DIST);          // Canal de Distribuição       - Fixo 01
                funcBAPI.SetValue("P_SPART", SETOR_ATV);           // Setor de atividade          - Fixo 01
                funcBAPI.SetValue("P_WERKS", CENTRO);              // Centro                      - Fixo 0008
                funcBAPI.SetValue("P_AUART", TIPO_DOC);            // Tipo de documento           - Fixo ZS44
                funcBAPI.SetValue("P_KVGR1", TBL_PRECO);           // Tabela de Preço             - opcional
                funcBAPI.SetValue("P_LGORT", DEPOSITO);            // Depósito                    - Fixo APA
                funcBAPI.SetValue("P_NAME1", NOME_CLIENTE);        // Nome do Cliente             - Opcional
                funcBAPI.SetValue("P_STCD1", CPF);                 // CPF                         - Obrigatório

                /*Parâmetros 2 - Produtos Vendidos*/
                IRfcTable objEstrutura = funcBAPI.GetTable("T_MATNR");

                string[] linhas = Regex.Split(PRODUTOS, "/");
                string[] Colunas;

                for (int i = 0; i < linhas.Length; i++)
                {
                    Colunas = Regex.Split(linhas[i], ":");

                    objEstrutura.Append();

                    objEstrutura.SetValue("MANDT", "500");                                      //Mandante
                    objEstrutura.SetValue("MATERIAL", Colunas[0].ToString().PadLeft(18, '0'));  //Material                
                    objEstrutura.SetValue("QUANTIDADE", Colunas[1]);                            //Quantidade                   
                }

                /*Parâmetros 3 - Endereço de Entrega*/
                IRfcTable objEstrutura2 = funcBAPI.GetTable("T_ADDR1");

                string[] linhas_end = Regex.Split(ENDERECO, "/"); //Linhas(registros) são separadas por / (barras)
                string[] Colunas_end;

                for (int i = 0; i < linhas_end.Length; i++)
                {
                    Colunas_end = Regex.Split(linhas_end[i], ":"); //Colunas são separadas por : (dois pontos)

                    objEstrutura2.Append();

                    objEstrutura2.SetValue("STREET", Colunas_end[0]);                               //Endereço                
                    objEstrutura2.SetValue("HOUSE_NUM1", Colunas_end[1]);                           //Número
                    objEstrutura2.SetValue("HOUSE_NUM2", Colunas_end[2]);                           //Tipo Residência
                    objEstrutura2.SetValue("CITY1", Colunas_end[3]);                                //Cidade
                    objEstrutura2.SetValue("CITY2", Colunas_end[4]);                                //Bairro
                    objEstrutura2.SetValue("POST_CODE1", Colunas_end[5]);                           //CEP
                    objEstrutura2.SetValue("REGION", Colunas_end[6]);
                    objEstrutura2.SetValue("TEL_NUMBER", Colunas_end[7]);                           //Telefone                    
                }


                funcBAPI.Invoke(dest);

                string fatura = funcBAPI.GetString("VBELN_VF");

                IRfcTable table_return_erro = funcBAPI.GetTable("IT_RETURN2"); //TAbela de erros

                //VErifica se retornou erro 
                if (!fatura.Equals(""))
                {//CAso Não teve erro
                    //Mensagem de sucesso e dados do pedido
                    row["STATUS"] = "S";
                    row["MESSAGE"] = "Pedido Criado com sucesso!";
                    row["ORDEM"] = funcBAPI.GetString("VBELN");
                    row["FORNECIMENTO"] = funcBAPI.GetString("VBELN_VL");
                    row["FATURA"] = funcBAPI.GetString("VBELN_VF");

                    //Log                    
                    escritor.WriteLine("Pedido Criado com sucesso: Ordem:" + row["ORDEM"] + "=>PARAMETROS:" + param);
                    escritor.Close();
                }
                else
                {//Caso tenha erros
                    //Mensgem de erro
                    row["STATUS"] = "E";
                    string monta = "";
                    foreach (IRfcStructure detail in table_return_erro)
                    {
                        monta += detail.GetString("MESSAGE") + " / ";
                    }

                    //Log                    
                    escritor.WriteLine("Erro: Ordem:" + monta + "=>PARAMETROS:" + param);
                    escritor.Close();


                    row["MESSAGE"] = monta;


                }

                table.Rows.Add(row);


            }
            catch (Exception e)
            {
                row["STATUS"] = "E";
                row["MESSAGE"] = e.Message;

                table.Rows.Add(row);

                //Log                    
                escritor.WriteLine("Erro: Ordem:" + row["MESSAGE"] + "=>PARAMETROS:" + param);
                escritor.Close();
            }


            return table;
        }

        ///


    }
}