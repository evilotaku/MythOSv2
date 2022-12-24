/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     InworldCharacterData is the data of inworld character.
    ///     It could be created locally, but mostly loaded and updated from Server.
    ///     `Inworld Studio Server` could update everything except `ID`
    ///     `Inworld Runtime Server` could update its `ID`.
    ///     NOTE:
    ///     The file of InworldCharacterData is stored via CharacterName locally.
    ///     However, CharacterName is not unique when retrieving from server.
    ///     So please make sure the `CharacterName` at your side is set as unique to prevent data collision.
    /// </summary>
    [CreateAssetMenu(fileName = "New NPC", menuName = "Inworld/Character", order = 1)]
    public class InworldCharacterData : ScriptableObject
    {
        /// <summary>
        ///     Copy All the data from another Data object that shares the same brain
        ///     NOTE: characterID would also be updated.
        /// </summary>
        /// <param name="rhs">The source of the InworldCharacterData</param>
        public void CopyFrom(InworldCharacterData rhs)
        {
            if (brain != rhs.brain)
                return;
            characterID = rhs.characterID;
            characterName = rhs.characterName;
            workspace = rhs.workspace;
            currentScene = rhs.currentScene;
            ownerShip = rhs.ownerShip;
            triggers.Clear();
            triggers.AddRange(rhs.triggers);
            scenes.Clear();
            scenes.AddRange(rhs.scenes);
            modelUri = rhs.modelUri;
            posUri = rhs.posUri;
            previewImgUri = rhs.previewImgUri;
        }

        #region Public Variables
        [Space(16)]
        public string characterName; //= givenName;
        public string brain;
        //public Texture2D thumbnail;
        public GameObject avatar;
        public string workspace;
        public string currentScene;
        public Ownership ownerShip = Ownership.Owned;
        public Texture2D defaultThumbnail;
        public int index;
        [Space(16)]
        [Header("Inworld Scenes & Triggers:")]
        public List<string> triggers;
        public List<string> scenes;
        [Space(16)][Header("URI References:")]
        public string modelUri;
        public string posUri;
        public string previewImgUri;
        #endregion

        #region Private Properties
        const string k_ResourcePath = "Assets/Inworld.AI/Resources";
        [HideInInspector] public string characterID; // = incomingID;
        #endregion

        #region Properties
        public string CharacterName => index <= 0 ? characterName : $"{characterName}_{index}";

        /// <summary>
        ///     Get the Character's Local Thumbnail File Name
        ///     If it's in Editor Mode:
        ///     The data would be `Resources/{InworldAI.User.Name}/{InworldAI.Settings.ThumbnailPath}/characterName.png
        ///     If it's in runtime Mode.
        ///     The data would be in {Application.persistentDataPath}/{InworldAI.Settings.ThumbnailPath}/xxxx-xxxx-xxxx.png.
        /// </summary>
        public string LocalThumbnailFileName
        {
            get
            {
                if (Application.isPlaying) // YAN: It would be like "xxxx-xxxx-xxxx.png" in persistantData folder.
                {
                    string folder = $"{Application.persistentDataPath}/{InworldAI.Settings.ThumbnailPath}";
                    if (!File.Exists(folder))
                        Directory.CreateDirectory(folder);
                    // string fileName = previewImgUri.Remove(0, previewImgUri.LastIndexOf('/'));
                    // return fileName;
                    return $"{folder}/{CharacterName}.png";
                }
                else // YAN: It would be like "characterName.png" In Resource folder.
                {
                    string userFolder = $"{k_ResourcePath}/{InworldAI.User.Name}";
                    if (!File.Exists(userFolder))
                        Directory.CreateDirectory(userFolder);
                    string folder = $"{userFolder}/{InworldAI.Settings.ThumbnailPath}";
                    if (!File.Exists(folder))
                        Directory.CreateDirectory(folder);
                    return $"{folder}/{CharacterName}.png";
                }
            }
        }
        /// <summary>
        ///     Get the Character's Local Avatar File Name.
        ///     If it's in Editor Mode:
        ///     The data would be `Resources/{InworldAI.User.Name}/{InworldAI.Settings.AvatarPath}/characterName.glb
        ///     If it's in runtime Mode.
        ///     The data would be in {Application.persistentDataPath}/{InworldAI.Settings.AvatarPath}/xxxx-xxxx-xxxx.glb.
        /// </summary>
        public string LocalAvatarFileName
        {
            get
            {
                if (Application.isPlaying)
                {
                    string folder = $"{Application.persistentDataPath}/{InworldAI.Settings.AvatarPath}";
                    if (!File.Exists(folder))
                        Directory.CreateDirectory(folder);
                    // string fileName = modelUri.Remove(0, modelUri.LastIndexOf('/'));
                    // return fileName;
                    return $"{folder}/{CharacterName}.glb";
                }
                else
                {
                    string userFolder = $"{k_ResourcePath}/{InworldAI.User.Name}";
                    if (!File.Exists(userFolder))
                        Directory.CreateDirectory(userFolder);
                    string folder = $"{userFolder}/{InworldAI.Settings.AvatarPath}";
                    if (!File.Exists(folder))
                        Directory.CreateDirectory(folder);
                    return $"{folder}/{CharacterName}.glb";
                }
            }
        }
        /// <summary>
        ///     Get the Character's Thumbnail.
        ///     If it has the data, return directly.
        ///     Otherwise, if it contains previewImgUri, download and load it.
        ///     Otherwise, returns the default Thumbnail.
        /// </summary>
        public Texture2D Thumbnail
        {
            get
            {
                if (defaultThumbnail)
                    return defaultThumbnail;
                Texture2D loadFromEditor = Resources.Load<Texture2D>($"{InworldAI.User.Name}/{InworldAI.Settings.ThumbnailPath}/{CharacterName}");
                if (loadFromEditor)
                    return loadFromEditor;
                if (!File.Exists(LocalThumbnailFileName))
                {
                    return null;
                }
                Texture2D loadFromDisk = new Texture2D(0, 0);
                loadFromDisk.LoadImage(File.ReadAllBytes(LocalThumbnailFileName));
                return loadFromDisk;
            }
        }

        /// <summary>
        ///     Get the Characters' Avatar (.glb format)
        ///     If it has the data, return directly,
        ///     Otherwise, if it contains ModelUri, download and load it,
        ///     Otherwise, returns the default avatar.
        /// </summary>
        public GameObject Avatar => avatar
            ? avatar
            : string.IsNullOrEmpty(modelUri)
                ? InworldAI.Settings.DefaultAvatar
                : File.Exists(LocalAvatarFileName)
                    ? _LoadAvatar()
                    : null;
        /// <summary>
        ///     Returns the data fetching progress of the Character.
        ///     TODO: Compare the data size of the files instead of checking if it exists.
        /// </summary>
        public float Progress
        {
            get
            {
                float fThumbnailExists = File.Exists(LocalThumbnailFileName) ? 0.2f : 0;
                float fAvatarExists = File.Exists(LocalAvatarFileName) ? 0.8f : 0;
                return fThumbnailExists + fAvatarExists;
            }
        }
        #endregion

        #region Private Functions
        Texture2D _LoadImage()
        {
            if (!File.Exists(LocalThumbnailFileName))
                return null;

            byte[] imageBytes = File.ReadAllBytes(LocalThumbnailFileName);
            Texture2D textureForImage = new Texture2D(0, 0);
            textureForImage.LoadImage(imageBytes);
            return textureForImage;
        }
        GameObject _LoadAvatar()
        {
            return InworldAI.AvatarLoader.LoadData(LocalAvatarFileName);
        }
        #endregion
    }
}
