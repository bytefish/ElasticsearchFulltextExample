# Elasticsearch Fulltext Search Example #

Every project grows to a point it needs to support a Fulltext Search. In this repository 
I will explore how to use the Elasticsearch Fulltext Search capabilities to index and 
search through academic papers.

It is a work in progress and not finished in any way.

## Queries ##

Here are some queries for the API.

### Index a Paper by DOI ###

```batch
curl -X PUT -d "Content-Length: 0" http://localhost:9000/api/index?doi=10.1016/j.adro.2019.05.007
```

## License ##

All code is released under terms of the [MIT License].

[MIT License]: https://opensource.org/licenses/MIT