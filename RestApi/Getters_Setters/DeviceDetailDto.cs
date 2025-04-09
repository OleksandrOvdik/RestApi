namespace RestApi.DTO;

public class DeviceDetailDto : DeviceDto
{
    
    public int? BatteryPercent { get; set; }
        
    public string OperatingSystem { get; set; }
        
    public string IPAddress { get; set; }
    public string NetworkName { get; set; }
    
}

