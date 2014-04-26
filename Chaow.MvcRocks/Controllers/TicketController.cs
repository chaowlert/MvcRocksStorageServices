using System.Net;
using System.Web.Http;
using Chaow.MvcRocks.Models;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Chaow.MvcRocks.Controllers
{
    public class TicketController : ApiController
    {
        public void Post(int count)
        {   
            var context = new TicketContext();
            for (int i = 0; i < count; i++)
            {
                context.ticket.AddMessage(new CloudQueueMessage(i.ToString()));
            }
        }

        public void Get()
        {
            var context = new TicketContext();
            var msg = context.ticket.GetMessage();
            if (msg == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            context.ticket.DeleteMessage(msg);
        }
    }
}
