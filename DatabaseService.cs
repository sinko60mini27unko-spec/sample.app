using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ScheduleTasksApp;

public sealed class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);
        _dbPath = Path.Combine(dataDir, "tasks.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Dates (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DateText TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DateId INTEGER NOT NULL,
                Title TEXT NOT NULL,
                Assignee TEXT NOT NULL DEFAULT '',
                Note TEXT NOT NULL DEFAULT '',
                IsDone INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (DateId) REFERENCES Dates(Id) ON DELETE CASCADE
            );";
        command.ExecuteNonQuery();

        UpsertDate(DateOnly.FromDateTime(DateTime.Today));
    }

    public List<DateItem> GetDates()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, DateText FROM Dates ORDER BY DateText;";

        var results = new List<DateItem>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new DateItem
            {
                Id = reader.GetInt32(0),
                Date = DateOnly.Parse(reader.GetString(1))
            });
        }

        return results;
    }

    public void UpsertDate(DateOnly date)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO Dates (DateText) VALUES ($dateText);";
        command.Parameters.AddWithValue("$dateText", date.ToString("yyyy-MM-dd"));
        command.ExecuteNonQuery();
    }

    public void DeleteDate(int dateId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Dates WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", dateId);
        command.ExecuteNonQuery();
    }

    public List<TaskItem> GetTasksByDate(int dateId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, DateId, Title, Assignee, Note, IsDone
            FROM Tasks
            WHERE DateId = $dateId
            ORDER BY Id;";
        command.Parameters.AddWithValue("$dateId", dateId);

        var results = new List<TaskItem>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new TaskItem
            {
                Id = reader.GetInt32(0),
                DateId = reader.GetInt32(1),
                Title = reader.GetString(2),
                Assignee = reader.GetString(3),
                Note = reader.GetString(4),
                IsDone = reader.GetInt32(5) == 1
            });
        }

        return results;
    }

    public void AddTask(TaskItem task)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Tasks (DateId, Title, Assignee, Note, IsDone)
            VALUES ($dateId, $title, $assignee, $note, $isDone);";
        command.Parameters.AddWithValue("$dateId", task.DateId);
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$assignee", task.Assignee);
        command.Parameters.AddWithValue("$note", task.Note);
        command.Parameters.AddWithValue("$isDone", task.IsDone ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void UpdateTask(TaskItem task)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Tasks
            SET Title = $title,
                Assignee = $assignee,
                Note = $note,
                IsDone = $isDone
            WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", task.Id);
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$assignee", task.Assignee);
        command.Parameters.AddWithValue("$note", task.Note);
        command.Parameters.AddWithValue("$isDone", task.IsDone ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void DeleteTask(int taskId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", taskId);
        command.ExecuteNonQuery();
    }
}
