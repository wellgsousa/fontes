using System;
using System.Collections.Generic;
using System.Web;
using SAP.Middleware.Connector;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using System.IO;


namespace sapw
{
    public class Rfc_Frota
    {
        RfcRepository repo;
        RfcDestination dest;
        int guarda_prox_complementoPM03=0;

        public Rfc_Frota()
        {
            try
            {
                SAPConnect objcon = new SAPConnect();
                RfcDestinationManager.RegisterDestinationConfiguration(objcon);
                dest = RfcDestinationManager.GetDestination("Frota");
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
            if (centro_p == "0301" || centro_p == "301") { centro_p = "B301"; }
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
             * OPERACAO; CENTRO_TRABALHO; CHAVE_CONTROLE; TEXTO_BREVE; PRECO; TEXTO_DESC; CENTRO; GRUPO_MERCADORIAS; GRUPO_COMPRAS; REQUISITANTE; FORNECEDOR; ACOMPANHAMENTO; SERVICO; QUANTIDADE; UN_MEDIDA; RECEBEDOR;
            */
            if (!String.IsNullOrEmpty(Complemento_ChavePM03))
            {
                string valor = Complemento_ChavePM03;
                string[] linhas = Regex.Split(valor, "]");
                string[] Colunas;
                string OPERACAO, CENTRO_TRABALHO, CHAVE_CONTROLE, TEXTO_BREVE, PRECO, TEXTO_DESC, CENTRO, GRUPO_MERCADORIAS, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, SERVICO, QUANTIDADE, UN_MEDIDA, RECEBEDOR, ORG_COMPRAS;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas             = Regex.Split(linhas[i], ";");
                    OPERACAO            = Colunas[0];
                    CENTRO_TRABALHO     = Colunas[1];
                    CHAVE_CONTROLE      = Colunas[2];
                    TEXTO_BREVE         = Colunas[3];
                    PRECO               = Colunas[4];
                    TEXTO_DESC          = Colunas[5];
                    CENTRO              = Colunas[6];
                    GRUPO_MERCADORIAS   = Colunas[7];
                    GRUPO_COMPRAS       = Colunas[8];
                    REQUISITANTE        = Colunas[9];
                    FORNECEDOR          = Colunas[10];
                    ACOMPANHAMENTO      = Colunas[11];
                    SERVICO             = Colunas[12];
                    QUANTIDADE          = Colunas[13];
                    UN_MEDIDA           = Colunas[14];
                    RECEBEDOR           = Colunas[15];
                    ORG_COMPRAS         = Colunas[16];

                    IRfcTable objPM03 = objRfc.GetTable("T_PM03");
                    objPM03.Append();
                    objPM03.SetValue("OPERACAO", OPERACAO);
                    objPM03.SetValue("CENTRO_TRABALHO", CENTRO_TRABALHO);
                    objPM03.SetValue("CHAVE_CONTROLE", CHAVE_CONTROLE);
                    objPM03.SetValue("TEXTO_BREVE", TEXTO_BREVE);
                    objPM03.SetValue("PRICE", (Convert.ToDouble(PRECO) / Convert.ToDouble(QUANTIDADE)));
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
                    TP_ITEM = Colunas[7];
                    DEPOSITO = Colunas[8];
                    CENTRO = Colunas[9];

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
                string OPERACAO, ITEM,TEXTO_DESC, GRUPO_COMPRAS, REQUISITANTE, FORNECEDOR, ACOMPANHAMENTO, UN_MEDIDA, COMPONENTE, QUANT_NECESSARIA, PRECO, TP_ITEM, DEPOSITO, CENTRO, RECEBEDOR, MAT_FORNECEDOR, ORG_COMPRAS;

                for (int i = 0; i <= linhas.Length - 1; i++)
                {
                    Colunas = Regex.Split(linhas[i], ";");

                    //Double val = (Convert.ToDouble(Colunas[10]) / Convert.ToDouble(Colunas[9]) / 100);

                    OPERACAO            = Colunas[0];
                    ITEM                = Colunas[1];
                    TEXTO_DESC          = Colunas[2];
                    GRUPO_COMPRAS       = Colunas[3];
                    REQUISITANTE        = Colunas[4];
                    FORNECEDOR          = Colunas[5];
                    ACOMPANHAMENTO      = Colunas[6];
                    UN_MEDIDA           = Colunas[7];
                    COMPONENTE          = Colunas[8];
                    QUANT_NECESSARIA    = Colunas[9];
                   // PRECO             = Convert.ToString(val).Replace(",", ".");
                    PRECO               = Colunas[10];
                    TP_ITEM             = Colunas[11];
                    DEPOSITO            = Colunas[12];
                    if (Colunas[13] == "0301" || Colunas[13] == "301") { Colunas[13] = "B301"; }
                    if (Colunas[13] == "0201" || Colunas[13] == "201") { Colunas[13] = "B201"; }
                    CENTRO              = Colunas[13];
                    RECEBEDOR           = Colunas[14];
                    MAT_FORNECEDOR      = Colunas[15];
                    ORG_COMPRAS         = Colunas[16];

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
                
                    Decimal val_ = 0;
                    PRECO = PRECO.Replace(".", ",");
                    val_ = Convert.ToDecimal(PRECO);                  

                    objCompN.SetValue("PRICE", Convert.ToString(val_).Replace(",", "."));
                    //objCompN.SetValue("PRICE", PRECO);
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




    }
}