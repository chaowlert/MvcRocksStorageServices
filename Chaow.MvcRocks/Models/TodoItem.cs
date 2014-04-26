using Microsoft.WindowsAzure.Storage.Table;

namespace Chaow.MvcRocks.Models
{
    public class TodoItem : TableEntity
    {
        public bool done { get; set; }
        public string text { get; set; }
    }
}