using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UI.Services;

namespace UI.Attributes
{
    public class UniquePhoneAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // Client-side validation sẽ được xử lý bởi JavaScript
            // Server-side validation sẽ được xử lý trong controller
            return true;
        }
    }

    // Remote validation attribute for phone uniqueness
    public class RemoteUniquePhoneAttribute : RemoteAttribute
    {
        public RemoteUniquePhoneAttribute() : base("CheckPhoneUnique", "Account")
        {
            ErrorMessage = "Số điện thoại này đã được sử dụng";
        }
    }
}