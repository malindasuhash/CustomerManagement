using StateManagment.Entity;
using StateManagment.Models;
using System.Text.RegularExpressions;

namespace ContactOrchestration.Checks
{
    internal class ContactValidationCheck(Check nextCheck) : Check(nextCheck)
    {
        string pattern = @"^[a-zA-Z]+$";

        public override Task RunCheckAsync(RuntimeInfo runtimeInfo)
        {
            var contact = (Contact)runtimeInfo.Submitted;

            // FirstName contains non-alphabetic characters
            if (!Regex.IsMatch(contact.FirstName, pattern)) 
            {
                Issues.Add("First name is invalid");
            }

            return nextCheck.RunCheckAsync(runtimeInfo);
        }
    }
}
