# FB.Cache Documentatiion
In memory cache implementation that follows approach of "last recently used" eviction mechanism

## Key Components
class FBGenericCache that implements IGenericCache, makes use of ConcurrentDictionary to achieve Thread safety and 
LinkedList to evict the least recently used cache item

## Projects
* FB.CacheLib
* FB.CacheLib.Tests

## Class Diagram
<img src="./ClassDiagramUpdated.jpg">


## Scope for improvements

* Further improvements to be done  
	* To Implement all the methods in the IGenericCache methods (ran out of time)
	* To support async methods to fetch or add items to cache
