using OSCLoader.Core.Handlers;
using Photon.Pun;
using Photon.Realtime;
using VRC.Callbacks;
using VRC.Managers;
using VRC.Modding;

[assembly: VRCModInfo("ChatboxPlus", "1.0.0", "YouTubey")]

namespace ChatboxPlus
{
    public class Main : VRCMod
    {
        public static string action = "";
        int delay = 120;
        int defaultDelay = 120;
        int section = 0;
        int playerPage = 0;
        List<Player> playerSnapshot = new List<Player>();

        public override void OnModStart()
        {
            blackout = new Blackout();
            pe = new PE();
            svrb = new SVRB();

            if (Device.GetPlatform() == Device.PlatformType.PC) defaultDelay = 60;
        }

        public override void Update()
        {
            if (!PhotonNetwork.InRoom) return;
            if (delay <= 0)
            {
                if (section != 5)
                {
                    ShowInfo();

                    playerSnapshot = PhotonNetwork.PlayerList.ToList();

                    playerPage = 0;
                    section++;
                    if (action != "")
                    {
                        delay = defaultDelay * 3;
                    }
                    else
                    {
                        delay = defaultDelay;
                    }
                    action = "";
                }
                else
                {
                    ShowPlayers();

                    int pageCount = (playerSnapshot.Count + 4) / 5;

                    playerPage++;

                    if (playerPage >= pageCount)
                    {
                        playerPage = 0;
                        section = 0;
                    }
                    delay = defaultDelay * 4;
                }
            }
            else
            {
                delay--;
            }
        }

        void ShowInfo()
        {
            Device.MediaSnapshot snapshot = Device.GetMediaSnapshot();

            string time = DateTime.Now.ToString("hh:mm tt") + "\n";
            string resources = $"ᴼˢᶜᴸᵒᵃᵈᵉʳ\nᴿᴬᴹ {Device.GetRAMUsage()}% ╏ ᴮᴬᵀᵀᴱᴿʸ {Device.GetBatteryPercent()}%\n";

            string media = "";

            if (snapshot.Title != "null")
            {
                string emoji = "🎵";

                if (!snapshot.IsPlaying)
                    emoji = "⏸️";

                media = $"\n{emoji} {snapshot.Title} ᵇʸ {snapshot.Artist}";
            }
            else
            {
                media = action;
            }

            string result = time + resources + media;

            if (result.Length > 144)
            {
                result = result.Replace(resources, "");
            }

            OSCManager.RPC_UpdateChatbox(result, true, false);
        }

        void ShowPlayers()
        {
            int start = playerPage * 5;
            int end = start + 5;

            if (end > playerSnapshot.Count) end = playerSnapshot.Count;

            string result = $"{WorldManager.currentWorld.Name} | 👥 Players ({playerSnapshot.Count})\n";

            for (int i = start; i < end; i++)
            {
                Player player = playerSnapshot[i];
                if (player == null) continue;
                result += player.NickName + "\n";
            }

            OSCManager.RPC_UpdateChatbox(result, true, false);
        }

        public override void OnPlayerJoined(Player photonPlayer)
        {
            action = $"{photonPlayer.NickName} joined.";
        }

        public override void OnPlayerLeft(Player photonPlayer)
        {
            action = $"{photonPlayer.NickName} left.";
        }

        public override void OnAvatarLoad(Player photonPlayer, string avatarName)
        {
            action = $"{photonPlayer.NickName} changed into {avatarName}.";
        }

        public override void OnSpawnedProp(Player photonPlayer, string propId)
        {
            action = $"{photonPlayer.NickName} spawned prop {propId}.";
        }

        public override void OnSpawnedSticker(Player photonPlayer, string stickerId)
        {
            action = $"{photonPlayer.NickName} spawned sticker {stickerId}.";
        }

        public override void OnGrabInteractable(string objectName, bool equipped, bool autoEquipType, string inputMethod, bool autoHold)
        {
            action = $"Grabbed: {objectName}.";
        }

        public override void OnDropInteractable(string objectName, bool wasEquipped, string reasonForDrop, string inputMethod)
        {
            action = $"Dropped: {objectName}.";
        }

        public override void OnNotificationRecieved(int id, string type, DateTime creationDate, string message)
        {
            action = $"Notification: {message}";
        }

        Blackout? blackout;
        PE? pe;
        SVRB? svrb;

        public class Blackout : BlackoutCallbacks
        {
            public override void OnDeath()
            {
                action = $"Blackout: Killed";
            }

            public override void OnDamageRecieved(int damage)
            {
                action = $"Blackout: Hurt ({damage})";
            }

            public override void OnGameEnded(Roles winningRole)
            {
                action = $"Blackout: {winningRole} won";
            }

            public override void OnGameStarted(Roles assignedRole)
            {
                action = $"Blackout: Game Started";
            }
        }

        public class PE : PrisonEscapeCallbacks
        {
            public override void OnPlayerKilled(Player photonPlayer)
            {
                action = $"PE: Killed {photonPlayer.NickName} ({photonPlayer.UserID})";
            }

            public override void OnDamageRecieved(int damage)
            {
                action = $"PE: Hurt ({damage})";
            }

            public override void OnGameEnd(Teams winningTeam)
            {
                action = $"PE: {winningTeam} won";
            }

            public override void OnGameStarted(Teams assignedTeam)
            {
                action = $"PE: Game Started as {assignedTeam}";
            }
        }

        public class SVRB : SuperVRBallCallbacks
        {
            public override void OnClientBeatLevel()
            {
                action = $"SVRB: Level Complete";
            }

            public override void OnLevelLoaded(int levelId)
            {
                action = $"SVRB: Level {levelId} loaded";
            }

            public override void OnPlayerFell(Player photonPlayer)
            {
                action = $"SVRB: {photonPlayer} died";
            }

            public override void OnPlayerStatsUpdated(Player photonPlayer, float totalTime, int levelsBeaten)
            {
                action = $"SVRB: {photonPlayer.NickName} new stats: Time: {totalTime}, Levels Beaten: {levelsBeaten}";
            }
        }
    }
}