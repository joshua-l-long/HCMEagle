# Technical Requirements

## Tech Stack

* .NET Core 8
* Angular/Material UI
* CSS
* HTML
* JavaScript
* JSON
* PostgreSQL
* Docker
* Kubernetes

## Database Rules

* No hard deletes
* Database will exist in 4NF
* Primary keys are `int`
* All tables will follow the same basic structure
```sql
id						int primary key not null,
loginname				varchar(20) not null,
//...other details about the person
recordstatusid			int not null,			// Foreign key to recordstatuses.id
createdby				int not null,			// Foreign key to users.id
createddate				timestamp not null,
createdip				varchar(16) not null,
updatedby				int not null,			// Foreign key to users.id
updateddate				timestamp not null,
updatedip				varchar(16) not null,
deletedby				int not null,			// Foreign key to users.id
deleteddate				timestamp not null,
deletedip				varchar(16) not null,
archivedby				int not null,			// Foreign key to users.id
archiveddate			timestamp not null,
archivedip				varchar(16) not null
```
* On insert, `updated` fields are set to the values of the `created` fields
* On record status change to `deleted`, `deleted` and `updated` fields are updated to the same values
* On record status change to `archived`, `archived` and `updated` fields are updated to the same values
* On record status change *away* from `deleted` or `archived`, the corresponding fields are cleared, but `updated` is updated with new values
* **Self-referencing tables** are expected and should be used to designate hierarchy; examples:
  	* Physical office locations (buildings, floors, rooms)
  	* Geography (cities, states, countries)
  	* Organizational hierarchies (agencies, components, divisions, branches, offices)
 
## Application Architecture

* Application may not hard code rules/logic into application code
* Application code is expected to implement the rules that exist in configuration
	* Configuration may be in JSON, XML, or Database
 	* Changes to rules must be achievable on-the-fly, but may not be implemented without code review and proper release management

## DevSecOps Pipeline

**TO BE DEVELOPED BY RAY HILL**
