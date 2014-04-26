using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using FastMember;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Models
{
    public abstract class StorageContext
    {
        static readonly ConcurrentDictionary<string, Dictionary<string, object>> _accounts = 
            new ConcurrentDictionary<string, Dictionary<string, object>>();

        protected StorageContext()
        {
            var key = ConfigurationManager.ConnectionStrings[this.GetType().Name].ConnectionString;
            var props = _accounts.GetOrAdd(key, CreateAccount);
            initializeProperties(props);
        }

        protected StorageContext(string connectionString)
        {
            var props = _accounts.GetOrAdd(connectionString, CreateAccount);
            initializeProperties(props);
        }

        void initializeProperties(Dictionary<string, object> props)
        {
            var type = TypeAccessor.Create(this.GetType());
            foreach (var prop in props)
            {
                type[this, prop.Key] = prop.Value;
            }
        }

        Dictionary<string, object> CreateAccount(string key)
        {
            var account = CloudStorageAccount.Parse(key);
            var tableClient = account.CreateCloudTableClient();
            var queueClient = account.CreateCloudQueueClient();
            var blobClient = account.CreateCloudBlobClient();

            Configure(tableClient.GetServiceProperties(), props => tableClient.SetServiceProperties(props));
            Configure(queueClient.GetServiceProperties(), props => queueClient.SetServiceProperties(props));
            Configure(blobClient.GetServiceProperties(), props => blobClient.SetServiceProperties(props));

            var type = TypeAccessor.Create(this.GetType());
            var dict = new Dictionary<string, object>();
            var tasks = new List<Task>();
            foreach (var member in type.GetMembers())
            {
                if (member.Type == typeof(CloudTable))
                {
                    var table = tableClient.GetTableReference(member.Name);
                    var task = table.CreateIfNotExistsAsync();
                    tasks.Add(task);
                    dict.Add(member.Name, table);
                }
                else if (member.Type == typeof(CloudQueue))
                {
                    var queue = queueClient.GetQueueReference(member.Name);
                    var task = queue.CreateIfNotExistsAsync();
                    tasks.Add(task);
                    dict.Add(member.Name, queue);
                }
                else if (member.Type == typeof(CloudBlobContainer))
                {
                    var container = blobClient.GetContainerReference(member.Name);
                    var task = container.CreateIfNotExistsAsync();
                    tasks.Add(task);
                    dict.Add(member.Name, container);
                }
            }
            Task.WaitAll(tasks.ToArray());
            return dict;
        }

        static void Configure(ServiceProperties serviceProps, Action<ServiceProperties> applyAction)
        {
            if (serviceProps.Cors != null && serviceProps.Cors.CorsRules.Count > 0)
                return;

            serviceProps.Cors = new CorsProperties();
            serviceProps.Cors.CorsRules.Add(new CorsRule
            {
                AllowedHeaders = new List<string> { "*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post | CorsHttpMethods.Delete,
                AllowedOrigins = new List<string> { "*" },
                ExposedHeaders = new List<string> { "*" },
                MaxAgeInSeconds = 1800 // 30 minutes
            });
            applyAction(serviceProps);
        }
    }
}