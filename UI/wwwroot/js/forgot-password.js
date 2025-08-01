// Forgot Password JavaScript
$(document).ready(function () {
    // Forgot Password Form
    $('#forgotPasswordForm').on('submit', function (e) {
        e.preventDefault();
        
        var form = $(this);
        var submitBtn = $('#submitBtn');
        var messageContainer = $('#message-container');
        
        // Disable submit button
        submitBtn.prop('disabled', true);
        submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>Đang gửi...');
        
        // Clear previous messages
        messageContainer.removeClass('alert-success alert-danger').hide();
        
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    messageContainer.removeClass('alert-danger').addClass('alert-success');
                    messageContainer.html('<i class="fas fa-check-circle me-2"></i>' + response.message);
                    messageContainer.show();
                    
                    // Redirect after 2 seconds with email as query parameter
                    setTimeout(function() {
                        var email = $('#Email').val();
                        window.location.href = '/Account/ResetPassword?email=' + encodeURIComponent(email);
                    }, 2000);
                } else {
                    messageContainer.removeClass('alert-success').addClass('alert-danger');
                    messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>' + response.message);
                    messageContainer.show();
                }
            },
            error: function (xhr, status, error) {
                var errorMessage = 'Đã xảy ra lỗi. Vui lòng thử lại.';
                
                // Try to parse error response
                try {
                    var errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        errorMessage = errorResponse.message;
                    } else if (errorResponse.Message) {
                        errorMessage = errorResponse.Message;
                    }
                } catch (e) {
                    // Silent fail - use default error message
                }
                
                messageContainer.removeClass('alert-success').addClass('alert-danger');
                messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>' + errorMessage);
                messageContainer.show();
            },
            complete: function () {
                // Re-enable submit button
                submitBtn.prop('disabled', false);
                submitBtn.html('<i class="fas fa-paper-plane me-2"></i>Gửi mã OTP');
            }
        });
    });

    // Reset Password Form
    $('#resetPasswordForm').on('submit', function (e) {
        e.preventDefault();
        
        var form = $(this);
        var submitBtn = $('#submitBtn');
        var messageContainer = $('#message-container');
        
        // Clear previous messages
        messageContainer.removeClass('alert-success alert-danger').hide();
        
        // Client-side validation
        var newPassword = $('#NewPassword').val();
        var confirmPassword = $('#ConfirmPassword').val();
        var otp = $('#OTP').val();
        var email = $('#Email').val();
        
        // Validate email
        if (!email || email.trim() === '') {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Email không được để trống');
            messageContainer.show();
            return;
        }
        
        // Validate OTP
        if (!otp || otp.length !== 6) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mã OTP phải có 6 ký tự');
            messageContainer.show();
            return;
        }
        
        // Validate password length
        if (!newPassword || newPassword.length < 6) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 6 ký tự');
            messageContainer.show();
            return;
        }
        
        // Validate password confirmation
        if (newPassword !== confirmPassword) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu xác nhận không khớp');
            messageContainer.show();
            return;
        }
        
        // Disable submit button
        submitBtn.prop('disabled', true);
        submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...');
        
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    messageContainer.removeClass('alert-danger').addClass('alert-success');
                    messageContainer.html('<i class="fas fa-check-circle me-2"></i>' + response.message);
                    messageContainer.show();
                    
                    // Redirect after 2 seconds
                    setTimeout(function() {
                        window.location.href = response.redirectUrl || '/Account/Login';
                    }, 2000);
                } else {
                    messageContainer.removeClass('alert-success').addClass('alert-danger');
                    messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>' + response.message);
                    messageContainer.show();
                }
            },
            error: function (xhr, status, error) {
                var errorMessage = 'Đã xảy ra lỗi. Vui lòng thử lại.';
                
                // Try to parse error response
                try {
                    var errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        errorMessage = errorResponse.message;
                    } else if (errorResponse.Message) {
                        errorMessage = errorResponse.Message;
                    }
                } catch (e) {
                    // Silent fail - use default error message
                }
                
                messageContainer.removeClass('alert-success').addClass('alert-danger');
                messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>' + errorMessage);
                messageContainer.show();
            },
            complete: function () {
                // Re-enable submit button
                submitBtn.prop('disabled', false);
                submitBtn.html('<i class="fas fa-save me-2"></i>Đặt lại mật khẩu');
            }
        });
    });

    // Toggle password visibility for new password
    $('#toggleNewPassword').on('click', function() {
        const passwordField = $('#NewPassword');
        const toggleIcon = $('#toggleNewPasswordIcon');
        
        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            toggleIcon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            toggleIcon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // Toggle password visibility for confirm password
    $('#toggleConfirmPassword').on('click', function() {
        const passwordField = $('#ConfirmPassword');
        const toggleIcon = $('#toggleConfirmPasswordIcon');
        
        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            toggleIcon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            toggleIcon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // Auto-format OTP input
    $('#OTP').on('input', function() {
        var value = $(this).val();
        // Remove non-numeric characters
        value = value.replace(/[^0-9]/g, '');
        // Limit to 6 digits
        value = value.substring(0, 6);
        $(this).val(value);
    });

    // Real-time password confirmation validation
    $('#NewPassword, #ConfirmPassword').on('input', function() {
        var newPassword = $('#NewPassword').val();
        var confirmPassword = $('#ConfirmPassword').val();
        var confirmPasswordField = $('#ConfirmPassword');
        var confirmPasswordError = confirmPasswordField.siblings('.text-danger');
        
        if (confirmPassword && newPassword !== confirmPassword) {
            if (confirmPasswordError.length === 0) {
                confirmPasswordField.after('<span class="text-danger">Mật khẩu xác nhận không khớp</span>');
            }
        } else {
            confirmPasswordError.remove();
        }
    });

    // Auto-focus OTP input when page loads
    if ($('#OTP').length > 0) {
        setTimeout(function() {
            $('#OTP').focus();
        }, 500);
    }

    // Show success message from TempData if exists
    if (typeof tempDataMessage !== 'undefined' && tempDataMessage) {
        var messageContainer = $('#message-container');
        messageContainer.removeClass('alert-danger').addClass('alert-success');
        messageContainer.html('<i class="fas fa-check-circle me-2"></i>' + tempDataMessage);
        messageContainer.show();
    }
}); 