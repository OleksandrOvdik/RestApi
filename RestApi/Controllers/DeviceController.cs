using Electronics;
using Microsoft.AspNetCore.Mvc;

namespace RestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
    {
        private readonly Device_Manager _deviceService;

        public DeviceController(Device_Manager deviceService)
        {
            _deviceService = deviceService;
        }

        // GET: api/Device
        [HttpGet]
        public ActionResult<IEnumerable<DeviceDto>> GetDevices()
        {
            var devices = _deviceService.GetAllDevices();
            return Ok(devices);
        }

        // GET: api/Device/5
        [HttpGet("{id}")]
        public ActionResult<DeviceDetailDto> GetDevice(string id)
        {
            var device = _deviceService.GetDeviceById(id);
            
            if (device == null)
            {
                return NotFound();
            }

            return Ok(device);
        }

        // POST: api/Device
        [HttpPost]
        public ActionResult<DeviceDto> CreateDevice(CreateDeviceDto createDeviceDto)
        {
            try
            {
                var deviceDto = _deviceService.AddDevice(createDeviceDto);
                return CreatedAtAction(nameof(GetDevice), new { id = deviceDto.ID }, deviceDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Device/5
        [HttpPut("{id}")]
        public IActionResult UpdateDevice(string id, UpdateDeviceDto updateDeviceDto)
        {
            if (id != updateDeviceDto.ID)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            try
            {
                _deviceService.UpdateDevice(updateDeviceDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Device/5
        [HttpDelete("{id}")]
        public IActionResult DeleteDevice(string id)
        {
            var result = _deviceService.RemoveDevice(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}