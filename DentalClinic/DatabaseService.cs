using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using BCrypt.Net;

namespace DentalClinic
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<DataTable> ExecuteQueryAsync(string query, params NpgsqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(query, connection);

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                await connection.OpenAsync();
                using var adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}", ex);
            }

            return dataTable;
        }

        private async Task<object> ExecuteScalarAsync(string query, params NpgsqlParameter[] parameters)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand(query, connection);

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                await connection.OpenAsync();
                return await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetUserProfileAsync(int userId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_user_profile(@userId)";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@userId", userId) };
            return await ExecuteQueryAsync(query, parameters);
        }

        public async Task<DataTable> GetUserMedicalHistoryAsync(int userId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_user_medical_history(@userId)";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@userId", userId) };
            return await ExecuteQueryAsync(query, parameters);
        }

        public async Task<User> AuthorizeUserAsync(string email, string password)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT id, email, password_hash, role_id, first_name, middle_name, last_name, phone_number, birth_date " +
                    "FROM opyonkov_vv.users WHERE email = @email", connection);
                command.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, email);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var storedHash = reader.GetString(reader.GetOrdinal("password_hash"));

                    if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                    {
                        return new User
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            PasswordHash = storedHash,
                            RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            MiddleName = reader.IsDBNull(reader.GetOrdinal("middle_name")) ? null : reader.GetString(reader.GetOrdinal("middle_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("phone_number")) ? null : reader.GetString(reader.GetOrdinal("phone_number")),
                            BirthDate = reader.GetDateTime(reader.GetOrdinal("birth_date"))
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Authorization error: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetUserByIdAsync(int userId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_user_by_id(@userId)";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@userId", userId) };
            return await ExecuteQueryAsync(query, parameters);
        }

        public async Task<User> GetUserObjectByIdAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT id, email, role_id, first_name, middle_name, last_name, phone_number, birth_date " +
                    "FROM opyonkov_vv.users WHERE id = @id", connection);
                command.Parameters.AddWithValue("id", NpgsqlDbType.Integer, userId);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        MiddleName = reader.IsDBNull(reader.GetOrdinal("middle_name")) ? null : reader.GetString(reader.GetOrdinal("middle_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("phone_number")) ? null : reader.GetString(reader.GetOrdinal("phone_number")),
                        BirthDate = reader.GetDateTime(reader.GetOrdinal("birth_date"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}", ex);
            }
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            string query = "SELECT COUNT(1) FROM opyonkov_vv.users WHERE id = @userId";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@userId", userId) };
            var result = await ExecuteQueryAsync(query, parameters);
            return result.Rows.Count > 0 && Convert.ToInt64(result.Rows[0][0]) > 0;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            string query = "SELECT COUNT(1) FROM opyonkov_vv.users WHERE email = @email";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@email", email) };
            var result = await ExecuteQueryAsync(query, parameters);
            return result.Rows.Count > 0 && Convert.ToInt64(result.Rows[0][0]) > 0;
        }

        public async Task<int?> RegisterUserAsync(string email, string password, string firstName,
            string middleName, string lastName, string phoneNumber, DateTime birthDate)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.register_user(@_email, @_password_hash, @_role_id, @_first_name, @_middle_name, @_last_name, @_phone_number, @_birth_date)",
                    connection);

                command.Parameters.AddWithValue("_email", NpgsqlDbType.Varchar, email);
                command.Parameters.AddWithValue("_password_hash", NpgsqlDbType.Varchar, passwordHash);
                command.Parameters.AddWithValue("_role_id", NpgsqlDbType.Integer, 2);
                command.Parameters.AddWithValue("_first_name", NpgsqlDbType.Varchar, firstName);
                command.Parameters.AddWithValue("_middle_name", NpgsqlDbType.Varchar, string.IsNullOrEmpty(middleName) ? (object)DBNull.Value : middleName);
                command.Parameters.AddWithValue("_last_name", NpgsqlDbType.Varchar, lastName);
                command.Parameters.AddWithValue("_phone_number", NpgsqlDbType.Varchar, string.IsNullOrEmpty(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                command.Parameters.AddWithValue("_birth_date", NpgsqlDbType.Date, birthDate);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration error: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateAppointmentAsync(int userId, int doctorId, int serviceId, DateTime appointmentDate)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.create_appointment(@user_id, @doctor_id, @service_id, @appointment_date)",
                    connection);

                command.Parameters.AddWithValue("user_id", NpgsqlDbType.Integer, userId);
                command.Parameters.AddWithValue("doctor_id", NpgsqlDbType.Integer, doctorId);
                command.Parameters.AddWithValue("service_id", NpgsqlDbType.Integer, serviceId);
                command.Parameters.AddWithValue("appointment_date", NpgsqlDbType.Timestamp, appointmentDate);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating appointment: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetServicesAsync()
        {
            string query = "SELECT * FROM opyonkov_vv.get_services()";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetServiceByIdAsync(int serviceId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_service_by_id(@serviceId)";
            var parameters = new NpgsqlParameter[] { new NpgsqlParameter("@serviceId", serviceId) };
            return await ExecuteQueryAsync(query, parameters);
        }

        public async Task<DataTable> GetDoctorsAsync()
        {
            return await ExecuteQueryAsync("SELECT * FROM get_all_doctors()");
        }

        public async Task<int> GetUsersCountAsync()
        {
            string query = "SELECT opyonkov_vv.get_users_count()";
            var result = await ExecuteQueryAsync(query);
            return Convert.ToInt32(result.Rows[0][0]);
        }

        public async Task<int> GetAppointmentsTodayCountAsync()
        {
            string query = "SELECT opyonkov_vv.get_appointments_today_count()";
            var result = await ExecuteQueryAsync(query);
            return Convert.ToInt32(result.Rows[0][0]);
        }

        public async Task<int> GetServicesCountAsync()
        {
            string query = "SELECT opyonkov_vv.get_services_count()";
            var result = await ExecuteQueryAsync(query);
            return Convert.ToInt32(result.Rows[0][0]);
        }

        public async Task<int> GetDoctorsCountAsync()
        {
            string query = "SELECT opyonkov_vv.get_doctors_count()";
            var result = await ExecuteQueryAsync(query);
            return Convert.ToInt32(result.Rows[0][0]);
        }

        public async Task<DataTable> GetAllAppointmentsAsync()
        {
            string query = "SELECT * FROM opyonkov_vv.get_all_appointments()";
            return await ExecuteQueryAsync(query);
        }

        public async Task<bool> DeleteAppointmentAsync(int appointmentId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.delete_appointment(@appointment_id)",
                    connection);

                command.Parameters.AddWithValue("appointment_id", NpgsqlDbType.Integer, appointmentId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting appointment: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetAllUsersAsync()
        {
            string query = "SELECT * FROM opyonkov_vv.get_all_users()";
            return await ExecuteQueryAsync(query);
        }

        public async Task<bool> UpdateUserAsync(int userId, string firstName, string lastName, string middleName,
                                              string email, string phoneNumber, DateTime birthDate, int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.update_user(@user_id, @first_name, @last_name, @middle_name, @email, @phone_number, @birth_date, @role_id)",
                    connection);

                command.Parameters.AddWithValue("user_id", NpgsqlDbType.Integer, userId);
                command.Parameters.AddWithValue("first_name", NpgsqlDbType.Varchar, firstName);
                command.Parameters.AddWithValue("last_name", NpgsqlDbType.Varchar, lastName);
                command.Parameters.AddWithValue("middle_name", NpgsqlDbType.Varchar, string.IsNullOrEmpty(middleName) ? (object)DBNull.Value : middleName);
                command.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, email);
                command.Parameters.AddWithValue("phone_number", NpgsqlDbType.Varchar, string.IsNullOrEmpty(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                command.Parameters.AddWithValue("birth_date", NpgsqlDbType.Date, birthDate);
                command.Parameters.AddWithValue("role_id", NpgsqlDbType.Integer, roleId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.delete_user(@user_id)",
                    connection);

                command.Parameters.AddWithValue("user_id", NpgsqlDbType.Integer, userId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}", ex);
            }
        }

        public async Task<int> CreateUserAdminAsync(string email, string password, int roleId, string firstName,
                                                  string middleName, string lastName, string phoneNumber, DateTime birthDate)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.create_user_admin(@email, @password_hash, @role_id, @first_name, @middle_name, @last_name, @phone_number, @birth_date)",
                    connection);

                command.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, email);
                command.Parameters.AddWithValue("password_hash", NpgsqlDbType.Varchar, passwordHash);
                command.Parameters.AddWithValue("role_id", NpgsqlDbType.Integer, roleId);
                command.Parameters.AddWithValue("first_name", NpgsqlDbType.Varchar, firstName);
                command.Parameters.AddWithValue("middle_name", NpgsqlDbType.Varchar, string.IsNullOrEmpty(middleName) ? (object)DBNull.Value : middleName);
                command.Parameters.AddWithValue("last_name", NpgsqlDbType.Varchar, lastName);
                command.Parameters.AddWithValue("phone_number", NpgsqlDbType.Varchar, string.IsNullOrEmpty(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                command.Parameters.AddWithValue("birth_date", NpgsqlDbType.Date, birthDate);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating user: {ex.Message}", ex);
            }
        }

        public async Task<int> CreateServiceAsync(string name, string description, decimal price)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.create_service(@name, @description, @price)",
                    connection);

                command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                command.Parameters.AddWithValue("description", NpgsqlDbType.Text, string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("price", NpgsqlDbType.Numeric, price);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateServiceAsync(int serviceId, string name, string description, decimal price)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.update_service(@service_id, @name, @description, @price)",
                    connection);

                command.Parameters.AddWithValue("service_id", NpgsqlDbType.Integer, serviceId);
                command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                command.Parameters.AddWithValue("description", NpgsqlDbType.Text, string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("price", NpgsqlDbType.Numeric, price);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteServiceAsync(int serviceId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(
                    "SELECT opyonkov_vv.delete_service(@service_id)",
                    connection);

                command.Parameters.AddWithValue("service_id", NpgsqlDbType.Integer, serviceId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting service: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsAppointmentTimeAvailableAsync(int doctorId, DateTime appointmentDateTime)
        {
            string query = "SELECT opyonkov_vv.is_appointment_time_available(@doctorId, @appointmentDateTime)";

            var parameters = new[]
            {
                new NpgsqlParameter("@doctorId", doctorId),
                new NpgsqlParameter("@appointmentDateTime", appointmentDateTime)
            };

            var result = await ExecuteScalarAsync(query, parameters);
            return result != null && (bool)result;
        }

        public async Task<int> GetDoctorIdByUserIdAsync(int userId)
        {
            string query = "SELECT opyonkov_vv.get_doctor_id_by_user_id(@userId)";
            var parameter = new NpgsqlParameter("@userId", userId);

            var result = await ExecuteScalarAsync(query, parameter);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public async Task<DataTable> GetDoctorByIdAsync(int doctorId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_doctor_by_id(@doctorId)";
            var parameter = new NpgsqlParameter("@doctorId", doctorId);

            return await ExecuteQueryAsync(query, parameter);
        }

        public async Task<int> CreateDoctorAsync(string firstName, string lastName, string middleName,
            string specialization, int experience, string education)
        {
            var result = await ExecuteScalarAsync(
                "SELECT opyonkov_vv.create_doctor(@firstName, @lastName, @middleName, @specialization, @experience, @education)",
                new NpgsqlParameter("@firstName", firstName),
                new NpgsqlParameter("@lastName", lastName),
                new NpgsqlParameter("@middleName", middleName ?? (object)DBNull.Value),
                new NpgsqlParameter("@specialization", specialization),
                new NpgsqlParameter("@experience", experience),
                new NpgsqlParameter("@education", education ?? (object)DBNull.Value)
            );
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateDoctorAsync(int doctorId, string firstName, string lastName, string middleName,
            string specialization, int experience, string education)
        {
            var result = await ExecuteScalarAsync(
                "SELECT opyonkov_vv.update_doctor(@doctorId, @firstName, @lastName, @middleName, @specialization, @experience, @education)",
                new NpgsqlParameter("@doctorId", doctorId),
                new NpgsqlParameter("@firstName", firstName),
                new NpgsqlParameter("@lastName", lastName),
                new NpgsqlParameter("@middleName", middleName ?? (object)DBNull.Value),
                new NpgsqlParameter("@specialization", specialization),
                new NpgsqlParameter("@experience", experience),
                new NpgsqlParameter("@education", education ?? (object)DBNull.Value)
            );
            return result != null && Convert.ToBoolean(result);
        }

        public async Task<int> CreateMedicalRecordAsync(int userId, int doctorId, DateTime visitDate, string diagnosis, string treatment)
        {
            string query = "SELECT opyonkov_vv.create_medical_record(@userId, @doctorId, @visitDate, @diagnosis, @treatment)";

            var parameters = new[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@doctorId", doctorId),
                new NpgsqlParameter("@visitDate", visitDate),
                new NpgsqlParameter("@diagnosis", diagnosis),
                new NpgsqlParameter("@treatment", treatment ?? (object)DBNull.Value)
            };

            var result = await ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public async Task<bool> DeleteDoctorAsync(int doctorId)
        {
            var result = await ExecuteScalarAsync("SELECT opyonkov_vv.delete_doctor(@doctorId)",
                new NpgsqlParameter("@doctorId", doctorId));
            return result != null && Convert.ToBoolean(result);
        }

        public async Task<DataTable> GetAllAppointmentsAdminAsync()
        {
            return await ExecuteQueryAsync("SELECT * FROM opyonkov_vv.get_all_appointments_admin()");
        }
        public async Task<bool> UpdateUserNameAsync(int userId, string firstName, string lastName, string middleName = null)
        {
            var result = await ExecuteScalarAsync(
                "SELECT update_user_name(@user_id, @first_name, @last_name, @middle_name)",
                new NpgsqlParameter("@user_id", userId),
                new NpgsqlParameter("@first_name", firstName),
                new NpgsqlParameter("@last_name", lastName),
                new NpgsqlParameter("@middle_name", (object)middleName ?? DBNull.Value)
            );
            return (bool)result;
        }

        public async Task<bool> UpdateUserBirthDateAsync(int userId, DateTime birthDate)
        {
            var result = await ExecuteScalarAsync(
                "SELECT update_user_birth_date(@user_id, @birth_date)",
                new NpgsqlParameter("@user_id", userId),
                new NpgsqlParameter("@birth_date", birthDate)
            );
            return (bool)result;
        }

        public async Task<int> UpdateUserEmailAsync(int userId, string email)
        {
            var result = await ExecuteScalarAsync(
                "SELECT update_user_email(@user_id, @email)",
                new NpgsqlParameter("@user_id", userId),
                new NpgsqlParameter("@email", email)
            );
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUserPhoneAsync(int userId, string phone)
        {
            var result = await ExecuteScalarAsync(
                "SELECT update_user_phone(@user_id, @phone)",
                new NpgsqlParameter("@user_id", userId),
                new NpgsqlParameter("@phone", phone)
            );
            return (bool)result;
        }
        public async Task<DataTable> GetDoctorAppointmentsForTodayAsync(int doctorId)
        {
            string query = "SELECT * FROM opyonkov_vv.get_doctor_appointments_today(@doctorId)";

            var parameter = new NpgsqlParameter("@doctorId", doctorId);

            return await ExecuteQueryAsync(query, parameter);
        }

        public async Task<int> CreateAppointmentAdminAsync(int patientId, int doctorId, int serviceId, DateTime appointmentDate)
        {
            var result = await ExecuteScalarAsync(
                "SELECT opyonkov_vv.create_appointment_admin(@patientId, @doctorId, @serviceId, @appointmentDate)",
                new NpgsqlParameter("@patientId", patientId),
                new NpgsqlParameter("@doctorId", doctorId),
                new NpgsqlParameter("@serviceId", serviceId),
                new NpgsqlParameter("@appointmentDate", appointmentDate)
            );
            return Convert.ToInt32(result);
        }

        public async Task<DataTable> GetPatientsAsync()
        {
            return await ExecuteQueryAsync("SELECT id, first_name, last_name, middle_name FROM opyonkov_vv.users WHERE role_id = 2 ORDER BY last_name, first_name");
        }

        public async Task<DataTable> GetAllDoctorsAsync()
        {
            return await ExecuteQueryAsync("SELECT * FROM opyonkov_vv.get_doctors()");
        }
    }
}