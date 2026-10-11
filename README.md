# Reqtificator-NoMasterLimit

> Reqtificator that no longer crashes with "Patch export failed! — too many masters". Automatically splits the patch when your load order needs more than 255 masters.

## Download

Go to Releases and download `Reqtificator-NoMasterLimit-1.1.0.zip`. See installation instructions below.

## What changed vs official Reqtificator

- Added `PatchSplitter.cs` (splits the patch into parts of at most 250 masters each).
- Changed `Backend.cs`: on a "too many masters" error the patch is written in parts instead of aborting.
- Self-contained build (.NET 5, win-x64) — no .NET runtime needs to be installed.

## Install (Mod Organizer 2)

1. Extract the release archive.
2. Drop the contents into MO2 as a mod (`Reqtificator.bat` + `Reqtificator` folder), or copy those two items into your existing Requiem mod next to `Requiem.esp`.
3. `Source/` in this repo is for reference only, you don't need it in MO2.

## After a split — read this!

1. The primary part overwrites the old patch. Extra parts (`Requiem for the Indifferent 2.esp`, `3.esp`, ...) are NEW files, so MO2 puts them into the **overwrite** folder. Move them into the SAME mod as the primary part.
2. Delete stale copies of `Requiem for the Indifferent.esp` in other mods — MO2 loads only the highest-priority one, which may be an old broken patch.

Full instructions: [README-EN.txt](README-EN.txt) | [README-RU.txt](README-RU.txt) (Russian)

## Build from source

Requirements: Windows x64, [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) (targets `net5.0`, WPF).

```powershell
# build the app
dotnet build Source/Reqtificator/Reqtificator/Reqtificator.csproj -c Release

# self-contained publish, as used for the release zip (no runtime install needed)
dotnet publish Source/Reqtificator/Reqtificator/Reqtificator.csproj -c Release -r win-x64 --self-contained true -o Reqtificator/app

# run tests (optional)
dotnet test Source/Reqtificator/ReqtificatorTest/ReqtificatorTest.csproj
```

Key changed files: `Source/Reqtificator/Reqtificator/PatchSplitter.cs` (new), `Source/Reqtificator/Reqtificator/Backend.cs` (split on "too many masters" instead of aborting).

## Version

- **1.1.0** — documentation update, tool identical to 1.0.0.
- **1.0.0** — first release with automatic patch splitting.

Credits: original Reqtificator by the Requiem Team.
