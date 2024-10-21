using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace SqlToFirebase
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"D:/burdyko.json");
            GoogleCredential.FromFile("D:/burdyko.json");
            FirestoreDb db = FirestoreDb.Create(@"library-768a2");

            // Чтение SQL файла
            string sqlFilePath = "D:/TableName.sql";
            string[] sqlLines = File.ReadAllLines(sqlFilePath);

            // Регулярное выражение для извлечения значений из SQL-запросов
            string pattern = @"insert\s+into\s+TableName\s*\(id,\s*Title,\s*Author,\s*Count,\s*IsOnStock\)\s*values\s*\(\s*(\d+),\s*'([^']+)',\s*'([^']+)',\s*(\d+),\s*(true|false)\s*\);";

            // Чтение SQL-файла построчно
            foreach (var line in File.ReadLines(sqlFilePath))
            {
                Match match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    // Извлечение значений
                    int id = int.Parse(match.Groups[1].Value);
                    string title = match.Groups[2].Value;
                    string author = match.Groups[3].Value;
                    int count = int.Parse(match.Groups[4].Value);
                    bool isOnStock = bool.Parse(match.Groups[5].Value);

                    // Создание объекта для Firestore
                    var data = new
                    {
                        Id = id,
                        Title = title,
                        Author = author,
                        Count = count,
                        IsOnStock = isOnStock
                    };

                    // Добавление записи в коллекцию Firestore
                    DocumentReference docRef = db.Collection("books").Document(id.ToString());
                    await docRef.SetAsync(data);

                    Console.WriteLine($"Запись с ID {id} добавлена в Firestore.");
                }
            }
        }
    }
}
