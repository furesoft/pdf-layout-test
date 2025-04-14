﻿using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace Moss.NET.Sdk.LayoutEngine.Nodes;

public class TextNode(YogaConfig config) : YogaNode(config)
{
    public double FontSize { get; set; } = 10;
    public string Text { get; set; }
    public string FontFamily { get; set; } = "Default";

    public Color? Color { get; set; } = Colors.Black;

    public TextDecoration TextDecoration { get; set; }

    public int TruncateSize { get; set; }

    public override void ReCalculate(PdfPageBuilder page)
    {
        var text = GetActualString();

        var measuredText = page.MeasureText(text, FontSize, PdfPoint.Origin, ParentLayout.GetFont(FontFamily));
        var leftMost = measuredText.Min(g => g.GlyphRectangle.Left);
        var rightMost = measuredText.Max(g => g.GlyphRectangle.Right);
        var textWidth = rightMost - leftMost;

        var textHeight = measuredText.Max(glyph => glyph.GlyphRectangle.Top)
                         - measuredText.Min(glyph => glyph.GlyphRectangle.Bottom);

        if (Width.Value < textWidth)
            Width = textWidth;

        if (Height.Value < textHeight)
            Height = textHeight;
    }

    private string GetActualString()
    {
        if (TruncateSize > 0 && Text.Length > TruncateSize)
        {
            Text = Text[..TruncateSize] + " ...";
        }

        return Text;
    }

    public override void Draw(PdfPageBuilder page, double absoluteX, double absoluteY)
    {
        base.Draw(page, absoluteX, absoluteY);

        var text = GetActualString();
        var font = ParentLayout.GetFont(FontFamily);

        var measuredText = page.MeasureText(text, FontSize, PdfPoint.Origin, font);
        var maxAscent = measuredText.Max(g => g.GlyphRectangle.Top);
        var minDescent = measuredText.Min(g => g.GlyphRectangle.Bottom);
        var yPosition = page.PageSize.Height - absoluteY - maxAscent;
        var textWidth = measuredText.Max(g => g.GlyphRectangle.Right) - measuredText.Min(g => g.GlyphRectangle.Left);

        if (Color != null)
        {
            page.SetStrokeColor(Color.r, Color.g, Color.b);
            page.SetTextAndFillColor(Color.r, Color.g, Color.b);
        }

        page.AddText(text, FontSize, new PdfPoint(absoluteX, yPosition), font);

        var lineThickness = FontSize * 0.04;
        if (TextDecoration == TextDecoration.Underline)
        {
            var underlineY = yPosition + minDescent * 0.25;
            page.DrawLine(new PdfPoint(absoluteX, underlineY), new PdfPoint(absoluteX + textWidth, underlineY), lineThickness);
        }
        else if (TextDecoration == TextDecoration.Strikethrough)
        {
            var strikeY = yPosition + (maxAscent * 0.4);
            page.DrawLine(new PdfPoint(absoluteX, strikeY), new PdfPoint(absoluteX + textWidth, strikeY), lineThickness);
        }

        page.ResetColor();
    }
}