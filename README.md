
# Fetcher

Sample application, periodically collecting data from external REST API, stroe payload and logs.

Architecure of solution is minimal, as functionality not required yet any stronger layer division. 
Azure Functions should be simple and quick.

## Diagrams

Application has three functions:

### Fetching Data

This component is responsible for download data from external API. It is time scheduled, once per minute.
After calling REST API endpoint, log is creted. If it is success call, then reposnse payload is stored in BLOB file.

![Component Fetch Data](https://github.com/pszabaciuk/fetcher/blob/main/doc/Component%20Fetch%20Data.jpg?raw=true)

### Retrieving Logs

This component is responsible to return all logs data for user.
User needs to prodive start and end date of required time period.
Result is returned as JSON containg all available data.

![Component Log Data](https://github.com/pszabaciuk/fetcher/blob/main/doc/Component%20Get%20Logs%20Data.jpg?raw=true)

### Retrieving Payload

This component is responsible to return payload of given log user requested.
User needs to prodive GUID of logs paylod.
Result is returned as string containg payload data.

![Component Payload Data](https://github.com/pszabaciuk/fetcher/blob/main/doc/Component%20Get%20Payload%20Data.jpg?raw=true)