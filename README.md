## Customer Management

# IDEAS
- Abort or reset endpoint
- System data store/concept for other systems to store information without triggering orchestration.

# UNDER DEV
- Complete sample implementation of validation for Contact

# PENDING
- Concept of operation data store as an event stream storage.
- Runtime to host Contact orchestration.
- Authorisation and scopes for making certain changes.

# DONE
- Submit all endpoint
- Get all draft changes endpoint (changed but not submitted)
- Base implementation of change detection and mapping to management case.
- Implemented Trading Location API
- Adding some tests to controller to increase confidence.
- Refactored to use generics
- Product agreement and APIs 
- Added Bank account and APIs
- Added billing group and APIs
- Added Legal Entity and APIs
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
