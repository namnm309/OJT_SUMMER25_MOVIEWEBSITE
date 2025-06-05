using ApplicationLayer.DTO.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfrastructureLayer.Repository;
using DomainLayer.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Application.ResponseCode;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Services
{
    public interface IAuthService
    {
        public Task<IActionResult> HandleRegister(Register req);
    }
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Users> _passwordHasher;


        public AuthService(IGenericRepository<Users> userRepo, IPasswordHasher<Users> passwordHasher, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> HandleRegister([FromBody] Register req)
        {
            var existUser = await _userRepo.FirstOrDefaultAsync(u => u.Username == req.Username || u.Email == req.Email);

            if (existUser != null)
            {
                return ErrorResp.BadRequest("Username or Email already exists.");
            }

            var user = _mapper.Map<Users>(req);

            //Hash mật khẩu trước khi lưu
            user.Password = _passwordHasher.HashPassword(user, req.Password);

            await _userRepo.CreateAsync(user);

            return SuccessResp.Ok("Register successfully.");        }
    }
}
