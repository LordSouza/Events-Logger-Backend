﻿using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests;

public class CreateEntryDTO
{
    public CreateEntryDTO()
    {
        FilesUrl = new List<string>();
    }
    public Guid ProjectId { get; set; }
    public string Description { get; init; } = string.Empty;

    public List<IFormFile>? Files { get; init; }

}