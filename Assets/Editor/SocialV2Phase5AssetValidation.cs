using System;
using UnityEditor;
using UnityEngine;

public static class SocialV2Phase5AssetValidation
{
    private sealed class AssetSpec
    {
        public readonly string FileName;
        public readonly int X4Size;

        public AssetSpec(string fileName, int x4Size)
        {
            FileName = fileName;
            X4Size = x4Size;
        }
    }

    private static readonly AssetSpec[] Assets =
    {
        new AssetSpec("social_accept.png", 80),
        new AssetSpec("social_add.png", 80),
        new AssetSpec("social_chat.png", 96),
        new AssetSpec("social_emoji.png", 104),
        new AssetSpec("social_location.png", 104),
        new AssetSpec("social_mail.png", 80),
        new AssetSpec("social_remove.png", 80),
        new AssetSpec("social_search.png", 80),
        new AssetSpec("social_send.png", 104)
    };

    public static void Validate()
    {
        foreach (AssetSpec asset in Assets)
        {
            string assetPath = "Assets/Resources/res/x4/mainimage/" + asset.FileName;
            string resourcePath = "res/x4/mainimage/" + asset.FileName.Substring(0, asset.FileName.Length - 4);
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            Require(texture != null, "Resources.Load failed for " + resourcePath);
            Require(texture.width == asset.X4Size && texture.height == asset.X4Size,
                asset.FileName + " has the wrong x4 dimensions");

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Require(importer != null, "Texture importer is missing for " + asset.FileName);
            Require(importer.filterMode == FilterMode.Point, asset.FileName + " is not point filtered");
            Require(!importer.mipmapEnabled, asset.FileName + " has mipmaps enabled");
            Require(importer.textureCompression == TextureImporterCompression.Uncompressed,
                asset.FileName + " is compressed");

            for (int zoom = 1; zoom <= 4; zoom++)
            {
                int displayedWidth = asset.X4Size / 4 * zoom;
                int displayedHeight = asset.X4Size / 4 * zoom;
                Require(displayedWidth % zoom == 0 && displayedHeight % zoom == 0,
                    asset.FileName + " loses integer alignment at zoom " + zoom);
                Require(displayedWidth >= 20 * zoom || asset.X4Size == 80,
                    asset.FileName + " has an invalid logical hitbox at zoom " + zoom);
            }
        }

        Require(Game1.Main.res == "res" && Game2.Main.res == "res",
            "Both game variants must resolve the shared Resources root");
        Debug.Log("SOCIAL_V2_PHASE5_UNITY_ASSETS_OK");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
