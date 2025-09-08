using FluentAssertions;
using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Helpers;
using LoopstationCompanionApi.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;

namespace LoopstationCompanionApi.Tests.Services;

public class PresetServiceTests
{
    [Fact]
    public async Task GetAllAsync_Maps_Summaries_And_Parses_DeviceModel()
    {
        const int page = 1;
        const int pageSize = 10;

        const DeviceModel firstExpectedModel = DeviceModel.RC505mkII;
        const DeviceModel secondExpectedModel = DeviceModel.RC505mkI;

        var firstDtoDeviceModel = firstExpectedModel.ToString();
        var secondDtoDeviceModel = secondExpectedModel.ToString();

        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.GetAllSummariesAsync(page, pageSize))
                .ReturnsAsync(new List<PresetSummaryDto>
                {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "A",
                    DeviceModel = firstDtoDeviceModel,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "B",
                    DeviceModel = secondDtoDeviceModel,
                    UpdatedAt = DateTime.UtcNow
                },
                }));

        var results = await sut.GetAllAsync(page, pageSize);

        results.Should().HaveCount(2);
        results[0].DeviceModel.Should().Be(firstExpectedModel);
        results[1].DeviceModel.Should().Be(secondExpectedModel);
    }


    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                                 .ReturnsAsync((PresetDto?)null));

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Maps_Dto_To_Model_And_Deserializes_Payload()
    {
        var payload = JsonSerializer.Serialize(new
        {
            database = new
            {
                ifx = new
                {
                    AA_LPF = new
                    {
                        Rate = new { _text = "5" }
                    }
                }
            }
        });

        var dto = new PresetDtoBuilder().WithPayload(payload).Build();

        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync(dto));

        var result = await sut.GetByIdAsync(dto.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(dto.Id);
        result.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Calls_Repo_And_Uses_DefaultPayloadJson()
    {
        const string presetName = "New";

        PresetDto captured = null!;
        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r
                .Setup(x => x.CreateAsync(It.IsAny<PresetDto>()))
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
                }));

        var created = await sut.CreateAsync(new Preset { Name = presetName, DeviceModel = DeviceModel.RC505mkI });

        captured.Should().NotBeNull();
        captured.Name.Should().Be(presetName);
        captured.DeviceModel.Should().Be(DeviceModel.RC505mkI.ToString());
        captured.PayloadJson.Should().NotBeNullOrWhiteSpace();

        created.Id.Should().NotBeEmpty();
        created.Name.Should().Be(presetName);
        created.Payload.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Returns_Null_When_Not_Found()
    {
        const string presetName = "X";

        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                                 .ReturnsAsync((PresetDto?)null));

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Preset { Name = presetName });

        result.Should().BeNull();
    }


    [Fact]
    public async Task UpdateAsync_Updates_Name_Model_And_UpdatedAt()
    {
        const string existingName = "Old";
        const string updatedName = "NewName";

        var id = Guid.NewGuid();
        var existing = new PresetDtoBuilder()
            .WithId(id)
            .WithName(existingName)
            .WithDevice(TestConstants.Devices.RC505mkII)
            .Build();

        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r =>
            {
                r.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(existing);
                r.Setup(x => x.UpdateAsync(id, It.IsAny<PresetDto>()))
                 .ReturnsAsync((Guid _, PresetDto d) => d);
            });

        var updated = await sut.UpdateAsync(id, new Preset { Name = updatedName, DeviceModel = DeviceModel.RC505mkI });

        updated.Should().NotBeNull();
        updated!.Name.Should().Be(updatedName);
        updated.DeviceModel.Should().Be(DeviceModel.RC505mkI);

        repo.Verify(x => x.UpdateAsync(id, It.Is<PresetDto>(d =>
            d.Name == updatedName && d.DeviceModel == TestConstants.Devices.RC505mkI)), Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_Forwards_To_Repo()
    {
        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true));

        var ok = await sut.DeleteAsync(Guid.NewGuid());

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task ImportRc0Async_Returns_Null_When_Not_Found()
    {
        var (sut, repo, _) = MockFactories.CreatePresetService(
            configureRepo: r => r.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                                 .ReturnsAsync((PresetDto?)null));

        var result = await sut.ImportRc0Async(Guid.NewGuid(),
            MockFiles.CreateFromString("a.rc0", XmlSamples.ValidDatabaseWithIfxAndParams));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ImportRc0Async_Calls_Importer_And_Updates_Payload()
    {
        const string presetName = "Preset";
        const string sanitizedRate = "9";

        var id = Guid.NewGuid();
        var existing = new PresetDtoBuilder().WithId(id).WithName(presetName).Build();

        var sanitized = JsonSerializer.Serialize(new
        {
            database = new
            {
                ifx = new
                {
                    AA_LPF = new
                    {
                        Rate = new { _text = sanitizedRate }
                    }
                }
            }
        });

        var (sut, repo, importer) = MockFactories.CreatePresetService(
            configureRepo: r =>
            {
                r.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(existing);
                r.Setup(x => x.UpdatePayloadAsync(id, sanitized, It.IsAny<DateTime>()))
                 .ReturnsAsync(() => new PresetDtoBuilder()
                    .WithId(existing.Id)
                    .WithName(existing.Name)
                    .WithDevice(existing.DeviceModel)
                    .WithUpdatedAt(DateTime.UtcNow)
                    .WithPayload(sanitized)
                    .Build());
            },
            configureImporter: i =>
            {
                i.Setup(x => x.ImportAndSanitizeAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(sanitized);
            });

        var result = await sut.ImportRc0Async(id,
            MockFiles.CreateFromString("a.rc0", XmlSamples.ValidDatabaseWithIfxAndParams));

        result.Should().NotBeNull();
        result!.Payload.Should().NotBeNull();
        importer.VerifyAll();
        repo.VerifyAll();
    }

}
