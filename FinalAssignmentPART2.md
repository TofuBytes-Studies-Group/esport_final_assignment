# Reflection for  part 2

## Key Findings on Database Optimization

Our key findings is a slight insight into how different database optimization techniques impact performance and maintainability. 

**Denormalization** --> speeds up queries by reducing costly joins, but it introduces challenges in keeping data consistent. This requires either triggers or application-level updates to ensure accuracy.

**Partitioning** --> improves query speed by limiting the amount of scanned data, making queries more efficient. However, it comes with trade-offs, such as MySQL’s foreign key limitations to the InnoDB engine and the need for manual partition updates.

**List Partitioning** --> is useful for queries filtering on specific categories, allowing for more efficient lookups.

**Range Partitioning** --> works well for continuously growing datasets, such as dates, ensuring scalability over time.
