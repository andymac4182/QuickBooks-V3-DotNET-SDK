﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Intuit.Ipp.Data;
using Intuit.Ipp.Exception;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.Security;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Intuit.Ipp.Core.Test.Common
{
    internal class Initializer
    {
        static OAuth2Client oauthClient = null;
        public static Dictionary<string, string> tokenDict = new Dictionary<string, string>();
        public static string pathFile = "";
        static int counter = 0;

        static IConfigurationRoot builder;
        public Initializer(string path)
        {
            //AuthorizationKeysQBO.tokenFilePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))), Assembly.GetCallingAssembly().GetName().Name, "TokenStore.json");
            AuthorizationKeysQBO.tokenFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TokenStore.json");
            string name= Assembly.GetCallingAssembly().GetName().Name;
            builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile(path, optional: true, reloadOnChange: true)
                 .Build();


            AuthorizationKeysQBO.accessTokenQBO = builder.GetSection("Oauth2Keys")["AccessToken"];

            AuthorizationKeysQBO.refreshTokenQBO = builder.GetSection("Oauth2Keys")["RefreshToken"];
            AuthorizationKeysQBO.clientIdQBO = builder.GetSection("Oauth2Keys")["ClientId"];
            AuthorizationKeysQBO.clientSecretQBO = builder.GetSection("Oauth2Keys")["ClientSecret"];
            AuthorizationKeysQBO.realmIdIAQBO = builder.GetSection("Oauth2Keys")["RealmId"];
            AuthorizationKeysQBO.redirectUrl = builder.GetSection("Oauth2Keys")["RedirectUrl"];
            AuthorizationKeysQBO.qboBaseUrl = builder.GetSection("Oauth2Keys")["QBOBaseUrl"];
            AuthorizationKeysQBO.appEnvironment = builder.GetSection("Oauth2Keys")["Environment"];

            FileInfo fileinfo = new FileInfo(AuthorizationKeysQBO.tokenFilePath);
            string jsonFile = File.ReadAllText(fileinfo.FullName);
            var jObj = JObject.Parse(jsonFile);
            jObj["Oauth2Keys"]["AccessToken"] = AuthorizationKeysQBO.accessTokenQBO;
            jObj["Oauth2Keys"]["RefreshToken"] = AuthorizationKeysQBO.refreshTokenQBO;

            string output = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            File.WriteAllText(fileinfo.FullName, output);

            counter++;


        }
        public Initializer() : this("Appsettings.json")
        {

        }
        private static void Initialize()
        {
            Initializer initializer = new Initializer();
        }
        internal static ServiceContext InitializeServiceContextQbo(string status=null)
        {
           

            if (counter == 0)
                Initialize();
            else
            {
                //Load the second json file
                FileInfo fileinfo = new FileInfo(AuthorizationKeysQBO.tokenFilePath);
                string jsonFile = File.ReadAllText(fileinfo.FullName);
                var jObj = JObject.Parse(jsonFile);
                AuthorizationKeysQBO.accessTokenQBO = jObj["Oauth2Keys"]["AccessToken"].ToString();
                AuthorizationKeysQBO.refreshTokenQBO = jObj["Oauth2Keys"]["RefreshToken"].ToString();
            }

            ServiceContext context = null;
            OAuth2RequestValidator reqValidator = null;
            try
            {
                if(status=="Invalid")
                    reqValidator = new OAuth2RequestValidator(status);
                else
                reqValidator = new OAuth2RequestValidator(AuthorizationKeysQBO.accessTokenQBO);
                context = new ServiceContext(AuthorizationKeysQBO.realmIdIAQBO, IntuitServicesType.QBO, reqValidator);
                context.IppConfiguration.MinorVersion.Qbo = "43";
                DataService.DataService service = new DataService.DataService(context);
                var compinfo = service.FindAll<CompanyInfo>(new CompanyInfo());
                return context;
            }
            catch (IdsException ex)
            {
                if (ex.Message == "Unauthorized-401")
                {
                    var serviceContext11 = Helper.GetNewTokens_ServiceContext();
                    return serviceContext11;

                }
                else
                {
                    throw;
                }
            }

        }


    }
}
