using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;
using SAP.Middleware.Connector;
using System.Data;
using System.Configuration;

namespace sapw
{
    public class SAPConnect:IDestinationConfiguration
    {

        public bool ChangeEventsSupported()
        {
            return true;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        
        public RfcConfigParameters GetParameters(string destinationName)
        {       
                SAP.Middleware.Connector.RfcConfigParameters conf = new SAP.Middleware.Connector.RfcConfigParameters();

                if (destinationName=="Frota")
                {

                    conf.Add(RfcConfigParameters.AppServerHost, ConfigurationManager.AppSettings["Frota_AppServerHost"].ToString());
                    conf.Add(RfcConfigParameters.GatewayHost, ConfigurationManager.AppSettings["Frota_GatewayHost"].ToString());
                    conf.Add(RfcConfigParameters.GatewayService, ConfigurationManager.AppSettings["Frota_GatewayService"].ToString());
                    conf.Add(RfcConfigParameters.SystemNumber, ConfigurationManager.AppSettings["Frota_SystemNumber"].ToString());
                    conf.Add(RfcConfigParameters.User, ConfigurationManager.AppSettings["Frota_User"].ToString());
                    conf.Add(RfcConfigParameters.Password, ConfigurationManager.AppSettings["Frota_Password"].ToString());
                    conf.Add(RfcConfigParameters.Client, ConfigurationManager.AppSettings["Frota_Client"].ToString());
                    conf.Add(RfcConfigParameters.Language, ConfigurationManager.AppSettings["Frota_Language"].ToString());
                    conf.Add(RfcConfigParameters.PoolSize, ConfigurationManager.AppSettings["Frota_PoolSize"].ToString());
                    conf.Add(RfcConfigParameters.MaxPoolSize, ConfigurationManager.AppSettings["Frota_MaxPoolSize"].ToString());
                    conf.Add(RfcConfigParameters.IdleTimeout, ConfigurationManager.AppSettings["Frota_IdleTimeout"].ToString());
                }
                else
                {
                    conf.Add(RfcConfigParameters.AppServerHost, ConfigurationManager.AppSettings["AppServerHost"].ToString());
                    conf.Add(RfcConfigParameters.GatewayHost, ConfigurationManager.AppSettings["GatewayHost"].ToString());
                    conf.Add(RfcConfigParameters.GatewayService, ConfigurationManager.AppSettings["GatewayService"].ToString());
                    conf.Add(RfcConfigParameters.SystemNumber, ConfigurationManager.AppSettings["SystemNumber"].ToString());
                    conf.Add(RfcConfigParameters.User, ConfigurationManager.AppSettings["User"].ToString());
                    conf.Add(RfcConfigParameters.Password, ConfigurationManager.AppSettings["Password"].ToString());
                    conf.Add(RfcConfigParameters.Client, ConfigurationManager.AppSettings["Client"].ToString());
                    conf.Add(RfcConfigParameters.Language, ConfigurationManager.AppSettings["Language"].ToString());
                    conf.Add(RfcConfigParameters.PoolSize, ConfigurationManager.AppSettings["PoolSize"].ToString());
                    conf.Add(RfcConfigParameters.MaxPoolSize, ConfigurationManager.AppSettings["MaxPoolSize"].ToString());
                    conf.Add(RfcConfigParameters.IdleTimeout, ConfigurationManager.AppSettings["IdleTimeout"].ToString());
                }
                return conf;            
        }  
    } 
}