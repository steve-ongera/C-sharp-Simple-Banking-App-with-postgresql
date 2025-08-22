using System;
using Npgsql;

namespace BankingApp
{
    class Program
    {
        static string connString = "Host=localhost;Port=5432;Username=postgres;Password=cp7kvt;Database=simple_bank;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== Welcome to Simple Bank ===");

            // Login
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            int userId = AuthenticateUser(username, password);
            if (userId == -1)
            {
                Console.WriteLine("❌ Invalid credentials.");
                return;
            }

            Console.WriteLine($"✅ Welcome {username}!");

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. View Account");
                Console.WriteLine("2. Deposit");
                Console.WriteLine("3. Send Money");
                Console.WriteLine("4. Exit");
                Console.Write("Choose option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowAccount(userId);
                        break;
                    case "2":
                        Deposit(userId);
                        break;
                    case "3":
                        SendMoney(userId);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static int AuthenticateUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT id FROM bank_users WHERE username=@u AND password=@p";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    cmd.Parameters.AddWithValue("p", password);

                    var result = cmd.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
        }

        static void ShowAccount(int userId)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT username, balance FROM bank_users WHERE id=@id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"👤 User: {reader.GetString(0)}");
                            Console.WriteLine($"💰 Balance: {reader.GetDecimal(1)}");
                        }
                    }
                }
            }
        }

        static void Deposit(int userId)
        {
            Console.Write("Enter deposit amount: ");
            decimal amount = Convert.ToDecimal(Console.ReadLine());

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = "UPDATE bank_users SET balance = balance + @amt WHERE id=@id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("amt", amount);
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine($"✅ Deposited {amount}");
        }

        static void SendMoney(int senderId)
        {
            Console.Write("Enter recipient username: ");
            string recipient = Console.ReadLine();
            Console.Write("Enter amount to send: ");
            decimal amount = Convert.ToDecimal(Console.ReadLine());

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Deduct from sender
                        string deduct = "UPDATE bank_users SET balance = balance - @amt WHERE id=@id AND balance >= @amt";
                        using (var cmd = new NpgsqlCommand(deduct, conn))
                        {
                            cmd.Parameters.AddWithValue("amt", amount);
                            cmd.Parameters.AddWithValue("id", senderId);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                Console.WriteLine("❌ Insufficient balance.");
                                tx.Rollback();
                                return;
                            }
                        }

                        // Add to recipient
                        string add = "UPDATE bank_users SET balance = balance + @amt WHERE username=@u";
                        using (var cmd = new NpgsqlCommand(add, conn))
                        {
                            cmd.Parameters.AddWithValue("amt", amount);
                            cmd.Parameters.AddWithValue("u", recipient);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                Console.WriteLine("❌ Recipient not found.");
                                tx.Rollback();
                                return;
                            }
                        }

                        tx.Commit();
                        Console.WriteLine($"✅ Sent {amount} to {recipient}");
                    }
                    catch
                    {
                        tx.Rollback();
                        Console.WriteLine("❌ Transaction failed.");
                    }
                }
            }
        }
    }
}
