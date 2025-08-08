# Creating a project using a JSON file

If you have multiple projects to create, or you have a template you use often, you might want to consider using the `/project create-from-json` command. This command essentially acts as a bundle of `/project create`, `/keystaff add`, `/additionalstaff add`, and `/project conga add`, allowing you to set up the majority of a project with one click.

## Fields

```typescript
{
    // Required fields
    Nickname: string,
    Title: string,
    Type: "TV" | "Movie" | "BD" | "OVA" | "ONA",
    Length: uint,
    IsPrivate: boolean,
    UpdateChannelId: ulong,
    ReleaseChannelId: ulong,
    KeyStaff: Staff[],
    AdditionalStaff: Map<string, Staff[]>,
	AniListId: int,

    // Optional fields
    PosterUri: string,
    FirstEpisode: decimal,
    AdministratorIds: ulong[],
    Aliases: string[],
    CongaParticipants: CongaNodeDto[]
}
```

## Example

```json

{
  "Nickname": "wolf",
  "Title": "Ookami to Koushinryou: MERCHANT MEETS THE WISE WOLF",
  "Type": "TV",
  "Length": 25,
  "PosterUri": "https://example.com/spicy-wolf-poster.png",
  "IsPrivate": false,
  "UpdateChannelId": 803139525312249906,
  "ReleaseChannelId": 804434067000393769,
  "AniListId": 145728,
  "Aliases": [
    "spice",
    "holo"
  ],
  "AdministratorIds": [
    134073223208763392
  ],
  "FirstEpisode": 1,
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
      "Abbreviation": "ED",
      "Type": "KeyStaff",
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

## Note

While this command will set up the majority of the project, other related steps will still need to be done manually - `/project conga-reminder enable` or `/project air-reminder enable`, for example.

I hope this feature proves useful to you. 
