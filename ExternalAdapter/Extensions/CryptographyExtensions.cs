using StateManagment.Entity;

namespace ExternalAdapter.Extensions
{
    public static class CryptographyExtensions
    {
        public static string ToMD5Hash(this string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }

        public static string GenerateContactChecksum(Contact contact)
        {
            return $"{contact.Title}|{contact.Name}|{contact.TelephoneCode}|{contact.AltTelephone}|{contact.AltTelephoneCode}|{contact.Email}".ToMD5Hash();
        }
    }
}
