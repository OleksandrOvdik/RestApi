using Microsoft.AspNetCore.Mvc;
using Electronics;
using RestApi.DTO;
using Swashbuckle.AspNetCore.Annotations;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private static List<Device> _devices = new List<Device>();
        private const int MaxDevices = 15;

        public DeviceController()
        {
            if (_devices.Count == 0)
            {
                _devices.Add(new SmartWatches("SW-1", "Apple Watch SE2", 85));
                _devices.Add(new Personal_Computer("P-1", "LinuxPC", "Linux Mint"));
                _devices.Add(new Embedded_devices("ED-1", "RaspberryPi", "192.168.1.5", "MD Ltd Network"));
            }
        }

        [HttpGet("getAllShits")]
        public ActionResult<IEnumerable<object>> getAllShits()
        {
            var deviceDtos = new List<object>();

            foreach (var device in _devices)
            {
                if (device is SmartWatches smartWatches)
                {
                    deviceDtos.Add(new
                    {
                        ID = device.ID,
                        Name = device.Name,
                        DeviceType = "SW",
                        BatteryPercent = smartWatches.batteryPercent
                    });
                }
                else if (device is Personal_Computer personalComputer)
                {
                    deviceDtos.Add(new
                    {
                        ID = device.ID,
                        Name = device.Name,
                        DeviceType = "P",
                        OperatingSystem = personalComputer.OperatingSystem
                    });
                }
                else if (device is Embedded_devices embeddedDevices)
                {
                    deviceDtos.Add(new
                    {
                        ID = device.ID,
                        Name = device.Name,
                        DeviceType = "ED",
                        IPAddress = embeddedDevices.IPAddress,
                        NetworkName = embeddedDevices.NetworkName   
                    });
                }
            } return Ok(deviceDtos);
        }

        [HttpGet("GetShitByID")]
        public ActionResult<DeviceDetailDto> getShitByID(string id)
        {
            var device = _devices.Find(d => d.ID == id);
            
            if (device == null)
            {
                return NotFound();
            }

            var detailDto = new DeviceDetailDto
            {
                ID = device.ID,
                Name = device.Name,
                DeviceType = GetDeviceType(device)
            };

            if (device is SmartWatches smartWatch)
            {
                detailDto.BatteryPercent = smartWatch.batteryPercent;
            }
            else if (device is Personal_Computer pc)
            {
                detailDto.OperatingSystem = pc.OperatingSystem;
            }
            else if (device is Embedded_devices embedded)
            {
                detailDto.IPAddress = embedded.IPAddress;
                detailDto.NetworkName = embedded.NetworkName;
            }

            return Ok(detailDto);
        }
    
        [HttpPost("CreateShit")]
        public ActionResult<DeviceDetailDto> CreateShit(CreateDeviceDto createDto)
        {
            if (_devices.Count >= MaxDevices)
            {
                return BadRequest("Send me a 100 dollars, if you want to create a new device, my number -> 787 325 044");
            }

            try
            {
                Device newDevice;

                switch (createDto.DeviceType.ToLower())
                {
                    case "sw":
                        if (!createDto.BatteryPercent.HasValue)
                            return BadRequest("Input a baterry %");
                            
                        newDevice = new SmartWatches(
                            createDto.ID, 
                            createDto.Name, 
                            createDto.BatteryPercent.Value);
                        break;
                        
                    case "pc":
                        newDevice = new Personal_Computer(
                            createDto.ID,
                            createDto.Name, 
                            createDto.OperatingSystem);
                        break;
                        
                    case "ed":
                        if (string.IsNullOrEmpty(createDto.IPAddress) || string.IsNullOrEmpty(createDto.NetworkName))
                            return BadRequest("Input IP Address and Network Name");
                            
                        newDevice = new Embedded_devices(
                            createDto.ID,
                            createDto.Name,
                            createDto.IPAddress,
                            createDto.NetworkName);
                        break;
                        
                    default:
                        return BadRequest("NOT GOOD DEVICE TYPE");
                }

                if (_devices.Any(d => d.ID == newDevice.ID))
                {
                    return BadRequest($"Shit with ID {newDevice.ID} already exists");
                }

                _devices.Add(newDevice);

                var detailDto = CreateDetailedDto(newDevice);
                
                return CreatedAtAction(nameof(getShitByID), new { id = detailDto.ID }, detailDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateShitByID")]
        public IActionResult UdpateShit(string id, UpdateDeviceDto updateDto)
        {
            if (id != updateDto.ID)
            {
                return BadRequest("Input correct id");
            }

            var deviceIndex = _devices.FindIndex(d => d.ID == id);
            
            if (deviceIndex == -1)
            {
                return NotFound();
            }

            var existingDevice = _devices[deviceIndex];

            try
            {
                Device updatedDevice;

                if (existingDevice is SmartWatches)
                {
                    if (!updateDto.BatteryPercent.HasValue)
                    {
                        return BadRequest("Input a baterry %");
                    }

                    updatedDevice = new SmartWatches(
                        updateDto.ID,
                        updateDto.Name ?? existingDevice.Name,
                        updateDto.BatteryPercent.Value);
                    
                    if (existingDevice.IsDeviceTurned)
                    {
                        try { updatedDevice.TurnedOn(); } 
                        catch { /* Ignore error */ }
                    }
                }
                else if (existingDevice is Personal_Computer)
                {
                    var pc = (Personal_Computer)existingDevice;
                    
                    updatedDevice = new Personal_Computer(
                        updateDto.ID,
                        updateDto.Name ?? existingDevice.Name,
                        updateDto.OperatingSystem ?? pc.OperatingSystem);
                    
                    if (existingDevice.IsDeviceTurned)
                    {
                        try { updatedDevice.TurnedOn(); } 
                        catch { /* Ignore error */ }
                    }
                }
                else if (existingDevice is Embedded_devices)
                {
                    var embedded = (Embedded_devices)existingDevice;
                    
                    updatedDevice = new Embedded_devices(
                        updateDto.ID,
                        updateDto.Name ?? existingDevice.Name,
                        updateDto.IPAddress ?? embedded.IPAddress,
                        updateDto.NetworkName ?? embedded.NetworkName);
                    
                    if (existingDevice.IsDeviceTurned)
                    {
                        try { updatedDevice.TurnedOn(); } 
                        catch { /* Ignore error */ }
                    }
                }
                else
                {
                    updatedDevice = new Device(
                        updateDto.ID, 
                        updateDto.Name ?? existingDevice.Name);
                    
                    if (existingDevice.IsDeviceTurned)
                    {
                        updatedDevice.TurnedOn();
                    }
                }

                _devices[deviceIndex] = updatedDevice;
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteShitByID")]
        public IActionResult DeleteShit(string id)
        {
            var device = _devices.Find(d => d.ID == id);
            
            if (device == null)
            {
                return NotFound();
            }
            
            _devices.Remove(device);
            return NoContent();
        }

        private string GetDeviceType(Device device)
        {
            if (device is SmartWatches)
                return "SW";
            if (device is Personal_Computer)
                return "PC";
            if (device is Embedded_devices)
                return "ED";
            
            return "ХЗ";
        }
        
        private DeviceDetailDto CreateDetailedDto(Device device)
        {
            var detailDto = new DeviceDetailDto
            {
                ID = device.ID,
                Name = device.Name,
                DeviceType = GetDeviceType(device)
            };

            if (device is SmartWatches smartWatch)
            {
                detailDto.BatteryPercent = smartWatch.batteryPercent;
            }
            else if (device is Personal_Computer pc)
            {
                detailDto.OperatingSystem = pc.OperatingSystem;
            }
            else if (device is Embedded_devices embedded)
            {
                detailDto.IPAddress = embedded.IPAddress;
                detailDto.NetworkName = embedded.NetworkName;
            }

            return detailDto;
        }
    }
}