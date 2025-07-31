# Tính năng Quên Mật khẩu - Cinema City

## Tổng quan
Tính năng quên mật khẩu cho phép người dùng đặt lại mật khẩu thông qua email OTP (One-Time Password). Tính năng này bao gồm 2 bước:
1. Nhập email để nhận mã OTP
2. Nhập mã OTP và mật khẩu mới

## Cấu trúc Files

### Backend (Đã có sẵn)
- `ApplicationLayer/Services/JWT/AuthService.cs` - Xử lý logic forgot password
- `ControllerLayer/Controllers/AuthController.cs` - API endpoints
- `ApplicationLayer/DTO/JWT/RequestOTP.cs` - DTO models
- `InfrastructureLayer/Core/Mail/MailService.cs` - Gửi email

### Frontend (Mới thêm)
- `UI/Models/ForgotPasswordViewModel.cs` - ViewModels
- `UI/Controllers/AccountController.cs` - Controller actions
- `UI/Views/Account/ForgotPassword.cshtml` - Trang nhập email
- `UI/Views/Account/ResetPassword.cshtml` - Trang đặt lại mật khẩu
- `UI/wwwroot/css/forgot-password.css` - Styles
- `UI/wwwroot/js/forgot-password.js` - JavaScript logic

## Luồng hoạt động

### Bước 1: Nhập Email
1. Người dùng click "Quên mật khẩu?" từ trang Login
2. Nhập email đã đăng ký
3. Hệ thống kiểm tra email tồn tại
4. Gửi mã OTP 6 số qua email (có hiệu lực 3 phút)
5. Chuyển đến trang Reset Password

### Bước 2: Đặt lại mật khẩu
1. Hiển thị email đã nhập (readonly)
2. Nhập mã OTP 6 số
3. Nhập mật khẩu mới và xác nhận
4. Hệ thống verify OTP và cập nhật mật khẩu
5. Chuyển về trang Login

## Tính năng

### Bảo mật
- OTP có hiệu lực 3 phút
- Mã OTP 6 số ngẫu nhiên
- Validate email tồn tại trước khi gửi OTP
- Hash mật khẩu mới trước khi lưu

### UX/UI
- Giao diện responsive, modern
- Loading states khi submit
- Thông báo lỗi/thành công rõ ràng
- Auto-focus vào OTP input
- Toggle hiển thị mật khẩu
- Auto-format OTP input (chỉ nhận số)

### Email Template
- HTML email đẹp mắt
- Branding Cinema City
- Thông tin rõ ràng về OTP
- Hướng dẫn bảo mật

## API Endpoints

### Gửi OTP
```
POST /api/v1/Auth/Forgot-Password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

### Verify OTP và đổi mật khẩu
```
POST /api/v1/Auth/Verify-ChangePassword
Content-Type: application/json

{
  "email": "user@example.com",
  "otp": "123456",
  "newPassword": "newpassword123"
}
```

## Cấu hình

### Email Settings
Đảm bảo cấu hình SMTP trong `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

### Redis Cache
OTP được lưu trong Redis với key pattern: `local:otp:{email}:forgot_password`

## Testing

### Test Cases
1. **Email không tồn tại**: Hiển thị lỗi "User not found"
2. **Email hợp lệ**: Gửi OTP thành công
3. **OTP sai**: Hiển thị lỗi "OTP is incorrect"
4. **OTP hết hạn**: Hiển thị lỗi "OTP is invalid"
5. **Mật khẩu mới yếu**: Validate độ mạnh mật khẩu
6. **Xác nhận mật khẩu không khớp**: Hiển thị lỗi validation

### Manual Testing
1. Truy cập `/Account/Login`
2. Click "Quên mật khẩu?"
3. Nhập email hợp lệ
4. Kiểm tra email nhận OTP
5. Nhập OTP và mật khẩu mới
6. Đăng nhập với mật khẩu mới

## Troubleshooting

### OTP không nhận được
- Kiểm tra cấu hình SMTP
- Kiểm tra email spam folder
- Verify email tồn tại trong database

### Lỗi validation
- Kiểm tra format email
- Đảm bảo OTP đúng 6 số
- Mật khẩu mới đủ mạnh (6+ ký tự)

### Lỗi Redis
- Kiểm tra kết nối Redis
- Verify Redis service đang chạy

## Security Considerations

1. **Rate Limiting**: Nên thêm rate limiting cho API gửi OTP
2. **Logging**: Log các lần gửi OTP để monitor
3. **Audit Trail**: Ghi log khi user đổi mật khẩu
4. **Session Management**: Clear session khi đổi mật khẩu thành công

## Future Enhancements

1. **SMS OTP**: Thêm option gửi OTP qua SMS
2. **Security Questions**: Thêm câu hỏi bảo mật
3. **Account Lockout**: Lock account sau nhiều lần thử sai
4. **Email Verification**: Verify email trước khi cho phép forgot password 