using Lomztein.Moduthulhu.Core.IO.Database;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public static class Consent
    {
        private static DoubleKeyJsonRepository _consent = new DoubleKeyJsonRepository("consent");

        public static void Init ()
        {
        }

        public static void AssertConsent(ulong guild, ulong user)
        {
            var queryRes = _consent.Get(guild, user.ToString(CultureInfo.InvariantCulture));
            if (!queryRes.ToObject<bool>())
            {
                throw new ConsentException("User has not given consent to storage of personal data.");
            }
        }

        public static void SetConsent (ulong guild, ulong user, bool value)
        {
            _consent.Set(guild, user.ToString(CultureInfo.InvariantCulture), value);
        }

        public static void DeleteConsent (ulong guild, ulong user)
        {
            SetConsent(guild, user, false);
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

    public class ConsentException : Exception, ISerializable
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
