using Microsoft.WindowsAzure.Storage.Blob;

namespace Chaow.MvcRocks.Models
{
    public class MovieStoreContext : StorageContext
    {
        public CloudBlobContainer video { get; set; }
    }
}