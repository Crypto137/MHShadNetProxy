using System.Runtime.InteropServices;

namespace MHShadNetProxy
{
    public enum ShadNetPacketType : byte
    {
        Request = 0,
        Reply = 1,
        Notification = 2,
        ServerInfo = 3,
    }

    public enum ShadNetCommandType : ushort
    {
        Login = 0,
        Terminate = 1,
        Create = 2,
        Delete = 3,
        SendToken = 4,
        SendResetToken = 5,
        ResetPassword = 6,
        ResetState = 7,
        AddFriend = 8,
        RemoveFriend = 9,
        AddBlock = 10,
        RemoveBlock = 11,
        GetServerFeatures = 12,
        SetClientVersion = 13,
        GetBoardInfos = 30,
        RecordScore = 31,
        RecordScoreData = 32,
        GetScoreData = 33,
        GetScoreRange = 34,
        GetScoreFriends = 35,
        GetScoreNpid = 36,
        GetScoreAccountId = 37,
        GetScoreGameDataByAccId = 38,
        GetToken = 39,
        SetAppearOffline = 40,
        LookupOnlineId = 41,
        // Matchmaking
        ContextStart = 100,
        CreateRoom = 101,
        JoinRoom = 102,
        LeaveRoom = 103,
        SearchRoom = 104,
        RequestSignalingInfos = 105,
        ContextStop = 106,
        SetUserInfo = 107,
        SetRoomDataInternal = 108,
        SetRoomDataExternal = 109,
        KickoutRoomMember = 110,
        GetWorldInfoList = 111,
        GetRoomDataExternalList = 112,
        GetUserInfoList = 113,
        GetRoomMemberDataExternalList = 114,
        SendRoomMessage = 115,
        // Title User Storage (TUS)
        TusSetData = 201,
        TusGetData = 202,
        TusSetMultiSlotVariable = 203,
        TusGetMultiSlotVariable = 204,
        TusAddAndGetVariable = 205,
        TusGetMultiSlotDataStatus = 206,
        TusGetMultiUserDataStatus = 207,
        TusGetFriendsDataStatus = 208,
        TusDeleteMultiSlotData = 209,
        TusGetMultiUserVariable = 210,
        TusTryAndSetVariable = 211,
        TusGetFriendsVariable = 212,
        TusDeleteMultiSlotVariable = 213,
        TssGetData = 214,
        // Trophies
        UnlockTrophy = 301,
        SyncTrophies = 302,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShadNetPacketHeader
    {
        public ShadNetPacketType Type;
        public ShadNetCommandType Command;
        public int Size;
        public ulong PacketId;
    }
}
