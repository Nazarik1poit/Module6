using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows;

namespace Task3
{
    public class LocalDatabase
    {
        private string connectionString = "Data Source=LocalLibrary.db;Version=3;";

        public LocalDatabase()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS RentedBooks (ID INTEGER PRIMARY KEY, Title TEXT, Author TEXT, Count INTEGER)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddRentedBook(Book book)
        {
            if (!IsBookInDatabase(book.ID))
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO RentedBooks (ID, Title, Author, Count) VALUES (@ID, @Title, @Author, @Count)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", book.ID);
                        command.Parameters.AddWithValue("@Title", book.Title);
                        command.Parameters.AddWithValue("@Author", book.Author);
                        command.Parameters.AddWithValue("@Count", book.Count);
                        command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                MessageBox.Show("Книга уже арендована");
            }
        }

        public void DeleteRentedBook(int bookID)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM RentedBooks WHERE ID = @ID";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ID", bookID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public bool IsBookInDatabase(int bookID)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT COUNT(*) FROM RentedBooks WHERE ID = @ID";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@ID", bookID);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public List<Book> GetRentedBooks()
        {
            List<Book> rentedBooks = new List<Book>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM RentedBooks";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book(reader.GetString(1), reader.GetString(2), reader.GetInt32(0), true, reader.GetInt32(3));
                            rentedBooks.Add(book);
                        }
                    }
                }
            }
            return rentedBooks;
        }
    }
}
