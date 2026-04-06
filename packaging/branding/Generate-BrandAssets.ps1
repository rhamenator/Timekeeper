param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
)

Add-Type -AssemblyName System.Drawing

$brandDir = Join-Path $Root "Timekeeper.Web\wwwroot\branding"
$webAssetDir = Join-Path $Root "Timekeeper.Web\Assets"
$msixAssetDir = Join-Path $Root "packaging\windows\msix\Assets"

New-Item -ItemType Directory -Force -Path $brandDir, $webAssetDir, $msixAssetDir | Out-Null

$night = [System.Drawing.Color]::FromArgb(255, 10, 27, 31)
$slate = [System.Drawing.Color]::FromArgb(255, 17, 55, 61)
$teal = [System.Drawing.Color]::FromArgb(255, 66, 183, 173)
$mint = [System.Drawing.Color]::FromArgb(255, 144, 235, 201)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 192, 105)
$coral = [System.Drawing.Color]::FromArgb(255, 255, 138, 110)
$paper = [System.Drawing.Color]::FromArgb(255, 245, 238, 226)

function New-Canvas {
    param([int]$Width, [int]$Height)
    $bitmap = [System.Drawing.Bitmap]::new($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Fill-RoundedRectangle {
    param(
        [System.Drawing.Graphics]$Graphics,
        [System.Drawing.Brush]$Brush,
        [double]$X,
        [double]$Y,
        [double]$Width,
        [double]$Height,
        [double]$Radius
    )

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = [Math]::Min($Radius * 2, [Math]::Min($Width, $Height))
    $path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    $path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    $path.AddArc($X + $Width - $diameter, $Y + $Height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    $Graphics.FillPath($Brush, $path)
    $path.Dispose()
}

function Draw-Mark {
    param(
        [System.Drawing.Graphics]$Graphics,
        [int]$Width,
        [int]$Height
    )

    $backgroundBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.Point]::new(0, 0),
        [System.Drawing.Point]::new($Width, $Height),
        $night,
        $slate)
    Fill-RoundedRectangle -Graphics $Graphics -Brush $backgroundBrush -X 0 -Y 0 -Width $Width -Height $Height -Radius ($Width * 0.16)

    $glow = [System.Drawing.Drawing2D.PathGradientBrush]::new(([System.Drawing.Point[]]@(
        [System.Drawing.Point]::new($Width * 0.1, $Height * 0.1),
        [System.Drawing.Point]::new($Width * 0.9, $Height * 0.15),
        [System.Drawing.Point]::new($Width * 0.8, $Height * 0.8),
        [System.Drawing.Point]::new($Width * 0.2, $Height * 0.9)
    )))
    $glow.CenterColor = [System.Drawing.Color]::FromArgb(80, $coral.R, $coral.G, $coral.B)
    $glow.SurroundColors = [System.Drawing.Color[]]@([System.Drawing.Color]::Transparent)
    $Graphics.FillRectangle($glow, 0, 0, $Width, $Height)

    $faceRect = [System.Drawing.RectangleF]::new($Width * 0.18, $Height * 0.14, $Width * 0.64, $Height * 0.64)
    $ringPen = [System.Drawing.Pen]::new($gold, [Math]::Max(6, $Width * 0.05))
    $Graphics.DrawEllipse($ringPen, $faceRect)

    $innerRect = [System.Drawing.RectangleF]::new($Width * 0.28, $Height * 0.24, $Width * 0.44, $Height * 0.44)
    $innerBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(225, $paper.R, $paper.G, $paper.B))
    $Graphics.FillEllipse($innerBrush, $innerRect)

    $ledgerBrush = [System.Drawing.SolidBrush]::new($teal)
    Fill-RoundedRectangle -Graphics $Graphics -Brush $ledgerBrush -X ($Width * 0.36) -Y ($Height * 0.33) -Width ($Width * 0.28) -Height ($Height * 0.2) -Radius ($Width * 0.05)

    $linePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(200, $paper.R, $paper.G, $paper.B), [Math]::Max(2, $Width * 0.016))
    $Graphics.DrawLine($linePen, $Width * 0.41, $Height * 0.38, $Width * 0.59, $Height * 0.38)
    $Graphics.DrawLine($linePen, $Width * 0.41, $Height * 0.43, $Width * 0.55, $Height * 0.43)
    $Graphics.DrawLine($linePen, $Width * 0.41, $Height * 0.48, $Width * 0.52, $Height * 0.48)

    $handPen = [System.Drawing.Pen]::new($coral, [Math]::Max(5, $Width * 0.028))
    $handPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $handPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $center = [System.Drawing.PointF]::new($Width * 0.5, $Height * 0.46)
    $Graphics.DrawLine($handPen, $center, [System.Drawing.PointF]::new($Width * 0.5, $Height * 0.28))
    $Graphics.DrawLine($handPen, $center, [System.Drawing.PointF]::new($Width * 0.63, $Height * 0.56))
    $Graphics.FillEllipse([System.Drawing.SolidBrush]::new($coral), $Width * 0.47, $Height * 0.43, $Width * 0.06, $Width * 0.06)

    $checkPen = [System.Drawing.Pen]::new($mint, [Math]::Max(6, $Width * 0.03))
    $checkPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $checkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $Graphics.DrawLine($checkPen, $Width * 0.33, $Height * 0.7, $Width * 0.44, $Height * 0.81)
    $Graphics.DrawLine($checkPen, $Width * 0.44, $Height * 0.81, $Width * 0.69, $Height * 0.59)

    $backgroundBrush.Dispose()
    $glow.Dispose()
    $ringPen.Dispose()
    $innerBrush.Dispose()
    $ledgerBrush.Dispose()
    $linePen.Dispose()
    $handPen.Dispose()
    $checkPen.Dispose()
}

function Draw-Lockup {
    param(
        [System.Drawing.Graphics]$Graphics,
        [int]$Width,
        [int]$Height
    )

    $Graphics.Clear($night)
    Draw-Mark -Graphics $Graphics -Width ($Height - 30) -Height ($Height - 30)

    $titleFont = [System.Drawing.Font]::new("Georgia", [Math]::Max(28, $Height * 0.26), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $subtitleFont = [System.Drawing.Font]::new("Trebuchet MS", [Math]::Max(10, $Height * 0.08), [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
    $titleBrush = [System.Drawing.SolidBrush]::new($paper)
    $subtitleBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(205, $mint.R, $mint.G, $mint.B))

    $left = $Height + 6
    $Graphics.DrawString("Timekeeper", $titleFont, $titleBrush, [System.Drawing.PointF]::new($left, $Height * 0.24))
    $Graphics.DrawString("TIME + PAYROLL + TAX WORKFLOWS", $subtitleFont, $subtitleBrush, [System.Drawing.PointF]::new($left + 2, $Height * 0.63))

    $titleFont.Dispose()
    $subtitleFont.Dispose()
    $titleBrush.Dispose()
    $subtitleBrush.Dispose()
}

function Save-Png {
    param([System.Drawing.Bitmap]$Bitmap, [string]$Path)
    $directory = Split-Path -Parent $Path
    if ($directory) { New-Item -ItemType Directory -Force -Path $directory | Out-Null }
    $Bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
}

function New-PngBytes {
    param([System.Drawing.Bitmap]$Bitmap)
    $memory = [System.IO.MemoryStream]::new()
    try {
        $Bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
        return $memory.ToArray()
    }
    finally {
        $memory.Dispose()
    }
}

function Save-IcoFromPngBytes {
    param([byte[]]$PngBytes, [string]$Path)

    $directory = Split-Path -Parent $Path
    if ($directory) { New-Item -ItemType Directory -Force -Path $directory | Out-Null }

    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
    $writer = [System.IO.BinaryWriter]::new($stream)

    try {
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]1)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]$PngBytes.Length)
        $writer.Write([UInt32]22)
        $writer.Write($PngBytes)
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

$mark512 = New-Canvas -Width 512 -Height 512
Draw-Mark -Graphics $mark512.Graphics -Width 512 -Height 512
Save-Png -Bitmap $mark512.Bitmap -Path (Join-Path $brandDir "timekeeper-mark-512.png")
Save-Png -Bitmap $mark512.Bitmap -Path (Join-Path $Root "Timekeeper.Web\wwwroot\favicon.png")
$icoBytes = New-PngBytes -Bitmap $mark512.Bitmap
Save-IcoFromPngBytes -PngBytes $icoBytes -Path (Join-Path $webAssetDir "timekeeper.ico")
$mark512.Graphics.Dispose()
$mark512.Bitmap.Dispose()

foreach ($size in 44, 50, 150, 310) {
    $canvasInfo = New-Canvas -Width $size -Height $size
    Draw-Mark -Graphics $canvasInfo.Graphics -Width $size -Height $size

    switch ($size) {
        44 { $name = "Square44x44Logo.png" }
        50 { $name = "StoreLogo.png" }
        150 { $name = "Square150x150Logo.png" }
        310 { $name = "Square310x310Logo.png" }
    }

    Save-Png -Bitmap $canvasInfo.Bitmap -Path (Join-Path $msixAssetDir $name)
    $canvasInfo.Graphics.Dispose()
    $canvasInfo.Bitmap.Dispose()
}

$wide = New-Canvas -Width 620 -Height 300
Draw-Lockup -Graphics $wide.Graphics -Width 620 -Height 300
Save-Png -Bitmap $wide.Bitmap -Path (Join-Path $msixAssetDir "Wide310x150Logo.png")
$wide.Graphics.Dispose()
$wide.Bitmap.Dispose()

$splash = New-Canvas -Width 620 -Height 300
$splash.Graphics.Clear($night)
Draw-Mark -Graphics $splash.Graphics -Width 220 -Height 220
$titleFont = [System.Drawing.Font]::new("Georgia", 34, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$titleBrush = [System.Drawing.SolidBrush]::new($paper)
$splash.Graphics.DrawString("Timekeeper", $titleFont, $titleBrush, [System.Drawing.PointF]::new(210, 120))
Save-Png -Bitmap $splash.Bitmap -Path (Join-Path $msixAssetDir "SplashScreen.png")
$titleFont.Dispose()
$titleBrush.Dispose()
$splash.Graphics.Dispose()
$splash.Bitmap.Dispose()
