﻿namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateUserDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhotoPath { get; set; }
}