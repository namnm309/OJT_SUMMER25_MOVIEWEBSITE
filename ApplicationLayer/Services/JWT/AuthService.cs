using Application.ResponseCode;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.JWT;
using ApplicationLayer.Helper;
using AutoMapper;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Core;
using InfrastructureLayer.Core.Cache;
using InfrastructureLayer.Core.Crypto;
using InfrastructureLayer.Core.JWT;
using InfrastructureLayer.Core.Mail;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.JWT
{
    public interface IAuthService
    {
        Task<IActionResult> HandleRegister(RegisterReq req);
        Task<IActionResult> HandleLogin(LoginReq req);
        Task<IActionResult> ViewUser();
        Task<IActionResult> HandleEditProfile(EditUserReq req);
        Task<IActionResult> HandleRegisterForCustomer(RegisterReq req);
        Task<IActionResult> ForgotPassword(RequestOTP req);
        Task<IActionResult> HandleVerifyOTPChangePassword(VerifyOTPChangePassword req);
    }
    public class AuthService : BaseService, IAuthService
    {
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IJwtService _jwtService;
        private readonly ICryptoService _cryptoService;
        private readonly IMailService _mailService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public AuthService(IGenericRepository<Users> userRepo, IJwtService jwtService, ICryptoService cryptoService, IMailService mailService, ICacheService cacheService, IMapper mapper, IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _cryptoService = cryptoService;
            _mailService = mailService;
            _cacheService = cacheService;
            _mapper = mapper;
            _httpCtx = httpCtx;
        }

        private string GenerateAccessTk(Guid userId, UserRole role, Guid sessionId, string email, bool isActive)
        {
            return _jwtService.GenerateToken(userId, role, sessionId, email, isActive, JwtConst.ACCESS_TOKEN_EXP);
        }

        public async Task<IActionResult> HandleRegister(RegisterReq req)
        {
            var existUser = await _userRepo.FirstOrDefaultAsync(e => e.Email == req.Email || e.Username == req.UserName);
            if (existUser != null)
                return ErrorResp.BadRequest("Account is already registerd");

            var hashedPassword = _cryptoService.HashPassword(req.Password);

            //var user = await _userRepo.FirstOrDefaultAsync(x => x.Email.Equals(req.Email));

            //if (user != null)
            //{
            //    return ErrorResp.BadRequest("Email is already taken");
            //}

            var newUser = new Users
            {
                Email = req.Email,
                Username = req.Email,
                FullName = req.FullName,
                IdentityCard = req.IdentityCard,
                Address = req.Address,
                BirthDate = req.Dob,
                Gender = req.Gender,
                Password = hashedPassword,
                Phone = req.Phone ?? "",
                IsActive = true,
                Avatar = req.Avatar
            };

            await _userRepo.CreateAsync(newUser);

            return SuccessResp.Ok(_mapper.Map<UserDto>(newUser));
        }

        public async Task<IActionResult> HandleLogin(LoginReq req)
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.Email.Equals(req.Account) || x.Username.Equals(req.Account));
            if (user == null)
                return ErrorResp.NotFound("User not found");

            if (user.Password == null || !_cryptoService.VerifyPassword(req.Password, user.Password))
            {
                return ErrorResp.BadRequest("Email or password is incorrect");
            }

            var sessionId = Guid.NewGuid();
            var accessToken = GenerateAccessTk(user.Id, user.Role, sessionId, user.Email, user.IsActive);

            var result = new LoginResp
            {
                Token = accessToken,
                Email = user.Email,
                FullName = user.FullName
            };

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> ViewUser()
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var userId = payload.UserId;

            var user = await _userRepo.FindAllAsync();
            if (user == null)
                return ErrorResp.NotFound("Not Found");

            var result = _mapper.Map<List<UserDto>>(user);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> HandleEditProfile(EditUserReq req)
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var userId = payload.UserId;

            var user = await _userRepo.FindByIdAsync(userId);
            if (user == null)
                return ErrorResp.NotFound("Not Found");

            user.FullName = req.FullName;
            user.Phone = req.Phone;
            user.Avatar = req.Avatar;
            user.Address = req.Address;

            await _userRepo.UpdateAsync(user);

            return SuccessResp.Ok(_mapper.Map<UserDto>(user));
        }

        public async Task<IActionResult> HandleRegisterForCustomer(RegisterReq req)
        {
            var existUser = await _userRepo.FirstOrDefaultAsync(e => e.Email == req.Email || e.Username == req.UserName);
            if (existUser != null)
                return ErrorResp.BadRequest("Account is already registerd");

            var password = _cryptoService.GenerateRandomPassword();
            var hashedPassword = _cryptoService.HashPassword(password);

            var newUser = new Users
            {
                Email = req.Email,
                Username = req.Email,
                FullName = req.FullName,
                IdentityCard = req.IdentityCard,
                Address = req.Address,
                BirthDate = req.Dob,
                Gender = req.Gender,
                Password = hashedPassword,
                Phone = req.Phone ?? "",
                IsActive = true,
                Avatar = req.Avatar
            };

            await _userRepo.CreateAsync(newUser);

            // ✅ Gửi thông tin tài khoản qua email
            var emailBody = $@"
                    <p>Chào {req.FullName},</p>
                    <p>Bạn đã được đăng ký tài khoản trên hệ thống <b>Cinema City</b>.</p>
                    <p><b>Email:</b> {req.Email}<br/>
                    <b>Mật khẩu:</b> {password}</p>
                    <p>Hãy đăng nhập và thay đổi mật khẩu sau khi đăng nhập lần đầu.</p>
                    <p>Trân trọng.</p>";

            await _mailService.SendEmailAsync(req.Email, "Thông tin tài khoản của bạn", emailBody);

            return SuccessResp.Ok(_mapper.Map<UserDto>(newUser));
        }

        public async Task<IActionResult> ForgotPassword(RequestOTP req)
        {
            var existAccount = await _userRepo.FirstOrDefaultAsync(a => a.Email == req.Email);
            if (existAccount == null)
                return ErrorResp.NotFound("User not found");

            var otp = StrHelper.GenerateRandomOTP();

            var redisKey = $"local:otp:{req.Email}:forgot_password";
            await _cacheService.Set(redisKey, otp, TimeSpan.FromMinutes(3));

            var subject = "Forgot Password OTP";
            var message = $"Your OTP is: {otp}";

            await _mailService.SendEmailAsync(req.Email, subject, message);

            return SuccessResp.Ok("OTP sent to your email");
        }

        public async Task<IActionResult> HandleVerifyOTPChangePassword(VerifyOTPChangePassword req)
        {
            var redisKey = $"local:otp:{req.Email}:forgot_password";

            var otp = await _cacheService.Get<string>(redisKey);

            if (otp == null)
                return ErrorResp.BadRequest("OTP is invalid");

            if (otp.Equals(req.OTP))
            {
                var user = await _userRepo.FirstOrDefaultAsync(u => u.Email.Equals(req.Email));
                if (user == null)
                    return ErrorResp.NotFound("User Not Found");

                user.Password = _cryptoService.HashPassword(req.NewPassword);

                await _userRepo.UpdateAsync(user);

                return SuccessResp.Ok("Password changed successfully");
            }
            else
            {
                return ErrorResp.BadRequest("OTP is incorrect");
            }
        }
    }
}
