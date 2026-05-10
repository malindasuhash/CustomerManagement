using StateManagment.Models;

namespace Api.ApiModels
{
    public class ChangeLink
    {
        const string ApiVersion = "1";

        private static readonly Dictionary<EntityName, (string Controller, string Action)> SubmitActions = new()
        {
            [EntityName.BankAccount] = ("BankAccount", "SubmitBankAccount"),
            [EntityName.Contact] = ("Contact", "SubmitContact"),
            [EntityName.LegalEntity] = ("LegalEntity", "SubmitLegalEntity"),
            [EntityName.ProductAgreement] = ("ProductAgreements", "SubmitProductAgreement"),
            [EntityName.TradingLocation] = ("TradingLocation", "SubmitTradingLocation"),
            [EntityName.BillingGroup] = ("BillingGroup", "SubmitBillingGroup"),
        };

        public EntityName Name { get; set; }
        public string EntityId { get; set; }
        public EntityState State { get; set; }
        public decimal DraftVersion { get; set; }
        public decimal SubmittedVersion { get; set; }
        public string? Link { get; set; }

        public static ChangeLink Create(EntityBasics entityBasics, LinkGenerator linkGenerator, string customerId, string legalEntityId)
        {
            return new ChangeLink()
            {
                Name = entityBasics.Name,
                EntityId = entityBasics.EntityId,
                State = entityBasics.State,
                DraftVersion = entityBasics.DraftVersion,
                SubmittedVersion = entityBasics.SubmittedVersion,
                Link = BuildSubmitLink(entityBasics.Name, customerId, entityBasics.EntityId, linkGenerator, legalEntityId)
            };
        }

        private static string? BuildSubmitLink(EntityName entityName, string customerId, string entityId, LinkGenerator linkGenerator, string? legalEntityId = null)
        {
            if (!SubmitActions.TryGetValue(entityName, out var target))
                return null;

            var routeValues = legalEntityId is not null   // There is a special case of LegalEntity
                ? GetRouteValues(entityName, customerId, entityId, legalEntityId)
                : GetRouteValues(entityName, customerId, entityId, entityName == EntityName.LegalEntity ? entityId : legalEntityId);

            return linkGenerator.GetPathByAction(
                action: target.Action,
                controller: target.Controller,
                values: routeValues
            );
        }

        private static object GetRouteValues(EntityName entityName, string customerId, string entityId, string? legalEntityId)
        {
            switch (entityName)
            {
                case EntityName.BankAccount:
                    return new { version = ApiVersion, customerId, legalEntityId, bankAccountId = entityId };
                case EntityName.Contact:
                    return new { version = ApiVersion, customerId, contactId = entityId };
                case EntityName.LegalEntity:
                    return new { version = ApiVersion, customerId, entityId = entityId };
                case EntityName.ProductAgreement:
                    return new { version = ApiVersion, customerId, legalEntityId, productAgreementId = entityId };
                case EntityName.TradingLocation:
                    return new { version = ApiVersion, customerId, legalEntityId, tradingLocationId = entityId };
                case EntityName.BillingGroup:
                    return new { version = ApiVersion, customerId, billingGroupId = entityId };
                default:
                    return new { version = ApiVersion, customerId, legalEntityId, entityId };
            }
        }
    }
}
