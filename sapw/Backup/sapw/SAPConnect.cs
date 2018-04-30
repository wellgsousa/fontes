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
                return conf;            
        }  
    } 
}