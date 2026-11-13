using System.Buffers.Binary;
using NutriVision.Application.Abstractions;

namespace NutriVision.Infrastructure.Storage;

/// <summary>
/// Xác định định dạng (JPEG/PNG/WEBP) và kích thước ảnh bằng cách đọc header theo đặc tả,
/// dựa vào nội dung file chứ không tin phần mở rộng hay Content-Type do client gửi.
/// Không giải mã điểm ảnh → không phụ thuộc thư viện xử lý ảnh, bề mặt tấn công nhỏ.
/// </summary>
public sealed class ImageHeaderInspector : IImageInspector
{
    /// <summary>Giới hạn cạnh ảnh hợp lệ (JPEG/WebP tối đa 65535/16383; chặn giá trị vô lý).</summary>
    public const int MaxSide = 20_000;

    public async Task<ImageInfo?> InspectAsync(Stream content, CancellationToken ct = default)
    {
        try
        {
            var head = new byte[30];
            var n = await ReadUpToAsync(content, head, ct);
            ImageInfo? info = null;
            if (n >= 24 && IsPng(head)) info = ParsePng(head);
            else if (n >= 30 && IsWebp(head)) info = ParseWebp(head);
            else if (n >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF)
                info = await ParseJpegAsync(content, head.AsMemory(2, n - 2), ct);

            return info is { Width: > 0 and <= MaxSide, Height: > 0 and <= MaxSide } ? info : null;
        }
        catch (EndOfStreamException)
        {
            return null; // file bị cắt cụt
        }
        finally
        {
            if (content.CanSeek) content.Position = 0;
        }
    }

    // ---------- PNG: chữ ký 8 byte, chunk đầu tiên bắt buộc là IHDR (13 byte) ----------
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static bool IsPng(ReadOnlySpan<byte> h) => h[..8].SequenceEqual(PngSignature);

    private static ImageInfo? ParsePng(ReadOnlySpan<byte> h)
    {
        if (BinaryPrimitives.ReadUInt32BigEndian(h[8..]) != 13 || !h[12..16].SequenceEqual("IHDR"u8)) return null;
        var w = BinaryPrimitives.ReadUInt32BigEndian(h[16..]);
        var hgt = BinaryPrimitives.ReadUInt32BigEndian(h[20..]);
        return w > int.MaxValue || hgt > int.MaxValue ? null : new ImageInfo((int)w, (int)hgt, "png", ".png", "image/png");
    }

    // ---------- WEBP: RIFF....WEBP + chunk VP8 (lossy) / VP8L (lossless) / VP8X (mở rộng) ----------
    private static bool IsWebp(ReadOnlySpan<byte> h) => h[..4].SequenceEqual("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8);

    private static ImageInfo? ParseWebp(ReadOnlySpan<byte> h)
    {
        var chunk = h[12..16];
        var d = h[20..]; // dữ liệu chunk (sau FourCC + kích thước 4 byte)
        int w, hgt;
        if (chunk.SequenceEqual("VP8 "u8))
        {
            // 3 byte frame tag (bit 0 = 0 nghĩa là key frame), mã bắt đầu 9D 01 2A, rồi 14 bit rộng/cao
            if ((d[0] & 1) != 0 || d[3] != 0x9D || d[4] != 0x01 || d[5] != 0x2A) return null;
            w = BinaryPrimitives.ReadUInt16LittleEndian(d[6..]) & 0x3FFF;
            hgt = BinaryPrimitives.ReadUInt16LittleEndian(d[8..]) & 0x3FFF;
        }
        else if (chunk.SequenceEqual("VP8L"u8))
        {
            if (d[0] != 0x2F) return null;
            var bits = BinaryPrimitives.ReadUInt32LittleEndian(d[1..]);
            w = (int)(bits & 0x3FFF) + 1;
            hgt = (int)((bits >> 14) & 0x3FFF) + 1;
        }
        else if (chunk.SequenceEqual("VP8X"u8))
        {
            w = (d[4] | d[5] << 8 | d[6] << 16) + 1;
            hgt = (d[7] | d[8] << 8 | d[9] << 16) + 1;
        }
        else return null;

        return new ImageInfo(w, hgt, "webp", ".webp", "image/webp");
    }

    // ---------- JPEG: duyệt các segment tới khung SOFn để lấy kích thước ----------
    private static async Task<ImageInfo?> ParseJpegAsync(Stream s, ReadOnlyMemory<byte> alreadyRead, CancellationToken ct)
    {
        // Các byte đã đọc (sau FF D8) được ghép trước luồng còn lại.
        var reader = new ByteReader(s, alreadyRead);
        for (var segments = 0; segments < 1000; segments++)
        {
            if (await reader.ReadByteAsync(ct) != 0xFF) return null;
            int marker;
            do marker = await reader.ReadByteAsync(ct); while (marker == 0xFF); // byte đệm

            if (marker is 0x01 or (>= 0xD0 and <= 0xD7)) continue;          // marker không có độ dài
            if (marker is 0xD9 or 0xDA or 0x00) return null;                 // hết ảnh / bắt đầu dữ liệu mà chưa thấy SOF

            var len = await reader.ReadUInt16BigEndianAsync(ct);
            if (len < 2) return null;

            var isSof = marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC;
            if (isSof)
            {
                if (len < 7) return null;
                await reader.ReadByteAsync(ct); // độ chính xác bit
                var h = await reader.ReadUInt16BigEndianAsync(ct);
                var w = await reader.ReadUInt16BigEndianAsync(ct);
                return new ImageInfo(w, h, "jpeg", ".jpg", "image/jpeg");
            }
            await reader.SkipAsync(len - 2, ct);
        }
        return null;
    }

    private static async Task<int> ReadUpToAsync(Stream s, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var r = await s.ReadAsync(buffer.AsMemory(total), ct);
            if (r == 0) break;
            total += r;
        }
        return total;
    }

    private sealed class ByteReader(Stream s, ReadOnlyMemory<byte> prefix)
    {
        private readonly byte[] _one = new byte[1];
        private ReadOnlyMemory<byte> _prefix = prefix;

        public async ValueTask<int> ReadByteAsync(CancellationToken ct)
        {
            if (!_prefix.IsEmpty)
            {
                var b = _prefix.Span[0];
                _prefix = _prefix[1..];
                return b;
            }
            if (await s.ReadAsync(_one, ct) == 0) throw new EndOfStreamException();
            return _one[0];
        }

        public async ValueTask<int> ReadUInt16BigEndianAsync(CancellationToken ct)
            => (await ReadByteAsync(ct) << 8) | await ReadByteAsync(ct);

        public async ValueTask SkipAsync(int count, CancellationToken ct)
        {
            var fromPrefix = Math.Min(count, _prefix.Length);
            _prefix = _prefix[fromPrefix..];
            count -= fromPrefix;
            if (count == 0) return;
            if (s.CanSeek)
            {
                if (s.Position + count > s.Length) throw new EndOfStreamException();
                s.Seek(count, SeekOrigin.Current);
                return;
            }
            var buf = new byte[Math.Min(count, 8192)];
            while (count > 0)
            {
                var r = await s.ReadAsync(buf.AsMemory(0, Math.Min(count, buf.Length)), ct);
                if (r == 0) throw new EndOfStreamException();
                count -= r;
            }
        }
    }
}
