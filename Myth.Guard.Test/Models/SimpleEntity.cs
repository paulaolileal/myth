using Myth.Interfaces;
using Myth.Models;
using Myth.Builder;
using Myth.Extensions;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Simple entity for basic validation testing
/// </summary>
public class SimpleEntity : IValidatable<SimpleEntity>
{
    public string Value { get; set; } = string.Empty;

    public void Validate(ValidationBuilder<SimpleEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(Value, x => x.NotEmpty());
    }
}

/// <summary>
/// Entity for testing various string validation rules
/// </summary>
public class StringTestEntity : IValidatable<StringTestEntity>
{
    public string Text { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string AlphaNumeric { get; set; } = string.Empty;

    public void Validate(ValidationBuilder<StringTestEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(Text, x => x
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(50));

        builder.For(Email, x => x.Email());
        builder.For(Url, x => x.Url());
        builder.For(AlphaNumeric, x => x.Alphanumeric());
    }
}

/// <summary>
/// Entity for testing numeric validation rules
/// </summary>
public class NumericTestEntity : IValidatable<NumericTestEntity>
{
    public int IntValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public long LongValue { get; set; }

    public void Validate(ValidationBuilder<NumericTestEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(IntValue, x => x
            .GreaterThan(0)
            .LessThan(100));

        builder.For(DecimalValue, x => x
            .Positive()
            .LessOrEquals(1000.99m));

        builder.For(DoubleValue, x => x
            .Between(0.0, 1.0));

        builder.For(LongValue, x => x
            .NotZero()
            .GreaterOrEquals(-1000L));
    }
}

/// <summary>
/// Entity for testing collection validation rules
/// </summary>
public class CollectionTestEntity : IValidatable<CollectionTestEntity>
{
    public IEnumerable<string> Items { get; set; } = new List<string>();
    public IEnumerable<string> Tags { get; set; } = Array.Empty<string>();
    public IEnumerable<int> Numbers { get; set; } = Enumerable.Empty<int>();

    public void Validate(ValidationBuilder<CollectionTestEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(Items, x => x
            .NotEmpty()
            .CountBetween(1, 5)
            .All((string item) => !string.IsNullOrWhiteSpace(item)));

        builder.For(Tags, x => x
            .CountGreaterThan(0)
            .Distinct());

        builder.For(Numbers, x => x
            .Any(n => n > 0)
            .None(n => n < 0));
    }
}

/// <summary>
/// Entity for testing date/time validation rules
/// </summary>
public class DateTimeTestEntity : IValidatable<DateTimeTestEntity>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateOnly EventDate { get; set; }

    public void Validate(ValidationBuilder<DateTimeTestEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(CreatedAt, x => x
            .Past()
            .After(new DateTime(2020, 1, 1)));

        // Note: Future validation on nullable DateTime would need to check for null first
        // For now, skipping this validation as it requires a specialized nullable rule
        // builder.For(UpdatedAt, x => x
        //     .Future()
        //     .When<DateTime?, DateTimeTestEntity>(entity => entity.UpdatedAt.HasValue));

        builder.For(EventDate, x => x
            .Between(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1))));
    }
}

/// <summary>
/// Entity for testing boolean validation rules
/// </summary>
public class BooleanTestEntity : IValidatable<BooleanTestEntity>
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool? IsOptional { get; set; }

    public void Validate(ValidationBuilder<BooleanTestEntity> builder, ValidationContextKey? context = null)
    {
        builder.For(IsActive, x => x.IsTrue());
        builder.For(IsDeleted, x => x.IsFalse());
        builder.For(IsOptional, x => x.NotNull());
    }
}