﻿using System.Diagnostics;
using Marius.Yoga;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace LayoutPdfTest;

class Program
{
    static void Main(string[] args)
    {
        var builder = new PdfDocumentBuilder();
        var headlineFont = builder.AddTrueTypeFont(File.ReadAllBytes("fonts/Jaini-Regular.ttf")); //builder.AddStandard14Font(Standard14Font.Helvetica);
        var textFont = builder.AddTrueTypeFont(File.ReadAllBytes("fonts/NoticiaText-Regular.ttf"));

        var page = builder.AddPage(PageSize.A4, false);
        var config = new YogaConfig();

        var root = new YogaNode(config)
        {
            Width = page.PageSize.Width,
            Height = page.PageSize.Height,
            Margin = 10,
            Padding = 10,
            Name = "root",
            BorderColor = new Color(0, 255, 0)
        };

        var header = new YogaNode(config)
        {
            Height = 120,
            MarginTop = 15,
            MarginLeft = 10,
            MarginRight = 10,
            FlexGrow = 0,
            AlignItems = YogaAlign.Center,
            Name = "header",
        };

        var headerText = new TextNode(config)
        {
            Text = "Daily E-Book Times",
            FontSize = 75,
            Font = headlineFont,
            AlignSelf = YogaAlign.Center,
            Width = 300,
            Height = 55,
            PositionType = YogaPositionType.Relative
        };
        var topLine = new HorizontalLineNode(config)
        {
            LineThickness = 2,
            Margin = 10
        };
        var bottomLine = new HorizontalLineNode(config)
        {
            LineThickness = 2,
            Margin = 10
        };

        var issueText = new TextNode(config)
        {
            Text = "Issue #1",
            Font = textFont,
            FontSize = 15,
            MarginLeft = 10,
            MarginBottom = 10,
            AlignSelf = YogaAlign.FlexStart,
        };

        header.Add(headerText);
        header.Add(topLine);
        header.Add(issueText);
        header.Add(bottomLine);

        var footer = new YogaNode(config)
        {
            Height = 10,
            MarginBottom = 5,
            MarginLeft = 10,
            MarginRight = 10,
            FlexDirection = YogaFlexDirection.Row,
            FlexGrow = 0,
            Name = "footer",
        };
        var footerLine = new HorizontalLineNode(config)
        {
            LineThickness = 1,
            Margin = 10
        };

        footer.Add(footerLine);

        var contentArea = new YogaNode(config)
        {
            FlexDirection = YogaFlexDirection.Row,
            FlexGrow = 1,
            Margin = 10,
            Name = "content"
        };

        var leftColumn = new YogaNode(config)
        {
            FlexGrow = 1,
            MarginLeft = 10,
            MarginRight = 10,
            Name = "left",
            BorderColor = new Color(0, 0, 255)
        };

        var middleColumn = new YogaNode(config)
        {
            FlexGrow = 2,
            MarginLeft = 10,
            MarginRight = 10,
            Name = "middle",
            BorderColor = new Color(0, 0, 255)
        };

        var rightColumn = new YogaNode(config)
        {
            FlexGrow = 1,
            MarginLeft = 10,
            MarginRight = 10,
            Name = "right",
            BorderColor = new Color(0, 0, 255)
        };

        contentArea.Add(leftColumn);
        contentArea.Add(middleColumn);
        contentArea.Add(rightColumn);

        root.StyleDirection = YogaDirection.LeftToRight;

        root.Add(header);
        root.Add(contentArea);
        root.Add(footer);

        ReCalculate(root, page);
        root.CalculateLayout();

        DrawNode(root, page);

        var documentBytes = builder.Build();

        File.WriteAllBytes("newPdf.pdf", documentBytes);
        Process.Start(new ProcessStartInfo("newPdf.pdf") { UseShellExecute = true });
    }

    private static void DrawNode(YogaNode root, PdfPageBuilder page, double offsetX = 0, double offsetY = 0)
    {
        foreach (var child in root.Children)
        {
            var absoluteX = offsetX + child.LayoutX;
            var absoluteY = offsetY + child.LayoutY;

            child.Draw(page, absoluteX, absoluteY);

            DrawNode(child, page, absoluteX, absoluteY);
        }
    }

    private static void ReCalculate(YogaNode root, PdfPageBuilder page)
    {
        foreach (var child in root.Children)
        {
            child.ReCalculate(page);

            ReCalculate(child, page);
        }
    }
}