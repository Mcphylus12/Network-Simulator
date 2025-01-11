using ConsoleApp;

namespace Tests;
public class RoutingTableTests
{
    [Fact]
    public void DefaultRoute()
    {
        var table = new IPRoutingTable();
        IPAddress expectedTarget = "10.0.0.1";

        table.AddEntry(new IPRoutingTable.RouteEntry("0.0.0.0", "0.0.0.0", expectedTarget, 10));

        var actual = table.Route("124.0.3.2");

        Assert.Equal(expectedTarget, actual);
    }

    [Fact]
    public void Routes()
    {
        var table = new IPRoutingTable();

        table.AddEntry(new IPRoutingTable.RouteEntry("0.0.0.0", "0.0.0.0", "10.0.0.1", 10));
        table.AddEntry(new IPRoutingTable.RouteEntry("82.5.26.3", "255.255.255.0", "10.0.0.2", 3));

        var actual = table.Route("82.5.24.1");
        var actual2 = table.Route("82.5.26.1");
        var actual3 = table.Route("82.5.26.5");

        Assert.Equal("10.0.0.1", actual);
        Assert.Equal("10.0.0.2", actual2);
        Assert.Equal("10.0.0.2", actual3);
    }
}
