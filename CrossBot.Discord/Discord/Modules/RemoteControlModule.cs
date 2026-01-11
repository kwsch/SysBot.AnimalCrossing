using Discord.Commands;
using SysBot.Base;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrossBot.Discord;
using CrossBot.SysBot;

namespace SysBot.Pokemon.Discord;

// ReSharper disable once UnusedType.Global
[Summary("Remotely controls a bot.")]
public class RemoteControlModule : ModuleBase<SocketCommandContext>
{
    private static Bot Bot => Globals.Bot;

    [Command("click")]
    [Summary("Clicks the specified button.")]
    [RequireSudo]
    public async Task ClickAsync(SwitchButton b)
    {
        await ClickAsyncImpl(b).ConfigureAwait(false);
    }

    [Command("setStick")]
    [Summary("Sets the stick to the specified position.")]
    [RequireSudo]
    public async Task SetStickAsync(SwitchStick s, short x, short y, ushort ms = 1_000)
    {
        await SetStickAsyncImpl(s, x, y, ms).ConfigureAwait(false);
    }

    private async Task ClickAsyncImpl(SwitchButton button)
    {
        var b = Globals.Bot;
        await b.Connection.SendAsync(SwitchCommand.Click(button, b.UseCRLF), CancellationToken.None).ConfigureAwait(false);
        await ReplyAsync($"{b.Connection.Name} has performed: {button}").ConfigureAwait(false);
    }

    private async Task SetStickAsyncImpl(SwitchStick s, short x, short y, ushort ms)
    {
        if (!Enum.IsDefined(typeof(SwitchStick), s))
        {
            await ReplyAsync($"Unknown stick: {s}").ConfigureAwait(false);
            return;
        }

        var b = Bot;
        await b.Connection.SendAsync(SwitchCommand.SetStick(s, x, y, b.UseCRLF), CancellationToken.None).ConfigureAwait(false);
        await ReplyAsync($"{b.Connection.Name} has performed: {s}").ConfigureAwait(false);
        await Task.Delay(ms).ConfigureAwait(false);
        await b.Connection.SendAsync(SwitchCommand.ResetStick(s, b.UseCRLF), CancellationToken.None).ConfigureAwait(false);
        await ReplyAsync($"{b.Connection.Name} has reset the stick position.").ConfigureAwait(false);
    }

    [Command("readMemory")]
    [Summary("Reads memory from the requested offset and writes it to the bot directory.")]
    [RequireSudo]
    public async Task ReadAsync(uint offset, int length)
    {
        var b = Bot;
        var result = await b.Connection.ReadBytesAsync(offset, length, CancellationToken.None).ConfigureAwait(false);
        await File.WriteAllBytesAsync("dump.bin", result);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("writeMemory")]
    [Summary("Writes memory to the requested offset.")]
    [RequireSudo]
    public async Task WriteAsync(uint offset, string hex)
    {
        var b = Bot;
        var data = GetBytesFromHexString(hex.Replace(" ", ""));
        await b.Connection.WriteBytesAsync(data, offset, CancellationToken.None).ConfigureAwait(false);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("getOffset"), Alias("eval")]
    [Summary("Evaluates a pointer request to offset.")]
    [RequireSudo]
    public async Task GetOffset(string expression)
    {
        var b = Bot;
        var ofs = PointerUtil.GetPointerAddressFromExpression((ISwitchConnectionAsync)b.Connection, expression, CancellationToken.None);
        await ReplyAsync($"Result: {ofs:X8}").ConfigureAwait(false);
    }

    private static byte[] GetBytesFromHexString(string seed)
    {
        return Enumerable.Range(0, seed.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(seed.Substring(x, 2), 16))
            .Reverse().ToArray();
    }
}