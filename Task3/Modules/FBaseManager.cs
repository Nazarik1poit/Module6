using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Task3
{
    internal class FBaseManager
    {
        public static void InitializeProjectId()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = "library.json";
            string fullPath = Path.Combine(basePath, relativePath);
            // [START firestore_setup_client_create_with_project_id]
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
            FirestoreDb db = FirestoreDb.Create(@"library-768a2");
            // [END firestore_setup_client_create_with_project_id]
        }

        public static async Task RetrieveAllDocuments(string project, BookList books)
        {
            FirestoreDb db = FirestoreDb.Create(project);
            // [START firestore_setup_dataset_read]
            CollectionReference booksRef = db.Collection("books");
            QuerySnapshot snapshot = await booksRef.GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                string title = Convert.ToString(documentDictionary["Title"]);
                string author = Convert.ToString(documentDictionary["Author"]);
                int count = Convert.ToInt32(documentDictionary["Count"]);
                int ID = Convert.ToInt32(documentDictionary["Id"]);
                bool isOnStock = Convert.ToBoolean(documentDictionary["IsOnStock"]);

                Book book = new Book(title, author, ID, isOnStock, count);
                books.Add(book);
            }
            // [END firestore_setup_dataset_read]
        }

        public static async Task RentBook(BookList booklist, int bookID, Book rentedBook)
        {
            FirestoreDb db = FirestoreDb.Create("library-768a2");
            CollectionReference booksRef = db.Collection("books");

            // Находим книгу с указанным ID
            Book bookToRent = booklist.books.FirstOrDefault(book => book.ID == bookID);

            // Проверка на наличие книги
            if (bookToRent == null)
            {
                Console.WriteLine($"Книга с ID {bookID} не найдена.");
                return; // Прекращаем выполнение, если книга не найдена
            }

            // Проверка, чтобы не арендовать больше книг, чем есть на складе
            if (rentedBook.Count > bookToRent.Count)
            {
                Console.WriteLine("Недостаточно книг на складе для аренды.");
                return; // Прекращаем выполнение, если недостаточно книг
            }

            // Уменьшаем количество книг в списке
            bookToRent.Count -= rentedBook.Count;

            DocumentReference bookRef = booksRef.Document(bookID.ToString());

            // Проверяем, нужно ли обновлять поле IsOnStock
            if (bookToRent.Count <= 0)
            {
                bookToRent.IsOnStock = false;
                var updateStock = new Dictionary<string, object>
                {
                    { "IsOnStock", false }
                };
                await bookRef.UpdateAsync(updateStock);
            }

            // Создаем словарь для обновления
            var updates = new Dictionary<string, object>
            {
                { "Count", bookToRent.Count }
            };

            // Обновляем поле Count в Firestore
            await bookRef.UpdateAsync(updates);

            Console.WriteLine($"Книга с ID {bookID} была успешно арендована. Остаток: {bookToRent.Count}");
        }


        public static async Task ReturnBook(BookList booklist, int bookID, LocalDatabase localDb, Book returnedBook)
        {
            localDb.DeleteRentedBook(bookID);
            FirestoreDb db = FirestoreDb.Create("library-768a2");
            CollectionReference booksRef = db.Collection("books");

            // Находим книгу с указанным ID
            Book bookToReturn = booklist.books.FirstOrDefault(book => book.ID == bookID);
            bookToReturn.Count += returnedBook.Count;

            DocumentReference bookRef = booksRef.Document(bookID.ToString());

            if (bookToReturn != null)
            {
                if (bookToReturn.Count >= 1)
                {
                    bookToReturn.IsOnStock = true;
                    var updateStock = new Dictionary<string, object>()
                    {
                        { "IsOnStock", true }
                    };
                    await bookRef.UpdateAsync(updateStock);
                }
                // Создаем словарь для обновления
                var updates = new Dictionary<string, object>
                {
                    { "Count", bookToReturn.Count }
                };

                // Обновляем поле Count в Firestore
                await bookRef.UpdateAsync(updates);

                Console.WriteLine($"Книга с ID {bookID} была успешно возвращена. Остаток: {bookToReturn.Count}");
            }
            else
            {
                Console.WriteLine($"Книга с ID {bookID} не найдена.");
            }
        }
    }
}



