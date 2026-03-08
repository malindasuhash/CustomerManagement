## Customer Management

# IDEAS
- Submit all endpoint
- Abort or reset endpoint
- Get all changes endpoint (changed but not submitted)
- System data store/concept for other systems to store information without triggering orchestration.

# UNDER DEV
- Add Legal Entity
- System data concept

# PENDING

- Concept of operation data store as an event stream storage.
- Sample implementation of Legal Entity, Product and Trading Location
- Complete sample implementation of validation for Contact
- Runtime to host Contact orchestration.
- Authorisation and scopes for making certain changes.

# DONE
- Soft deletes for entities.
- Added system data objects and Legal entity (most complex one)
- Finite State Model 
- State Model itself
- Introduced new MongoDB database for contacts
- Basic set of integration tests
- Introduce concept of "touch"
- Delete endpoint - first cut.
- Audit manager embeded without actual audits records.
- Completed Contact entity
- Added Orchestration data and feedbacks to entity document.
- Queue capability to send messages back to customer manager.
- Data change event for consumers to react (for contact only).
- Integration of pub/sub.
- Incorporate CustomerId in Contact and Mongo
- GET, POST, PATCH, DELETE, SUBMIT Contact


### GET Contact response 
```
{
  "customerId": "Cus123",
  "entityId": "5522a8da-0186-47cb-b552-74df7958bd4a",
  "state": "SYNCHRONISED",
  "draft": {
    "firstName": "John",
    "lastName": "Doe",
    "telephone": null,
    "telephoneCode": null,
    "email": null,
    "altTelephone": null,
    "altTelephoneCode": null,
    "postalAddress": null,
    "descriptors": [],
    "label": null
  },
  "draftVersion": 1,
  "submitted": {
    "firstName": "John",
    "lastName": "Doe",
    "telephone": null,
    "telephoneCode": null,
    "email": null,
    "altTelephone": null,
    "altTelephoneCode": null,
    "postalAddress": null,
    "descriptors": [],
    "label": null
  },
  "submittedVersion": 1,
  "applied": {
    "firstName": "John",
    "lastName": "Doe",
    "telephone": null,
    "telephoneCode": null,
    "email": null,
    "altTelephone": null,
    "altTelephoneCode": null,
    "postalAddress": null,
    "descriptors": [],
    "label": null
  },
  "appliedVersion": 1,
  "updateUser": null,
  "updateTimestamp": "0001-01-01T00:00:00",
  "createdUser": "Tester",
  "createdTimestamp": "2026-03-05T06:03:45.648Z",
  "feedback": []
}
```