using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3
{
    internal class GlobalEventHandler
    {
        public delegate void BookReturnedEventHandler(object sender, EventArgs e);
        public delegate void BookRentedEventHandler(object sender, EventArgs e);

        // Событие, которое будет срабатывать при возврате книги
        public static event BookReturnedEventHandler BookReturned;
        public static event BookRentedEventHandler BookRented;

        // Метод для вызова события
        public static void OnBookReturned()
        {
            // Вызываем событие, если на него подписаны обработчики
            BookReturned?.Invoke(null, EventArgs.Empty);
        }
        public static void OnBookRented()
        {
            BookRented?.Invoke(null, EventArgs.Empty);
        }
    }
}
