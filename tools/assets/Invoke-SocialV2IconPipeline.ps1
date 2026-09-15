param(
    [string]$SourceRoot = 'C:\Users\PC\Downloads\icon_chat_friend',
    [string]$DestinationRoot = 'C:\Users\PC\Music\client-nro-unity\Assets\Resources\res\x4\mainimage'
)

$ErrorActionPreference = 'Stop'

$rasterizerSource = @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class SocialV2IconRasterizer
{
    public static void Normalize(string sourcePath, string destinationPath, int size, string backgroundColor)
    {
        using (Bitmap original = new Bitmap(sourcePath))
        using (Bitmap source = ToArgb(original))
        {
            Rectangle contentBounds = FindContentBounds(source);
            if (contentBounds.Width <= 0 || contentBounds.Height <= 0)
            {
                throw new InvalidOperationException("Source icon has no visible pixels.");
            }

            using (Bitmap content = new Bitmap(contentBounds.Width, contentBounds.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics contentGraphics = Graphics.FromImage(content))
                {
                    contentGraphics.CompositingMode = CompositingMode.SourceCopy;
                    contentGraphics.DrawImage(source, new Rectangle(0, 0, content.Width, content.Height), contentBounds, GraphicsUnit.Pixel);
                }
                NormalizeRoundButtonBackground(content, backgroundColor);
                using (Bitmap output = new Bitmap(size, size, PixelFormat.Format32bppArgb))
                using (Graphics graphics = Graphics.FromImage(output))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.None;
                    int inset = Math.Max(4, size / 20);
                    double scale = Math.Min((double)(size - 2 * inset) / content.Width, (double)(size - 2 * inset) / content.Height);
                    int drawWidth = Math.Max(1, (int)Math.Round(content.Width * scale));
                    int drawHeight = Math.Max(1, (int)Math.Round(content.Height * scale));
                    int drawX = (size - drawWidth) / 2;
                    int drawY = (size - drawHeight) / 2;
                    graphics.DrawImage(content, new Rectangle(drawX, drawY, drawWidth, drawHeight));
                    using (Bitmap centered = CenterVisibleContent(output))
                    {
                        centered.Save(destinationPath, ImageFormat.Png);
                    }
                }
            }
        }
    }

    private static Bitmap ToArgb(Bitmap original)
    {
        Bitmap converted = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(converted))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(original, 0, 0, original.Width, original.Height);
        }
        return converted;
    }

    private static Rectangle FindContentBounds(Bitmap bitmap)
    {
        int minX = bitmap.Width;
        int minY = bitmap.Height;
        int maxX = -1;
        int maxY = -1;
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int stride = Math.Abs(data.Stride);
            byte[] pixels = new byte[stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            for (int y = 0; y < bitmap.Height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (pixels[rowOffset + x * 4 + 3] == 0)
                    {
                        continue;
                    }
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
        return maxX < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }

    private static Bitmap CenterVisibleContent(Bitmap source)
    {
        Rectangle bounds = FindContentBounds(source);
        Bitmap centered = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(centered))
        {
            graphics.Clear(Color.Transparent);
            int contentCenterX = bounds.Left + (bounds.Width - 1) / 2;
            int contentCenterY = bounds.Top + (bounds.Height - 1) / 2;
            int targetCenterX = (source.Width - 1) / 2;
            int targetCenterY = (source.Height - 1) / 2;
            graphics.DrawImageUnscaled(source, targetCenterX - contentCenterX, targetCenterY - contentCenterY);
        }
        return centered;
    }

    private static void NormalizeRoundButtonBackground(Bitmap bitmap, string targetHex)
    {
        if (String.IsNullOrEmpty(targetHex))
        {
            return;
        }
        Color target = ColorTranslator.FromHtml(targetHex);
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.A == 0 || !IsButtonBackground(pixel, target))
                {
                    continue;
                }
                bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, target.R, target.G, target.B));
            }
        }
    }

    private static bool IsButtonBackground(Color pixel, Color target)
    {
        if (target.G > target.R && target.G > target.B)
        {
            return pixel.G > 80 && pixel.G > pixel.R * 1.15 && pixel.G > pixel.B * 1.15;
        }
        if (target.B > target.R && target.B > target.G)
        {
            return pixel.B > 90 && pixel.B > pixel.R * 1.15 && pixel.B > pixel.G * 1.05;
        }
        return pixel.R > 100 && pixel.R > pixel.G * 1.35 && pixel.R > pixel.B * 1.35;
    }
}
'@

$assets = @(
    @{ Hash = '248D304244DC5F618A985934CB20FF3938F45124439657C82A9878D615861F3D'; Asset = 'social_accept.png'; Size = 80; Color = '#57AA05' },
    @{ Hash = '348CE25ABCCB88259CE236725428B920BB279E13CE63630B15EF1E9220AD8AA3'; Asset = 'social_add.png'; Size = 80; Color = '#5170FF' },
    @{ Hash = '970FCFE45C59ABD6C26E8CD8BC352C5AC297E98715F4FAE6E8071A003B61274C'; Asset = 'social_chat.png'; Size = 96; Color = '' },
    @{ Hash = 'C07C06EC932974EF7E1AFA8F8D9A02EA13F43993276F78EE5C6FDB7560A9CA71'; Asset = 'social_emoji.png'; Size = 104; Color = '' },
    @{ Hash = '3DA8873B3467D4842225B5CD9BC31C7A2A073EA69EE882D2F7961E22337823BC'; Asset = 'social_location.png'; Size = 104; Color = '' },
    @{ Hash = '283E677B49E1E3B9FB385218C0C3126C895AF65D417EA5611360613304C48009'; Asset = 'social_mail.png'; Size = 80; Color = '' },
    @{ Hash = '6358BD5398F838FC0D5DDF5C4F9B09FF4181370D88DEE6C885B39252F61ED636'; Asset = 'social_remove.png'; Size = 80; Color = '#CF3B2E' },
    @{ Hash = '9A8DAA86E54FC633993855143CE637C62C0A76D70DF1BFE0EEE01B4CBDE17073'; Asset = 'social_search.png'; Size = 80; Color = '#5170FF' },
    @{ Hash = '59A159C8D8D77BF7A2FF4CE62B1EF790D2939BF1B86F1910ED89F42A8B80D257'; Asset = 'social_send.png'; Size = 104; Color = '' }
)

if (!(Test-Path -LiteralPath $SourceRoot)) {
    throw "Source root does not exist: $SourceRoot"
}

$sourceByHash = @{}
foreach ($source in (Get-ChildItem -LiteralPath $SourceRoot -File)) {
    $sourceByHash[(Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash] = $source.FullName
}
if ($sourceByHash.Count -ne $assets.Count) {
    throw 'Source inventory does not match the locked nine-icon manifest.'
}
foreach ($asset in $assets) {
    if (!$sourceByHash.ContainsKey($asset.Hash)) {
        throw "Source hash is missing or changed: $($asset.Hash)"
    }
}

$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('social-v2-icon-stage-' + [guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Path $stagingRoot | Out-Null
    New-Item -ItemType Directory -Path $DestinationRoot -Force | Out-Null
    Add-Type -AssemblyName System.Drawing
    Add-Type -TypeDefinition $rasterizerSource -Language CSharp -ReferencedAssemblies @([System.Drawing.Graphics].Assembly.Location)
    foreach ($asset in $assets) {
        $stagedPath = Join-Path $stagingRoot $asset.Asset
        Copy-Item -LiteralPath $sourceByHash[$asset.Hash] -Destination $stagedPath
        if ((Get-FileHash -LiteralPath $stagedPath -Algorithm SHA256).Hash -ne $asset.Hash) {
            throw "Staging integrity check failed for $($asset.Asset)"
        }
        [SocialV2IconRasterizer]::Normalize($stagedPath, (Join-Path $DestinationRoot $asset.Asset), $asset.Size, $asset.Color)
    }
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Force -Recurse
    }
}
