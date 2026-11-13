using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NutriVision.Infrastructure.Persistence;

public class SqliteDateTimeOffsetConverter() : ValueConverter<DateTimeOffset, long>(
    v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));

public class SqliteNullableDateTimeOffsetConverter() : ValueConverter<DateTimeOffset?, long?>(
    v => v.HasValue ? v.Value.UtcTicks : null, v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : null);
