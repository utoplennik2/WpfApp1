using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace HospitalApp
{
    public partial class AddPatientWindow : Window
    {
        private readonly MySqlConnection _conn;

        public AddPatientWindow(MySqlConnection conn)
        {
            InitializeComponent();
            _conn = conn;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)     //додавання
        {
            try
            {
                // Валідація введених даних
                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                {
                    MessageBox.Show("введіть прізвище!");
                    return;
                }
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                {
                    MessageBox.Show("введіть ім'я!");
                    return;
                }
                if (!BirthDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("виберіть дату народження!");
                    return;
                }
                if (!AdmissionDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("виберіть дату поступлення!");
                    return;
                }
                if (GenderComboBox.SelectedItem == null)
                {
                    MessageBox.Show("виберіть стать!");
                    return;
                }
                if (!int.TryParse(TreatmentPeriodTextBox.Text, out int treatmentPeriod) || treatmentPeriod <= 0)
                {
                    MessageBox.Show("введіть коректний період лікування (ціле число більше 0)!");
                    return;
                }
                if (DoctorComboBox.SelectedItem == null)
                {
                    MessageBox.Show("виберіть доктора!");
                    return;
                }

                string lastName = LastNameTextBox.Text;
                string firstName = FirstNameTextBox.Text;
                DateTime birthDate = BirthDatePicker.SelectedDate.Value;
                DateTime admissionDate = AdmissionDatePicker.SelectedDate.Value;

                string selectedGender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string gender = selectedGender switch                   //конвертування 
                {
                    "Чоловічий" => "Male",
                    "Жіночий" => "Female",
                    _ => throw new Exception("Невідоме значення статі")
                };

                int doctorId = int.Parse((DoctorComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());

                                                                    // SQL запрос для добавления данных
                string query = "INSERT INTO patients (last_name, first_name, birth_date, admission_date, gender, treatment_period, doctor_id) " +
                               "VALUES (@last_name, @first_name, @birth_date, @admission_date, @gender, @treatment_period, @doctor_id)";

                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@last_name", lastName);
                    cmd.Parameters.AddWithValue("@first_name", firstName);
                    cmd.Parameters.AddWithValue("@birth_date", birthDate);
                    cmd.Parameters.AddWithValue("@admission_date", admissionDate);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@treatment_period", treatmentPeriod);
                    cmd.Parameters.AddWithValue("@doctor_id", doctorId);

                                                                        // виконуємо заприт
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пацієнт доданий успішно!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Помилка при додаванні пацієнта.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)       //заповнення форми даними пацієнта з бд
        {
            try
            {
                string query = "SELECT doctor_id, CONCAT(last_name, ' ', first_name) AS full_name FROM doctors";
                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = reader["full_name"].ToString(),
                            Tag = reader["doctor_id"]
                        };
                        DoctorComboBox.Items.Add(item);
                    }
                }

                                                                // перевірка, чи є лікарі в списку
                if (DoctorComboBox.Items.Count == 0)
                {
                    MessageBox.Show("Немає доступних лікарів. Додайте хоча б одного лікаря перед додаванням пацієнта.");
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні списку докторів: " + ex.Message);
                Close();
            }
        }
    }
}