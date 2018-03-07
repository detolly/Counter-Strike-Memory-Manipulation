using static hazedumper.signatures;
using static hazedumper.netvars;

namespace newHack
{
    public static class Offsets
    {
        //Mostly Static 
        public static int oFlags = m_fFlags;
        public static int oHealth = m_iHealth;
        public static int oTeam = m_iTeamNum;
        public static int oDormant = 0x000000E9;
        public static int oGlowIndex = m_iGlowIndex;
        public static int oCrossHairID = m_iCrosshairId;
        public static int oFlashMaxAlpha = m_flFlashMaxAlpha;
        public static int oEntityLoopDistance = 0x10;
        public static int oViewAngles = dwClientState_ViewAngles;
        public static int vecOrigin = m_vecOrigin;
        public static int dwBoneMatrix = m_dwBoneMatrix;
        public static int dwPunchOffset = m_aimPunchAngle;
        public static int oSpotted = m_bSpotted;
        //Common Changes
        public static int oViewMatrix = dwViewMatrix;
        public static int oJump = dwForceJump;
        public static int oLocalPlayer = dwLocalPlayer;
        public static int oClientState = dwClientState;
        public static int oEntityList = dwEntityList;
        public static int oGlowObject = dwGlowObjectManager;
        public static int oAttack = dwForceAttack;
        public static int radarbase = dwRadarBase;
    }

    //public static partial class Offsets
    //{
    //    private static string fileName = "~/.csHackFiles/thing.ini";
    //    static FileIniDataParser parser = new FileIniDataParser();
    //    static IniData data = parser.ReadFile(fileName);

    //    public static int dwLocalPlayer
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwLocalPlayer"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwLocalPlayer"] = value.ToString();
    //        }
    //    }
    //    public static int dwViewMatrix
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwViewMatrix"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwViewMatrix"] = value.ToString();
    //        }
    //    }
    //    public static int dwForceJump
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwForceJump"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwForceJump"] = value.ToString();
    //        }
    //    }
    //    public static int dwClientState
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwClientState"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwClientState"] = value.ToString();
    //        }
    //    }
    //    public static int dwEntityList
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwEntityList"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwEntityList"] = value.ToString();
    //        }
    //    }
    //    public static int dwGlowObject
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwGlowObject"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwGlowObject"] = value.ToString();
    //        }
    //    }
    //    public static int dwForceAttack
    //    {
    //        get
    //        {
    //            return Int32.Parse(data["Offsets"]["dwForceAttack"]);
    //        }
    //        private set
    //        {
    //            data["Offsets"]["dwForceAttack"] = value.ToString();
    //        }
    //    }
    //}
}
