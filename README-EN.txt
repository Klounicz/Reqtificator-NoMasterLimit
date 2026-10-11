========================================================================
 Reqtificator No Master Limit
 Modified Reqtificator without the 255-master limit
========================================================================

VERSION
--------
1.1.0 - documentation update. The tool itself is unchanged and
        identical to 1.0.0; the READMEs now cover the real-world
        issues people run into after a split (parts landing in the
        MO2 overwrite folder, stale patch copies in other mods).
1.0.0 - first release with automatic patch splitting.

WHAT THIS IS
------------
The official Reqtificator (from the Requiem mod) crashes with
"Patch export failed! / Too many masters" once your load order needs
more than 255 masters in one plugin. This is a hard limit of the
Bethesda plugin format and cannot be worked around with ESL flags.

This build of the Reqtificator automatically splits the resulting
patch "Requiem for the Indifferent.esp" into several plugins when the
masters no longer fit into a single file. Records created by the
patch itself always stay in the primary plugin, so load order and
record integrity are preserved.

WHAT CHANGED
------------
- Added PatchSplitter.cs (splits the patch into parts of at most 250
  masters each).
- Changed Backend.cs: on a "too many masters" error the patch is
  written in parts instead of aborting.
- Self-contained build (.NET 5, win-x64) - no .NET runtime needs to
  be installed.

HOW TO INSTALL (Mod Organizer 2)
--------------------------------
1. Extract the archive.
2. Drop the contents into MO2 as a mod (the Reqtificator.bat file and
   the Reqtificator folder), or copy those two items into the root of
   your existing Requiem mod (next to Requiem.esp).
   When updating an existing installation, just replace the
   Reqtificator\app folder with the new one.
3. If installed as a separate mod from Requiem, make sure the
   Reqtificator folder (with Config, Data) is visible in the virtual
   Data folder.
4. The Source folder is optional - it is the source code for
   reference only.

HOW TO RUN
----------
Exactly as before: the "Reqtificator" shortcut/executable in MO2 (or
your usual launcher). No extra configuration is needed.

RESULT
------
If there are fewer than 255 masters, the patch is written exactly as
before, as a single "Requiem for the Indifferent.esp" file.

If there are more than 255 masters, the log will show:
  patch exceeds the master list limit, retrying as a split patch
  wrote patch part Requiem for the Indifferent.esp
  wrote patch part Requiem for the Indifferent 2.esp
  ...
  The patch was split into N plugins due to the master list limit.

IMPORTANT: after a split, ALL parts must be enabled in your load
order:
  - the parts go right after each other in numeric order
    (the primary part, then ... 2.esp, 3.esp, etc.);
  - all parts load AFTER every other mod;
  - the primary part "Requiem for the Indifferent.esp" stays where it
    already was, so your existing load order does not need to change.

AFTER THE SPLIT - CLEANUP CHECKLIST (read this!)
------------------------------------------------
1. WHERE ARE THE PARTS? The primary part is written into the mod
   folder that already contained the old patch (MO2 keeps overwriting
   the existing file). Extra parts (Requiem for the Indifferent
   2.esp, 3.esp, ...) are NEW file names, so MO2's VFS puts them
   into the "overwrite" folder. Move every extra part from
   overwrite into the SAME mod as the primary part, otherwise the
   parts may disappear the next time you clear overwrite.

2. DELETE STALE COPIES. If several of your mods contain a file named
   "Requiem for the Indifferent.esp" (old backups, old patch
   versions), MO2 will load only the one from the mod with the
   HIGHEST priority - which may be an OLD, broken patch, while the
   new parts 2/3 load on top of it. The result is a mismatched
   patch and errors like "Requiem could not initialize all core
   scripts". Keep the new patch in exactly ONE mod and remove
   (rename with a different extension, e.g. .esp.old) every other
   copy in your load order.

3. CHECK ORDER. After moving the parts, the load order must read:
       Requiem for the Indifferent.esp
       Requiem for the Indifferent 2.esp
       Requiem for the Indifferent 3.esp   (if present)
   directly after each other. Run LOOT, then verify they are still
   adjacent.

4. START A NEW GAME after a re-patch. Requiem validates the patch on
   savegame start; a save made with a different patch layout can
   fail its core script initialization.

COMPATIBILITY
-------------
- Skyrim Special Edition / Anniversary Edition (Mutagen 0.30.6)
- Requiem - The Roleplaying Overhaul (Reqtificator 6.x)
- Windows x64

SOURCE CODE
-----------
The Source folder contains the full modified source code (see
PatchSplitter.cs and Backend.cs). To build it: .NET SDK 5+ and
`dotnet publish -c Release -r win-x64 --self-contained true`.

LIMITATIONS
-----------
- Splitting only kicks in when the limit is exceeded; normal runs are
  unchanged.
- If the "new" content of the patch alone (globals, form lists, new
  records) references more than 250 mods, splitting cannot help - in
  that case reduce your load order.

Credits: original Reqtificator by the Requiem Team.
========================================================================
