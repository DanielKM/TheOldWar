using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoDisplay : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    private ulong steamId;

    [SerializeField] private RawImage profileImage = null;
    [SerializeField] private Text displayNameText = null;
    [SyncVar(hook = nameof(HandleSteamNameUpdated))]
    public string steamName = null;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    public string GetSteamName() 
    {
        return steamName;
    }

    #region Server

    public void SetSteamId(ulong steamId)
    {
        this.steamId = steamId;
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);

        steamName = SteamFriends.GetFriendPersonaName(cSteamId);

        displayNameText.text = steamName;

        int imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);

        if(imageId == -1) { return; }

        // profileImage.texture = GetSteamImageAsTexture(imageId);
    }

    private void HandleSteamNameUpdated(string oldSteamName, string newSteamName)
    {
        steamName = newSteamName;

        displayNameText.text = steamName;
    }


    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if(callback.m_steamID.m_SteamID != steamId) { return; }

        profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if(isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if(isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }

    #endregion
}
