// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// <using_directives> 
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
// </using_directives>

// <endpoint_key> 
// New instance of CosmosClient class using an endpoint and key string
using CosmosClient client = new(
    accountEndpoint: "https://az204demo412.documents.azure.com:443/",
    authKeyOrResourceToken: "7p6f1Z9gF8PQ9Zz5GQk4poKszgv0U4nj1LlwA3IWlsuhXjXKPLDnCzjsaMZynIIbshm3dgvTzO5CACDbJmpCeQ=="
);
// </endpoint_key>

// <create_database>
// New instance of Database class referencing the server-side database
Database database = await client.CreateDatabaseIfNotExistsAsync(
    id: "adventureworks"
);
// </create_database>

// <create_container>
// New instance of Container class referencing the server-side container
Container container = await database.CreateContainerIfNotExistsAsync(
    id: "products",
    partitionKeyPath: "/category",
    throughput: 400
);
// </create_container>

// <create_items> 
// Create new items and add to container
Product firstNewItem = new(
    id: "68719518389",
    category: "gear-surf-surfboards",
    name: "Sunnox Surfboard",
    quantity: 8,
    sale: true
);

Product secondNewitem = new(
    id: "68719518399",
    category: "gear-surf-surfboards",
    name: "Noosa Surfboard",
    quantity: 15,
    sale: false
);

await container.CreateItemAsync<Product>(
    item: firstNewItem,
    partitionKey: new PartitionKey("gear-surf-surfboards")
);

await container.CreateItemAsync<Product>(
    item: secondNewitem,
    partitionKey: new PartitionKey("gear-surf-surfboards")
);
// </create_items> 

// <query_items_sql>
// Query multiple items from container
using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(
    queryText: "SELECT * FROM products"
);

// Iterate query result pages
while (feed.HasMoreResults)
{
    FeedResponse<Product> response = await feed.ReadNextAsync();

    // Iterate query results
    foreach (Product item in response)
    {
        Console.WriteLine($"Found item:\t{item.name}");
    }
}
// </query_items_sql>

// <query_items_sql_parameters>
// Build query definition
var parameterizedQuery = new QueryDefinition(
    query: "SELECT * FROM products p WHERE p.category = @partitionKey"
)
    .WithParameter("@partitionKey", "gear-surf-surfboards");

// Query multiple items from container
using FeedIterator<Product> filteredFeed = container.GetItemQueryIterator<Product>(
    queryDefinition: parameterizedQuery
);

// Iterate query result pages
while (filteredFeed.HasMoreResults)
{
    FeedResponse<Product> response = await filteredFeed.ReadNextAsync();

    // Iterate query results
    foreach (Product item in response)
    {
        Console.WriteLine($"Found item:\t{item.name}");
    }
}
// </query_items_sql_parameters>

// <query_items_queryable>
// Get LINQ IQueryable object
IOrderedQueryable<Product> queryable = container.GetItemLinqQueryable<Product>();

// Construct LINQ query
var matches = queryable
    .Where(p => p.category == "gear-surf-surfboards")
    .Where(p => p.sale == false)
    .Where(p => p.quantity > 10);

// Convert to feed iterator
using FeedIterator<Product> linqFeed = matches.ToFeedIterator();

// Iterate query result pages
while (linqFeed.HasMoreResults)
{
    FeedResponse<Product> response = await linqFeed.ReadNextAsync();

    // Iterate query results
    foreach (Product item in response)
    {
        Console.WriteLine($"Matched item:\t{item.name}");
    }
}
// </query_items_queryable>