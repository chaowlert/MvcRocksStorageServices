using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Models
{
    public class LoadItem : TableEntity
    {
        public int Value { get; set; }
    }

    public class LoadItem2 : TableEntity
    {
        public int Value { get; set; }
    }
}