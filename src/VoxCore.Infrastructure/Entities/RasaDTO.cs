namespace VoxCore.Infrastructure.Services;

public sealed record RasaIntent(string? Name, double? Confidence);

public sealed record RasaEntity(string? Entity, string? Value);

public sealed record RasaDTO(RasaIntent? Intent, RasaEntity[]? Entities);