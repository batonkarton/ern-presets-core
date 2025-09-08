using FluentAssertions;
using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Repositories;
using LoopstationCompanionApi.Tests.Helpers;

namespace LoopstationCompanionApi.Tests.Repositories;

public class JsonPresetRepositoryTests : IDisposable
{
    private readonly TempHostEnvironment _env;
    private readonly JsonPresetRepository _repo;

    public JsonPresetRepositoryTests()
    {
        _env = new TempHostEnvironment();
        _repo = new JsonPresetRepository(_env);
    }

    [Fact]
    public async Task Seed_Is_Created_On_First_Run()
    {
        var items = await _repo.GetAllSummariesAsync(1, 100);
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_And_GetById_Work()
    {
        const string presetName = "Created";
        const string payload = "{\"x\":1}";

        var dto = new PresetDtoBuilder()
            .WithName(presetName)
            .WithPayload(payload)
            .Build();

        var saved = await _repo.CreateAsync(dto);

        var reloaded = await _repo.GetByIdAsync(saved.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be(presetName);
        reloaded.PayloadJson.Should().Be(payload);
    }

    [Fact]
    public async Task GetAllSummaries_Pages_And_Orders()
    {
        for (int i = 0; i < 3; i++)
            await _repo.CreateAsync(new PresetDtoBuilder().WithName("P" + i).Build());

        var page1 = await _repo.GetAllSummariesAsync(1, 2);
        var page2 = await _repo.GetAllSummariesAsync(2, 2);

        page1.Should().HaveCount(2);
        page2.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_Replaces_Item_In_List()
    {
        const string beforeName = "Before";
        const string afterName = "After";
        const string payloadBefore = "{\"a\":1}";

        var created = await _repo.CreateAsync(new PresetDtoBuilder()
            .WithName(beforeName)
            .WithPayload(payloadBefore)
            .Build());

        var updated = await _repo.UpdateAsync(created.Id, new PresetDto
        {
            Name = afterName,
            DeviceModel = TestConstants.Devices.RC505mkI_Hyphen,
            PayloadJson = null
        });

        updated.Should().NotBeNull();
        updated!.Name.Should().Be(afterName);
        updated.DeviceModel.Should().Be(TestConstants.Devices.RC505mkI_Hyphen);
        updated.PayloadJson.Should().Be(payloadBefore);

        var reloaded = await _repo.GetByIdAsync(created.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be(afterName);
    }


    [Fact]
    public async Task UpdatePayloadAsync_Updates_Payload_And_UpdatedAt()
    {
        const string presetName = "PL";
        const string updatedPayload = "{\"k\":2}";

        var created = await _repo.CreateAsync(new PresetDtoBuilder().WithName(presetName).Build());

        var now = DateTime.UtcNow;
        var updated = await _repo.UpdatePayloadAsync(created.Id, updatedPayload, now);

        updated.Should().NotBeNull();
        updated!.PayloadJson.Should().Be(updatedPayload);
        updated.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));

        var reloaded = await _repo.GetByIdAsync(created.Id);
        reloaded!.PayloadJson.Should().Be(updatedPayload);
    }


    [Fact]
    public async Task DeleteAsync_Removes_Item()
    {
        var created = await _repo.CreateAsync(new PresetDtoBuilder().WithName("Del").Build());

        var ok = await _repo.DeleteAsync(created.Id);
        ok.Should().BeTrue();

        var missing = await _repo.GetByIdAsync(created.Id);
        missing.Should().BeNull();
    }

    public void Dispose() => _env.Dispose();
}
