namespace SmartXDashboard.Models
{
    public enum ZoneLocation
    {
        ZoneA_Environmental,
        ZoneB_PowerGrid,
        ZoneC_ActuatorControl,
        ZoneD_IngestionEdge
    }

    public enum SensorCategory
    {
        Environmental,
        Electrical,
        Mechanical
    }

    public enum NodeStatus
    {
        Unprovisioned,
        Active,
        Warning,
        Critical
    }
}