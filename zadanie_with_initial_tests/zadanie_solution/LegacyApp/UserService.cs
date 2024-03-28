using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!CheckTheName(firstName, lastName))
                return false;

            if (!CheckTheEmail(email))
                return false;

            if (CheckTheAge(dateOfBirth))
                return false;

            var client = GetClient(clientId);
            if (client == null)
                return false;

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);
            if (user == null)
                return false;

            if (CheckCreditLimit(user, client))
                return false;

            SaveUser(user);
            return true;
        }

        private bool CheckTheName(string firstName, string lastName)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }

        private bool CheckTheEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private bool CheckTheAge(DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth) < 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) 
                age--;
            return age;
        }

        private Client GetClient(int clientId)
        {
            var clientRepository = new ClientRepository();
            return clientRepository.GetById(clientId);
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private bool CheckCreditLimit(User user, Client client)
        {
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                int creditLimit;
                using (var userCreditService = new UserCreditService())
                {
                    creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    if (client.Type == "ImportantClient")
                        creditLimit *= 2;
                }
                user.CreditLimit = creditLimit;
                user.HasCreditLimit = true;
            }

            return user.HasCreditLimit && user.CreditLimit < 500;
        }

        private void SaveUser(User user)
        {
            UserDataAccess.AddUser(user);
        }
    }
}
