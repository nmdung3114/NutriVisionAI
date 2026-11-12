using NutriVision.Infrastructure.Storage;

namespace NutriVision.UnitTests;

/// <summary>Ảnh mẫu trong Fixtures/Images được tạo bằng thư viện ảnh thật (Pillow), không phải dữ liệu tự dựng.</summary>
public class ImageHeaderInspectorTests
{
    private static readonly ImageHeaderInspector Inspector = new();
    private static byte[] Fixture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Images", name));

    [Theory]
    [InlineData("JpegBaselineWithExif_300x250.jpg", "jpeg", 300, 250)]
    [InlineData("JpegProgressive_640x480.jpg", "jpeg", 640, 480)]
    [InlineData("Png_301x257.png", "png", 301, 257)]
    [InlineData("WebpLossy_300x250.webp", "webp", 300, 250)]
    [InlineData("WebpLossless_229x224.webp", "webp", 229, 224)]
    [InlineData("WebpExtendedAlpha_301x257.webp", "webp", 301, 257)]
    public async Task Detects_format_and_size_from_real_files(string file, string format, int w, int h)
    {
        using var ms = new MemoryStream(Fixture(file));
        ms.Position = 0;
        var info = await Inspector.InspectAsync(ms);

        Assert.NotNull(info);
        Assert.Equal((format, w, h), (info.Format, info.Width, info.Height));
        Assert.Equal(0, ms.Position); // trả stream về đầu để lưu file
    }

    [Fact]
    public async Task Works_on_non_seekable_stream()
    {
        var info = await Inspector.InspectAsync(new ForwardOnlyStream(Fixture("JpegBaselineWithExif_300x250.jpg")));
        Assert.Equal((300, 250), (info!.Width, info.Height));
    }

    [Fact]
    public async Task Rejects_unsupported_format_even_with_image_extension()
    {
        Assert.Null(await Inspector.InspectAsync(new MemoryStream(Fixture("Gif_300x250.gif"))));
        Assert.Null(await Inspector.InspectAsync(new MemoryStream("<script>alert(1)</script> not an image"u8.ToArray())));
        Assert.Null(await Inspector.InspectAsync(new MemoryStream([])));
    }

    [Fact]
    public async Task Rejects_truncated_jpeg()
    {
        var jpg = Fixture("JpegBaselineWithExif_300x250.jpg");
        Assert.Null(await Inspector.InspectAsync(new MemoryStream(jpg[..200]))); // cắt giữa segment EXIF, chưa tới SOF
    }

    [Fact]
    public async Task Rejects_png_with_zero_or_absurd_dimensions()
    {
        var png = Fixture("Png_301x257.png");
        var zero = (byte[])png.Clone();
        zero[16] = zero[17] = zero[18] = zero[19] = 0;
        Assert.Null(await Inspector.InspectAsync(new MemoryStream(zero)));

        var huge = (byte[])png.Clone();
        huge[16] = 0x7F; // chiều rộng ~2 tỉ px
        Assert.Null(await Inspector.InspectAsync(new MemoryStream(huge)));
    }

    private sealed class ForwardOnlyStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, Math.Min(count, 7)); // đọc từng mẩu nhỏ
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
