using Api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Api.Controllers
{
    internal class MessageEnvelop_ToEntityResponse_ProductAgreement
    {
        internal static ApiContract.EntityResponse_ProductAgreement Convert(MessageEnvelop result)
        {
            if (result == null)
            {
                return null;
            }

            var productAgreement = result.Draft as StateManagment.Entity.ProductAgreement;
           
            if (productAgreement == null)
            {
                return null;
            }
            
            return new ApiContract.EntityResponse_ProductAgreement()
            {
                
            };
        }
    }
}