# PictureBox Control

## Overview
The `PictureBox` control displays images in your canvas-based WinForms application. Images are loaded from server URLs and rendered on the HTML5 canvas.

## Features
- ✅ Load images from server URLs
- ✅ Image caching for performance
- ✅ Multiple size modes (Normal, Stretch, Center, Zoom)
- ✅ Async image loading with fallback placeholders
- ✅ Error handling with visual indicators

## Basic Usage

```csharp
var pictureBox = new PictureBox
{
    Left = 10,
    Top = 10,
    Width = 200,
    Height = 150,
    ImageUrl = "/images/logo.png",
    SizeMode = PictureBoxSizeMode.StretchImage
};
Controls.Add(pictureBox);
```

## Properties

### ImageUrl
Gets or sets the URL of the image to display.

```csharp
pictureBox.ImageUrl = "https://example.com/image.png";
pictureBox.ImageUrl = "/images/local-image.jpg";
```

Supported formats: Any format supported by HTML `<img>` element (PNG, JPG, GIF, WebP, SVG, etc.)

### SizeMode
Gets or sets how the image is displayed in the PictureBox.

```csharp
pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
```

Options:
- **Normal**: Image displayed at original size from top-left corner
- **StretchImage**: Image stretched/shrunk to fill the control
- **CenterImage**: Image centered in the control
- **Zoom**: Image sized to fit while maintaining aspect ratio

## Image Sources

### Local Server Images
Place images in `wwwroot` folder of your Blazor host project:

```
Canvas.Windows.Forms.Host/
  wwwroot/
    images/
      logo.png
      photo.jpg
```

Reference them:
```csharp
pictureBox.ImageUrl = "/images/logo.png";
```

### External URLs
Use any publicly accessible image URL:

```csharp
pictureBox.ImageUrl = "https://via.placeholder.com/300x200";
pictureBox.ImageUrl = "https://example.com/api/images/123";
```

### Data URLs
You can also use base64-encoded data URLs:

```csharp
pictureBox.ImageUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA...";
```

## Performance

### Image Caching
Images are automatically cached in JavaScript. Once an image is loaded, subsequent displays of the same URL use the cached version.

### Async Loading
Images load asynchronously and don't block the UI. While loading, the PictureBox shows its background color.

### Error Handling
If an image fails to load:
- A placeholder with gray background is displayed
- An "X" pattern indicates the error
- The error is logged to browser console

## Examples

### Simple Image Display
```csharp
var pictureBox = new PictureBox
{
    Location = new Point(10, 10),
    Size = new Size(200, 150),
    ImageUrl = "/images/photo.jpg"
};
```

### Stretched Logo
```csharp
var logo = new PictureBox
{
    Location = new Point(0, 0),
    Size = new Size(100, 50),
    ImageUrl = "/images/logo.png",
    SizeMode = PictureBoxSizeMode.StretchImage,
    BackColor = Color.White
};
```

### Dynamic Image Loading
```csharp
private PictureBox _avatarPictureBox;

private void LoadUserAvatar(int userId)
{
    _avatarPictureBox.ImageUrl = $"/api/users/{userId}/avatar";
}

private void button_Click(object sender, EventArgs e)
{
    LoadUserAvatar(123);
}
```

### Placeholder While Loading
```csharp
var pictureBox = new PictureBox
{
    Location = new Point(10, 10),
    Size = new Size(300, 200),
    BackColor = Color.FromArgb(240, 240, 240), // Light gray background
    ImageUrl = "https://example.com/large-image.jpg"
};
```

## Implementation Details

### JavaScript Integration
The PictureBox uses the `drawImageAsync` JavaScript function which:
1. Checks the image cache
2. Loads the image if not cached
3. Draws the image on the canvas
4. Handles errors gracefully

### Drawing Command
Internally, the PictureBox generates a `DrawImageCommand` that translates to:
```javascript
await drawImageAsync(ctx, imageUrl, x, y, width, height);
```

## Limitations

### Current
- Size modes use simple implementations (future versions will properly handle aspect ratios)
- No built-in image dimension detection
- No image manipulation (rotation, filters, etc.)

### Planned Features
- Image dimension callbacks
- Click events with pixel coordinates
- Image loading progress events
- Built-in image effects (grayscale, brightness, etc.)
- Support for animated GIFs

## Troubleshooting

### Image Not Displaying
1. Check browser console for errors
2. Verify the image URL is accessible
3. Check CORS policy for external URLs
4. Ensure image format is supported

### CORS Issues with External URLs
If loading images from external domains, the server must allow CORS:
```
Access-Control-Allow-Origin: *
```

### Slow Loading
- Use appropriately sized images
- Consider using CDN for external images
- Optimize image format and compression

## See Also
- [Graphics Class](Graphics.md)
- [Control Base Class](Control.md)
- [Form Container](Form.md)
