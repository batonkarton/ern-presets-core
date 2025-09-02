using FluentAssertions;
using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Repositories;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.UnitTests.Helpers;
using LoopstationCompanionApi.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;

namespace LoopstationCompanionApi.UnitTests.Services;

public class PresetServiceTests
{
    [Fact]
    public async Task GetAllAsync_Maps_Summaries_And_Parses_DeviceModel()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();

        repo.Setup(r => r.GetAllSummariesAsync(1, 10))
            .ReturnsAsync(new List<PresetSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Name = "A", DeviceModel = "RC505mkII", UpdatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "B", DeviceModel = "RC505mkI",  UpdatedAt = DateTime.UtcNow },
            });

        var sut = new PresetService(repo.Object, importer.Object);

        var list = await sut.GetAllAsync(1, 10);

        list.Should().HaveCount(2);
        list[0].DeviceModel.Should().Be(DeviceModel.RC505mkII);
        list[1].DeviceModel.Should().Be(DeviceModel.RC505mkI);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PresetDto?)null);

        var sut = new PresetService(repo.Object, importer.Object);

        var result = await sut.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Maps_Dto_To_Model_And_Deserializes_Payload()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();

        var payload = JsonSerializer.Serialize(new { database = new { ifx = new { AA_LPF = new { Rate = new { _text = "5" } } } } });
        var dto = new PresetDtoBuilder().WithPayload(payload).Build();

        repo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(dto);

        var sut = new PresetService(repo.Object, importer.Object);

        var result = await sut.GetByIdAsync(dto.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(dto.Id);
        result.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Calls_Repo_And_Uses_DefaultPayloadJson()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();

        PresetDto captured = null!;
        repo.Setup(r => r.CreateAsync(It.IsAny<PresetDto>()))
            .ReturnsAsync((PresetDto d) =>
            {
                captured = d;
                return new PresetDto
                {
                    Id = Guid.NewGuid(),
                    Name = d.Name,
                    DeviceModel = d.DeviceModel,
                    UpdatedAt = d.UpdatedAt,
                    PayloadJson = d.PayloadJson
                };
            });

        var sut = new PresetService(repo.Object, importer.Object);

        var created = await sut.CreateAsync(new Preset { Name = "New", DeviceModel = DeviceModel.RC505mkI });

        captured.Should().NotBeNull();
        captured.Name.Should().Be("New");
        captured.DeviceModel.Should().Be(DeviceModel.RC505mkI.ToString());
        captured.PayloadJson.Should().NotBeNullOrWhiteSpace();

        created.Id.Should().NotBeEmpty();
        created.Name.Should().Be("New");
        created.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Returns_Null_When_Not_Found()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PresetDto?)null);

        var sut = new PresetService(repo.Object, importer.Object);

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Preset { Name = "X" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Updates_Name_Model_And_UpdatedAt()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();

        var id = Guid.NewGuid();
        var existing = new PresetDtoBuilder().WithId(id).WithName("Old").WithDevice("RC505mkII").Build();
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<PresetDto>()))
            .ReturnsAsync((Guid _, PresetDto d) => d);

        var sut = new PresetService(repo.Object, importer.Object);

        var updated = await sut.UpdateAsync(id, new Preset { Name = "NewName", DeviceModel = DeviceModel.RC505mkI });

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("NewName");
        updated.DeviceModel.Should().Be(DeviceModel.RC505mkI);

        repo.Verify(r => r.UpdateAsync(id, It.Is<PresetDto>(d => d.Name == "NewName" && d.DeviceModel == "RC505mkI")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Forwards_To_Repo()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();
        repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var sut = new PresetService(repo.Object, importer.Object);

        var ok = await sut.DeleteAsync(Guid.NewGuid());
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task ImportRc0Async_Returns_Null_When_Not_Found()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PresetDto?)null);

        var sut = new PresetService(repo.Object, importer.Object);

        var result = await sut.ImportRc0Async(Guid.NewGuid(), MockFiles.FromString("a.rc0", XmlSamples.ValidDatabaseWithIfxAndParams));
        result.Should().BeNull();
    }

    [Fact]
    public async Task ImportRc0Async_Calls_Importer_And_Updates_Payload()
    {
        var repo = new Mock<IPresetRepository>();
        var importer = new Mock<IRc0Importer>();

        var id = Guid.NewGuid();
        var existing = new PresetDtoBuilder().WithId(id).WithName("Preset").Build();
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

        var sanitized = JsonSerializer.Serialize(new { database = new { ifx = new { AA_LPF = new { Rate = new { _text = "9" } } } } });
        importer.Setup(i => i.ImportAndSanitizeAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sanitized);

        repo.Setup(r => r.UpdatePayloadAsync(id, sanitized, It.IsAny<DateTime>()))
            .ReturnsAsync(() => new PresetDtoBuilder()
                .WithId(existing.Id)
                .WithName(existing.Name)
                .WithDevice(existing.DeviceModel)
                .WithUpdatedAt(DateTime.UtcNow)
                .WithPayload(sanitized)
                .Build());

        var sut = new PresetService(repo.Object, importer.Object);

        var result = await sut.ImportRc0Async(id, MockFiles.FromString("a.rc0", XmlSamples.ValidDatabaseWithIfxAndParams));

        result.Should().NotBeNull();
        result!.Payload.Should().NotBeNull();
        importer.VerifyAll();
        repo.VerifyAll();
    }
}
