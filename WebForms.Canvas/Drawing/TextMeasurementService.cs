using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace WebForms.Canvas.Drawing;

/// <summary>
/// Service for measuring text width using JavaScript canvas.measureText()
/// Caches results for performance
/// </summary>
public class TextMeasurementService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ConcurrentDictionary<string, int> _cache = new();

    public TextMeasurementService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Measure text width in pixels using browser's canvas.measureText()
    /// </summary>
    public async Task<int> MeasureTextAsync(string text, string fontFamily, int fontSize)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Create cache key
        var key = $"{fontFamily}|{fontSize}|{text}";

        // Check cache first
        if (_cache.TryGetValue(key, out var cachedWidth))
            return cachedWidth;

        try
        {
            // Call JavaScript measureText function
            var width = await _jsRuntime.InvokeAsync<int>("measureText", fontFamily, fontSize, text);

            // Cache the result
            _cache.TryAdd(key, width);

            return width;
        }
        catch
        {
            // Fallback to estimation if JS fails
            return EstimateTextWidth(text, fontSize);
        }
    }

    /// <summary>
    /// Measure text width synchronously using estimation
    /// Used during render when async is not possible
    /// </summary>
    public int MeasureTextEstimate(string text, string fontFamily, int fontSize)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Check cache first
        var key = $"{fontFamily}|{fontSize}|{text}";
        if (_cache.TryGetValue(key, out var cachedWidth))
            return cachedWidth;

        // Use estimation as fallback
        return EstimateTextWidth(text, fontSize);
    }

    /// <summary>
    /// Clear the measurement cache
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }

    private int EstimateTextWidth(string text, int fontSize)
    {
        // For Arial 12px, these are actual measured values from canvas
        // Each character is measured individually then summed
        double width = 0;
        foreach (char c in text)
        {
            width += EstimateCharWidth(c);
        }

        // Scale based on font size (calibrated for 12px)
        var scale = fontSize / 12.0;
        return (int)Math.Ceiling(width * scale);
    }

    private double EstimateCharWidth(char c)
    {
        // Actual Arial 12px measurements from canvas.measureText()
        // These values are more accurate approximations
        if (c == ' ')
            return 3.3;
        else if (char.IsUpper(c))
        {
            return c switch
            {
                'W' => 10.7,
                'M' => 9.3,
                'Q' => 8.7,
                'G' or 'O' or 'D' or 'C' => 8.0,
                'I' => 3.3,
                'J' => 5.3,
                'A' => 7.3,
                'B' or 'E' or 'F' or 'K' or 'P' or 'R' or 'S' => 7.3,
                'H' or 'N' or 'U' => 7.3,
                'L' => 6.0,
                'T' => 6.7,
                'V' or 'X' or 'Y' or 'Z' => 6.7,
                _ => 7.3
            };
        }
        else if (char.IsLower(c))
        {
            return c switch
            {
                'i' or 'l' => 2.7,
                'j' => 2.7,
                't' or 'f' or 'r' => 3.3,
                'm' => 8.7,
                'w' => 7.3,
                'a' or 'c' or 'e' or 'g' or 'o' or 'q' or 's' => 6.0,
                'b' or 'd' or 'h' or 'n' or 'p' or 'u' => 6.0,
                'k' => 5.3,
                'v' or 'x' or 'y' or 'z' => 5.3,
                _ => 6.0
            };
        }
        else if (char.IsDigit(c))
        {
            return 6.7; // Arial digits are uniform width
        }
        else
        {
            return c switch
            {
                '.' or ',' or ':' or ';' or '!' or '|' or '\'' => 2.7,
                'i' => 2.7,
                '-' => 3.3,
                '(' or ')' => 4.0,
                '=' or '+' => 6.7,
                _ => 5.3
            };
        }
    }
}
