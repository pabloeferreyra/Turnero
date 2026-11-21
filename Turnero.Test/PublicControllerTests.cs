using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using Moq;
using Turnero.Application.Services.TurnsServices;
using Turnero.Application.Services.MedicServices;
using Turnero.Domain.Entities;
using Turnero.Web.Controllers;
using Turnero.Web.Models;
using Xunit;

namespace Turnero.Test;

public class PublicControllerTests
{
    private static PublicController CreateController(IPublicTurnBookingService bookingService)
    {
        return new PublicController(
            bookingService,
            Mock.Of<IInsertTurnsServices>(),
            Mock.Of<IGetTurnsServices>(),
            Mock.Of<IGetMedicsServices>(),
            Mock.Of<IGetTimeTurnsServices>(),
            Mock.Of<ILogger<PublicController>>());
    }

    [Theory]
    [InlineData(nameof(PublicController.GetMedics))]
    [InlineData(nameof(PublicController.GetAvailability))]
    public void GetEndpointsAllowAnonymousRequestsFromPublicClient(string actionName)
    {
        var action = typeof(PublicController).GetMethod(actionName)!;
        Assert.NotEmpty(action.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        var cors = Assert.Single(action.GetCustomAttributes(typeof(EnableCorsAttribute), true));
        Assert.Equal("PublicTurnClient", Assert.IsType<EnableCorsAttribute>(cors).PolicyName);
    }

    [Fact]
    public async Task GetMedicsReturnsPublicDataInsteadOfTask()
    {
        var medicId = Guid.NewGuid();
        var booking = new Mock<IPublicTurnBookingService>();
        booking.Setup(service => service.GetMedicsAsync())
            .ReturnsAsync([new MedicDto { Id = medicId, Name = "Profesional" }]);
        var controller = CreateController(booking.Object);

        var response = Assert.IsType<OkObjectResult>(await controller.GetMedics());
        var medics = Assert.IsAssignableFrom<IEnumerable<PublicMedicResponse>>(response.Value);
        Assert.Equal(new PublicMedicResponse(medicId, "Profesional"), Assert.Single(medics));
    }

    [Fact]
    public async Task GetAvailabilityValidatesAndReturnsPublicTimes()
    {
        var medicId = Guid.NewGuid();
        var timeId = Guid.NewGuid();
        var date = new DateOnly(2030, 1, 7);
        var booking = new Mock<IPublicTurnBookingService>();
        booking.Setup(service => service.GetAvailableTimesAsync(medicId, date))
            .ReturnsAsync([new TimeTurn { Id = timeId, Time = "09:00" }]);
        var controller = CreateController(booking.Object);

        Assert.IsType<BadRequestObjectResult>(await controller.GetAvailability(Guid.Empty, date));
        var response = Assert.IsType<OkObjectResult>(await controller.GetAvailability(medicId, date));
        var times = Assert.IsAssignableFrom<IEnumerable<PublicTimeResponse>>(response.Value);
        Assert.Equal(new PublicTimeResponse(timeId, "09:00"), Assert.Single(times));
    }

    [Theory]
    [InlineData(PublicTurnBookingResult.Created, typeof(OkObjectResult))]
    [InlineData(PublicTurnBookingResult.InvalidTime, typeof(BadRequestObjectResult))]
    [InlineData(PublicTurnBookingResult.Unavailable, typeof(ConflictObjectResult))]
    public async Task CreateReturnsMatchingStatus(PublicTurnBookingResult result, Type expected)
    {
        var booking = new Mock<IPublicTurnBookingService>();
        booking.Setup(service => service.CreateAsync(It.IsAny<Turn>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(result);
        var controller = CreateController(booking.Object);
        var request = new CreatePublicTurnRequest
        {
            Name = " Persona ",
            Dni = 12345678,
            MedicId = Guid.NewGuid(),
            Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            TimeId = Guid.NewGuid()
        };

        var response = await controller.Create(request);

        Assert.IsType(expected, response);
        booking.Verify(service => service.CreateAsync(
            It.Is<Turn>(turn => turn.Name == "Persona" && !turn.Accessed),
            It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task CreateRejectsInvalidModelAndPastDate()
    {
        var booking = new Mock<IPublicTurnBookingService>();
        var controller = CreateController(booking.Object);
        controller.ModelState.AddModelError("Dni", "invalid");

        Assert.IsType<BadRequestObjectResult>(await controller.Create(new CreatePublicTurnRequest()));
        controller.ModelState.Clear();
        Assert.IsType<BadRequestObjectResult>(await controller.Create(new CreatePublicTurnRequest
        {
            Name = "Persona", Dni = 12345678, MedicId = Guid.NewGuid(), TimeId = Guid.NewGuid(),
            Date = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")
        }));
        booking.Verify(service => service.CreateAsync(It.IsAny<Turn>(), It.IsAny<DateOnly>()), Times.Never);
    }

}
