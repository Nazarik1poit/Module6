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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Task3;
using Google.Cloud.Firestore;
using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Net;

namespace Task3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FBaseManager baseManager = new FBaseManager();
        private bool isInitialized = false;
        private CollectionViewSource bookViewSource = new CollectionViewSource();
        public static BookList bookList = new BookList();
        List<Book> rentedBooks = new List<Book>();
        private string selectedFilterMethod = "Название";
        private int selectedBook_ID;
        LocalDatabase sqlManager = new LocalDatabase(); 
        public MainWindow()
        {
            InitializeComponent();

            // Применяем фильтр через делегат
            bookViewSource.Filter += new FilterEventHandler(BookFilter);
            

            isInitialized = true;
            FBaseManager.InitializeProjectId();
            FBaseManager.RetrieveAllDocuments("library-768a2", bookList);
            bookViewSource.Source = bookList.books;
            BookGrid.ItemsSource = bookViewSource.View;
            GlobalEventHandler.BookReturned += OnBookReturn;
        }

        public LocalDatabase LocalDatabase
        {
            get => default;
            set
            {
            }
        }

        public Window1 Window1
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

        private void Check_Rented(object sender, RoutedEventArgs e)
        {
            Window1 rentedBooksWindow = new Window1(sqlManager);
            rentedBooksWindow.Show();
        }
        private void Rent_Click(object sender, RoutedEventArgs e)
        {
            if (BookGrid.SelectedItem != null)
            {
                Book originalBook = bookList.books.FirstOrDefault(book => book.ID == selectedBook_ID);
                DialogWindow dlg = new DialogWindow();
                dlg.ShowDialog();

                if (dlg.DialogResult == true)
                {
                    if (!sqlManager.IsBookInDatabase(originalBook.ID))
                    {
                        // Создаем новую книгу для аренды с нужным количеством
                        Book rentedBook = new Book(originalBook.Title, originalBook.Author, originalBook.ID, originalBook.IsOnStock, dlg.Quantity);

                        // Уменьшаем количество в оригинальной книге
                        if (originalBook.Count >= dlg.Quantity)
                        {
                            FBaseManager.RentBook(bookList, selectedBook_ID, rentedBook);
                            bookViewSource.View.Refresh(); // Обновляем отображение после аренды
                            rentedBooks.Add(rentedBook);
                            sqlManager.AddRentedBook(rentedBook);
                        }
                        else
                        {
                            MessageBox.Show("Недостаточно книг на складе для аренды.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Книга уже арендована");
                    }

                    GlobalEventHandler.OnBookRented();
                }
            }
            else
            {
                MessageBox.Show("Выделите книгу в списке");
            }
        }


        private void OnBookReturn(object sender, EventArgs e)
        {
            bookViewSource.View.Refresh();

        }
        private void BookGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранную строку
            var selectedRow = BookGrid.SelectedItem;

            if (selectedRow != null)
            {
                // Обработка выбранной строки
                Book book = selectedRow as Book;
                selectedBook_ID = book.ID;
            }
        }

        private void BookFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is Book book)
            {
                if (!string.IsNullOrEmpty(FilterTextBox.Text))
                {
                    // Фильтруем в зависимости от выбранного метода
                    switch (selectedFilterMethod)
                    {
                        case "Название":
                            e.Accepted = book.Title.ToLower().Contains(FilterTextBox.Text.ToLower());
                            break;
                        case "Автор":
                            e.Accepted = book.Author.ToLower().Contains(FilterTextBox.Text.ToLower());
                            break;
                        case "ID":
                            e.Accepted = (book.ID).ToString().Contains(FilterTextBox.Text.ToLower());
                            break;
                        case "Количество":
                            e.Accepted = (book.Count).ToString().Contains(FilterTextBox.Text.ToLower());
                            break;
                        case "Наличие":
                            // Преобразуем булевое значение в строку "true" или "false"
                            string isOnStockString = book.IsOnStock ? "true" : "false";

                            // Сравниваем с текстом в TextBox, игнорируя регистр
                            e.Accepted = isOnStockString.ToLower().Contains(FilterTextBox.Text.ToLower());
                            break;
                    }
                }
                else
                {
                    e.Accepted = true; // Показываем все, если фильтр пустой
                }
            }
        }

        // Обработчик для TextBox (обновление фильтра при вводе текста)
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bookViewSource.View.Refresh();
        }

        // Обработчик для ComboBox (смена метода фильтрации)
        private void FilterMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                if (FilterMethodComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    selectedFilterMethod = selectedItem.Content.ToString();
                    bookViewSource.View.Refresh(); // Обновляем фильтр при изменении метода фильтрации
                }
            }
        }
    }
}
