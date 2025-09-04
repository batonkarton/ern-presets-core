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
        var dto = new PresetDtoBuilder()
            .WithName("Created")
            .WithPayload("{\"x\":1}")
            .Build();

        var saved = await _repo.CreateAsync(dto);

        var reloaded = await _repo.GetByIdAsync(saved.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be("Created");
        reloaded.PayloadJson.Should().Be("{\"x\":1}");
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
        var created = await _repo.CreateAsync(new PresetDtoBuilder()
            .WithName("Before")
            .WithPayload("{\"a\":1}")
            .Build());

        var updated = await _repo.UpdateAsync(created.Id, new PresetDto
        {
            Name = "After",
            DeviceModel = "RC-505mkI",
            PayloadJson = null // preserve existing payload
        });

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("After");
        updated.DeviceModel.Should().Be("RC-505mkI");
        updated.PayloadJson.Should().Be("{\"a\":1}");

        var reloaded = await _repo.GetByIdAsync(created.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be("After");
    }

    [Fact]
    public async Task UpdatePayloadAsync_Updates_Payload_And_UpdatedAt()
    {
        var created = await _repo.CreateAsync(new PresetDtoBuilder().WithName("PL").Build());

        var now = DateTime.UtcNow;
        var updated = await _repo.UpdatePayloadAsync(created.Id, "{\"k\":2}", now);

        updated.Should().NotBeNull();
        updated!.PayloadJson.Should().Be("{\"k\":2}");
        updated.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));

        var reloaded = await _repo.GetByIdAsync(created.Id);
        reloaded!.PayloadJson.Should().Be("{\"k\":2}");
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
