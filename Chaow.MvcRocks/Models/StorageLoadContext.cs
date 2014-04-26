using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Models
{
    public class StorageLoadContext : StorageContext
    {
        public CloudTable OrderedSet { get; set; }

        public CloudTable UnorderedSet { get; set; }
    }
}