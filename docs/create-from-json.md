# Creating a project using a JSON file

If you have multiple projects to create, or you have a template you use often, you might want to consider using the `/project create-from-json` command. This command essentially acts as a bundle of `/project create`, `/keystaff add`, `/additionalstaff add`, and `/project conga add`, allowing you to set up the majority of a project with one click.

## Fields

```typescript
{
    // Required fields
    Nickname: string,
    AniListId: number,
    IsPrivate: boolean,
    UpdateChannelId: number,
    ReleaseChannelId: number,
    KeyStaff: Staff[],

    // Optional fields
    Title: string,
    Type: "TV" | "Movie" | "BD" | "OVA" | "ONA",
    Length: number,
    PosterUri: string,
    FirstEpisode: number,
    AdministratorIds: number[],
    Aliases: string[],
    CongaParticipants: CongaNode[],
    AdditionalStaff: Map<string, Staff[]>
}
```

## Types

### Staff

```typescript
{
  UserId: number,
  Role: {
    Abbreviation: string,
    Name: string,
    Weight: number // Optional
  }
  IsPseudo: boolean
}
```

### CongaNode

```typescript
{
  Abbreviation: string,
  Type: "KeyStaff" | "AdditionalStaff" | "Special",
  Dependents: string[]
}
```

## Examples

### Minimal

```json
{
  "Nickname": "frieren",
  "AniListId": 154587,
  "IsPrivate": false,
  "UpdateChannelId": 803139525312249906,
  "ReleaseChannelId": 804434067000393769,
  "KeyStaff": [
    {
      "UserId": 248600185423396866,
      "Role": {
        "Abbreviation": "ED",
        "Name": "Editing"
      },
      "IsPseudo": false
    },
    {
      "UserId": 134073223208763392,
      "Role": {
        "Abbreviation": "QC",
        "Name": "Quality Checking"
      },
      "IsPseudo": false
    }
  ]
}
```

### Full

```json
{
  "Nickname": "wolf",
  "AniListId": 145728,
  "IsPrivate": false,
  "UpdateChannelId": 803139525312249906,
  "ReleaseChannelId": 804434067000393769,
  "Aliases": ["spice", "holo"],
  "AdministratorIds": [134073223208763392],
  "FirstEpisode": 1,
  "KeyStaff": [
    {
      "UserId": 248600185423396866,
      "Role": {
        "Abbreviation": "ED",
        "Name": "Editing",
        "Weight": 1
      },
      "IsPseudo": false
    },
    {
      "UserId": 134073223208763392,
      "Role": {
        "Abbreviation": "QC",
        "Name": "Quality Checking",
        "Weight": 2
      },
      "IsPseudo": false
    }
  ],
  "AdditionalStaff": {
    "1": [
      {
        "UserId": 248600185423396866,
        "Role": {
          "Abbreviation": "KFX",
          "Name": "Song Styling",
          "Weight": 1.5
        },
        "IsPseudo": false
      }
    ]
  },
  "CongaParticipants": [
    {
      "Abbreviation": "$AIR",
      "Type": "Special",
      "Dependents": ["ED"]
    },
    {
      "Abbreviation": "ED",
      "Type": "KeyStaff",
      "Dependents": ["QC"]
    },
    {
      "Abbreviation": "KFX",
      "Type": "AdditionalStaff",
      "Dependents": ["QC"]
    },
    {
      "Abbreviation": "QC",
      "Type": "KeyStaff",
      "Dependents": []
    }
  ]
}
```

By default, AniList is used for the following fields, but you can supply them manually if desired. You may need to manually supply some or all of them if the data is missing from AniList.

```json
{
  "Title": "Ookami to Koushinryou: MERCHANT MEETS THE WISE WOLF",
  "Type": "TV",
  "Length": 25,
  "PosterUri": "https://example.com/spicy-wolf-poster.png"
}
```

## Minimal Copypasta

```json
{
  "Nickname": "",
  "AniListId": 0,
  "IsPrivate": false,
  "UpdateChannelId": 0,
  "ReleaseChannelId": 0,
  "KeyStaff": [
    {
      "UserId": 0,
      "Role": {
        "Abbreviation": "",
        "Name": ""
      },
      "IsPseudo": false
    }
  ]
}
```

## Note

While this command will set up the majority of the project, other related steps will still need to be done manually - `/project conga-reminder enable` or `/project air-reminder enable`, for example.

I hope this feature proves useful to you.
