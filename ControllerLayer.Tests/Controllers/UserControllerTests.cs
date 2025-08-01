using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ControllerLayer.Controllers;
using ApplicationLayer.Services.UserManagement;
using ApplicationLayer.DTO.UserManagement;
using DomainLayer.Enum;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace ControllerLayer.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;
        private readonly Mock<IAuthenticationService> _mockAuthService;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthenticationService>();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices.GetService(typeof(IAuthenticationService)))
                           .Returns(_mockAuthService.Object);

            _controller = new UserController(_mockUserService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        private void SimulateUserAuthentication(Guid userId, string username, UserRole role)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(userClaims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _controller.ControllerContext.HttpContext.User = claimsPrincipal;
        }

        // Test các trường hợp đăng nhập: thành công, thất bại, và dữ liệu không hợp lệ.
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithSuccess()
        {
            var loginRequest = new LoginRequestDto { Username = "test", Password = "password" };
            var userDto = new UserDto { UserId = Guid.NewGuid(), Username = "test", Role = UserRole.Member };
            _mockUserService.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync(new LoginResultDto { Success = true, User = userDto });

            var result = await _controller.Login(loginRequest);

            result.Should().BeOfType<OkObjectResult>();
            _mockAuthService.Verify(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnOkWithFailure()
        {
            var loginRequest = new LoginRequestDto { Username = "test", Password = "wrongpassword" };
            _mockUserService.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync(new LoginResultDto { Success = false });

            var result = await _controller.Login(loginRequest);

            result.Should().BeOfType<OkObjectResult>();
            var value = (result as OkObjectResult)?.Value;
            value?.GetType().GetProperty("success")?.GetValue(value).Should().Be(false);
            _mockAuthService.Verify(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidModel_ShouldReturnBadRequest()
        {
            _controller.ModelState.AddModelError("Error", "Sample error");

            var result = await _controller.Login(new LoginRequestDto());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // Test các trường hợp đăng ký: thành công và thất bại (do service).
        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkWithSuccess()
        {
            var registerRequest = new RegisterRequestDto { Username = "newuser" };
            _mockUserService.Setup(s => s.RegisterAsync(registerRequest)).ReturnsAsync(new ServiceResponse { Success = true });

            var result = await _controller.Register(registerRequest);

            result.Should().BeOfType<OkObjectResult>();
            var value = (result as OkObjectResult)?.Value;
            value?.GetType().GetProperty("success")?.GetValue(value).Should().Be(true);
        }

        [Fact]
        public async Task Register_WhenServiceFails_ShouldReturnOkWithFailure()
        {
            var registerRequest = new RegisterRequestDto { Username = "existinguser" };
            _mockUserService.Setup(s => s.RegisterAsync(registerRequest)).ReturnsAsync(new ServiceResponse { Success = false });

            var result = await _controller.Register(registerRequest);

            result.Should().BeOfType<OkObjectResult>();
            var value = (result as OkObjectResult)?.Value;
            value?.GetType().GetProperty("success")?.GetValue(value).Should().Be(false);
        }

        // Test chức năng đăng xuất.
        [Fact]
        public async Task Logout_WhenCalled_ShouldSignOutAndReturnOk()
        {
            SimulateUserAuthentication(Guid.NewGuid(), "testuser", UserRole.Member);

            var result = await _controller.Logout();

            result.Should().BeOfType<OkObjectResult>();
            _mockAuthService.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        // Test các chức năng liên quan đến thông tin cá nhân: lấy và sửa profile.
        [Fact]
        public async Task GetProfile_WhenUserFound_ShouldReturnOkWithUser()
        {
            var userId = Guid.NewGuid();
            SimulateUserAuthentication(userId, "testuser", UserRole.Member);
            var userDto = new UserDto { UserId = userId, Username = "testuser" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(userDto);

            var result = await _controller.GetProfile();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Fact]
        public async Task GetProfile_WhenUserNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            SimulateUserAuthentication(userId, "testuser", UserRole.Member);
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDto)null);

            var result = await _controller.GetProfile();

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task EditProfile_WhenSuccessful_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            SimulateUserAuthentication(userId, "testuser", UserRole.Member);
            var editRequest = new EditProfileRequestDto { FullName = "New Name" };
            _mockUserService.Setup(s => s.EditProfileAsync(userId, editRequest)).ReturnsAsync(new ServiceResponse<UserDto> { Success = true });

            var result = await _controller.EditProfile(editRequest);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task EditProfile_WhenServiceFails_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            SimulateUserAuthentication(userId, "testuser", UserRole.Member);
            var editRequest = new EditProfileRequestDto { FullName = "New Name" };
            _mockUserService.Setup(s => s.EditProfileAsync(userId, editRequest)).ReturnsAsync(new ServiceResponse<UserDto> { Success = false });

            var result = await _controller.EditProfile(editRequest);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // Test các chức năng quản trị: thêm, sửa, xóa, thay đổi trạng thái và lấy người dùng.
        [Fact]
        public async Task CreateUser_WhenSuccessful_ShouldReturnOk()
        {
            var createDto = new UserCreateDto { Username = "admin_created" };
            _mockUserService.Setup(s => s.CreateUserAsync(createDto)).ReturnsAsync(new ServiceResponse<UserDto> { Success = true });

            var result = await _controller.CreateUser(createDto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_WhenSuccessful_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var updateDto = new UserUpdateDto { FullName = "Updated by Admin" };
            _mockUserService.Setup(s => s.UpdateUserAsync(userId, updateDto)).ReturnsAsync(new ServiceResponse<UserDto> { Success = true });

            var result = await _controller.UpdateUser(userId, updateDto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteUser_WhenSuccessful_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(new ServiceResponse { Success = true });

            var result = await _controller.DeleteUser(userId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ToggleUserStatus_WhenSuccessful_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.ToggleUserStatusAsync(userId)).ReturnsAsync(new ServiceResponse<UserDto> { Success = true });

            var result = await _controller.ToggleUserStatus(userId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserById_WhenUserFound_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var userDto = new UserDto { UserId = userId };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(userDto);

            var result = await _controller.GetUserById(userId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value?.GetType().GetProperty("data")?.GetValue(okResult.Value);
            data.Should().BeEquivalentTo(userDto);
        }

        [Fact]
        public async Task GetUserById_WhenUserNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDto)null);

            var result = await _controller.GetUserById(userId);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}