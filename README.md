# Neo4jClient.Ndp

The beginnings of an NDP driver for Neo4j

## Information

Packing information and general start up all from https://github.com/nigelsmall/ndp-howto

## Hello World

This uses the default 'Movie' database you can instantiate on the Neo4j database.

With the class:

```C#
public class Movie 
{
    public string tagline { get; set; }
    public string title { get; set; }
    public int released { get; set; }
}
```

Then in your code:

```C#
var client = new Neo4jNdpClient(new Uri("http://localhost.:7687"));
client.Connect();

var results = client.ExecuteCypher("MATCH (n:Movie) WHERE n.released = 2012 RETURN n");
foreach(var record in results)
{
    var nodes = record.As<Movie>();
    foreach (var node in nodes)
        Console.WriteLine("{0} ({1}) -- '{2}'", node.Data.title, node.Data.released, node.Data.tagline);
}
```

### Output

```
Cloud Atlas (2012) -- 'Everything is connected'
```