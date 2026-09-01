namespace OSDC.Drilling.EarthVerticalDatum.Model;

public sealed class UsageStatisticsEarthVerticalDatum
{
    private long restConversions_;
    private long mcpConversions_;
    private long failedConversions_;
    private long positionsConverted_;
    private long modelInfoRequests_;
    private long statisticsRequests_;

    public UsageStatisticsEarthVerticalDatum() : this(DateTimeOffset.UtcNow, 0, 0, 0, 0, 0, 0)
    {
    }

    private UsageStatisticsEarthVerticalDatum(DateTimeOffset startedAt, long restConversions,
        long mcpConversions, long failedConversions, long positionsConverted, long modelInfoRequests,
        long statisticsRequests)
    {
        StartedAt = startedAt;
        restConversions_ = restConversions;
        mcpConversions_ = mcpConversions;
        failedConversions_ = failedConversions;
        positionsConverted_ = positionsConverted;
        modelInfoRequests_ = modelInfoRequests;
        statisticsRequests_ = statisticsRequests;
    }

    public DateTimeOffset StartedAt { get; }
    public string Scope => "persistent-service";
    public long RestConversions => Interlocked.Read(ref restConversions_);
    public long MCPConversions => Interlocked.Read(ref mcpConversions_);
    public long FailedConversions => Interlocked.Read(ref failedConversions_);
    public long PositionsConverted => Interlocked.Read(ref positionsConverted_);
    public long ModelInfoRequests => Interlocked.Read(ref modelInfoRequests_);
    public long StatisticsRequests => Interlocked.Read(ref statisticsRequests_);

    public void IncrementConversion(bool mcp, int positions)
    {
        if (mcp) Interlocked.Increment(ref mcpConversions_);
        else Interlocked.Increment(ref restConversions_);
        Interlocked.Add(ref positionsConverted_, Math.Max(positions, 0));
    }

    public void IncrementFailedConversion() => Interlocked.Increment(ref failedConversions_);
    public void IncrementModelInfo() => Interlocked.Increment(ref modelInfoRequests_);
    public void IncrementStatistics() => Interlocked.Increment(ref statisticsRequests_);

    public static UsageStatisticsEarthVerticalDatum FromTotals(DateTimeOffset startedAt, long restConversions,
        long mcpConversions, long failedConversions, long positionsConverted, long modelInfoRequests,
        long statisticsRequests) =>
        new(startedAt, restConversions, mcpConversions, failedConversions, positionsConverted,
            modelInfoRequests, statisticsRequests);
}
