# Simple Banking App

A C# console application that provides basic banking functionality including account login, balance viewing, deposits, and money transfers using PostgreSQL database.

## Features

- User authentication
- View account balance
- Deposit money
- Send money to other users
- Transaction handling with rollback support

## How to Run

1. Install PostgreSQL and create a database named `simple_bank`
2. Create a table `bank_users` with columns: `id`, `username`, `password`, `balance`
3. Install Npgsql NuGet package
4. Update the connection string with your database credentials
5. Compile and run the program
6. Login with your username and password
7. Use the menu to perform banking operations

## Requirements

- .NET Framework or .NET Core
- PostgreSQL database
- Npgsql NuGet package

## How to install Npssql 
- cd SimpleBanking
- dotnet add package Npgsql

## How to switch btn Database and create database view tables and inert data

- CREATE DATABASE simple_bank
- \c simple_bank

```sql
INSERT INTO bank_users (username, password, balance) VALUES
('steve', '1234', 1000.00),
('john', 'abcd', 500.00);
```

## Database Setup


```sql
CREATE TABLE bank_users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    password VARCHAR(50),
    balance DECIMAL(10,2) DEFAULT 0
);
```

## Example Usage

```
=== Welcome to Simple Bank ===
Enter username: john
Enter password: password123
✅ Welcome john!

--- Menu ---
1. View Account
2. Deposit
3. Send Money
4. Exit
Choose option: 1
👤 User: john
💰 Balance: 1000.00
```