# Elasticsearch Fulltext Search Example #

Every project grows to a point it needs to support a Fulltext Search. In this repository I will explore how to use Elasticsearch to index files, provide a Fulltext Search on file contents and provide useful auto-completes. Please regard this repository as a work in progress and it is not finished work in any way.

The Article for this repository can be found at:

* [https://bytefish.de/blog/elasticsearch_fulltext_search/](https://bytefish.de/blog/elasticsearch_fulltext_search/)

## What we are going to build ##

### Auto-Complete Search Box ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_Auto_Completion.png">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_Auto_Completion.png" alt="Search Box with Auto Complete" width="50%" />
</a>

### Full-Text Search Results ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_Search_Results.png">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_Search_Results.png" alt="Search Box with Auto Complete" width="50%" />
</a>

### Document Indexing ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_AddDocument.png">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_AddDocument.png" alt="Document Indexing Dialog" width="50%" />
</a>

### Document Status Tracking ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_DocumentStatus.png">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Frontend_DocumentStatus.png" alt="Document Indexing Dialog" width="50%" />
</a>

### Create Migrations ###

Install the package ``Microsoft.EntityFrameworkCore.Tools``:

```
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
```

Then you can add migrations for your database like this:

```
PM> add-migration InitialCreate -Context ApplicationDbContext -OutputDir "Database/Migrations" 
```


## License ##

All code is released under terms of the [MIT License].

[MIT License]: https://opensource.org/licenses/MIT
