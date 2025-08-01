using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

public class BookingApiIntegrationTests : IClassFixture<WebApplicationFactory<ControllerLayer.Program>>
{
    private readonly HttpClient _client;

    public BookingApiIntegrationTests(WebApplicationFactory<ControllerLayer.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DatVe_BangAPI_ThanhCong()
    {
        // Chuẩn bị dữ liệu booking (thay các GUID bằng dữ liệu hợp lệ trong database test)
        var bookingRequest = new
        {
            ShowTimeId = "GUID_SHOWTIME",
            SeatIds = new[] { "GUID_SEAT1", "GUID_SEAT2" },
            MemberId = "GUID_USER",
            PointsUsed = 0,
            ConvertedTickets = 0
        };
        var content = new StringContent(JsonConvert.SerializeObject(bookingRequest), Encoding.UTF8, "application/json");

        // Gọi API đặt vé
        var response = await _client.PostAsync("/api/v1/booking-ticket/confirm-booking-with-score", content);

        // Kiểm tra kết quả trả về
        response.EnsureSuccessStatusCode(); // Đảm bảo HTTP status code là 2xx
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("BookingCode", responseString); // Kiểm tra response có mã booking
    }
} 