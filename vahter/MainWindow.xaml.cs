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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace vahter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string? path;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                string express = "";
                for (int i = 0; i < SotrudnikiList.Items.Count; i++)
                {
                    express += String.Format("('{0}')", SotrudnikiList.Items[i]);
                    if (i < SotrudnikiList.Items.Count - 1) express += ',';
                }
                string sqlExpression = String.Format("INSERT INTO timeTable (Name) VALUES {0}", express);
                try
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand(sqlExpression, connection);
                    command.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Customer ID was not returned. Account could not be created.");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            SotrudnikiList.Items.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                // Define a t-SQL query string that has a parameter for orderID.
                const string sql = "SELECT name FROM timeTable GROUP by name";
                // Create a SqlCommand object.
                SQLiteCommand sqlCommand = new SQLiteCommand(sql, connection);
                try
                {
                    connection.Open();
                    SQLiteDataReader reader = sqlCommand.ExecuteReader();

                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read()) // построчно считываем данные
                        {
                            string name = reader.GetString(0);
                            SotrudnikiList.Items.Add(name);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("The requested order could not be loaded into the form.");
                }
                finally
                {
                    // Close the connection.
                    connection.Close();
                }

            }
        }
        private void timeOutButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                string sqlExpression = "INSERT INTO timeTable (Name, timeOut) VALUES (@name, datetime('now', 'localtime'))";
                try
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand(sqlExpression, connection);

                    SQLiteParameter nameParam = new SQLiteParameter("@name", SotrudnikiList.SelectedItem.ToString());
                    command.Parameters.Add(nameParam);

                    command.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Customer ID was not returned. Account could not be created.");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void addEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (addEmployee.Text != "")
            { 
                SotrudnikiList.Items.Add(addEmployee.Text);
                addEmployee.Text = "Введите имя сотрудника ";
            }
            else MessageBox.Show("Введите имя сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void timeInButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                string sqlExpression = "INSERT INTO timeTable (Name, timeIn) VALUES (@name, datetime('now', 'localtime'))";
                try
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand(sqlExpression, connection);

                    SQLiteParameter nameParam = new SQLiteParameter("@name", SotrudnikiList.SelectedItem.ToString());
                    command.Parameters.Add(nameParam);

                    command.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Customer ID was not returned. Account could not be created.");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path2 = Directory.GetCurrentDirectory();
            path2 += "\\database.db";
           
            if (!File.Exists(path2)) // если базы данных нету, то...
            {
                SQLiteConnection.CreateFile(path2); // создать базу данных, по указанному пути содаётся пустой файл базы данных
            }
            path = $"Data Source= {path2}; Version=3;";
            using (SQLiteConnection Connect = new SQLiteConnection(@path)) // в строке указывается к какой базе подключаемся
            {
                // строка запроса, который надо будет выполнить
                string commandText = "CREATE TABLE IF NOT EXISTS timeTable ( id    INTEGER NOT NULL UNIQUE, name  TEXT NOT NULL, timeIn   TEXT NOT NULL DEFAULT 100, timeOut   TEXT NOT NULL DEFAULT 100, PRIMARY KEY(id AUTOINCREMENT))"; // создать таблицу, если её нет
                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Connect.Open(); // открыть соединение
                Command.ExecuteNonQuery(); // выполнить запрос
                Connect.Close(); // закрыть соединение
            }
            loadButton_Click(sender, e);
            datePicker1.SelectedDate = DateTime.Now; 
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                var dialogResult = MessageBox.Show(String.Format("Вы действительно хотите удалить '{0}'?", SotrudnikiList.SelectedItem), "Предупреждение", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        connection.Open();
                        const string sql = "DELETE FROM timeTable WHERE name = @name";
                        SQLiteCommand command = new SQLiteCommand(sql, connection);

                        SQLiteParameter nameParam = new SQLiteParameter("@name", SotrudnikiList.SelectedItem.ToString());
                        command.Parameters.Add(nameParam);

                        command.ExecuteNonQuery();

                        SotrudnikiList.Items.Remove(SotrudnikiList.SelectedItem);

                    }
                    catch
                    {
                        MessageBox.Show("Customer ID was not returned. Account could not be created.");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            timeInList.Items.Clear();
            timeOutList.Items.Clear();
            SotrudnikiList.Items.Clear();
        }
        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            timeInList.Items.Clear();
            timeOutList.Items.Clear();
            DateTime date = (DateTime)datePicker1.SelectedDate;
            string timeIn;
            string timeOut;
            string stringDate = date.ToShortDateString();
            using (SQLiteConnection connection = new SQLiteConnection(@path))
            {
                // Define a t-SQL query string that has a parameter for orderID.
                string sql = "SELECT strftime('%H:%M:%S', timeIn), strftime('%H:%M:%S', timeOut), strftime('%d.%m.%Y', timeIn), strftime('%d.%m.%Y', timeOut) FROM timeTable WHERE name = @name";
                // Create a SqlCommand object.
                SQLiteCommand sqlCommand = new SQLiteCommand(sql, connection);
                try
                {
                    connection.Open();
                    SQLiteParameter nameParam = new SQLiteParameter("@name", SotrudnikiList.SelectedItem.ToString());
                    sqlCommand.Parameters.Add(nameParam);

                    using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                    {

                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read()) // построчно считываем данные
                            {
                                
                                timeIn = reader.GetString(0);
                                if (timeIn != "00:00:00" && reader.GetString(2) == stringDate) timeInList.Items.Add(timeIn);
                                timeOut = reader.GetString(1);
                                if (timeOut != "00:00:00" && reader.GetString(3) == stringDate) timeOutList.Items.Add(timeOut);
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("The requested order could not be loaded into the form.");
                }
                finally
                {
                    // Close the connection.
                    connection.Close();
                }
            }
        }

        private void addEmployee_GotFocus(object sender, RoutedEventArgs e)
        {
            addEmployee.Text = "";
        }

    }
}
