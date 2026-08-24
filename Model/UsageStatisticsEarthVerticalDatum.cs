namespace OSDC.Drilling.EarthVerticalDatum.Model;

public sealed class UsageStatisticsEarthVerticalDatum
{
    private long restConversions_;
    private long mcpConversions_;
    private long failedConversions_;
    private long positionsConverted_;
    private long modelInfoRequests_;
    private long statisticsRequests_;

    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
    public string Scope => "process-replica";
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
}
