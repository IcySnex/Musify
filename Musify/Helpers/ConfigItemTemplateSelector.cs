﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Plugins.Models;

namespace Musify.Helpers;

public class ConfigItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate InvalidTemplate { get; set; } = default!;

    public DataTemplate StringTemplate { get; set; } = default!;

    public DataTemplate LongTemplate { get; set; } = default!;

    public DataTemplate BoolTemplate { get; set; } = default!;


    protected override DataTemplate SelectTemplateCore(
        object item,
        DependencyObject container)
    {
        if (item is not PluginConfigItem configItem)
            return base.SelectTemplateCore(item, container);

        switch (Type.GetTypeCode(configItem.Value.GetType()))
        {
            case TypeCode.String:
                return StringTemplate;
            case TypeCode.Int64:
                return LongTemplate;
            case TypeCode.Boolean:
                return BoolTemplate;
            default:
                return InvalidTemplate;
        }
    }
}