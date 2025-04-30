using System;
using System.Data;
using System.Windows;
using HospitalApp;
using HospitalConsoleSelect;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace HospitalWpfApp
{
    public partial class MainWindow : Window
    {
        private readonly MySqlConnection conn;

        public MainWindow()
        {                                                            //підключення за монті
            InitializeComponent();
            conn = DBMySQLUtils.GetDBConnection("localhost", 3306, "hospital_db", "monty", "some_pass");
            conn.Open();
            LoadData();
        }

        private void LoadData()                    //вивід табл 
        {
            LoadTableData("departament", DepartamentDataGrid);
            LoadTableData("doctors", DoctorsDataGrid);
            LoadTableData("patients", PatientsDataGrid);
        }

        private void LoadTableData(string tableName, DataGrid dataGrid)              //завантаження табл
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних таблиці {tableName}: {ex.Message}");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)          //додати пацієнта
        {
            try
            {
                AddPatientWindow addPatientWindow = new AddPatientWindow(conn);
                if (addPatientWindow.ShowDialog() == true)
                {
                    LoadTableData("patients", PatientsDataGrid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відкритті вікна додавання: {ex.Message}");
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)          //відредагувати пацієнта
        {
            if (PatientsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Оберіть пацієнта для редагування.");
                return;
            }

            DataRowView row = PatientsDataGrid.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Не вдалося отримати дані про пацієнта.");
                return;
            }

            int patientId = Convert.ToInt32(row["patient_id"]);

            try
            {
                EditPatientWindow editPatientWindow = new EditPatientWindow(conn, patientId);
                if (editPatientWindow.ShowDialog() == true)
                {
                    LoadTableData("patients", PatientsDataGrid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при редагуванні пацієнта: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)            //видалити
        {
            if (PatientsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Оберіть пацієнта для видалення.");
                return;
            }

            DataRowView row = PatientsDataGrid.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Не вдалося отримати дані про пацієнта.");
                return;
            }

            int patientId = Convert.ToInt32(row["patient_id"]);

            MessageBoxResult result = MessageBox.Show("Ви впевнені, що хочете видалити цього пацієнта?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM patients WHERE patient_id = @patient_id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@patient_id", patientId);
                        int affectedRows = cmd.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("пацієнт успішно видалений.");
                            LoadTableData("patients", PatientsDataGrid);
                        }
                        else
                        {
                            MessageBox.Show("не вдалося видалити пацієнта.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"помилка при видаленні пацієнта: {ex.Message}");
                }
            }
        }

    }
}