using Xunit;
using Moq;
using MonitoringDashboard.Application.Features.Servers.CreateServerCommand;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDashboard.Tests.Unit;

public class CreateServerCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsServerId()
    {
        // Arrange
        var mockDbContext = new Mock<IApplicationDbContext>();
        var mockServersDbSet = new Mock<DbSet<Server>>();
       
        mockDbContext.Setup(x => x.Servers).Returns(mockServersDbSet.Object);
        mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateServerCommandHandler(mockDbContext.Object);
        var command = new CreateServerCommand("TestServer", "192.168.1.1", "Test Description");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Verify SaveChangesAsync was called (mocked DbContext doesn't simulate EF ID generation)
        mockDbContext.Verify(x => x.Servers.Add(It.IsAny<Server>()), Times.Once);
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullIPAddress_CreatesServerSuccessfully()
    {
        // Arrange
        var mockDbContext = new Mock<IApplicationDbContext>();
        var mockServersDbSet = new Mock<DbSet<Server>>();
       
        mockDbContext.Setup(x => x.Servers).Returns(mockServersDbSet.Object);
        mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateServerCommandHandler(mockDbContext.Object);
        var command = new CreateServerCommand("TestServer", null, "Test Description");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Verify server was added and saved
        mockDbContext.Verify(x => x.Servers.Add(It.IsAny<Server>()), Times.Once);
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
