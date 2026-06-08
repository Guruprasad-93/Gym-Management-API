using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Files;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService) => _fileService = fileService;

    [HttpPost("upload")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<FileDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<FileDto>>> Upload(
        IFormFile file,
        [FromForm] UploadFileRequestDto request,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<FileDto>.Fail("No file provided."));

        await using var stream = file.OpenReadStream();
        var result = await _fileService.UploadAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FileDto>.Ok(ToPublicDto(result), "File uploaded."));
    }

    [HttpGet("{fileId:long}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(
        long fileId,
        [FromQuery] Guid g,
        [FromQuery] long exp,
        [FromQuery] string sig,
        CancellationToken cancellationToken)
    {
        try
        {
            var (stream, contentType, fileName) = await _fileService.DownloadAsync(
                fileId, g, exp, sig, cancellationToken);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("{fileId:long}")]
    [RequirePermission(Permissions.ViewFiles)]
    public async Task<ActionResult<ApiResponse<FileDto>>> GetMetadata(long fileId, CancellationToken cancellationToken)
    {
        var file = await _fileService.GetMetadataAsync(fileId, cancellationToken);
        return Ok(ApiResponse<FileDto>.Ok(ToPublicDto(file)));
    }

    [HttpDelete("{fileId:long}")]
    [RequireAnyPermission(Permissions.DeleteFiles, Permissions.ManageFiles)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long fileId, CancellationToken cancellationToken)
    {
        await _fileService.DeleteAsync(fileId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "File deleted."));
    }

    [HttpGet("members/{memberId:int}")]
    [RequireAnyPermission(Permissions.ViewFiles, Permissions.ViewMembers, Permissions.ViewMemberDetails)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberFileDto>>>> GetMemberFiles(
        int memberId,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var files = await _fileService.GetMemberFilesAsync(memberId, category, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberFileDto>>.Ok(files));
    }

    [HttpGet("trainers/{trainerId:int}")]
    [RequireAnyPermission(Permissions.ViewFiles, Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrainerFileDto>>>> GetTrainerFiles(
        int trainerId,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var files = await _fileService.GetTrainerFilesAsync(trainerId, category, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TrainerFileDto>>.Ok(files));
    }

    [HttpGet("gym/logo")]
    [RequirePermission(Permissions.ViewFiles)]
    public async Task<ActionResult<ApiResponse<FileDto>>> GetGymLogo(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var file = await _fileService.GetGymLogoAsync(gymId, cancellationToken);
        return Ok(ApiResponse<FileDto>.Ok(file is null ? null! : ToPublicDto(file)));
    }

    private static FileDto ToPublicDto(FileDto file) => new()
    {
        FileId = file.FileId,
        GymId = file.GymId,
        FileCategory = file.FileCategory,
        StorageProvider = file.StorageProvider,
        PublicUrl = file.PublicUrl,
        OriginalFileName = file.OriginalFileName,
        ContentType = file.ContentType,
        FileSizeBytes = file.FileSizeBytes,
        Width = file.Width,
        Height = file.Height,
        UploadedByUserId = file.UploadedByUserId,
        CreatedAt = file.CreatedAt
    };
}
