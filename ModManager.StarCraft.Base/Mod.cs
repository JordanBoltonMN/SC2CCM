﻿using System.IO;
using ModManager.StarCraft.Services.Tracing;

namespace ModManager.StarCraft.Base
{
    public class Mod
    {
        public Mod(ModMetadata modMetadata, string metadataFilePath)
        {
            this.Metadata = modMetadata;
            this.MetadataFilePath = metadataFilePath;
            this.IsActive = false;
        }

        public static bool TryCreate(ITracingService tracingService, string metadataFilePath, out Mod mod)
        {
            if (
                !File.Exists(metadataFilePath)
                || !ModMetadata.TryCreate(
                    tracingService,
                    File.ReadAllText(metadataFilePath),
                    out ModMetadata modMetadata
                )
            )
            {
                mod = null;
                return false;
            }

            mod = new Mod(modMetadata, metadataFilePath);
            return true;
        }

        public ModMetadata Metadata { get; }
        public string MetadataFilePath { get; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"{this.Metadata.Title} ({this.Metadata.Version})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Mod other)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals(Mod other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Metadata.Equals(other.Metadata);
        }

        public override int GetHashCode()
        {
            return this.Metadata.GetHashCode();
        }
    }
}
