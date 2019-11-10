using Lomztein.Moduthulhu.Core.IO.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public static class Consent
    {
        private static IDatabaseConnector GetConnector() => new PostgreSQLDatabaseConnector();

        public static void Init ()
        {
            GetConnector().CreateTable("consent", "CREATE TABLE consent (guild text, user text, value bool, CONSTRAINT consentguilduser UNIQUE (guild, user)");
        }

        public static void AssertConsent(ulong guild, ulong user)
        {
            var queryRes = GetConnector().ReadQuery("SELECT value FROM consent WHERE guild = @guild AND user = @user", new Dictionary<string, object>() { { "@guild", guild }, { "@user", user } });
            if (queryRes.Length == 0)
            {
                throw new ConsentException("User has not yet decided on consent to storage of personal data.");
            }

            if (queryRes.Length == 1)
            {
                bool value = (bool)queryRes[0]["value"];
                if (value == false)
                {
                    throw new ConsentException("User has decided not to consent to storage of personal data.");
                }
            }
        }

        public static void SetConsent (ulong guild, ulong user, bool value)
        {
            string query = $"INSERT INTO consent VALUES (@guild @user, @value) ON CONFLICT ON CONSTRAINT consentguilduser DO UPDATE SET value = @value WHERE consent.guild = @identifier AND consent.user = @key";
            GetConnector ().UpdateQuery(query, new Dictionary<string, object>() { { "@guild", guild }, { "@user", user }, { "@value", value } });
        }

        public static bool TryAssertConsent(ulong guild, ulong user)
        {
            try
            {
                AssertConsent(guild, user);
                return true;
            }
            catch (ConsentException)
            {
                return false;
            }
        }
    }

    public class ConsentException : Exception
    {
        public ConsentException(string message) : base(message)
        {
        }

        public ConsentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConsentException()
        {
        }
    }
}
