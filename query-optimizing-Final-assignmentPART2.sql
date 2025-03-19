-- Exercise 1
EXPLAIN SELECT order_id, total_amount,
       (SELECT name FROM Customers WHERE customer_id = Orders.customer_id) AS customer_name
FROM Orders
WHERE total_amount > 100;
-- to using join:
EXPLAIN SELECT o.order_id, o.total_amount, c.name
FROM Orders o
JOIN Customers c ON o.customer_id = c.customer_id
WHERE o.total_amount > 100;
-- Index's are not used

-- Exercise 2
explain SELECT o.order_id, o.total_amount, c.name
FROM Orders o
JOIN Customers c ON o.customer_id = c.customer_id
WHERE o.order_date > '2023-01-01';
--
CREATE INDEX customer_id_index
ON Orders ( customer_id ); -- indexes are now used when using above join.

-- Exercise 3
-- Fetch all orders first
SELECT order_id, customer_id FROM Orders WHERE order_date > '2023-01-01';

-- Then, for each order, fetch customer name separately
SELECT name FROM Customers WHERE customer_id = 1;
SELECT name FROM Customers WHERE customer_id = 2;
SELECT name FROM Customers WHERE customer_id = 3;
SELECT name FROM Customers WHERE customer_id = 4;
SELECT name FROM Customers WHERE customer_id = 5;
SELECT name FROM Customers WHERE customer_id = 6;
SELECT name FROM Customers WHERE customer_id = 7;
SELECT name FROM Customers WHERE customer_id = 8;
SELECT name FROM Customers WHERE customer_id = 9;
SELECT name FROM Customers WHERE customer_id = 10;
SELECT name FROM Customers WHERE customer_id = 11;
SELECT name FROM Customers WHERE customer_id = 12;
SELECT name FROM Customers WHERE customer_id = 13;
SELECT name FROM Customers WHERE customer_id = 14;
SELECT name FROM Customers WHERE customer_id = 15;
SELECT name FROM Customers WHERE customer_id = 16;
SELECT name FROM Customers WHERE customer_id = 17;
SELECT name FROM Customers WHERE customer_id = 18;
SELECT name FROM Customers WHERE customer_id = 19;
SELECT name FROM Customers WHERE customer_id = 20;
-- AS JOIN
SELECT 
    o.order_id, 
    o.customer_id, 
    c.name
FROM Orders AS o
	JOIN Customers AS c 
    ON o.customer_id = c.customer_id
WHERE o.order_date > '2023-01-01';

-- Exercise 4
CREATE TABLE Products (
    product_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    category VARCHAR(50) NOT NULL,
    price DECIMAL(10,2) NOT NULL
);
CREATE TABLE Sales (
    sale_id INT PRIMARY KEY AUTO_INCREMENT,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    sale_date DATE NOT NULL,
    total_amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);
SELECT p.category, SUM(s.total_amount) AS total_sales
FROM Sales s
JOIN Products p ON s.product_id = p.product_id
GROUP BY p.category;

SELECT p.category, SUM(s.total_amount) AS total_sales
FROM Sales s
JOIN Products p ON s.product_id = p.product_id
GROUP BY p.category;
--  Analyze Query Performance Use EXPLAIN to check execution plan:
EXPLAIN SELECT p.category, SUM(s.total_amount) AS total_sales
FROM Sales s
JOIN Products p ON s.product_id = p.product_id
GROUP BY p.category;
-- 21 rows scanned and no indexes used 
SHOW INDEXES FROM Sales;
CREATE INDEX idx_product_id ON Sales(product_id);
CREATE INDEX idx_category ON Products(category);
CREATE INDEX idx_sales_optimized ON Sales(product_id, total_amount);
-- Perfomance is near impossible to test for unless data used is significantly larger than our setup.
-- Out setup consists of 20 units in each table and that still wasn't enough to measure any significant change.