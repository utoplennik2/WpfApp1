using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace HospitalApp
{
    public partial class EditPatientWindow : Window
    {
        private readonly MySqlConnection _conn;
        private readonly int _patientId;

        public EditPatientWindow(MySqlConnection conn, int patientId)
        {
            InitializeComponent();
            _conn = conn;
            _patientId = patientId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)        // завантажуємо лікарів 
        {
            try
            {
                string queryDoctors = "SELECT doctor_id, CONCAT(last_name, ' ', first_name) AS full_name FROM doctors";
                using (MySqlCommand cmd = new MySqlCommand(queryDoctors, _conn))
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

                                                                         // Перевірка, чи є лікарі в списку
                if (DoctorComboBox.Items.Count == 0)
                {
                    MessageBox.Show("додайте хоча б одного лікаря перед редагуванням.");
                    Close();
                    return;
                }

                                                                             // Завантажуємо дані обраного пацієнта
                string queryPatient = "SELECT * FROM patients WHERE patient_id = @patient_id";
                using (MySqlCommand cmd = new MySqlCommand(queryPatient, _conn))
                {
                    cmd.Parameters.AddWithValue("@patient_id", _patientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            LastNameTextBox.Text = reader["last_name"].ToString();
                            FirstNameTextBox.Text = reader["first_name"].ToString();
                            BirthDatePicker.SelectedDate = Convert.ToDateTime(reader["birth_date"]);
                            AdmissionDatePicker.SelectedDate = Convert.ToDateTime(reader["admission_date"]);
                            TreatmentPeriodTextBox.Text = reader["treatment_period"].ToString();

                            string gender = reader["gender"].ToString();
                            foreach (ComboBoxItem item in GenderComboBox.Items)
                            {
                                string displayGender = item.Content.ToString() == "чоловічий" ? "мale" : "female";
                                if (displayGender == gender)
                                {
                                    GenderComboBox.SelectedItem = item;
                                    break;
                                }
                            }

                            int doctorId = Convert.ToInt32(reader["doctor_id"]);
                            foreach (ComboBoxItem item in DoctorComboBox.Items)
                            {
                                if (Convert.ToInt32(item.Tag) == doctorId)
                                {
                                    DoctorComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("пацієнта не знайдено.");
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("помилка при завантаженні даних: " + ex.Message);
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)             //збереження даних 
        {
            try
            {
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

                                                                            //отримуємо дані з форми
                string lastName = LastNameTextBox.Text;
                string firstName = FirstNameTextBox.Text;
                DateTime birthDate = BirthDatePicker.SelectedDate.Value;
                DateTime admissionDate = AdmissionDatePicker.SelectedDate.Value;

                string selectedGender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string gender = selectedGender switch
                {
                    "Чоловічий" => "Male",
                    "Жіночий" => "Female",
                    _ => throw new Exception("невідоме значення статі")
                };

                int doctorId = int.Parse((DoctorComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());

                                                                                //-запит для оновлення даних
                string query = @"
                    UPDATE patients 
                    SET last_name = @last_name, 
                        first_name = @first_name, 
                        birth_date = @birth_date, 
                        admission_date = @admission_date, 
                        gender = @gender, 
                        treatment_period = @treatment_period, 
                        doctor_id = @doctor_id 
                    WHERE patient_id = @patient_id";

                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@last_name", lastName);
                    cmd.Parameters.AddWithValue("@first_name", firstName);
                    cmd.Parameters.AddWithValue("@birth_date", birthDate);
                    cmd.Parameters.AddWithValue("@admission_date", admissionDate);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@treatment_period", treatmentPeriod);
                    cmd.Parameters.AddWithValue("@doctor_id", doctorId);
                    cmd.Parameters.AddWithValue("@patient_id", _patientId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("пацієнт успішно оновлений!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("помилка при оновленні пацієнта.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("помилка: " + ex.Message);
            }
        }
    }
}