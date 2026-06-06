using System;

namespace DentalClinic
{
    public static class AuthManager
    {
        public static bool IsAuthenticated { get; set; }
        public static int? CurrentUserId { get; set; }
        public static string CurrentUserEmail { get; set; }
        public static int CurrentUserRole { get; set; }
        public static User CurrentUser { get; set; }

        public const int ROLE_ADMIN = 1;
        public const int ROLE_PATIENT = 2;
        public const int ROLE_DOCTOR = 3;

        public static void Login(int userId, string email, int roleId)
        {
            IsAuthenticated = true;
            CurrentUserId = userId;
            CurrentUserEmail = email;
            CurrentUserRole = roleId;
        }

        public static void Login(User user)
        {
            IsAuthenticated = true;
            CurrentUserId = user.Id;
            CurrentUserEmail = user.Email;
            CurrentUserRole = user.RoleId;
            CurrentUser = user;
        }

        public static void Logout()
        {
            IsAuthenticated = false;
            CurrentUserId = null;
            CurrentUserEmail = null;
            CurrentUserRole = 0;
            CurrentUser = null;
        }

        public static int? GetCurrentUserId()
        {
            return CurrentUserId;
        }

        public static bool IsAdmin()
        {
            return IsAuthenticated && CurrentUserRole == ROLE_ADMIN;
        }

        public static bool IsDoctor()
        {
            return IsAuthenticated && CurrentUserRole == ROLE_DOCTOR;
        }

        public static bool IsPatient()
        {
            return IsAuthenticated && CurrentUserRole == ROLE_PATIENT;
        }

        public static string GetRoleName()
        {
            return CurrentUserRole switch
            {
                ROLE_ADMIN => "Администратор",
                ROLE_DOCTOR => "Врач",
                ROLE_PATIENT => "Пользователь",
                _ => "Неизвестно"
            };
        }
    }
}