using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Turnero.Web.Controllers;
using Turnero.Web.Hubs;
using Turnero.Web.Models;

namespace Turnero.Test;

public class TurnsAvailabilityControllerTests
{
    [Theory]
    [InlineData(-5, -5, true, true)]
    [InlineData(-6, -5, true, false)]
    [InlineData(-5, -5, false, false)]
    [InlineData(0, -5, false, true)]
    [InlineData(1, -5, false, true)]
    public async Task SaveAcceptsOnlyAnExistingHistoricalStart(
        int submittedOffset, int existingOffset, bool submitExistingId, bool expectedSuccess)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var medicId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var medics = new Mock<IGetMedicsServices>();
        medics.Setup(service => service.ExistMedic(medicId)).Returns(true);
        medics.Setup(service => service.GetAvailabilities(medicId)).ReturnsAsync([
            new MedicAvailability
            {
                Id = existingId, MedicId = medicId, Day = DayOfWeek.Monday,
                DateFrom = today.AddDays(existingOffset), DateTo = today.AddDays(10)
            }
        ]);
        var times = new Mock<IGetTimeTurnsServices>();
        times.Setup(service => service.GetCachedTimes()).ReturnsAsync([
            new TimeTurn { Id = fromId, Time = "09:00" },
            new TimeTurn { Id = toId, Time = "10:00" }
        ]);
        var controller = new TurnsController(
            null!, Mock.Of<IInsertTurnsServices>(),
            Mock.Of<IGetTurnsServices>(), Mock.Of<IGetTurnDTOServices>(),
            Mock.Of<IUpdateTurnsServices>(), medics.Object, times.Object,
            Mock.Of<IAntiforgery>(), Mock.Of<IHubContext<TurnsTableHub>>(),
            Mock.Of<ILogger<TurnsController>>());
        var model = new TurnAvailabilityViewModel
        {
            MedicId = medicId, DateFrom = today.AddDays(submittedOffset), DateTo = today.AddDays(10)
        };
        foreach (var day in model.Days)
        {
            day.IsUnavailable = true;
        }
        model.Days[0].AvailabilityId = submitExistingId ? existingId : null;
        model.Days[1].IsUnavailable = false;
        model.Days[1].TimeFromId = fromId;
        model.Days[1].TimeToId = toId;

        var result = await controller.Availability(model);

        if (expectedSuccess)
        {
            Assert.IsType<OkObjectResult>(result);
            medics.Verify(service => service.SaveAvailabilities(
                medicId, It.IsAny<IReadOnlyCollection<MedicAvailability>>(),
                It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once);
        }
        else
        {
            Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.DateFrom)));
            medics.Verify(service => service.SaveAvailabilities(
                medicId, It.IsAny<IReadOnlyCollection<MedicAvailability>>(),
                It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
        }
    }
}
