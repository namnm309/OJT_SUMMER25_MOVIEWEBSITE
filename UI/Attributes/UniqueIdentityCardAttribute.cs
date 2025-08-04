using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UI.Attributes
{
    public class UniqueIdentityCardAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // Client-side validation sẽ được xử lý bởi JavaScript
            // Server-side validation sẽ được xử lý trong controller
            return true;
        }
    }

    // Remote validation attribute for identity card uniqueness
    public class RemoteUniqueIdentityCardAttribute : RemoteAttribute
    {
        public RemoteUniqueIdentityCardAttribute() : base("CheckIdentityCardUnique", "Account")
        {
            ErrorMessage = "Số CCCD này đã được sử dụng";
        }
    }
}