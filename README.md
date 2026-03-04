## Customer Management

# IDEAS
- Submit all endpoint
- Abort or reset endpoint
- Get all changes endpoint (changed but not submitted)
- System data store/concept for other systems to store information without triggering orchestration.

# UNDER DEV
- Add Legal Entity
- Incorporate CustomerId in Contact
- JSON API for Contact

# PENDING

- Concept of operation data store as an event stream storage.
- Sample implementation of Legal Entity, Product and Trading Location
- Complete sample implementation of validation for Contact
- Runtime to host Contact orchestration.
- Authorisation and scopes for making certain changes.

# DONE
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