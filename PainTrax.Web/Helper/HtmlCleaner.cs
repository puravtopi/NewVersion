﻿using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

public class HtmlCleaner
{
    public static string CleanHtmlContent(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        RemoveVisuallyEmptyParagraphs(doc);
        ConvertParagraphsToBreaks(doc);
        //RemoveExtraBreakTags(doc);
     
        return doc.DocumentNode.OuterHtml;

    }
    private static void RemoveVisuallyEmptyParagraphs(HtmlDocument doc)
    {
        var paragraphs = doc.DocumentNode.SelectNodes("//p");
        if (paragraphs == null) return;

        foreach (var p in paragraphs.ToList())
        {
            string innerHtml = p.InnerHtml?.ToLowerInvariant().Trim();
            string innerText = HtmlEntity.DeEntitize(p.InnerText).Trim();

            // Normalize innerHtml for <br> variations
            string normalized = innerHtml
                .Replace("<br>", "")
                .Replace("<br/>", "")
                .Replace("<br />", "")
                .Replace("&nbsp;", "")
                .Replace("&#160;", "") // Unicode for &nbsp;
                .Trim();

            // If it's truly empty or contains only visual fillers
            if (string.IsNullOrWhiteSpace(innerText) && string.IsNullOrEmpty(normalized))
            {
                p.Remove();
            }
        }
    }

    private static void RemoveExtraBreakTags(HtmlDocument doc)
    {
        // Replace all sequences of multiple <br> with just one
        string html = doc.DocumentNode.OuterHtml;

        // Normalize <br> tags
        html = Regex.Replace(html, @"(<br\s*/?>\s*){2,}", "<br>"); // keep one <br>
        html = Regex.Replace(html, @"(&nbsp;\s*){2,}", "&nbsp;"); // keep one &nbsp;
        html = Regex.Replace(html, @"</p>\s*<br\s*/?>", "</p>", RegexOptions.IgnoreCase);

        doc.LoadHtml(html); // Reload cleaned HTML back into doc
    }

    private static void ConvertParagraphsToBreaks(HtmlDocument doc)
    {
        var paragraphs = doc.DocumentNode.SelectNodes("//p");
        if (paragraphs == null) return;

        foreach (var p in paragraphs.ToList())
        {
            var newFragment = HtmlNode.CreateNode("<span></span>");
            // Append all child nodes from <p>
            foreach (var child in p.ChildNodes.ToList())
            {
                newFragment.AppendChild(child);
            }

            // Add a <br/> after the paragraph content
            var brNode = doc.CreateElement("br");
            var brNode1 = doc.CreateElement("br");
            newFragment.AppendChild(brNode);
            newFragment.AppendChild(brNode1);

            // Replace the <p> with the new fragment
            p.ParentNode.ReplaceChild(newFragment, p);
        }
    }



}
