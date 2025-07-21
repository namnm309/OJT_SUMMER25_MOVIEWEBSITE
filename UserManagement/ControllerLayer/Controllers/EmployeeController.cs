using ApplicationLayer.DTO;
using ApplicationLayer.DTO.EmployeeManagement;
using ApplicationLayer.Services.EmployeeManagement;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeCreateDto Dto)
        {
            _logger.LogInformation("Add Employee");
            return await _employeeService.AddEmployee(Dto);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchEmployee([FromQuery] string? query)
        {
            _logger.LogInformation("Search Employee");
            return await _employeeService.SearchEmployee(query);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllEmployee()
        {
            _logger.LogInformation("Get All Employee");
            return await _employeeService.GetAllEmployee();
        }

        [HttpGet("GetAll_Pagination")]
        public async Task<IActionResult> GetAllEmployeePagination([FromQuery] PaginationReq query)
        {
            _logger.LogInformation("Get All Employee (Pagination)");
            return await _employeeService.GetAllEmployeePagination(query);
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetEmployeeById(Guid Id)
        {
            _logger.LogInformation("Get Employee By Id");
            return await _employeeService.GetEmployeeById(Id);
        }

        [HttpPatch("Update")]
        public async Task<IActionResult> UpdateEmployee(Guid Id, [FromBody] EmployeeUpdateDto Dto)
        {
            _logger.LogInformation("Update Employee");
            return await _employeeService.UpdateEmployee(Id, Dto);
        }

        [HttpDelete("Detele")]
        public async Task<IActionResult> DeleteEmployee(Guid Id)
        {
            _logger.LogInformation("Delete Employee");
            return await _employeeService.DeteleEmployee(Id);
        }

        [HttpPatch("Change-Status")]
        public async Task<IActionResult> ChangeStatusEmployee(Guid Id)
        {
            _logger.LogInformation("Change Status Employee");
            return await _employeeService.ChangeStatusEmployee(Id);
        }
    }
}
