using StateManagment.Entity;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Services
{
    public class ContactService
    {
        private readonly IChangeProcessor changeProcessor;
        private readonly IDataRetriever dataRetriver;

        public ContactService(IChangeProcessor changeProcessor, IDataRetriever dataRetriver)
        {
            this.changeProcessor = changeProcessor;
            this.dataRetriver = dataRetriver;
        }

        public async Task<MessageEnvelop> Post(Contact contact, bool submit)
        {
            var envelop = new MessageEnvelop
            {
                Change = ChangeType.Create,
                Name = EntityName.Contact,
                Draft = contact,
                IsSubmitted = submit
            };
            await changeProcessor.ProcessChangeAsync(envelop);

            return await dataRetriver.GetEntityEnvelop(envelop.EntityId, envelop.Name);
        }
    }
}
