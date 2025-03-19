# **Performance Analysis Report: Optimistic vs. Pessimistic Concurrency Control**

## **📝 Student Names: Isak Møgelvang, Jamie Callan og Helena Lykstoft**

---

## **📌 Introduction**
### **Objective:**
This report analyzes and compares the performance of **Optimistic Concurrency Control (OCC) vs. Pessimistic Concurrency Control (PCC)** when handling concurrent transactions in an Esports Tournament database.

### **Scenario Overview:**
- **OCC is tested** by simulating multiple players registering for the same tournament concurrently.
- **PCC is tested** by simulating multiple administrators updating the same match result simultaneously.

---

## **📌 Experiment Setup**
### **Database Schema Used:**
```sql
CREATE TABLE Players (
    player_id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    ranking INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Tournaments (
    tournament_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    game VARCHAR(50) NOT NULL,
    max_players INT NOT NULL,
    start_date DATETIME NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Tournament_Registrations (
    registration_id INT PRIMARY KEY AUTO_INCREMENT,
    tournament_id INT NOT NULL,
    player_id INT NOT NULL,
    registered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (tournament_id) REFERENCES Tournaments(tournament_id) ON DELETE CASCADE,
    FOREIGN KEY (player_id) REFERENCES Players(player_id) ON DELETE CASCADE
);

CREATE TABLE Matches (
    match_id INT PRIMARY KEY AUTO_INCREMENT,
    tournament_id INT NOT NULL,
    player1_id INT NOT NULL,
    player2_id INT NOT NULL,
    winner_id INT NULL,
    match_date DATETIME NOT NULL,
    FOREIGN KEY (tournament_id) REFERENCES Tournaments(tournament_id) ON DELETE CASCADE,
    FOREIGN KEY (player1_id) REFERENCES Players(player_id) ON DELETE CASCADE,
    FOREIGN KEY (player2_id) REFERENCES Players(player_id) ON DELETE CASCADE,
    FOREIGN KEY (winner_id) REFERENCES Players(player_id) ON DELETE SET NULL
);
```

### **Concurrency Control Techniques Implemented:**
- **Optimistic Concurrency Control (OCC)** using a **version column** in the `Tournaments` table.
- **Pessimistic Concurrency Control (PCC)** using `SELECT ... FOR UPDATE` when updating `Matches`.

### **Test Parameters:**
| Parameter        | Value |
|-----------------|-------|
| **Number of concurrent transactions** | 2 |
| **Database** | MySql |
| **Execution Environment** | .net C# |
| **Java Thread Pool Size** | 5 and 10 |

---

## **📌 Results & Observations**

### **1️⃣ Optimistic Concurrency Control (OCC) Results**
**Test Scenario:** OCC on registrate player

| **Metric** | **Value** |
|-----------|----------|
| Execution Time (ms) | 1128ms |
| Number of successful transactions | 9/10 |
| Number of retries due to version mismatch | 25 |

**Observations:**
- Optimistic is more efficient and offers higher performance but is primarily useful if the possibility for conflicts is very low – there are many records but relatively few users, or very few updates and mostly read-type operations.

---

### **2️⃣ Pessimistic Concurrency Control (PCC) Results**
**Test Scenario:** PCC on update match winner

| **Metric** | **Value** |
|-----------|----------|
| Execution Time (ms) | 5180 |
| Number of successful transactions | 5/5 |
| Number of transactions that had to wait due to locks | 4 |

**Observations:**
- Pessimistic locking is useful if there are a lot of updates and relatively high chances of users trying to update data at the same time. For example, if each operation can update a large number of records at a time.
- It's also more appropriate in applications that contain small tables that are frequently updated. In the case of these so-called hotspots, conflicts are so probable that optimistic locking wastes effort in rolling back conflicting transactions.

---

## **📌 Comparison Table**
| **Metric**               | **Optimistic CC** | **Pessimistic CC** |
|--------------------------|------------------|------------------|
| **Execution Time**       | Faster (1128) | Slower (5180) |
| **Transaction Failures** | More failures | Less/no failures |
| **Lock Contention**      | High | Low |
| **Best Use Case**       | Read-heavy | Write-heavy |


---

## **📌 Conclusion & Recommendations**
### **Key Findings:**
- Optimistic locking is a great choice when conflicts are rare, such as in systems with a high number of records but relatively few updates. It allows for better performance since transactions don’t have to wait for locks, making it ideal for read-heavy applications. However, when multiple users are frequently updating the same data, the risk of conflicts increases, making optimistic locking less efficient due to the overhead of rolling back failed transactions. In these cases, pessimistic locking is the better approach, as it prevents conflicts before they happen by locking records during updates. This is especially useful for small but frequently modified tables, often referred to as hotspots, where the chances of conflicts are so high that optimistic locking would waste resources on retries. Ultimately, choosing between the two depends on the balance between read and write operations, the likelihood of concurrent updates, and the cost of handling conflicts.

### **Final Recommendations:**
- When deciding between optimistic and pessimistic locking, consider the specific needs of the application:

Use optimistic locking when dealing with large datasets that are mostly read-heavy, where conflicts are rare, and performance is a priority. This is best for applications where users primarily retrieve data rather than frequently modifying it.


Use pessimistic locking if updates are frequent and there's a high likelihood of multiple users modifying the same records at the same time. This is especially important for small, frequently updated tables (hotspots), where preventing conflicts upfront is more efficient than constantly rolling back transactions.

Ultimately, the right choice depends on how often conflicts occur, the cost of handling them, and whether preventing them proactively (pessimistic) or resolving them after they happen (optimistic) is the better trade-off for the given scenario.

 ---
For part 2 click here: https://github.com/TofuBytes-Studies-Group/esport_final_assignment/blob/main/FinalAssignmentPART2.md
