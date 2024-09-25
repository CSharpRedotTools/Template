﻿using Godot;

namespace Template.Inventory;

public class Item
{
    public string Name { get; private set; }
    public int Count { get; set; }
    public Color Color { get; private set; }
    public string ResourcePath { get; private set; }

    private Item(string name, int count = 1)
    {
        Name = name;
        Count = count;
    }

    public Item(Item other)
    {
        Name = other.Name;
        Count = other.Count;
        Color = other.Color;
        ResourcePath = other.ResourcePath;
    }

    public bool Equals(Item other)
    {
        if (other == null)
            return false;

        return Name == other.Name;
    }

    public override string ToString()
    {
        return $"{Name} (x{Count})";
    }

    public class Builder(string name, int count = 1)
    {
        private const string BaseResourcePath = "res://Sandbox/Inventory/";

        private Item _item = new(name, count);

        public Builder SetResourcePath(string resourcePath)
        {
            _item.ResourcePath = $"{BaseResourcePath}{resourcePath}";
            return this;
        }

        public Builder SetColor(Color color)
        {
            _item.Color = color;
            return this;
        }

        public Item Build() => _item;
    }
}