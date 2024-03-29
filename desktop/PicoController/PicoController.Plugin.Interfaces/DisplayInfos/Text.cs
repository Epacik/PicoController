﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin.DisplayInfos;

public class Text
{
    public Text(string text, double fontSize = 14, int fontWeight = 400)
    {
        Content    = text;
        FontSize   = fontSize;
        FontWeight = fontWeight;
    }

    public string Content { get; }
    public double FontSize { get; }
    public int FontWeight { get; }

    public void Deconstruct(out string content, out double fontSize, out int fontWeight)
    {
        content    = Content;
        fontSize   = FontSize;
        fontWeight = FontWeight;
    }

    public override bool Equals(object? other)
        => other is Text otherText
        && Content.Equals(otherText.Content)
        && FontSize.Equals(otherText.FontSize)
        && FontWeight.Equals(otherText.FontWeight);

    public static bool operator ==(Text one, Text other)
        => one.Content    == other.Content
        && one.FontSize   == other.FontSize
        && one.FontWeight == other.FontWeight;

    public static bool operator !=(Text one, Text other)
        => one.Content    != other.Content
        || one.FontSize   != other.FontSize
        || one.FontWeight != other.FontWeight;

    public override int GetHashCode()
        => HashCode.Combine(Content, FontSize, FontWeight);

    public override string ToString()
    {
        return $"Text {{Content: {Content}, FontSize: {FontSize}, FontWeight: {FontWeight}}}";
    }
}
