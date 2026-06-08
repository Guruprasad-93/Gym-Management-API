using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Website;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/website")]
[Authorize]
public class WebsiteSettingsController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteSettingsController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet("settings")]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteSettingsDto>>> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _websiteService.GetSettingsAsync(cancellationToken);
        return Ok(ApiResponse<GymWebsiteSettingsDto>.Ok(result));
    }

    [HttpPut("settings")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteSettingsDto>>> UpsertSettings(
        [FromBody] UpsertGymWebsiteSettingsDto dto, CancellationToken cancellationToken)
    {
        var result = await _websiteService.UpsertSettingsAsync(dto, cancellationToken);
        return Ok(ApiResponse<GymWebsiteSettingsDto>.Ok(result));
    }

    [HttpPost("settings/publish")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Publish(CancellationToken cancellationToken)
    {
        await _websiteService.PublishAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Website published."));
    }

    [HttpPost("settings/unpublish")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Unpublish(CancellationToken cancellationToken)
    {
        await _websiteService.UnpublishAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Website unpublished."));
    }
}

[ApiController]
[Route("api/website/pages")]
[Authorize]
public class WebsitePagesController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsitePagesController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymWebsitePageDto>>>> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<GymWebsitePageDto>>.Ok(await _websiteService.GetPagesAsync(cancellationToken)));

    [HttpPost]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsitePageDto>>> Create([FromBody] CreateGymWebsitePageDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<GymWebsitePageDto>.Ok(await _websiteService.CreatePageAsync(dto, cancellationToken)));

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsitePageDto>>> Update(int id, [FromBody] UpdateGymWebsitePageDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return Ok(ApiResponse<GymWebsitePageDto>.Ok(await _websiteService.UpdatePageAsync(dto, cancellationToken)));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _websiteService.DeletePageAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Page deleted."));
    }
}

[ApiController]
[Route("api/website/sections")]
[Authorize]
public class WebsiteSectionsController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteSectionsController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymWebsiteSectionDto>>>> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<GymWebsiteSectionDto>>.Ok(await _websiteService.GetSectionsAsync(cancellationToken)));

    [HttpPost]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteSectionDto>>> Create([FromBody] CreateGymWebsiteSectionDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<GymWebsiteSectionDto>.Ok(await _websiteService.CreateSectionAsync(dto, cancellationToken)));

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteSectionDto>>> Update(int id, [FromBody] UpdateGymWebsiteSectionDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return Ok(ApiResponse<GymWebsiteSectionDto>.Ok(await _websiteService.UpdateSectionAsync(dto, cancellationToken)));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _websiteService.DeleteSectionAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Section deleted."));
    }
}

[ApiController]
[Route("api/website/testimonials")]
[Authorize]
public class WebsiteTestimonialsController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteTestimonialsController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymWebsiteTestimonialDto>>>> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<GymWebsiteTestimonialDto>>.Ok(await _websiteService.GetTestimonialsAsync(cancellationToken)));

    [HttpPost]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteTestimonialDto>>> Create([FromBody] CreateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<GymWebsiteTestimonialDto>.Ok(await _websiteService.CreateTestimonialAsync(dto, cancellationToken)));

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteTestimonialDto>>> Update(int id, [FromBody] UpdateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return Ok(ApiResponse<GymWebsiteTestimonialDto>.Ok(await _websiteService.UpdateTestimonialAsync(dto, cancellationToken)));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _websiteService.DeleteTestimonialAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Testimonial deleted."));
    }
}

[ApiController]
[Route("api/website/gallery")]
[Authorize]
public class WebsiteGalleryController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteGalleryController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymWebsiteGalleryItemDto>>>> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<GymWebsiteGalleryItemDto>>.Ok(await _websiteService.GetGalleryAsync(cancellationToken)));

    [HttpPost]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<GymWebsiteGalleryItemDto>>> Create([FromBody] CreateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<GymWebsiteGalleryItemDto>.Ok(await _websiteService.CreateGalleryItemAsync(dto, cancellationToken)));

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        await _websiteService.UpdateGalleryItemAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gallery item updated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _websiteService.DeleteGalleryItemAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gallery item deleted."));
    }
}

[ApiController]
[Route("api/website/leads")]
[Authorize]
public class WebsiteLeadsController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteLeadsController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteBuilder)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<WebsiteLeadCaptureDto>>>> Get(
        [FromQuery] WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResultDto<WebsiteLeadCaptureDto>>.Ok(await _websiteService.GetLeadsAsync(query, cancellationToken)));
}

[ApiController]
[Route("api/website/analytics")]
[Authorize]
public class WebsiteAnalyticsController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteAnalyticsController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet]
    [RequirePermission(Permissions.ViewWebsiteAnalytics)]
    public async Task<ActionResult<ApiResponse<WebsiteAnalyticsOverviewDto>>> Get([FromQuery] int days = 30, CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<WebsiteAnalyticsOverviewDto>.Ok(await _websiteService.GetAnalyticsAsync(days, cancellationToken)));

    [HttpGet("export/{format}")]
    [RequirePermission(Permissions.ViewWebsiteAnalytics)]
    public async Task<IActionResult> Export(string format, [FromQuery] WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _websiteService.ExportLeadsAsync(format, query, cancellationToken);
        var contentType = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";
        var ext = format.Equals("excel", StringComparison.OrdinalIgnoreCase) ? "xlsx" : "pdf";
        return File(bytes, contentType, $"website-leads.{ext}");
    }
}

[ApiController]
[Route("api/public/website")]
[AllowAnonymous]
public class PublicWebsiteController : ControllerBase
{
    private readonly IWebsiteService _websiteService;

    public PublicWebsiteController(IWebsiteService websiteService) => _websiteService = websiteService;

    [HttpGet("{gymSlug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicWebsiteDto>>> Get(string gymSlug, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PublicWebsiteDto>.Ok(await _websiteService.GetPublicWebsiteAsync(gymSlug, cancellationToken)));

    [HttpPost("lead")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<int>>> CaptureLead([FromBody] PublicWebsiteLeadDto dto, CancellationToken cancellationToken)
    {
        var id = await _websiteService.CapturePublicLeadAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Enquiry submitted."));
    }

    [HttpPost("trial-booking")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<int>>> BookTrial([FromBody] PublicTrialBookingDto dto, CancellationToken cancellationToken)
    {
        var id = await _websiteService.BookPublicTrialAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Trial booking submitted."));
    }

    [HttpGet("{gymSlug}/sitemap")]
    [AllowAnonymous]
    public async Task<IActionResult> Sitemap(string gymSlug, CancellationToken cancellationToken)
    {
        var xml = await _websiteService.GenerateSitemapAsync(gymSlug, cancellationToken);
        return Content(xml, "application/xml");
    }

    [HttpGet("{gymSlug}/robots.txt")]
    [AllowAnonymous]
    public async Task<IActionResult> Robots(string gymSlug, CancellationToken cancellationToken)
    {
        var text = await _websiteService.GenerateRobotsTxtAsync(gymSlug, cancellationToken);
        return Content(text, "text/plain");
    }
}
