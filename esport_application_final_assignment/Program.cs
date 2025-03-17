using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;


class Program
{
    private const string ConnectionString = "server=localhost;database=esport_final_assignment;user=root;password=password";
    private const string ConnectionString2 = "server=localhost;database=esport_final_assignment;user=user;password=password";


    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(@"

Menu:
1. Optimistic Concurrency Control
2. Pessimistic Concurrency Control
3. Registration of player
4. Pessimistic registration of player
5. OCC for registration of player
6. PCC for match result
7. Exit");

            Console.Write("\nChoose an option (1-7): ");
            string choice = Console.ReadLine();
            

            if (choice == "7") break;
            
            int threadCount = 10; // Number of threads to simulate concurrent operations

            int tournamentId = 6; // Tournament ID being updated
            int matchId = 1;
            int winnerId = 2;
            DateTime newStartDate1 = DateTime.Now.AddDays(10);
            DateTime newStartDate2 = DateTime.Now.AddDays(11);
            
            int playerId = 2; // Player ID
            string adminName = "Admin 1"; // Admin performing the operation

            
            if (choice == "1")
            {
                Console.WriteLine("\n[Using Optimistic Concurrency Control]\n");
                Task admin1 = Task.Run(() => OptimisticUpdateTournament(tournamentId, newStartDate1, "Admin 1", ConnectionString));
                Task admin2 = Task.Run(() => OptimisticUpdateTournament(tournamentId, newStartDate2, "Admin 2", ConnectionString2));
                Task.WaitAll(admin1, admin2);
            }
            else if (choice == "2")
            {
                Console.WriteLine("\n[Using Pessimistic Concurrency Control]\n");
                Task admin1 = Task.Run(() => PessimisticUpdateMatch(matchId, winnerId, "Admin 1", ConnectionString));
                Task admin2 = Task.Run(() => PessimisticUpdateMatch(matchId, winnerId, "Admin 2", ConnectionString2));
                Task.WaitAll(admin1, admin2);
            }
            else if (choice == "3")
            {
                Console.WriteLine("\n[Using Atomicity]\n");
                RegisterPlayerInTournament(tournamentId, playerId, adminName, ConnectionString);

            }
            else if (choice == "4")
            {
                Console.WriteLine("\n[Using Pessimistic Concurrency To Register Player]\n");
                Task admin1 = Task.Run(() => PessimisticRegisterPlayerInTournament(tournamentId, playerId, ConnectionString ));
                Task admin2 = Task.Run(() => PessimisticRegisterPlayerInTournament(tournamentId, playerId, ConnectionString2 ));
                Task.WaitAll(admin1, admin2);
            }
            else if (choice == "5")
            {
                Console.WriteLine("\n[Using OCC to register player]\n");

                int tournamentId1 = 2;

                Task[] tasks = new Task[threadCount];
                Stopwatch stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < threadCount; i++)
                {
                    int threadNum = i + 1;
                    tasks[i] = Task.Run(() => OptimisticRegisterPlayer(tournamentId1, playerId, threadNum));
                }

                Task.WaitAll(tasks);
                stopwatch.Stop();

                Console.WriteLine($"Simulation completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
            else if (choice == "6")
            {
                Console.WriteLine("\n[Using PCC to register player]\n");

                int matchIdP = 2;
                int threadCountP = 5; // Simulating 5 concurrent users

                Task[] tasks = new Task[threadCountP];
                Stopwatch stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < threadCountP; i++)
                {
                    int threadNum = i + 1;
                    tasks[i] = Task.Run(() => PessimisticUpdateMatchWinner(matchIdP, threadNum));
                }

                Task.WaitAll(tasks);
                stopwatch.Stop();

                Console.WriteLine($"Simulation completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
            else
            {
                Console.WriteLine("Invalid choice. Please select 1, 2, 3, 4 or 5.");
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    // 🔹 Optimistic Concurrency Control
    static void OptimisticUpdateTournament(int tournamentId, DateTime newStartDate, string adminName, string connString)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();

            int currentVersion;
            using (MySqlCommand cmd = new MySqlCommand("SELECT version FROM Tournaments WHERE tournament_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", tournamentId);
                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine($"{adminName}: Tournament not found.");
                    return;
                }
                currentVersion = Convert.ToInt32(result);
            }

            // Simulate processing time (race condition test)
            Thread.Sleep(2000);

            using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE Tournaments SET start_date = @date, version = version + 1 WHERE tournament_id = @id AND version = @version", conn))
            {
                cmd.Parameters.AddWithValue("@id", tournamentId);
                cmd.Parameters.AddWithValue("@date", newStartDate);
                cmd.Parameters.AddWithValue("@version", currentVersion);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"{adminName}: Successfully updated start date to {newStartDate}");
                }
                else
                {
                    Console.WriteLine($"{adminName}: Update failed due to version mismatch (Optimistic Concurrency Control).");
                }
            }
        }
    }

    // 🔹 Pessimistic Concurrency Control
    static void PessimisticUpdateMatch(int matchId, int winnerId, string adminName, string connString)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            using (MySqlTransaction transaction = conn.BeginTransaction()) // Start transaction
            {
                try
                {
                    // Lock the row
                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT winner_id FROM Matches WHERE match_id = @id FOR UPDATE", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", matchId);
                        object result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            Console.WriteLine($"{adminName}: Matches not found.");
                            return;
                        }
                    }

                    // Simulate processing time (to demonstrate lock behavior)
                    Thread.Sleep(2000);

                    using (MySqlCommand cmd = new MySqlCommand(
                        "UPDATE Matches SET winner_id = @wID WHERE match_id = @id", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", matchId);
                        cmd.Parameters.AddWithValue("@wID", winnerId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"{adminName}: Successfully updated winner id to {winnerId}" + $"Updated at: {DateTime.Now}" );
                        }
                        else
                        {
                            Console.WriteLine($"{adminName}: Update failed.");
                        }
                    }
                    
                    using (MySqlCommand stmt = new MySqlCommand("CALL UpdateRanking(@playerId)", conn, transaction))
                    {
                        stmt.Parameters.AddWithValue("@playerId", winnerId); // Using the winner's ID
                        stmt.ExecuteNonQuery(); // Execute the procedure to update ranking

                        Console.WriteLine($"{adminName}: Ranking updated for player {winnerId}");
                    }

                    transaction.Commit(); // Commit transaction
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback on failure
                    Console.WriteLine($"{adminName}: Transaction failed - {ex.Message}");
                }
            }
        }
    }
    
    static void RegisterPlayerInTournament(int tournamentId, int playerId, string adminName, string connString)
{
    using (MySqlConnection conn = new MySqlConnection(connString))
    {
        conn.Open();
        using (MySqlTransaction transaction = conn.BeginTransaction()) // Start transaction
        {
            try
            {
                // Step 1: Check if the tournament is full
                int currentPlayerCount;
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM Tournament_Registrations WHERE tournament_id = @id", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", tournamentId);
                    currentPlayerCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int maxPlayers;
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT max_players FROM Tournaments WHERE tournament_id = @id", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", tournamentId);
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        Console.WriteLine($"{adminName}: Tournament not found.");
                        return;
                    }
                    maxPlayers = Convert.ToInt32(result);
                }

                // Step 2: If the tournament is full, rollback the transaction
                if (currentPlayerCount >= maxPlayers)
                {
                    Console.WriteLine($"{adminName}: Tournament is full, registration failed.");
                    transaction.Rollback();
                    return;
                }

                // Step 3: Register the player by inserting into Tournament_Registrations
                using (MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO Tournament_Registrations (tournament_id, player_id) VALUES (@tournamentId, @playerId)", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@tournamentId", tournamentId);
                    cmd.Parameters.AddWithValue("@playerId", playerId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"{adminName}: Player registration failed.");
                        transaction.Rollback();
                        return;
                    }
                }

                // Step 4: Update the player's ranking (just an example of what can be done)
                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Players SET ranking = ranking + 10 WHERE player_id = @playerId", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"{adminName}: Player ranking update failed.");
                        transaction.Rollback();
                        return;
                    }
                }

                // Step 5: Commit transaction if everything was successful
                transaction.Commit();
                Console.WriteLine($"{adminName}: Player {playerId} successfully registered in tournament {tournamentId}.");
            }
            catch (Exception ex)
            {
                // Rollback on any failure
                transaction.Rollback();
                Console.WriteLine($"{adminName}: Transaction failed - {ex.Message}");
            }
        }
    }
}
    
    static bool PessimisticRegisterPlayerInTournament(int tournamentId, int playerId, string connString)
{
    // Open connection
    using (MySqlConnection conn = new MySqlConnection(connString))
    {
        conn.Open();

        // Start a transaction
        using (MySqlTransaction transaction = conn.BeginTransaction())
        {
            try
            {
                // Step 1: Lock the row for the tournament to ensure no one else modifies it
                using (MySqlCommand cmd = new MySqlCommand(
                           "SELECT max_players, (SELECT COUNT(*) FROM Tournament_Registrations WHERE tournament_id = @id) AS current_players FROM Tournaments WHERE tournament_id = @id FOR UPDATE", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", tournamentId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        Console.WriteLine("Tournament not found.");
                        reader.Close();
                        transaction.Rollback();
                        return false;
                    }

                    int maxPlayers = reader.GetInt32("max_players");
                    int currentPlayerCount = reader.GetInt32("current_players");
                    reader.Close();

                    // Step 2: If the tournament is full, reject registration
                    if (currentPlayerCount >= maxPlayers)
                    {
                        Console.WriteLine("Tournament is full, registration failed.");
                        transaction.Rollback(); // Rollback the transaction if registration is not successful
                        return false;
                    }

                    // Step 3: Register the player by inserting into Tournament_Registrations
                    using (MySqlCommand insertCmd = new MySqlCommand(
                               "INSERT INTO Tournament_Registrations (tournament_id, player_id) VALUES (@tournamentId, @playerId)", conn, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@tournamentId", tournamentId);
                        insertCmd.Parameters.AddWithValue("@playerId", playerId);

                        int rowsAffected = insertCmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine("Player registration failed.");
                            transaction.Rollback(); // Rollback if insertion fails
                            return false;
                        }
                    }

                    // Step 4: Commit transaction if everything is successful
                    transaction.Commit();
                    Console.WriteLine("Player successfully registered in tournament.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                transaction.Rollback();
                Console.WriteLine($"Transaction failed: {ex.Message}");
                return false;
            }
        }
    }
}

    static void OptimisticRegisterPlayer(int tournamentId, int playerId, int threadNum)
    {
        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            conn.Open();
            bool success = false;
            int attempts = 0;

            while (!success && attempts < 5) // Retry up to 5 times
            {
                attempts++;
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Step 1: Read tournament version
                    int currentVersion;
                    int currentPlayers;
                    int maxPlayers;

                    using (MySqlCommand cmd = new MySqlCommand(
                               "SELECT version, max_players, (SELECT COUNT(*) FROM Tournament_Registrations WHERE tournament_id = @id) AS current_players FROM Tournaments WHERE tournament_id = @id",
                               conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", tournamentId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Console.WriteLine($"[Thread {threadNum}] Tournament not found.");
                                return;
                            }

                            currentVersion = reader.GetInt32("version");
                            maxPlayers = reader.GetInt32("max_players");
                            currentPlayers = reader.GetInt32("current_players");
                        }
                    }

                    // Step 2: Check if space is available
                    if (currentPlayers >= maxPlayers)
                    {
                        Console.WriteLine($"[Thread {threadNum}] Tournament is full. Registration failed.");
                        return;
                    }

                    // Step 3: Try to register using OCC (version check)
                    using (MySqlCommand cmd = new MySqlCommand(
                               "UPDATE Tournaments SET version = version + 1 WHERE tournament_id = @id AND version = @version",
                               conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", tournamentId);
                        cmd.Parameters.AddWithValue("@version", currentVersion);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"[Thread {threadNum}] OCC version mismatch. Retrying...");
                            transaction.Rollback();
                            Thread.Sleep(100); // Wait before retrying
                            continue;
                        }
                    }

                    // Step 4: Insert player into tournament
                    using (MySqlCommand cmd = new MySqlCommand(
                               "INSERT INTO Tournament_Registrations (tournament_id, player_id) VALUES (@tournamentId, @playerId)",
                               conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@tournamentId", tournamentId);
                        cmd.Parameters.AddWithValue("@playerId", playerId);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Console.WriteLine(
                        $"[Thread {threadNum}] Player {playerId} successfully registered in tournament {tournamentId}.");
                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"[Thread {threadNum}] Transaction failed: {ex.Message}");
                }
            }

            if (!success)
            {
                Console.WriteLine($"[Thread {threadNum}] Registration failed after multiple attempts.");
            }
        }
    }
    
    static void PessimisticUpdateMatchWinner(int matchId, int playerId)
    {
        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction();

            try
            {
                Stopwatch threadTimer = Stopwatch.StartNew();

                // Step 1: Lock the match row to prevent concurrent updates
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT winner_id FROM Matches WHERE match_id = @id FOR UPDATE", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", matchId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        Console.WriteLine($"[Thread {playerId}] Match not found.");
                        return;
                    }

                    int? currentWinner = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                    reader.Close();

                    // Simulate processing delay
                    Thread.Sleep(new Random().Next(500, 1500));

                    // Step 2: Update the match winner
                    using (MySqlCommand updateCmd = new MySqlCommand(
                        "UPDATE Matches SET winner_id = @winnerId WHERE match_id = @id", conn, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("@id", matchId);
                        updateCmd.Parameters.AddWithValue("@winnerId", playerId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"[Thread {playerId}] Match update failed.");
                            transaction.Rollback();
                            return;
                        }
                    }

                    // Step 3: Commit the transaction
                    transaction.Commit();
                    threadTimer.Stop();
                    Console.WriteLine($"[Thread {playerId}] Match {matchId} winner set to Player {playerId}. Time taken: {threadTimer.ElapsedMilliseconds} ms");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"[Thread {playerId}] Transaction failed: {ex.Message}");
            }
        }
    }


}
