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
        
        // Validate password length and complexity
        if (!newPassword) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu không được để trống');
            messageContainer.show();
            return;
        }
        
        // Kiểm tra độ dài
        if (newPassword.length < 8) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 8 ký tự');
            messageContainer.show();
            return;
        }
        
        // Kiểm tra chữ hoa
        if (!/[A-Z]/.test(newPassword)) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 1 chữ hoa');
            messageContainer.show();
            return;
        }
        
        // Kiểm tra chữ thường
        if (!/[a-z]/.test(newPassword)) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 1 chữ thường');
            messageContainer.show();
            return;
        }
        
        // Kiểm tra số
        if (!/[0-9]/.test(newPassword)) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 1 số');
            messageContainer.show();
            return;
        }
        
        // Kiểm tra ký tự đặc biệt
        if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>/?~`]/.test(newPassword)) {
            messageContainer.removeClass('alert-success').addClass('alert-danger');
            messageContainer.html('<i class="fas fa-exclamation-circle me-2"></i>Mật khẩu phải có ít nhất 1 ký tự đặc biệt');
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

    // Kiểm tra độ mạnh mật khẩu
    function checkPasswordStrength(password) {
        // Hiển thị container đánh giá độ mạnh mật khẩu
        $('#passwordStrengthContainer').show();
        
        // Kiểm tra các yêu cầu
        const hasLength = password.length >= 8;
        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasNumber = /[0-9]/.test(password);
        const hasSpecial = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>/?~`]/.test(password);
        
        // Cập nhật trạng thái các yêu cầu
        updateRequirement('length', hasLength);
        updateRequirement('uppercase', hasUpperCase);
        updateRequirement('lowercase', hasLowerCase);
        updateRequirement('number', hasNumber);
        updateRequirement('special', hasSpecial);
        
        // Tính điểm độ mạnh
        let strength = 0;
        if (hasLength) strength += 1;
        if (hasUpperCase) strength += 1;
        if (hasLowerCase) strength += 1;
        if (hasNumber) strength += 1;
        if (hasSpecial) strength += 1;
        
        // Cập nhật thanh độ mạnh và văn bản
        const strengthBar = $('#passwordStrengthBar');
        const strengthText = $('#passwordStrengthText');
        
        strengthBar.removeClass('strength-weak strength-fair strength-good strength-strong strength-very-strong');
        strengthText.removeClass('text-weak text-fair text-good text-strong text-very-strong');
        
        if (password.length === 0) {
            strengthBar.css('width', '0');
            strengthText.text('');
        } else if (strength === 1) {
            strengthBar.addClass('strength-weak');
            strengthText.addClass('text-weak');
            strengthText.text('Rất yếu');
        } else if (strength === 2) {
            strengthBar.addClass('strength-fair');
            strengthText.addClass('text-fair');
            strengthText.text('Yếu');
        } else if (strength === 3) {
            strengthBar.addClass('strength-good');
            strengthText.addClass('text-good');
            strengthText.text('Khá');
        } else if (strength === 4) {
            strengthBar.addClass('strength-strong');
            strengthText.addClass('text-strong');
            strengthText.text('Mạnh');
        } else {
            strengthBar.addClass('strength-very-strong');
            strengthText.addClass('text-very-strong');
            strengthText.text('Rất mạnh');
        }
        
        return strength;
    }
    
    // Cập nhật trạng thái của từng yêu cầu
    function updateRequirement(requirementId, isMet) {
        const requirement = $(`#${requirementId}-requirement`);
        const icon = requirement.find('.requirement-icon');
        
        if (isMet) {
            requirement.addClass('requirement-met');
            icon.removeClass('fa-circle').addClass('fa-check-circle');
        } else {
            requirement.removeClass('requirement-met');
            icon.removeClass('fa-check-circle').addClass('fa-circle');
        }
    }
    
    // Kiểm tra độ mạnh mật khẩu khi nhập
    $('#NewPassword').on('input', function() {
        const password = $(this).val();
        checkPasswordStrength(password);
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