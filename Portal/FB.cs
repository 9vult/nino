using System;

namespace Portal;

public class FBProject
{
    public string? airTime {get; set;}
    public string? anidb {get; set;}
    public bool done {get; set;}
    public Dictionary<string, FBEpisode>? episodes {get; set;}
    public Dictionary<string, FBStaff>? keyStaff {get; set;}
    public bool? isPrivate {get; set;}
    public int length {get; set;}
    public string nickname {get; set;}
    public string owner {get; set;}
    public string poster {get; set;}
    public string releaseChannel {get; set;}
    public string title {get; set;}
    public string type {get; set;}
    public string updateChannel {get; set;}
    public string[]? administrators {get; set;}
    public string[]? aliases {get; set;}
    public Dictionary<string, FBConga>? conga {get; set;}
    public string? motd {get; set;}
    public string? airReminderChannel {get; set;}
    public bool? airReminderEnabled {get; set;}
    public string? airReminderRole {get; set;}
}

public class FBEpisode
{
    public Dictionary<string, FBStaff>? additionalStaff {get; set;}
    public bool done {get; set;}
    public decimal number {get; set;}
    public Dictionary<string, FBTask>? tasks {get; set;}
    public long? updated {get; set;}
    public bool? airReminderPosted {get; set;}
}

public class FBStaff
{
    public string id {get; set;}
    public FBRole role {get; set;}
}

public class FBRole
{
    public string abbreviation {get; set;}
    public string title {get; set;}
    public decimal? weight {get; set;}
}

public class FBTask
{
    public string abbreviation {get; set;}
    public bool done {get; set;}
}

public class FBConga
{
    public string current {get; set;}
    public string next {get; set;}
}