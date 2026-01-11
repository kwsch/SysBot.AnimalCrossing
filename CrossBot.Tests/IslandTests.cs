using System.Diagnostics;
using CrossBot.SysBot;
using FluentAssertions;
using NHSE.Core;
using Xunit;

namespace CrossBot.Tests;

public class IslandTests
{
    [Fact]
    public void CoordinateTest()
    {
        var bc = new BotConfig();
        var cfg = bc.FieldItemConfig;
        cfg.ValidateCoordinates().Should().Be(0);
        var bot = new Bot(bc);

        const int count = 5;
        (int x, int y) = bot.FieldItemState.GetNextInjectCoordinates(count, count);
        var canDrop = FieldItemDropper.CanFitDropped(x, y, count, count, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
        canDrop.Should().BeTrue();
    }

    [Fact]
    public void SpawnMany()
    {
        var bc = new BotConfig();
        var cfg = bc.FieldItemConfig;
        cfg.ValidateCoordinates().Should().Be(0);
        var bot = new Bot(bc);

        for (int i = 0; i < 100; i++)
        {
            const int count = 5;
            (int x, int y) = bot.FieldItemState.GetNextInjectCoordinates(count, count);
            var canDrop = FieldItemDropper.CanFitDropped(x, y, count, count, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
            canDrop.Should().BeTrue();
            Debug.WriteLine($"Dropped at {x},{y}.");
        }
    }
}