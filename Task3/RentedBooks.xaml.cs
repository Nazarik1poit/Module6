using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Task3
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private LocalDatabase localDatabase;
        public BookList rentedBooks;

        public Window1(LocalDatabase database)
        {
            InitializeComponent();
            localDatabase = database;
            LoadRentedBooks();
            GlobalEventHandler.BookRented += OnBookRented;
        }

        public LocalDatabase LocalDatabase
        {
            get => default;
            set
            {
            }
        }

        public DialogWindow DialogWindow
        {
            get => default;
            set
            {
            }
        }

        // Загружаем арендованные книги из локальной базы данных
        private void LoadRentedBooks()
        {
            List<Book> books = localDatabase.GetRentedBooks();
            rentedBooks = new BookList(books);
            RentedBooksGrid.ItemsSource = rentedBooks.books;
        }

        // Обрабатываем возврат книги
        private async void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (RentedBooksGrid.SelectedItem != null)
            {
                Book selectedBook = RentedBooksGrid.SelectedItem as Book;
                if (selectedBook != null)
                {
                    DialogWindow dlg = new DialogWindow();
                    dlg.ShowDialog();
                    if (dlg.DialogResult == true)
                    {
                        int quantity = dlg.Quantity;
                        if (quantity == selectedBook.Count)
                        {
                            // Удаляем книгу из списка арендованных книг
                            rentedBooks.books.Remove(selectedBook);
                            // Увеличиваем количество книг в базе данных Firestore
                            await FBaseManager.ReturnBook(MainWindow.bookList, selectedBook.ID, localDatabase, selectedBook);
                            // Обновляем DataGrid
                            RentedBooksGrid.ItemsSource = null;
                            RentedBooksGrid.ItemsSource = rentedBooks.books;
                            GlobalEventHandler.OnBookReturned();
                        }
                        else if (quantity < selectedBook.Count)
                        {
                            Book returnedBook = new Book(selectedBook.Title, selectedBook.Author, selectedBook.ID, selectedBook.IsOnStock, quantity);
                            selectedBook.Count -= quantity;
                            await FBaseManager.ReturnBook(MainWindow.bookList, selectedBook.ID, localDatabase, returnedBook);
                            RentedBooksGrid.ItemsSource = null;
                            RentedBooksGrid.ItemsSource = rentedBooks.books;
                            GlobalEventHandler.OnBookReturned();
                        }
                        else
                        {
                            MessageBox.Show("У вас нет столько книг");
                            return;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для возврата");
            }
        }
        private void OnBookRented(object sender, EventArgs e)
        {
            LoadRentedBooks();
        }
    }
}
