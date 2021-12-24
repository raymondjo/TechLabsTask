using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel.Description;
using Microsoft.Xrm.Tooling.Connector;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

namespace TechLabsTask
{
    public static class Function1
    {
        [FunctionName("TechLabsTask")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string email = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "email", true) == 0)
                .Value;
            string firstName = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "firstName", true) == 0)
               .Value;
            string lastName = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "lastName", true) == 0)
               .Value;

            if (email == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                email = data?.email;
                firstName = data?.firstName;
                lastName = data?.lastName;
            }
            IOrganizationService service = Connection(log);
            string responseMessage = "";
            if (service != null)
            {
                //If Connection is established
                var contactEntities = GetEntityCollection(service, "contact", "emailaddress1", email, new ColumnSet("contactid"));

                if (contactEntities.Entities.Count > 0)
                {
                    updateContacts(service, contactEntities, firstName, lastName);
                    responseMessage = "Contacts with email ="+email+" updated successfully";
                }
                else
                {
                    Entity createdContactEntity = createContact(log, service, firstName, lastName, email);
                    responseMessage = createdContactEntity !=null ?
                                                            "contact created successfully with Guid = " + createdContactEntity.Id.ToString()
                                                            :"There is an error will contact creation";
                }
            }

            return email == null || firstName == null || lastName == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass first name , last name and email on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, responseMessage);
        }

        private static Entity createContact(TraceWriter log,IOrganizationService service, string firstName, string lastName, string email)
        {
            try
            {
                Entity contact = new Entity("contact");
                contact["firstname"]     = firstName;
                contact["lastname"]      = lastName;
                contact["emailaddress1"] = email;
                service.Create(contact);
                return contact;
            }catch(Exception ex)
            {
                 log.Info(ex.Message);
                return null;
            }
        }

        private static void updateContacts(IOrganizationService service, EntityCollection contactEntities, string firstName, string lastName)
        {
            ExecuteTransactionRequest transactionReq = new ExecuteTransactionRequest();

            transactionReq.Requests = new OrganizationRequestCollection();

            foreach (Entity entity in contactEntities.Entities)
            {
                entity["lastname"] = lastName;
                entity["firstname"] = firstName;
                
                UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                transactionReq.Requests.Add(updateRequest);
            }
            
            ExecuteTransactionResponse response = (ExecuteTransactionResponse)service.Execute(transactionReq);
        }

        private static IOrganizationService Connection(TraceWriter log)
        {
            #region Credentials Code
            //Credentials
            string URL = "https://interviews.api.crm4.dynamics.com/XRMServices/2011/Organization.svc";
            string userName = "danj@CRM097147.OnMicrosoft.com";
            string password = "TechLabs1";
            string AuthType = "Office365";
            //if you are using App password then add AuthType=Office365
            #endregion
            ClientCredentials credentials = new ClientCredentials();
            credentials.UserName.UserName = userName;
            credentials.UserName.Password = password;
            Uri serviceUri = new Uri(URL);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors) { return true; };
            OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);

            // to increase the time out of the request
            proxy.Timeout = new TimeSpan(1, 0, 0); //hour , min, sec
            proxy.EnableProxyTypes();

            IOrganizationService Service = (IOrganizationService)proxy;
            Guid userid = ((WhoAmIResponse)Service.Execute(new WhoAmIRequest())).UserId;
            if (userid != Guid.Empty)
            {
                Console.WriteLine("Connection Established Successfully");
                return Service;
            }
            return null;
        }

        private static EntityCollection GetEntityCollection(IOrganizationService service, string entityName, string attributeName, string attributeValue, ColumnSet cols)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = cols,
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = attributeName,
                            Operator = ConditionOperator.Equal,
                            Values = { attributeValue }
                        }
                    }
                }
            };
            return service.RetrieveMultiple(query);
        }
    }
}
