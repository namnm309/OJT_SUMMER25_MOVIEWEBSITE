using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UI.Attributes
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // Client-side validation sẽ được xử lý bởi JavaScript
            // Server-side validation sẽ được xử lý trong controller
            return true;
        }
    }

    // Remote validation attribute for email uniqueness
    public class RemoteUniqueEmailAttribute : RemoteAttribute
    {
        public RemoteUniqueEmailAttribute() : base("CheckEmailUnique", "Account")
        {
            ErrorMessage = "Email này đã được sử dụng";
        }
    }
}