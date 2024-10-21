using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Task3
{
    public class Book
    {
        public string Title { get; }

        public string Author { get; }

        public int ID { get; }  

        public bool IsOnStock { get; set; }

        public int Count { get; set; }

        public Book(string title, string author, int ID, bool isOnStock, int count)
        {
            this.Title = title;
            this.Author = author;
            this.ID = ID;
            this.IsOnStock = isOnStock;
            this.Count = count;
        }
    }
    public class BookList
    {
        public ObservableCollection<Book> books;

        public BookList()
        {
            books = new ObservableCollection<Book>();  // Инициализация списка
        }

        public BookList(List<Book> Books)
        {
            books = new ObservableCollection<Book>(Books);
        } 

        public void Print()
        {
            for (int i = 0; i < books.Count; i++)
            {
                Console.WriteLine(books[i].Title);
            }
        }
        public void Add(Book book)
        {
            
            books.Add(book);
        }

        public void Remove(Book book)
        {
            books.Remove(book);
        }
    }
}
