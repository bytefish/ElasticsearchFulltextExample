# Elasticsearch Fulltext Search Example #

Every project grows to a point it needs to support a Fulltext Search. In this repository I will explore how to use Elasticsearch to index files, provide a Fulltext Search on file contents and provide useful auto-completes. Please regard this repository as a work in progress and it is not finished work in any way.

## What we are going to build ##

### Document Indexing ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Index_Document_Dialog.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Index_Document_Dialog.jpg" alt="Document Indexing Dialog" width="50%" />
</a>

### Auto-Complete Search Box ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Search_Box_Auto_Complete.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Search_Box_Auto_Complete.jpg" alt="Search Box with Auto Complete" width="50%" />
</a>

### Full-Text Search Results ###

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Search_Results.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchFulltextExample/master/Screenshots/Search_Results.jpg" alt="Search Box with Auto Complete" width="50%" />
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
