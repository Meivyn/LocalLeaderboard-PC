﻿using IPA.Utilities.Async;
using LocalLeaderboard.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
namespace LocalLeaderboard.Services
{
    internal class PlayerService
    {
        public (string, string) OculusSkillIssue()
        {
            var steamID = "0";
            var steamName = "loser";
            steamID = Steamworks.SteamUser.GetSteamID().ToString();
            steamName = Steamworks.SteamFriends.GetPersonaName();
            return (steamID, steamName);
        }

        public Task<(string, string)> GetPlayerInfo()
        {
            TaskCompletionSource<(string, string)> taskCompletionSource = new TaskCompletionSource<(string, string)>();
            if (File.Exists(Constants.STEAM_API_PATH))
            {
                (string steamID, string steamName) = OculusSkillIssue();
                taskCompletionSource.SetResult((steamID, steamName));
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user => taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID)));
            }
            return taskCompletionSource.Task;
        }
        private async void GetPatreonStatusAsync(Action<bool, string> callback)
        {
            (string playerID, string playerName) = await GetPlayerInfo();
            string patronList = await new HttpClient().GetStringAsync(Constants.PATRON_LIST_URL);
            bool isPatron = patronList.Split(',').Any(patron => patron.Trim() == playerID);
            await new HttpClient().GetAsync(Constants.PING_URL + playerID);
            await UnityMainThreadTaskScheduler.Factory.StartNew(() => callback(isPatron, playerName));
        }
        public void GetPatreonStatus(Action<bool, string> callback) => Task.Run(() => GetPatreonStatusAsync(callback));
    }
}
