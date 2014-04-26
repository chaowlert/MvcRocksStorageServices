using Microsoft.WindowsAzure.Storage.Queue;

namespace Chaow.MvcRocks.Models
{
    public class TicketContext : StorageContext
    {
        public CloudQueue ticket { get; set; }
    }
}