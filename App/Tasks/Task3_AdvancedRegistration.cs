using System;
using System.Collections.Generic;
using Bogus;
using App.Models;

namespace App.Tasks
{
    // ������� ���������� ��� ����������� �������������
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(string message) : base(message) { }
        public UserRegistrationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    // ���������� ��� ������������������ �������������
    public class UnderageUserException : UserRegistrationException
    {
        public UnderageUserException(string message) : base(message) { }
        public UnderageUserException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UserService
    {
        private readonly DateTime _currentDate = new DateTime(2024, 1, 1);
        private readonly Faker<User> _userFaker;

        public UserService()
        {
            // ������������� Bogus ��� ��������� ��������� �������������
            _userFaker = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.DateOfBirth, f => f.Date.Past(80, DateTime.Now.AddYears(-5)));
        }

        public List<User> GenerateUsers(int count)
        {
            try
            {
                return _userFaker.Generate(count);
            }
            catch (Exception ex)
            {
                // ����������� ����� ���������� ��� ��������� � UserRegistrationException
                throw new UserRegistrationException("Failed to generate users", ex);
            }
        }

        public void RegisterUser(User user)
        {
            try
            {
                // ���������, ��� ������������ �� null
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                }

                // ��������� ���� ��������
                if (user.DateOfBirth == default)
                {
                    throw new ArgumentException("Date of birth is required", nameof(user));
                }

                // ��������� ������� ������������ ������������� ����
                int age = CalculateAge(user.DateOfBirth);

                // ��������� �������
                if (age < 14)
                {
                    throw new UnderageUserException($"User is underage. Age: {age}, minimum required: 14");
                }

                // ����� ����� �� ���� �������������� ������ �����������
                // ��������, ���������� � ���� ������ � �.�.
            }
            catch (UnderageUserException)
            {
                // ������������� UnderageUserException ��� ������������
                throw;
            }
            catch (Exception ex)
            {
                // ����������� ��� ������ ���������� � UserRegistrationException
                throw new UserRegistrationException("Failed to register user", ex);
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            try
            {
                int age = _currentDate.Year - dateOfBirth.Year;

                // ���� ���� �������� ��� �� �������� � ������� ����, �������� 1 ���
                if (dateOfBirth.Date > _currentDate.AddYears(-age))
                {
                    age--;
                }

                return age;
            }
            catch (Exception ex)
            {
                // ����������� ������ ���������� ��������
                throw new UserRegistrationException("Failed to calculate age", ex);
            }
        }
    }
}