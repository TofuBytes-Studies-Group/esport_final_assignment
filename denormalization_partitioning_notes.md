Exercise 1: Denormalizing Total Sales per Order 

What are the performance benefits of this approach?
fast read time 

How should we ensure the total_amount stays accurate when an order is updated?

it can be done in more ways you cloud have a trigger that update the total_amount when an order_details is created or updated. 

but it could also be handled on application level.

Exercise 2: Denormalizing Customer Data in Orders

Discuss the downsides of this approach.
how updates need to be handled because data could be inconsistent across the two tables. 

When would this denormalization be useful?
when there are many costly join queries and when read performance is a priority, simplifying queries and reducing the need for complex joins. 


How should updates to Customers be handled in this case?
All the orders that contains the old customer_name and customer_email needs to be updated as well as the customer table itself.

Exercise 3: Using Partitioning for Sales Data

How does partitioning improve query speed?
Partitioning improves performance by reducing the amount of data that the database has to scan, sort, join, or aggregate for each query. As well as allows for improve scalability, reduce contention, and optimize performance

Why does MySQL not allow foreign keys in partitioned tables?
because the mysql storage engine innodb does not support it.

What happens when a new year starts?
At the start of the new year you would have too update your partition. 
but this can not be done directly be can be done with some work around

Exercise 4: Using List Partitioning for Regional Data

What types of queries does list partitioning optimize?
List partitioning optimizes queries that filter on specific discrete values in the partition key, such as:

- Equality filters (=) → Only relevant partitions are scanned.
- IN queries → Limits scanning to specified partitions.
- Joins on the partition key → Improves partition pruning.
- Aggregations on the partitioned column → Enables efficient partition-wise processing.

What if a new region needs to be added?
adding a new region can not be done directly be can be done with some work around. like dropping and remaking the partition.

How does list partitioning compare to range partitioning?

Use List Partitioning if:

Data is grouped by specific categories (e.g., region = 'US').
Queries often filter by exact matches (= or IN).
Categories do not change frequently.

Use Range Partitioning if:

Data is naturally ordered (e.g., date, age).
Queries use range filters (BETWEEN, <, >).
Newer data grows over time, and partitions need to be extended.
