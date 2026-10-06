using System.Security.Claims;
using DigitalNoticeBoard.Application.Notices;
using DigitalNoticeBoard.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalNoticeBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notices")]
public sealed class NoticesController(NoticeService noticeService) : ControllerBase
{
    [HttpGet("published")]
    public async Task<ActionResult<IReadOnlyList<NoticeDto>>> GetPublished(
        CancellationToken cancellationToken)
    {
        return Ok(await noticeService.GetPublishedAsync(cancellationToken));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoticeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await noticeService.GetAllAsync(cancellationToken));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoticeDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await noticeService.GetByIdAsync(id, cancellationToken));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<NoticeDto>> Create(
        SaveNoticeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notice = await noticeService.CreateAsync(
            request,
            userId,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = notice.Id }, notice);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoticeDto>> Update(
        Guid id,
        SaveNoticeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await noticeService.UpdateAsync(id, request, cancellationToken));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await noticeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
