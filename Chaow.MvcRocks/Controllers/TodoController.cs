using System;
using System.Web.Http;
using Chaow.MvcRocks.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Controllers
{
    public class TodoController : ApiController
    {
        public object Get(string id)
        {
            var context = new TodoContext();
            var policy = new SharedAccessTablePolicy
            {
                Permissions = SharedAccessTablePermissions.Add |
                              SharedAccessTablePermissions.Delete |
                              SharedAccessTablePermissions.Query |
                              SharedAccessTablePermissions.Update,
                SharedAccessExpiryTime = DateTime.Now.AddHours(1),
            };
            var sas = context.Todo.GetSharedAccessSignature(policy,
                accessPolicyIdentifier: null,
                startPartitionKey: id,
                startRowKey: null,
                endPartitionKey: id,
                endRowKey: null);

            return new {id, uri = context.Todo.Uri, sas};
        }
    }
}
