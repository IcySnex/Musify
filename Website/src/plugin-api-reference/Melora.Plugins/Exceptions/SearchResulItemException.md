﻿# SearchResulItemException
Represents errors that occurr when an requested item of a search result is invalid in\.
- **Type:** Class
- **Namespace:** [Melora.Plugins.Exceptions](/Melora/plugin-api-reference/Melora.Plugins/Exceptions/)
- **Implements:**  [Exception](https://learn.microsoft.com/dotnet/api/system.exception), [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)
```cs
public class SearchResulItemException : Exception, ISerializable
```


## Constructors
Creates a new instance of SearchResulItemException
```cs
public SearchResulItemException(
  string requestedItem, 
  SearchResult searchResult, 
  Exception innerException)
```
| Parameter | Summary |
| --------- | ------- |
| `string` requestedItem | The name of the rquested search result item. |
| `SearchResult` searchResult | The search result from which the item was requested. |
| `Exception` innerException | The inner exception. |





## Properties

### RequestedItem
The name of the rquested search result item\.
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### SearchResult
The search result from which the item was requested\.
- **Type:** [Melora.Plugins.Models.SearchResult](/Melora/plugin-api-reference/Melora.Plugins/Models/SearchResult.html)
- **Is Read Only:** `True`

### TargetSite
- **Type:** [System.Reflection.MethodBase](https://learn.microsoft.com/dotnet/api/system.reflection.methodbase)
- **Is Read Only:** `True`

### Message
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`

### Data
- **Type:** [System.Collections.IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.idictionary)
- **Is Read Only:** `True`

### InnerException
- **Type:** [System.Exception](https://learn.microsoft.com/dotnet/api/system.exception)
- **Is Read Only:** `True`

### HelpLink
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### Source
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `False`

### HResult
- **Type:** [System.Int32](https://learn.microsoft.com/dotnet/api/system.int)
- **Is Read Only:** `False`

### StackTrace
- **Type:** [System.String](https://learn.microsoft.com/dotnet/api/system.string)
- **Is Read Only:** `True`