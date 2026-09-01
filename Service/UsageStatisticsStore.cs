using System.Text.Json;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service;

public sealed class UsageStatisticsStore : BackgroundService
{
    private readonly string filePath_;
    private readonly TimeSpan saveInterval_;
    private readonly ILogger<UsageStatisticsStore> logger_;
    private readonly SemaphoreSlim saveLock_ = new(1, 1);
    private StatisticsSnapshot lastSaved_;

    public UsageStatisticsStore(EarthVerticalDatumServiceOptions options, IHostEnvironment environment,
        ILogger<UsageStatisticsStore> logger)
    {
        filePath_ = Path.IsPathRooted(options.UsageStatisticsFile)
            ? options.UsageStatisticsFile
            : Path.Combine(environment.ContentRootPath, options.UsageStatisticsFile);
        saveInterval_ = TimeSpan.FromSeconds(options.UsageStatisticsSaveIntervalSeconds);
        logger_ = logger;
        Statistics = Load();
        lastSaved_ = StatisticsSnapshot.From(Statistics);
    }

    public UsageStatisticsEarthVerticalDatum Statistics { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(saveInterval_);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FlushAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await FlushAsync(CancellationToken.None);
    }

    internal async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        StatisticsSnapshot snapshot = StatisticsSnapshot.From(Statistics);
        if (snapshot == lastSaved_)
        {
            return;
        }

        await saveLock_.WaitAsync(cancellationToken);
        try
        {
            snapshot = StatisticsSnapshot.From(Statistics);
            if (snapshot == lastSaved_)
            {
                return;
            }

            string? directory = Path.GetDirectoryName(filePath_);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = filePath_ + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(snapshot, JsonSettings.Options), cancellationToken);
            File.Move(temporaryPath, filePath_, true);
            lastSaved_ = snapshot;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            logger_.LogError(ex, "Unable to persist Earth Vertical Datum usage statistics to {StatisticsFile}", filePath_);
        }
        finally
        {
            saveLock_.Release();
        }
    }

    private UsageStatisticsEarthVerticalDatum Load()
    {
        if (!File.Exists(filePath_))
        {
            return new UsageStatisticsEarthVerticalDatum();
        }

        try
        {
            StatisticsSnapshot? snapshot = JsonSerializer.Deserialize<StatisticsSnapshot>(File.ReadAllText(filePath_), JsonSettings.Options);
            return snapshot is null
                ? new UsageStatisticsEarthVerticalDatum()
                : UsageStatisticsEarthVerticalDatum.FromTotals(snapshot.StartedAt, snapshot.RestConversions,
                    snapshot.MCPConversions, snapshot.FailedConversions, snapshot.PositionsConverted,
                    snapshot.ModelInfoRequests, snapshot.StatisticsRequests);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            logger_.LogWarning(ex, "Unable to load Earth Vertical Datum usage statistics from {StatisticsFile}; counters will start at zero", filePath_);
            return new UsageStatisticsEarthVerticalDatum();
        }
    }

    private sealed record StatisticsSnapshot(DateTimeOffset StartedAt, long RestConversions, long MCPConversions,
        long FailedConversions, long PositionsConverted, long ModelInfoRequests, long StatisticsRequests)
    {
        public static StatisticsSnapshot From(UsageStatisticsEarthVerticalDatum value) =>
            new(value.StartedAt, value.RestConversions, value.MCPConversions, value.FailedConversions,
                value.PositionsConverted, value.ModelInfoRequests, value.StatisticsRequests);
    }
}
