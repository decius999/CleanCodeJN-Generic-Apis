﻿using System.Text.Json;
using CleanCodeJN.GenericApis.Models;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ApiExtensions
{
    public static string GetSortOrder(this string direction) => direction?.Contains("desc") == true ? "-1" : "1";

    public static SearchFilter GetFilter(this string filter) => filter is null ? null : JsonSerializer.Deserialize<SearchFilter>(filter);
}
