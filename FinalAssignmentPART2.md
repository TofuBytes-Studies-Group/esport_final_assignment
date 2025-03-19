# Reflection for  part 2

## Key Findings on Database Optimization

Our key findings is a slight insight into how different database optimization techniques impact performance and maintainability. 

**Denormalization** --> speeds up queries by reducing costly joins, but it introduces challenges in keeping data consistent. This requires either triggers or application-level updates to ensure accuracy.

**Partitioning** --> improves query speed by limiting the amount of scanned data, making queries more efficient. However, it comes with trade-offs, such as MySQL’s foreign key limitations to the InnoDB engine and the need for manual partition updates.

**List Partitioning** --> is useful for queries filtering on specific categories, allowing for more efficient lookups.

**Range Partitioning** --> works well for continuously growing datasets, such as dates, ensuring scalability over time.



## Key Findings on Query Optimizaion

Our key findings weren't the most exciting in this part of the exercise, the theoretical science behind doing these optimization steps makes sense but in practice in our unique scenario it proved no difference in performance with or without joins nor with the addition of indexes. 
- We think the reason being the insurmountable mass of data that might be needed to properly tests these performance differences, in our setup we started with 10 entries per table, doubled it to 20 and tripled it to 30.
- in one table even reaching 60

And these changes still proved no change in performance on our end, atleast running it directly throug the MySQL workbench. Maybe if used through an application the results could've proved different but as of now as we didn't find it as a requirement or a suggestion to do so we didn't go through with running it through an application.



## Final Thoughts

- The exercises provided enabled us to think about optimization for queries and keep normalization aswell as denormalization in mind as there can be significant performance increases by denormalizing to NF-2 or even NF-1 in some cases which we found engaging. 
- Whereas the struggle with the detection of perfomance changes if any at all during the exercises for Query optimization we in turn got to learn about tools and techniques added to our repertoire to consider for future development of future projects and work assignments.
