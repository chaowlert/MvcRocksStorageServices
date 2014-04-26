using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Chaow.MvcRocks.Models;

namespace Chaow.MvcRocks.Controllers
{
    public class SqlController : ApiController
    {
        readonly Lazy<SqlLoadContext> _db = new Lazy<SqlLoadContext>(GetLoadContext);
        static SqlLoadContext GetLoadContext()
        {
            var db = new SqlLoadContext();
            var config = db.Configuration;
            config.LazyLoadingEnabled = false;
            config.ProxyCreationEnabled = false;
            config.ValidateOnSaveEnabled = false;
            return db;
        }

        public IQueryable<LoadItem> Get()
        {
            return _db.Value.UnorderedSet.AsNoTracking().Take(1000);
        }

        public Task<LoadItem> Get(string id, CancellationToken cancellationToken)
        {
            return _db.Value.UnorderedSet.AsNoTracking().FirstOrDefaultAsync(item => item.PartitionKey == id && item.RowKey == "", cancellationToken);
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
                    _db.Value.OrderedSet.Add(item);
                    break;
                }
                case "unordered":
                {
                    var item = new LoadItem
                    {
                        PartitionKey = Guid.NewGuid().ToString("N"), 
                        RowKey = ""
                    };
                    _db.Value.UnorderedSet.Add(item);
                    break;
                }
                default:
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "only ordered and unordered allowed"));
            }
            return _db.Value.SaveChangesAsync(cancellationToken);
        }

        public Task Put(string id, CancellationToken cancellationToken)
        {
            return _db.Value.Database.ExecuteSqlCommandAsync("UPDATE LoadItems SET Value = Value + 1 WHERE PartitionKey = @p0 and RowKey = ''", cancellationToken, id);
        }

        protected override void Dispose(bool disposing)
        {
            if (_db.IsValueCreated)
            {
                _db.Value.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
