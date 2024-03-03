using Fetcher.DTOs;
using Fetcher.Persistence;
using Fetcher.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace FetcherTests.ServiceTests;

public sealed class DataFetcherServiceTests
{
    [Fact]
    public async Task FetchDataAsync_ApiServiceReturnsSuccess_CallsLogAndPayloadPersistence()
    {
        // Arrange
        var ct = new CancellationToken(false);
        var path = "this/is/path/to/file";
        var payload = new Payload(new byte[1] { 0x66 });

        var apiServiceMock = new Mock<IApiService>();
        apiServiceMock.Setup(s => s.GetDataAsync(ct))
            .ReturnsAsync(Result<Payload>.Success(payload));

        var payloadPersistenceMock = new Mock<IPayloadPersistence>();
        payloadPersistenceMock.Setup(s => s.AddPayloadAsync(payload, ct))
            .ReturnsAsync(Result<string>.Success(path));

        var logPersistenceMock = new Mock<ILogPersistence>();
        logPersistenceMock.Setup(s => s.AddLogAsync(It.IsAny<Status>(), ct))
            .ReturnsAsync(Result<Empty>.Success(new Empty()));

        var loggerMock = new Mock<ILogger<DataFetcherService>>();

        var service = new DataFetcherService(apiServiceMock.Object, logPersistenceMock.Object, payloadPersistenceMock.Object, loggerMock.Object);

        // Act
        await service.FetchDataAsync(ct);

        // Assert
        payloadPersistenceMock.Verify(v => v.AddPayloadAsync(It.IsAny<Payload>(), ct), Times.Once);
        logPersistenceMock.Verify(v => v.AddLogAsync(It.IsAny<Status>(), ct), Times.Once);
    }

    [Fact]
    public async Task FetchDataAsync_ApiServiceReturnsFailure_LogsError()
    {
        // Arrange
        var ct = new CancellationToken(false);
        var payload = new Payload(new byte[1] { 0x66 });

        var apiServiceMock = new Mock<IApiService>();
        apiServiceMock.Setup(s => s.GetDataAsync(ct))
            .ReturnsAsync(Result<Payload>.Failure(""));

        var payloadPersistenceMock = new Mock<IPayloadPersistence>();
        var logPersistenceMock = new Mock<ILogPersistence>();
        var loggerMock = new Mock<ILogger<DataFetcherService>>();

        var service = new DataFetcherService(apiServiceMock.Object, logPersistenceMock.Object, payloadPersistenceMock.Object, loggerMock.Object);

        // Act
        await service.FetchDataAsync(ct);

        // Assert
        payloadPersistenceMock.Verify(v => v.AddPayloadAsync(It.IsAny<Payload>(), ct), Times.Never);
        logPersistenceMock.Verify(v => v.AddLogAsync(It.IsAny<Status>(), ct), Times.Never);

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("There was an error while receiving data from API")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }

    [Fact]
    public async Task FetchDataAsync_ApiServiceReturnsEmptyPayload_LogsError()
    {
        // Arrange
        var ct = new CancellationToken(false);

        var apiServiceMock = new Mock<IApiService>();
        apiServiceMock.Setup(s => s.GetDataAsync(ct))
            .ReturnsAsync(Result<Payload>.Success(null));

        var payloadPersistenceMock = new Mock<IPayloadPersistence>();
        var logPersistenceMock = new Mock<ILogPersistence>();
        var loggerMock = new Mock<ILogger<DataFetcherService>>();

        var service = new DataFetcherService(apiServiceMock.Object, logPersistenceMock.Object, payloadPersistenceMock.Object, loggerMock.Object);

        // Act
        await service.FetchDataAsync(ct);

        // Assert
        payloadPersistenceMock.Verify(v => v.AddPayloadAsync(It.IsAny<Payload>(), ct), Times.Never);
        logPersistenceMock.Verify(v => v.AddLogAsync(It.IsAny<Status>(), ct), Times.Never);

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "There was an error while receiving data from API: there was no payload."),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }

    [Fact]
    public async Task FetchDataAsync_PayloadAddFails_LogsError()
    {
        // Arrange
        var ct = new CancellationToken(false);
        var payload = new Payload(new byte[1] { 0x66 });

        var apiServiceMock = new Mock<IApiService>();
        apiServiceMock.Setup(s => s.GetDataAsync(ct))
            .ReturnsAsync(Result<Payload>.Success(payload));

        var payloadPersistenceMock = new Mock<IPayloadPersistence>();
        payloadPersistenceMock.Setup(s => s.AddPayloadAsync(payload, ct))
            .ReturnsAsync(Result<string>.Failure(""));

        var logPersistenceMock = new Mock<ILogPersistence>();

        var loggerMock = new Mock<ILogger<DataFetcherService>>();

        var service = new DataFetcherService(apiServiceMock.Object, logPersistenceMock.Object, payloadPersistenceMock.Object, loggerMock.Object);

        // Act
        await service.FetchDataAsync(ct);

        // Assert
        payloadPersistenceMock.Verify(v => v.AddPayloadAsync(It.IsAny<Payload>(), ct), Times.Once);

        logPersistenceMock.Verify(v => v.AddLogAsync(It.IsAny<Status>(), ct), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("There was an error while adding payload:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchDataAsync_LogAddFails_LogsError()
    {
        // Arrange
        var ct = new CancellationToken(false);
        var path = "this/is/path/to/file";
        var payload = new Payload(new byte[1] { 0x66 });

        var apiServiceMock = new Mock<IApiService>();
        apiServiceMock.Setup(s => s.GetDataAsync(ct))
            .ReturnsAsync(Result<Payload>.Success(payload));

        var payloadPersistenceMock = new Mock<IPayloadPersistence>();
        payloadPersistenceMock.Setup(s => s.AddPayloadAsync(payload, ct))
            .ReturnsAsync(Result<string>.Success(path));

        var logPersistenceMock = new Mock<ILogPersistence>();
        logPersistenceMock.Setup(s => s.AddLogAsync(It.IsAny<Status>(), ct))
            .ReturnsAsync(Result<Empty>.Failure(""));

        var loggerMock = new Mock<ILogger<DataFetcherService>>();

        var service = new DataFetcherService(apiServiceMock.Object, logPersistenceMock.Object, payloadPersistenceMock.Object, loggerMock.Object);

        // Act
        await service.FetchDataAsync(ct);

        // Assert
        payloadPersistenceMock.Verify(v => v.AddPayloadAsync(It.IsAny<Payload>(), ct), Times.Once);

        logPersistenceMock.Verify(v => v.AddLogAsync(It.IsAny<Status>(), ct), Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("There was an error while adding logs for payload:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
