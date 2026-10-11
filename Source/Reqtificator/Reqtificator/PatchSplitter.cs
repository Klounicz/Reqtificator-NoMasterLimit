using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Serilog;

namespace Reqtificator
{
    internal class SplitPart
    {
        public SkyrimMod Mod { get; }
        public HashSet<ModKey> Masters { get; } = new HashSet<ModKey>();

        public SplitPart(SkyrimMod mod)
        {
            Mod = mod;
        }
    }

    /// <summary>
    ///     Splits a generated patch into multiple plugins so that no single plugin
    ///     exceeds the engine limit of 255 masters in its master list.
    ///     Records created by the patch itself (new FormKeys owned by the patch)
    ///     always stay in the primary plugin, so cross references between split
    ///     parts only ever point back to the primary plugin or to original mods.
    /// </summary>
    internal static class PatchSplitter
    {
        public static List<SkyrimMod> SplitIntoPlugins(SkyrimMod patch, int maxMastersPerPlugin)
        {
            var parts = new List<SplitPart> { new SplitPart(CreatePartMod(patch, patch.ModKey, 1)) };

            void Distribute<T>(Group<T> source, Func<SkyrimMod, Group<T>> selector, bool keepInMain = false)
                where T : class, ISkyrimMajorRecordInternal, IBinaryItem, IMajorRecordGetter,
                    IFormLinkContainerGetter, IMajorRecordIdentifier
            {
                var records = new List<T>();
                foreach (var existing in source)
                {
                    records.Add(existing);
                }

                foreach (var rec in records)
                {
                    SplitPart part;
                    if (keepInMain || rec.FormKey.ModKey == patch.ModKey)
                    {
                        part = parts[0];
                    }
                    else
                    {
                        part = PickPart(parts, maxMastersPerPlugin, rec);
                    }

                    part.Masters.UnionWith(NeededMasters(rec));
                    selector(part.Mod).Add(rec);
                }
            }

            // globals and formlists are tiny, keep them together with the new records
            Distribute(patch.Globals, m => m.Globals, keepInMain: true);
            Distribute(patch.FormLists, m => m.FormLists, keepInMain: true);
            Distribute(patch.Ammunitions, m => m.Ammunitions);
            Distribute(patch.EncounterZones, m => m.EncounterZones);
            Distribute(patch.Doors, m => m.Doors);
            Distribute(patch.Containers, m => m.Containers);
            Distribute(patch.LeveledItems, m => m.LeveledItems);
            Distribute(patch.LeveledNpcs, m => m.LeveledNpcs);
            Distribute(patch.Armors, m => m.Armors);
            Distribute(patch.Weapons, m => m.Weapons);
            Distribute(patch.Npcs, m => m.Npcs);
            Distribute(patch.Races, m => m.Races);

            if (parts[0].Masters.Count > maxMastersPerPlugin)
            {
                Log.Warning(
                    $"primary patch part references {parts[0].Masters.Count} masters, above the target of {maxMastersPerPlugin}");
            }

            if (parts.Count > 1)
            {
                Log.Information($"patch split into {parts.Count} plugins to respect the master list limit");
            }

            return parts.Select(p => p.Mod).ToList();
        }

        private static SplitPart PickPart(List<SplitPart> parts, int maxMastersPerPlugin, IFormLinkContainerGetter rec)
        {
            var needed = NeededMasters(rec);
            foreach (var part in parts)
            {
                var newMasters = needed.Count(m => !part.Masters.Contains(m));
                if (part.Masters.Count + newMasters <= maxMastersPerPlugin)
                {
                    return part;
                }
            }

            var created = new SplitPart(CreatePartMod(parts[0].Mod, parts[0].Mod.ModKey, parts.Count + 1));
            parts.Add(created);
            return created;
        }

        private static HashSet<ModKey> NeededMasters(IFormLinkContainerGetter rec)
        {
            var result = new HashSet<ModKey>();
            var major = rec as IMajorRecordGetter;
            if (major != null)
            {
                result.Add(major.FormKey.ModKey);
            }

            foreach (var link in rec.ContainedFormLinks)
            {
                var key = link.FormKeyNullable;
                if (key != null)
                {
                    result.Add(key.Value.ModKey);
                }
            }

            return result;
        }

        private static SkyrimMod CreatePartMod(SkyrimMod template, ModKey mainKey, int partNumber)
        {
            var key = partNumber == 1
                ? mainKey
                : ModKey.FromNameAndExtension(
                    $"{Path.GetFileNameWithoutExtension(mainKey.FileName)} {partNumber}.esp");
            var mod = new SkyrimMod(key, SkyrimRelease.SkyrimSE)
            {
                ModHeader =
                {
                    Author = template.ModHeader.Author,
                    Description = template.ModHeader.Description +
                                  (partNumber == 1 ? "" : $" -- part {partNumber} of the split patch")
                }
            };
            return mod;
        }
    }
}
