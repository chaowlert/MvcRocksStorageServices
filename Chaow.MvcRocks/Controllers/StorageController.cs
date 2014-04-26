using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Chaow.MvcRocks.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Controllers
{
    public class StorageController : ApiController
    {
        public IQueryable<LoadItem> Get()
        {
            var db = new StorageLoadContext();
            return db.UnorderedSet.CreateQuery<LoadItem>().AsQueryable().Take(1000);
        }

        public async Task<LoadItem> Get(string id, CancellationToken cancellationToken)
        {
            var db = new StorageLoadContext();
            var op = TableOperation.Retrieve<LoadItem>(id, "");
            var result = await db.UnorderedSet.ExecuteAsync(op, cancellationToken);
            return (LoadItem)result.Result;
        }

        public Task Post(string id, CancellationToken cancellationToken)
        {
            switch (id)
            {
                case "ordered":
                    {
                        var item = new LoadItem2
                        {
                            PartitionKey = DateTime.UtcNow.Ticks.ToString("x16") + Guid.NewGuid().ToString("N"),
                            RowKey = ""
                        };
                        var op = TableOperation.Insert(item);
                        var db = new StorageLoadContext();
                        return db.OrderedSet.ExecuteAsync(op, cancellationToken);
                    }
                case "unordered":
                    {
                        var item = new LoadItem
                        {
                            PartitionKey = Guid.NewGuid().ToString("N"),
                            RowKey = ""
                        };
                        var op = TableOperation.Insert(item);
                        var db = new StorageLoadContext();
                        return db.UnorderedSet.ExecuteAsync(op, cancellationToken);
                    }
                default:
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "only ordered and unordered allowed"));
            }
        }

        public async Task Put(string id, CancellationToken cancellationToken)
        {
            var db = new StorageLoadContext();
            var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token;
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(timeout, cancellationToken).Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                var op = TableOperation.Retrieve<LoadItem>(id, "");
                var result = await db.UnorderedSet.ExecuteAsync(op, cancellationToken);
                var item = (LoadItem)result.Result;
                item.Value++;
                var op2 = TableOperation.Replace(item);
                try
                {
                    await db.UnorderedSet.ExecuteAsync(op2, cancellationToken);
                    return;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode != (int)HttpStatusCode.PreconditionFailed &&
                        e.RequestInformation.HttpStatusCode != (int)HttpStatusCode.Conflict)
                        throw;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.RequestTimeout, "Update timeout"));
        }
    }
}
