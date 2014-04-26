using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Models
{
    public class TodoContext : StorageContext
    {
        public CloudTable Todo { get; set; }
    }
}