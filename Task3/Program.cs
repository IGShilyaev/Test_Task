using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Console_Meeting_Planner
{
    internal class Program
    {
        static MeetingList my_meetings = new MeetingList(); //Храниение всех созданных встреч в памяти
        static bool stop_notifier = false; //Флаг для остановки потока, отправляющего уведомления
        static void Main(string[] args)
        {
            //Запуск потока для отправки уведомления
            Thread notifier = new Thread(Notifier);
            notifier.IsBackground = true;
            notifier.Start();

            //Вывод главного меню приложения
            Main_menu();
        }

        //Метод для отправки уведомлений пользователю
        public static void Notifier()
        {
            do
            {
                try
                {
                    foreach (Meeting meeting in my_meetings.Meetings)
                    {
                        if (DateTime.Now >= meeting.Start_time.AddMinutes(-meeting.NotificationMinutes) && !meeting.NotificationSent)
                        {
                            meeting.NotificationSent = true;
                            Console.WriteLine("\n---NOTIFICATION---");
                            Console.WriteLine("Скоро следующая встреча:");
                            Console.WriteLine(meeting.MeetingInfo());
                            Console.WriteLine("------------------\n");
                        }
                    }
                }
                catch 
                {
                   
                }
            }
            while (!stop_notifier);
        }

        //Метод для получения команды в главном меню приложения
        public static void Main_menu()
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Просмотреть имеющиеся встречи");
            Console.WriteLine("2 - Создать новую встречу");
            Console.WriteLine("3 - Редактировать встречу");
            Console.WriteLine("4 - Удалить встречу");
            Console.WriteLine("5 - Экспортировать список встреч в текстовый файл");
            Console.WriteLine("0 - Выход из программы");

            Console.WriteLine();
            Console.Write("Введите номер команды: ");
            int command = ReadNum();
            if (command != 0)
            {
                GetAction(command);
            }
            else
            {
                stop_notifier = true;
                return;
            }
        }

        //Метод для вызова основной функции приложения в соответствии со введенной командой
        public static void GetAction(int command)
        {
            switch (command) 
            {
                case 1: 
                    {
                        ShowMeetings(my_meetings); //Вывод встреч за определенный день
                        break; 
                    }
                case 2: 
                    {
                        PlanMeeting(); //Создание новой встречи
                        break;
                    }
                case 3:
                    {
                        EditMeeting(); //Редактирование элементов встречи
                        break;
                    }
                case 4:
                    {
                        DeleteMeeting(); //Удаление встречи
                        break;
                    }
                case 5:
                    {
                        WriteToFile(); //Запись в файл информации о встречах за определенный день
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Такой команды не существует");
                        break;
                    }
            }
            Main_menu(); //Возвращение к главному меню приложения
        }

        // Выбор дня и отображение списка встреч за определенный день
        public static void ShowMeetings(MeetingList list)
        {
            if (list.Meetings.Count() == 0)
            {
                Console.WriteLine("У вас нет запланнированных встреч");
                return;
            }
            Console.WriteLine("На какой день показать встречи?");
            DateTime target = ChooseDate();
            MeetingList selection = my_meetings.SearchDate(target);
            if (selection.Meetings.Count == 0) 
            {
                Console.WriteLine("На заданный день нет запланированных встреч");
                return;
            }
            foreach (Meeting meeting in selection.Meetings)
            {
                Console.WriteLine(meeting.MeetingInfo());
            }
        }

        #region Create Functions
        //Метод для задания даты пользователем
        public static DateTime ChooseDate()
        {
            DateTime? res = null;
            do
            {
                Console.WriteLine("Задайте дату");
                Console.Write("Год: ");
                int year = ReadNum();
                Console.Write("Месяц (числом): ");
                int month = ReadNum();
                Console.Write("День: ");
                int day = ReadNum();
                try
                {
                    res = new DateTime(year, month, day);
                }
                catch
                {
                    Console.WriteLine("Дата задана некорректно, попробуйте еще раз");
                }
            }while (!res.HasValue);
            return (DateTime) res;
        }

        //Метод для задания времени пользователем
        public static DateTime ChooseTime()
        {
            DateTime? temp = null;
            int minutes;
            int hours;
            do
            {
                try
                {
                    Console.Write("Часы: ");
                    hours = ReadNum();
                    Console.Write("Минуты: ");
                    minutes = ReadNum();
                    temp = new DateTime(2000, 1, 1, hours, minutes, 0);
                }
                catch
                {
                    Console.WriteLine("Время задано некорректно, попробуйте еще раз");
                }
            } while (!temp.HasValue);
            return (DateTime) temp;
        }

        //Метод для задания пользователем даты и времени
        public static DateTime ChooseDateTime() 
        {
            DateTime res = ChooseDate();
            Console.WriteLine("Задайте время для даты " + res.Date.ToString());
            DateTime temp = ChooseTime();
            res = res.Date + ((DateTime)temp).TimeOfDay;
            return (DateTime) res;
        }


        //Метод для создания новой встречи
        public static void PlanMeeting()
        {
            Console.WriteLine("Введите дату и время начала встречи");
            DateTime start = ChooseDateTime();
            if (start < DateTime.Now)
            {
                Console.WriteLine("Невозможно назначить встречу на прошедшую дату. Операция завершена");
                return;
            }
            Console.WriteLine("Введите примерную длительность встречи");
            DateTime end = start + ChooseTime().TimeOfDay;
            Meeting res = new Meeting(start, end, "Встреча " + my_meetings.Name_counter.ToString());
            if (my_meetings.MeetingIntersect(res)) 
            {
                Console.WriteLine("Невозможно добавить дату из-за пересечений в расписании. Операция завершена");
                return;
            }
            my_meetings.AddMeeting(res);
            Console.WriteLine("Встреча успешно добавлена");
        }
        #endregion

        #region Search Function
        //Метод для выбора встречи по её названию
        public static Meeting ChooseMeeting(out int index)
        {
            if (my_meetings.Meetings.Count > 0) 
            {
                Console.WriteLine("Все встречи:");
                Console.WriteLine(my_meetings.ShowAll());
                Console.Write("Введите название встречи: ");
                Meeting target = my_meetings.SearchName(Console.ReadLine(), out index);
                return target;
            }
            else
            {
                Console.WriteLine("Нет созданных встреч");
            }
            index = -1;
            return null;
        }
        #endregion

        #region Edit Functions
        //Метод для выбора встречи на редактирование и вызова меню редактирования
        public static void EditMeeting()
        {
            int index = -1;
            Meeting target = ChooseMeeting(out index);
            if (index != -1) 
            {
                EditMenu(index);
            }
            else
            {
                Console.WriteLine("Не удалось найти встречу с указанным названием. Операция завершена");
            }
        }

        //Метод для получения от пользователя команды в меню редактирования
        public static void EditMenu(int index)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Изменить название встречи");
            Console.WriteLine("2 - Передвинуть начало встречи");
            Console.WriteLine("3 - Изменить длительность встречи");
            Console.WriteLine("4 - Задать время отправки уведомления до встречи");
            Console.WriteLine("0 - Отменить операцию");

            Console.WriteLine();
            Console.Write("Введите номер команды: ");
            int command = ReadNum();
            if (command != 0)
            {
                GetEdit(command, index);
            }
            else
            {
                return;
            }
        }

        //Переход к редактированию в соответствии с выбранной пользователем командой
        public static void GetEdit(int command, int index)
        {
            switch (command)
            {
                case 1:
                    {
                        EditName(index); //Редактирование названия встречи
                        break;
                    }
                case 2:
                    {
                        EditStart(index); //Редактирование начала встречи
                        break;
                    }
                case 3:
                    {
                        EditEnd(index); //Редактирование длительности встречи
                        break;
                    }
                case 4:
                    {
                        EditNotificationTime(index); //Редактирование времени отправки уведомления
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Такой команды не существует. Операция завершена");
                        break;
                    }
            }
        }

        //Метод для редактирования названия встречи
        private static void EditName(int index)
        {
            Console.Write("Введите новое название для встречи: ");
            if (my_meetings.Meetings[index].SetName(Console.ReadLine()))
            {
                Console.WriteLine("Изменения сохранены");
            }
            else
            {
                Console.WriteLine("Не удалось внести изменения. Операция завершена");
            }
        }

        //Метод для редактирования времени начала встречи
        private static void EditStart(int index)
        {
            DateTime newStart = ChooseDateTime();
            if (my_meetings.Meetings[index].SetStart(newStart, my_meetings))
            {
                Console.WriteLine("Изменения сохранены");
            }
            else
            {
                Console.WriteLine("Не удалось внести изменения. Операция завершена");
            }
        }

        //Метод для редактирования длительности встречи
        private static void EditEnd(int index)
        {
            DateTime newEnd = my_meetings.Meetings[index].Start_time + ChooseTime().TimeOfDay;
            if (my_meetings.Meetings[index].SetEnd(newEnd, my_meetings))
            {
                Console.WriteLine("Изменения сохранены");
            }
            else
            {
                Console.WriteLine("Не удалось внести изменения. Операция завершена");
            }
        }

        //Метод для редактирования времени уведомления о встрече
        private static void EditNotificationTime(int index)
        {
            Console.Write("Введите за сколько минут до начала встречи надо отправить уведомление: ");
            int newTime = ReadNum();
            if (my_meetings.Meetings[index].SetNotificationTime(newTime))
            {
                Console.WriteLine("Изменения сохранены");
            }
            else
            {
                Console.WriteLine("Не удалось внести изменения. Операция завершена");
            }
        }

        #endregion

        #region Delete Functions
        //Метод для удаления выбранной по имени встречи
        public static void DeleteMeeting()
        {
            int index = -1;
            Meeting target = ChooseMeeting(out index);

            if (index != -1)
            {
                my_meetings.DeleteMeeting(my_meetings.Meetings[index]);
                Console.WriteLine("Встреча удалена");
            }
            else
            {
                Console.WriteLine("Не удалось найти встречу с данным именем. Операция завершена");
            }

        }
        #endregion

        #region Write File Functions
        //Метод для записи в текстовый файл встреч за выбранный пользователем день
        public static void WriteToFile()
        {
            Console.WriteLine("Задайте дату, за которую нужно сохранить встречи");
            DateTime date = ChooseDate();
            MeetingList selection = my_meetings.SearchDate(date);

            Console.Write("Введите название файла: ");
            string fileName = Console.ReadLine();

            if(selection.WriteToFile(fileName + ".txt"))
            {
                Console.WriteLine("Данные о встречах записаны в файл " + fileName + ".txt");
            }
            else
            {
                Console.WriteLine("Ошибка во время записи файла. Операция прервана");
            }
        }



        #endregion


        #region General Functions
        //Метод для задания пользователем целого неотрицательного числа
        public static int ReadNum()
        {
            int? num = null;
            do
            {
                try
                {
                    num = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Значение введено некорректно. Должно быть введено целое неотрицательное число.");
                    Console.Write("Задайте значение еще раз: ");
                }
            } while (!num.HasValue || num<0);
            return (int) num;
        }
        #endregion

    }

    #region MeetingList Class
    //Класс со списком (List) встреч (Meeting)
    public class MeetingList
    {
        private int name_counter; //Счетчик для автоматического формирования названия встреч
        private List<Meeting> meetingList; //Список встреч

        //Доступ к счетчику только на чтение
        public int Name_counter { get { return name_counter; } }

        //Доступ ко списку только на чтение
        public List<Meeting> Meetings { get { return meetingList; } }

        //Конструктор по умолчанию
        //Создает пустой список и ставит счетчик на "1"
        public MeetingList()
        {
            meetingList = new List<Meeting>();
            name_counter = 1;
        }

        //Добавление встречи в список
        public void AddMeeting(Meeting m)
        {
            meetingList.Add(m);
            name_counter++;
        }

        //Удаление встречи из списка
        public void DeleteMeeting(Meeting m)
        {
            meetingList.Remove(m);
        }

        //Метод для проверки пересечения встречи с имеющимся расписанием.
        //Возвращает True в случае нахождения пересечения.
        //При отсутствии перечений возвращает False.
        public bool MeetingIntersect(Meeting m)
        {
            foreach (Meeting item in this.meetingList)
            {
                if (m.Start_time >= item.Start_time && m.Start_time <= item.End_time)
                {
                    return true;
                }
                if (m.End_time >= item.Start_time && m.End_time <= item.End_time)
                {
                    return true;
                }
                if (m.Start_time <= item.Start_time && m.End_time >= item.End_time)
                {
                    return true;
                }
            }
            return false;
        }

        //Получение информации обо всех встречах в списке
        //Возвращает строковое значение
        public string ShowAll()
        {
            string res = "";
            if (meetingList.Count == 0)
            {
                res = "Нет встреч";
                return res;
            }

            foreach (Meeting item in this.meetingList)
            {
                res += item.Name + ": " + item.Start_time.ToString() + " - " + item.End_time + "\n";
            }
            return res;
        }

        //Поиск встречи в списке по её названию
        public Meeting SearchName(string target_name, out int index)
        {
            foreach (Meeting item in this.meetingList)
            {
                if (item.Name == target_name)
                {
                    index = meetingList.IndexOf(item);
                    return item;
                }
            }
            index = -1;
            return null;
        }

        //Поиск всех встреч в списке, назначенных на определенную дату
        public MeetingList SearchDate(DateTime target_date)
        {
            MeetingList res = new MeetingList();
            foreach (Meeting item in this.meetingList)
            {
                if(item.Start_time.Date == target_date.Date)
                {
                    res.AddMeeting(item);
                }
            }
            return res;
        }

        //Запись информации о встречах в списке в текстовый файл
        public bool WriteToFile(string fileName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    foreach (Meeting item in this.meetingList)
                    {
                        writer.WriteLine(item.MeetingInfo() + "\n");
                    }
                }
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
    #endregion

    #region Meeting Class
    //Класс для представления встречи
    public class Meeting
    {
        
        public string Name { get; set; } //Название встречи
        public DateTime Start_time { get; set; } //Дата-время начала встречи
        public DateTime End_time { get; set; } //Примерная дата-время окончания встречи
        public int NotificationMinutes { get; set; } //Время до встречи в минутах, за которое нужно отправить уведомление
        public bool NotificationSent { get; set; }  //Было ли отправлено уведомление


        //Конструктор по умолчанию устанавливает время начала встречи на текущее,
        //а окончание встречи планирует на текущее + 1 час
        public Meeting()
        {
            Name = "Встреча";
            Start_time = DateTime.Now;
            End_time = Start_time.AddHours(1);
            NotificationSent = false;
            SetNotificationTime(30);
        }

        //Конструктор с указанием начала и окончания встречи
        public Meeting(DateTime start, DateTime end, string meeting_name)
        {
            Start_time = start;
            End_time = end;
            Name = meeting_name;
            NotificationSent = false;
            SetNotificationTime(30);
        }

        //Метод для задания количество минут до начала встречи, за которое должно быть отправлено уведомление
        //Возращает True в случае успешного внесения изменений
        //При возникновении ошибки возвращает False
        public bool SetNotificationTime(int minutes)
        {
            try
            {
                NotificationMinutes = minutes;
                NotificationSent = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Метод для задания новой даты и времени начала встречи
        //Возращает True в случае успешного внесения изменений
        //При возникновении ошибки возвращает False
        public bool SetStart(DateTime date, MeetingList src)
        {
            try
            {
                if ((date < End_time && date >= DateTime.Now))
                {
                    DateTime temp = this.Start_time;
                    Start_time = date;
                    if (CheckIntersec(src))
                    {
                        Start_time = temp;
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        //Метод для задания новой длительности встречи
        //Возращает True в случае успешного внесения изменений
        //При возникновении ошибки возвращает False
        public bool SetEnd(DateTime date, MeetingList src)
        {
            try 
            {
                if (date > Start_time && date > DateTime.Now)
                {
                    DateTime temp = this.End_time;
                    End_time = date;
                    if (CheckIntersec(src))
                    {
                        End_time = temp;
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        //Метод для задания нового названия встречи
        //Возращает True в случае успешного внесения изменений
        //При возникновении ошибки возвращает False
        public bool SetName(string name)
        {
            try
            {
                Name = name;
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Проверка перечения встречи со встречами из заданного списка
        private bool CheckIntersec(MeetingList src)
        {
            return src.MeetingIntersect(this);
        }

        //Получение информации о встрече
        //Возвращает строковое значение
        public string MeetingInfo()
        {
            return Name + "\nВстреча назначена на: " + Start_time.ToString() + "\nПримерное время окончания:" + End_time.ToString(); 
        }

    }
    #endregion
}
