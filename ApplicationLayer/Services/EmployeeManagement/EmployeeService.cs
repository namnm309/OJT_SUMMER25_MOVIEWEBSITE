using Application.ResponseCode;
using ApplicationLayer.DTO;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.EmployeeManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.EmployeeManagement
{
    public interface IEmployeeService
    {
        public Task<IActionResult> AddEmployee(EmployeeCreateDto Dto);
        public Task<IActionResult> SearchEmployee(string? keyword);
        public Task<IActionResult> GetAllEmployee();
        public Task<IActionResult> GetAllEmployeePagination(PaginationReq query);
        public Task<IActionResult> GetEmployeeById(Guid Id);
        public Task<IActionResult> UpdateEmployee(Guid Id, EmployeeUpdateDto Dto);
        public Task<IActionResult> DeteleEmployee(Guid Id);
        public Task<IActionResult> ChangeStatusEmployee(Guid Id);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericRepository<Employee> _employeeRepo;
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IMapper _mapper;

        public EmployeeService(IGenericRepository<Employee> employeeRepo, IGenericRepository<Users> userRepo, IMapper mapper)
        {
            _employeeRepo = employeeRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> AddEmployee(EmployeeCreateDto Dto)
        {
            if (Dto.Password != Dto.ConfirmPassword)
                return ErrorResp.BadRequest("Password and Confirm Password do not match");

            var usernameExists = await _userRepo.FirstOrDefaultAsync(u => u.Username.ToLower() == Dto.Account.ToLower());
            if (usernameExists != null)
                return ErrorResp.BadRequest("Account name is already taken");

            var emailExists = await _userRepo.FirstOrDefaultAsync(u => u.Email.ToLower() == Dto.Email.ToLower());
            if (emailExists != null)
                return ErrorResp.BadRequest("Email is already registered");

            var user = new Users
            {
                Username = Dto.Account,
                Password = BCrypt.Net.BCrypt.HashPassword(Dto.Password),
                Email = Dto.Email,
                FullName = Dto.FullName,
                IdentityCard = Dto.IdentityCard,
                Address = Dto.Address,
                Gender = Dto.Gender,
                Phone = Dto.PhoneNumber,
                BirthDate = Dto.DateOfBirth,
                Avatar = Dto.ProfileImageUrl,
                Role = UserRole.Staff,
                IsActive = true
            };

            await _userRepo.CreateAsync(user);

            var employee = _mapper.Map<Employee>(Dto);
            employee.HireDate = DateTime.UtcNow;
            employee.Position = "Staff";
            employee.Salary = 0;
            employee.UserId = user.Id.ToString();

            await _employeeRepo.CreateAsync(employee);

            return SuccessResp.Created("Employee added successfully");
        }

        public async Task<IActionResult> SearchEmployee(string? keyword)
        {
            var employee = string.IsNullOrWhiteSpace(keyword)
                ? await _employeeRepo.ListAsync()
                : await _employeeRepo.WhereAsync(e => e.FullName.ToLower().Contains(keyword.ToLower()));

            var result = _mapper.Map<List<EmployeeListDto>>(employee);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetAllEmployee()
        {
            var employee = await _employeeRepo.ListAsync();

            var result = _mapper.Map<List<EmployeeListDto>>(employee);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetAllEmployeePagination(PaginationReq query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var allEmployee = await _employeeRepo.ListAsync();
            int total = allEmployee.Count;

            var pagedEmployee = allEmployee
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            var result = _mapper.Map<List<EmployeeListDto>>(pagedEmployee);

            var response = new
            {
                Data = result,
                Total = total,
                Page = page,
                PageSize = pageSize,
            };

            return SuccessResp.Ok(response);
        }

        public async Task<IActionResult> GetEmployeeById(Guid Id)
        {
            var employee = await _employeeRepo.FindByIdAsync(Id);
            if (employee == null)
                return ErrorResp.NotFound("Employee Not Found");

            var result = _mapper.Map<EmployeeListDto>(employee);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> UpdateEmployee(Guid Id, EmployeeUpdateDto Dto)
        {
            var employee = await _employeeRepo.FindByIdAsync(Id);
            if (employee == null)
                return ErrorResp.NotFound("Employee Not Found");

            var user = await _userRepo.FindByIdAsync(Guid.Parse(employee.UserId));
            if (user == null)
                return ErrorResp.NotFound("Linked user not found");

            // Cập nhật thông tin Employee
            _mapper.Map(Dto, employee);
            employee.UpdatedAt = DateTime.UtcNow;

            // Cập nhật thông tin Users
            user.Email = Dto.Email;
            user.FullName = Dto.FullName;
            user.IdentityCard = Dto.IdentityCard;
            user.Phone = Dto.PhoneNumber;
            user.Address = Dto.Address;
            user.Gender = Dto.Gender;
            user.BirthDate = Dto.DateOfBirth;
            user.Avatar = Dto.ProfileImageUrl;

            if (!string.IsNullOrWhiteSpace(Dto.Password) || !string.IsNullOrWhiteSpace(Dto.ConfirmPassword))
            {
                if (Dto.Password != Dto.ConfirmPassword)
                    return ErrorResp.BadRequest("Password and Confirm Password do not match");

                user.Password = BCrypt.Net.BCrypt.HashPassword(Dto.Password);
            }

            await _employeeRepo.UpdateAsync(employee);

            await _userRepo.UpdateAsync(user);

            return SuccessResp.Ok("Updated Employee successfully");
        }

        public async Task<IActionResult> DeteleEmployee(Guid Id)
        {
            var employee = await _employeeRepo.FindByIdAsync(Id);
            if (employee == null)
                return ErrorResp.NotFound("Employee Not Found");

            employee.IsActive = false;

            await _employeeRepo.UpdateAsync(employee);

            return SuccessResp.Ok("Deleted Employee successfully");
        }

        public async Task<IActionResult> ChangeStatusEmployee(Guid Id)
        {
            var employee = await _employeeRepo.FindByIdAsync(Id);
            if (employee == null)
                return ErrorResp.NotFound("Employee Not Found");

            employee.IsActive = !employee.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;

            await _employeeRepo.UpdateAsync(employee);

            string status = employee.IsActive ? "Activated" : "De-Activated";
            return SuccessResp.Ok($"Employee has been {status} successfully");
        }
    }
}
