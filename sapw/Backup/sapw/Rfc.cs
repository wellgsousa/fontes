using System;
using System.Collections.Generic;
using System.Web;
using SAP.Middleware.Connector;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;

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

        /*Exemplo de Envio para o SAP, usando estrutura*/
        public String RegistraAbastecimento(string Equipamento, string Posto, string Data, string Hora, string Trajeto_consumo, string Consumo, string Posicao_contador)
        {
            IRfcFunction objRfc = repo.CreateFunction("ZRFC_FROTAWEB_ABASTECE");
            IRfcStructure objEstrutura = objRfc.GetStructure("I_DADOS");

            objEstrutura.SetValue("equip", Equipamento);
            objEstrutura.SetValue("POSTO_ABAST", Posto);
            objEstrutura.SetValue("DATA", Data);
            objEstrutura.SetValue("HORA", Hora);
            objEstrutura.SetValue("TRAJETO", Trajeto_consumo);
            objEstrutura.SetValue("CONSUMO", Consumo);
            objEstrutura.SetValue("CONTADOR", Posicao_contador);

            objRfc.Invoke(dest);  
           
            return objRfc.GetValue(0).ToString();

           
           
        }

        public String CriaOrdem(string TPO, string Equipamento, string centro_p, string desc_ordem, string dt_ini, string dt_fim, string desc_breve, string Operacao, string Complemento_ChavePM03, string Componente_L, string Componente_N)
        {            

            IRfcFunction objRfc = repo.CreateFunction("ZRFC_FROTAWEB_CRIAORDEM"); 

            /*CABEÇALHO*/
            IRfcStructure objEstrutura = objRfc.GetStructure("I_CAB");

            objEstrutura.SetValue("TP_ORDEM" , TPO);
            objEstrutura.SetValue("EQUIPAMENTO" , Equipamento);
            objEstrutura.SetValue("CENTRO_PLANEJAMENTO" , centro_p);
            objEstrutura.SetValue("DESC_ORDEM" , desc_ordem);
            objEstrutura.SetValue("DT_INICIO" , dt_ini);
            objEstrutura.SetValue("DT_FIM" , dt_fim);
         

            /* OPERAÇÃO  */            
            /*LAY-OUT: 
             * CASO A CHAVE SEJA PM01(SERVIÇOS INTERNO/PRÓPRIO), BASTA CHAMAR ESTE BLOCO, CASO SEJA PM03(SERVIÇOS DE TERCEIROS, DEVE-SE CHAMAR ESTE BLOCO E O BLOCO LOGO ABAIXO DE COMPLEMENTO EM CASO DE PM03)
             * COMO PODEM TER DIVERSAS OPERAÇÕES, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
             * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
             * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
             * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
             * OPERACAO;CENTRO_TRABALHO; CHAVE_CONTROLE;TEXTO_BREVE;TEXTO_DESC;CENTRO
            */
   
                if (!String.IsNullOrEmpty(Operacao))
                {                    
                    string valor = Operacao;
                    string[] linhas = Regex.Split(valor, "]");
                    string[] Colunas;
                    string OPERACAO, CENTRO_TRABALHO, CHAVE_CONTROLE, TEXTO_BREVE, TEXTO_DESC, CENTRO;

                    for (int i = 0; i <= linhas.Length - 1; i++)
                    {
                        Colunas = Regex.Split(linhas[i], ";");
                        OPERACAO = Colunas[0];
                        CENTRO_TRABALHO = Colunas[1];
                        CHAVE_CONTROLE = Colunas[2];
                        TEXTO_BREVE = Colunas[3];
                        TEXTO_DESC = Colunas[4];
                        CENTRO = Colunas[5];

                        IRfcTable objPM01 = objRfc.GetTable("T_PM01");
                        objPM01.Append();
                        objPM01.SetValue("OPERACAO", OPERACAO);
                        objPM01.SetValue("CENTRO_TRABALHO", CENTRO_TRABALHO);
                        objPM01.SetValue("CHAVE_CONTROLE", CHAVE_CONTROLE);
                        objPM01.SetValue("TEXTO_BREVE", TEXTO_BREVE);
                        objPM01.SetValue("TEXTO_DESC", TEXTO_DESC);
                        objPM01.SetValue("CENTRO", CENTRO);                      
                    }
                }

                /* COMPLEMENTO EM CASO DE CHAVE PM03 - SERVIÇO EXTERNO/TERCEIRO  */
                /*LAY-OUT: 
                 * CASO A CHAVE SEJA PM03(SERVIÇOS EXTERNOS/TERCEIRO) NO BLOCO ACIMA, É PRECISO CHAMAR ESTE BLOCO)
                 * COMO PODEM TER DIVERSOS COMPLEMENTOS OPERAÇÕES DE OPERAÇÃO COM CHAVE PM03, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
                 * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
                 * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
                 * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
                 * OPERACAO; CENTRO_TRABALHO; CHAVE_CONTROLE; TEXTO_BREVE; TEXTO_DESC; CENTRO; GRUPO_MERCADORIAS; GRUPO_COMPRAS; REQUISITANTE; FORNECEDOR; ACOMPANHAMENTO; SERVICO; QUANTIDADE; UN_MEDIDA; RECEBEDOR;
                */
                if (!String.IsNullOrEmpty(Complemento_ChavePM03))
                {
                    string valor = Complemento_ChavePM03;
                    string[] linhas = Regex.Split(valor, "]");
                    string[] Colunas;
                    string OPERACAO, CENTRO_TRABALHO, CHAVE_CONTROLE, TEXTO_BREVE, TEXTO_DESC, CENTRO, GRUPO_MERCADORIAS, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, SERVICO, QUANTIDADE, UN_MEDIDA, RECEBEDOR,ORG_COMPRAS;

                    for (int i = 0; i <= linhas.Length - 1; i++)
                    {
                        Colunas = Regex.Split(linhas[i], ";");
                        OPERACAO = Colunas[0];
                        CENTRO_TRABALHO = Colunas[1];
                        CHAVE_CONTROLE = Colunas[2];
                        TEXTO_BREVE = Colunas[3];
                        TEXTO_DESC = Colunas[4];
                        CENTRO = Colunas[5];
                        GRUPO_MERCADORIAS = Colunas[6];
                        GRUPO_COMPRAS = Colunas[7];
                        REQUISITANTE = Colunas[8];
                        FORNECEDOR = Colunas[9];
                        ACOMPANHAMENTO = Colunas[10];
                        SERVICO = Colunas[11];
                        QUANTIDADE = Colunas[12];
                        UN_MEDIDA = Colunas[13];
                        RECEBEDOR = Colunas[14];
                        ORG_COMPRAS = Colunas[15];
                       
                        IRfcTable objPM03 = objRfc.GetTable("T_PM03");
                        objPM03.Append();
                        objPM03.SetValue("OPERACAO", OPERACAO);
                        objPM03.SetValue("CENTRO_TRABALHO", CENTRO_TRABALHO);
                        objPM03.SetValue("CHAVE_CONTROLE", CHAVE_CONTROLE);
                        objPM03.SetValue("TEXTO_BREVE", TEXTO_BREVE);
                        objPM03.SetValue("TEXTO_DESC", TEXTO_DESC);
                        objPM03.SetValue("CENTRO", CENTRO);  
                        objPM03.SetValue("GRUPO_MERCADORIAS", GRUPO_MERCADORIAS);
                        objPM03.SetValue("GRUPO_COMPRAS", GRUPO_COMPRAS);
                        objPM03.SetValue("REQUISITANTE", REQUISITANTE);
                        objPM03.SetValue("FORNECEDOR", FORNECEDOR);
                        objPM03.SetValue("ACOMPANHAMENTO", ACOMPANHAMENTO);
                        objPM03.SetValue("SERVICO", SERVICO);
                        objPM03.SetValue("QUANTIDADE", QUANTIDADE);
                        objPM03.SetValue("UN_MEDIDA", UN_MEDIDA);
                        objPM03.SetValue("RECEBEDOR", RECEBEDOR); 
                        objPM03.SetValue("Org_compras", ORG_COMPRAS);
                    }
                }


            /*COMPONENTE L - PEÇAS PRÓPRIAS/ESTOQUE*/

            /*COMPONENTES/PEÇAS DO ESTOQUE DA ORDEM DE MANUTENÇÃO*/
            /*LAY-OUT: 
             * COMO PODEM TER DIVERSAS PEÇAS, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
            * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
            * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
            * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
            * OPERACAO;TEXTO_DESC; UN_MEDIDA;COMPONENTE;QUANT_NECESSARIA;TP_ITEM;DEPOSITO;CENTRO]TEXTO_DESC; UN_MEDIDA;COMPONENTE;QUANT_NECESSARIA;TP_ITEM;DEPOSITO;CENTRO
            */

            if (!String.IsNullOrEmpty(Componente_L))
            {
                string valor = Componente_L;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, TEXTO_DESC, UN_MEDIDA, COMPONENTE, QUANT_NECESSARIA, TP_ITEM, DEPOSITO, CENTRO;

                for (int i = 0; i <= linhas.Length-1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];
                    TEXTO_DESC = Colunas[1];
                    UN_MEDIDA = Colunas[2];
                    COMPONENTE = Colunas[3];
                    QUANT_NECESSARIA = Colunas[4];
                    TP_ITEM = Colunas[5];
                    DEPOSITO = Colunas[6];
                    CENTRO = Colunas[7];

                    IRfcTable objCompL = objRfc.GetTable("T_COMPL");
                    objCompL.Append();
                    objCompL.SetValue("OPERACAO", OPERACAO);
                    objCompL.SetValue("TEXTO_DESC", TEXTO_DESC);
                    objCompL.SetValue("UN_MEDIDA", UN_MEDIDA);
                    objCompL.SetValue("COMPONENTE", COMPONENTE);
                    objCompL.SetValue("QUANT_NECESSARIA", QUANT_NECESSARIA);
                    objCompL.SetValue("TP_ITEM", TP_ITEM);
                    objCompL.SetValue("DEPOSITO", DEPOSITO);
                    objCompL.SetValue("CENTRO", CENTRO);
                }
            }

            /*COMPONENTE N - PEÇAS COMPRADAS/DÉBITO DIRETO*/

            /*COMPONENTES/PEÇAS COMPRA DIRETA DA ORDEM DE MANUTENÇÃO*/
            /*LAY-OUT: 
             * COMO PODEM TER DIVERSAS PEÇAS, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
            * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
            * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
            * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
            * OPERACAO;OPERACAO, GRUPO_COMPRAS; REQUISITANTE; FORNECEDOR; ACOMPANHAMENTO; UN_MEDIDA; COMPONENTE; QUANT_NECESSARIA; TP_ITEM; DEPOSITO; CENTRO; RECEBEDOR; MAT_FORNECEDOR]
            */

            if (!String.IsNullOrEmpty(Componente_N))
            {
                string valor = Componente_N;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, UN_MEDIDA, COMPONENTE, QUANT_NECESSARIA, TP_ITEM, DEPOSITO, CENTRO, RECEBEDOR, MAT_FORNECEDOR, ORG_COMPRAS, TEXTO_DESC;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];                   
                    GRUPO_COMPRAS = Colunas[1];
                    REQUISITANTE = Colunas[2];
                    FORNECEDOR = Colunas[3];
                    ACOMPANHAMENTO = Colunas[4];
                    UN_MEDIDA = Colunas[5];
                    COMPONENTE = Colunas[6];
                    QUANT_NECESSARIA = Colunas[7];
                    TP_ITEM = Colunas[8];
                    DEPOSITO = Colunas[9];
                    CENTRO = Colunas[10];
                    RECEBEDOR = Colunas[11];
                    MAT_FORNECEDOR = Colunas[12];
                    ORG_COMPRAS = Colunas[13];
                    TEXTO_DESC = Colunas[14];

                    IRfcTable objCompN = objRfc.GetTable("T_COMPN");
                    objCompN.Append();
                    objCompN.SetValue("OPERACAO", OPERACAO);
                    objCompN.SetValue("GRUPO_COMPRAS", GRUPO_COMPRAS);
                    objCompN.SetValue("REQUISITANTE", REQUISITANTE);
                    objCompN.SetValue("FORNECEDOR", FORNECEDOR);
                    objCompN.SetValue("ACOMPANHAMENTO", ACOMPANHAMENTO);
                    objCompN.SetValue("UN_MEDIDA", UN_MEDIDA);
                    objCompN.SetValue("COMPONENTE", COMPONENTE);
                    objCompN.SetValue("QUANT_NECESSARIA", QUANT_NECESSARIA);
                    objCompN.SetValue("TP_ITEM", TP_ITEM);
                    objCompN.SetValue("DEPOSITO", DEPOSITO);
                    objCompN.SetValue("CENTRO", CENTRO);
                    objCompN.SetValue("RECEBEDOR", RECEBEDOR);
                    objCompN.SetValue("MAT_FORNECEDOR", MAT_FORNECEDOR);
                    objCompN.SetValue("ORG_COMPRAS", ORG_COMPRAS);
                    objCompN.SetValue("TEXTO_DESC", TEXTO_DESC);
                }

            }
            
            objRfc.Invoke(dest);

           IRfcTable tabelaSAP = objRfc.GetTable("T_RETURN_REQ");
           return tabelaSAP.ToString() + " / " + objRfc.GetTable("T_RETURN_BAPI").ToString();
              
        }

        public String FechaOrdem(string NUM_ORDEM, string DESC_ORDEM, string DT_INICIO, string DT_FIM,string DT_REFER, string DESC_BREVE_ORDEM, string FECHA_ORDEM, string MODIFICA_ORDEM,string Exclui_Operacao,string Exclui_Componente, string Operacao, string Complemento_ChavePM03, string Componente_L, string Componente_N)
        {

           // IRfcFunction objRfc = repo.CreateFunction("ZRFC_FROTAWEB_FECHAORDEM");
            IRfcFunction objRfc = repo.CreateFunction("ZRFC_FROTAWEB_FECHAORDEM_NEW");

            /*CABEÇALHO*/
            IRfcStructure objEstrutura = objRfc.GetStructure("I_CAB");

            objEstrutura.SetValue("NUM_ORDEM", NUM_ORDEM);
            objEstrutura.SetValue("DESC_ORDEM", DESC_ORDEM);
            objEstrutura.SetValue("DT_INICIO", DT_INICIO);
            objEstrutura.SetValue("DT_FIM", DT_FIM);
            objEstrutura.SetValue("DT_REFER", DT_REFER);
           // objEstrutura.SetValue("DESC_BREVE_ORDEM", DESC_BREVE_ORDEM);
            objEstrutura.SetValue("FECHA_ORDEM", FECHA_ORDEM);
            objEstrutura.SetValue("MODIFICA_ORDEM", MODIFICA_ORDEM);



            /* OPERAÇÃO  */
            /*LAY-OUT: 
             * CASO A CHAVE SEJA PM01(SERVIÇOS INTERNO/PRÓPRIO), BASTA CHAMAR ESTE BLOCO, CASO SEJA PM03(SERVIÇOS DE TERCEIROS, DEVE-SE CHAMAR ESTE BLOCO E O BLOCO LOGO ABAIXO DE COMPLEMENTO EM CASO DE PM03)
             * COMO PODEM TER DIVERSAS OPERAÇÕES, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
             * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
             * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
             * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
             * OPERACAO;CENTRO_TRABALHO; CHAVE_CONTROLE;TEXTO_BREVE;TEXTO_DESC;CENTRO
            */

            if (!String.IsNullOrEmpty(Operacao))
            {
                string valor = Operacao;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, CENTRO_TRABALHO, CHAVE_CONTROLE, TEXTO_BREVE, TEXTO_DESC, CENTRO;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];
                    CENTRO_TRABALHO = Colunas[1];
                    CHAVE_CONTROLE = Colunas[2];
                    TEXTO_BREVE = Colunas[3];
                    TEXTO_DESC = Colunas[4];
                    CENTRO = Colunas[5];

                    IRfcTable objPM01 = objRfc.GetTable("T_PM01");
                    objPM01.Append();
                    objPM01.SetValue("OPERACAO", OPERACAO);
                    objPM01.SetValue("CENTRO_TRABALHO", CENTRO_TRABALHO);
                    objPM01.SetValue("CHAVE_CONTROLE", CHAVE_CONTROLE);
                    objPM01.SetValue("TEXTO_BREVE", TEXTO_BREVE);
                    objPM01.SetValue("TEXTO_DESC", TEXTO_DESC);
                    objPM01.SetValue("CENTRO", CENTRO);
                }
            }
/**/

            /* EXCLUIR OPERAÇÃO  */
            /*LAY-OUT:             
             * COMO PODEM EXCLUIR DIVERSAS OPERAÇÕES, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
             * AS OPERAÇÕES DEVEM SER SEPARADOS POR PONTO E VÍGULA (;) 
             * OPERACAO
            */

            if (!String.IsNullOrEmpty(Exclui_Operacao))
            {
                string valor = Exclui_Operacao;
                string[] linhas = Regex.Split(valor, ";");
               // string[] Colunas;
                string OPERACAO;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    OPERACAO = linhas[i];                 

                    IRfcTable objPM01 = objRfc.GetTable("T_EXCLUI_OPER");
                    objPM01.Append();
                    objPM01.SetValue("OPERACAO", OPERACAO);                    
                }
            }


            /* EXCLUIR COMPONENTE  */
            /*LAY-OUT:             
             * COMO PODEM EXCLUIR DIVERSAS OPERAÇÕES, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
             * AS OPERAÇÕES DEVEM SER SEPARADOS POR PONTO E VÍGULA (;) 
             * OPERACAO
            */

            if (!String.IsNullOrEmpty(Exclui_Componente))
            {
                string valor = Exclui_Componente;
                string[] linhas = Regex.Split(valor, ";");
                // string[] Colunas;
                string COMPONENTE;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    COMPONENTE = linhas[i];

                    IRfcTable objPM01 = objRfc.GetTable("T_EXCLUI_COMP");
                    objPM01.Append();
                    objPM01.SetValue("ITEM_EXCLUIR", COMPONENTE);
                }
            }



            /**/


            /* COMPLEMENTO EM CASO DE CHAVE PM03 - SERVIÇO EXTERNO/TERCEIRO  */
            /*LAY-OUT: 
             * CASO A CHAVE SEJA PM03(SERVIÇOS EXTERNOS/TERCEIRO) NO BLOCO ACIMA, É PRECISO CHAMAR ESTE BLOCO)
             * COMO PODEM TER DIVERSOS COMPLEMENTOS OPERAÇÕES DE OPERAÇÃO COM CHAVE PM03, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
             * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
             * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
             * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
             * OPERACAO; CENTRO_TRABALHO; CHAVE_CONTROLE; TEXTO_BREVE; TEXTO_DESC; CENTRO; GRUPO_MERCADORIAS; GRUPO_COMPRAS; REQUISITANTE; FORNECEDOR; ACOMPANHAMENTO; SERVICO; QUANTIDADE; UN_MEDIDA; RECEBEDOR;
            */
            if (!String.IsNullOrEmpty(Complemento_ChavePM03))
            {
                string valor = Complemento_ChavePM03;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, CENTRO_TRABALHO, CHAVE_CONTROLE, TEXTO_BREVE, TEXTO_DESC, CENTRO, GRUPO_MERCADORIAS, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, SERVICO, QUANTIDADE, UN_MEDIDA, RECEBEDOR, ORG_COMPRAS;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];
                    CENTRO_TRABALHO = Colunas[1];
                    CHAVE_CONTROLE = Colunas[2];
                    TEXTO_BREVE = Colunas[3];
                    TEXTO_DESC = Colunas[4];
                    CENTRO = Colunas[5];
                    GRUPO_MERCADORIAS = Colunas[6];
                    GRUPO_COMPRAS = Colunas[7];
                    REQUISITANTE = Colunas[8];
                    FORNECEDOR = Colunas[9];
                    ACOMPANHAMENTO = Colunas[10];
                    SERVICO = Colunas[11];
                    QUANTIDADE = Colunas[12];
                    UN_MEDIDA = Colunas[13];
                    RECEBEDOR = Colunas[14];
                    ORG_COMPRAS = Colunas[15];

                    IRfcTable objPM03 = objRfc.GetTable("T_PM03");
                    objPM03.Append();
                    objPM03.SetValue("OPERACAO", OPERACAO);
                    objPM03.SetValue("CENTRO_TRABALHO", CENTRO_TRABALHO);
                    objPM03.SetValue("CHAVE_CONTROLE", CHAVE_CONTROLE);
                    objPM03.SetValue("TEXTO_BREVE", TEXTO_BREVE);
                    objPM03.SetValue("TEXTO_DESC", TEXTO_DESC);
                    objPM03.SetValue("CENTRO", CENTRO);
                    objPM03.SetValue("GRUPO_MERCADORIAS", GRUPO_MERCADORIAS);
                    objPM03.SetValue("GRUPO_COMPRAS", GRUPO_COMPRAS);
                    objPM03.SetValue("ORG_COMPRAS", ORG_COMPRAS);
                    objPM03.SetValue("REQUISITANTE", REQUISITANTE);
                    objPM03.SetValue("FORNECEDOR", FORNECEDOR);
                    objPM03.SetValue("ACOMPANHAMENTO", ACOMPANHAMENTO);
                    objPM03.SetValue("SERVICO", SERVICO);
                    objPM03.SetValue("QUANTIDADE", QUANTIDADE);
                    objPM03.SetValue("UN_MEDIDA", UN_MEDIDA);
                    objPM03.SetValue("RECEBEDOR", RECEBEDOR);
                    
                }
            }


            /*COMPONENTE L - PEÇAS PRÓPRIAS/ESTOQUE*/

            /*COMPONENTES/PEÇAS DO ESTOQUE DA ORDEM DE MANUTENÇÃO*/
            /*LAY-OUT: 
             * COMO PODEM TER DIVERSAS PEÇAS, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
            * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
            * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
            * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
            * OPERACAO;TEXTO_DESC; UN_MEDIDA;COMPONENTE;QUANT_NECESSARIA;TP_ITEM;DEPOSITO;CENTRO]TEXTO_DESC; UN_MEDIDA;COMPONENTE;QUANT_NECESSARIA;TP_ITEM;DEPOSITO;CENTRO
            */
           
            if (!String.IsNullOrEmpty(Componente_L))
            {
                string valor = Componente_L;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
               // string ITEM;/*Valor Fixo*/
                
                string OPERACAO,ITEM, TEXTO_DESC, UN_MEDIDA, COMPONENTE, QUANT_NECESSARIA, TP_ITEM, DEPOSITO, CENTRO;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];
                    ITEM = Colunas[1];
                    TEXTO_DESC = Colunas[2];
                    UN_MEDIDA = Colunas[3];
                    COMPONENTE = Colunas[4];
                    QUANT_NECESSARIA = Colunas[5];
                    TP_ITEM = Colunas[6];
                    DEPOSITO = Colunas[7];
                    CENTRO = Colunas[8];

                    IRfcTable objCompL = objRfc.GetTable("T_COMPL");
                    objCompL.Append();
                    objCompL.SetValue("OPERACAO", OPERACAO);
                    objCompL.SetValue("ITEM", ITEM);
                    objCompL.SetValue("TEXTO_DESC", TEXTO_DESC);
                    objCompL.SetValue("UN_MEDIDA", UN_MEDIDA);
                    objCompL.SetValue("COMPONENTE", COMPONENTE);
                    objCompL.SetValue("QUANT_NECESSARIA", QUANT_NECESSARIA);
                    objCompL.SetValue("TP_ITEM", TP_ITEM);
                    objCompL.SetValue("DEPOSITO", DEPOSITO);
                    objCompL.SetValue("CENTRO", CENTRO);
                }
            }

            /*COMPONENTE N - PEÇAS COMPRADAS/DÉBITO DIRETO*/

            /*COMPONENTES/PEÇAS COMPRA DIRETA DA ORDEM DE MANUTENÇÃO*/
            /*LAY-OUT: 
             * COMO PODEM TER DIVERSAS PEÇAS, ESSE CAMPO DEVERÁ RECEBER DADOS ESTRUTURADOS, CONFORME O LAY-OUT ABAIXO:
            * OS CAMPOS DEVEM SER SEPARADOS POR PONTO E VÍGULA (;)
            * OS REGISTROS DEVEM SER SEPARDOS POR COCHETE DE FECHAMENTO (])
            * O ÚLTIMO REGISTRO NÃO DEVE SER FECHADO COM COCHETE(])
            * OPERACAO;OPERACAO, GRUPO_COMPRAS; REQUISITANTE; FORNECEDOR; ACOMPANHAMENTO; UN_MEDIDA; COMPONENTE; QUANT_NECESSARIA; TP_ITEM; DEPOSITO; CENTRO; RECEBEDOR; MAT_FORNECEDOR]
            */

            if (!String.IsNullOrEmpty(Componente_N))
            {
                string valor = Componente_N;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, ITEM,TEXTO_DESC, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, UN_MEDIDA, COMPONENTE, QUANT_NECESSARIA, TP_ITEM, DEPOSITO, CENTRO, RECEBEDOR, MAT_FORNECEDOR, ORG_COMPRAS;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");
                    OPERACAO = Colunas[0];
                    ITEM = Colunas[1];
                    TEXTO_DESC = Colunas[2];
                    GRUPO_COMPRAS = Colunas[3];
                    REQUISITANTE = Colunas[4];
                    FORNECEDOR = Colunas[5];
                    ACOMPANHAMENTO = Colunas[6];
                    UN_MEDIDA = Colunas[7];
                    COMPONENTE = Colunas[8];
                    QUANT_NECESSARIA = Colunas[9];
                    TP_ITEM = Colunas[10];
                    DEPOSITO = Colunas[11];
                    CENTRO = Colunas[12];
                    RECEBEDOR = Colunas[13];
                    MAT_FORNECEDOR = Colunas[14];
                    ORG_COMPRAS = Colunas[15];

                    IRfcTable objCompN = objRfc.GetTable("T_COMPN");
                    objCompN.Append();
                    objCompN.SetValue("OPERACAO", OPERACAO);
                    objCompN.SetValue("ITEM", ITEM);
                    objCompN.SetValue("TEXTO_DESC", TEXTO_DESC);                   

                    objCompN.SetValue("GRUPO_COMPRAS", GRUPO_COMPRAS);
                    objCompN.SetValue("REQUISITANTE", REQUISITANTE);
                    objCompN.SetValue("FORNECEDOR", FORNECEDOR);
                    objCompN.SetValue("ACOMPANHAMENTO", ACOMPANHAMENTO);
                    objCompN.SetValue("UN_MEDIDA", UN_MEDIDA);
                    objCompN.SetValue("COMPONENTE", COMPONENTE);
                    objCompN.SetValue("QUANT_NECESSARIA", QUANT_NECESSARIA);
                    objCompN.SetValue("TP_ITEM", TP_ITEM);
                    objCompN.SetValue("DEPOSITO", DEPOSITO);
                    objCompN.SetValue("CENTRO", CENTRO);
                    objCompN.SetValue("RECEBEDOR", RECEBEDOR);
                    objCompN.SetValue("MAT_FORNECEDOR", MAT_FORNECEDOR);
                   // objCompN.SetValue("ORG_COMPRAS", ORG_COMPRAS);
                }

            }

            objRfc.Invoke(dest);

            IRfcTable tabelaSAP = objRfc.GetTable("T_RETURN_REQ");
            return tabelaSAP.ToString() + " / " + objRfc.GetTable("T_RETURN_BAPI").ToString();

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
                tblOptions3.SetValue("TEXT", "KUNNR EQ '" + cod_sap + "' AND MANDT EQ '500'");

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
                tblOptions.SetValue("TEXT", "  AND BLART IN ('DI','DA','RV')");

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
            tblOptions.SetValue("TEXT", "KUNNR EQ '" + codcli.ToString().PadLeft(10, '0') + "' AND MANDT EQ '500'");

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
                IRfcFunction rfc_read_table3 = repo.CreateFunction("/BODS/RFC_READ_TABLE"); //Chama RFC clientes vendas KNVV
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

                    tblOptions_rota.SetValue("TEXT", "ZZ_ROTA EQ '" + rota + "' OR ZZ_ROTAE EQ '" + rota + "'");

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
                    string[] fieldValues3;
                    fieldValues3 = returnTable3.GetValue("WA").ToString().Split(';');

                    /*Fim Pegar as rotas cliente KNVk*/


                    row["COMPRADOR"] = fieldValues3[0]; ;
                    row["ENDERECO"] = fieldValues[4];
                    row["BAIRRO"] = fieldValues[5];

                    row["CIDADE"] = fieldValues[8];
                    row["UF"] = fieldValues[9];
                    row["CEP"] = fieldValues[10];
                    row["TELEFONE1"] = fieldValues[11];
                    row["INSCRICAO_ESTAD"] = fieldValues[12];

                    row["STATUS"] = fieldValues[6];

                    /*Pegar as rotas do cliente na KNVV*/

                    /*Faz a seleção do Registro*/
                    IRfcTable tblOptions2 = rfc_read_table2.GetTable("OPTIONS");
                    tblOptions2.Clear();
                    tblOptions2.Append();
                    tblOptions2.SetValue("TEXT", "KUNNR EQ '" + fieldValues[0] + "' AND MANDT EQ '" + fieldValues[7] + "'");

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
            fieldsTable.SetValue("FIELDNAME", "ERDAT");//DATA CRIAÇÃO 
            fieldsTable.Append();
            fieldsTable.SetValue("FIELDNAME", "TPLST");//UNIDADE DE TRANSPORTE
            

            /*Faz a seleção do Registro*/
            IRfcTable tblOptions = rfc_read_table.GetTable("OPTIONS");
            tblOptions.Clear();
            tblOptions.Append();
            tblOptions.SetValue("TEXT", "ERDAT >= '" + data_inicial + "' AND ERDAT <= '" + data_final + "'");
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
                IRfcFunction funcBAPI = repo.CreateFunction("Z_RFC_MVT_LGORT");

                /*Parâmetros 1*/
                funcBAPI.SetValue("PI_PSTNG_DATE", "20150101");
                funcBAPI.SetValue("PI_DOC_DATE", "20150101");
                funcBAPI.SetValue("PI_AVAILABILITY", ""); //Campo utilizado, caso queira consumir estoque
                /*Parâmetros 2 - Tabela*/
                IRfcTable objEstrutura = funcBAPI.GetTable("T_MVT_LGORT01");
                objEstrutura.Append();
                objEstrutura.SetValue("MATERIAL", cod_prod.ToString().PadLeft(18, '0'));//Produto
                objEstrutura.SetValue("PLANT", centro);//Centro
                objEstrutura.SetValue("STGE_LOC", "APA");//Centro
                objEstrutura.SetValue("ENTRY_QNT", "99.999.999,999");//Centro

                funcBAPI.Invoke(dest);

                IRfcTable table_return = funcBAPI.GetTable("T_MVT_LGORT01");

                row["STATUS"] = "S";
                row["MESSAGE"] = "Registro encontrado.";
               
                row["CENTRO"] = centro;
                row["COD_PROD"] = table_return.GetString("MATERIAL");
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

    }
}