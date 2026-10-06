using DigitalNoticeBoard.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalNoticeBoard.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/storage")]
public sealed class StorageController(IStorageStatus storageStatus) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            storageStatus.Provider,
            storageStatus.IsFallback,
            storageStatus.Message
        });
    }
}
