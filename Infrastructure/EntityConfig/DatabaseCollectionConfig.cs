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
        private static bool _initialized = false;

        static DatabaseCollectionConfig()
        {
            Run();
        }
        public static void Run()
        {
            if (_initialized) return;
            _initialized = true;

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

            BsonClassMap.RegisterClassMap<TradingLocation>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<ContactReference>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(a => a.ContactType).SetSerializer(new EnumSerializer<ContactType>(BsonType.String));
            });

            BsonClassMap.RegisterClassMap<MessageEnvelop>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(a => a.EntityId);
                cm.MapMember(a => a.State).SetSerializer(new EnumSerializer<EntityState>(BsonType.String));
                cm.MapMember(a => a.Name).SetDefaultValue(EntityName.None);
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
            var predicate = messageEnvelop.SearchBy();
            var stored = await FindBy<T>(predicate, db);

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

            if (messageEnvelop.Name == EntityName.TradingLocation && messageEnvelop.Change == ChangeType.Update)
            {
                var receivedTradingLocation = receivedEntity as TradingLocation;
                var storedTradingLocation = storedEntity as TradingLocation;

                if (receivedTradingLocation.Name != null)
                {
                    storedTradingLocation.Name = receivedTradingLocation.Name;
                }

                if (receivedTradingLocation.Website != null)
                {
                    storedTradingLocation.Website = receivedTradingLocation.Website;
                }
            }

            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.DraftVersion, latestDraftVersion)
            .Set(a => a.Draft, storedEntity);

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter.And(filterDefs)
            };
        }

        public static async Task<DbEexecutionParams> UpdateData<T>(LookupPredicate predicate, EntityState entityState, IMongoDatabase db, Feedback[] feedbacks, OrchestrationData[] orchestrationData, string updatedUser = "SYSTEM") where T : IEntity
        {
            var contact = await FindBy<T>(predicate, db);

            // set properties
            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);
            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.State, entityState)
            .Set(b => b.Feedback, feedbacks)
            .Set(b => b.OrchestrationData, orchestrationData)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter.And(filterDefs)
            };
        }

        public static async Task<DbEexecutionParams> AddToSubmitted<T>(LookupPredicate predicate, string updatedUser, IMongoDatabase db) where T : IEntity
        {
            var storedEntityDocument = await FindBy<T>(predicate, db);

            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);

            var onUpdate = Builders<MessageEnvelop>.Update
            .Set(a => a.SubmittedVersion, storedEntityDocument.DraftVersion)
            .Set(a => a.Submitted, (T)storedEntityDocument.Draft)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser);

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onUpdate,
                Filter = filter.And(filterDefs)
            };
        }

        public static async Task<DbEexecutionParams> AddToApplied<T>(LookupPredicate predicate, bool confirmRemoval, IMongoDatabase db, string updatedUser = "SYSTEM") where T : IEntity
        {
            var contact = await FindBy<T>(predicate, db);

            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);

            var onUpdate = Builders<MessageEnvelop>.Update;
            var definition = onUpdate.Set(a => a.AppliedVersion, contact.SubmittedVersion)
            .Set(a => a.Applied, (T)contact.Submitted)
            .Set(a => a.UpdateTimestamp, DateTime.UtcNow)
            .Set(a => a.UpdateUser, updatedUser)
            .Set(a => a.Removed, confirmRemoval);

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return new DbEexecutionParams
            {
                Collection = contacts,
                Definition = definition,
                Filter = filter.And(filterDefs)
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

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter.And(filterDefs)
            });
        }

        public static Task<DbEexecutionParams> SetMarkForRemoval<T>(LookupPredicate predicate, IMongoDatabase db) where T : IEntity
        {
            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);

            var onInsert = Builders<MessageEnvelop>.Update
            .Set(a => a.RemoveRequested, true);

            var contacts = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return Task.FromResult(new DbEexecutionParams
            {
                Collection = contacts,
                Definition = onInsert,
                Filter = filter.And(filterDefs)
            });
        }

        public static Task<MessageEnvelop> FindBy<T>(LookupPredicate predicate, IMongoDatabase db) where T : IEntity
        {
            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);

            var entities = db.GetCollection<MessageEnvelop>(EntityCollectionConfig.Config<T>().Collection);

            return entities.Find(filter.And(filterDefs)).FirstOrDefaultAsync();
        }

        public static Task<EntityBasics> GetEntityBasics<T>(LookupPredicate predicate, IMongoDatabase db) where T : IEntity
        {
            BuildFilters(predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs);

            var config = EntityCollectionConfig.Config<T>();
            var contacts = db.GetCollection<MessageEnvelop>(config.Collection);

            return contacts.Find(filter.And(filterDefs))
                .Project(p => new EntityBasics
                {
                    DraftVersion = p.DraftVersion,
                    EntityId = predicate.EntityId,
                    Name = config.Name,
                    State = p.State,
                    SubmittedVersion = p.SubmittedVersion,
                }).FirstOrDefaultAsync();
        }

        private static void BuildFilters(LookupPredicate predicate, out FilterDefinitionBuilder<MessageEnvelop> filter, out List<FilterDefinition<MessageEnvelop>> filterDefs)
        {
            filter = Builders<MessageEnvelop>.Filter;
            filterDefs =
            [
                Builders<MessageEnvelop>.Filter.Eq(o => o.EntityId, predicate.EntityId),
                Builders<MessageEnvelop>.Filter.Eq(o => o.CustomerId, predicate.CustomerId)
            ];
            if (predicate.LegalEntityId != null)
            {
                filterDefs.Add(new BsonDocument("Draft.LegalEntityId", predicate.LegalEntityId));
            }
        }

        public static void CreateDataChangedEvent(MessageEnvelop messageEnvelop)
        {

        }
    }
}
