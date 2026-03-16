using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StateManagment.Entity;
using StateManagment.Models;

namespace Infrastructure.EntityConfig
{
    internal static class DatabaseCollectionConfig
    {
        static DatabaseCollectionConfig()
        {
            BsonClassMap.RegisterClassMap<Contact>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<LegalEntity>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<PersonWithControl>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<LegalEntityWithControl>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<RegisteredAddress>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<BillingGroup>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<BankAccount>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<ProductAgreement>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<ProductConfiguration>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<ProductFeature>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<MessageEnvelop>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(a => a.EntityId);
                cm.MapMember(a => a.State).SetSerializer(new EnumSerializer<EntityState>(BsonType.String));
                cm.MapMember(a => a.Name).SetDefaultValue(EntityName.Contact);
                cm.MapMember(a => a.Change).SetDefaultValue(ChangeType.Read);
                cm.UnmapMember(c => c.Change);
                cm.UnmapMember(c => c.Name);
                cm.UnmapMember(c => c.IsSubmitted);
            });

            BsonClassMap.RegisterClassMap<Feedback>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(a => a.Type).SetSerializer(new EnumSerializer<FeedbackType>(BsonType.String));
            });

            BsonClassMap.RegisterClassMap<OrchestrationData>(cm =>
            {
                cm.AutoMap();
            });
        }

        public static async Task<DbEexecutionParams> Patch<T>(MessageEnvelop messageEnvelop, int latestDraftVersion, IMongoDatabase db, string updatedUser = "SYSTEM") where T : IEntity
        {
            var stored = await GetById(messageEnvelop.EntityId, messageEnvelop.Name, db);

            var receivedEntity = (T)messageEnvelop.Draft;
            var storedEntity = (T)stored.Draft;

            // This is not quite right. 
            // I need to figure out a way to do this more elegantly.
            // I'm reading the entire object and applying changes and saving it.
            // Perhaps I can apply changes selectively instead.
            if (messageEnvelop.Name == EntityName.Contact && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedContact = receivedEntity as Contact;
                var storedContact = storedEntity as Contact;

                if (receivedContact.FirstName != null)
                {
                    storedContact.FirstName = receivedContact.FirstName;
                }

                if (receivedContact.LastName != null)
                {
                    storedContact.LastName = receivedContact.LastName;
                }
            }
            if (messageEnvelop.Name == EntityName.LegalEntity && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedLegalEntity = receivedEntity as LegalEntity;
                var storedLegalEntity = storedEntity as LegalEntity;

                if (receivedLegalEntity.TradingName != null)
                {
                    storedLegalEntity.TradingName = receivedLegalEntity.TradingName;
                }

                if (receivedLegalEntity.BusinessEmail != null)
                {
                    storedLegalEntity.BusinessEmail = receivedLegalEntity.BusinessEmail;
                }
            }
            if (messageEnvelop.Name == EntityName.BillingGroup && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedBillingGroup = receivedEntity as BillingGroup;
                var storedBillingGroup = storedEntity as BillingGroup;

                if (receivedBillingGroup.Name != null)
                {
                    storedBillingGroup.Name = receivedBillingGroup.Name;
                }

                if (receivedBillingGroup.Description != null)
                {
                    storedBillingGroup.Description = receivedBillingGroup.Description;
                }
            }

            if (messageEnvelop.Name == EntityName.BankAccount && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedBankAccount = receivedEntity as BankAccount;
                var storedBankAccount = storedEntity as BankAccount;

                if (receivedBankAccount.Name != null)
                {
                    storedBankAccount.Name = receivedBankAccount.Name;
                }

                if (receivedBankAccount.Iban != null)
                {
                    storedBankAccount.Iban = receivedBankAccount.Iban;
                }
            }

            if (messageEnvelop.Name == EntityName.ProductAgreement && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedProductAgreement = receivedEntity as ProductAgreement;
                var storedProductAgreement = storedEntity as ProductAgreement;

                if (receivedProductAgreement.DisplayName != null)
                {
                    storedProductAgreement.DisplayName = receivedProductAgreement.DisplayName;
                }

                if (receivedProductAgreement.RateCardId != null)
                {
                    storedProductAgreement.RateCardId = receivedProductAgreement.RateCardId;
                }
            }

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, messageEnvelop.EntityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.DraftVersion, latestDraftVersion)
            .Set(a => a.Draft, storedEntity);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(messageEnvelop.Name));

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> UpdateData(EntityName entityName, string entityId, EntityState entityState, IMongoDatabase db, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string updatedUser = "SYSTEM")
        {
            var contact = await GetById(entityId, entityName, db);

            // set properties
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.State, entityState)
            .Set(b => b.Feedback, feedbacks)
            .Set(b => b.OrchestrationData, orchestrationData)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> AddToSubmitted<T>(IEntity entity, string entityId, string updatedUser, EntityName entityName, IMongoDatabase db) where T : IEntity
        {
            // Read the entity document - I need the latest document here.
            var contact = await GetById(entityId, entityName, db);

            // set properties
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.SubmittedVersion, contact.DraftVersion)
            .Set(a => a.Submitted, (T)contact.Draft)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter
            };
        }

        public static async Task<DbEexecutionParams> AddToApplied(string entityId, IEntity entity, bool confirmRemoval, IMongoDatabase db, EntityName entityName, string updatedUser = "SYSTEM")
        {
            var contact = await GetById(entityId, entityName, db);

            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var onUpdate = Builders<MessageEnvelop>.Update;
            var definition = onUpdate.Set(a => a.AppliedVersion, contact.SubmittedVersion)
            .Set(a => a.Applied, (Contact)entity)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.Removed, confirmRemoval);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = definition,
                Filter = filter
            };
        }

        public static Task<DbEexecutionParams> AddToDraft<T>(MessageEnvelop messageEnvelop, int incrementalDraftVersion, IMongoDatabase db) where T : IEntity
        {
            messageEnvelop.EntityId = Guid.NewGuid().ToString();
            messageEnvelop.DraftVersion = incrementalDraftVersion;

            var filter = Builders<MessageEnvelop>.Filter;
            var filterDefs = new List<FilterDefinition<MessageEnvelop>>
            {
                Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, messageEnvelop.EntityId),
                Builders<MessageEnvelop>.Filter.Eq(o => o.CustomerId, messageEnvelop.CustomerId)
            };

            if (messageEnvelop.Draft is ILegalEntityAttached withLegalEntity)
            {
                filterDefs.Add(new BsonDocument("Draft.LegalEntityId", withLegalEntity.LegalEntityId));
            }

            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.DraftVersion, messageEnvelop.DraftVersion)
            .Set(a => a.SubmittedVersion, messageEnvelop.SubmittedVersion)
            .Set(a => a.AppliedVersion, messageEnvelop.AppliedVersion)
            .Set(a => a.Draft, (T)messageEnvelop.Draft)
            .Set(a => a.State, messageEnvelop.State)
            .Set(a => a.CustomerId, messageEnvelop.CustomerId)
            .Set(a => a.RemoveRequested, false)
            .Set(a => a.Removed, false)
            .Set(a => a.SystemData, messageEnvelop.SystemData)
            .SetOnInsert(a => a.CreatedTimestamp, DateTime.UtcNow)
            .SetOnInsert(a => a.CreatedUser, messageEnvelop.CreatedUser)
            .SetOnInsert(a => a.EntityId, messageEnvelop.EntityId);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(messageEnvelop.Name));

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter.And(filterDefs)
            });
        }

        public static Task<DbEexecutionParams> SetMarkForRemoval(string entityId, EntityName entityName, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);
            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.RemoveRequested, true);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter
            });
        }

        public static Task<MessageEnvelop> GetById(string entityId, EntityName entityName, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var entities = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return entities.Find(filter).FirstOrDefaultAsync();
        }

        public static Task<EntityBasics> GetEntityBasics(string entityId, EntityName entityName, IMongoDatabase db)
        {
            var filter = Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, entityId);

            var contacts = db.GetCollection<MessageEnvelop>(EntityNameToCollectionName.GetCollectionName(entityName));

            return contacts.Find(filter)
                .Project(p => new EntityBasics
                {
                    DraftVersion = p.DraftVersion,
                    EntityId = entityId,
                    Name = entityName,
                    State = p.State,
                    SubmittedVersion = p.SubmittedVersion,
                }).FirstOrDefaultAsync();
        }

        public static void CreateDataChangedEvent(MessageEnvelop messageEnvelop)
        {

        }
    }
}
